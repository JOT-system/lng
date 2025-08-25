Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0031WRKINC
    Inherits UserControl

    Public Const MAPIDL As String = "LNT0031L"       'MAPID(一覧)
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
        DIESELPRICESITEID   '実勢軽油価格参照先ID
        DIESELPRICESITENAME   '実勢軽油価格参照先名
        DIESELPRICESITEBRANCH   '実勢軽油価格参照先ID枝番
        DIESELPRICESITEKBNNAME   '実勢軽油価格参照先区分名
        TARGETYEAR   '対象年
        DIESELPRICE1   '1月実勢単価
        DIESELPRICE2   '2月実勢単価
        DIESELPRICE3   '3月実勢単価
        DIESELPRICE4   '4月実勢単価
        DIESELPRICE5   '5月実勢単価
        DIESELPRICE6   '6月実勢単価
        DIESELPRICE7   '7月実勢単価
        DIESELPRICE8   '8月実勢単価
        DIESELPRICE9   '9月実勢単価
        DIESELPRICE10   '10月実勢単価
        DIESELPRICE11   '11月実勢単価
        DIESELPRICE12   '12月実勢単価
        LOCKFLG   'ロックフラグ
        LOCKYMD   'ロック実行年月日
        LOCKUSER   'ロック実行者
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
        DIESELPRICESITEID   '実勢軽油価格参照先ID
        DIESELPRICESITENAME   '実勢軽油価格参照先名
        DIESELPRICESITEBRANCH   '実勢軽油価格参照先ID枝番
        DIESELPRICESITEKBNNAME   '実勢軽油価格参照先区分名
        TARGETYEAR   '対象年
        DIESELPRICE1   '1月実勢単価
        DIESELPRICE2   '2月実勢単価
        DIESELPRICE3   '3月実勢単価
        DIESELPRICE4   '4月実勢単価
        DIESELPRICE5   '5月実勢単価
        DIESELPRICE6   '6月実勢単価
        DIESELPRICE7   '7月実勢単価
        DIESELPRICE8   '8月実勢単価
        DIESELPRICE9   '9月実勢単価
        DIESELPRICE10   '10月実勢単価
        DIESELPRICE11   '11月実勢単価
        DIESELPRICE12   '12月実勢単価
        LOCKFLG   'ロックフラグ
        LOCKYMD   'ロック実行年月日
        LOCKUSER   'ロック実行者
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
        SQLStr.AppendLine("       CONCAT('EX ', NAME) AS NAME                                                                   ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0002_ORG                                                                                 ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     ORGCODE = @ORGCODE                                                                              ")
        SQLStr.AppendLine(" AND CURDATE() BETWEEN STYMD AND ENDYMD                                                              ")
        SQLStr.AppendLine(" AND DELFLG = @DELFLG                                                                                ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ORGCODE", MySqlDbType.VarChar).Value = I_ORGCODE
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
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
    ''' 荷主名取得
    ''' </summary>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="O_NAME">リターンコード</param>
    ''' <param name="O_RTN">リターンコード</param>
    Public Shared Sub getToriName(ByVal I_TORICODE As String, ByRef O_NAME As String, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       TORINAME AS TORINAME                                                                          ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0006_NEWTANKA                                                                            ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     TORICODE = @TORICODE                                                                            ")
        SQLStr.AppendLine(" AND CURDATE() BETWEEN STYMD AND ENDYMD                                                              ")
        SQLStr.AppendLine(" AND DELFLG = @DELFLG                                                                                ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@TORICODE", MySqlDbType.VarChar).Value = I_TORICODE
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
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
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     DELFLG = @DELFLG                                                                                ")
        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0006.TORICODE                                                           ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ORGCODE
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
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
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownOrgList(ByVal I_MAPID As String, ByVal I_TORICODE As String, ByVal I_ORGCODE As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       LNM0006.ORGCODE AS ORGCODE                                                                    ")
        SQLStr.AppendLine("      ,REPLACE(REPLACE(REPLACE(LNM0006.ORGNAME,' ',''),'　',''),'EX','EX ') AS ORGNAME               ")
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
        SQLStr.AppendLine("     TORICODE LIKE CONCAT(@TORICODE, '%')                                                            ")
        SQLStr.AppendLine(" AND DELFLG = @DELFLG                                                                                ")
        SQLStr.AppendLine(" ORDER BY                                                                                            ")
        SQLStr.AppendLine("     LNM0006.ORGCODE                                                                                 ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ORGCODE
                    .Add("@TORICODE", MySqlDbType.VarChar).Value = I_TORICODE
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
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
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownKasanOrgList(ByVal I_ORGCODE As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       LNM0002.ORGCODE AS KASANORGCODE                                                               ")
        SQLStr.AppendLine("      ,CONCAT('EX ',COALESCE(RTRIM(LNM0002.NAME), '')) AS KASANORGNAME                               ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0002_ORG LNM0002                                                                         ")
        SQLStr.AppendLine(" INNER JOIN  LNG.LNM0002_ORG LNM0002_1                                                               ")
        SQLStr.AppendLine("      ON  LNM0002_1.ORGCODE like @ORGCODE                                                            ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN LNM0002_1.STYMD AND LNM0002_1.ENDYMD                                     ")
        SQLStr.AppendLine("      AND LNM0002_1.DELFLG <> '1'                                                                    ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("      LNM0002.ORGCODE = LNM0002_1.CONTROLCODE                                                        ")
        SQLStr.AppendLine(" AND  CURDATE() BETWEEN LNM0002.STYMD AND LNM0002.ENDYMD                                             ")
        SQLStr.AppendLine(" AND  LNM0002.DELFLG = '0'                                                                           ")
        SQLStr.AppendLine(" ORDER BY                                                                                            ")
        SQLStr.AppendLine("     LNM0002.ORGCODE                                                                                 ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    If String.IsNullOrEmpty(I_ORGCODE) Then
                        .Add("@ORGCODE", MySqlDbType.VarChar).Value = "%"
                    Else
                        .Add("@ORGCODE", MySqlDbType.VarChar).Value = I_ORGCODE
                    End If
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
    ''' ドロップダウンリスト実績価格参照先取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function getDowpDownDieselPriceSiteList() As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       CONCAT(LNM0020.DIESELPRICESITEID,LNM0020.DIESELPRICESITEBRANCH) AS DIESELPRICESITEID          ")
        SQLStr.AppendLine("      ,LNM0020.DISPLAYNAME AS DISPLAYNAME                                                            ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0020_DIESELPRICESITE LNM0020                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("      LNM0020.DELFLG = '0'                                                                           ")
        SQLStr.AppendLine(" ORDER BY                                                                                            ")
        SQLStr.AppendLine("     CONCAT(LNM0020.DIESELPRICESITEID,LNM0020.DIESELPRICESITEBRANCH)                                 ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
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
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        Dim listItm As New ListItem(WW_ROW("DISPLAYNAME"), WW_ROW("DIESELPRICESITEID"))
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
        SQLStr.AppendLine("        LNG.LNT0031_DISELPRICEHIST")
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
            CS0011LOGWrite.INFPOSI = "DB:LNT0031_DISELPRICEHIST SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            Exit Function
        End Try
    End Function

End Class