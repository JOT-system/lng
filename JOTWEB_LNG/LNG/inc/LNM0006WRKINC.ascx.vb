Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0006WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNM0006S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNM0006L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNM0006D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0006H"       'MAPID(履歴)
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
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        AVOCADOSHUKABASHO   '実績出荷場所コード
        AVOCADOSHUKANAME   '実績出荷場所名称
        SHUKABASHO   '変換後出荷場所コード
        SHUKANAME   '変換後出荷場所名称
        AVOCADOTODOKECODE   '実績届先コード
        AVOCADOTODOKENAME   '実績届先名称
        TODOKECODE   '変換後届先コード
        TODOKENAME   '変換後届先名称
        TANKNUMBER   '陸事番号
        SHABAN   '車番
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        BRANCHCODE   '枝番
        TANKAKBN   '単価区分
        MEMO   '単価用途
        TANKA   '単価
        CALCKBN   '計算区分
        ROUNDTRIP   '往復距離
        TOLLFEE   '通行料
        SYAGATA   '車型
        SYAGATANAME   '車型名
        SYABARA   '車腹
        BIKOU1   '備考1
        BIKOU2   '備考2
        BIKOU3   '備考3
    End Enum

    ''' <summary>
    ''' 変更履歴出力項目位置
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOL
        OPERATEKBNNAME    '操作区分
        MODIFYKBNNAME   '変更区分
        MODIFYYMD   '変更日時
        MODIFYUSER   '変更USER
        DELFLG   '削除フラグ
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        AVOCADOSHUKABASHO   '実績出荷場所コード
        AVOCADOSHUKANAME   '実績出荷場所名称
        SHUKABASHO   '変換後出荷場所コード
        SHUKANAME   '変換後出荷場所名称
        AVOCADOTODOKECODE   '実績届先コード
        AVOCADOTODOKENAME   '実績届先名称
        TODOKECODE   '変換後届先コード
        TODOKENAME   '変換後届先名称
        TANKNUMBER   '陸事番号
        SHABAN   '車番
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        BRANCHCODE   '枝番
        TANKAKBN   '単価区分
        MEMO   '単価用途
        TANKA   '単価
        CALCKBN   '計算区分
        ROUNDTRIP   '往復距離
        TOLLFEE   '通行料
        SYAGATA   '車型
        SYAGATANAME   '車型名
        SYABARA   '車腹
        BIKOU1   '備考1
        BIKOU2   '備考2
        BIKOU3   '備考3
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
    ''' ドロップダウンリスト荷主データ取得
    ''' </summary>
    ''' <param name="I_MAPID">MAPID</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownToriList(ByVal I_MAPID As String, ByVal I_ORGCODE As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       TORICODE AS TORICODE                                                                          ")
        SQLStr.AppendLine("      ,TORINAME AS TORINAME                                                                          ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA LNM0006                                                                    ")
        SQLStr.AppendLine(" INNER JOIN                                                                                          ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          CODE                                                                                       ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0005_ROLE                                                                           ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          OBJECT = 'ORG'                                                                             ")
        SQLStr.AppendLine("      AND ROLE = @ROLE                                                                               ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS0005                                                                                        ")
        SQLStr.AppendLine("      ON  LNM0006.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0006.TORICODE                                                           ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ORGCODE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        Return retList
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If I_MAPID = MAPIDL And WW_Tbl.Rows.Count > 1 Then
                        If AdminCheck(I_ORGCODE) Then
                            Dim listBlankItm As New ListItem("全て表示", "")
                            retList.Items.Add(listBlankItm)
                        End If
                    End If
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        Dim listItm As New ListItem(WW_ROW("TORINAME"), WW_ROW("TORICODE"))
                        retList.Items.Add(listItm)
                    Next
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' ドロップダウンリスト部門データ取得
    ''' </summary>
    ''' <param name="I_MAPID">MAPID</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownOrgList(ByVal I_MAPID As String, ByVal I_ORGCODE As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       LNM0006.ORGCODE AS ORGCODE                                                                    ")
        SQLStr.AppendLine("      ,REPLACE(REPLACE(REPLACE(LNM0006.ORGNAME,' ',''),'　',''),'EX','EX ') AS ORGNAME               ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA LNM0006                                                                      ")
        SQLStr.AppendLine(" INNER JOIN                                                                                          ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          CODE                                                                                       ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0005_ROLE                                                                           ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          OBJECT = 'ORG'                                                                             ")
        SQLStr.AppendLine("      AND ROLE = @ROLE                                                                               ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS0005                                                                                        ")
        SQLStr.AppendLine("      ON  LNM0006.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0006.ORGCODE                                                           ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ORGCODE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        Return retList
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If I_MAPID = MAPIDL And WW_Tbl.Rows.Count > 1 Then
                        Dim listBlankItm As New ListItem("全て表示", "")
                        retList.Items.Add(listBlankItm)
                    End If
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        Dim listItm As New ListItem(WW_ROW("ORGNAME"), WW_ROW("ORGCODE"))
                        retList.Items.Add(listItm)
                    Next
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' ドロップダウンリスト加算先部門データ取得
    ''' </summary>
    ''' <param name="I_MAPID">MAPID</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownKasanOrgList(ByVal I_MAPID As String, ByVal I_ORGCODE As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       LNM0006.KASANORGCODE AS KASANORGCODE                                                          ")
        SQLStr.AppendLine("      ,REPLACE(REPLACE(REPLACE(COALESCE(RTRIM(LNM0006.KASANORGNAME), ''),' ',''),'　',''),'EX','EX ') AS KASANORGNAME ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA LNM0006                                                                      ")
        SQLStr.AppendLine(" INNER JOIN                                                                                          ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          CODE                                                                                       ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0005_ROLE                                                                           ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          OBJECT = 'ORG'                                                                             ")
        SQLStr.AppendLine("      AND ROLE = @ROLE                                                                               ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS0005                                                                                        ")
        SQLStr.AppendLine("      ON  LNM0006.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0006.KASANORGCODE                                                           ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ORGCODE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        Return retList
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If I_MAPID = MAPIDL And WW_Tbl.Rows.Count > 1 Then
                        Dim listBlankItm As New ListItem("全て表示", "")
                        retList.Items.Add(listBlankItm)
                    End If
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        Dim listItm As New ListItem(WW_ROW("KASANORGNAME"), WW_ROW("KASANORGCODE"))
                        retList.Items.Add(listItm)
                    Next
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' ドロップダウンリスト実績出荷場所データ取得
    ''' </summary>
    ''' <param name="I_MAPID">MAPID</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownAvocadoshukaList(ByVal I_MAPID As String, ByVal I_ORGCODE As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       AVOCADOSHUKABASHO AS AVOCADOSHUKABASHO                                                                          ")
        SQLStr.AppendLine("      ,AVOCADOSHUKANAME AS AVOCADOSHUKANAME                                                                          ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA LNM0006                                                                    ")
        SQLStr.AppendLine(" INNER JOIN                                                                                          ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          CODE                                                                                       ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0005_ROLE                                                                           ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          OBJECT = 'ORG'                                                                             ")
        SQLStr.AppendLine("      AND ROLE = @ROLE                                                                               ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS0005                                                                                        ")
        SQLStr.AppendLine("      ON  LNM0006.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0006.AVOCADOSHUKABASHO                                                  ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ORGCODE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        Return retList
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If I_MAPID = MAPIDL And WW_Tbl.Rows.Count > 1 Then
                        If AdminCheck(I_ORGCODE) Then
                            Dim listBlankItm As New ListItem("全て表示", "")
                            retList.Items.Add(listBlankItm)
                        End If
                    End If
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        Dim listItm As New ListItem(WW_ROW("AVOCADOSHUKANAME"), WW_ROW("AVOCADOSHUKABASHO"))
                        retList.Items.Add(listItm)
                    Next
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' ドロップダウンリスト実績届先データ取得
    ''' </summary>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownAvocadotodokeList(ByVal I_MAPID As String, ByVal I_ORGCODE As String, Optional ByVal I_TORINAME As String = "") As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       AVOCADOTODOKECODE AS AVOCADOTODOKECODE                                                                      ")
        SQLStr.AppendLine("      ,AVOCADOTODOKENAME AS AVOCADOTODOKENAME                                                                      ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA LNM0006                                                                    ")
        SQLStr.AppendLine(" INNER JOIN                                                                                          ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          CODE                                                                                       ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0005_ROLE                                                                           ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          OBJECT = 'ORG'                                                                             ")
        SQLStr.AppendLine("      AND ROLE = @ROLE                                                                               ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS0005                                                                                        ")
        SQLStr.AppendLine("      ON  LNM0006.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     '0' = '0'                                                                                       ")
        If Not I_TORINAME = "" Then
            SQLStr.AppendLine(" AND  LNM0006.TORINAME COLLATE UTF8_UNICODE_CI LIKE'%" & I_TORINAME & "%'                                               ")
        End If

        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0006.AVOCADOTODOKECODE                                                         ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ORGCODE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        Return retList
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If I_MAPID = MAPIDL And WW_Tbl.Rows.Count > 1 Then
                        Dim listBlankItm As New ListItem("全て表示", "")
                        retList.Items.Add(listBlankItm)
                    End If
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        Dim listItm As New ListItem(WW_ROW("AVOCADOTODOKENAME"), WW_ROW("AVOCADOTODOKECODE"))
                        retList.Items.Add(listItm)
                    Next
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' 名称取得(取引先名)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_NAMEht">取引先名格納HT</param>
    Public Sub CODENAMEGetTORI(ByVal SQLcon As MySqlConnection,
                               ByRef O_NAMEht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       TORICODE AS TORICODE")
        SQLStr.AppendLine("      ,RTRIM(TORINAME) AS TORINAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
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
                    '取引先コード、取引先名格納
                    If Not O_NAMEht.ContainsKey(WW_Row("TORICODE")) Then
                        O_NAMEht.Add(WW_Row("TORICODE"), WW_Row("TORINAME"))
                    End If
                Next
            End Using
        Catch ex As Exception
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
    ''' 名称取得(実績届先名)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_NAMEht">実績届先名格納HT</param>
    Public Sub CODENAMEGetAVOCADOTODOKE(ByVal SQLcon As MySqlConnection,
                                   ByRef O_NAMEht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       AVOCADOTODOKECODE AS AVOCADOTODOKECODE")
        SQLStr.AppendLine("      ,RTRIM(AVOCADOTODOKENAME) AS AVOCADOTODOKENAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
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
                    '届先コード、届先名格納
                    If Not O_NAMEht.ContainsKey(WW_Row("AVOCADOTODOKECODE")) Then
                        O_NAMEht.Add(WW_Row("AVOCADOTODOKECODE"), WW_Row("AVOCADOTODOKENAME"))
                    End If
                Next
            End Using
        Catch ex As Exception
        End Try

    End Sub

    ''' <summary>
    ''' 操作権限のある組織コード取得
    ''' </summary>
    Public Sub GetPermitOrg(ByVal SQLcon As MySqlConnection,
                                   ByVal I_CAMPCODE As String,
                                   ByVal I_ROLEORG As String,
                                   ByRef O_ORGHT As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("       CODE AS CODE")
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
    ''' 枝番生成
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Shared Function GenerateBranchCode(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByRef O_MESSAGENO As String) As String

        GenerateBranchCode = "1"

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("       MAX(BRANCHCODE) AS BRANCHCODE")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                           ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                             ")
        SQLStr.AppendLine("   AND KASANORGCODE  = @KASANORGCODE                   ")
        SQLStr.AppendLine("   AND AVOCADOSHUKABASHO  = @AVOCADOSHUKABASHO         ")
        SQLStr.AppendLine("   AND AVOCADOTODOKECODE  = @AVOCADOTODOKECODE         ")
        SQLStr.AppendLine("   AND SHABAN  = @SHABAN                               ")
        SQLStr.AppendLine("   AND STYMD  = @STYMD                                 ")
        SQLStr.AppendLine("   AND BRANCHCODE  = @BRANCHCODE                       ")
        SQLStr.AppendLine("   AND SYAGATA  = @SYAGATA                             ")
        SQLStr.AppendLine("   AND SYABARA  = @SYABARA                             ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = WW_ROW("SHABAN") '車番
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
                P_SYAGATA.Value = WW_ROW("SYAGATA") '車型
                P_SYABARA.Value = WW_ROW("SYABARA") '車腹

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count >= 1 Then
                        'ゼロ埋め削除、数値変換、1加算、文字列変換ゼロ埋め
                        'GenerateBranchCode = (CInt(WW_Tbl.Rows(0)("BRANCHCODE").ToString.TrimStart(New Char() {"0"c})) + 1).ToString.PadLeft(2, "0"c)
                        '1加算
                        GenerateBranchCode = (CInt(WW_Tbl.Rows(0)("BRANCHCODE").ToString) + 1).ToString

                    End If
                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006_NEWTANKA SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Function
        End Try
    End Function

    ''' <summary>
    ''' 有効開始日取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Shared Function GetSTYMD(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByRef O_MESSAGENO As String) As String

        GetSTYMD = ""

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("       DATE_FORMAT(MAX(STYMD), '%Y/%m/%d') AS STYMD ")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                           ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                             ")
        SQLStr.AppendLine("   AND KASANORGCODE  = @KASANORGCODE                   ")
        SQLStr.AppendLine("   AND AVOCADOSHUKABASHO  = @AVOCADOSHUKABASHO         ")
        SQLStr.AppendLine("   AND AVOCADOTODOKECODE  = @AVOCADOTODOKECODE         ")
        SQLStr.AppendLine("   AND SHABAN  = @SHABAN                               ")
        SQLStr.AppendLine("   AND BRANCHCODE  = @BRANCHCODE                       ")
        SQLStr.AppendLine("   AND SYAGATA  = @SYAGATA                             ")
        SQLStr.AppendLine("   AND SYABARA  = @SYABARA                             ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = WW_ROW("SHABAN") '車番
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
                P_SYAGATA.Value = WW_ROW("SYAGATA") '車型
                P_SYABARA.Value = WW_ROW("SYABARA") '車腹

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count >= 1 Then
                        GetSTYMD = WW_Tbl.Rows(0)("STYMD").ToString
                    End If
                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006_NEWTANKA SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Function
        End Try
    End Function

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
    ''' <param name="I_KASANORGCODE">加算先部門コード</param>
    ''' <param name="I_AVOCADOSHUKABASHO">実績出荷場所コード</param>
    ''' <param name="I_AVOCADOTODOKECODE">実績届先コード</param>
    ''' <param name="I_SHABAN">車番</param>
    ''' <param name="I_STYMD">有効開始日</param>
    ''' <param name="I_BRANCHCODE">枝番</param>
    ''' <param name="I_SYAGATA">車型</param> 
    ''' <param name="I_SYABARA">車腹</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_KASANORGCODE As String,
                          ByVal I_AVOCADOSHUKABASHO As String, ByVal I_AVOCADOTODOKECODE As String, ByVal I_SHABAN As String,
                          ByVal I_STYMD As String, ByVal I_BRANCHCODE As String,
                          ByVal I_SYAGATA As String, ByVal I_SYABARA As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                                ")
        SQLStr.AppendLine("    UPDTIMSTP                                          ")
        SQLStr.AppendLine(" FROM                                                  ")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA                              ")
        SQLStr.AppendLine(" WHERE                                                 ")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                           ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                             ")
        SQLStr.AppendLine("   AND KASANORGCODE  = @KASANORGCODE                   ")
        SQLStr.AppendLine("   AND AVOCADOSHUKABASHO  = @AVOCADOSHUKABASHO         ")
        SQLStr.AppendLine("   AND AVOCADOTODOKECODE  = @AVOCADOTODOKECODE         ")
        SQLStr.AppendLine("   AND SHABAN  = @SHABAN                               ")
        SQLStr.AppendLine("   AND STYMD  = @STYMD                                 ")
        SQLStr.AppendLine("   AND BRANCHCODE  = @BRANCHCODE                       ")
        SQLStr.AppendLine("   AND SYAGATA  = @SYAGATA                             ")
        SQLStr.AppendLine("   AND SYABARA  = @SYABARA                             ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                P_TORICODE.Value = I_TORICODE '取引先コード
                P_ORGCODE.Value = I_ORGCODE '部門コード
                P_KASANORGCODE.Value = I_KASANORGCODE '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = I_AVOCADOSHUKABASHO           '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = I_AVOCADOTODOKECODE           '実績届先コード
                P_SHABAN.Value = I_SHABAN           '車番
                P_STYMD.Value = I_STYMD           '有効開始日
                P_BRANCHCODE.Value = I_BRANCHCODE '枝番
                P_SYAGATA.Value = I_SYAGATA           '車型
                P_SYABARA.Value = I_SYABARA           '車腹

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0006Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0006Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0006Chk.Load(SQLdr)

                    If LNM0006Chk.Rows.Count > 0 Then
                        Dim LNM0006row As DataRow
                        LNM0006row = LNM0006Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0006row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0006row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0006D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub
End Class