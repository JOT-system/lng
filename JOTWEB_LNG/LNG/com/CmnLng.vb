Option Strict On

Imports MySql.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' コンテナ用 共通クラス
''' </summary>
''' <remarks>各種受注データテーブルに登録する際はこちらに定義</remarks>
Public Class CmnLng
    Inherits System.Web.UI.Page

    ''' <summary>
    ''' 固定値マスタデータ取得
    ''' </summary>
    ''' <param name="prmCampCode">会社コード</param>
    ''' <param name="prmClassKey">固定値マスタのCLASS(分類)</param>
    ''' <returns></returns>
    Public Shared Function GetFixValueTbl(ByVal prmCampCode As String, ByVal prmClassKey As String) As DataTable

        Dim dt = New DataTable
        Dim CS0050Session As New CS0050SESSION

        Using SQLcon As New MySqlConnection(CS0050Session.DBCon)
            SQLcon.Open()       'DataBase接続

            Dim sqlBldr As New StringBuilder
            sqlBldr.AppendLine(" SELECT")
            sqlBldr.AppendLine("     FIX.KEYCODE AS [key]")
            sqlBldr.AppendLine("     , RTRIM(FIX.VALUE1) AS [value]")
            sqlBldr.AppendLine(" FROM")
            sqlBldr.AppendLine("     com.LNS0006_FIXVALUE FIX")
            sqlBldr.AppendLine(" WHERE")
            sqlBldr.AppendLine("     FIX.CAMPCODE = @P01")
            sqlBldr.AppendLine(" AND FIX.CLASS = @P02")
            sqlBldr.AppendLine(" AND DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN FIX.STYMD AND FIX.ENDYMD")
            sqlBldr.AppendLine(" AND FIX.DELFLG = @P03")
            sqlBldr.AppendLine(" ORDER BY")
            sqlBldr.AppendLine("     CONVERT(INT, FIX.KEYCODE)")

            Try
                Using SQLcmd As New MySqlCommand(sqlBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 20)
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 20)
                    Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 1)

                    'パラメータ設定
                    PARA01.Value = prmCampCode
                    PARA02.Value = prmClassKey
                    PARA03.Value = C_DELETE_FLG.ALIVE

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                End Using

            Catch ex As Exception
                Throw ex '呼び出し元の例外にスロー
            End Try

        End Using

        Return dt

    End Function

    ''' <summary>
    ''' 固定値マスタデータ取得
    ''' </summary>
    ''' <param name="prmCampCode">会社コード</param>
    ''' <param name="prmClassKey">固定値マスタのCLASS(分類)</param>
    ''' <returns></returns>
    Public Shared Function GetFixValueTbl(ByVal prmCampCode As String, ByVal prmClassKey As String, ByVal prmKeyValueNum As String) As DataTable

        Dim dt = New DataTable
        Dim CS0050Session As New CS0050SESSION

        Using SQLcon As New MySqlConnection(CS0050Session.DBCon)
            SQLcon.Open()       'DataBase接続

            Dim sqlBldr As New StringBuilder
            sqlBldr.AppendLine(" SELECT")
            sqlBldr.AppendLine("     FIX.VALUE" & prmKeyValueNum & " AS [key]")
            sqlBldr.AppendLine("    , RTrim(Fix.VALUE1) As [value]")
            sqlBldr.AppendLine(" FROM")
            sqlBldr.AppendLine("     com.LNS0006_FIXVALUE FIX")
            sqlBldr.AppendLine(" WHERE")
            sqlBldr.AppendLine("     FIX.CAMPCODE = @P01")
            sqlBldr.AppendLine(" And FIX.Class = @P02")
            sqlBldr.AppendLine(" And DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN FIX.STYMD AND FIX.ENDYMD")
            sqlBldr.AppendLine(" AND FIX.DELFLG = @P03")
            sqlBldr.AppendLine(" ORDER BY")
            sqlBldr.AppendLine("     CONVERT(INT, FIX.KEYCODE)")

            Try
                Using SQLcmd As New MySqlCommand(sqlBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 20)
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 20)
                    Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 1)

                    'パラメータ設定
                    PARA01.Value = prmCampCode
                    PARA02.Value = prmClassKey
                    PARA03.Value = C_DELETE_FLG.ALIVE

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                End Using

            Catch ex As Exception
                Throw ex '呼び出し元の例外にスロー
            End Try

        End Using

        Return dt

    End Function

    ''' <summary>
    ''' ドロップダウンリスト(固定値マスタ用) データ取得
    ''' </summary>
    ''' <param name="prmCampCode">会社コード</param>
    ''' <param name="prmClassKey">固定値マスタのCLASS(分類)</param>
    ''' <param name="blnBlank">空白追加フラグ</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownFixedList(ByVal prmCampCode As String, ByVal prmClassKey As String, Optional ByVal blnBlank As Boolean = False) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT")
        sqlStat.AppendLine("       FIX.KEYCODE as CODE")
        sqlStat.AppendLine("      ,FIX.VALUE1 as NAME")
        sqlStat.AppendLine("  FROM COM.LNS0006_FIXVALUE as FIX")
        sqlStat.AppendLine(" WHERE")
        sqlStat.AppendLine("     FIX.CAMPCODE = @CAMPCODE")
        sqlStat.AppendLine("     AND FIX.CLASS  = @CLASS")
        sqlStat.AppendLine("     AND FIX.DELFLG = @DELFLG")
        sqlStat.AppendLine("     AND DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN FIX.STYMD AND FIX.ENDYMD")
        sqlStat.AppendLine(" ORDER BY CONVERT(DECIMAL, (CASE coalesce(FIX.KEYCODE, '') WHEN '' THEN '0' ELSE FIX.KEYCODE END))")

        Try
            '空白行判定
            If blnBlank = True Then
                Dim listBlankItm As New ListItem("", "")
                retList.Items.Add(listBlankItm)
            End If

            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@CAMPCODE", MySqlDbType.VarChar).Value = prmCampCode
                    .Add("@CLASS", MySqlDbType.VarChar).Value = prmClassKey
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With
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
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' ドロップダウンリスト(組織コード用) データ取得
    ''' </summary>
    ''' <param name="blnBlank">空白追加フラグ</param>
    ''' <returns></returns>
    Public Shared Function getDropDownOrgCdList(Optional ByVal blnBlank As Boolean = False, Optional ByVal blnCtnFlg As Boolean = False) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT")
        sqlStat.AppendLine("       ORGCODE as CODE")
        sqlStat.AppendLine("      ,NAME as NAME")
        sqlStat.AppendLine("  FROM COM.LNS0019_ORG with(nolock)")
        sqlStat.AppendLine(" WHERE")
        sqlStat.AppendLine("     DELFLG = @DELFLG")
        sqlStat.AppendLine(" AND DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN STYMD AND ENDYMD")
        sqlStat.AppendLine(" AND CLASS01  IN(1,2,4)")
        If blnCtnFlg = True Then
            sqlStat.AppendLine("    AND CTNFLG = 1")
        End If
        sqlStat.AppendLine(" ORDER BY ORGCODE")

        Try
            '空白行判定
            If blnBlank = True Then
                Dim listBlankItm As New ListItem("", "")
                retList.Items.Add(listBlankItm)
            End If

            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = BaseDllConst.C_DELETE_FLG.ALIVE
                End With
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

        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' 支店選択用コンボボックス作成(三大帳票専用)
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function getCmbOfficeList() As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        '○ 『全体表示』を追加
        retList.Items.Add(New ListItem("★全支店", "ALL"))

        sqlStat.AppendLine(" SELECT                                                       ")
        sqlStat.AppendLine("     ORGCODE                                          as CODE ")
        sqlStat.AppendLine("    ,CASE                                                     ")
        sqlStat.AppendLine("         WHEN ORGCODE  = CONTROLCODE THEN NAME                ")
        sqlStat.AppendLine("         WHEN ORGCODE <> CONTROLCODE THEN '　' + NAME         ")
        sqlStat.AppendLine("     END                                              as NAME ")
        sqlStat.AppendLine(" FROM                                                         ")
        sqlStat.AppendLine("     com.LNS0019_ORG WITH(nolock)                             ")
        sqlStat.AppendLine(" WHERE                                                        ")
        sqlStat.AppendLine("     ORGSELECTFLAG = 1                                        ")
        sqlStat.AppendLine(" AND DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN STYMD AND ENDYMD  ")
        sqlStat.AppendLine(" AND DELFLG = @DELFLG                                         ")
        sqlStat.AppendLine(" ORDER BY                                                     ")
        sqlStat.AppendLine("     CONTROLCODE                                              ")
        sqlStat.AppendLine("    ,CLASS01                                                  ")
        sqlStat.AppendLine("    ,ORGCODE                                                  ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With
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
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' リース支店選択用コンボボックス作成
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function getCmbLeaseOfficeList() As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        '○ 『全体表示』を追加
        retList.Items.Add(New ListItem("全支店", "ALL"))

        sqlStat.AppendLine("SELECT                                                          ")
        sqlStat.AppendLine("    CODE                                                        ")
        sqlStat.AppendLine("    , NAME                                                      ")
        sqlStat.AppendLine("FROM (                                                          ")
        sqlStat.AppendLine("    SELECT                                                      ")
        sqlStat.AppendLine("        ORGCODE AS CODE                                         ")
        sqlStat.AppendLine("        , RTRIM(NAME) AS NAME                                   ")
        sqlStat.AppendLine("        , 1 AS SEQ                                              ")
        sqlStat.AppendLine("    FROM                                                        ")
        sqlStat.AppendLine("        com.LNS0019_ORG                                       ")
        sqlStat.AppendLine("    WHERE                                                       ")
        sqlStat.AppendLine("        CTNFLG = '1'                                            ")
        sqlStat.AppendLine("    AND CLASS01 = 4                                             ")
        sqlStat.AppendLine("    AND DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN STYMD AND ENDYMD ")
        sqlStat.AppendLine("    UNION ALL                                                   ")
        sqlStat.AppendLine("    SELECT                                                      ")
        sqlStat.AppendLine("        ORGCODE AS CODE                                         ")
        sqlStat.AppendLine("        , RTRIM(NAME) AS NAME                                   ")
        sqlStat.AppendLine("        , 2 AS SEQ                                              ")
        sqlStat.AppendLine("    FROM                                                        ")
        sqlStat.AppendLine("        com.LNS0019_ORG                                       ")
        sqlStat.AppendLine("    WHERE                                                       ")
        sqlStat.AppendLine("        CTNFLG = '1'                                            ")
        sqlStat.AppendLine("    AND CLASS01 = 1                                             ")
        sqlStat.AppendLine("    AND DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN STYMD AND ENDYMD ")
        sqlStat.AppendLine(") AS T1                                                         ")
        sqlStat.AppendLine("ORDER BY                                                        ")
        sqlStat.AppendLine("     SEQ                                                        ")
        sqlStat.AppendLine("     , CODE                                                     ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With
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
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' エリア選択用コンボボックス作成(三大帳票専用)
    ''' </summary>
    ''' <param name="officeCode">支店コード</param>
    ''' <returns></returns>
    Public Shared Function getCmbAreaList(officeCode As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        '○ 『全体表示』を追加
        retList.Items.Add(New ListItem("★全エリア", "ALL"))

        If officeCode = "ALL" Then
            Return retList
        End If

        sqlStat.AppendLine("SELECT                                                             ")
        sqlStat.AppendLine("       A.CLASS01 as CODE                                           ")
        sqlStat.AppendLine("      ,B.VALUE1 as NAME                                            ")
        sqlStat.AppendLine("  FROM COM.LNS0020_STATION as A with(nolock)                       ")
        sqlStat.AppendLine("  LEFT JOIN COM.LNS0006_FIXVALUE as B with(nolock)                 ")
        sqlStat.AppendLine("    ON B.CAMPCODE = A.CAMPCODE                                     ")
        sqlStat.AppendLine("   AND B.CLASS    = 'PREFECTURE'                                   ")
        sqlStat.AppendLine("   AND B.KEYCODE  = CONVERT(nvarchar,A.CLASS01)                    ")
        sqlStat.AppendLine("   AND B.DELFLG   = @DELFLG                                        ")
        sqlStat.AppendLine(" WHERE A.ORGCODE  = @OFFICECODE                                    ")
        sqlStat.AppendLine("   AND DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN B.STYMD AND B.ENDYMD ")
        sqlStat.AppendLine("   AND A.DELFLG   = @DELFLG                                        ")
        sqlStat.AppendLine(" GROUP BY B.VALUE1,A.CLASS01                                       ")
        sqlStat.AppendLine(" ORDER BY A.CLASS01                                                ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@OFFICECODE", MySqlDbType.VarChar).Value = officeCode
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With
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

        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' 駅選択用コンボボックス作成(三大帳票専用)
    ''' </summary>
    ''' <param name="officeCode">支店</param>
    ''' <returns></returns>
    Public Shared Function getCmbStationList(ByVal officeCode As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        If officeCode = "ALL" Then
            Return retList
        End If

        If officeCode = "ALL" Then
            sqlStat.AppendLine("     SELECT                                  ")
            sqlStat.AppendLine("         STATION as hensuu                   ")
            sqlStat.AppendLine("     FROM                                    ")
            sqlStat.AppendLine("          com.LNS0020_STATION WITH(nolock)   ")
            sqlStat.AppendLine("     WHERE                                   ")
            sqlStat.AppendLine("         STATIONSELECTFLAG = '1'             ")
            sqlStat.AppendLine("     AND DELFLG            = @DELFLG         ")
        Else
            sqlStat.AppendLine(" DECLARE @hensuu AS bigint ;                 ")
            sqlStat.AppendLine("     SET @hensuu = 0 ;                       ")
            sqlStat.AppendLine(" DECLARE hensuu CURSOR FOR                   ")
            sqlStat.AppendLine("     SELECT                                  ")
            sqlStat.AppendLine("         STATION as hensuu                   ")
            sqlStat.AppendLine("     FROM                                    ")
            sqlStat.AppendLine("          com.LNS0020_STATION WITH(nolock)   ")
            sqlStat.AppendLine("     WHERE                                   ")
            sqlStat.AppendLine("         ORGCODE           = @OFFICECODE     ")
            sqlStat.AppendLine("     AND STATIONSELECTFLAG = '1'             ")
            sqlStat.AppendLine("     AND DELFLG            = @DELFLG         ")
            sqlStat.AppendLine(" OPEN hensuu ;                               ")
            sqlStat.AppendLine(" FETCH NEXT FROM hensuu INTO @hensuu ;       ")
            sqlStat.AppendLine(" IF (@@FETCH_STATUS = 0)                     ")
            sqlStat.AppendLine("     SELECT                                  ")
            sqlStat.AppendLine("         STATION as CODE                     ")
            sqlStat.AppendLine("        ,NAMES   as NAME                     ")
            sqlStat.AppendLine("     FROM                                    ")
            sqlStat.AppendLine("          com.LNS0020_STATION WITH(nolock)   ")
            sqlStat.AppendLine("     WHERE                                   ")
            sqlStat.AppendLine("         ORGCODE           = @OFFICECODE     ")
            sqlStat.AppendLine("     AND STATIONSELECTFLAG = '1'             ")
            sqlStat.AppendLine("     AND DELFLG            = @DELFLG         ")
            sqlStat.AppendLine("     ORDER BY                                ")
            sqlStat.AppendLine("         STATION                             ")
            sqlStat.AppendLine(" IF (@@FETCH_STATUS <> 0)                    ")
            sqlStat.AppendLine("     SELECT                                  ")
            sqlStat.AppendLine("         STATION as CODE                     ")
            sqlStat.AppendLine("        ,NAMES   as NAME                     ")
            sqlStat.AppendLine("     FROM                                    ")
            sqlStat.AppendLine("          com.LNS0020_STATION WITH(nolock)   ")
            sqlStat.AppendLine("     WHERE                                   ")
            sqlStat.AppendLine("         GOVERNORGCODE     = @OFFICECODE     ")
            sqlStat.AppendLine("     AND STATIONSELECTFLAG = '1'             ")
            sqlStat.AppendLine("     AND DELFLG            = @DELFLG         ")
            sqlStat.AppendLine("     ORDER BY                                ")
            sqlStat.AppendLine("         STATION                             ")
            sqlStat.AppendLine(" CLOSE hensuu ;                              ")
            sqlStat.AppendLine(" DEALLOCATE hensuu ;                         ")
        End If

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@OFFICECODE", MySqlDbType.VarChar).Value = officeCode
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With
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

        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' コンボボックス インデックス取得
    ''' </summary>
    ''' <param name="ddlTargetList">コンボボックス</param>
    ''' <param name="strUpdPeriodFirst">値</param>
    ''' <returns></returns>
    Public Shared Function getCmbDataListIndex(ByRef ddlTargetList As DropDownList, ByVal strUpdPeriodFirst As String) As Integer

        Dim intRetIndex As Integer = 0
        Dim intIndex As Integer = 0
        Dim blnOnFlg As Boolean = False

        ' アイテムをループで検索し、条件が一致した場合、選択状態にする
        For Each lstItem As ListItem In ddlTargetList.Items
            '値が一致するかの判定
            If (lstItem.Value.Equals(strUpdPeriodFirst)) Then
                intRetIndex = intIndex
                blnOnFlg = True
                Exit For
            End If

            intIndex += 1
        Next

        If blnOnFlg = False Then
            intIndex = -1
        End If

        Return intIndex

    End Function

    ''' <summary>
    ''' 月末日取得処理
    ''' </summary>
    ''' <param name="dtTarget">対象日付</param>
    ''' <returns>月末日</returns>
    Public Shared Function GetEndDate(ByVal dtTarget As DateTime) As DateTime
        '月初の翌月の1日前を返す
        Return dtTarget.AddDays(-(dtTarget.Day - 1)).AddMonths(1).AddDays(-1)
    End Function

    ''' <summary>
    ''' 締め日取得処理
    ''' </summary>
    ''' <param name="prmServiceID">業務ID</param>
    ''' <param name="prmInvoiceOrgCode">請求担当部店コード</param>
    ''' <param name="prmKeijoYYYYMM">計上年月</param>
    ''' <returns></returns>
    Public Shared Function GetClosingDayTbl(ByVal prmServiceID As String,
                                            ByVal prmInvoiceOrgCode As String, ByVal prmKeijoYYYYMM As String) As String

        Dim dt = New DataTable
        Dim strClosingDay As String = ""
        Dim CS0050Session As New CS0050SESSION

        Using SQLcon As New MySqlConnection(CS0050Session.DBCon)
            SQLcon.Open()       'DataBase接続

            Dim sqlBldr As New StringBuilder
            sqlBldr.AppendLine(" SELECT")
            sqlBldr.AppendLine("     CLOSINGDAY.CLOSINGDAY AS [CLOSINGDAY]")
            sqlBldr.AppendLine(" FROM")
            sqlBldr.AppendLine("     lng.LNT0076_CLOSINGDAY CLOSINGDAY")
            sqlBldr.AppendLine(" WHERE")
            sqlBldr.AppendLine("     CLOSINGDAY.SERVICEID = @P01")
            sqlBldr.AppendLine(" AND CLOSINGDAY.INVOICEORGCODE = @P02")
            sqlBldr.AppendLine(" AND CLOSINGDAY.KEIJOYM = @P03")
            sqlBldr.AppendLine(" AND CLOSINGDAY.DELFLG = @P04")

            Try
                Using SQLcmd As New MySqlCommand(sqlBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 20)
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 6)
                    Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.Int32, 10)
                    Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 1)

                    'パラメータ設定
                    PARA01.Value = prmServiceID
                    PARA02.Value = prmInvoiceOrgCode
                    PARA03.Value = prmKeijoYYYYMM
                    PARA04.Value = C_DELETE_FLG.ALIVE

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                End Using

            Catch ex As Exception
                Throw ex '呼び出し元の例外にスロー
            End Try

        End Using

        If dt.Rows.Count > 0 Then
            strClosingDay = CmnSetFmt.YYYYMMDDToStr(dt.Rows(0)("CLOSINGDAY"))
        End If

        Return strClosingDay

    End Function

#Region "コード、名称入力変換処理"

    ''' <summary>
    ''' コード、名称入力変換処理
    ''' </summary>
    ''' <param name="FildID">項目ID</param>
    ''' <param name="SetData">画面に入力した値</param>
    ''' <returns></returns>
    Public Shared Function GetCodeChange(ByVal FildID As String, ByVal SetData As String) As String

        Dim ReturnCode As String = ""
        Select Case FildID
            '取引先
            Case "ToriCode"
                '取引先名称存在チェック
                GetToriCode(SetData, ReturnCode)
                'データ無しでも、取引先コードに該当する数値が入力された場合、コードで再検索
                If String.IsNullOrEmpty(ReturnCode) AndAlso
                            IsNumeric(StrConv(SetData, VbStrConv.Narrow)) Then
                    GetToriCode_ByCode(StrConv(SetData, VbStrConv.Narrow), ReturnCode)
                End If
            '発駅・着駅
            Case "StationCode"
                '駅名称存在チェック
                GetStationCode(SetData, ReturnCode)
                'データ無しでも、駅コードに該当する数値が入力された場合、コードで再検索
                If String.IsNullOrEmpty(ReturnCode) AndAlso
                    IsNumeric(StrConv(SetData, VbStrConv.Narrow)) Then
                    GetStationCode_ByCode(StrConv(SetData, VbStrConv.Narrow), ReturnCode)
                End If

        End Select

        Return ReturnCode

    End Function

#Region "テーブル読込処理"

    ''' <summary>
    ''' 取引先コード取得(名称検索)
    ''' </summary>
    Public Shared Sub GetToriCode(ByVal SetTori As String, ByRef ToriCode As String)
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT")
        sqlStat.AppendLine("       coalesce(MAX(TORICODE),'') AS TORICODE")
        sqlStat.AppendLine("FROM   LNG.LNM0024_KEKKJM")
        sqlStat.AppendLine("WHERE  RTRIM(TORINAME + coalesce(TORIDIVNAME,'')) = @NAME")
        sqlStat.AppendLine("AND    DELFLG = @DELFLG")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@NAME", MySqlDbType.VarChar).Value = SetTori
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With

                Dim dt = New DataTable
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        dt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(sqlDr)
                End Using

                If dt.Rows.Count > 0 AndAlso Not String.IsNullOrEmpty(CType(dt.Rows(0)("TORICODE"), String)) Then
                    ToriCode = CType(dt.Rows(0)("TORICODE"), String)
                End If

            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 取引先コード取得(コード検索)
    ''' </summary>
    Public Shared Sub GetToriCode_ByCode(ByVal SetTori As String, ByRef ToriCode As String)
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT")
        sqlStat.AppendLine("       coalesce(MAX(TORICODE),'') AS TORICODE")
        sqlStat.AppendLine("FROM   LNG.LNM0024_KEKKJM")
        sqlStat.AppendLine("WHERE  RTRIM(TORICODE) = @TORICODE")
        sqlStat.AppendLine("AND    DELFLG = @DELFLG")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@TORICODE", MySqlDbType.VarChar).Value = SetTori
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With

                Dim dt = New DataTable
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        dt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(sqlDr)
                End Using

                If dt.Rows.Count > 0 AndAlso Not String.IsNullOrEmpty(CType(dt.Rows(0)("TORICODE"), String)) Then
                    ToriCode = CType(dt.Rows(0)("TORICODE"), String)
                End If

            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub


    ''' <summary>
    ''' 駅コード取得(名称検索)
    ''' </summary>
    Public Shared Sub GetStationCode(ByVal SetStation As String, ByRef StationCode As String)
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT")
        sqlStat.AppendLine("       STATION")
        sqlStat.AppendLine("  FROM com.LNS0020_STATION")
        sqlStat.AppendLine(" WHERE NAME   = @NAME")
        sqlStat.AppendLine("   AND DELFLG = @DELFLG")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@NAME", MySqlDbType.VarChar).Value = SetStation
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With

                Dim dt = New DataTable
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        dt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(sqlDr)
                End Using

                If dt.Rows.Count > 0 Then
                    StationCode = CType(dt.Rows(0)("STATION"), String)
                End If

            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 駅コード取得(コード検索)
    ''' </summary>
    Public Shared Sub GetStationCode_ByCode(ByVal SetStation As String, ByRef StationCode As String)
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT")
        sqlStat.AppendLine("       STATION")
        sqlStat.AppendLine("  FROM com.LNS0020_STATION")
        sqlStat.AppendLine(" WHERE STATION = @STATION")
        sqlStat.AppendLine("   AND DELFLG  = @DELFLG")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@STATION", MySqlDbType.VarChar).Value = SetStation
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With

                Dim dt = New DataTable
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        dt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(sqlDr)
                End Using

                If dt.Rows.Count > 0 Then
                    StationCode = CType(dt.Rows(0)("STATION"), String)
                End If

            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' ドロップダウンリスト(固定値マスタ用) 状態データ取得(個別用)
    ''' </summary>
    ''' <param name="prmCampCode">会社コード</param>
    ''' <param name="prmClassKey">固定値マスタのCLASS(分類)</param>
    ''' <param name="blnBlank">空白追加フラグ</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownAccountStatusKbnList(ByVal prmCampCode As String, ByVal prmClassKey As String,
                                              Optional ByVal blnBlank As Boolean = False,
                                              Optional ByVal blnNewFlg As Boolean = False) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT")
        sqlStat.AppendLine("       FIX.KEYCODE as CODE")
        sqlStat.AppendLine("      ,FIX.VALUE1 as NAME")
        sqlStat.AppendLine("  FROM COM.LNS0006_FIXVALUE as FIX")
        sqlStat.AppendLine(" WHERE")
        sqlStat.AppendLine("     FIX.CAMPCODE = @CAMPCODE")
        sqlStat.AppendLine("     AND FIX.CLASS  = @CLASS")
        sqlStat.AppendLine("     AND FIX.DELFLG = @DELFLG")
        sqlStat.AppendLine("     AND DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN FIX.STYMD AND FIX.ENDYMD")
        If blnNewFlg = True Then
            sqlStat.AppendLine("     AND FIX.VALUE3 = '1'")
        End If
        sqlStat.AppendLine(" ORDER BY CONVERT(INT, FIX.VALUE4)")

        Try
            '空白行判定
            If blnBlank = True Then
                Dim listBlankItm As New ListItem("", "")
                retList.Items.Add(listBlankItm)
            End If

            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@CAMPCODE", MySqlDbType.VarChar).Value = prmCampCode
                    .Add("@CLASS", MySqlDbType.VarChar).Value = prmClassKey
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With
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
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' ドロップダウンリスト(固定値マスタ用) ソート用
    ''' </summary>
    ''' <param name="prmCampCode">会社コード</param>
    ''' <param name="prmClassKey">固定値マスタのCLASS(分類)</param>
    ''' <param name="blnBlank">空白追加フラグ</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownKbnSortList(ByVal prmCampCode As String, ByVal prmClassKey As String,
                                              Optional ByVal blnBlank As Boolean = False) As DropDownList

        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT")
        sqlStat.AppendLine("       FIX.KEYCODE as CODE")
        sqlStat.AppendLine("      ,FIX.VALUE1 as NAME")
        sqlStat.AppendLine("  FROM COM.LNS0006_FIXVALUE as FIX")
        sqlStat.AppendLine(" WHERE")
        sqlStat.AppendLine("     FIX.CAMPCODE = @CAMPCODE")
        sqlStat.AppendLine("     AND FIX.CLASS  = @CLASS")
        sqlStat.AppendLine("     AND FIX.DELFLG = @DELFLG")
        sqlStat.AppendLine("     AND DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN FIX.STYMD AND FIX.ENDYMD")
        sqlStat.AppendLine("     AND coalesce(FIX.VALUE5, '') <> ''")
        sqlStat.AppendLine(" ORDER BY CONVERT(INT, FIX.VALUE5)")

        Try
            '空白行判定
            If blnBlank = True Then
                Dim listBlankItm As New ListItem("", "")
                retList.Items.Add(listBlankItm)
            End If

            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@CAMPCODE", MySqlDbType.VarChar).Value = prmCampCode
                    .Add("@CLASS", MySqlDbType.VarChar).Value = prmClassKey
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With
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
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

    ''' <summary>
    ''' ドロップダウンリスト コピー処理
    ''' (ドロップダウンリストの中身をそのままドロップダウンリストにセットする)
    ''' </summary>
    ''' <param name="ddlTargetList">空白追加フラグ</param>
    ''' <returns></returns>
    Public Shared Function getDropDownCopy(ByVal ddlTargetList As DropDownList, Optional ByVal blnBlank As Boolean = False) As DropDownList
        Dim retList As New DropDownList

        Try
            '空白行判定
            If blnBlank = True Then
                Dim listBlankItm As New ListItem("", "")
                retList.Items.Add(listBlankItm)
            End If

            For Each listItm As ListItem In ddlTargetList.Items
                Dim listItmtarget As New ListItem(listItm.Text, listItm.Value)
                retList.Items.Add(listItmtarget)
            Next

        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

        Return retList

    End Function

#End Region


#End Region


End Class
