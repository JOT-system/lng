Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0014WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNM0014S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNM0014L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNM0014D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0014H"       'MAPID(履歴)
    'タイトル区分
    Public Const TITLEKBNS As String = "C"   'タイトル区分

    'デフォルト値
    Public Const DEFAULT_FROMORGNAME As String = "日本石油輸送株式会社　高圧ガス輸送事業部　高圧ガス１部" '請求書発行部店名
    Public Const KABUSHIKIKAISHA As String = "株式会社"

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
        TARGETYM   '対象年月
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        BIGCATECODE     '大分類コード
        BIGCATENAME     '大分類名
        MIDCATECODE     '中分類コード
        MIDCATENAME     '中分類名
        SMALLCATECODE   '小分類コード
        SMALLCATENAME   '小分類名
        'TODOKECODE   '届先コード
        'TODOKENAME   '届先名称
        'GROUPSORTNO   'グループソート順
        'GROUPID   'グループID
        'GROUPNAME   'グループ名
        'DETAILSORTNO   '明細ソート順
        'DETAILID   '明細ID
        'DETAILNAME   '明細名
        TANKA   '単価
        QUANTITY   '数量
        CALCUNIT   '計算単位
        DEPARTURE   '出荷地
        MILEAGE   '走行距離
        SHIPPINGCOUNT   '輸送回数
        NENPI   '燃費
        DIESELPRICECURRENT   '実勢軽油価格
        DIESELPRICESTANDARD   '基準経由価格
        DIESELCONSUMPTION   '燃料使用量
        DISPLAYFLG   '表示フラグ
        ASSESSMENTFLG   '鑑分けフラグ
        ATENACOMPANYNAME   '宛名会社名
        ATENACOMPANYDEVNAME   '宛名会社部門名
        FROMORGNAME   '請求書発行部店名
        MEISAICATEGORYID   '明細区分
        ACCOUNTCODE   '勘定科目コード
        ACCOUNTNAME   '勘定科目名
        SEGMENTCODE   'セグメントコード
        SEGMENTNAME   'セグメント名
        JOTPERCENTAGE   '割合JOT
        ENEXPERCENTAGE   '割合ENEX
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
        TARGETYM   '対象年月
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        BIGCATECODE   '大分類コード
        BIGCATENAME   '大分類名
        MIDCATECODE   '中分類コード
        MIDCATENAME   '中分類名
        SMALLCATECODE   '小分類コード
        SMALLCATENAME   '小分類名
#Region "コメント-2025/08/04(分類追加対応のため)"
        'TODOKECODE   '届先コード
        'TODOKENAME   '届先名称
        'GROUPSORTNO   'グループソート順
        'GROUPID   'グループID
        'GROUPNAME   'グループ名
        'DETAILSORTNO   '明細ソート順
        'DETAILID   '明細ID
        'DETAILNAME   '明細名
#End Region
        TANKA   '単価
        QUANTITY   '数量
        CALCUNIT   '計算単位
        DEPARTURE   '出荷地
        MILEAGE   '走行距離
        SHIPPINGCOUNT   '輸送回数
        NENPI   '燃費
        DIESELPRICECURRENT   '実勢軽油価格
        DIESELPRICESTANDARD   '基準経由価格
        DIESELCONSUMPTION   '燃料使用量
        DISPLAYFLG   '表示フラグ
        ASSESSMENTFLG   '鑑分けフラグ
        ATENACOMPANYNAME   '宛名会社名
        ATENACOMPANYDEVNAME   '宛名会社部門名
        FROMORGNAME   '請求書発行部店名
        MEISAICATEGORYID   '明細区分
        ACCOUNTCODE   '勘定科目コード
        ACCOUNTNAME   '勘定科目名
        SEGMENTCODE   'セグメントコード
        SEGMENTNAME   'セグメント名
        JOTPERCENTAGE   '割合JOT
        ENEXPERCENTAGE   '割合ENEX
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

