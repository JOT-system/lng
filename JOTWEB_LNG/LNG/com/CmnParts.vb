Option Explicit On
Imports MySql.Data.MySqlClient
Imports System.Data.SqlClient
Imports System.IO
Imports ClosedXML.Excel
Public Class CmnParts
    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    ''' <summary>
    ''' SQL検索処理
    ''' </summary>
    ''' <param name="I_STRSQL">SQL文字</param>
    ''' <remarks></remarks>
    Public Function SelectSearch(ByVal I_STRSQL As String) As DataTable
        Dim selectChecktbl As DataTable = Nothing
        If IsNothing(selectChecktbl) Then
            selectChecktbl = New DataTable
        End If
        If selectChecktbl.Columns.Count <> 0 Then
            selectChecktbl.Columns.Clear()
        End If
        selectChecktbl.Clear()

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)
            MySqlConnection.ClearPool(SQLcon)

            Using SQLcmd As New MySqlCommand(I_STRSQL, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        selectChecktbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    selectChecktbl.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return selectChecktbl
    End Function

    ''' <summary>
    ''' 変換マスタTBL検索
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <param name="I_CLASS">分類</param>
    ''' <param name="I_KEYCODE01">KEYコード01</param>
    ''' <param name="I_KEYCODE02">KEYコード02</param>
    ''' <param name="I_KEYCODE03">KEYコード03</param>
    ''' <param name="I_KEYCODE04">KEYコード04</param>
    ''' <param name="I_KEYCODE05">KEYコード05</param>
    ''' <param name="I_KEYCODE06">KEYコード06</param>
    ''' <param name="I_KEYCODE07">KEYコード07</param>
    ''' <param name="I_KEYCODE08">KEYコード08</param>
    ''' <param name="I_KEYCODE09">KEYコード09</param>
    ''' <param name="I_KEYCODE10">KEYコード10</param>
    ''' <param name="O_dtCONVERTMas">検索結果取得用</param>
    ''' <remarks></remarks>
    Public Sub SelectCONVERTMaster(ByVal SQLcon As MySqlConnection,
                                   ByVal I_CLASS As String, ByRef O_dtCONVERTMas As DataTable,
                                   Optional ByVal I_KEYCODE01 As String = Nothing, Optional ByVal I_KEYCODE02 As String = Nothing,
                                   Optional ByVal I_KEYCODE03 As String = Nothing, Optional ByVal I_KEYCODE04 As String = Nothing,
                                   Optional ByVal I_KEYCODE05 As String = Nothing, Optional ByVal I_KEYCODE06 As String = Nothing,
                                   Optional ByVal I_KEYCODE07 As String = Nothing, Optional ByVal I_KEYCODE08 As String = Nothing,
                                   Optional ByVal I_KEYCODE09 As String = Nothing, Optional ByVal I_KEYCODE10 As String = Nothing,
                                   Optional ByVal I_ORDERBY_KEY As String = Nothing)
        If IsNothing(O_dtCONVERTMas) Then
            O_dtCONVERTMas = New DataTable
        End If
        If O_dtCONVERTMas.Columns.Count <> 0 Then
            O_dtCONVERTMas.Columns.Clear()
        End If
        O_dtCONVERTMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "   LNM0005.CLASS "
        SQLStr &= " , LNM0005.KEYCODE01, LNM0005.KEYCODE02 "
        SQLStr &= " , LNM0005.KEYCODE03, LNM0005.KEYCODE04 "
        SQLStr &= " , LNM0005.KEYCODE05, LNM0005.KEYCODE06 "
        SQLStr &= " , LNM0005.KEYCODE07, LNM0005.KEYCODE08 "
        SQLStr &= " , LNM0005.KEYCODE09, LNM0005.KEYCODE10 "
        SQLStr &= " , LNM0005.VALUE01,   LNM0005.VALUE02 "
        SQLStr &= " , LNM0005.VALUE03,   LNM0005.VALUE04 "
        SQLStr &= " , LNM0005.VALUE05,   LNM0005.VALUE06 "
        SQLStr &= " , LNM0005.VALUE07,   LNM0005.VALUE08 "
        SQLStr &= " , LNM0005.VALUE09,   LNM0005.VALUE10 "
        SQLStr &= " , LNM0005.VALUE11,   LNM0005.VALUE12 "
        SQLStr &= " , LNM0005.VALUE13,   LNM0005.VALUE14 "
        SQLStr &= " , LNM0005.VALUE15,   LNM0005.VALUE16 "
        SQLStr &= " , LNM0005.VALUE17,   LNM0005.VALUE18 "
        SQLStr &= " , LNM0005.VALUE19,   LNM0005.VALUE20 "
        SQLStr &= " , LNM0005.CLASSNAME "
        SQLStr &= " , LNM0005.REMARKS "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0005_CONVERT LNM0005 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
        '〇KEYコード01チェック
        If Not IsNothing(I_KEYCODE01) Then
            SQLStr &= String.Format(" AND LNM0005.KEYCODE01 = '{0}' ", I_KEYCODE01)
        End If
        '〇KEYコード02チェック
        If Not IsNothing(I_KEYCODE02) Then
            SQLStr &= String.Format(" AND LNM0005.KEYCODE02 = '{0}' ", I_KEYCODE02)
        End If
        '〇KEYコード03チェック
        If Not IsNothing(I_KEYCODE03) Then
            SQLStr &= String.Format(" AND LNM0005.KEYCODE03 = '{0}' ", I_KEYCODE03)
        End If
        '〇KEYコード04チェック
        If Not IsNothing(I_KEYCODE04) Then
            SQLStr &= String.Format(" AND LNM0005.KEYCODE04 = '{0}' ", I_KEYCODE04)
        End If
        '〇KEYコード05チェック
        If Not IsNothing(I_KEYCODE05) Then
            SQLStr &= String.Format(" AND LNM0005.KEYCODE05 = '{0}' ", I_KEYCODE05)
        End If
        '〇KEYコード06チェック
        If Not IsNothing(I_KEYCODE06) Then
            SQLStr &= String.Format(" AND LNM0005.KEYCODE06 = '{0}' ", I_KEYCODE06)
        End If
        '〇KEYコード07チェック
        If Not IsNothing(I_KEYCODE07) Then
            SQLStr &= String.Format(" AND LNM0005.KEYCODE07 = '{0}' ", I_KEYCODE07)
        End If
        '〇KEYコード08チェック
        If Not IsNothing(I_KEYCODE08) Then
            SQLStr &= String.Format(" AND LNM0005.KEYCODE08 = '{0}' ", I_KEYCODE08)
        End If
        '〇KEYコード09チェック
        If Not IsNothing(I_KEYCODE09) Then
            SQLStr &= String.Format(" AND LNM0005.KEYCODE09 = '{0}' ", I_KEYCODE09)
        End If
        '〇KEYコード10チェック
        If Not IsNothing(I_KEYCODE10) Then
            SQLStr &= String.Format(" AND LNM0005.KEYCODE10 = '{0}' ", I_KEYCODE10)
        End If

        '〇ORDERBYKEY(SORT)チェック
        If Not IsNothing(I_ORDERBY_KEY) Then
            SQLStr &= String.Format(" ORDER BY {0} ", I_ORDERBY_KEY)
        End If

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtCONVERTMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtCONVERTMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 単価マスタTBL検索
    ''' </summary>
    Public Sub SelectTANKAMaster(ByVal SQLcon As MySqlConnection,
                                 ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByVal I_CLASS As String, ByRef O_dtTANKAMas As DataTable,
                                 Optional ByVal I_TODOKECODE As String = Nothing)
        If IsNothing(O_dtTANKAMas) Then
            O_dtTANKAMas = New DataTable
        End If
        If O_dtTANKAMas.Columns.Count <> 0 Then
            O_dtTANKAMas.Columns.Clear()
        End If
        O_dtTANKAMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0006.TORICODE "
        SQLStr &= "   ,LNM0006.TORINAME "
        SQLStr &= "   ,LNM0006.ORGCODE "
        SQLStr &= "   ,LNM0006.ORGNAME "
        SQLStr &= "   ,LNM0006.KASANORGCODE "
        SQLStr &= "   ,LNM0006.KASANORGNAME "
        SQLStr &= "   ,LNM0006.TODOKECODE "
        SQLStr &= "   ,LNM0006.BRANCHCODE AS TODOKEBRANCHCODE "
        SQLStr &= "   ,LNM0006.TODOKENAME "
        SQLStr &= "   ,LNM0006.STYMD "
        SQLStr &= "   ,LNM0006.ENDYMD "
        SQLStr &= "   ,LNM0006.TANKA "
        SQLStr &= "   ,LNM0006.SYAGATA "
        SQLStr &= "   ,CASE "
        SQLStr &= "    WHEN LNM0006.SYAGATA='1' THEN '単車' "
        SQLStr &= "    ELSE 'トレーラ' END AS SYAGATANAME "
        SQLStr &= "   ,LNM0006.SYAGOU "
        SQLStr &= "   ,LNM0006.SYABARA "
        SQLStr &= "   ,LNM0006.SYUBETSU "
        SQLStr &= "   ,LNM0006.BIKOU1 "
        SQLStr &= "   ,LNM0006.BIKOU2 "
        SQLStr &= "   ,LNM0006.BIKOU3 "
        SQLStr &= "   ,CAST(LNM0005.KEYCODE03 AS SIGNED) AS SORTNO "
        SQLStr &= "   ,CAST(LNM0005.VALUE04 AS SIGNED) AS MASTERNO "
        SQLStr &= "   ,LNM0005.VALUE01 AS TODOKENAME_MASTER "
        SQLStr &= "   ,LNM0005.VALUE06 AS TODOKENAME_SHEET "
        SQLStr &= "   ,LNM0005.KEYCODE08 AS GRPNO "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0006_TANKA LNM0006 "
        SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT LNM0005 ON "
        SQLStr &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
        SQLStr &= " AND LNM0005.KEYCODE01 = LNM0006.TODOKECODE "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0006.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0006.TORICODE = '{0}' ", I_TORICODE)
        If Not IsNothing(I_ORGCODE) Then
            SQLStr &= String.Format(" AND LNM0006.ORGCODE = '{0}' ", I_ORGCODE)
        End If
        SQLStr &= String.Format(" AND LNM0006.STYMD <= '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0006.ENDYMD >= '{0}' ", I_TAISHOYM)
        If Not IsNothing(I_TODOKECODE) Then
            SQLStr &= String.Format(" AND LNM0006.TODOKECODE = '{0}' ", I_TODOKECODE)
        End If

        '-- ORDER BY
        SQLStr &= " ORDER BY CAST(LNM0005.KEYCODE03 AS SIGNED), LNM0006.BRANCHCODE "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtTANKAMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtTANKAMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 統合版単価マスタTBL検索
    ''' </summary>
    Public Sub SelectNEWTANKAMaster(ByVal SQLcon As MySqlConnection,
                                    ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByVal I_CLASS As String, ByRef O_dtTANKAMas As DataTable,
                                    Optional ByVal I_TODOKECODE As String = Nothing, Optional ByVal I_SEKIYU_HONSHU_FLG As Boolean = False,
                                    Optional ByRef dtCenergyTodoke As DataTable = Nothing, Optional ByRef dtElNessTodoke As DataTable = Nothing)
        If IsNothing(O_dtTANKAMas) Then
            O_dtTANKAMas = New DataTable
        End If
        If O_dtTANKAMas.Columns.Count <> 0 Then
            O_dtTANKAMas.Columns.Clear()
        End If
        O_dtTANKAMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0006.TORICODE "
        SQLStr &= "   ,LNM0006.TORINAME "
        SQLStr &= "   ,LNM0006.ORGCODE "
        SQLStr &= "   ,LNM0006.ORGNAME "
        SQLStr &= "   ,LNM0006.KASANORGCODE "
        SQLStr &= "   ,LNM0006.KASANORGNAME "
        SQLStr &= "   ,LNM0006.AVOCADOSHUKABASHO "
        SQLStr &= "   ,LNM0006.AVOCADOSHUKANAME "
        SQLStr &= "   ,LNM0006.SHUKABASHO "
        SQLStr &= "   ,LNM0006.SHUKANAME "
        SQLStr &= "   ,LNM0006.AVOCADOTODOKECODE AS TODOKECODE "
        SQLStr &= "   ,LNM0006.AVOCADOTODOKENAME AS TODOKENAME "
        SQLStr &= "   ,LNM0006.TODOKECODE AS CONV_TODOKECODE "
        SQLStr &= "   ,LNM0006.TODOKENAME AS CONV_TODOKENAME "
        SQLStr &= "   ,LNM0006.TANKNUMBER "
        SQLStr &= "   ,LNM0006.SHABAN AS SYAGOU "
        SQLStr &= "   ,LNM0006.STYMD "
        SQLStr &= "   ,LNM0006.ENDYMD "
        SQLStr &= "   ,LPAD(LNM0006.BRANCHCODE,2,'0') AS TODOKEBRANCHCODE "
        SQLStr &= "   ,LNM0006.TANKAKBN "
        SQLStr &= "   ,LNM0006.MEMO "
        SQLStr &= "   ,LNM0006.TANKA "
        SQLStr &= "   ,LNM0006.CALCKBN AS SYUBETSU "
        SQLStr &= "   ,LNM0006.ROUNDTRIP "
        SQLStr &= "   ,LNM0006.TOLLFEE "
        SQLStr &= "   ,LNM0006.SYAGATA "
        SQLStr &= "   ,LNM0006.SYAGATANAME "
        SQLStr &= "   ,LNM0006.SYABARA "
        SQLStr &= "   ,LNM0006.BIKOU1 "
        SQLStr &= "   ,LNM0006.BIKOU2 "
        SQLStr &= "   ,LNM0006.BIKOU3 "

        '★取引先コードが「シーエナジー」の場合
        If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
            SQLStr &= "   ,CAST(LNM0005.KEYCODE03 AS SIGNED) AS SORTNO "
            SQLStr &= "   ,LNM0005.VALUE07 AS TODOKE_DISPLAY "
            SQLStr &= "   ,CAST(LNM0005.VALUE08 AS SIGNED) AS MASTERNO "
            SQLStr &= "   ,LNM0005.KEYCODE04 AS TODOKENAME_MASTER "
            'SQLStr &= "   ,'' AS TODOKENAME_SHEET "
            SQLStr &= "   ,'' AS GRPNO "
            SQLStr &= "   ,'' AS TODOKESHEET_CELL "
            SQLStr &= "   ,0 AS TODOKESHEET_DISPLAYFLG "
        ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0239900000 Then
            '★取引先コードが「北海道LNG」の場合
            SQLStr &= "   ,CAST(LNM0005.KEYCODE03 AS SIGNED) AS SORTNO "
            SQLStr &= "   ,CAST(LNM0005.VALUE04 AS SIGNED) AS MASTERNO "
            SQLStr &= "   ,LNM0005.VALUE01 AS TODOKENAME_MASTER "
            SQLStr &= "   ,LNM0005.VALUE06 AS TODOKENAME_SHEET "
            SQLStr &= "   ,'' AS GRPNO "
            SQLStr &= "   ,CAST(LNM0005.VALUE03 AS SIGNED) AS TODOKESHEET_CELL "
            SQLStr &= "   ,CAST(LNM0005.VALUE02 AS SIGNED) AS TODOKESHEET_DISPLAYFLG "
        ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 _
            AndAlso I_ORGCODE = BaseDllConst.CONST_ORDERORGCODE_020104 Then
            '★取引先コードが「石油資源開発(北海道)」の場合
            SQLStr &= "   ,CAST(LNM0005.KEYCODE03 AS SIGNED) AS SORTNO "
            SQLStr &= "   ,CAST(LNM0005.VALUE04 AS SIGNED) AS MASTERNO "
            SQLStr &= "   ,LNM0005.VALUE01 AS TODOKENAME_MASTER "
            SQLStr &= "   ,LNM0005.VALUE06 AS TODOKENAME_SHEET "
            SQLStr &= "   ,LNM0005.KEYCODE08 AS GRPNO "
            SQLStr &= "   ,CAST(LNM0005.VALUE03 AS SIGNED) AS TODOKESHEET_CELL "
            SQLStr &= "   ,CAST(LNM0005.VALUE02 AS SIGNED) AS TODOKESHEET_DISPLAYFLG "
        Else
            SQLStr &= "   ,CAST(LNM0005.KEYCODE03 AS SIGNED) AS SORTNO "
            SQLStr &= "   ,CAST(LNM0005.VALUE04 AS SIGNED) AS MASTERNO "
            SQLStr &= "   ,LNM0005.VALUE01 AS TODOKENAME_MASTER "
            SQLStr &= "   ,LNM0005.VALUE06 AS TODOKENAME_SHEET "
            SQLStr &= "   ,LNM0005.KEYCODE08 AS GRPNO "
            SQLStr &= "   ,'' AS TODOKESHEET_CELL "
            SQLStr &= "   ,0 AS TODOKESHEET_DISPLAYFLG "
        End If

        '-- FROM
        SQLStr &= " FROM LNG.LNM0006_NEWTANKA LNM0006 "
        SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT LNM0005 ON "
        SQLStr &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)

        '★取引先コードが「シーエナジー」の場合
        If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
            '〇北陸エルネスも含める
            SQLStr &= String.Format(" AND LNM0005.CLASS IN ('{0}','{1}') ", I_CLASS, "ELNESS_TODOKE")
            SQLStr &= " AND LNM0005.CLASSNAME = LNM0006.TORICODE "
            SQLStr &= " AND LNM0005.KEYCODE03 = LNM0006.TODOKECODE "
            'ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 AndAlso I_SEKIYU_HONSHU_FLG = True Then
            '    '★取引先コードが「石油資源開発(本州)」の場合
            '    SQLStr &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            '    SQLStr &= " AND LNM0005.KEYCODE01 = LNM0006.AVOCADOTODOKECODE "
            '    SQLStr &= " AND LNM0005.KEYCODE04 = LNM0006.KASANORGCODE "

        ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0239900000 Then
            '★取引先コードが「北海道LNG」の場合
            SQLStr &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.KEYCODE01 = LNM0006.AVOCADOTODOKECODE "
            SQLStr &= " AND LNM0005.KEYCODE04 = LNM0006.AVOCADOSHUKABASHO "

        Else
            SQLStr &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.KEYCODE01 = LNM0006.AVOCADOTODOKECODE "
        End If

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0006.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        '★取引先コードが「シーエナジー」の場合
        If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
            '〇北陸エルネスも含める
            SQLStr &= String.Format(" AND LNM0006.TORICODE IN ('{0}','{1}') ", I_TORICODE, BaseDllConst.CONST_TORICODE_0238900000)
        Else
            SQLStr &= String.Format(" AND LNM0006.TORICODE = '{0}' ", I_TORICODE)
        End If
        If Not IsNothing(I_ORGCODE) Then
            SQLStr &= String.Format(" AND LNM0006.ORGCODE = '{0}' ", I_ORGCODE)
        End If
        SQLStr &= String.Format(" AND LNM0006.STYMD <= '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0006.ENDYMD >= '{0}' ", I_TAISHOYM)
        If Not IsNothing(I_TODOKECODE) Then
            SQLStr &= String.Format(" AND LNM0006.AVOCADOTODOKECODE = '{0}' ", I_TODOKECODE)
        End If

        '-- ORDER BY
        SQLStr &= " ORDER BY LNM0006.TORICODE, LNM0006.ORGCODE, LNM0006.SHUKABASHO, CAST(LNM0005.KEYCODE03 AS SIGNED), LNM0006.BRANCHCODE "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtTANKAMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtTANKAMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        '★取引先コードが「シーエナジー」の場合
        If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
            '★項目[単価]作成
            '〇基準シート用
            'O_dtTANKAMas.Columns.Add("GRPNO", Type.GetType("System.String"))
            O_dtTANKAMas.Columns.Add("SHEET_CELLNO01", Type.GetType("System.String"))
            O_dtTANKAMas.Columns.Add("SHEET_CELLNO02", Type.GetType("System.String"))
            O_dtTANKAMas.Columns.Add("SHEET_CELLNO03", Type.GetType("System.String"))
            O_dtTANKAMas.Columns.Add("SHEET_CELLNO04", Type.GetType("System.String"))
            '〇マスタシート用
            O_dtTANKAMas.Columns.Add("MASTER_CELLLINE", Type.GetType("System.String"))
            O_dtTANKAMas.Columns.Add("MASTER_CELLTANI", Type.GetType("System.String"))
            O_dtTANKAMas.Columns.Add("MASTER_CELLKYORITANKA", Type.GetType("System.String"))
            O_dtTANKAMas.Columns.Add("MASTER_CELLKIHONUNCHIN", Type.GetType("System.String"))

            Dim dtCENERGY_TANK As New DataTable
            '〇変換マスタ(陸事番号(シーエナジー))取得
            SelectCONVERTMaster(SQLcon, "CENERGY_TANK", dtCENERGY_TANK)
            Try
                For Each dtCENERGY_TANKrow As DataRow In dtCENERGY_TANK.Rows
                    Dim condition As String = ""
                    condition &= String.Format("SYAGOU = '{0}'", dtCENERGY_TANKrow("KEYCODE04").ToString())
                    For Each O_dtTANKAMasrow As DataRow In O_dtTANKAMas.Select(condition)
                        O_dtTANKAMasrow("GRPNO") = dtCENERGY_TANKrow("KEYCODE03").ToString()
                        O_dtTANKAMasrow("SHEET_CELLNO01") = dtCENERGY_TANKrow("VALUE16").ToString()
                        O_dtTANKAMasrow("SHEET_CELLNO02") = dtCENERGY_TANKrow("VALUE17").ToString()
                        O_dtTANKAMasrow("SHEET_CELLNO03") = dtCENERGY_TANKrow("VALUE18").ToString()
                        O_dtTANKAMasrow("SHEET_CELLNO04") = dtCENERGY_TANKrow("VALUE19").ToString()

                        O_dtTANKAMasrow("MASTER_CELLLINE") = dtCENERGY_TANKrow("VALUE11").ToString()
                        O_dtTANKAMasrow("MASTER_CELLTANI") = dtCENERGY_TANKrow("VALUE12").ToString()
                        O_dtTANKAMasrow("MASTER_CELLKYORITANKA") = dtCENERGY_TANKrow("VALUE13").ToString()
                        O_dtTANKAMasrow("MASTER_CELLKIHONUNCHIN") = dtCENERGY_TANKrow("VALUE14").ToString()
                    Next
                Next
            Catch ex As Exception

            End Try

            '〇統合版単価マスタ(届先(グルーピング))取得
            Try
                '①シーエナジー
                Dim grCenergyTankaMas = From row In O_dtTANKAMas.AsEnumerable()
                                        Where row.Field(Of String)("TORICODE") = BaseDllConst.CONST_TORICODE_0110600000
                                        Group row By CONV_TODOKECODE = row.Field(Of String)("CONV_TODOKECODE"),
                                                     CONV_TODOKENAME = row.Field(Of String)("CONV_TODOKENAME"),
                                                     TODOKECODE = row.Field(Of String)("TODOKECODE"),
                                                     TODOKENAME = row.Field(Of String)("TODOKENAME") Into Group
                                        Select New With {
                                            .CONV_TODOKECODE = CONV_TODOKECODE,
                                            .CONV_TODOKENAME = CONV_TODOKENAME,
                                            .TODOKECODE = TODOKECODE,
                                            .TODOKENAME = TODOKENAME
                                        }

                '★基準シート(コード、届先)統合版単価マスタより取得
                For Each result In grCenergyTankaMas
                    Dim resCONV_TODOKECODE = result.CONV_TODOKECODE
                    Dim resCONV_TODOKENAME = result.CONV_TODOKENAME
                    Dim resTODOKECODE = result.TODOKECODE
                    Dim resTODOKENAME = result.TODOKENAME
                    Dim condition As String = ""
                    condition &= String.Format("KEYCODE03 = '{0}'", resCONV_TODOKECODE)
                    For Each dtCenergyTodokerow As DataRow In dtCenergyTodoke.Select(condition)
                        dtCenergyTodokerow("KEYCODE03") = resCONV_TODOKECODE
                        dtCenergyTodokerow("KEYCODE04") = resCONV_TODOKENAME
                        dtCenergyTodokerow("KEYCODE01") = resTODOKECODE
                        dtCenergyTodokerow("KEYCODE02") = resTODOKENAME
                    Next
                Next
            Catch ex As Exception
            End Try

            Try
                '②エルネス
                Dim grElNessTankaMas = From row In O_dtTANKAMas.AsEnumerable()
                                       Where row.Field(Of String)("TORICODE") = BaseDllConst.CONST_TORICODE_0238900000
                                       Group row By CONV_TODOKECODE = row.Field(Of String)("CONV_TODOKECODE"),
                                                    CONV_TODOKENAME = row.Field(Of String)("CONV_TODOKENAME"),
                                                    TODOKECODE = row.Field(Of String)("TODOKECODE"),
                                                    TODOKENAME = row.Field(Of String)("TODOKENAME") Into Group
                                       Select New With {
                                            .CONV_TODOKECODE = CONV_TODOKECODE,
                                            .CONV_TODOKENAME = CONV_TODOKENAME,
                                            .TODOKECODE = TODOKECODE,
                                            .TODOKENAME = TODOKENAME
                                        }

                '★基準シート(コード、届先)統合版単価マスタより取得
                For Each result In grElNessTankaMas
                    Dim resCONV_TODOKECODE = result.CONV_TODOKECODE
                    Dim resCONV_TODOKENAME = result.CONV_TODOKENAME
                    Dim resTODOKECODE = result.TODOKECODE
                    Dim resTODOKENAME = result.TODOKENAME
                    Dim condition As String = ""
                    condition &= String.Format("KEYCODE03 = '{0}'", resCONV_TODOKECODE)
                    For Each dtElNessTodokerow As DataRow In dtElNessTodoke.Select(condition)
                        dtElNessTodokerow("KEYCODE03") = resCONV_TODOKECODE
                        dtElNessTodokerow("KEYCODE04") = resCONV_TODOKENAME
                        dtElNessTodokerow("KEYCODE01") = resTODOKECODE
                        dtElNessTodokerow("KEYCODE02") = resTODOKENAME
                    Next
                Next

            Catch ex As Exception
            End Try

        End If

    End Sub

    ''' <summary>
    ''' 固定費マスタTBL検索
    ''' </summary>
    Public Sub SelectKOTEIHIMaster(ByVal SQLcon As MySqlConnection,
                                   ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByRef O_dtKOTEIHIMas As DataTable,
                                   Optional ByVal I_CLASS As String = Nothing,
                                   Optional ByVal I_RIKUBAN As String = Nothing)
        If IsNothing(O_dtKOTEIHIMas) Then
            O_dtKOTEIHIMas = New DataTable
        End If
        If O_dtKOTEIHIMas.Columns.Count <> 0 Then
            O_dtKOTEIHIMas.Columns.Clear()
        End If
        O_dtKOTEIHIMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0007.TORICODE "
        SQLStr &= "   ,LNM0007.TORINAME "
        SQLStr &= "   ,LNM0007.ORGCODE "
        SQLStr &= "   ,LNM0007.ORGNAME "
        SQLStr &= "   ,LNM0007.KASANORGCODE "
        SQLStr &= "   ,LNM0007.KASANORGNAME "
        SQLStr &= "   ,LNM0007.STYMD "
        SQLStr &= "   ,LNM0007.ENDYMD "
        SQLStr &= "   ,LNM0007.SYABAN "
        SQLStr &= "   ,LNM0007.RIKUBAN "
        SQLStr &= "   ,LNM0007.SYAGATA "
        SQLStr &= "   ,LNM0007.SYAGATANAME "
        SQLStr &= "   ,LNM0007.SYABARA "
        SQLStr &= "   ,LNM0007.KOTEIHI "
        SQLStr &= "   ,LNM0007.BIKOU1 "
        SQLStr &= "   ,LNM0007.BIKOU2 "
        SQLStr &= "   ,LNM0007.BIKOU3 "
        If Not IsNothing(I_CLASS) Then
            SQLStr &= "   ,LNM0005.VALUE08 AS KOTEIHI_CELLNUM "
        End If

        '-- FROM
        SQLStr &= " FROM LNG.LNM0007_KOTEIHI LNM0007 "
        If Not IsNothing(I_CLASS) Then
            SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT LNM0005 ON "
            SQLStr &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.KEYCODE01 = LNM0007.RIKUBAN "
        End If

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0007.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0007.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= String.Format(" AND LNM0007.ORGCODE = '{0}' ", I_ORGCODE)
        SQLStr &= String.Format(" AND LNM0007.STYMD <= '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0007.ENDYMD >= '{0}' ", I_TAISHOYM)
        '★陸事番号が指定されている場合
        If Not IsNothing(I_RIKUBAN) Then
            SQLStr &= String.Format(" AND LNM0007.RIKUBAN = '{0}' ", I_RIKUBAN)
        End If

        '-- ORDER BY
        If Not IsNothing(I_CLASS) Then
            SQLStr &= " ORDER BY CAST(LNM0005.VALUE08 AS SIGNED) "
        End If

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtKOTEIHIMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtKOTEIHIMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 統合版固定費マスタTBL検索
    ''' </summary>
    Public Sub SelectFIXEDMaster(ByVal SQLcon As MySqlConnection,
                                 ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByRef O_dtFIXEDMas As DataTable,
                                 Optional ByVal I_CLASS As String = Nothing,
                                 Optional ByVal I_RIKUBAN As String = Nothing)
        If IsNothing(O_dtFIXEDMas) Then
            O_dtFIXEDMas = New DataTable
        End If
        If O_dtFIXEDMas.Columns.Count <> 0 Then
            O_dtFIXEDMas.Columns.Clear()
        End If
        O_dtFIXEDMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0007.TORICODE "
        SQLStr &= "   ,LNM0007.TORINAME "
        SQLStr &= "   ,LNM0007.ORGCODE "
        SQLStr &= "   ,LNM0007.ORGNAME "
        SQLStr &= "   ,LNM0007.KASANORGCODE "
        SQLStr &= "   ,LNM0007.KASANORGNAME "
        SQLStr &= "   ,LNM0007.TARGETYM "
        SQLStr &= "   ,LNM0007.SYABAN "
        SQLStr &= "   ,LNM0007.RIKUBAN "
        SQLStr &= "   ,LNM0007.SYAGATA "
        SQLStr &= "   ,LNM0007.SYAGATANAME "
        SQLStr &= "   ,LNM0007.SYABARA "
        SQLStr &= "   ,LNM0007.SEASONKBN "
        SQLStr &= "   ,LNM0007.SEASONSTART "
        SQLStr &= "   ,LNM0007.SEASONEND "
        SQLStr &= "   ,IFNULL(LNM0007.KOTEIHIM,0) AS KOTEIHI "
        SQLStr &= "   ,IFNULL(LNM0007.KOTEIHID,0) AS KOTEIHID "
        SQLStr &= "   ,IFNULL(LNM0007.KAISU,0) AS KAISU "
        SQLStr &= "   ,IFNULL(LNM0007.GENGAKU,0) AS GENGAKU "
        SQLStr &= "   ,IFNULL(LNM0007.AMOUNT,0) AS AMOUNT "
        SQLStr &= "   ,LNM0007.BIKOU1 "
        SQLStr &= "   ,LNM0007.BIKOU2 "
        SQLStr &= "   ,LNM0007.BIKOU3 "
        If Not IsNothing(I_CLASS) Then
            '--★シーエナジーの場合
            If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
                SQLStr &= "   ,LNM0005.VALUE11 AS KOTEIHI_CELLNUM "
                SQLStr &= "   ,LNM0005.VALUE12 AS KOTEIHI_CELL01 "
                SQLStr &= "   ,LNM0005.VALUE13 AS KOTEIHI_CELL02 "
                SQLStr &= "   ,LNM0005.VALUE14 AS KOTEIHI_CELL03 "
                SQLStr &= "   ,LNM0005.VALUE15 AS KOTEIHI_CELL04 "
            Else
                SQLStr &= "   ,LNM0005.VALUE08 AS KOTEIHI_CELLNUM "
            End If
        End If

        '-- FROM
        SQLStr &= " FROM LNG.LNM0007_FIXED LNM0007 "
        If Not IsNothing(I_CLASS) Then
            SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT LNM0005 ON "
            SQLStr &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            '--★シーエナジーの場合
            If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
                SQLStr &= " AND LNM0005.KEYCODE04 = LNM0007.SYABAN "
            Else
                SQLStr &= " AND LNM0005.KEYCODE01 = LNM0007.RIKUBAN "
            End If
        End If

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0007.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        '--★シーエナジーの場合(北陸エルネスも含める)
        If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
            SQLStr &= String.Format(" AND LNM0007.TORICODE IN ('{0}','{1}') ", I_TORICODE, BaseDllConst.CONST_TORICODE_0238900000)
        Else
            SQLStr &= String.Format(" AND LNM0007.TORICODE = '{0}' ", I_TORICODE)
        End If
        SQLStr &= String.Format(" AND LNM0007.ORGCODE IN ({0}) ", I_ORGCODE)
        SQLStr &= String.Format(" AND LNM0007.TARGETYM = '{0}' ", I_TAISHOYM)
        '★陸事番号が指定されている場合
        If Not IsNothing(I_RIKUBAN) Then
            '--★シーエナジーの場合(車番)
            If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
                SQLStr &= String.Format(" AND LNM0007.SYABAN = '{0}' ", I_RIKUBAN)
            Else
                SQLStr &= String.Format(" AND LNM0007.RIKUBAN = '{0}' ", I_RIKUBAN)
            End If
        End If

        '-- ORDER BY
        If Not IsNothing(I_CLASS) Then
            '--★シーエナジーの場合
            If I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
                SQLStr &= " ORDER BY CAST(LNM0005.VALUE11 AS SIGNED) "
            Else
                SQLStr &= " ORDER BY CAST(LNM0005.VALUE08 AS SIGNED) "
            End If
        End If

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtFIXEDMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtFIXEDMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    ''' <summary>
    ''' SK固定運賃マスタTBL検索
    ''' </summary>
    Public Sub SelectSKKOTEIHIMaster(ByVal SQLcon As MySqlConnection,
                                     ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByRef O_dtSKKOTEIHIMas As DataTable,
                                     Optional ByVal I_CLASS As String = Nothing,
                                     Optional ByVal I_SYABAN As String = Nothing)
        If IsNothing(O_dtSKKOTEIHIMas) Then
            O_dtSKKOTEIHIMas = New DataTable
        End If
        If O_dtSKKOTEIHIMas.Columns.Count <> 0 Then
            O_dtSKKOTEIHIMas.Columns.Clear()
        End If
        O_dtSKKOTEIHIMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0008.TORICODE "
        SQLStr &= "   ,LNM0008.TORINAME "
        SQLStr &= "   ,LNM0008.ORGCODE "
        SQLStr &= "   ,LNM0008.ORGNAME "
        SQLStr &= "   ,LNM0008.KASANORGCODE "
        SQLStr &= "   ,LNM0008.KASANORGNAME "
        'SQLStr &= "   ,LNM0008.TAISHOYM "
        SQLStr &= "   ,LNM0008.STYMD "
        SQLStr &= "   ,LNM0008.ENDYMD "
        SQLStr &= "   ,LNM0008.SYABAN "
        SQLStr &= "   ,LNM0008.SYABARA "
        SQLStr &= "   ,LNM0008.GETSUGAKU "
        SQLStr &= "   ,LNM0008.GENGAKU "
        SQLStr &= "   ,LNM0008.KOTEIHI "
        SQLStr &= "   ,LNM0008.BIKOU "
        If Not IsNothing(I_CLASS) Then
            SQLStr &= "   ,LNM0005.VALUE01 AS KOTEIHI_DISPLAY "
            SQLStr &= "   ,LNM0005.VALUE02 AS KOTEIHI_CELLNUM "
        End If

        '-- FROM
        SQLStr &= " FROM LNG.LNM0008_SKKOTEIHI LNM0008 "
        If Not IsNothing(I_CLASS) Then
            SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT LNM0005 ON "
            SQLStr &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.KEYCODE04 = LNM0008.SYABAN "
        End If

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0008.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0008.TORICODE = '{0}' ", I_TORICODE)
        'SQLStr &= String.Format(" AND LNM0008.TAISHOYM <= '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0008.STYMD <= '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0008.ENDYMD >= '{0}' ", I_TAISHOYM)
        '★部門コードが指定されている場合
        If Not IsNothing(I_ORGCODE) Then
            SQLStr &= String.Format(" AND LNM0008.ORGCODE = '{0}' ", I_ORGCODE)
        End If
        '★車番が指定されている場合
        If Not IsNothing(I_SYABAN) Then
            SQLStr &= String.Format(" AND LNM0008.SYABAN = '{0}' ", I_SYABAN)
        End If

        '-- ORDER BY
        If Not IsNothing(I_CLASS) Then
            SQLStr &= " ORDER BY CAST(LNM0005.KEYCODE05 AS SIGNED),CAST(LNM0005.KEYCODE06 AS SIGNED) "
        End If

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtSKKOTEIHIMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtSKKOTEIHIMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 統合版固定費マスタTBL検索(SK用)
    ''' </summary>
    Public Sub SelectSKFIXEDMaster(ByVal SQLcon As MySqlConnection,
                                   ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByRef O_dtSKFIXEDMas As DataTable,
                                   Optional ByVal I_CLASS As String = Nothing,
                                   Optional ByVal I_SYABAN As String = Nothing)
        If IsNothing(O_dtSKFIXEDMas) Then
            O_dtSKFIXEDMas = New DataTable
        End If
        If O_dtSKFIXEDMas.Columns.Count <> 0 Then
            O_dtSKFIXEDMas.Columns.Clear()
        End If
        O_dtSKFIXEDMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0007.TORICODE "
        SQLStr &= "   ,LNM0007.TORINAME "
        SQLStr &= "   ,LNM0007.ORGCODE "
        SQLStr &= "   ,LNM0007.ORGNAME "
        SQLStr &= "   ,LNM0007.KASANORGCODE "
        SQLStr &= "   ,LNM0007.KASANORGNAME "
        SQLStr &= "   ,LNM0007.TARGETYM "
        SQLStr &= "   ,LNM0007.SYABAN "
        SQLStr &= "   ,LNM0007.RIKUBAN "
        SQLStr &= "   ,LNM0007.SYAGATA "
        SQLStr &= "   ,LNM0007.SYAGATANAME "
        SQLStr &= "   ,LNM0007.SYABARA "
        SQLStr &= "   ,LNM0007.SEASONKBN "
        SQLStr &= "   ,LNM0007.SEASONSTART "
        SQLStr &= "   ,LNM0007.SEASONEND "
        SQLStr &= "   ,IFNULL(LNM0007.KOTEIHIM,0) AS GETSUGAKU "
        SQLStr &= "   ,IFNULL(LNM0007.KOTEIHID,0) AS KOTEIHID "
        SQLStr &= "   ,IFNULL(LNM0007.KAISU,0) AS KAISU "
        SQLStr &= "   ,IFNULL(LNM0007.GENGAKU,0) AS GENGAKU "
        SQLStr &= "   ,IFNULL(LNM0007.AMOUNT,0) AS AMOUNT "
        SQLStr &= "   ,LNM0007.BIKOU1 "
        SQLStr &= "   ,LNM0007.BIKOU2 "
        SQLStr &= "   ,LNM0007.BIKOU3 "
        If Not IsNothing(I_CLASS) Then
            SQLStr &= "   ,LNM0005.VALUE01 AS KOTEIHI_DISPLAY "
            SQLStr &= "   ,LNM0005.VALUE02 AS KOTEIHI_CELLNUM "
        End If

        '-- FROM
        SQLStr &= " FROM LNG.LNM0007_FIXED LNM0007 "
        If Not IsNothing(I_CLASS) Then
            SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT LNM0005 ON "
            SQLStr &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.KEYCODE04 = LNM0007.SYABAN "
        End If

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0007.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0007.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= String.Format(" AND LNM0007.ORGCODE IN ({0}) ", I_ORGCODE)
        SQLStr &= String.Format(" AND LNM0007.TARGETYM = '{0}' ", I_TAISHOYM)

        '★車番が指定されている場合
        If Not IsNothing(I_SYABAN) Then
            SQLStr &= String.Format(" AND LNM0007.SYABAN = '{0}' ", I_SYABAN)
        End If

        '-- ORDER BY
        If Not IsNothing(I_CLASS) Then
            SQLStr &= " ORDER BY CAST(LNM0005.KEYCODE05 AS SIGNED),CAST(LNM0005.KEYCODE06 AS SIGNED) "
        End If

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtSKFIXEDMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtSKFIXEDMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    ''' <summary>
    ''' SK固定値マスタTBL検索
    ''' </summary>
    Public Sub SelectSKKOTEICHIMaster(ByVal SQLcon As MySqlConnection, ByVal I_dtTANKAMas As DataTable, ByRef O_dtSKKOTEICHIMas As DataTable,
                                      Optional ByRef O_dtSKKOTEICHIOtherMas As DataTable = Nothing)
        If IsNothing(O_dtSKKOTEICHIMas) Then
            O_dtSKKOTEICHIMas = New DataTable
        End If
        If O_dtSKKOTEICHIMas.Columns.Count <> 0 Then
            O_dtSKKOTEICHIMas.Columns.Clear()
        End If
        O_dtSKKOTEICHIMas.Clear()

        Dim SQLStr As String = ""
        Dim arrAkitaSyaban As String() = {"宿泊有", "宿泊無"}
        Dim arrAkitaSyaban01 As String() = {String.Format("334号車({0})", arrAkitaSyaban(0)), String.Format("329号車({0})", arrAkitaSyaban(0))}
        Dim arrAkitaSyaban02 As String() = {String.Format("334号車({0})", arrAkitaSyaban(1)), String.Format("329号車({0})", arrAkitaSyaban(1))}

        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0005.CLASS "
        SQLStr &= "  , CASE "
        SQLStr &= "    WHEN LNM0005_TODOKE.KEYCODE08 = '4' THEN '5' "
        SQLStr &= "    WHEN LNM0005_TODOKE.KEYCODE08 = '3' THEN '4' "
        SQLStr &= "    WHEN LNM0005_TODOKE.KEYCODE08 = '2' THEN '3' "
        SQLStr &= "    WHEN LNM0005_TODOKE.KEYCODE08 = '1' AND (LNM0005.VALUE05='45' OR LNM0005.VALUE05 = '126') THEN '2' "
        SQLStr &= "    ELSE '1' "
        SQLStr &= "    END AS GRPNO "
        SQLStr &= "  , LNM0005_TODOKE.KEYCODE01 AS TODOKENO "
        SQLStr &= "  , LNM0005_TODOKE.VALUE01   AS TODOKENAME "
        SQLStr &= "  , LNM0005.VALUE05          AS MEISAI_GYO "
        SQLStr &= "  , LNM0005.VALUE07          AS MEISAI_HYOJIFLG "
        SQLStr &= "  , LNM0005.KEYCODE03        AS KOTEICHI_GYOMU "
        SQLStr &= "  , LNM0005.KEYCODE04        AS KOTEICHI_GYOMUNO "
        SQLStr &= "  , CASE "
        SQLStr &= String.Format("    WHEN LNM0005.KEYCODE03='{0}' OR LNM0005.KEYCODE03='{1}' THEN '01' ", arrAkitaSyaban01(0), arrAkitaSyaban01(1))
        SQLStr &= String.Format("    WHEN LNM0005.KEYCODE03='{0}' OR LNM0005.KEYCODE03='{1}' THEN '02' ", arrAkitaSyaban02(0), arrAkitaSyaban02(1))
        SQLStr &= "    ELSE '' "
        SQLStr &= "    END AS KOTEICHI_GYOMUNOSUB "
        SQLStr &= "  , LNM0005.VALUE18          AS KOTEICHI_YOKOCELL "
        SQLStr &= "  , (4 + CAST(LNM0005_TODOKE.KEYCODE03 AS SIGNED)) - 1 AS SET_CELL "
        SQLStr &= "  , LPAD(IF(LNM0005.KEYCODE09='','1',LNM0005.KEYCODE09), 2, '0') AS BRANCHCODE "
        'SQLStr &= "  , LPAD(LNM0005.KEYCODE09, 2, '0') AS BRANCHCODE "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0005_CONVERT LNM0005 "

        '-- LEFT JOIN
        SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT LNM0005_TODOKE ON "
        SQLStr &= String.Format("     LNM0005_TODOKE.CLASS = '{0}' ", "SEKIYUSIG_TODOKE_MAS")
        SQLStr &= " AND LNM0005_TODOKE.KEYCODE08 = LNM0005.VALUE19 "
        SQLStr &= String.Format(" AND LNM0005_TODOKE.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        '-- WHERE
        'SQLStr &= String.Format(" WHERE LNM0005.CLASS = '{0}' ", "SEKIYUSIGEN_TANK")
        SQLStr &= " WHERE LNM0005.CLASS = @CLASS "
        SQLStr &= String.Format(" AND LNM0005.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)
        SQLStr &= "   AND LNM0005_TODOKE.VALUE01 NOT LIKE 'TMP%' "

#Region "コメント"
        ''★１控え用
        'Dim SQLStrWork As String = SQLStr
        'SQLStrWork &= " AND LNM0005.KEYCODE07 = LNM0005_TODOKE.KEYCODE01 "
        'SQLStrWork &= " AND LNM0005.KEYCODE05 = LNM0005_TODOKE.KEYCODE06 "

        ''★２（★１で取得する内容を省く条件）
        'SQLStr &= String.Format(" AND LNM0005_TODOKE.KEYCODE01 NOT IN ('{0}', '{1}') ", BaseDllConst.CONST_TODOKECODE_004012, BaseDllConst.CONST_TODOKECODE_005890)
#End Region

        '-- ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "   LNM0005.VALUE19 "
        SQLStr &= " , CAST(LNM0005.VALUE05 AS SIGNED) "
        SQLStr &= " , LNM0005_TODOKE.VALUE02 "
        SQLStr &= " , LNM0005_TODOKE.VALUE03 "
        SQLStr &= " , LNM0005.VALUE18 "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim P_CLASS As MySqlParameter = SQLcmd.Parameters.Add("@CLASS", MySqlDbType.VarChar)                              '
                P_CLASS.Value = "SEKIYUSIGEN_TANK"

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtSKKOTEICHIMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtSKKOTEICHIMas.Load(SQLdr)
                End Using
            End Using
#Region "コメント"
            'If IsNothing(O_dtSKKOTEICHIOtherMas) Then
            '    O_dtSKKOTEICHIOtherMas = New DataTable
            'End If
            'If O_dtSKKOTEICHIOtherMas.Columns.Count <> 0 Then
            '    O_dtSKKOTEICHIOtherMas.Columns.Clear()
            'End If
            'O_dtSKKOTEICHIOtherMas.Clear()
            'Using SQLcmd As New MySqlCommand(SQLStrWork, SQLcon)
            '    Dim P_CLASS As MySqlParameter = SQLcmd.Parameters.Add("@CLASS", MySqlDbType.VarChar)                              '
            '    P_CLASS.Value = "SEKIYUSIGEN_TANK_OTR"

            '    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
            '        '○ フィールド名とフィールドの型を取得
            '        For index As Integer = 0 To SQLdr.FieldCount - 1
            '            O_dtSKKOTEICHIOtherMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
            '        Next

            '        '○ テーブル検索結果をテーブル格納
            '        'O_dtSKKOTEICHIOtherMas.Load(SQLdr)
            '        O_dtSKKOTEICHIMas.Load(SQLdr)
            '    End Using
            'End Using
#End Region

        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        '★項目[単価]作成
        O_dtSKKOTEICHIMas.Columns.Add("TANKA", Type.GetType("System.Decimal"))
        'O_dtSKKOTEICHIMas.Columns.Add("BRANCHCODE", Type.GetType("System.String"))
        O_dtSKKOTEICHIMas.Columns.Add("TANKAKBN", Type.GetType("System.String"))
        O_dtSKKOTEICHIMas.Columns.Add("MEMO", Type.GetType("System.String"))
        '-- 〇茨城
        Dim conditionSub As String = "GRPNO='4'"
        For Each dtTANKAMasrow As DataRow In I_dtTANKAMas.Select(conditionSub)
            Dim condition As String = "GRPNO='5' "
            condition &= String.Format("AND TODOKENO ='{0}' ", dtTANKAMasrow("TODOKECODE").ToString())
            condition &= String.Format("AND KOTEICHI_GYOMUNO ='{0}' ", dtTANKAMasrow("SYAGOU").ToString())
            condition &= String.Format("AND BRANCHCODE ='{0}' ", dtTANKAMasrow("TODOKEBRANCHCODE").ToString())
            For Each dtSKKOTEICHIMasrow As DataRow In O_dtSKKOTEICHIMas.Select(condition)
                dtSKKOTEICHIMasrow("TANKA") = dtTANKAMasrow("TANKA")
                dtSKKOTEICHIMasrow("TANKAKBN") = dtTANKAMasrow("TANKAKBN")
                dtSKKOTEICHIMasrow("MEMO") = dtTANKAMasrow("MEMO")

                ''★単価調整がある場合
                'Dim iSET_CELL As Integer = 0
                'Select Case dtSKKOTEICHIMasrow("BRANCHCODE").ToString()
                '    Case "02"
                '        '■不積料金
                '        iSET_CELL = CInt(dtSKKOTEICHIMasrow("SET_CELL").ToString())
                '        iSET_CELL += 18
                '        dtSKKOTEICHIMasrow("SET_CELL") = iSET_CELL.ToString()
                'End Select
            Next
        Next

        '-- 〇東北
        conditionSub = "GRPNO='3'"
        For Each dtTANKAMasrow As DataRow In I_dtTANKAMas.Select(conditionSub)
            Dim condition As String = "GRPNO='4' "
            condition &= String.Format("AND TODOKENO ='{0}' ", dtTANKAMasrow("TODOKECODE").ToString())
            condition &= String.Format("AND KOTEICHI_GYOMUNO ='{0}' ", dtTANKAMasrow("SYAGOU").ToString())
            condition &= String.Format("AND BRANCHCODE ='{0}' ", dtTANKAMasrow("TODOKEBRANCHCODE").ToString())
            For Each dtSKKOTEICHIMasrow As DataRow In O_dtSKKOTEICHIMas.Select(condition)
                dtSKKOTEICHIMasrow("TANKA") = dtTANKAMasrow("TANKA")
                dtSKKOTEICHIMasrow("TANKAKBN") = dtTANKAMasrow("TANKAKBN")
                dtSKKOTEICHIMasrow("MEMO") = dtTANKAMasrow("MEMO")

                ''★単価調整がある場合
                'Dim iSET_CELL As Integer = 0
                'Select Case dtSKKOTEICHIMasrow("BRANCHCODE").ToString()
                '    Case "02"
                '        '■不積料金
                '        iSET_CELL = CInt(dtSKKOTEICHIMasrow("SET_CELL").ToString())
                '        iSET_CELL += 18
                '        dtSKKOTEICHIMasrow("SET_CELL") = iSET_CELL.ToString()
                'End Select
            Next
        Next

        '-- 〇秋田
        'conditionSub = String.Format("GRPNO='2' AND ORGCODE='{0}' ", BaseDllConst.CONST_ORDERORGCODE_020601)
        'conditionSub = String.Format("GRPNO IN ('1','2') AND ORGCODE='{0}' ", BaseDllConst.CONST_ORDERORGCODE_020601)
        conditionSub = String.Format(" GRPNO IN ('1','2') AND (ORGCODE='{0}' OR ORGCODE='{1}') ", BaseDllConst.CONST_ORDERORGCODE_021502, BaseDllConst.CONST_ORDERORGCODE_020601)
        conditionSub &= String.Format(" AND AVOCADOSHUKABASHO='{0}' ", "005690")
        For Each dtTANKAMasrow As DataRow In I_dtTANKAMas.Select(conditionSub)
            'Dim condition As String = "GRPNO='3' "
            Dim condition As String = "GRPNO IN ('1','3') "
            condition &= String.Format("AND TODOKENO ='{0}' ", dtTANKAMasrow("TODOKECODE").ToString())
            condition &= String.Format("AND KOTEICHI_GYOMUNO ='{0}' ", dtTANKAMasrow("SYAGOU").ToString())
            condition &= String.Format("AND BRANCHCODE ='{0}' ", dtTANKAMasrow("TODOKEBRANCHCODE").ToString())
            For Each dtSKKOTEICHIMasrow As DataRow In O_dtSKKOTEICHIMas.Select(condition)
                dtSKKOTEICHIMasrow("TANKA") = dtTANKAMasrow("TANKA")
                dtSKKOTEICHIMasrow("TANKAKBN") = dtTANKAMasrow("TANKAKBN")
                dtSKKOTEICHIMasrow("MEMO") = dtTANKAMasrow("MEMO")

            Next
        Next
        '-- ★秋田(宿泊有)
        'conditionSub = String.Format("GRPNO='2' AND (ORGCODE='{0}' OR ORGCODE='{1}') ", BaseDllConst.CONST_ORDERORGCODE_021502, BaseDllConst.CONST_ORDERORGCODE_020601)
        conditionSub = String.Format("GRPNO='2' AND AVOCADOSHUKABASHO='{0}' ", "005690")
        conditionSub &= String.Format("AND MEMO='{0}' ", arrAkitaSyaban(0))
        For Each dtTANKAMasrow As DataRow In I_dtTANKAMas.Select(conditionSub)
            Dim condition As String = "GRPNO='3' "
            condition &= String.Format("AND TODOKENO ='{0}' ", dtTANKAMasrow("TODOKECODE").ToString())
            condition &= String.Format("AND KOTEICHI_GYOMUNO ='{0}' ", dtTANKAMasrow("SYAGOU").ToString())
            condition &= "AND KOTEICHI_GYOMUNOSUB ='01' "
            condition &= String.Format("AND BRANCHCODE ='{0}' ", dtTANKAMasrow("TODOKEBRANCHCODE").ToString())
            For Each dtSKKOTEICHIMasrow As DataRow In O_dtSKKOTEICHIMas.Select(condition)
                dtSKKOTEICHIMasrow("TANKA") = dtTANKAMasrow("TANKA")
                'dtSKKOTEICHIMasrow("BRANCHCODE") = dtTANKAMasrow("TODOKEBRANCHCODE")
                dtSKKOTEICHIMasrow("TANKAKBN") = dtTANKAMasrow("TANKAKBN")
                dtSKKOTEICHIMasrow("MEMO") = dtTANKAMasrow("MEMO")

                ''★単価調整がある場合
                'Dim iSET_CELL As Integer = 0
                'Select Case dtSKKOTEICHIMasrow("BRANCHCODE").ToString()
                '    Case "02", "04"
                '        '■不積料金
                '        iSET_CELL = CInt(dtSKKOTEICHIMasrow("SET_CELL").ToString())
                '        iSET_CELL += 18
                '        dtSKKOTEICHIMasrow("SET_CELL") = iSET_CELL.ToString()
                'End Select
            Next
        Next
        '-- ★秋田(宿泊無)
        'conditionSub = String.Format("GRPNO='2' AND (ORGCODE='{0}' OR ORGCODE='{1}') ", BaseDllConst.CONST_ORDERORGCODE_021502, BaseDllConst.CONST_ORDERORGCODE_020601)
        'conditionSub &= String.Format("AND MEMO='{0}' ", arrAkitaSyaban(1))
        conditionSub = String.Format("GRPNO='2' AND AVOCADOSHUKABASHO='{0}' ", "005690")
        conditionSub &= String.Format("AND MEMO='{0}' ", "")
        For Each dtTANKAMasrow As DataRow In I_dtTANKAMas.Select(conditionSub)
            Dim condition As String = "GRPNO='3' "
            condition &= String.Format("AND TODOKENO ='{0}' ", dtTANKAMasrow("TODOKECODE").ToString())
            condition &= String.Format("AND KOTEICHI_GYOMUNO ='{0}' ", dtTANKAMasrow("SYAGOU").ToString())
            condition &= "AND KOTEICHI_GYOMUNOSUB ='02' "
            condition &= String.Format("AND BRANCHCODE ='{0}' ", dtTANKAMasrow("TODOKEBRANCHCODE").ToString())
            For Each dtSKKOTEICHIMasrow As DataRow In O_dtSKKOTEICHIMas.Select(condition)
                dtSKKOTEICHIMasrow("TANKA") = dtTANKAMasrow("TANKA")
                'dtSKKOTEICHIMasrow("BRANCHCODE") = dtTANKAMasrow("TODOKEBRANCHCODE")
                dtSKKOTEICHIMasrow("TANKAKBN") = dtTANKAMasrow("TANKAKBN")
                dtSKKOTEICHIMasrow("MEMO") = dtTANKAMasrow("MEMO")

                ''★単価調整がある場合
                'Dim iSET_CELL As Integer = 0
                'Select Case dtSKKOTEICHIMasrow("BRANCHCODE").ToString()
                '    Case "02", "04"
                '        '■不積料金
                '        iSET_CELL = CInt(dtSKKOTEICHIMasrow("SET_CELL").ToString())
                '        iSET_CELL += 18
                '        dtSKKOTEICHIMasrow("SET_CELL") = iSET_CELL.ToString()
                'End Select
            Next
        Next

        '-- 〇新潟
        'conditionSub = String.Format("GRPNO ='1' AND ORGCODE='{0}' AND AVOCADOSHUKABASHO<>'{1}' ", BaseDllConst.CONST_ORDERORGCODE_021502, "005690")
        conditionSub = String.Format(" GRPNO ='1' AND (ORGCODE='{0}' OR ORGCODE='{1}') ", BaseDllConst.CONST_ORDERORGCODE_021502, BaseDllConst.CONST_ORDERORGCODE_020601)
        conditionSub &= String.Format(" AND AVOCADOSHUKABASHO <> '{0}' ", "005690")
        For Each dtTANKAMasrow As DataRow In I_dtTANKAMas.Select(conditionSub)
            Dim condition As String = "GRPNO IN ('1','2') "
            condition &= String.Format("AND TODOKENO ='{0}' ", dtTANKAMasrow("TODOKECODE").ToString())
            condition &= String.Format("AND KOTEICHI_GYOMUNO ='{0}' ", dtTANKAMasrow("SYAGOU").ToString())
            condition &= String.Format("AND BRANCHCODE ='{0}' ", dtTANKAMasrow("TODOKEBRANCHCODE").ToString())
            For Each dtSKKOTEICHIMasrow As DataRow In O_dtSKKOTEICHIMas.Select(condition)
                dtSKKOTEICHIMasrow("TANKA") = dtTANKAMasrow("TANKA")
                dtSKKOTEICHIMasrow("TANKAKBN") = dtTANKAMasrow("TANKAKBN")
                dtSKKOTEICHIMasrow("MEMO") = dtTANKAMasrow("MEMO")

            Next
        Next

    End Sub

    ''' <summary>
    ''' 八戸特別料金マスタTBL検索
    ''' </summary>
    Public Sub SelectHACHINOHESPRATEMaster(ByVal SQLcon As MySqlConnection,
                                           ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByRef O_dtHACHINOHESPRATEMas As DataTable,
                                           Optional ByVal I_KASANORGCODE As String = Nothing)
        If IsNothing(O_dtHACHINOHESPRATEMas) Then
            O_dtHACHINOHESPRATEMas = New DataTable
        End If
        If O_dtHACHINOHESPRATEMas.Columns.Count <> 0 Then
            O_dtHACHINOHESPRATEMas.Columns.Clear()
        End If
        O_dtHACHINOHESPRATEMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0010.RECOID "
        SQLStr &= "   ,LNM0010.RECONAME "
        SQLStr &= "   ,LNM0010.TORICODE "
        SQLStr &= "   ,LNM0010.TORINAME "
        SQLStr &= "   ,LNM0010.ORGCODE "
        SQLStr &= "   ,LNM0010.ORGNAME "
        SQLStr &= "   ,LNM0010.KASANORGCODE "
        SQLStr &= "   ,LNM0010.KASANORGNAME "
        SQLStr &= "   ,LNM0010.STYMD "
        SQLStr &= "   ,LNM0010.ENDYMD "
        SQLStr &= "   ,LNM0010.KINGAKU "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0010_HACHINOHESPRATE LNM0010 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0010.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0010.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= String.Format(" AND LNM0010.ORGCODE = '{0}' ", I_ORGCODE)
        SQLStr &= String.Format(" AND LNM0010.STYMD <= '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0010.ENDYMD >= '{0}' ", I_TAISHOYM)
        If Not IsNothing(I_KASANORGCODE) Then
            SQLStr &= String.Format(" AND LNM0010.KASANORGCODE = '{0}' ", I_KASANORGCODE)
        End If

        '-- ORDER BY
        SQLStr &= " ORDER BY CAST(LNM0010.RECOID AS SIGNED) "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtHACHINOHESPRATEMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtHACHINOHESPRATEMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' ENEOS業務委託料マスタTBL検索
    ''' </summary>
    Public Sub SelectENEOSCOMFEEMaster(ByVal SQLcon As MySqlConnection,
                                       ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByRef O_dtENEOSCOMFEEMas As DataTable,
                                       Optional ByVal I_KASANORGCODE As String = Nothing)
        If IsNothing(O_dtENEOSCOMFEEMas) Then
            O_dtENEOSCOMFEEMas = New DataTable
        End If
        If O_dtENEOSCOMFEEMas.Columns.Count <> 0 Then
            O_dtENEOSCOMFEEMas.Columns.Clear()
        End If
        O_dtENEOSCOMFEEMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0011.RECOID "
        SQLStr &= "   ,LNM0011.RECONAME "
        SQLStr &= "   ,LNM0011.TORICODE "
        SQLStr &= "   ,LNM0011.TORINAME "
        SQLStr &= "   ,LNM0011.ORGCODE "
        SQLStr &= "   ,LNM0011.ORGNAME "
        SQLStr &= "   ,LNM0011.KASANORGCODE "
        SQLStr &= "   ,LNM0011.KASANORGNAME "
        SQLStr &= "   ,LNM0011.STYMD "
        SQLStr &= "   ,LNM0011.ENDYMD "
        SQLStr &= "   ,LNM0011.KINGAKU "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0011_ENEOSCOMFEE LNM0011 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0011.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0011.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= String.Format(" AND LNM0011.ORGCODE = '{0}' ", I_ORGCODE)
        SQLStr &= String.Format(" AND LNM0011.STYMD <= '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0011.ENDYMD >= '{0}' ", I_TAISHOYM)
        If Not IsNothing(I_KASANORGCODE) Then
            SQLStr &= String.Format(" AND LNM0011.KASANORGCODE = '{0}' ", I_KASANORGCODE)
        End If

        '-- ORDER BY

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtENEOSCOMFEEMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtENEOSCOMFEEMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' (新)統合版特別料金マスタTBL検索
    ''' </summary>
    Public Sub SelectNewIntegrationSprateFEEMaster(ByVal SQLcon As MySqlConnection,
                                                ByVal I_TORICODE As String, ByVal I_TAISHOYM As String, ByRef O_dtSPRATEFEEMas As DataTable,
                                                Optional ByVal I_ORGCODE As String = Nothing,
                                                Optional ByVal I_CLASS As String = Nothing)
        If IsNothing(O_dtSPRATEFEEMas) Then
            O_dtSPRATEFEEMas = New DataTable
        End If
        If O_dtSPRATEFEEMas.Columns.Count <> 0 Then
            O_dtSPRATEFEEMas.Columns.Clear()
        End If
        O_dtSPRATEFEEMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "      LNM0014.TARGETYM "                 '-- 対象年月
        SQLStr &= "    , LNM0014.TORICODE "                 '-- 取引先コード
        SQLStr &= "    , LNM0014.TORINAME "                 '-- 取引先名称
        SQLStr &= "    , LNM0014.ORGCODE "                  '-- 部門コード
        SQLStr &= "    , LNM0014.ORGNAME "                  '-- 部門名称
        SQLStr &= "    , LNM0014.KASANORGCODE "             '-- 加算先部門コード
        SQLStr &= "    , LNM0014.KASANORGNAME "             '-- 加算先部門名称
        SQLStr &= "    , LNM0014.BIGCATECODE "              '-- 大分類コード
        SQLStr &= "    , LNM0014.BIGCATENAME "              '-- 大分類名
        SQLStr &= "    , LNM0014.MIDCATECODE "              '-- 中分類コード
        SQLStr &= "    , LNM0014.MIDCATENAME "              '-- 中分類名
        SQLStr &= "    , LNM0014.SMALLCATECODE "            '-- 小分類コード
        SQLStr &= "    , LNM0014.SMALLCATENAME "            '-- 小分類名
        SQLStr &= "    , '' AS TODOKECODE "                 '-- 届先コード
        SQLStr &= "    , MIDCATENAME AS TODOKENAME "        '-- 届先名称
        SQLStr &= "    , LNM0014.TANKA "                    '-- 単価
        SQLStr &= "    , IFNULL(LNM0014.QUANTITY, 0) AS QUANTITY "                          '-- 数量
        SQLStr &= "    , LNM0014.CALCUNIT "                                                 '-- 計算単位
        SQLStr &= "    , IFNULL(LNM0014.DEPARTURE, '') AS DEPARTURE "                       '-- 出荷地
        SQLStr &= "    , IFNULL(LNM0014.MILEAGE, 0) AS MILEAGE "                            '-- 走行距離
        SQLStr &= "    , IFNULL(LNM0014.SHIPPINGCOUNT, 0) AS SHIPPINGCOUNT "                '-- 輸送回数
        SQLStr &= "    , IFNULL(LNM0014.NENPI, 0) AS NENPI "                                '-- 燃費
        SQLStr &= "    , IFNULL(LNM0014.DIESELPRICECURRENT, 0) AS DIESELPRICECURRENT "      '-- 実勢軽油価格
        SQLStr &= "    , IFNULL(LNM0014.DIESELPRICESTANDARD, 0) AS DIESELPRICESTANDARD "    '-- 基準経由価格
        SQLStr &= "    , IFNULL(LNM0014.DIESELCONSUMPTION, 0) AS DIESELCONSUMPTION "        '-- 燃料使用量
        SQLStr &= "    , IFNULL(LNM0014.DISPLAYFLG, '') AS DISPLAYFLG "                     '-- 表示フラグ
        SQLStr &= "    , IFNULL(LNM0014.ASSESSMENTFLG, '') AS ASSESSMENTFLG "               '-- 鑑分けフラグ
        SQLStr &= "    , IFNULL(LNM0014.ATENACOMPANYNAME, '') AS ATENACOMPANYNAME "         '-- 宛名会社名
        SQLStr &= "    , IFNULL(LNM0014.ATENACOMPANYDEVNAME, '') AS ATENACOMPANYDEVNAME "   '-- 宛名会社部門名
        SQLStr &= "    , IFNULL(LNM0014.FROMORGNAME, '') AS FROMORGNAME "                   '-- 請求書発行部店名
        SQLStr &= "    , IFNULL(LNM0014.MEISAICATEGORYID, '') AS MEISAICATEGORYID "         '-- 明細区分
        SQLStr &= "    , IFNULL(LNM0014.BIKOU1, '') AS BIKOU1 "                             '-- 備考1
        SQLStr &= "    , IFNULL(LNM0014.BIKOU2, '') AS BIKOU2 "                             '-- 備考2
        SQLStr &= "    , IFNULL(LNM0014.BIKOU3, '') AS BIKOU3 "                             '-- 備考3
        SQLStr &= "    , '' AS KOTEIHI_DISPLAYFLG "
        SQLStr &= "    , '' AS KOTEIHI_CELLNUM "
        SQLStr &= "    , '' AS KOTEIHI_CLASSIFYCODE "
        SQLStr &= "    , '' AS KOTEIHI_CONVERT "

        '-- FROM(統合版特別料金マスタ)
        SQLStr &= " FROM ( "
        SQLStr &= " SELECT LNM0014.* "
        SQLStr &= "      , CONVERT(LNM0014.BIGCATECODE  ,SIGNED) AS GROUPID_INT "
        SQLStr &= "      , CONVERT(LNM0014.SMALLCATECODE ,SIGNED) AS DETAILID_INT "
        SQLStr &= "      , CONVERT(LNM0014.MIDCATECODE ,SIGNED) AS TODOKECODE_INT "
        SQLStr &= " FROM LNG.LNM0014_SPRATE2 LNM0014 "
        SQLStr &= " ) LNM0014 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0014.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0014.TARGETYM = '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0014.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= " AND LNM0014.DISPLAYFLG = '1' "                              '-- 表示フラグ("1"(表示する))
        '★部門コード
        If Not IsNothing(I_ORGCODE) Then
            SQLStr &= String.Format(" AND LNM0014.ORGCODE IN ({0}) ", I_ORGCODE)
        End If

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtSPRATEFEEMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtSPRATEFEEMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            'Throw '呼び出し元の例外にスロー
        End Try

        '-- ★変換マスタより取得
        Dim convertMAS As New DataTable
        If IsNothing(convertMAS) Then
            convertMAS = New DataTable
        End If
        If convertMAS.Columns.Count <> 0 Then
            convertMAS.Columns.Clear()
        End If
        convertMAS.Clear()

        Dim SQLStrSub As String = ""
        If Not IsNothing(I_CLASS) _
            AndAlso I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 _
            AndAlso I_ORGCODE = BaseDllConst.CONST_ORDERORGCODE_020104 Then

            '〇石油資源開発(北海道)
            '-- SELECT
            SQLStrSub &= " SELECT LNM0005.* "
            SQLStrSub &= " , ROW_NUMBER() OVER(PARTITION BY LNM0005.KEYCODE01 "
            SQLStrSub &= "   ORDER BY LNM0005.KEYCODE01,LNM0005.KEYCODE04,LNM0005.KEYCODE07) RNUM "
            '-- FROM
            SQLStrSub &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- WHERE
            SQLStrSub &= " WHERE "
            SQLStrSub &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStrSub &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            'SQLStrSub &= " AND LNM0005.KEYCODE08 NOT LIKE '日祝%' "   '※(日祝%)以外が設定されている

            Try
                Using SQLcmd As New MySqlCommand(SQLStrSub, SQLcon)
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            convertMAS.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        convertMAS.Load(SQLdr)
                    End Using
                End Using

                For Each dtSPRATEFEEMasrow As DataRow In O_dtSPRATEFEEMas.Rows
                    '★届先コード(取得)※
                    Select Case dtSPRATEFEEMasrow("TODOKENAME").ToString()
                        Case "(SK)釧路ガス"
                            dtSPRATEFEEMasrow("TODOKECODE") = BaseDllConst.CONST_TODOKECODE_003561
                        Case "(SK)室蘭ガス"
                            dtSPRATEFEEMasrow("TODOKECODE") = BaseDllConst.CONST_TODOKECODE_003563
                        Case "ＳＫ勇払（工場）"
                            dtSPRATEFEEMasrow("TODOKECODE") = BaseDllConst.CONST_TODOKECODE_005834
                        Case "室蘭港バンカリング"
                            dtSPRATEFEEMasrow("TODOKECODE") = BaseDllConst.CONST_TODOKECODE_006915

                        '※（注意）届先が追加された場合CASE分追加
                        Case "テスト１"
                            dtSPRATEFEEMasrow("TODOKECODE") = "009999"
                        Case "テスト２"
                            dtSPRATEFEEMasrow("TODOKECODE") = "008888"

                    End Select

                    Dim condition As String = ""
                    '〇条件
                    '・大分類コード
                    condition &= String.Format(" KEYCODE01='{0}' ", dtSPRATEFEEMasrow("BIGCATECODE"))
                    '・中分類コード
                    condition &= String.Format(" AND KEYCODE04='{0}' ", dtSPRATEFEEMasrow("MIDCATECODE"))
                    '・届先コード
                    condition &= String.Format(" AND KEYCODE05='{0}' ", dtSPRATEFEEMasrow("TODOKECODE"))
                    '・小分類コード
                    condition &= String.Format(" AND KEYCODE09='{0}' ", dtSPRATEFEEMasrow("SMALLCATECODE"))

                    For Each convertMASrow As DataRow In convertMAS.Select(condition)
                        '■シート[内訳]
                        '・表示セルフラグ(1:表示)
                        dtSPRATEFEEMasrow("KOTEIHI_DISPLAYFLG") = convertMASrow("VALUE01")
                        '・行(設定)セル
                        dtSPRATEFEEMasrow("KOTEIHI_CELLNUM") = convertMASrow("VALUE02")
                        '・分類
                        dtSPRATEFEEMasrow("KOTEIHI_CONVERT") = convertMASrow("CLASS")
                    Next

                Next

            Catch ex As Exception
                'Throw '呼び出し元の例外にスロー
            End Try

        ElseIf Not IsNothing(I_CLASS) _
            AndAlso I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 _
            AndAlso I_ORGCODE <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
            '〇石油資源開発(本州)

            '-- SELECT
            SQLStrSub &= " SELECT LNM0005.* "
            '-- FROM
            SQLStrSub &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- WHERE
            SQLStrSub &= " WHERE "
            SQLStrSub &= String.Format("     LNM0005.DELFLG = '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStrSub &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStrSub &= " AND LNM0005.KEYCODE10 <> '' "    '※(大分類コード + 小分類コード)が設定されている

            Try
                Using SQLcmd As New MySqlCommand(SQLStrSub, SQLcon)
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            convertMAS.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        convertMAS.Load(SQLdr)
                    End Using
                End Using

                For Each dtSPRATEFEEMasrow As DataRow In O_dtSPRATEFEEMas.Rows
                    '(結合)大分類コード + 小分類コード
                    Dim joinItem As String = dtSPRATEFEEMasrow("BIGCATECODE").ToString() + dtSPRATEFEEMasrow("SMALLCATECODE").ToString()
                    Dim condition As String = ""
                    '〇条件
                    '・大分類コード + 小分類コード
                    condition &= String.Format(" KEYCODE10='{0}' ", joinItem)
                    '・部署コード
                    condition &= String.Format(" AND KEYCODE04='{0}' ", dtSPRATEFEEMasrow("ORGCODE"))

                    For Each convertMASrow As DataRow In convertMAS.Select(condition)
                        '■シート[従量運賃]
                        '・表示セルフラグ(1:表示)
                        dtSPRATEFEEMasrow("KOTEIHI_DISPLAYFLG") = convertMASrow("VALUE02")
                        '・行(設定)セル
                        dtSPRATEFEEMasrow("KOTEIHI_CELLNUM") = convertMASrow("VALUE03")
                    Next

                Next

            Catch ex As Exception
                'Throw '呼び出し元の例外にスロー
            End Try

        ElseIf Not IsNothing(I_CLASS) _
            AndAlso I_TORICODE = BaseDllConst.CONST_TORICODE_0239900000 _
            AndAlso I_ORGCODE = BaseDllConst.CONST_ORDERORGCODE_020104 Then
            '〇北海道LNG

            '★その他明細の小分類コードの付替え実施
            Dim i As Decimal = 0
            For Each dtSPRATEFEEMasrow As DataRow In O_dtSPRATEFEEMas.Select("BIGCATECODE='5'", "SMALLCATECODE")
                i += 1
                dtSPRATEFEEMasrow("SMALLCATECODE") = i
            Next

            '-- SELECT
            SQLStrSub &= " SELECT LNM0005.* "
            '-- FROM
            SQLStrSub &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- WHERE
            SQLStrSub &= " WHERE "
            SQLStrSub &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStrSub &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStrSub &= " AND LNM0005.KEYCODE01 IN ('5','6','7','8','9','10') "    '※委託料、その他

            Try
                Using SQLcmd As New MySqlCommand(SQLStrSub, SQLcon)
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            convertMAS.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        convertMAS.Load(SQLdr)
                    End Using
                End Using

                For Each dtSPRATEFEEMasrow As DataRow In O_dtSPRATEFEEMas.Rows
                    Dim condition As String = ""
                    '〇条件
                    '・大分類コード
                    condition &= String.Format(" KEYCODE04='{0}' ", dtSPRATEFEEMasrow("BIGCATECODE"))
                    '・小分類コード
                    condition &= String.Format(" AND KEYCODE07='{0}' ", dtSPRATEFEEMasrow("SMALLCATECODE"))

                    For Each convertMASrow As DataRow In convertMAS.Select(condition)
                        '■シート[従量運賃]
                        '・表示セルフラグ(1:表示)
                        dtSPRATEFEEMasrow("KOTEIHI_DISPLAYFLG") = convertMASrow("VALUE01")
                        '・行(設定)セル
                        dtSPRATEFEEMasrow("KOTEIHI_CELLNUM") = convertMASrow("VALUE02")
                    Next

                Next

            Catch ex As Exception
                'Throw '呼び出し元の例外にスロー
            End Try

        End If

    End Sub

    ''' <summary>
    ''' 統合版特別料金マスタTBL検索
    ''' </summary>
    Public Sub SelectIntegrationSprateFEEMaster(ByVal SQLcon As MySqlConnection,
                                                ByVal I_TORICODE As String, ByVal I_TAISHOYM As String, ByRef O_dtSPRATEFEEMas As DataTable,
                                                Optional ByVal I_ORGCODE As String = Nothing,
                                                Optional ByVal I_CLASS As String = Nothing)
        If IsNothing(O_dtSPRATEFEEMas) Then
            O_dtSPRATEFEEMas = New DataTable
        End If
        If O_dtSPRATEFEEMas.Columns.Count <> 0 Then
            O_dtSPRATEFEEMas.Columns.Clear()
        End If
        O_dtSPRATEFEEMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "      LNM0014.TARGETYM "                 '-- 対象年月
        SQLStr &= "    , LNM0014.TORICODE "                 '-- 取引先コード
        SQLStr &= "    , LNM0014.TORINAME "                 '-- 取引先名称
        SQLStr &= "    , LNM0014.ORGCODE "                  '-- 部門コード
        SQLStr &= "    , LNM0014.ORGNAME "                  '-- 部門名称
        SQLStr &= "    , LNM0014.KASANORGCODE "             '-- 加算先部門コード
        SQLStr &= "    , LNM0014.KASANORGNAME "             '-- 加算先部門名称
        SQLStr &= "    , IFNULL(LNM0014.TODOKECODE, '') AS TODOKECODE "                     '-- 届先コード
        SQLStr &= "    , IFNULL(LNM0014.TODOKENAME, '') AS TODOKENAME "                     '-- 届先名称
        SQLStr &= "    , LNM0014.GROUPSORTNO "              '-- グループソート順
        SQLStr &= "    , LNM0014.GROUPID "                  '-- グループID
        SQLStr &= "    , LNM0014.GROUPNAME "                '-- グループ名
        SQLStr &= "    , LNM0014.DETAILSORTNO "             '-- 明細ソート順
        SQLStr &= "    , LNM0014.DETAILID "                 '-- 明細ID
        SQLStr &= "    , LNM0014.DETAILNAME "               '-- 明細名
        SQLStr &= "    , LNM0014.TANKA "                    '-- 単価
        SQLStr &= "    , IFNULL(LNM0014.QUANTITY, 0) AS QUANTITY "                          '-- 数量
        SQLStr &= "    , LNM0014.CALCUNIT "                                                 '-- 計算単位
        SQLStr &= "    , IFNULL(LNM0014.DEPARTURE, '') AS DEPARTURE "                       '-- 出荷地
        SQLStr &= "    , IFNULL(LNM0014.MILEAGE, 0) AS MILEAGE "                            '-- 走行距離
        SQLStr &= "    , IFNULL(LNM0014.SHIPPINGCOUNT, 0) AS SHIPPINGCOUNT "                '-- 輸送回数
        SQLStr &= "    , IFNULL(LNM0014.NENPI, 0) AS NENPI "                                '-- 燃費
        SQLStr &= "    , IFNULL(LNM0014.DIESELPRICECURRENT, 0) AS DIESELPRICECURRENT "      '-- 実勢軽油価格
        SQLStr &= "    , IFNULL(LNM0014.DIESELPRICESTANDARD, 0) AS DIESELPRICESTANDARD "    '-- 基準経由価格
        SQLStr &= "    , IFNULL(LNM0014.DIESELCONSUMPTION, 0) AS DIESELCONSUMPTION "        '-- 燃料使用量
        SQLStr &= "    , IFNULL(LNM0014.DISPLAYFLG, '') AS DISPLAYFLG "                     '-- 表示フラグ
        SQLStr &= "    , IFNULL(LNM0014.ASSESSMENTFLG, '') AS ASSESSMENTFLG "               '-- 鑑分けフラグ
        SQLStr &= "    , IFNULL(LNM0014.ATENACOMPANYNAME, '') AS ATENACOMPANYNAME "         '-- 宛名会社名
        SQLStr &= "    , IFNULL(LNM0014.ATENACOMPANYDEVNAME, '') AS ATENACOMPANYDEVNAME "   '-- 宛名会社部門名
        SQLStr &= "    , IFNULL(LNM0014.FROMORGNAME, '') AS FROMORGNAME "                   '-- 請求書発行部店名
        SQLStr &= "    , IFNULL(LNM0014.MEISAICATEGORYID, '') AS MEISAICATEGORYID "         '-- 明細区分
        SQLStr &= "    , IFNULL(LNM0014.BIKOU1, '') AS BIKOU1 "                             '-- 備考1
        SQLStr &= "    , IFNULL(LNM0014.BIKOU2, '') AS BIKOU2 "                             '-- 備考2
        SQLStr &= "    , IFNULL(LNM0014.BIKOU3, '') AS BIKOU3 "                             '-- 備考3
        SQLStr &= "    , '' AS KOTEIHI_DISPLAYFLG "
        SQLStr &= "    , '' AS KOTEIHI_CELLNUM "
        SQLStr &= "    , '' AS KOTEIHI_CLASSIFYCODE "

        '-- FROM(統合版特別料金マスタ)
        SQLStr &= " FROM ( "
        SQLStr &= " SELECT LNM0014.* "
        SQLStr &= "      , CONVERT(LNM0014.GROUPID  ,SIGNED) AS GROUPID_INT "
        SQLStr &= "      , CONVERT(LNM0014.DETAILID ,SIGNED) AS DETAILID_INT "
        SQLStr &= "      , IFNULL(LNM0014.TODOKECODE,'')     AS TODOKECODE_CONVERT "
        SQLStr &= " FROM LNG.LNM0014_SPRATE LNM0014 "
        SQLStr &= " ) LNM0014 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0014.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0014.TARGETYM = '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0014.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= " AND LNM0014.DISPLAYFLG = '1' "                              '-- 表示フラグ("1"(表示する))
        '★部門コード
        If Not IsNothing(I_ORGCODE) Then
            SQLStr &= String.Format(" AND LNM0014.ORGCODE IN ({0}) ", I_ORGCODE)
        End If

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtSPRATEFEEMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtSPRATEFEEMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            'Throw '呼び出し元の例外にスロー
        End Try

        '-- ★変換マスタより取得
        Dim convertMAS As New DataTable
        If IsNothing(convertMAS) Then
            convertMAS = New DataTable
        End If
        If convertMAS.Columns.Count <> 0 Then
            convertMAS.Columns.Clear()
        End If
        convertMAS.Clear()

        Dim SQLStrSub As String = ""
        If Not IsNothing(I_CLASS) _
            AndAlso I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 _
            AndAlso I_ORGCODE = BaseDllConst.CONST_ORDERORGCODE_020104 Then

            '〇石油資源開発(北海道)
            '-- SELECT
            SQLStrSub &= " SELECT LNM0005.* "
            SQLStrSub &= " , ROW_NUMBER() OVER(PARTITION BY LNM0005.KEYCODE01 "
            SQLStrSub &= "   ORDER BY LNM0005.KEYCODE01,LNM0005.KEYCODE04,LNM0005.KEYCODE07) RNUM "
            '-- FROM
            SQLStrSub &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- WHERE
            SQLStrSub &= " WHERE "
            SQLStrSub &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStrSub &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            'SQLStrSub &= " AND LNM0005.KEYCODE08 NOT LIKE '日祝%' "   '※(日祝%)以外が設定されている

            Try
                Using SQLcmd As New MySqlCommand(SQLStrSub, SQLcon)
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            convertMAS.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        convertMAS.Load(SQLdr)
                    End Using
                End Using

                For Each dtSPRATEFEEMasrow As DataRow In O_dtSPRATEFEEMas.Rows
                    Dim condition As String = ""
                    '〇条件
                    '・GRPID
                    condition &= String.Format(" KEYCODE01='{0}' ", dtSPRATEFEEMasrow("GROUPID"))
                    '・届先コード
                    condition &= String.Format(" AND KEYCODE05='{0}' ", dtSPRATEFEEMasrow("TODOKECODE"))
                    '・明細ID
                    condition &= String.Format(" AND KEYCODE09='{0}' ", dtSPRATEFEEMasrow("DETAILID"))

                    For Each convertMASrow As DataRow In convertMAS.Select(condition)
                        '■シート[内訳]
                        '・表示セルフラグ(1:表示)
                        dtSPRATEFEEMasrow("KOTEIHI_DISPLAYFLG") = convertMASrow("VALUE01")
                        '・行(設定)セル
                        dtSPRATEFEEMasrow("KOTEIHI_CELLNUM") = convertMASrow("VALUE02")
                    Next

                Next

            Catch ex As Exception
                'Throw '呼び出し元の例外にスロー
            End Try

        ElseIf Not IsNothing(I_CLASS) _
            AndAlso I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 _
            AndAlso I_ORGCODE <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
            '〇石油資源開発(本州)

            '-- SELECT
            SQLStrSub &= " SELECT LNM0005.* "
            '-- FROM
            SQLStrSub &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- WHERE
            SQLStrSub &= " WHERE "
            SQLStrSub &= String.Format("     LNM0005.DELFLG = '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStrSub &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStrSub &= " AND LNM0005.KEYCODE10 <> '' "    '※(GRPID + 明細ID)が設定されている

            Try
                Using SQLcmd As New MySqlCommand(SQLStrSub, SQLcon)
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            convertMAS.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        convertMAS.Load(SQLdr)
                    End Using
                End Using

                For Each dtSPRATEFEEMasrow As DataRow In O_dtSPRATEFEEMas.Rows
                    '(結合)GRPID + 明細ID
                    Dim joinItem As String = dtSPRATEFEEMasrow("GROUPID").ToString() + dtSPRATEFEEMasrow("DETAILID").ToString()
                    Dim condition As String = ""
                    '〇条件
                    '・GRPID + 明細ID
                    condition &= String.Format(" KEYCODE10='{0}' ", joinItem)
                    '・部署コード
                    condition &= String.Format(" AND KEYCODE04='{0}' ", dtSPRATEFEEMasrow("ORGCODE"))

                    For Each convertMASrow As DataRow In convertMAS.Select(condition)
                        '■シート[従量運賃]
                        '・表示セルフラグ(1:表示)
                        dtSPRATEFEEMasrow("KOTEIHI_DISPLAYFLG") = convertMASrow("VALUE02")
                        '・行(設定)セル
                        dtSPRATEFEEMasrow("KOTEIHI_CELLNUM") = convertMASrow("VALUE03")
                    Next

                Next

            Catch ex As Exception
                'Throw '呼び出し元の例外にスロー
            End Try

        ElseIf Not IsNothing(I_CLASS) _
            AndAlso I_TORICODE = BaseDllConst.CONST_TORICODE_0239900000 _
            AndAlso I_ORGCODE = BaseDllConst.CONST_ORDERORGCODE_020104 Then
            '〇北海道LNG

            '★その他明細の明細IDの付替え実施
            Dim i As Decimal = 0
            For Each dtSPRATEFEEMasrow As DataRow In O_dtSPRATEFEEMas.Select("GROUPID='5'", "DETAILID")
                i += 1
                dtSPRATEFEEMasrow("DETAILID") = i
            Next

            '-- SELECT
            SQLStrSub &= " SELECT LNM0005.* "
            '-- FROM
            SQLStrSub &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- WHERE
            SQLStrSub &= " WHERE "
            SQLStrSub &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStrSub &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStrSub &= " AND LNM0005.KEYCODE01 IN ('5','10') "    '※委託料、その他

            Try
                Using SQLcmd As New MySqlCommand(SQLStrSub, SQLcon)
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            convertMAS.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        convertMAS.Load(SQLdr)
                    End Using
                End Using

                For Each dtSPRATEFEEMasrow As DataRow In O_dtSPRATEFEEMas.Rows
                    Dim condition As String = ""
                    '〇条件
                    '・GRPID
                    condition &= String.Format(" KEYCODE04='{0}' ", dtSPRATEFEEMasrow("GROUPID"))
                    '・明細ID
                    condition &= String.Format(" AND KEYCODE07='{0}' ", dtSPRATEFEEMasrow("DETAILID"))

                    For Each convertMASrow As DataRow In convertMAS.Select(condition)
                        '■シート[従量運賃]
                        '・表示セルフラグ(1:表示)
                        dtSPRATEFEEMasrow("KOTEIHI_DISPLAYFLG") = convertMASrow("VALUE01")
                        '・行(設定)セル
                        dtSPRATEFEEMasrow("KOTEIHI_CELLNUM") = convertMASrow("VALUE02")
                    Next

                Next

            Catch ex As Exception
                'Throw '呼び出し元の例外にスロー
            End Try

        End If

    End Sub

    ''' <summary>
    ''' SK特別料金マスタTBL検索
    ''' </summary>
    Public Sub SelectSKSpecialFEEMaster(ByVal SQLcon As MySqlConnection,
                                        ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByRef O_dtSKSPECIALFEEMas As DataTable,
                                        Optional ByVal I_KASANORGCODE As String = Nothing,
                                        Optional ByVal I_CLASS As String = Nothing)
        If IsNothing(O_dtSKSPECIALFEEMas) Then
            O_dtSKSPECIALFEEMas = New DataTable
        End If
        If O_dtSKSPECIALFEEMas.Columns.Count <> 0 Then
            O_dtSKSPECIALFEEMas.Columns.Clear()
        End If
        O_dtSKSPECIALFEEMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "      LNM0014.BIGCATEGORYCODE "
        SQLStr &= "    , LNM0014.BIGCATEGORYNAME "
        SQLStr &= "    , LNM0014.CATEGORYCODE "
        SQLStr &= "    , LNM0014.CATEGORYNAME "
        SQLStr &= "    , LNM0014.DETAILCODE "
        SQLStr &= "    , LNM0014.DETAILNAME "
        SQLStr &= "    , LNM0014.SORT "
        SQLStr &= "    , LNM0014.TORICODE "
        SQLStr &= "    , LNM0014.TORINAME "
        SQLStr &= "    , LNM0014.ORGCODE "
        SQLStr &= "    , LNM0014.ORGNAME "
        SQLStr &= "    , LNM0014.KASANORGCODE "
        SQLStr &= "    , LNM0014.KASANORGNAME "
        SQLStr &= "    , LNM0014.TODOKECODE "
        SQLStr &= "    , LNM0014.TODOKENAME "
        SQLStr &= "    , LNM0014.STYMD "
        SQLStr &= "    , LNM0014.ENDYMD "
        SQLStr &= "    , LNM0014.TANKA "
        SQLStr &= "    , LNM0014.KUBUN "
        SQLStr &= "    , LNM0014.KUBUNNAME "
        SQLStr &= "    , LNM0014.QUANTITY "
        SQLStr &= "    , LNM0014.BIKOU1 "
        SQLStr &= "    , LNM0014.BIKOU2 "
        SQLStr &= "    , LNM0014.BIKOU3 "
        If Not IsNothing(I_CLASS) Then
            SQLStr &= "   ,LNM0005.VALUE01 AS KOTEIHI_DISPLAYFLG "
            SQLStr &= "   ,LNM0005.VALUE02 AS KOTEIHI_CELLNUM "
        End If

        '-- FROM
        SQLStr &= " FROM LNG.LNM0014_SKSPRATE LNM0014 "
        If Not IsNothing(I_CLASS) Then
            SQLStr &= " LEFT JOIN LNG.LNM0005_CONVERT LNM0005 ON "
            SQLStr &= String.Format("     LNM0005.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.KEYCODE01 = LNM0014.BIGCATEGORYCODE "
            SQLStr &= " AND LNM0005.KEYCODE04 = LNM0014.CATEGORYCODE "
            SQLStr &= " AND LNM0005.KEYCODE07 = LNM0014.SORT "
        End If

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0014.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0014.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= String.Format(" AND LNM0014.ORGCODE = '{0}' ", I_ORGCODE)
        SQLStr &= String.Format(" AND LNM0014.STYMD <= '{0}' ", I_TAISHOYM)
        SQLStr &= String.Format(" AND LNM0014.ENDYMD >= '{0}' ", I_TAISHOYM)
        If Not IsNothing(I_KASANORGCODE) Then
            SQLStr &= String.Format(" AND LNM0014.KASANORGCODE = '{0}' ", I_KASANORGCODE)
        End If

        '-- ORDER BY

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtSKSPECIALFEEMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtSKSPECIALFEEMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' SK燃料サーチャージマスタTBL検索
    ''' </summary>
    Public Sub SelectSKFuelSurchargeMaster(ByVal SQLcon As MySqlConnection,
                                           ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String, ByRef O_dtSKFUELSURCHARGEMas As DataTable,
                                           Optional ByVal I_KASANORGCODE As String = Nothing)
        If IsNothing(O_dtSKFUELSURCHARGEMas) Then
            O_dtSKFUELSURCHARGEMas = New DataTable
        End If
        If O_dtSKFUELSURCHARGEMas.Columns.Count <> 0 Then
            O_dtSKFUELSURCHARGEMas.Columns.Clear()
        End If
        O_dtSKFUELSURCHARGEMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "      LNM0015.TORICODE "
        SQLStr &= "    , LNM0015.TORINAME "
        SQLStr &= "    , LNM0015.ORGCODE "
        SQLStr &= "    , LNM0015.ORGNAME "
        SQLStr &= "    , LNM0015.KASANORGCODE "
        SQLStr &= "    , LNM0015.KASANORGNAME "
        SQLStr &= "    , LNM0015.TODOKECODE "
        SQLStr &= "    , LNM0015.TODOKENAME "
        SQLStr &= "    , LNM0015.TAISHOYM "
        SQLStr &= "    , LNM0015.KYORI "
        SQLStr &= "    , LNM0015.KEIYU "
        SQLStr &= "    , LNM0015.KIZYUN "
        SQLStr &= "    , LNM0015.TANKASA "
        SQLStr &= "    , LNM0015.KAISU "
        SQLStr &= "    , LNM0015.USAGECHARGE "
        SQLStr &= "    , LNM0015.SURCHARGE "
        SQLStr &= "    , LNM0015.BIKOU1 "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0015_SKSURCHARGE LNM0015 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0015.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0015.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= String.Format(" AND LNM0015.ORGCODE = '{0}' ", I_ORGCODE)
        SQLStr &= String.Format(" AND LNM0015.TAISHOYM <= '{0}' ", I_TAISHOYM)
        'SQLStr &= String.Format(" AND LNM0015.STYMD <= '{0}' ", I_TAISHOYM)
        'SQLStr &= String.Format(" AND LNM0015.ENDYMD >= '{0}' ", I_TAISHOYM)
        If Not IsNothing(I_KASANORGCODE) Then
            SQLStr &= String.Format(" AND LNM0015.KASANORGCODE = '{0}' ", I_KASANORGCODE)
        End If

        '-- ORDER BY

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtSKFUELSURCHARGEMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtSKFUELSURCHARGEMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' カレンダーマスタTBL検索
    ''' </summary>
    Public Sub SelectCALENDARMaster(ByVal SQLcon As MySqlConnection,
                                    ByVal I_TORICODE As String, ByVal I_TAISHOYM As String, ByRef O_dtCALENDARMas As DataTable,
                                    Optional ByVal I_ORGCODE As String = Nothing)
        If IsNothing(O_dtCALENDARMas) Then
            O_dtCALENDARMas = New DataTable
        End If
        If O_dtCALENDARMas.Columns.Count <> 0 Then
            O_dtCALENDARMas.Columns.Clear()
        End If
        O_dtCALENDARMas.Clear()

        '★月末日取得
        Dim lastDay As String = ""
        lastDay = Date.Parse(I_TAISHOYM).AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
        '★取引コード(北海道LNG)の場合
        If I_TORICODE = BaseDllConst.CONST_TORICODE_0239900000 Then
            Dim thisYear As String = Date.Parse(I_TAISHOYM).ToString("yyyy")
            I_TAISHOYM = thisYear + "/01/01"
            lastDay = thisYear + "/12/31"
        End If

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0016.TORICODE "
        SQLStr &= "   ,LNM0016.YMD "
        SQLStr &= "   ,LNM0016.WEEKDAY "
        SQLStr &= "   ,LNM0016.WORKINGDAY "
        SQLStr &= "   ,LNM0016.WORKINGDAYNAME "
        SQLStr &= "   ,LNM0016.PUBLICHOLIDAYNAME "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0016_CALENDAR LNM0016 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format("     LNM0016.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0016.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= String.Format(" AND LNM0016.YMD BETWEEN '{0}' AND '{1}' ", I_TAISHOYM, lastDay)
        'If Not IsNothing(I_ORGCODE) Then
        '    SQLStr &= String.Format(" AND LNM0016.ORGCODE = '{0}' ", I_ORGCODE)
        'End If

        '-- ORDER BY

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtCALENDARMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtCALENDARMas.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    ''' <summary>
    ''' 休日割増単価マスタTBL検索
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="O_dtHOLIDAYRATEMas">検索結果取得用</param>
    ''' <param name="I_dtTODOKEMas">設定(行)取得用TABLE</param>
    ''' <param name="I_ORDERORGCODE">受注受付部署コード</param>
    ''' <param name="I_SHUKABASHO">出荷場所コード</param>
    ''' <param name="I_TODOKECODE">届先コード</param>
    ''' <param name="I_CLASS">変換マスタ(分類コード)</param>
    Public Sub SelectHOLIDAYRATEMaster(ByVal SQLcon As MySqlConnection,
                                       ByVal I_TORICODE As String, ByRef O_dtHOLIDAYRATEMas As DataTable,
                                       Optional ByVal I_dtTODOKEMas As DataTable = Nothing,
                                       Optional ByVal I_ORDERORGCODE As String = Nothing,
                                       Optional ByVal I_SHUKABASHO As String = Nothing,
                                       Optional ByVal I_TODOKECODE As String = Nothing,
                                       Optional ByVal I_CLASS As String = Nothing)
        If IsNothing(O_dtHOLIDAYRATEMas) Then
            O_dtHOLIDAYRATEMas = New DataTable
        End If
        If O_dtHOLIDAYRATEMas.Columns.Count <> 0 Then
            O_dtHOLIDAYRATEMas.Columns.Clear()
        End If
        O_dtHOLIDAYRATEMas.Clear()

        Dim SQLStr As String = ""
        SQLStr = selectHOLIDAYRATESentence(I_TORICODE, I_ORDERORGCODE, I_CLASS)

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtHOLIDAYRATEMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtHOLIDAYRATEMas.Load(SQLdr)
                End Using
            End Using

            '★休日範囲(網羅チェック)
            For Each dtHOLIDAYRATEMasrow As DataRow In O_dtHOLIDAYRATEMas.Rows
                Dim iRangeLen As Integer = dtHOLIDAYRATEMasrow("RANGECODE").ToString().Length
                Dim j As Integer = 0
                For i As Integer = 1 To iRangeLen

                    Select Case dtHOLIDAYRATEMasrow("RANGECODE").ToString().Substring(j, 1)
                        '★日曜
                        Case "1"
                            dtHOLIDAYRATEMasrow("RANGE_SUNDAY") = "1"
                        '★祝日
                        Case "2"
                            dtHOLIDAYRATEMasrow("RANGE_HOLIDAY") = "1"
                        '★年末年始
                        Case "3", "4"
                            dtHOLIDAYRATEMasrow("RANGE_YEAREND_NEWYEAR") = "1"
                        '★メーデー(労働者の祭典)
                        Case "5"
                            dtHOLIDAYRATEMasrow("RANGE_MAYDAY") = "1"
                    End Select

                    j += 1
                Next
            Next

            '★取引マスタTBL検索
            SelectTORIMaster(SQLcon, I_dtTORISet:=O_dtHOLIDAYRATEMas)
            '★受注受付部署マスタTBL検索
            SelectORDERORGMaster(SQLcon, I_dtORDERORGSet:=O_dtHOLIDAYRATEMas)
            '★出荷場所マスタTBL検索
            SelectSHUKABASHOMaster(SQLcon, I_dtSHUKABASHOSet:=O_dtHOLIDAYRATEMas)
            '★届先マスタTBL検索
            SelectTODOKEMaster(SQLcon, I_dtTODOKESet:=O_dtHOLIDAYRATEMas)

            '★マスターシート(設定)項目追加 -----------------------------------------------
            I_dtTODOKEMas.Columns.Add("CHK_TODOKECODE", Type.GetType("System.String"))
            I_dtTODOKEMas.Columns.Add("CHK_MASCELL", Type.GetType("System.String"))
            Dim setItem As String() = {"KEYCODE01", "VALUE04"}
            If I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 _
                AndAlso I_ORDERORGCODE = BaseDllConst.CONST_ORDERORGCODE_020104 Then
                '■石油資源開発(北海道)の場合
                setItem(0) = "TODOKECODE"
                setItem(1) = "KOTEIHI_CELLNUM"
            ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
                '■シーエナジーの場合
                setItem(0) = "KEYCODE04"    '※届先がないので、一旦業務車番設定
                setItem(1) = "VALUE11"
            End If
            For Each dtTODOKEMasrow As DataRow In I_dtTODOKEMas.Rows
                dtTODOKEMasrow("CHK_TODOKECODE") = dtTODOKEMasrow(setItem(0))
                dtTODOKEMasrow("CHK_MASCELL") = dtTODOKEMasrow(setItem(1))
            Next
            ' ----------------------------------------------------------------------------/

            '★[ﾏｽﾀ]シート設定セル取得
            If Not IsNothing(I_dtTODOKEMas) Then
                '■石油資源開発(本州)
                If I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 _
                    AndAlso I_ORDERORGCODE <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
                    For Each dtTODOKEMasrow As DataRow In I_dtTODOKEMas.Rows
                        '★条件
                        Dim conditionSub As String = ""
                        '受注受付部署コード
                        Dim orderOrgcode As String = dtTODOKEMasrow("KEYCODE04").ToString()
                        conditionSub &= String.Format("ORDERORGCODE = '{0}' ", orderOrgcode)
                        ''出荷場所コード
                        'Dim shukaBashocode As String = dtTODOKEMasrow("KEYCODE06").ToString()
                        'conditionSub &= String.Format("AND SHUKABASHOCODE = '{0}' ", shukaBashocode)
                        '届先コード
                        Dim todokeCode As String = dtTODOKEMasrow("KEYCODE01").ToString()
                        conditionSub &= String.Format("AND TODOKECODE_LNM0005 = '{0}' ", todokeCode)

                        For Each dtHOLIDAYRATEMasrow As DataRow In O_dtHOLIDAYRATEMas.Select(conditionSub)
                            '〇届け先判定区分(1：対象　2：除外)
                            selectHOLIDAYRATETodokeJudge(dtTODOKEMasrow, dtHOLIDAYRATEMasrow)
                        Next
                    Next

                ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 _
                    AndAlso I_ORDERORGCODE = BaseDllConst.CONST_ORDERORGCODE_020104 Then
                    '■石油資源開発(北海道)
                    For Each dtTODOKEMasrow As DataRow In I_dtTODOKEMas.Rows
                        Dim detailName As String = dtTODOKEMasrow("SMALLCATENAME").ToString()
                        If detailName.Substring(0, 2) <> "日祝" Then
                            Continue For
                        End If
                        For Each dtHOLIDAYRATEMasrow As DataRow In O_dtHOLIDAYRATEMas.Rows
                            '〇届け先判定区分(1：対象　2：除外)
                            selectHOLIDAYRATETodokeJudge(dtTODOKEMasrow, dtHOLIDAYRATEMasrow)
                        Next
                    Next

                ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
                    '■シーエナジーの場合(※業務車番毎のため、判定なし)

                Else
                    '※上記(以外)
                    For Each dtTODOKEMasrow As DataRow In I_dtTODOKEMas.Rows
                        For Each dtHOLIDAYRATEMasrow As DataRow In O_dtHOLIDAYRATEMas.Rows
                            '〇届け先判定区分(1：対象　2：除外)
                            selectHOLIDAYRATETodokeJudge(dtTODOKEMasrow, dtHOLIDAYRATEMasrow)
                        Next
                    Next
                End If

            End If

        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    Private Function selectHOLIDAYRATESentence(ByVal I_TORICODE As String, ByVal I_ORDERORGCODE As String, ByVal I_CLASS As String) As String
        Dim SQLStr As String = ""
        '-- SELECT(共通)
        SQLStr &= " SELECT "
        SQLStr &= "    IFNULL(LNM0017.TORICODE,'') AS TORICODE "
        SQLStr &= " ,  ''   AS TORINAME "
        SQLStr &= " ,  IFNULL(LNM0017.ORDERORGCODE,'') AS ORDERORGCODE "
        SQLStr &= " ,  ''   AS ORDERORGNAME "
        SQLStr &= " ,  IFNULL(LNM0017.ORDERORGCATEGORY,'') AS ORDERORGCATEGORY "
        SQLStr &= " ,  IFNULL(LNM0017.SHUKABASHO,'') AS SHUKABASHOCODE "
        SQLStr &= " ,  ''   AS SHUKABASHONAME "
        SQLStr &= " ,  IFNULL(LNM0017.SHUKABASHOCATEGORY,'') AS SHUKABASHOCATEGORY "
        SQLStr &= " ,  IFNULL(LNM0017.TODOKECODE,'') AS TODOKECODE "
        SQLStr &= " ,  ''   AS TODOKENAME "
        SQLStr &= " ,  IFNULL(LNM0017.TODOKECATEGORY,'') AS TODOKECATEGORY "
        SQLStr &= " ,  IFNULL(LNM0017.RANGECODE,'') AS RANGECODE "
        SQLStr &= " ,  '0' AS RANGE_SUNDAY "            '-- 日曜
        SQLStr &= " ,  '0' AS RANGE_HOLIDAY "           '-- 祝日
        'SQLStr &= " ,  '0' AS RANGE_NEWYEAR "           '-- 元旦
        SQLStr &= " ,  '0' AS RANGE_YEAREND_NEWYEAR "   '-- 年末年始(元旦含む)
        SQLStr &= " ,  '0' AS RANGE_MAYDAY "            '-- 労働者の祭典
        SQLStr &= " ,  IFNULL(LNM0017.GYOMUTANKNUMFROM,'') AS GYOMUTANKNUMFROM "
        SQLStr &= " ,  IFNULL(LNM0017.GYOMUTANKNUMTO,'') AS GYOMUTANKNUMTO "
        SQLStr &= " ,  IFNULL(LNM0017.TANKA,0) AS TANKA "

        If I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 _
            AndAlso I_ORDERORGCODE <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
            '■石油資源開発(本州)
            SQLStr &= " ,  LNM0005.VALUE04   AS SETMASTERCELL "
            SQLStr &= " ,  LNM0005.KEYCODE04 AS ORDERORGCODE_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE05 AS ORDERORGNAME_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE06 AS SHUKABASHOCODE_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE07 AS SHUKABASHONAME_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE01 AS TODOKECODE_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE02 AS TODOKENAME_LNM0005 "
            SQLStr &= " ,  '' AS GYOMUTANKNUM_LNM0005 "

            '-- FROM
            SQLStr &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- LEFT JOIN
            SQLStr &= " LEFT JOIN ( "
            SQLStr &= " SELECT * FROM LNG.LNM0017_HOLIDAYRATE LNM0017 "
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0017.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0017.TORICODE = '{0}' ", I_TORICODE)

            '★受注受付部署コード
            If Not IsNothing(I_ORDERORGCODE) Then
                SQLStr &= String.Format(" AND LNM0017.ORDERORGCODE IN ({0}) ", I_ORDERORGCODE)
            End If
            SQLStr &= " ) LNM0017 ON "
            SQLStr &= " LNM0017.ORDERORGCODE = LNM0005.KEYCODE04 "

            '-- WHERE
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.KEYCODE02 NOT LIKE 'TMP%'  "
            SQLStr &= " AND LNM0005.VALUE11 = '1' "


        ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0132800000 _
            AndAlso I_ORDERORGCODE = BaseDllConst.CONST_ORDERORGCODE_020104 Then
            '■石油資源開発(北海道)
            SQLStr &= " ,  LNM0005.VALUE02   AS SETMASTERCELL "
            SQLStr &= " ,  '' AS ORDERORGCODE_LNM0005 "
            SQLStr &= " ,  '' AS ORDERORGNAME_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE02 AS SHUKABASHOCODE_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE03 AS SHUKABASHONAME_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE05 AS TODOKECODE_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE06 AS TODOKENAME_LNM0005 "
            SQLStr &= " ,  '' AS GYOMUTANKNUM_LNM0005 "

            '-- FROM
            SQLStr &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- LEFT JOIN
            SQLStr &= " LEFT JOIN ( "
            SQLStr &= " SELECT * FROM LNG.LNM0017_HOLIDAYRATE LNM0017 "
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0017.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0017.TORICODE = '{0}' ", I_TORICODE)
            '★受注受付部署コード
            If Not IsNothing(I_ORDERORGCODE) Then
                SQLStr &= String.Format(" AND LNM0017.ORDERORGCODE IN ({0}) ", I_ORDERORGCODE)
            End If
            SQLStr &= " ) LNM0017 ON "
            SQLStr &= " LNM0017.SHUKABASHO = LNM0005.KEYCODE02 "
            SQLStr &= " AND LNM0017.TODOKECODE = LNM0005.KEYCODE05 "

            '-- WHERE
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.KEYCODE08 LIKE '日祝%' "
            SQLStr &= " AND IFNULL(LNM0017.TORICODE, '') <> '' "

        ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0110600000 Then
            '■シーエナジー
            SQLStr &= " ,  IFNULL(LNM0005.VALUE11,'')   AS SETMASTERCELL "
            SQLStr &= " ,  '' AS ORDERORGCODE_LNM0005 "
            SQLStr &= " ,  '' AS ORDERORGNAME_LNM0005 "
            SQLStr &= " ,  IFNULL(LNM0005.KEYCODE05,'') AS SHUKABASHOCODE_LNM0005 "
            SQLStr &= " ,  IFNULL(LNM0005.KEYCODE06,'') AS SHUKABASHONAME_LNM0005 "
            SQLStr &= " ,  '' AS TODOKECODE_LNM0005 "
            SQLStr &= " ,  '' AS TODOKENAME_LNM0005 "
            SQLStr &= " ,  IFNULL(LNM0005.KEYCODE04,'') AS GYOMUTANKNUM_LNM0005 "

            '-- FROM
            SQLStr &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- LEFT JOIN
            SQLStr &= " LEFT JOIN ( "
            SQLStr &= " SELECT LNM0017.* ,SUBSTRING(LNM0017.GYOMUTANKNUMFROM, 1, 1) AS GYOMUTANK_FIRST "
            SQLStr &= " FROM LNG.LNM0017_HOLIDAYRATE LNM0017 "
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0017.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0017.TORICODE = '{0}' ", I_TORICODE)
            ''★受注受付部署コード
            'If Not IsNothing(I_ORDERORGCODE) Then
            '    SQLStr &= String.Format(" AND LNM0017.ORDERORGCODE IN ({0}) ", I_ORDERORGCODE)
            'End If
            SQLStr &= " ) LNM0017 ON "
            SQLStr &= " 1=1 "

            '-- WHERE
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND SUBSTRING(LNM0005.KEYCODE04, 1, 1) = GYOMUTANK_FIRST "

            '-- ORDER BY
            SQLStr &= " ORDER by CAST(LNM0005.VALUE11 AS SIGNED) "

        ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0051200000 _
            AndAlso I_ORDERORGCODE = BaseDllConst.CONST_ORDERORGCODE_022702 Then
            '■DAIGAS(泉北)
            SQLStr &= " ,  IFNULL(LNM0005.VALUE04,'')   AS SETMASTERCELL "
            SQLStr &= " ,  '' AS ORDERORGCODE_LNM0005 "
            SQLStr &= " ,  '' AS ORDERORGNAME_LNM0005 "
            SQLStr &= " ,  '' AS SHUKABASHOCODE_LNM0005 "
            SQLStr &= " ,  '' AS SHUKABASHONAME_LNM0005 "
            SQLStr &= " ,  IFNULL(LNM0005.KEYCODE01,'') AS TODOKECODE_LNM0005 "
            SQLStr &= " ,  IFNULL(LNM0005.KEYCODE02,'') AS TODOKENAME_LNM0005 "
            SQLStr &= " ,  '' AS GYOMUTANKNUM_LNM0005 "

            '-- FROM
            SQLStr &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- LEFT JOIN
            SQLStr &= " LEFT JOIN ( "
            SQLStr &= " SELECT LNM0017.* "
            SQLStr &= " FROM LNG.LNM0017_HOLIDAYRATE LNM0017 "
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0017.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0017.TORICODE = '{0}' ", I_TORICODE)
            '★受注受付部署コード
            If Not IsNothing(I_ORDERORGCODE) Then
                SQLStr &= String.Format(" AND LNM0017.ORDERORGCODE IN ({0}) ", I_ORDERORGCODE)
            End If
            SQLStr &= " ) LNM0017 ON "
            SQLStr &= " 1=1 "
            SQLStr &= " AND (LNM0017.TODOKECODE = LNM0005.KEYCODE01 OR LNM0017.TODOKECODE = '') "

            '-- WHERE
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.VALUE11 = '1' "

        ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0051200000 _
            AndAlso I_ORDERORGCODE = BaseDllConst.CONST_ORDERORGCODE_022801 Then
            '■DAIGAS(姫路営業所)
            SQLStr &= " ,  IFNULL(LNM0005.VALUE04,'')   AS SETMASTERCELL "
            SQLStr &= " ,  '' AS ORDERORGCODE_LNM0005 "
            SQLStr &= " ,  '' AS ORDERORGNAME_LNM0005 "
            SQLStr &= " ,  '' AS SHUKABASHOCODE_LNM0005 "
            SQLStr &= " ,  '' AS SHUKABASHONAME_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE01 AS TODOKECODE_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE02 AS TODOKENAME_LNM0005 "
            SQLStr &= " ,  '' AS GYOMUTANKNUM_LNM0005 "

            '-- FROM
            SQLStr &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- LEFT JOIN
            SQLStr &= " LEFT JOIN ( "
            SQLStr &= " SELECT LNM0017.* "
            SQLStr &= " FROM LNG.LNM0017_HOLIDAYRATE LNM0017 "
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0017.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0017.TORICODE = '{0}' ", I_TORICODE)
            '★受注受付部署コード
            If Not IsNothing(I_ORDERORGCODE) Then
                SQLStr &= String.Format(" AND LNM0017.ORDERORGCODE IN ({0}) ", I_ORDERORGCODE)
            End If
            SQLStr &= " ) LNM0017 ON "
            SQLStr &= " 1=1 "

            '-- WHERE
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.VALUE11 = '1' "

        ElseIf I_TORICODE = BaseDllConst.CONST_TORICODE_0239900000 Then
            '■北海道LNG
            SQLStr &= " ,  LNM0005.VALUE02   AS SETMASTERCELL "
            SQLStr &= " ,  '' AS ORDERORGCODE_LNM0005 "
            SQLStr &= " ,  '' AS ORDERORGNAME_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE04 AS SHUKABASHOCODE_LNM0005 "
            SQLStr &= " ,  LNM0005.KEYCODE06 AS SHUKABASHONAME_LNM0005 "
            SQLStr &= " ,  '' AS TODOKECODE_LNM0005 "
            SQLStr &= " ,  '' AS TODOKENAME_LNM0005 "
            SQLStr &= " ,  '' AS GYOMUTANKNUM_LNM0005 "

            '-- FROM
            SQLStr &= " FROM LNG.LNM0005_CONVERT LNM0005 "
            '-- LEFT JOIN
            SQLStr &= " LEFT JOIN ( "
            SQLStr &= " SELECT * FROM LNG.LNM0017_HOLIDAYRATE LNM0017 "
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0017.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0017.TORICODE = '{0}' ", I_TORICODE)
            '★受注受付部署コード
            If Not IsNothing(I_ORDERORGCODE) Then
                SQLStr &= String.Format(" AND LNM0017.ORDERORGCODE IN ({0}) ", I_ORDERORGCODE)
            End If
            SQLStr &= " ) LNM0017 ON "
            SQLStr &= " LNM0017.SHUKABASHO = LNM0005.KEYCODE02 "
            SQLStr &= " AND LNM0017.TODOKECODE = LNM0005.KEYCODE05 "

            '-- WHERE
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0005.CLASS = '{0}' ", I_CLASS)
            SQLStr &= " AND LNM0005.KEYCODE01 = '11' "

        Else
            '※上記(以外)
            SQLStr &= " ,  '' AS SETMASTERCELL "
            SQLStr &= " ,  '' AS ORDERORGCODE_LNM0005 "
            SQLStr &= " ,  '' AS ORDERORGNAME_LNM0005 "
            SQLStr &= " ,  '' AS SHUKABASHOCODE_LNM0005 "
            SQLStr &= " ,  '' AS SHUKABASHONAME_LNM0005 "
            SQLStr &= " ,  '' AS TODOKECODE_LNM0005 "
            SQLStr &= " ,  '' AS TODOKENAME_LNM0005 "
            SQLStr &= " ,  '' AS GYOMUTANKNUM_LNM0005 "

            '-- FROM
            SQLStr &= " FROM LNG.LNM0017_HOLIDAYRATE LNM0017 "

            '-- WHERE
            SQLStr &= " WHERE "
            SQLStr &= String.Format("     LNM0017.DELFLG <> '{0}' ", BaseDllConst.C_DELETE_FLG.DELETE)
            SQLStr &= String.Format(" AND LNM0017.TORICODE = '{0}' ", I_TORICODE)

            '★受注受付部署コード
            If Not IsNothing(I_ORDERORGCODE) Then
                SQLStr &= String.Format(" AND LNM0017.ORDERORGCODE IN ({0}) ", I_ORDERORGCODE)
            End If

        End If

        Return SQLStr
    End Function

    Private Sub selectHOLIDAYRATETodokeJudge(ByVal dtTODOKEMasrow As DataRow, ByRef dtHOLIDAYRATEMasrow As DataRow)
        '〇届け先判定区分(1：対象　2：除外)
        If dtHOLIDAYRATEMasrow("TODOKECATEGORY").ToString() = "1" Then
            If dtHOLIDAYRATEMasrow("TODOKECODE").ToString() = dtTODOKEMasrow("CHK_TODOKECODE").ToString() Then
                dtHOLIDAYRATEMasrow("SETMASTERCELL") = dtTODOKEMasrow("CHK_MASCELL").ToString()
            End If
        ElseIf dtHOLIDAYRATEMasrow("TODOKECATEGORY").ToString() = "2" Then
            If dtHOLIDAYRATEMasrow("TODOKECODE").ToString() = dtTODOKEMasrow("CHK_TODOKECODE").ToString() Then
                dtHOLIDAYRATEMasrow("SETMASTERCELL") = ""
            End If
        End If
    End Sub

    ''' <summary>
    ''' 取引マスタTBL検索
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    Public Sub SelectTORIMaster(ByVal SQLcon As MySqlConnection,
                                Optional ByRef I_dtTORISet As DataTable = Nothing,
                                Optional ByRef O_dtTORIMas As DataTable = Nothing)
        If IsNothing(O_dtTORIMas) Then
            O_dtTORIMas = New DataTable
        End If
        If O_dtTORIMas.Columns.Count <> 0 Then
            O_dtTORIMas.Columns.Clear()
        End If
        O_dtTORIMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNT0001.SHUKATORICODE "
        SQLStr &= " ,  LNT0001.SHUKATORINAME "

        '-- FROM
        SQLStr &= " FROM ( "
        SQLStr &= "     SELECT "
        SQLStr &= "       LNT0001.SHUKATORICODE"
        SQLStr &= "     , LNT0001.SHUKATORINAME"
        SQLStr &= "     , ROW_NUMBER() OVER(PARTITION BY LNT0001.SHUKATORICODE ORDER BY LNT0001.SHUKATORICODE) RNUM"
        SQLStr &= "     FROM ( "
        SQLStr &= "         SELECT DISTINCT "
        SQLStr &= "           LNT0001.SHUKATORICODE "
        SQLStr &= "         , LNT0001.SHUKATORINAME "
        SQLStr &= "         FROM LNG.LNT0001_ZISSEKI LNT0001 "
        SQLStr &= "         WHERE LNT0001.SHUKATORICODE <> '' "
        SQLStr &= "     ) LNT0001 "
        SQLStr &= " ) LNT0001 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= "     LNT0001.RNUM = 1 "

        '-- ORDER BY
        SQLStr &= " ORDER BY LNT0001.SHUKATORICODE "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtTORIMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtTORIMas.Load(SQLdr)
                End Using
            End Using

            If Not IsNothing(I_dtTORISet) Then
                For Each dtTORIMasrow As DataRow In O_dtTORIMas.Rows
                    Dim condition As String = ""
                    condition = String.Format("TORICODE='{0}' ", dtTORIMasrow("SHUKATORICODE").ToString())
                    For Each dtTORISetrow As DataRow In I_dtTORISet.Select(condition)
                        dtTORISetrow("TORINAME") = dtTORIMasrow("SHUKATORINAME").ToString()
                    Next
                Next
            End If

        Catch ex As Exception
            'Throw '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 受注受付部署マスタTBL検索
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    Public Sub SelectORDERORGMaster(ByVal SQLcon As MySqlConnection,
                                    Optional ByRef I_dtORDERORGSet As DataTable = Nothing,
                                    Optional ByRef O_dtORDERORGMas As DataTable = Nothing)
        If IsNothing(O_dtORDERORGMas) Then
            O_dtORDERORGMas = New DataTable
        End If
        If O_dtORDERORGMas.Columns.Count <> 0 Then
            O_dtORDERORGMas.Columns.Clear()
        End If
        O_dtORDERORGMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNT0001.ORDERORGCODE "
        SQLStr &= " ,  LNT0001.ORDERORGNAME "

        '-- FROM
        SQLStr &= " FROM ( "
        SQLStr &= "     SELECT "
        SQLStr &= "       LNT0001.ORDERORGCODE"
        SQLStr &= "     , LNT0001.ORDERORGNAME"
        SQLStr &= "     , ROW_NUMBER() OVER(PARTITION BY LNT0001.ORDERORGCODE ORDER BY LNT0001.ORDERORGCODE) RNUM"
        SQLStr &= "     FROM ( "
        SQLStr &= "         SELECT DISTINCT "
        SQLStr &= "           LNT0001.ORDERORGCODE "
        SQLStr &= "         , LNT0001.ORDERORGNAME "
        SQLStr &= "         FROM LNG.LNT0001_ZISSEKI LNT0001 "
        SQLStr &= "         WHERE LNT0001.ORDERORGCODE <> '' "
        SQLStr &= "     ) LNT0001 "
        SQLStr &= " ) LNT0001 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= "     LNT0001.RNUM = 1 "

        '-- ORDER BY
        SQLStr &= " ORDER BY LNT0001.ORDERORGCODE "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtORDERORGMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtORDERORGMas.Load(SQLdr)
                End Using
            End Using

            If Not IsNothing(I_dtORDERORGSet) Then
                For Each dtORDERORGMasrow As DataRow In O_dtORDERORGMas.Rows
                    Dim condition As String = ""
                    condition = String.Format("ORDERORGCODE='{0}' ", dtORDERORGMasrow("ORDERORGCODE").ToString())
                    For Each dtORDERORGSetrow As DataRow In I_dtORDERORGSet.Select(condition)
                        dtORDERORGSetrow("ORDERORGNAME") = dtORDERORGMasrow("ORDERORGNAME").ToString()
                    Next
                Next
            End If

        Catch ex As Exception
            'Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    ''' <summary>
    ''' 出荷場所マスタTBL検索
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    Public Sub SelectSHUKABASHOMaster(ByVal SQLcon As MySqlConnection,
                                      Optional ByRef I_dtSHUKABASHOSet As DataTable = Nothing,
                                      Optional ByRef O_dtSHUKABASHOMas As DataTable = Nothing)
        If IsNothing(O_dtSHUKABASHOMas) Then
            O_dtSHUKABASHOMas = New DataTable
        End If
        If O_dtSHUKABASHOMas.Columns.Count <> 0 Then
            O_dtSHUKABASHOMas.Columns.Clear()
        End If
        O_dtSHUKABASHOMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNT0001.SHUKABASHO "
        SQLStr &= " ,  LNT0001.SHUKANAME "

        '-- FROM
        SQLStr &= " FROM ( "
        SQLStr &= "     SELECT "
        SQLStr &= "       LNT0001.SHUKABASHO"
        SQLStr &= "     , LNT0001.SHUKANAME"
        SQLStr &= "     , ROW_NUMBER() OVER(PARTITION BY LNT0001.SHUKABASHO ORDER BY LNT0001.SHUKABASHO) RNUM"
        SQLStr &= "     FROM ( "
        SQLStr &= "         SELECT DISTINCT "
        SQLStr &= "           LNT0001.SHUKABASHO "
        SQLStr &= "         , LNT0001.SHUKANAME "
        SQLStr &= "         FROM LNG.LNT0001_ZISSEKI LNT0001 "
        SQLStr &= "         WHERE LNT0001.SHUKABASHO <> '' "
        SQLStr &= "     ) LNT0001 "
        SQLStr &= " ) LNT0001 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= "     LNT0001.RNUM = 1 "

        '-- ORDER BY
        SQLStr &= " ORDER BY LNT0001.SHUKABASHO "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtSHUKABASHOMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtSHUKABASHOMas.Load(SQLdr)
                End Using
            End Using

            If Not IsNothing(I_dtSHUKABASHOSet) Then
                For Each dtSHUKABASHOMasrow As DataRow In O_dtSHUKABASHOMas.Rows
                    Dim condition As String = ""
                    condition = String.Format("SHUKABASHOCODE='{0}' ", dtSHUKABASHOMasrow("SHUKABASHO").ToString())
                    For Each dtSHUKABASHOSetrow As DataRow In I_dtSHUKABASHOSet.Select(condition)
                        dtSHUKABASHOSetrow("SHUKABASHONAME") = dtSHUKABASHOMasrow("SHUKANAME").ToString()
                    Next
                Next
            End If

        Catch ex As Exception
            'Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    ''' <summary>
    ''' 届先マスタTBL検索
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    Public Sub SelectTODOKEMaster(ByVal SQLcon As MySqlConnection,
                                  Optional ByRef I_dtTODOKESet As DataTable = Nothing,
                                  Optional ByRef O_dtTODOKEMas As DataTable = Nothing)
        If IsNothing(O_dtTODOKEMas) Then
            O_dtTODOKEMas = New DataTable
        End If
        If O_dtTODOKEMas.Columns.Count <> 0 Then
            O_dtTODOKEMas.Columns.Clear()
        End If
        O_dtTODOKEMas.Clear()

        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT "
        SQLStr &= "    LNT0001.TODOKECODE "
        SQLStr &= " ,  LNT0001.TODOKENAME "

        '-- FROM
        SQLStr &= " FROM ( "
        SQLStr &= "     SELECT "
        SQLStr &= "       LNT0001.TODOKECODE"
        SQLStr &= "     , LNT0001.TODOKENAME"
        SQLStr &= "     , ROW_NUMBER() OVER(PARTITION BY LNT0001.TODOKECODE ORDER BY LNT0001.TODOKECODE) RNUM"
        SQLStr &= "     FROM ( "
        SQLStr &= "         SELECT DISTINCT "
        SQLStr &= "           LNT0001.TODOKECODE "
        SQLStr &= "         , LNT0001.TODOKENAME "
        SQLStr &= "         FROM LNG.LNT0001_ZISSEKI LNT0001 "
        SQLStr &= "         WHERE LNT0001.TODOKECODE <> '' "
        SQLStr &= "     ) LNT0001 "
        SQLStr &= " ) LNT0001 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= "     LNT0001.RNUM = 1 "

        '-- ORDER BY
        SQLStr &= " ORDER BY LNT0001.TODOKECODE "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtTODOKEMas.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtTODOKEMas.Load(SQLdr)
                End Using
            End Using

            If Not IsNothing(I_dtTODOKESet) Then
                For Each dtTODOKEMasrow As DataRow In O_dtTODOKEMas.Rows
                    Dim condition As String = ""
                    condition = String.Format("TODOKECODE='{0}' ", dtTODOKEMasrow("TODOKECODE").ToString())
                    For Each dtTODOKESetrow As DataRow In I_dtTODOKESet.Select(condition)
                        dtTODOKESetrow("TODOKENAME") = dtTODOKEMasrow("TODOKENAME").ToString()
                    Next
                Next
            End If

        Catch ex As Exception
            'Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    ''' <summary>
    ''' 実績TBL検索
    ''' </summary>
    Public Function SelectZissekiSQL(ByVal WF_TORI As String, ByVal WF_TORIORG As String,
                                     Optional ByVal WF_TODOKE As String = Nothing,
                                     Optional ByVal WF_TANKNUMBER As String = Nothing,
                                     Optional ByVal WF_GYOMUTANKNO As String = Nothing,
                                     Optional ByVal WF_TORIORG_MAP As String = Nothing) As String
        Dim SQLStr As String =
              " Select                                                                            " _
            & "      1                                                    AS 'SELECT'             " _
            & "     ,0                                                    AS HIDDEN               " _
            & "     ,0                                                    AS LINECNT              " _
            & "     ,''                                                   AS OPERATION            " _
            & "     ,''                                                   AS OPERATIONCB          " _
            & "     ,coalesce(LT1.RECONO, '')                             AS RECONO			    " _
            & "     ,coalesce(LT1.LOADUNLOTYPE, '')                       AS LOADUNLOTYPE		    " _
            & "     ,coalesce(LT1.STACKINGTYPE, '')                       AS STACKINGTYPE		    " _
            & "     ,coalesce(LT1.HSETID, '')                             AS HSETID			    " _
            & "     ,coalesce(LT1.ORDERORGSELECT, '')                     AS ORDERORGSELECT	    " _
            & "     ,coalesce(LT1.ORDERORGNAME, '')                       AS ORDERORGNAME		    " _
            & "     ,coalesce(LT1.ORDERORGCODE, '')                       AS ORDERORGCODE		    " _
            & "     ,coalesce(LT1.ORDERORGNAMES, '')                      AS ORDERORGNAMES	    " _
            & "     ,coalesce(LT1.KASANAMEORDERORG, '')                   AS KASANAMEORDERORG	    " _
            & "     ,coalesce(LT1.KASANCODEORDERORG, '')                  AS KASANCODEORDERORG	" _
            & "     ,coalesce(LT1.KASANAMESORDERORG, '')                  AS KASANAMESORDERORG	" _
            & "     ,coalesce(LT1.ORDERORG, '')                           AS ORDERORG				" _
            & "     ,coalesce(LT1.KASANORDERORG, '')                      AS KASANORDERORG		" _
            & "     ,coalesce(LT1.PRODUCTSLCT, '')                        AS PRODUCTSLCT			" _
            & "     ,coalesce(LT1.PRODUCTSYOSAI, '')                      AS PRODUCTSYOSAI		" _
            & "     ,coalesce(LT1.PRODUCT2NAME, '')                       AS PRODUCT2NAME			" _
            & "     ,coalesce(LT1.PRODUCT2, '')                           AS PRODUCT2				" _
            & "     ,coalesce(LT1.PRODUCT1NAME, '')                       AS PRODUCT1NAME			" _
            & "     ,coalesce(LT1.PRODUCT1, '')                           AS PRODUCT1				" _
            & "     ,coalesce(LT1.OILNAME, '')                            AS OILNAME				" _
            & "     ,coalesce(LT1.OILTYPE, '')                            AS OILTYPE				" _
            & "     ,coalesce(LT1.TODOKESLCT, '')                         AS TODOKESLCT			" _
            & "     ,coalesce(LT1.TODOKECODE, '')                         AS TODOKECODE			" _
            & "     ,coalesce(LT1.TODOKENAME, '')                         AS TODOKENAME			" _
            & "     ,coalesce(LT1.TODOKENAMES, '')                        AS TODOKENAMES			" _
            & "     ,coalesce(LT1.TORICODE, '')                           AS TORICODE				" _
            & "     ,coalesce(LT1.TORINAME, '')                           AS TORINAME				" _
            & "     ,coalesce(LT1.TODOKEADDR, '')                         AS TODOKEADDR			" _
            & "     ,coalesce(LT1.TODOKETEL, '')                          AS TODOKETEL			" _
            & "     ,coalesce(LT1.TODOKEMAP, '')                          AS TODOKEMAP			" _
            & "     ,coalesce(LT1.TODOKEIDO, '')                          AS TODOKEIDO			" _
            & "     ,coalesce(LT1.TODOKEKEIDO, '')                        AS TODOKEKEIDO			" _
            & "     ,coalesce(LT1.TODOKEBIKO1, '')                        AS TODOKEBIKO1			" _
            & "     ,coalesce(LT1.TODOKEBIKO2, '')                        AS TODOKEBIKO2			" _
            & "     ,coalesce(LT1.TODOKEBIKO3, '')                        AS TODOKEBIKO3			" _
            & "     ,coalesce(LT1.TODOKECOLOR1, '')                       AS TODOKECOLOR1			" _
            & "     ,coalesce(LT1.TODOKECOLOR2, '')                       AS TODOKECOLOR2			" _
            & "     ,coalesce(LT1.TODOKECOLOR3, '')                       AS TODOKECOLOR3			" _
            & "     ,coalesce(LT1.SHUKASLCT, '')                          AS SHUKASLCT			" _
            & "     ,coalesce(LT1.SHUKABASHO, '')                         AS SHUKABASHO			" _
            & "     ,coalesce(LT1.SHUKANAME, '')                          AS SHUKANAME			" _
            & "     ,coalesce(LT1.SHUKANAMES, '')                         AS SHUKANAMES			" _
            & "     ,coalesce(LT1.SHUKATORICODE, '')                      AS SHUKATORICODE		" _
            & "     ,coalesce(LT1.SHUKATORINAME, '')                      AS SHUKATORINAME		" _
            & "     ,coalesce(LT1.SHUKAADDR, '')                          AS SHUKAADDR			" _
            & "     ,coalesce(LT1.SHUKAADDRTEL, '')                       AS SHUKAADDRTEL			" _
            & "     ,coalesce(LT1.SHUKAMAP, '')                           AS SHUKAMAP				" _
            & "     ,coalesce(LT1.SHUKAIDO, '')                           AS SHUKAIDO				" _
            & "     ,coalesce(LT1.SHUKAKEIDO, '')                         AS SHUKAKEIDO			" _
            & "     ,coalesce(LT1.SHUKABIKOU1, '')                        AS SHUKABIKOU1			" _
            & "     ,coalesce(LT1.SHUKABIKOU2, '')                        AS SHUKABIKOU2			" _
            & "     ,coalesce(LT1.SHUKABIKOU3, '')                        AS SHUKABIKOU3			" _
            & "     ,coalesce(LT1.SHUKACOLOR1, '')                        AS SHUKACOLOR1			" _
            & "     ,coalesce(LT1.SHUKACOLOR2, '')                        AS SHUKACOLOR2			" _
            & "     ,coalesce(LT1.SHUKACOLOR3, '')                        AS SHUKACOLOR3			" _
            & "     ,coalesce(LT1.SHUKADATE, '')                          AS SHUKADATE			" _
            & "     ,coalesce(LT1.LOADTIME, '')                           AS LOADTIME				" _
            & "     ,coalesce(LT1.LOADTIMEIN, '')                         AS LOADTIMEIN			" _
            & "     ,coalesce(LT1.LOADTIMES, '')                          AS LOADTIMES			" _
            & "     ,coalesce(LT1.TODOKEDATE, '')                         AS TODOKEDATE			" _
            & "     ,ROW_NUMBER() OVER(PARTITION BY coalesce(LT1.SYAGATA, ''),coalesce(LT1.SHUKADATE_MG, '') ORDER BY coalesce(LT1.SYAGATA, ''),coalesce(LT1.SHUKADATE, ''),coalesce(LT1.TODOKEDATE, '') ) TODOKEDATE_ROWNUM " _
            & "     ,ROW_NUMBER() OVER(PARTITION BY coalesce(LT1.TODOKECODE, ''),coalesce(LT1.TODOKEDATE, '') ORDER BY coalesce(LT1.TODOKECODE, ''),coalesce(LT1.TODOKEDATE, ''),coalesce(LT1.SHITEITIMES, '') ) TODOKEDATE_ORDER " _
            & "     ,coalesce(LT1.SHITEITIME, '')                         AS SHITEITIME			" _
            & "     ,coalesce(LT1.SHITEITIMEIN, '')                       AS SHITEITIMEIN			" _
            & "     ,coalesce(LT1.SHITEITIMES, '')                        AS SHITEITIMES			" _
            & "     ,coalesce(LT1.ZYUTYU, '')                             AS ZYUTYU				" _
            & "     ,coalesce(LT1.ZISSEKI, '')                            AS ZISSEKI				" _
            & "     ,coalesce(LT1.TANNI, '')                              AS TANNI				" _
            & "     ,coalesce(LT1.GYOUMUSIZI1, '')                        AS GYOUMUSIZI1			" _
            & "     ,coalesce(LT1.GYOUMUSIZI2, '')                        AS GYOUMUSIZI2			" _
            & "     ,coalesce(LT1.GYOUMUSIZI3, '')                        AS GYOUMUSIZI3			" _
            & "     ,coalesce(LT1.NINUSHIBIKOU, '')                       AS NINUSHIBIKOU			" _
            & "     ,coalesce(LT1.GYOMUSYABAN, '')                        AS GYOMUSYABAN			" _
            & "     ,coalesce(LT1.SHIPORGNAME, '')                        AS SHIPORGNAME			" _
            & "     ,coalesce(LT1.SHIPORG, '')                            AS SHIPORG				" _
            & "     ,coalesce(LT1.SHIPORGNAMES, '')                       AS SHIPORGNAMES			" _
            & "     ,coalesce(LT1.KASANSHIPORGNAME, '')                   AS KASANSHIPORGNAME	    " _
            & "     ,coalesce(LT1.KASANSHIPORG, '')                       AS KASANSHIPORG			" _
            & "     ,coalesce(LT1.KASANSHIPORGNAMES, '')                  AS KASANSHIPORGNAMES	" _
            & "     ,coalesce(LT1.TANKNUM, '')                            AS TANKNUM				" _
            & "     ,coalesce(LT1.TANKNUMBER, '')                         AS TANKNUMBER			" _
            & "     ,coalesce(LT1.SYAGATA, '')                            AS SYAGATA				" _
            & "     ,coalesce(LT1.SYABARA, '')                            AS SYABARA				" _
            & "     ,coalesce(LT1.NINUSHINAME, '')                        AS NINUSHINAME			" _
            & "     ,coalesce(LT1.CONTYPE, '')                            AS CONTYPE				" _
            & "     ,coalesce(LT1.PRO1SYARYOU, '')                        AS PRO1SYARYOU			" _
            & "     ,coalesce(LT1.TANKMEMO, '')                           AS TANKMEMO				" _
            & "     ,coalesce(LT1.TANKBIKOU1, '')                         AS TANKBIKOU1			" _
            & "     ,coalesce(LT1.TANKBIKOU2, '')                         AS TANKBIKOU2			" _
            & "     ,coalesce(LT1.TANKBIKOU3, '')                         AS TANKBIKOU3			" _
            & "     ,coalesce(LT1.TRACTORNUM, '')                         AS TRACTORNUM			" _
            & "     ,coalesce(LT1.TRACTORNUMBER, '')                      AS TRACTORNUMBER		" _
            & "     ,coalesce(LT1.TRIP, '')                               AS TRIP					" _
            & "     ,ROW_NUMBER() OVER(PARTITION BY coalesce(LT1.TANKNUMBER, ''),coalesce(LT1.SHUKADATE_MG, '') ORDER BY coalesce(LT1.TANKNUMBER, ''),coalesce(LT1.SHUKADATE, ''),coalesce(LT1.TODOKEDATE, ''),coalesce(LT1.TRIP, '') ) TRIP_REP " _
            & "     ,coalesce(LT1.DRP, '')                                AS DRP					" _
            & "     ,coalesce(LT1.UNKOUMEMO, '')                          AS UNKOUMEMO			" _
            & "     ,coalesce(LT1.SHUKKINTIME, '')                        AS SHUKKINTIME			" _
            & "     ,coalesce(LT1.STAFFSLCT, '')                          AS STAFFSLCT			" _
            & "     ,coalesce(LT1.STAFFNAME, '')                          AS STAFFNAME			" _
            & "     ,coalesce(LT1.STAFFCODE, '')                          AS STAFFCODE			" _
            & "     ,coalesce(LT1.SUBSTAFFSLCT, '')                       AS SUBSTAFFSLCT			" _
            & "     ,coalesce(LT1.SUBSTAFFNAME, '')                       AS SUBSTAFFNAME			" _
            & "     ,coalesce(LT1.SUBSTAFFNUM, '')                        AS SUBSTAFFNUM			" _
            & "     ,coalesce(LT1.CALENDERMEMO1, '')                      AS CALENDERMEMO1		" _
            & "     ,coalesce(LT1.CALENDERMEMO2, '')                      AS CALENDERMEMO2		" _
            & "     ,coalesce(LT1.CALENDERMEMO3, '')                      AS CALENDERMEMO3		" _
            & "     ,coalesce(LT1.CALENDERMEMO4, '')                      AS CALENDERMEMO4		" _
            & "     ,coalesce(LT1.CALENDERMEMO5, '')                      AS CALENDERMEMO5		" _
            & "     ,coalesce(LT1.CALENDERMEMO6, '')                      AS CALENDERMEMO6		" _
            & "     ,coalesce(LT1.CALENDERMEMO7, '')                      AS CALENDERMEMO7		" _
            & "     ,coalesce(LT1.CALENDERMEMO8, '')                      AS CALENDERMEMO8		" _
            & "     ,coalesce(LT1.CALENDERMEMO9, '')                      AS CALENDERMEMO9		" _
            & "     ,coalesce(LT1.CALENDERMEMO10, '')                     AS CALENDERMEMO10		" _
            & "     ,coalesce(LT1.GYOMUTANKNUM, '')                       AS GYOMUTANKNUM			" _
            & "     ,coalesce(LT1.YOUSYA, '')                             AS YOUSYA				" _
            & "     ,coalesce(LT1.RECOTITLE, '')                          AS RECOTITLE			" _
            & "     ,coalesce(LT1.SHUKODATE, '')                          AS SHUKODATE			" _
            & "     ,coalesce(LT1.KIKODATE, '')                           AS KIKODATE				" _
            & "     ,coalesce(LT1.KIKOTIME, '')                           AS KIKOTIME				" _
            & "     ,coalesce(LT1.CREWBIKOU1, '')                         AS CREWBIKOU1			" _
            & "     ,coalesce(LT1.CREWBIKOU2, '')                         AS CREWBIKOU2			" _
            & "     ,coalesce(LT1.SUBCREWBIKOU1, '')                      AS SUBCREWBIKOU1		" _
            & "     ,coalesce(LT1.SUBCREWBIKOU2, '')                      AS SUBCREWBIKOU2		" _
            & "     ,coalesce(LT1.SUBSHUKKINTIME, '')                     AS SUBSHUKKINTIME		" _
            & "     ,coalesce(LT1.CALENDERMEMO11, '')                     AS CALENDERMEMO11		" _
            & "     ,coalesce(LT1.CALENDERMEMO12, '')                     AS CALENDERMEMO12		" _
            & "     ,coalesce(LT1.CALENDERMEMO13, '')                     AS CALENDERMEMO13		" _
            & "     ,coalesce(LT1.SYABARATANNI, '')                       AS SYABARATANNI			" _
            & "     ,coalesce(LT1.TAIKINTIME, '')                         AS TAIKINTIME			" _
            & "     ,coalesce(LT1.SUBTIKINTIME, '')                       AS SUBTIKINTIME			" _
            & "     ,coalesce(LT1.KVTITLE, '')                            AS KVTITLE				" _
            & "     ,coalesce(LT1.KVZYUTYU, '')                           AS KVZYUTYU				" _
            & "     ,coalesce(LT1.KVZISSEKI, '')                          AS KVZISSEKI			" _
            & "     ,coalesce(LT1.KVCREW, '')                             AS KVCREW				" _
            & "     ,coalesce(LT1.CREWCODE, '')                           AS CREWCODE				" _
            & "     ,coalesce(LT1.SUBCREWCODE, '')                        AS SUBCREWCODE			" _
            & "     ,coalesce(LT1.KVSUBCREW, '')                          AS KVSUBCREW			" _
            & "     ,coalesce(LT1.ORDERHENKO, '')                         AS ORDERHENKO			" _
            & "     ,coalesce(LT1.RIKUUNKYOKU, '')                        AS RIKUUNKYOKU			" _
            & "     ,coalesce(LT1.BUNRUINUMBER, '')                       AS BUNRUINUMBER			" _
            & "     ,coalesce(LT1.HIRAGANA, '')                           AS HIRAGANA				" _
            & "     ,coalesce(LT1.ITIRENNUM, '')                          AS ITIRENNUM			" _
            & "     ,coalesce(LT1.TRACTER1, '')                           AS TRACTER1				" _
            & "     ,coalesce(LT1.TRACTER2, '')                           AS TRACTER2				" _
            & "     ,coalesce(LT1.TRACTER3, '')                           AS TRACTER3				" _
            & "     ,coalesce(LT1.TRACTER4, '')                           AS TRACTER4				" _
            & "     ,coalesce(LT1.TRACTER5, '')                           AS TRACTER5				" _
            & "     ,coalesce(LT1.TRACTER6, '')                           AS TRACTER6				" _
            & "     ,coalesce(LT1.TRACTER7, '')                           AS TRACTER7				" _
            & "     ,coalesce(LT1.HAISYAHUKA, '')                         AS HAISYAHUKA			" _
            & "     ,coalesce(LT1.HYOZIZYUNT, '')                         AS HYOZIZYUNT			" _
            & "     ,coalesce(LT1.HYOZIZYUNH, '')                         AS HYOZIZYUNH			" _
            & "     ,coalesce(LT1.HONTRACTER1, '')                        AS HONTRACTER1			" _
            & "     ,coalesce(LT1.HONTRACTER2, '')                        AS HONTRACTER2			" _
            & "     ,coalesce(LT1.HONTRACTER3, '')                        AS HONTRACTER3			" _
            & "     ,coalesce(LT1.HONTRACTER4, '')                        AS HONTRACTER4			" _
            & "     ,coalesce(LT1.HONTRACTER5, '')                        AS HONTRACTER5			" _
            & "     ,coalesce(LT1.HONTRACTER6, '')                        AS HONTRACTER6			" _
            & "     ,coalesce(LT1.HONTRACTER7, '')                        AS HONTRACTER7			" _
            & "     ,coalesce(LT1.HONTRACTER8, '')                        AS HONTRACTER8			" _
            & "     ,coalesce(LT1.HONTRACTER9, '')                        AS HONTRACTER9			" _
            & "     ,coalesce(LT1.HONTRACTER10, '')                       AS HONTRACTER10			" _
            & "     ,coalesce(LT1.HONTRACTER11, '')                       AS HONTRACTER11			" _
            & "     ,coalesce(LT1.HONTRACTER12, '')                       AS HONTRACTER12			" _
            & "     ,coalesce(LT1.HONTRACTER13, '')                       AS HONTRACTER13			" _
            & "     ,coalesce(LT1.HONTRACTER14, '')                       AS HONTRACTER14			" _
            & "     ,coalesce(LT1.HONTRACTER15, '')                       AS HONTRACTER15			" _
            & "     ,coalesce(LT1.HONTRACTER16, '')                       AS HONTRACTER16			" _
            & "     ,coalesce(LT1.HONTRACTER17, '')                       AS HONTRACTER17			" _
            & "     ,coalesce(LT1.HONTRACTER18, '')                       AS HONTRACTER18			" _
            & "     ,coalesce(LT1.HONTRACTER19, '')                       AS HONTRACTER19			" _
            & "     ,coalesce(LT1.HONTRACTER20, '')                       AS HONTRACTER20			" _
            & "     ,coalesce(LT1.HONTRACTER21, '')                       AS HONTRACTER21			" _
            & "     ,coalesce(LT1.HONTRACTER22, '')                       AS HONTRACTER22			" _
            & "     ,coalesce(LT1.HONTRACTER23, '')                       AS HONTRACTER23			" _
            & "     ,coalesce(LT1.HONTRACTER24, '')                       AS HONTRACTER24			" _
            & "     ,coalesce(LT1.HONTRACTER25, '')                       AS HONTRACTER25			" _
            & "     ,coalesce(LT1.CALENDERMEMO14, '')                     AS CALENDERMEMO14		" _
            & "     ,coalesce(LT1.CALENDERMEMO15, '')                     AS CALENDERMEMO15		" _
            & "     ,coalesce(LT1.CALENDERMEMO16, '')                     AS CALENDERMEMO16		" _
            & "     ,coalesce(LT1.CALENDERMEMO17, '')                     AS CALENDERMEMO17		" _
            & "     ,coalesce(LT1.CALENDERMEMO18, '')                     AS CALENDERMEMO18		" _
            & "     ,coalesce(LT1.CALENDERMEMO19, '')                     AS CALENDERMEMO19		" _
            & "     ,coalesce(LT1.CALENDERMEMO20, '')                     AS CALENDERMEMO20		" _
            & "     ,coalesce(LT1.CALENDERMEMO21 , '')                    AS CALENDERMEMO21		" _
            & "     ,coalesce(LT1.CALENDERMEMO22, '')                     AS CALENDERMEMO22		" _
            & "     ,coalesce(LT1.CALENDERMEMO23, '')                     AS CALENDERMEMO23		" _
            & "     ,coalesce(LT1.CALENDERMEMO24, '')                     AS CALENDERMEMO24		" _
            & "     ,coalesce(LT1.CALENDERMEMO25, '')                     AS CALENDERMEMO25		" _
            & "     ,coalesce(LT1.CALENDERMEMO26, '')                     AS CALENDERMEMO26		" _
            & "     ,coalesce(LT1.CALENDERMEMO27, '')                     AS CALENDERMEMO27		" _
            & "     ,coalesce(LT1.BRANCHCODE, '')                         AS BRANCHCODE			" _
            & "     ,''                                                   AS BRANCHNAME			" _
            & "     ,coalesce(LT1.UPDATEUSER, '')                         AS UPDATEUSER			" _
            & "     ,coalesce(LT1.CREATEUSER, '')                         AS CREATEUSER			" _
            & "     ,coalesce(LT1.UPDATEYMD, '')                          AS UPDATEYMD			" _
            & "     ,coalesce(LT1.CREATEYMD, '')                          AS CREATEYMD			" _
            & "     ,coalesce(LT1.DELFLG, '')                             AS DELFLG				" _
            & "     ,coalesce(LT1.INITYMD, '')                            AS INITYMD				" _
            & "     ,coalesce(LT1.INITUSER, '')                           AS INITUSER				" _
            & "     ,coalesce(LT1.INITTERMID, '')                         AS INITTERMID			" _
            & "     ,coalesce(LT1.INITPGID, '')                           AS INITPGID				" _
            & "     ,coalesce(LT1.UPDYMD, '')                             AS UPDYMD				" _
            & "     ,coalesce(LT1.UPDUSER, '')                            AS UPDUSER				" _
            & "     ,coalesce(LT1.UPDTERMID, '')                          AS UPDTERMID			" _
            & "     ,coalesce(LT1.UPDPGID, '')                            AS UPDPGID				" _
            & "     ,coalesce(LT1.RECEIVEYMD, '')                         AS RECEIVEYMD			" _
            & "     ,coalesce(LT1.UPDTIMSTP, '')                          AS UPDTIMSTP			" _
            & " FROM (                                                                " _
            & " SELECT                                                                " _
            & "      LT1.*                                                            " _
            & "     ,CASE @P4 " _
            & "      WHEN DATE_FORMAT(LT1.SHUKADATE, '%Y/%m') THEN LT1.TODOKEDATE " _
            & "      ELSE LT1.SHUKADATE " _
            & "      END AS SHUKADATE_MG " _
            & " FROM                                                                " _
            & "     LNG.LNT0001_ZISSEKI LT1                                         " _
            & " WHERE                                                               " _
            & "     date_format(LT1.TODOKEDATE, '%Y/%m/%d') >= @P2                  " _
            & " AND date_format(LT1.TODOKEDATE, '%Y/%m/%d') <= @P3                  " _
            & " AND LT1.ZISSEKI <> 0                                                "

        '〇シーエナジー
        If WF_TORI = BaseDllConst.CONST_TORICODE_0110600000 Then
            '★北陸エルネスも含める
            SQLStr &= String.Format(" AND LT1.TORICODE IN (@P5, '{0}') ", BaseDllConst.CONST_TORICODE_0238900000)
        Else
            SQLStr &= " AND LT1.TORICODE = @P5                                              "
        End If
        SQLStr &= " AND LT1.ORDERORGCODE in (" & WF_TORIORG & ")"

        '〇西日本支店車庫
        If WF_TORIORG_MAP = CONST_ORDERORGCODE_022702 + "01" Then
            '★[Daigas泉北]選択時
            SQLStr &= String.Format(" AND LT1.TODOKECODE <> '{0}' ", BaseDllConst.CONST_TODOKECODE_001640)
        ElseIf WF_TORIORG_MAP = CONST_ORDERORGCODE_022702 + "02" Then
            '★[Daigas新宮]選択時
            SQLStr &= String.Format(" AND LT1.TODOKECODE = '{0}' ", BaseDllConst.CONST_TODOKECODE_001640)
        ElseIf WF_TORIORG_MAP = CONST_ORDERORGCODE_022702 + "03" Then
            '★[エスケイ産業]選択時
            SQLStr &= String.Format(" AND LT1.TODOKECODE = '{0}' ", BaseDllConst.CONST_TODOKECODE_004559)
        End If

        SQLStr &= String.Format(" AND LT1.DELFLG = '{0}' ", BaseDllConst.C_DELETE_FLG.ALIVE)

        '〇届先(設定時)
        If Not IsNothing(WF_TODOKE) AndAlso WF_TODOKE <> "" Then
            SQLStr &= " AND LT1.TODOKECODE in (" & WF_TODOKE & ")"
        End If
        '〇陸自番号(設定時)
        If Not IsNothing(WF_TANKNUMBER) AndAlso WF_TANKNUMBER <> "" Then
            SQLStr &= " AND LT1.TANKNUMBER in (" & WF_TANKNUMBER & ")"
        End If
        '〇業務車番(設定時)
        If Not IsNothing(WF_GYOMUTANKNO) AndAlso WF_GYOMUTANKNO <> "" Then
            SQLStr &= " AND LT1.GYOMUTANKNUM in (" & WF_GYOMUTANKNO & ")"
        End If

        SQLStr &= " ) LT1                                                                "

        '★統合版単価マスタ('1':調整単価)
        SQLStr &= " INNER JOIN LNG.LNM0006_NEWTANKA LNM0006 ON "
        SQLStr &= "     LNM0006.TANKAKBN = '1' "
        SQLStr &= " AND LT1.TODOKEDATE BETWEEN LNM0006.STYMD AND LNM0006.ENDYMD "
        SQLStr &= " AND LNM0006.TORICODE = LT1.TORICODE "
        SQLStr &= " AND LNM0006.AVOCADOTODOKECODE = LT1.TODOKECODE "

        '★車番も条件の取引コードの場合は含める
        If WF_TORI = BaseDllConst.CONST_TORICODE_0110600000 Then
            '〇シーエナジー
            SQLStr &= " AND LNM0006.SHABAN = LT1.GYOMUTANKNUM "
            SQLStr &= " AND LNM0006.AVOCADOSHUKABASHO = LT1.SHUKABASHO "

        ElseIf WF_TORI = BaseDllConst.CONST_TORICODE_0175400000 Then
            '〇東北電力
            SQLStr &= " AND LNM0006.SHABAN = LT1.GYOMUTANKNUM "

        ElseIf WF_TORI = BaseDllConst.CONST_TORICODE_0132800000 _
            AndAlso WF_TORIORG <> BaseDllConst.CONST_ORDERORGCODE_020104 Then
            '〇石油資源開発(本州)
            SQLStr &= " AND LNM0006.SHABAN = LT1.GYOMUTANKNUM "

        End If

        SQLStr &= " ORDER BY                                                            "
        SQLStr &= "     LT1.ORDERORGCODE, LT1.SHUKADATE, LT1.TODOKEDATE, LT1.TODOKECODE  "

        Return SQLStr
    End Function

    ''' <summary>
    ''' 北海道LNG(シート[輸送費明細])【基本料金A】取得用SQL
    ''' </summary>
    Public Sub SelectHokkaidoLNG_YusouhiKihonFeeA(ByVal I_CLASS As String,
                                                  ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TAISHOYM As String,
                                                  ByRef O_dtYusouhiFEEA As DataTable, ByRef O_dtYusouhiSyabanFEEA As DataTable)
        Dim SQLStr As String = ""
        Dim SQLSYABANStr As String = ""
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0005.SYAKONO "
        SQLStr &= " ,  LNM0005.SYAKONAME "
        SQLStr &= " ,  LNM0005.SYAGATA "
        SQLStr &= " ,  LNM0005.SYAGATANAME "
        SQLStr &= " ,  LNM0005.SORTNO "
        SQLStr &= " ,  IFNULL(LNM0007.SYABARA, 0)     AS SYABARA "
        SQLStr &= " ,  IFNULL(LNM0007.SYAKO_COUNT, 0) AS SYAKO_COUNT "
        SQLStr &= " ,  IFNULL(LNM0007.KOTEIHIM, 0)    AS KOTEIHIM "
        SQLStr &= " ,  LNM0005.SETCELLNO "
        SQLStr &= " ,  '' AS SYABAN "

        '-- FROM
        SQLStr &= " FROM "
        '-- ①変換マスタ【基本料金A】(雛型)取得
        SQLStr &= " ( "
        SQLStr &= "     SELECT "
        SQLStr &= "        LNM0005.KEYCODE03 "
        SQLStr &= "     ,  LNM0005.KEYCODE04 AS SYAKONO "
        SQLStr &= "     ,  LNM0005.KEYCODE06 AS SYAKONAME "
        SQLStr &= "     ,  LNM0005.KEYCODE07 AS SYAGATA "
        SQLStr &= "     ,  LNM0005.KEYCODE08 AS SYAGATANAME "
        SQLStr &= "     ,  LNM0005.KEYCODE09 AS SORTNO "
        SQLStr &= "     ,  LNM0005.VALUE02 AS SETCELLNO "
        SQLStr &= "     FROM LNG.LNM0005_CONVERT LNM0005 "
        SQLStr &= String.Format("     WHERE LNM0005.CLASS = '{0}' ", I_CLASS)
        SQLStr &= "       AND LNM0005.KEYCODE01 = '1' "
        SQLStr &= " ) LNM0005 "

        '-- LEFT JOIN
        SQLStr &= " LEFT JOIN "
        '-- ②統合版固定費マスタ【基本料金A】[台数][料金]取得
        SQLStr &= " ( "
        SQLStr &= "     SELECT "
        SQLStr &= "        LNM0007.SYAKONO "
        SQLStr &= "     ,  LNM0007.SYAKONAME "
        SQLStr &= "     ,  LNM0007.SYAGATA "
        SQLStr &= "     ,  LNM0007.SYAGATANAME "
        SQLStr &= "     ,  ROW_NUMBER() OVER(PARTITION BY LNM0007.SYAKONO, LNM0007.SYAGATA"
        SQLStr &= "                          ORDER BY LNM0007.SYAKONO, LNM0007.SYAGATA ) AS SORTNO "
        SQLStr &= "     ,  LNM0007.SYABARA "
        SQLStr &= "     ,  LNM0007.KOTEIHIM "
        SQLStr &= "     ,  COUNT(1) AS SYAKO_COUNT "
        SQLStr &= "     FROM ( "

        SQLSYABANStr &= "         SELECT "
        SQLSYABANStr &= "            CASE "
        SQLSYABANStr &= "            WHEN SUBSTRING(LNM0007.RIKUBAN,1,2) = '室蘭' THEN '2' "
        SQLSYABANStr &= "            WHEN SUBSTRING(LNM0007.RIKUBAN,1,2) = '釧路' THEN '3' "
        SQLSYABANStr &= "            ELSE '1' "
        SQLSYABANStr &= "            END AS SYAKONO "
        SQLSYABANStr &= "         ,  SUBSTRING(LNM0007.RIKUBAN,1,2) AS SYAKONAME "
        SQLSYABANStr &= "         ,  LNM0007.SYABAN "
        SQLSYABANStr &= "         ,  LNM0007.SYAGATA "
        SQLSYABANStr &= "         ,  LNM0007.SYAGATANAME "
        SQLSYABANStr &= "         ,  LNM0007.SYABARA "
        SQLSYABANStr &= "         ,  LNM0007.KOTEIHIM "
        SQLSYABANStr &= "         FROM LNG.LNM0007_FIXED LNM0007 "
        SQLSYABANStr &= String.Format("         WHERE LNM0007.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)
        SQLSYABANStr &= String.Format("           AND LNM0007.TORICODE = '{0}'  ", I_TORICODE)
        SQLSYABANStr &= String.Format("           AND LNM0007.ORGCODE  = '{0}'  ", I_ORGCODE)
        SQLSYABANStr &= String.Format("           AND LNM0007.TARGETYM = '{0}'  ", I_TAISHOYM)

        SQLStr &= SQLSYABANStr
        SQLStr &= "     ) LNM0007 "
        SQLStr &= "     GROUP BY "
        SQLStr &= "        LNM0007.SYAKONO "
        SQLStr &= "     ,  LNM0007.SYAKONAME "
        SQLStr &= "     ,  LNM0007.SYAGATA "
        SQLStr &= "     ,  LNM0007.SYAGATANAME "
        SQLStr &= "     ,  LNM0007.SYABARA "
        SQLStr &= "     ,  LNM0007.KOTEIHIM "
        SQLStr &= " ) LNM0007 ON "

        '--LEFT JOIN(条件)
        SQLStr &= "     LNM0005.SYAKONO = LNM0007.SYAKONO "
        SQLStr &= " AND LNM0005.SYAGATA = LNM0007.SYAGATA "
        SQLStr &= " AND LNM0005.SORTNO  = LNM0007.SORTNO "

        '--ORDER BY
        SQLStr &= " ORDER BY LNM0005.SYAKONO, LNM0005.SYAGATA, LNM0005.SORTNO, LNM0007.KOTEIHIM "

        '〇SQL結果取得
        O_dtYusouhiFEEA = SelectSearch(SQLStr)
        O_dtYusouhiSyabanFEEA = SelectSearch(SQLSYABANStr)

        '〇車号の設定
        For Each O_dtYusouhiFEEArow As DataRow In O_dtYusouhiFEEA.Rows
            Dim arrSyaban As String = ""
            Dim listSyaban As New List(Of String)
            Dim condition As String = ""
            condition &= String.Format(" SYAKONO='{0}' ", O_dtYusouhiFEEArow("SYAKONO").ToString())
            condition &= String.Format(" AND SYAGATA='{0}' ", O_dtYusouhiFEEArow("SYAGATA").ToString())
            condition &= String.Format(" AND KOTEIHIM='{0}' ", O_dtYusouhiFEEArow("KOTEIHIM").ToString())
            Dim i As Integer = 0
            Dim firstFlg As Boolean = True
            For Each O_dtYusouhiSyabanFEEArow As DataRow In O_dtYusouhiSyabanFEEA.Select(condition)
                '★室蘭や釧路表記を削除
                Dim syban As String = O_dtYusouhiSyabanFEEArow("SYABAN").ToString().Replace("室蘭", "").Replace("釧路", "")
                syban &= "号車"
                '★初回車番の場合
                If firstFlg = True Then
                    firstFlg = False
                    arrSyaban &= syban
                Else
                    '★４車両毎に改行
                    If i = 4 Then
                        arrSyaban &= ControlChars.NewLine   '// 改行
                        arrSyaban &= syban                  '// 改行後は、カンマなし
                        i = 0
                    Else
                        arrSyaban &= "," + syban
                    End If
                End If
                i += 1
            Next
            O_dtYusouhiFEEArow("SYABAN") = arrSyaban
        Next

    End Sub

    ''' <summary>
    ''' 北海道LNG(シート[輸送費明細])【休日割増料金(回数)】取得用SQL
    ''' </summary>
    Public Sub SelectHokkaidoLNG_YusouhiHolidayRate(ByVal I_TORICODE As String, ByVal I_TAISHOYM As String,
                                                    ByRef O_dtYusouhiHRate As DataTable)
        '★月末日取得
        Dim lastDay As String = ""
        lastDay = Date.Parse(I_TAISHOYM).AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")

        Dim SQLStr As String = ""
        SQLStr &= " SELECT "
        SQLStr &= "    LNM0016.GRPKEY "
        SQLStr &= " ,  LNM0016.GRPKEYNAME "
        SQLStr &= " ,  COUNT(1) AS GRPCNT "
        SQLStr &= " ,  LNM0016.TANKA "

        '--FROM
        SQLStr &= " FROM "
        '-- ①カレンダーマスタ【休日】(実績データ)取得
        SQLStr &= " ( "
        SQLStr &= "     SELECT "
        SQLStr &= "        LNM0016.TORICODE "
        SQLStr &= "     ,  LNM0016.YMD "
        SQLStr &= "     ,  LNM0016.WORKINGDAY "
        SQLStr &= "     ,  LNM0016.WORKINGDAYNAME "
        SQLStr &= "     ,  LNM0016.PUBLICHOLIDAYNAME "
        SQLStr &= "     ,  CASE "
        SQLStr &= "        WHEN SUBSTRING(LNT0001.TANKNUMBER,1,2) = '室蘭' THEN 2 "
        SQLStr &= "        WHEN SUBSTRING(LNT0001.TANKNUMBER,1,2) = '釧路' THEN 3 "
        SQLStr &= "        ELSE 1 "
        SQLStr &= "        END GRPKEY "
        SQLStr &= "     ,  CASE "
        SQLStr &= "        WHEN SUBSTRING(LNT0001.TANKNUMBER,1,2) = '室蘭' THEN '室蘭車庫' "
        SQLStr &= "        WHEN SUBSTRING(LNT0001.TANKNUMBER,1,2) = '釧路' THEN '釧路車庫' "
        SQLStr &= "        ELSE '石狩車庫' "
        SQLStr &= "        END GRPKEYNAME "
        SQLStr &= "     ,  LNT0001.GYOMUTANKNUM "
        SQLStr &= "     ,  LNM0017.TANKA "
        SQLStr &= "     FROM LNG.LNM0016_CALENDAR LNM0016 "

        '--INNER JOIN(実績データ)
        SQLStr &= "     INNER JOIN LNG.LNT0001_ZISSEKI LNT0001 ON "
        SQLStr &= "           LNT0001.ZISSEKI <> 0 "
        SQLStr &= "       AND LNT0001.TORICODE = LNM0016.TORICODE "
        SQLStr &= "       AND LNT0001.TODOKEDATE = LNM0016.YMD "

        '--INNER JOIN(休日割増単価マスタ)
        SQLStr &= "     INNER JOIN LNG.LNM0017_HOLIDAYRATE LNM0017 ON "
        SQLStr &= "           LNM0017.TORICODE = LNM0016.TORICODE "

        '--WHERE
        SQLStr &= String.Format("     WHERE LNM0016.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= String.Format("       AND LNM0016.YMD BETWEEN '{0}' AND '{1}' ", I_TAISHOYM, lastDay)
        SQLStr &= "       AND LNM0016.WORKINGDAY <> '0' "
        SQLStr &= "       AND LNM0016.WORKINGDAY <> '5' "
        SQLStr &= " ) LNM0016 "

        '--GROUP BY
        SQLStr &= " GROUP BY "
        SQLStr &= "    LNM0016.GRPKEY "
        SQLStr &= " ,  LNM0016.GRPKEYNAME "
        SQLStr &= " ,  LNM0016.TANKA "

        '--ORDER BY
        SQLStr &= " ORDER BY "
        SQLStr &= "    LNM0016.GRPKEY "
        SQLStr &= " ,  LNM0016.GRPKEYNAME "

        '〇SQL結果取得
        O_dtYusouhiHRate = SelectSearch(SQLStr)

    End Sub

    ''' <summary>
    ''' 統合版単価マスタ(枝番)取得用SQL
    ''' </summary>
    Public Sub SelectNewTanka_BRANCHCODE(ByVal I_TORICODE As String, ByVal I_TAISHOYM As String,
                                         ByRef O_dtNewTanka_BRANCHCODE As DataTable)
        Dim SQLStr As String = ""
        '-- SELECT
        SQLStr &= " SELECT DISTINCT "
        SQLStr &= "    LNM0006.BRANCHCODE "

        '-- FROM
        SQLStr &= " FROM LNG.LNM0006_NEWTANKA LNM0006 "

        '-- WHERE
        SQLStr &= " WHERE "
        SQLStr &= String.Format(" LNM0006.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)
        SQLStr &= String.Format(" AND LNM0006.TORICODE = '{0}' ", I_TORICODE)
        SQLStr &= String.Format(" AND '{0}' BETWEEN LNM0006.STYMD AND LNM0006.ENDYMD ", I_TAISHOYM)

        '〇SQL結果取得
        O_dtNewTanka_BRANCHCODE = SelectSearch(SQLStr)

    End Sub

    ''' <summary>
    ''' マスタ検索処理
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="O_VALUE"></param>
    Public Sub FixvalueMasterSearch(ByVal I_CODE As String,
                                       ByVal I_CLASS As String,
                                       ByVal I_KEYCODE As String,
                                       ByRef O_VALUE() As String,
                                       Optional ByVal I_LODDATE As String = Nothing,
                                       Optional ByVal I_PARA01 As String = Nothing,
                                       Optional ByVal blnDelete As Boolean = False)
        Dim Fixvaltbl As DataTable = Nothing
        If IsNothing(Fixvaltbl) Then
            Fixvaltbl = New DataTable
        End If

        If Fixvaltbl.Columns.Count <> 0 Then
            Fixvaltbl.Columns.Clear()
        End If

        Fixvaltbl.Clear()

        Try
            'DBより取得
            Fixvaltbl = FixvalueMasterDataGet(I_CODE, I_CLASS, I_KEYCODE, I_PARA01)
            Dim j As Integer = 0
            If blnDelete = True AndAlso Fixvaltbl.Rows.Count > 1 Then
                For Each dtfxrow As DataRow In Fixvaltbl.Rows
                    If j > 0 Then
                        dtfxrow("DELFLG") = C_DELETE_FLG.DELETE
                    End If
                    j += 1
                Next
            End If

            If I_KEYCODE.Equals("") Then

                If IsNothing(I_PARA01) Then
                    For Each dtfxrow As DataRow In Fixvaltbl.Rows
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = Convert.ToString(dtfxrow("VALUE" & i.ToString()))
                        Next
                    Next
                ElseIf I_PARA01 = "1" Then    '### 油種登録用の油種コードを取得 ###
                    Dim i As Integer = 0
                    For Each dtfxrow As DataRow In Fixvaltbl.Rows
                        Try
                            If Date.Parse(dtfxrow("STYMD").ToString()) <= Date.Parse(I_LODDATE) _
                                AndAlso Date.Parse(dtfxrow("ENDYMD").ToString()) >= Date.Parse(I_LODDATE) Then
                                O_VALUE(i) = Convert.ToString(dtfxrow("KEYCODE")).Replace(Convert.ToString(dtfxrow("VALUE2")), "")
                                i += 1
                            End If
                        Catch ex As Exception
                            Exit For
                        End Try
                    Next
                End If

            Else
                If IsNothing(I_PARA01) Then
                    For Each dtfxrow As DataRow In Fixvaltbl.Rows
                        If blnDelete = True AndAlso Convert.ToString(dtfxrow("DELFLG")) = C_DELETE_FLG.DELETE Then
                            Continue For
                        End If
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = Convert.ToString(dtfxrow("VALUE" & i.ToString()))
                        Next
                    Next
                ElseIf I_PARA01 = "1" Then
                    Dim i As Integer = 0
                    For Each dtfxrow As DataRow In Fixvaltbl.Rows
                        Try
                            If Date.Parse(dtfxrow("STYMD").ToString()) <= Date.Parse(I_LODDATE) _
                                AndAlso Date.Parse(dtfxrow("ENDYMD").ToString()) >= Date.Parse(I_LODDATE) Then
                                O_VALUE(0) = Convert.ToString(dtfxrow("KEYCODE")).Replace(Convert.ToString(dtfxrow("VALUE2")), "")
                                O_VALUE(1) = Convert.ToString(dtfxrow("VALUE3"))
                                O_VALUE(2) = Convert.ToString(dtfxrow("VALUE2"))
                                O_VALUE(3) = Convert.ToString(dtfxrow("VALUE1"))
                                'O_VALUE(i) = Convert.ToString(OIT0003WKrow("KEYCODE")).Replace(Convert.ToString(OIT0003WKrow("VALUE2")), "")
                                'i += 1
                            End If
                        Catch ex As Exception
                            Exit For
                        End Try
                    Next
                End If
            End If

        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub
    ''' <summary>
    ''' マスタ検索処理（同じパラメータならDB抽出せずに保持内容を返却）
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="I_PARA01"></param>
    ''' <returns></returns>
    Private Function FixvalueMasterDataGet(I_CODE As String, I_CLASS As String, I_KEYCODE As String, I_PARA01 As String) As DataTable
        Static keyValues As Dictionary(Of String, String)
        Static retDt As DataTable
        Dim retFilterdDt As DataTable
        'キー情報を比較または初期状態または異なるキーの場合は再抽出
        If keyValues Is Nothing OrElse
           (Not (keyValues("I_CODE") = I_CODE _
                 AndAlso keyValues("I_CLASS") = I_CLASS _
                 AndAlso keyValues("I_PARA01") = I_PARA01)) Then
            keyValues = New Dictionary(Of String, String) _
                      From {{"I_CODE", I_CODE}, {"I_CLASS", I_CLASS}, {"I_PARA01", I_PARA01}}
            retDt = New DataTable
        Else
            retFilterdDt = retDt
            '抽出キー情報が一致しているので保持内容を返却
            If I_KEYCODE <> "" Then
                Dim qKeyFilterd = From dr In retDt Where dr("KEYCODE").Equals(I_KEYCODE)
                If qKeyFilterd.Any Then
                    retFilterdDt = qKeyFilterd.CopyToDataTable
                Else
                    retFilterdDt = retDt.Clone
                End If
            End If

            Return retFilterdDt
        End If
        'キーが変更された場合の抽出処理
        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)
        MySqlConnection.ClearPool(SQLcon)

        '検索SQL文
        Dim SQLStr As String =
           " SELECT" _
            & "   ISNULL(RTRIM(VIW0001.CAMPCODE), '')    AS CAMPCODE" _
            & " , ISNULL(RTRIM(VIW0001.CLASS), '')       AS CLASS" _
            & " , ISNULL(RTRIM(VIW0001.KEYCODE), '')     AS KEYCODE" _
            & " , ISNULL(RTRIM(VIW0001.STYMD), '')       AS STYMD" _
            & " , ISNULL(RTRIM(VIW0001.ENDYMD), '')      AS ENDYMD" _
            & " , ISNULL(RTRIM(VIW0001.VALUE1), '')      AS VALUE1" _
            & " , ISNULL(RTRIM(VIW0001.VALUE2), '')      AS VALUE2" _
            & " , ISNULL(RTRIM(VIW0001.VALUE3), '')      AS VALUE3" _
            & " , ISNULL(RTRIM(VIW0001.VALUE4), '')      AS VALUE4" _
            & " , ISNULL(RTRIM(VIW0001.VALUE5), '')      AS VALUE5" _
            & " , ISNULL(RTRIM(VIW0001.VALUE6), '')      AS VALUE6" _
            & " , ISNULL(RTRIM(VIW0001.VALUE7), '')      AS VALUE7" _
            & " , ISNULL(RTRIM(VIW0001.VALUE8), '')      AS VALUE8" _
            & " , ISNULL(RTRIM(VIW0001.VALUE9), '')      AS VALUE9" _
            & " , ISNULL(RTRIM(VIW0001.VALUE10), '')     AS VALUE10" _
            & " , ISNULL(RTRIM(VIW0001.VALUE11), '')     AS VALUE11" _
            & " , ISNULL(RTRIM(VIW0001.VALUE12), '')     AS VALUE12" _
            & " , ISNULL(RTRIM(VIW0001.VALUE13), '')     AS VALUE13" _
            & " , ISNULL(RTRIM(VIW0001.VALUE14), '')     AS VALUE14" _
            & " , ISNULL(RTRIM(VIW0001.VALUE15), '')     AS VALUE15" _
            & " , ISNULL(RTRIM(VIW0001.VALUE16), '')     AS VALUE16" _
            & " , ISNULL(RTRIM(VIW0001.VALUE17), '')     AS VALUE17" _
            & " , ISNULL(RTRIM(VIW0001.VALUE18), '')     AS VALUE18" _
            & " , ISNULL(RTRIM(VIW0001.VALUE19), '')     AS VALUE19" _
            & " , ISNULL(RTRIM(VIW0001.VALUE20), '')     AS VALUE20" _
            & " , ISNULL(RTRIM(VIW0001.SYSTEMKEYFLG), '')   AS SYSTEMKEYFLG" _
            & " , ISNULL(RTRIM(VIW0001.DELFLG), '')      AS DELFLG" _
            & " FROM  LNG.VIW0001_FIXVALUE VIW0001" _
            & " WHERE VIW0001.CLASS = @P01" _
            & " AND VIW0001.DELFLG <> @P03"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '会社コード
        If Not String.IsNullOrEmpty(I_CODE) Then
            SQLStr &= String.Format("    AND VIW0001.CAMPCODE = '{0}'", I_CODE)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    VIW0001.KEYCODE"

        Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)

            Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)
            'Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)
            Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar)

            PARA01.Value = I_CLASS
            'PARA02.Value = I_KEYCODE
            PARA03.Value = C_DELETE_FLG.DELETE

            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    retDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                retDt.Load(SQLdr)
            End Using
            'CLOSE
            SQLcmd.Dispose()
        End Using

        retFilterdDt = retDt
        '抽出キー情報が一致しているので保持内容を返却
        If I_KEYCODE <> "" Then
            Dim qKeyFilterd = From dr In retDt Where dr("KEYCODE").Equals(I_KEYCODE)
            If qKeyFilterd.Any Then
                retFilterdDt = qKeyFilterd.CopyToDataTable
            Else
                retFilterdDt = retDt.Clone
            End If
        End If

        Return retFilterdDt
    End Function

    ''' <summary>
    ''' TBL更新(訂正更新用)
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <param name="I_TABLENAME">更新対象テーブル名(スキーマ付)</param>
    ''' <param name="I_WCONDITION">WHERE追加条件(ない場合は空文字)</param>
    ''' <param name="I_TABLEITEM">更新対象(項目)</param>
    ''' <param name="I_TABLEITEM_PARA">更新対象(値)</param>
    ''' <remarks></remarks>
    Public Sub UpdateTableCRT(ByVal SQLcon As MySqlConnection,
                              ByVal I_TABLENAME As String, ByVal I_WCONDITION As String,
                              ByVal I_TABLEITEM As String, ByVal I_TABLEITEM_PARA As String)

        '更新SQL文
        Dim SQLStr As String = ""
        '-- TABLE
        SQLStr &= " UPDATE " & I_TABLENAME

        '-- SET
        SQLStr &= "    SET "
        If I_TABLEITEM_PARA = "" Then
            SQLStr &= String.Format(" {0} = {1} ", I_TABLEITEM, DBNull.Value)
        Else
            SQLStr &= String.Format(" {0} = '{1}' ", I_TABLEITEM, I_TABLEITEM_PARA)
        End If

        '-- WHERE
        If I_WCONDITION = "" Then
            SQLStr &= String.Format("  WHERE DELFLG     <> '{0}' ", C_DELETE_FLG.DELETE)
        Else
            SQLStr &= String.Format("  WHERE {0} ", I_WCONDITION)
            SQLStr &= String.Format("    AND DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)
        End If

        Try
            Dim SQLcmd As New MySqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300
            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    Public Function filterItem(ByVal dt As DataTable, ByVal fieldItem01 As String, ByVal fieldItem02 As String) As String
        Dim viw As New DataView(dt)
        Dim cols() As String = {fieldItem01, fieldItem02}
        Dim dtFilter As DataTable = viw.ToTable(True, cols)
        Dim itemJoint As String = ""

        Dim i As Integer = 0
        For Each row As DataRow In dtFilter.Rows
            If i = 0 Then
                itemJoint = String.Format("'{0}'", row(fieldItem01))
                i += 1
            Else
                itemJoint &= String.Format(",'{0}'", row(fieldItem01))
            End If
        Next

        Return itemJoint
    End Function

End Class
