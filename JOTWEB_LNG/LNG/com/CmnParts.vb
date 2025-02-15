﻿Option Explicit On
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
        SQLStr &= String.Format(" AND LNM0006.ORGCODE = '{0}' ", I_ORGCODE)
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

End Class