#Region "組織コードチェック"
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
    ''' 石狩営業所の組織コードか確認する
    ''' </summary>
    ''' <param name="I_ORG">対象組織コード</param>
    ''' <remarks></remarks>
    Public Shared Function IshikariCheck(ByVal I_ORG As Object) As Boolean
        Dim WW_HT As New Hashtable
        WW_HT.Add("020104", "EX石狩営業所")

        Return WW_HT.ContainsKey(I_ORG)
    End Function

    ''' <summary>
    ''' 八戸営業所の組織コードか確認する
    ''' </summary>
    ''' <param name="I_ORG">対象組織コード</param>
    ''' <remarks></remarks>
    Public Shared Function HachinoheCheck(ByVal I_ORG As Object) As Boolean
        Dim WW_HT As New Hashtable
        WW_HT.Add("020202", "EX八戸営業所")

        Return WW_HT.ContainsKey(I_ORG)
    End Function

    ''' <summary>
    ''' 東北支店の組織コードか確認する
    ''' </summary>
    ''' <param name="I_ORG">対象組織コード</param>
    ''' <remarks></remarks>
    Public Shared Function TohokuCheck(ByVal I_ORG As Object) As Boolean
        Dim WW_HT As New Hashtable
        WW_HT.Add("020402", "EX東北支店")

        Return WW_HT.ContainsKey(I_ORG)
    End Function

    ''' <summary>
    ''' 水島営業所の組織コードか確認する
    ''' </summary>
    ''' <param name="I_ORG">対象組織コード</param>
    ''' <remarks></remarks>
    Public Shared Function MizushimaCheck(ByVal I_ORG As Object) As Boolean
        Dim WW_HT As New Hashtable
        WW_HT.Add("023301", "EX水島営業所")

        Return WW_HT.ContainsKey(I_ORG)
    End Function
#End Region

    ''' <summary>
    ''' ドロップダウンリスト荷主データ取得
    ''' </summary>
    ''' <param name="I_MAPID">MAPID</param>
    ''' <param name="I_ROLEORGCODE">ロール</param>
    ''' <param name="I_TORICODE">荷主</param>
    ''' <param name="I_ORGCODE">部門</param>
    ''' <param name="I_KASANORGCODE">加算先部門</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownToriList(ByVal I_MAPID As String, ByVal I_ROLEORGCODE As String,
                                               Optional ByVal I_TORICODE As String = Nothing,
                                               Optional ByVal I_ORGCODE As String = Nothing,
                                               Optional ByVal I_KASANORGCODE As String = Nothing) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       TORICODE AS TORICODE                                                                          ")
        SQLStr.AppendLine("      ,TORINAME AS TORINAME                                                                          ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 LNM0014                                                                      ")
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
        SQLStr.AppendLine("      ON  LNM0014.ORGCODE = LNS0005.CODE                                                             ")

        SQLStr.AppendLine(" WHERE                ")
        SQLStr.AppendFormat("     DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        If Not IsNothing(I_TORICODE) AndAlso I_TORICODE <> "" Then
            'If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
            '    SQLStr.AppendFormat(" AND TORICODE IN ('{0}','{1}') ", I_TORICODE, BaseDllConst.CONST_TORICODE_0238900000)
            'Else
            SQLStr.AppendFormat(" AND TORICODE = '{0}' ", I_TORICODE)
            'End If
        End If

        If Not IsNothing(I_ORGCODE) AndAlso I_ORGCODE <> "" Then
            SQLStr.AppendFormat(" AND ORGCODE = '{0}' ", I_ORGCODE)
        End If

        If Not IsNothing(I_KASANORGCODE) AndAlso I_KASANORGCODE <> "" Then
            SQLStr.AppendFormat(" AND KASANORGCODE = '{0}' ", I_KASANORGCODE)
        End If

        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0014.TORICODE                                                           ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ROLEORGCODE
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
                        If AdminCheck(I_ROLEORGCODE) Then
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
    ''' <param name="I_ROLEORGCODE">ロール</param>
    ''' <param name="I_TORICODE">荷主</param>
    ''' <param name="I_ORGCODE">部門</param>
    ''' <param name="I_KASANORGCODE">加算先部門</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownOrgList(ByVal I_MAPID As String, ByVal I_ROLEORGCODE As String,
                                              Optional ByVal I_TORICODE As String = Nothing,
                                              Optional ByVal I_ORGCODE As String = Nothing,
                                              Optional ByVal I_KASANORGCODE As String = Nothing) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       LNM0014.ORGCODE AS ORGCODE                                                                    ")
        SQLStr.AppendLine("      ,REPLACE(REPLACE(REPLACE(LNM0014.ORGNAME,' ',''),'　',''),'EX','EX ') AS ORGNAME               ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 LNM0014                                                                      ")
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
        SQLStr.AppendLine("      ON  LNM0014.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                ")
        SQLStr.AppendFormat("     DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        If Not IsNothing(I_TORICODE) AndAlso I_TORICODE <> "" Then
            If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
                SQLStr.AppendFormat(" AND TORICODE IN ('{0}','{1}') ", I_TORICODE, BaseDllConst.CONST_TORICODE_0238900000)
            Else
                SQLStr.AppendFormat(" AND TORICODE = '{0}' ", I_TORICODE)
            End If
        End If

        If Not IsNothing(I_ORGCODE) AndAlso I_ORGCODE <> "" Then
            SQLStr.AppendFormat(" AND ORGCODE = '{0}' ", I_ORGCODE)
        End If

        If Not IsNothing(I_KASANORGCODE) AndAlso I_KASANORGCODE <> "" Then
            SQLStr.AppendFormat(" AND KASANORGCODE = '{0}' ", I_KASANORGCODE)
        End If

        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0014.ORGCODE                                                           ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ROLEORGCODE
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
    ''' <param name="I_ROLEORGCODE">ロール</param>
    ''' <param name="I_TORICODE">荷主</param>
    ''' <param name="I_ORGCODE">部門</param>
    ''' <param name="I_KASANORGCODE">加算先部門</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownKasanOrgList(ByVal I_MAPID As String, ByVal I_ROLEORGCODE As String,
                                                   Optional ByVal I_TORICODE As String = Nothing,
                                                   Optional ByVal I_ORGCODE As String = Nothing,
                                                   Optional ByVal I_KASANORGCODE As String = Nothing) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       LNM0014.KASANORGCODE AS KASANORGCODE                                                          ")
        SQLStr.AppendLine("      ,REPLACE(REPLACE(REPLACE(COALESCE(RTRIM(LNM0014.KASANORGNAME), ''),' ',''),'　',''),'EX','EX ') AS KASANORGNAME ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 LNM0014                                                                      ")
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
        SQLStr.AppendLine("      ON  LNM0014.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     COALESCE(RTRIM(LNM0014.KASANORGCODE), '') <> ''                                                 ")

        If Not IsNothing(I_TORICODE) AndAlso I_TORICODE <> "" Then
            If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
                SQLStr.AppendFormat(" AND TORICODE IN ('{0}','{1}') ", I_TORICODE, BaseDllConst.CONST_TORICODE_0238900000)
            Else
                SQLStr.AppendFormat(" AND TORICODE = '{0}' ", I_TORICODE)
            End If
        End If

        If Not IsNothing(I_ORGCODE) AndAlso I_ORGCODE <> "" Then
            SQLStr.AppendFormat(" AND ORGCODE = '{0}' ", I_ORGCODE)
        End If

        If Not IsNothing(I_KASANORGCODE) AndAlso I_KASANORGCODE <> "" Then
            SQLStr.AppendFormat(" AND KASANORGCODE = '{0}' ", I_KASANORGCODE)
        End If

        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0014.KASANORGCODE                                                           ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ROLEORGCODE
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

#Region "コメント-2025/07/30(分類追加対応のため)"
    '''' <summary>
    '''' ドロップダウンリスト届先データ取得
    '''' </summary>
    '''' <param name="I_MAPID">MAPID</param>
    '''' <param name="I_ORGCODE">部門コード</param>
    '''' <returns></returns>
    'Public Shared Function getDowpDownTodokeList(ByVal I_MAPID As String, ByVal I_ORGCODE As String) As DropDownList
    '    Dim retList As New DropDownList
    '    Dim CS0050Session As New CS0050SESSION
    '    Dim SQLStr As New StringBuilder

    '    SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
    '    SQLStr.AppendLine("       TODOKECODE AS TODOKECODE                                                                      ")
    '    SQLStr.AppendLine("      ,TODOKENAME AS TODOKENAME                                                                      ")
    '    SQLStr.AppendLine(" FROM                                                                                                ")
    '    SQLStr.AppendLine("     LNG.LNM0014_SPRATE LNM0014                                                                      ")
    '    SQLStr.AppendLine(" INNER JOIN                                                                                          ")
    '    SQLStr.AppendLine("    (                                                                                                ")
    '    SQLStr.AppendLine("      SELECT                                                                                         ")
    '    SQLStr.AppendLine("          CODE                                                                                       ")
    '    SQLStr.AppendLine("      FROM                                                                                           ")
    '    SQLStr.AppendLine("          COM.LNS0005_ROLE                                                                           ")
    '    SQLStr.AppendLine("      WHERE                                                                                          ")
    '    SQLStr.AppendLine("          OBJECT = 'ORG'                                                                             ")
    '    SQLStr.AppendLine("      AND ROLE = @ROLE                                                                               ")
    '    SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
    '    SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
    '    SQLStr.AppendLine("    ) LNS0005                                                                                        ")
    '    SQLStr.AppendLine("      ON  LNM0014.ORGCODE = LNS0005.CODE                                                             ")
    '    SQLStr.AppendLine(" WHERE                                                                                               ")
    '    SQLStr.AppendLine("     COALESCE(RTRIM(LNM0014.TODOKENAME), '') <> ''                                                   ")
    '    SQLStr.AppendLine(" ORDER BY                                                                       ")
    '    SQLStr.AppendLine("     LNM0014.TODOKECODE                                                         ")

    '    Try
    '        Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
    '          sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
    '            sqlCon.Open()
    '            MySqlConnection.ClearPool(sqlCon)
    '            With sqlCmd.Parameters
    '                .Add("@ROLE", MySqlDbType.VarChar).Value = I_ORGCODE
    '            End With
    '            Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
    '                If sqlDr.HasRows = False Then
    '                    Return retList
    '                End If
    '                Dim WW_Tbl = New DataTable
    '                '○ フィールド名とフィールドの型を取得
    '                For index As Integer = 0 To sqlDr.FieldCount - 1
    '                    WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
    '                Next
    '                '○ テーブル検索結果をテーブル格納
    '                WW_Tbl.Load(sqlDr)
    '                If I_MAPID = MAPIDL And WW_Tbl.Rows.Count > 1 Then
    '                    Dim listBlankItm As New ListItem("全て表示", "")
    '                    retList.Items.Add(listBlankItm)
    '                End If
    '                For Each WW_ROW As DataRow In WW_Tbl.Rows
    '                    Dim listItm As New ListItem(WW_ROW("TODOKENAME"), WW_ROW("TODOKECODE"))
    '                    retList.Items.Add(listItm)
    '                Next
    '            End Using
    '        End Using
    '    Catch ex As Exception
    '        Throw ex '呼び出し元の例外にスロー
    '    End Try

    '    Return retList

    'End Function
#End Region

    ''' <summary>
    ''' ドロップダウンリスト出荷地データ取得
    ''' </summary>
    ''' <param name="I_MAPID">MAPID</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownDepartureList(ByVal I_MAPID As String, ByVal I_ORGCODE As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       DEPARTURE AS DEPARTURE                                                                        ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 LNM0014                                                                      ")
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
        SQLStr.AppendLine("      ON  LNM0014.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     COALESCE(RTRIM(LNM0014.DEPARTURE), '') <> ''                                                    ")
        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0014.DEPARTURE                                                          ")


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
                        Dim listItm As New ListItem(WW_ROW("DEPARTURE"), WW_ROW("DEPARTURE"))
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
    ''' ドロップダウンリスト勘定科目データ取得
    ''' </summary>
    ''' <param name="I_STYMD">有効開始日</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownAccountList(ByVal I_STYMD As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       ACCOUNTCODE AS ACCOUNTCODE                                                                    ")
        SQLStr.AppendLine("      ,ACCOUNTNAME AS ACCOUNTNAME                                                                    ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0018_ACCOUNT LNM0018                                                                     ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("       @STYMD BETWEEN FROMYMD AND ENDYMD                                                             ")
        SQLStr.AppendLine("   AND DELFLG <> '1'                                                                                 ")
        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0018.ACCOUNTCODE                                                         ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@STYMD", MySqlDbType.Date).Value = CDate(I_STYMD & "/01")
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
                    'If I_MAPID = MAPIDL And WW_Tbl.Rows.Count > 1 Then
                    '    Dim listBlankItm As New ListItem("全て表示", "")
                    '    retList.Items.Add(listBlankItm)
                    'End If
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        Dim listItm As New ListItem(WW_ROW("ACCOUNTNAME"), WW_ROW("ACCOUNTCODE"))
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
    ''' ドロップダウンリストセグメントデータ取得
    ''' </summary>
    ''' <param name="I_STYMD">有効開始日</param>
    ''' <param name="I_ACCOUNTCODE">勘定科目コード</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownSegmentList(ByVal I_STYMD As String, ByVal I_ACCOUNTCODE As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       SEGMENTCODE AS SEGMENTCODE                                                                    ")
        SQLStr.AppendLine("      ,SEGMENTNAME AS SEGMENTNAME                                                                    ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNM0018_ACCOUNT LNM0018                                                                     ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("       ACCOUNTCODE = @ACCOUNTCODE                                                                    ")
        SQLStr.AppendLine("   AND @STYMD BETWEEN FROMYMD AND ENDYMD                                                             ")
        SQLStr.AppendLine("   AND DELFLG <> '1'                                                                                 ")
        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     LNM0018.SEGMENTCODE                                                         ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@STYMD", MySqlDbType.Date).Value = CDate(I_STYMD & "/01")
                    .Add("@ACCOUNTCODE", MySqlDbType.VarChar).Value = I_ACCOUNTCODE
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
                    'If I_MAPID = MAPIDL And WW_Tbl.Rows.Count > 1 Then
                    '    Dim listBlankItm As New ListItem("全て表示", "")
                    '    retList.Items.Add(listBlankItm)
                    'End If
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        Dim listItm As New ListItem(WW_ROW("SEGMENTNAME"), WW_ROW("SEGMENTCODE"))
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
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 ")

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
    ''' 名称取得(加算先部門名)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_NAMEht">加算先部門名格納HT</param>
    Public Sub CODENAMEGetKASANORG(ByVal SQLcon As MySqlConnection,
                                   ByRef O_NAMEht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       KASANORGCODE AS KASANORGCODE")
        SQLStr.AppendLine("      ,RTRIM(KASANORGNAME) AS KASANORGNAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 ")

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
                    '加算先部門コード、加算先部門名格納
                    If Not O_NAMEht.ContainsKey(WW_Row("KASANORGCODE")) Then
                        O_NAMEht.Add(WW_Row("KASANORGCODE"), WW_Row("KASANORGNAME"))
                    End If
                Next
            End Using
        Catch ex As Exception
        End Try

    End Sub

#Region "コメント-2025/07/30(分類追加対応のため)"
    '''' <summary>
    '''' 名称取得(届先名)
    '''' </summary>
    '''' <param name="SQLcon"></param>
    '''' <param name="O_NAMEht">届先名格納HT</param>
    'Public Sub CODENAMEGetTODOKE(ByVal SQLcon As MySqlConnection,
    '                               ByRef O_NAMEht As Hashtable)

    '    '○ 対象データ取得
    '    Dim SQLStr = New StringBuilder
    '    SQLStr.AppendLine(" SELECT DISTINCT")
    '    SQLStr.AppendLine("       TODOKECODE AS TODOKECODE")
    '    SQLStr.AppendLine("      ,RTRIM(TODOKENAME) AS TODOKENAME")
    '    SQLStr.AppendLine(" FROM")
    '    SQLStr.AppendLine("     LNG.LNM0014_SPRATE")

    '    Try
    '        Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
    '            Dim WW_Tbl = New DataTable
    '            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
    '                '○ フィールド名とフィールドの型を取得
    '                For index As Integer = 0 To SQLdr.FieldCount - 1
    '                    WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
    '                Next
    '                '○ テーブル検索結果をテーブル格納
    '                WW_Tbl.Load(SQLdr)
    '            End Using
    '            'ハッシュテーブルにコードと名称を格納
    '            For Each WW_Row As DataRow In WW_Tbl.Rows
    '                '届先コード、届先名格納
    '                If Not O_NAMEht.ContainsKey(WW_Row("TODOKECODE")) Then
    '                    O_NAMEht.Add(WW_Row("TODOKECODE"), WW_Row("TODOKENAME"))
    '                End If
    '            Next
    '        End Using
    '    Catch ex As Exception
    '    End Try

    'End Sub
#End Region

    ''' <summary>
    ''' ID取得(大分類コード)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_NAMEht">大分類コード格納HT</param>
    Public Sub IDGetBIGCATE(ByVal SQLcon As MySqlConnection,
                            ByRef O_NAMEht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       BIGCATECODE AS BIGCATECODE")
        SQLStr.AppendLine("      ,RTRIM(BIGCATENAME) AS BIGCATENAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 ")

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
                    '大分類コード、大分類名格納
                    If Not O_NAMEht.ContainsKey(WW_Row("BIGCATENAME")) Then
                        O_NAMEht.Add(WW_Row("BIGCATENAME"), WW_Row("BIGCATECODE"))
                    End If
                Next
            End Using
        Catch ex As Exception
        End Try

    End Sub

    ''' <summary>
    ''' ID取得(中分類コード)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_NAMEht">中分類コード格納HT</param>
    Public Sub IDGetMIDCATE(ByVal SQLcon As MySqlConnection,
                            ByRef O_NAMEht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       MIDCATECODE AS MIDCATECODE")
        SQLStr.AppendLine("      ,RTRIM(MIDCATENAME) AS MIDCATENAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 ")

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
                    '大分類コード、大分類名格納
                    If Not O_NAMEht.ContainsKey(WW_Row("MIDCATENAME")) Then
                        O_NAMEht.Add(WW_Row("MIDCATENAME"), WW_Row("MIDCATECODE"))
                    End If
                Next
            End Using
        Catch ex As Exception
        End Try

    End Sub

    ''' <summary>
    ''' ID取得(小分類コード)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_NAMEht">中分類コード格納HT</param>
    Public Sub IDGetSMALLCATE(ByVal SQLcon As MySqlConnection,
                              ByRef O_NAMEht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       SMALLCATECODE AS SMALLCATECODE")
        SQLStr.AppendLine("      ,RTRIM(SMALLCATENAME) AS SMALLCATENAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 ")

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
                    '小分類コード、小分類名格納
                    If Not O_NAMEht.ContainsKey(WW_Row("SMALLCATENAME")) Then
                        O_NAMEht.Add(WW_Row("SMALLCATENAME"), WW_Row("SMALLCATECODE"))
                    End If
                Next
            End Using
        Catch ex As Exception
        End Try

    End Sub

#Region "コメント-2025/07/30(分類追加対応のため)"
    '''' <summary>
    '''' ID取得(グループID)
    '''' </summary>
    '''' <param name="SQLcon"></param>
    '''' <param name="O_NAMEht">グループID格納HT</param>
    'Public Sub IDGetGROUP(ByVal SQLcon As MySqlConnection,
    '                               ByRef O_NAMEht As Hashtable)

    '    '○ 対象データ取得
    '    Dim SQLStr = New StringBuilder
    '    SQLStr.AppendLine(" SELECT DISTINCT")
    '    SQLStr.AppendLine("       GROUPID AS GROUPID")
    '    SQLStr.AppendLine("      ,RTRIM(GROUPNAME) AS GROUPNAME")
    '    SQLStr.AppendLine(" FROM")
    '    SQLStr.AppendLine("     LNG.LNM0014_SPRATE")

    '    Try
    '        Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
    '            Dim WW_Tbl = New DataTable
    '            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
    '                '○ フィールド名とフィールドの型を取得
    '                For index As Integer = 0 To SQLdr.FieldCount - 1
    '                    WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
    '                Next
    '                '○ テーブル検索結果をテーブル格納
    '                WW_Tbl.Load(SQLdr)
    '            End Using
    '            'ハッシュテーブルにコードと名称を格納
    '            For Each WW_Row As DataRow In WW_Tbl.Rows
    '                '届先コード、届先名格納
    '                If Not O_NAMEht.ContainsKey(WW_Row("GROUPNAME")) Then
    '                    O_NAMEht.Add(WW_Row("GROUPNAME"), WW_Row("GROUPID"))
    '                End If
    '            Next
    '        End Using
    '    Catch ex As Exception
    '    End Try

    'End Sub
#End Region

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
    ''' 大分類コード生成
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Shared Function GenerateBigcateCode(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByRef O_MESSAGENO As String) As String

        GenerateBigcateCode = "1"

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("       MAX(BIGCATECODE) AS BIGCATECODE")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '') = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')  = @ORGCODE ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)  '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count >= 1 Then
                        '1加算
                        GenerateBigcateCode = (CInt(WW_Tbl.Rows(0)("BIGCATECODE").ToString) + 1).ToString
                    End If
                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE2(BIGCATECODE) SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Function
        End Try
    End Function

    ''' <summary>
    ''' 中分類コード生成
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Shared Function GenerateMidcateCode(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByRef O_MESSAGENO As String) As String

        GenerateMidcateCode = "1"

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("       MAX(MIDCATECODE) AS MIDCATECODE")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')     = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')      = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BIGCATECODE, '0') = @BIGCATECODE ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)      '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2) '大分類コード
                P_TORICODE.Value = WW_ROW("TORICODE")       '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")         '部門コード
                P_BIGCATECODE.Value = WW_ROW("BIGCATECODE") '大分類コード

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count >= 1 AndAlso WW_ROW("MIDCATENAME").ToString() <> "" Then
                        '1加算
                        GenerateMidcateCode = (CInt(WW_Tbl.Rows(0)("MIDCATECODE").ToString) + 1).ToString
                    Else
                        'そのまま
                        GenerateMidcateCode = (CInt(WW_Tbl.Rows(0)("MIDCATECODE").ToString)).ToString
                    End If
                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE2(MIDCATECODE) SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Function
        End Try
    End Function

    ''' <summary>
    ''' 小分類コード生成
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Shared Function GenerateSmallcateCode(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByRef O_MESSAGENO As String) As String

        GenerateSmallcateCode = "1"

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("       MAX(SMALLCATECODE) AS SMALLCATECODE")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')     = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')      = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BIGCATECODE, '0') = @BIGCATECODE ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)      '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2) '大分類コード
                P_TORICODE.Value = WW_ROW("TORICODE")       '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")         '部門コード
                P_BIGCATECODE.Value = WW_ROW("BIGCATECODE") '大分類コード

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count >= 1 Then
                        '1加算
                        GenerateSmallcateCode = (CInt(WW_Tbl.Rows(0)("SMALLCATECODE").ToString) + 1).ToString
                    End If
                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE2(SMALLCATECODE) SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Function
        End Try
    End Function

