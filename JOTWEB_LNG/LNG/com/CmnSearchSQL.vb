Option Strict On

Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRC0002SELECTIONPOPUPWORKINC

''' <summary>
''' コンテナ用 共通クラス(検索用SQL)
''' </summary>
''' <remarks>各種受注データテーブルに登録する際はこちらに定義</remarks>
Public Class CmnSearchSQL
    Inherits System.Web.UI.Page

    ''' <summary>
    ''' 取引先コード　書式
    ''' </summary>
    Public Const C_TORICODE_FORMAT As String = "^([0-9]{10}|[Z]{5}[0-9]{5})$"

    ''' <summary>
    ''' 受託人検索タイトル取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetTrusteeTitle() As IEnumerable(Of DispFieldItem)

        Dim colTitle As IEnumerable(Of DispFieldItem)
        colTitle = {
            New DispFieldItem("TRUSTEECD", "受託人コード", "140"),
            New DispFieldItem("TRUSTEESUBCD", "受託人サブコード", "140"),
            New DispFieldItem("VIEWTRUSTEENM", "受託人", "320"),
            New DispFieldItem("INVSUBCD", "細分コード", "100"),
            New DispFieldItem("DEPTNAME", "請求部店", "120"),
            New DispFieldItem("BRANCHNAME", "計上部店", "120")
        }

        Return colTitle

    End Function

    ''' <summary>
    ''' 受託人取得SQL
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetTrusteeSQL(ByVal prmStationCode As String) As String

        Dim SQLBldr As New StringBuilder
        Dim WW_DATENOW As DateTime = DateTime.Now
        Dim strDate As String = WW_DATENOW.ToString("yyyy/MM/dd")
        strDate = "'" & strDate & "'"

        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    (")
        SQLBldr.AppendLine("    FORMAT(MAIN.DEPSTATION, '000000')")
        SQLBldr.AppendLine("  + FORMAT(MAIN.DEPTRUSTEECD, '00000')")
        SQLBldr.AppendLine("  + FORMAT(MAIN.DEPTRUSTEESUBCD, '000')")
        SQLBldr.AppendLine("    ) AS KEYCODE")
        SQLBldr.AppendLine("   ,MAIN.DEPSTATION AS STATION")
        SQLBldr.AppendLine("   ,MAIN.DEPTRUSTEECD AS TRUSTEECD")
        SQLBldr.AppendLine("   ,MAIN.DEPTRUSTEESUBCD AS TRUSTEESUBCD")
        SQLBldr.AppendLine("   ,RTRIM(MAIN.DEPTRUSTEENM) AS TRUSTEENM")
        SQLBldr.AppendLine("   ,coalesce(RTRIM(MAIN.DEPTRUSTEESUBNM) , '') AS TRUSTEESUBNM")
        SQLBldr.AppendLine("   ,RTRIM(MAIN.DEPTRUSTEENM) + coalesce(RTRIM(MAIN.DEPTRUSTEESUBNM) , '') AS VIEWTRUSTEENM") '画面表示用
        SQLBldr.AppendLine("   ,MAIN.TORICODE AS TORICODE")
        SQLBldr.AppendLine("   ,MAIN.INVSUBCD AS INVSUBCD")
        SQLBldr.AppendLine("   ,DEPT.NAMES AS DEPTNAME")
        SQLBldr.AppendLine("   ,BRNC.NAMES AS BRANCHNAME")
        SQLBldr.AppendLine("FROM")
        SQLBldr.AppendLine("    lng.LNM0003_REKEJM MAIN")
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0014_ORG DEPT")
        SQLBldr.AppendLine("        ON DEPT.CAMPCODE = '01'")
        SQLBldr.AppendLine("        AND DEPT.ORGCODE = MAIN.INVFILINGDEPT")
        SQLBldr.AppendLine("        AND DEPT.DELFLG = '0'")
        SQLBldr.AppendLine("        AND " & strDate & " >= DEPT.STYMD")
        SQLBldr.AppendLine("        AND " & strDate & " <= DEPT.ENDYMD")
        SQLBldr.AppendLine("    LEFT JOIN com.LNS0014_ORG BRNC")
        SQLBldr.AppendLine("        ON BRNC.CAMPCODE = '01'")
        SQLBldr.AppendLine("        AND BRNC.ORGCODE = MAIN.INVKEIJYOBRANCHCD")
        SQLBldr.AppendLine("        AND BRNC.DELFLG = '0'")
        SQLBldr.AppendLine("        AND " & strDate & " >= BRNC.STYMD")
        SQLBldr.AppendLine("        AND " & strDate & " <= BRNC.ENDYMD")
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("   MAIN.DELFLG = '0'")
        '駅コードが入力されている場合条件に含める
        If Not prmStationCode = "" Then
            SQLBldr.AppendLine("  AND MAIN.DEPSTATION = '" & prmStationCode & "'")
        End If
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     STATION, TRUSTEECD, TRUSTEESUBCD")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 受託人コード取得SQL(受託人名称検索)
    ''' </summary>
    ''' <param name="prmTrusteeName">受託人名</param>
    ''' <returns></returns>
    Public Shared Function GetTrusteeCodeSQL(ByVal prmStationCode As String,
                                             ByVal prmTrusteeName As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 受託人コード取得(受託人名称検索)
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     A.TRUSTEECD")
        SQLBldr.AppendLine("    ,A.TRUSTEESUBCD")
        SQLBldr.AppendLine("    ,A.TRUSTEENM")
        SQLBldr.AppendLine("    ,A.TRUSTEESUBNM")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine(" (")
        SQLBldr.AppendLine("    SELECT")
        SQLBldr.AppendLine("        DEPTRUSTEECD AS TRUSTEECD")
        SQLBldr.AppendLine("       ,DEPTRUSTEESUBCD AS TRUSTEESUBCD")
        SQLBldr.AppendLine("       ,RTRIM(DEPTRUSTEENM) AS TRUSTEENM")
        SQLBldr.AppendLine("       ,RTRIM(DEPTRUSTEESUBNM) AS TRUSTEESUBNM")
        SQLBldr.AppendLine("       ,RTRIM(DEPTRUSTEENM) + coalesce(RTRIM(DEPTRUSTEESUBNM) , '') AS NAME")
        SQLBldr.AppendLine("    FROM")
        SQLBldr.AppendLine("     lng.LNM0003_REKEJM")
        SQLBldr.AppendLine("    WHERE")
        SQLBldr.AppendLine("        DELFLG = '0'")
        '駅コードが入力されている場合条件に含める
        If Not prmStationCode = "" Then
            SQLBldr.AppendLine("  AND DEPSTATION = '" & prmStationCode & "'")
        End If
        SQLBldr.AppendLine(" ) A")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     A.NAME LIKE '%" & prmTrusteeName & "%'")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 受託人名取得SQL(受託人コード検索)
    ''' </summary>
    ''' <param name="prmTrusteeCode">発受託人コード</param>
    ''' <returns></returns>
    Public Shared Function GetTrusteeNameSQL(ByVal prmStationCode As String,
                                             ByVal prmTrusteeCode As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 受託人コード取得(受託人名称検索)
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("       DEPTRUSTEECD AS TRUSTEECD")
        SQLBldr.AppendLine("      ,DEPTRUSTEESUBCD AS TRUSTEESUBCD")
        SQLBldr.AppendLine("      ,RTRIM(DEPTRUSTEENM) AS TRUSTEENM")
        SQLBldr.AppendLine("      ,RTRIM(DEPTRUSTEESUBNM) AS TRUSTEESUBNM")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0003_REKEJM")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     DELFLG = '0'")
        SQLBldr.AppendLine(" AND DEPTRUSTEECD = '" & prmTrusteeCode & "'")
        '駅コードが入力されている場合条件に含める
        If Not prmStationCode = "" Then
            SQLBldr.AppendLine("  AND DEPSTATION = '" & prmStationCode & "'")
        End If
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     DEPTRUSTEECD")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 品目検索タイトル取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetItemTitle() As IEnumerable(Of DispFieldItem)

        Dim colTitle As IEnumerable(Of DispFieldItem)
        colTitle = {
            New DispFieldItem("ITEMCD", "品目コード", "110"),
            New DispFieldItem("NAME", "品目名称", "350")
        }

        Return colTitle

    End Function

    ''' <summary>
    ''' 品目取得SQL
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetItemSQL() As String

        Dim SQLBldr As New StringBuilder

        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("       ITEMCD AS KEYCODE")
        SQLBldr.AppendLine("     , RTRIM(ITEMCD) AS ITEMCD")
        SQLBldr.AppendLine("     , RTRIM(NAME) AS NAME")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0021_ITEM")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     DELFLG = '0'")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     ITEMCD")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 品目コード取得SQL(品目名称検索)
    ''' </summary>
    ''' <param name="prmItemName">荷主名</param>
    ''' <returns></returns>
    Public Shared Function GetItemCodeSQL(ByVal prmItemName As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 品目コード取得(品目名称検索)
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     ITEMCD")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0021_ITEM")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     NAME LIKE '%" & prmItemName & "%'")
        SQLBldr.AppendLine(" AND DELFLG = '0'")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 品目名取得SQL(品目コード検索)
    ''' </summary>
    ''' <param name="prmItemCode">荷主コード</param>
    ''' <returns></returns>
    Public Shared Function GetItemNameSQL(ByVal prmItemCode As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 品目コード取得(品目名称検索)
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("       ITEMCD")
        SQLBldr.AppendLine("     , NAME AS NAME")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0021_ITEM")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     ITEMCD = '" & prmItemCode & "'")
        SQLBldr.AppendLine(" AND DELFLG = '0'")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     ITEMCD")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 荷主検索タイトル取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetShipperTitle() As IEnumerable(Of DispFieldItem)

        Dim colTitle As IEnumerable(Of DispFieldItem)
        colTitle = {
            New DispFieldItem("SHIPPERCD", "荷主コード", "110"),
            New DispFieldItem("NAME", "荷主名称", "350")
        }

        Return colTitle

    End Function

    ''' <summary>
    ''' 荷主取得SQL
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetShipperSQL() As String

        Dim SQLBldr As New StringBuilder

        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("       SHIPPERCD AS KEYCODE")
        SQLBldr.AppendLine("     , RTRIM(SHIPPERCD) AS SHIPPERCD")
        SQLBldr.AppendLine("     , RTRIM(NAME) AS NAME")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0023_SHIPPER")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     DELFLG = '0'")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     SHIPPERCD")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 荷主コード取得SQL(荷主名称検索)
    ''' </summary>
    ''' <param name="prmShipperName">荷主名</param>
    ''' <returns></returns>
    Public Shared Function GetShipperCodeSQL(ByVal prmShipperName As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 荷主コード取得(荷主名称検索)
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     SHIPPERCD")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0023_SHIPPER")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     NAME LIKE '%" & prmShipperName & "%'")
        SQLBldr.AppendLine(" AND DELFLG = '0'")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 荷主名取得SQL(荷主コード検索)
    ''' </summary>
    ''' <param name="prmShipperCode">荷主コード</param>
    ''' <returns></returns>
    Public Shared Function GetShipperNameSQL(ByVal prmShipperCode As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 荷主コード取得(荷主名称検索)
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("       SHIPPERCD")
        SQLBldr.AppendLine("     , NAME AS NAME")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0023_SHIPPER")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     SHIPPERCD = '" & prmShipperCode & "'")
        SQLBldr.AppendLine(" AND DELFLG = '0'")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     SHIPPERCD")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 取引先検索タイトル取得(請求先)※支払先とは違うため、注意
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetToriTitle() As IEnumerable(Of DispFieldItem)

        Dim colTitle As IEnumerable(Of DispFieldItem)
        colTitle = {
            New DispFieldItem("TORICODE", "取引先コード", "110"),
            New DispFieldItem("ORGNAMES", "提出部店名", "110"),
            New DispFieldItem("TORINAME", "取引先名", "510"),
            New DispFieldItem("DIVNAME", "取引先部門名", "380")
        }

        Return colTitle

    End Function

    ''' <summary>
    ''' 取引先取得SQL(請求先)※支払先とは違うため、注意
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetToriSQL() As String

        Dim SQLBldr As New StringBuilder
        Dim WW_DATENOW As DateTime = DateTime.Now
        Dim strDate As String = WW_DATENOW.ToString("yyyy/MM/dd")
        strDate = "'" & strDate & "'"

        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("       LNM0024.TORICODE + LNM0024.INVFILINGDEPT AS KEYCODE")
        SQLBldr.AppendLine("     , LNM0024.TORICODE AS TORICODE")
        SQLBldr.AppendLine("     , LNM0024.INVFILINGDEPT AS INVFILINGDEPT")
        SQLBldr.AppendLine("     , MAX(RTRIM(LNS0019.NAMES)) AS ORGNAMES") '組織名称（短）
        SQLBldr.AppendLine("     , MAX(LNM0024.TORINAME) AS TORINAME")
        SQLBldr.AppendLine("     , MAX(coalesce(LNM0024.TORIDIVNAME,'')) AS DIVNAME")
        SQLBldr.AppendLine("     , LNM0024.TORICODE AS CODE")
        SQLBldr.AppendLine("     , MAX(LNM0024.TORINAME) + MAX(coalesce(LNM0024.TORIDIVNAME,'')) AS NAME")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0024_KEKKJM LNM0024")
        SQLBldr.AppendLine(" INNER JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG LNS0019")
        SQLBldr.AppendLine(" ON LNS0019.CAMPCODE = '01'")
        SQLBldr.AppendLine(" AND LNS0019.ORGCODE = LNM0024.INVFILINGDEPT")
        SQLBldr.AppendLine(" AND LNS0019.DELFLG = '0'")
        SQLBldr.AppendLine(" AND " & strDate & " >= LNS0019.STYMD")
        SQLBldr.AppendLine(" AND " & strDate & " <= LNS0019.ENDYMD")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     LNM0024.DELFLG = '0'")
        SQLBldr.AppendLine(" GROUP BY")
        SQLBldr.AppendLine("     LNM0024.TORICODE, LNM0024.INVFILINGDEPT")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     LNM0024.TORICODE, LNM0024.INVFILINGDEPT")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 取引先コード取得SQL(取引先名称検索)(請求先)※支払先とは違うため、注意
    ''' </summary>
    ''' <param name="prmToriName">取引先名</param>
    ''' <returns></returns>
    Public Shared Function GetToriCodeSQL(ByVal prmToriName As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 取引先コード取得(取引先名称検索)
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     TORICODE")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0024_KEKKJM")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     TORINAME + coalesce(TORIDIVNAME , '') LIKE '%" & prmToriName & "%'")
        SQLBldr.AppendLine(" AND DELFLG = '0'")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 取引先名取得SQL(取引先コード検索)(請求先)※支払先とは違うため、注意
    ''' </summary>
    ''' <param name="prmToriCode">取引先コード</param>
    ''' <returns></returns>
    Public Shared Function GetToriNameSQL(ByVal prmToriCode As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 取引先コード取得(取引先名称検索)
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("       TORICODE")
        SQLBldr.AppendLine("     , TORINAME + coalesce(TORIDIVNAME , '') AS TORINAME")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0024_KEKKJM")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     TORICODE = '" & prmToriCode & "'")
        SQLBldr.AppendLine(" AND DELFLG = '0'")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     TORICODE")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 取引先名取得SQL(取引先コード検索)_ドロップダウンリスト用
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetToriDdlSQL() As String

        Dim SQLBldr As New StringBuilder
        Dim WW_DATENOW As DateTime = DateTime.Now
        Dim strDate As String = WW_DATENOW.ToString("yyyy/MM/dd")
        strDate = "'" & strDate & "'"

        SQLBldr.AppendLine(" SELECT DISTINCT")
        SQLBldr.AppendLine("       LNM0024.TORICODE AS CODE")
        SQLBldr.AppendLine("     , LNM0024.TORINAME + coalesce(LNM0024.TORIDIVNAME,'') AS NAME")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNM0024_KEKKJM LNM0024")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     LNM0024.DELFLG = '0'")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 取引先ドロップダウンリスト作成(非表示項目)
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function getDdlTori() As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION

        Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
            sqlCmd As New MySqlCommand(CmnSearchSQL.GetToriDdlSQL, sqlCon)
            sqlCon.Open()
            MySqlConnection.ClearPool(sqlCon)

            Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows = False Then
                    Return retList
                End If
                While sqlDr.Read
                    Dim listItm As New ListItem(Convert.ToString(sqlDr("NAME")), Convert.ToString(sqlDr("CODE")))
                    retList.Items.Add(listItm)
                End While
            End Using
        End Using

        Return retList

    End Function

    ''' <summary>
    ''' 駅検索タイトル取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetStationTitle() As IEnumerable(Of DispFieldItem)

        Dim colTitle As IEnumerable(Of DispFieldItem)
        colTitle = {
                New DispFieldItem("STATION", "駅コード", "110"),
                New DispFieldItem("NAMES", "駅名", "350"),
                New DispFieldItem("ORGNAMES", "組織名", "200")
            }

        Return colTitle

    End Function

    ''' <summary>
    ''' 駅取得SQL
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetStationSQL(ByVal prmCampCode As String, Optional ByVal prmOrgCode As String = "") As String

        Dim SQLBldr As New StringBuilder
        Dim WW_DATENOW As DateTime = DateTime.Now
        Dim strDate As String = WW_DATENOW.ToString("yyyy/MM/dd")
        strDate = "'" & strDate & "'"

        SQLBldr.AppendLine(" SELECT ")
        SQLBldr.AppendLine("       RTRIM(LNS0020.STATION) AS KEYCODE")  'プライマリーキー
        SQLBldr.AppendLine("     , RTRIM(LNS0020.STATION) AS STATION")  '駅コード
        SQLBldr.AppendLine("     , RTRIM(LNS0020.NAME)    AS NAME")     '駅名称
        SQLBldr.AppendLine("     , RTRIM(LNS0020.NAMES)   AS NAMES")    '駅名称（短） 基本はこちらを使用
        SQLBldr.AppendLine("     , RTRIM(LNS0020.ORGCODE) AS ORGCODE")  '組織コード
        SQLBldr.AppendLine("     , RTRIM(LNS0019.NAMES)   AS ORGNAMES") '組織名称（短）
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     com.LNS0020_STATION LNS0020")
        SQLBldr.AppendLine(" LEFT JOIN")
        SQLBldr.AppendLine("     com.LNS0014_ORG LNS0019")
        SQLBldr.AppendLine(" ON LNS0019.CAMPCODE = LNS0020.CAMPCODE")
        SQLBldr.AppendLine(" AND LNS0019.ORGCODE = LNS0020.ORGCODE")
        SQLBldr.AppendLine(" AND LNS0019.DELFLG = '0'")
        SQLBldr.AppendLine(" AND " & strDate & " >= LNS0019.STYMD")
        SQLBldr.AppendLine(" AND " & strDate & " <= LNS0019.ENDYMD")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     LNS0020.DELFLG = '0'")
        SQLBldr.AppendLine(" AND LNS0020.CAMPCODE = '" & prmCampCode & "'")
        ' 組織コード
        If Not String.IsNullOrEmpty(prmOrgCode) Then
            SQLBldr.AppendLine(" AND LNS0020.ORGCODE = '" & prmOrgCode & "'")
        End If
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     LNS0020.STATION")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 駅コード取得SQL(駅名称検索)
    ''' </summary>
    ''' <param name="prmStationName">駅名</param>
    ''' <returns></returns>
    Public Shared Function GetStationCodeSQL(ByVal prmCampCode As String, ByVal prmStationName As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 取引先コード取得(取引先名称検索)
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     STATION")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     com.LNS0020_STATION")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     DELFLG = '0'")
        SQLBldr.AppendLine(" AND CAMPCODE = '" & prmCampCode & "'")
        SQLBldr.AppendLine(" AND (")
        SQLBldr.AppendLine("      NAME LIKE '%" & prmStationName & "%'")
        SQLBldr.AppendLine("   OR NAMES LIKE '%" & prmStationName & "%'")
        SQLBldr.AppendLine(" )")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 駅名取得SQL(駅コード検索)
    ''' </summary>
    ''' <param name="prmStationCode">取引先コード</param>
    ''' <returns></returns>
    Public Shared Function GetStationNameSQL(ByVal prmCampCode As String, ByVal prmStationCode As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 取引先コード取得(取引先名称検索)
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("       STATION")
        SQLBldr.AppendLine("     , RTRIM(NAME)  AS NAME")
        SQLBldr.AppendLine("     , RTRIM(NAMES) AS NAMES")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     com.LNS0020_STATION")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     DELFLG = '0'")
        SQLBldr.AppendLine(" AND CAMPCODE = '" & prmCampCode & "'")
        SQLBldr.AppendLine(" AND STATION = '" & prmStationCode & "'")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     STATION")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 顧客(支払先)検索タイトル取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetClientTitle() As IEnumerable(Of DispFieldItem)

        Dim colTitle As IEnumerable(Of DispFieldItem)
        colTitle = {
                New DispFieldItem("TORICODE", "支払先コード", "110"),
                New DispFieldItem("CLIENTCODE", "顧客コード", "150"),
                New DispFieldItem("CLIENTNAME", "顧客名", "510")
            }

        Return colTitle

    End Function

    ''' <summary>
    ''' 顧客(支払先)取得SQL
    ''' </summary>
    ''' <param name="prmToriCode">外部コード</param>
    ''' <param name="prmClientCode">顧客コード</param>
    ''' <returns></returns>
    Public Shared Function GetClientSQL(ByVal prmToriCode As String,
                                        ByVal prmClientCode As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 顧客コード取得
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("       TORICODE + CLIENTCODE AS KEYCODE")
        SQLBldr.AppendLine("      ,TORICODE AS TORICODE")
        SQLBldr.AppendLine("      ,CLIENTCODE AS CLIENTCODE")
        SQLBldr.AppendLine("      ,RTRIM(CLIENTNAME) AS CLIENTNAME")
        SQLBldr.AppendLine("      ,RTRIM(TORINAME) AS TORINAME")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.LNT0072_PAYEE")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     DELFLG = '0'")
        '外部コードが入力されている場合条件に含める
        If Not prmToriCode = "" Then
            SQLBldr.AppendLine("  AND TORICODE = '" & prmToriCode & "'")
        End If
        '顧客コードが入力されている場合条件に含める
        If Not prmClientCode = "" Then
            SQLBldr.AppendLine("  AND CLIENTCODE = '" & prmClientCode & "'")
        End If
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     TORICODE")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 銀行コード検索タイトル取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetBankCodeTitle() As IEnumerable(Of DispFieldItem)

        Dim colTitle As IEnumerable(Of DispFieldItem)
        colTitle = {
                New DispFieldItem("BANKCODE", "銀行コード", "110"),
                New DispFieldItem("BANKNAME", "銀行名", "250"),
                New DispFieldItem("BANKNAMEKANA", "銀行名カナ", "250")
            }

        Return colTitle

    End Function

    ''' <summary>
    ''' 銀行コード取得SQL
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetBankCodeSQL(ByVal prmBankCode As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 銀行コード取得
        SQLBldr.AppendLine(" SELECT DISTINCT")
        SQLBldr.AppendLine("       BANKCODE AS KEYCODE")
        SQLBldr.AppendLine("      ,BANKCODE AS BANKCODE")
        SQLBldr.AppendLine("      ,RTRIM(BANKNAME) AS BANKNAME")
        SQLBldr.AppendLine("      ,RTRIM(BANKNAMEKANA) AS BANKNAMEKANA")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     com.LNS0022_BANK")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     DELFLG = '0'")
        '銀行コードが入力されている場合条件に含める
        'If Not prmBankCode = "" Then
        '    SQLBldr.AppendLine("     AND BANKCODE = '" & prmBankCode & "'")
        'End If
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     BANKCODE")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 銀行支店コード検索タイトル取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetBankBranchCodeTitle() As IEnumerable(Of DispFieldItem)

        Dim colTitle As IEnumerable(Of DispFieldItem)
        colTitle = {
                New DispFieldItem("BANKCODE", "銀行コード", "110"),
                New DispFieldItem("BANKNAME", "銀行名", "250"),
                New DispFieldItem("BANKBRANCHCODE", "支店コード", "110"),
                New DispFieldItem("BANKBRANCHNAME", "支店名", "180"),
                New DispFieldItem("BANKBRANCHNAMEKANA", "支店名カナ", "180")
            }

        Return colTitle

    End Function

    ''' <summary>
    ''' 銀行支店コード取得SQL
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetBankBranchCodeSQL(ByVal prmBankCode As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 銀行支店コード取得
        SQLBldr.AppendLine(" SELECT DISTINCT")
        SQLBldr.AppendLine("       BANKCODE + BANKBRANCHCODE + SORTCODE AS KEYCODE")
        SQLBldr.AppendLine("      ,BANKCODE AS BANKCODE")
        SQLBldr.AppendLine("      ,RTRIM(BANKNAME) AS BANKNAME")
        SQLBldr.AppendLine("      ,BANKBRANCHCODE AS BANKBRANCHCODE")
        SQLBldr.AppendLine("      ,RTRIM(BANKBRANCHNAME) AS BANKBRANCHNAME")
        SQLBldr.AppendLine("      ,RTRIM(BANKBRANCHNAMEKANA) AS BANKBRANCHNAMEKANA")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     com.LNS0022_BANK")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     DELFLG = '0'")
        SQLBldr.AppendLine("    AND BANKCODE = '" & prmBankCode & "'")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     BANKBRANCHCODE")

        Return SQLBldr.ToString

    End Function

    ''' <summary>
    ''' 社内口座検索タイトル取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetBankAccountTitle() As IEnumerable(Of DispFieldItem)

        Dim colTitle As IEnumerable(Of DispFieldItem)
        colTitle = {
                New DispFieldItem("BANKCODE", "銀行コード", "100"),
                New DispFieldItem("BANKBRANCHCODE", "支店コード", "100"),
                New DispFieldItem("BANKNAME", "銀行名", "150"),
                New DispFieldItem("BANKBRANCHNAME", "支店名", "150"),
                New DispFieldItem("BANKNAMEKANA", "銀行名カナ", "150"),
                New DispFieldItem("BANKBRANCHNAMEKANA", "支店名カナ", "150")
            }

        Return colTitle

    End Function

    ''' <summary>
    ''' 社内口座取得SQL
    ''' </summary>
    ''' <param name="prmBankCode">銀行コード</param>
    ''' <returns></returns>
    Public Shared Function GetBankAccountSQL(ByVal prmBankCode As String) As String

        Dim SQLBldr As New StringBuilder

        '-- 社内口座取得
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("       ROW_NUMBER() OVER(ORDER BY A.BANKCODE,A.BANKBRANCHCODE ASC) AS KEYCODE")
        SQLBldr.AppendLine("      ,A.BANKCODE AS BANKCODE")
        SQLBldr.AppendLine("      ,A.BANKBRANCHCODE AS BANKBRANCHCODE")
        SQLBldr.AppendLine("      ,RTRIM(B.BANKNAMEKANA) AS BANKNAMEKANA")
        SQLBldr.AppendLine("      ,RTRIM(B.BANKNAME) AS BANKNAME")
        SQLBldr.AppendLine("      ,RTRIM(B.BANKBRANCHNAMEKANA) AS BANKBRANCHNAMEKANA")
        SQLBldr.AppendLine("      ,RTRIM(B.BANKBRANCHNAME) AS BANKBRANCHNAME")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("    (")
        SQLBldr.AppendLine("       SELECT DISTINCT")
        SQLBldr.AppendLine("             BANKCODE")
        SQLBldr.AppendLine("            ,BANKBRANCHCODE")
        SQLBldr.AppendLine("       FROM")
        SQLBldr.AppendLine("           com.LNS0023_BANKACCOUNT")
        SQLBldr.AppendLine("       WHERE")
        SQLBldr.AppendLine("           DELFLG = '0'")
        '銀行コードが入力されている場合条件に含める
        If Not prmBankCode = "" Then
            SQLBldr.AppendLine("     AND BANKCODE = '" & prmBankCode & "'")
        End If
        SQLBldr.AppendLine("    ) A")
        SQLBldr.AppendLine(" LEFT JOIN ")
        SQLBldr.AppendLine("    com.LNS0022_BANK B ")
        SQLBldr.AppendLine("   ON A.BANKCODE = B.BANKCODE ")
        SQLBldr.AppendLine("   AND A.BANKBRANCHCODE = B.BANKBRANCHCODE ")
        SQLBldr.AppendLine("   AND B.DELFLG = '0' ")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     A.BANKCODE")
        SQLBldr.AppendLine("     ,A.BANKBRANCHCODE")

        Return SQLBldr.ToString

    End Function

End Class