#Region "コメント-2025/07/30(分類追加対応のため)"
    '''' <summary>
    '''' グループID生成
    '''' </summary>
    '''' <param name="SQLcon"></param>
    '''' <param name="WW_ROW"></param>
    'Public Shared Function GenerateGroupId(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByRef O_MESSAGENO As String) As String

    '    GenerateGroupId = "1"

    '    Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    '    O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

    '    '○ 対象データ取得
    '    Dim SQLStr = New StringBuilder
    '    SQLStr.AppendLine(" SELECT ")
    '    SQLStr.AppendLine("       MAX(GROUPID) AS GROUPID")
    '    SQLStr.AppendLine(" FROM")
    '    SQLStr.AppendLine("     LNG.LNM0014_SPRATE")
    '    SQLStr.AppendLine(" WHERE")
    '    'SQLStr.AppendLine("         COALESCE(TARGETYM, '')             = @TARGETYM ")
    '    SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
    '    SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")

    '    Try
    '        Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
    '            'Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
    '            Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
    '            Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード

    '            'P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
    '            P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
    '            P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード

    '            Dim WW_Tbl = New DataTable
    '            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
    '                '○ フィールド名とフィールドの型を取得
    '                For index As Integer = 0 To SQLdr.FieldCount - 1
    '                    WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
    '                Next
    '                '○ テーブル検索結果をテーブル格納
    '                WW_Tbl.Load(SQLdr)

    '                If WW_Tbl.Rows.Count >= 1 Then
    '                    '1加算
    '                    GenerateGroupId = (CInt(WW_Tbl.Rows(0)("GROUPID").ToString) + 1).ToString
    '                End If
    '            End Using
    '        End Using
    '    Catch ex As Exception
    '        CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
    '        CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE SELECT"
    '        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
    '        CS0011LOGWrite.TEXT = ex.ToString()
    '        CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
    '        Exit Function
    '    End Try
    'End Function

    '''' <summary>
    '''' 明細ID生成
    '''' </summary>
    '''' <param name="SQLcon"></param>
    '''' <param name="WW_ROW"></param>
    'Public Shared Function GenerateDetailId(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByRef O_MESSAGENO As String) As String

    '    GenerateDetailId = "1"

    '    Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    '    O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

    '    '○ 対象データ取得
    '    Dim SQLStr = New StringBuilder
    '    SQLStr.AppendLine(" SELECT ")
    '    SQLStr.AppendLine("       MAX(DETAILID) AS DETAILID")
    '    SQLStr.AppendLine(" FROM")
    '    SQLStr.AppendLine("     LNG.LNM0014_SPRATE")
    '    SQLStr.AppendLine(" WHERE")
    '    'SQLStr.AppendLine("         COALESCE(TARGETYM, '')             = @TARGETYM ")
    '    SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
    '    SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
    '    SQLStr.AppendLine("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")

    '    Try
    '        Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
    '            'Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
    '            Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
    '            Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
    '            Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID

    '            'P_TARGETYM.Value = WW_ROW("TARGETYM")           '対象年月
    '            P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
    '            P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
    '            P_GROUPID.Value = WW_ROW("GROUPID")           'グループID

    '            Dim WW_Tbl = New DataTable
    '            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
    '                '○ フィールド名とフィールドの型を取得
    '                For index As Integer = 0 To SQLdr.FieldCount - 1
    '                    WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
    '                Next
    '                '○ テーブル検索結果をテーブル格納
    '                WW_Tbl.Load(SQLdr)

    '                If WW_Tbl.Rows.Count >= 1 Then
    '                    '1加算
    '                    GenerateDetailId = (CInt(WW_Tbl.Rows(0)("DETAILID").ToString) + 1).ToString
    '                End If
    '            End Using
    '        End Using
    '    Catch ex As Exception
    '        CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
    '        CS0011LOGWrite.INFPOSI = "DB:LNM0014_SPRATE SELECT"
    '        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
    '        CS0011LOGWrite.TEXT = ex.ToString()
    '        CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
    '        Exit Function
    '    End Try
    'End Function
#End Region

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
    ''' <param name="I_TARGETYM">対象年月</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_BIGCATECODE">大分類コード</param>
    ''' <param name="I_MIDCATECODE">中分類コード</param>
    ''' <param name="I_SMALLCATECODE">小分類コード</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_TARGETYM As String, ByVal I_TORICODE As String, ByVal I_ORGCODE As String,
                          ByVal I_BIGCATECODE As String, ByVal I_MIDCATECODE As String, ByVal I_SMALLCATECODE As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2                     ")
        SQLStr.AppendLine(" WHERE                                       ")
        SQLStr.AppendLine("       TARGETYM  = @TARGETYM                 ")
        SQLStr.AppendLine("   AND TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND BIGCATECODE  = @BIGCATECODE           ")
        SQLStr.AppendLine("   AND MIDCATECODE  = @MIDCATECODE           ")
        SQLStr.AppendLine("   AND SMALLCATECODE  = @SMALLCATECODE       ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)   '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)  '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.VarChar, 2)     '大分類コード
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.VarChar, 2)     '中分類コード
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.VarChar, 2) '小分類コード

                P_TARGETYM.Value = I_TARGETYM           '対象年月
                P_TORICODE.Value = I_TORICODE           '取引先コード
                P_ORGCODE.Value = I_ORGCODE             '部門コード
                P_BIGCATECODE.Value = I_BIGCATECODE     '大分類コード
                P_MIDCATECODE.Value = I_MIDCATECODE     '中分類コード
                P_SMALLCATECODE.Value = I_SMALLCATECODE '小分類コード

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0014Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0014Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0014Chk.Load(SQLdr)

                    If LNM0014Chk.Rows.Count > 0 Then
                        Dim LNM0014row As DataRow
                        LNM0014row = LNM0014Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0014row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0014row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0014D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub
End Class