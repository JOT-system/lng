﻿Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0007WRKINC
    Inherits UserControl

    Public Const MAPIDL As String = "LNM0007L"       'MAPID(一覧)
    Public Const MAPIDLSK As String = "LNM0007LSK"   'MAPID(SK固定費一覧)
    Public Const MAPIDLTNG As String = "LNM0007LTNG"   'MAPID(TNG固定費一覧)
    Public Const MAPIDD As String = "LNM0007D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0007H"       'MAPID(履歴)
    Public Const MAPIDHSK As String = "LNM0007HSK"       'MAPID(SK履歴)
    Public Const MAPIDHTNG As String = "LNM0007HTNG"       'MAPID(TNG履歴)

    'タイトル区分
    Public Const TITLEKBNS As String = "C"   'タイトル区分

    Public Const MAX_ENDYMD As String = "2099/12/31"

    'テーブル名
    Public Const TBLKOTEIHI As String = "LNM0007_KOTEIHI"
    Public Const TBLSKKOTEIHI As String = "LNM0008_SKKOTEIHI"
    Public Const TBLTNGKOTEIHI As String = "LNM0009_TNGKOTEIHI"
    Public Const TBLKOTEIHIHIST As String = "LNT0006_KOTEIHIHIST"
    Public Const TBLSKKOTEIHIHIST As String = "LNT0007_SKKOTEIHIHIST"
    Public Const TBLTNGKOTEIHIHIST As String = "LNT0008_TNGKOTEIHIHIST"

    'リスト名(東北電力固定費)
    Public Const LISTNAMETOHOKU As String = "東北電力固定費"

    ''' <summary>
    ''' テーブル一覧
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum TableList
        固定費
        TNG固定費
        SK固定費
    End Enum

    ''' <summary>
    ''' ファイルタイプ
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum FILETYPE
        EXCEL
        PDF
    End Enum

    ''' <summary>
    ''' 入出力項目位置(固定費)
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
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        SYABAN   '車番
        RIKUBAN   '陸事番号
        SYAGATA   '車型
        SYAGATANAME   '車型名
        SYABARA   '車腹
        KOTEIHI   '固定費
        BIKOU1   '備考1
        BIKOU2   '備考2
        BIKOU3   '備考3
    End Enum

    ''' <summary>
    ''' 入出力項目位置(SK固定費)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOLSK
        DELFLG   '削除フラグ
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        SYABAN   '車番
        SYABARA   '車腹
        GETSUGAKU   '月額運賃
        GENGAKU   '減額対象額
        KOTEIHI   '固定費
        BIKOU   '備考
    End Enum

    ''' <summary>
    ''' 入出力項目位置(TNG固定費)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOLTNG
        DELFLG   '削除フラグ
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        SYABAN   '車番
        KOTEIHIM   '月額固定費
        KOTEIHID   '日額固定費
        KAISU   '使用回数
        KINGAKU   '金額
        BIKOU   '備考
    End Enum

    ''' <summary>
    ''' 変更履歴出力項目位置(固定費)
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
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        SYABAN   '車番
        RIKUBAN   '陸事番号
        SYAGATA   '車型
        SYAGATANAME   '車型名
        SYABARA   '車腹
        KOTEIHI   '固定費
        BIKOU1   '備考1
        BIKOU2   '備考2
        BIKOU3   '備考3
    End Enum

    ''' <summary>
    ''' 変更履歴出力項目位置(SK固定費)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOLSK
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
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        SYABAN   '車番
        SYABARA   '車腹
        GETSUGAKU   '月額運賃
        GENGAKU   '減額対象額
        KOTEIHI   '固定費
        BIKOU   '備考
    End Enum

    ''' <summary>
    ''' 変更履歴出力項目位置(TNG固定費)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOLTNG
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
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        SYABAN   '車番
        KOTEIHIM   '月額固定費
        KOTEIHID   '日額固定費
        KAISU   '使用回数
        KINGAKU   '金額
        BIKOU   '備考
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
    ''' ドロップダウンリスト荷主データ取得
    ''' </summary>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <returns></returns>
    Public Shared Function getDowpDownToriList(ByVal I_ORGCODE As String, ByVal I_TABLEID As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       TORICODE AS TORICODE                                                                          ")
        SQLStr.AppendLine("      ,TORINAME AS TORINAME                                                                          ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.VIW0002_KOTEIHI VIW0002                                                                     ")
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
        SQLStr.AppendLine("      ON  VIW0002.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("    VIW0002.TABLEID = @TABLEID                                                                       ")
        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     VIW0002.TORICODE                                                           ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ROLE", MySqlDbType.VarChar).Value = I_ORGCODE
                    .Add("@TABLEID", MySqlDbType.VarChar).Value = I_TABLEID
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
                    If WW_Tbl.Rows.Count > 1 Then
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
        SQLStr.AppendLine("     LNG.LNM0007_TANKA")

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
        SQLStr.AppendLine("     LNG.LNM0007_TANKA")

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

    ''' <summary>
    ''' 名称取得(届先名)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_NAMEht">届先名格納HT</param>
    Public Sub CODENAMEGetTODOKE(ByVal SQLcon As MySqlConnection,
                                   ByRef O_NAMEht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       TODOKECODE AS TODOKECODE")
        SQLStr.AppendLine("      ,RTRIM(TODOKENAME) AS TODOKENAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0007_TANKA")

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
                    If Not O_NAMEht.ContainsKey(WW_Row("TODOKECODE")) Then
                        O_NAMEht.Add(WW_Row("TODOKECODE"), WW_Row("TODOKENAME"))
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
    ''' 有効開始日取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Shared Function GetSTYMD(ByVal SQLcon As MySqlConnection, ByVal WW_CONTROLTABLE As String,
                                    ByVal WW_ROW As DataRow, ByRef O_MESSAGENO As String) As String

        GetSTYMD = ""

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("       DATE_FORMAT(MAX(STYMD), '%Y/%m/%d') AS STYMD ")
        SQLStr.AppendLine(" FROM")
        Select Case WW_CONTROLTABLE
            Case MAPIDL '固定費マスタ
                SQLStr.Append("     LNG.LNM0007_KOTEIHI             ")
            Case MAPIDLSK 'SK固定費マスタ
                SQLStr.Append("     LNG.LNM0008_SKKOTEIHI           ")
            Case MAPIDLTNG 'TNG固定費マスタ
                SQLStr.Append("     LNG.LNM0009_TNGKOTEIHI          ")
        End Select
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND SYABAN  = @SYABAN             ")
        SQLStr.AppendLine("   AND DELFLG  = '0'             ")
        'SQLStr.AppendLine("   AND  @STYMD BETWEEN STYMD AND ENDYMD      ")
        'SQLStr.AppendLine("   AND COALESCE(DATE_FORMAT(ENDYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@ENDYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20) '車番
                'Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date) '有効開始日
                'Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date) '有効終了日

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_SYABAN.Value = WW_ROW("SYABAN") '車番
                'P_STYMD.Value = CDate(WW_ROW("STYMD")) '有効開始日
                'P_ENDYMD.Value = CDate(MAX_ENDYMD) '有効終了日

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
            CS0011LOGWrite.INFPOSI = "DB:LNM0007 SELECT"
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
    ''' 排他チェック(固定費マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_STYMD">有効開始日</param>
    ''' <param name="I_SYABAN">車番</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String,
                          ByVal I_STYMD As String, ByVal I_SYABAN As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0007_KOTEIHI                     ")
        SQLStr.AppendLine(" WHERE                                       ")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND DATE_FORMAT(STYMD, '%Y/%m/%d') = DATE_FORMAT(@STYMD, '%Y/%m/%d')")
        SQLStr.AppendLine("   AND SYABAN  = @SYABAN                     ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20) '車番

                P_TORICODE.Value = I_TORICODE '取引先コード
                P_ORGCODE.Value = I_ORGCODE '部門コード
                P_STYMD.Value = CDate(I_STYMD) '有効開始日
                P_SYABAN.Value = I_SYABAN '車番

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0007Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0007Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0007Chk.Load(SQLdr)

                    If LNM0007Chk.Rows.Count > 0 Then
                        Dim LNM0007row As DataRow
                        LNM0007row = LNM0007Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0007row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0007row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0007D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 排他チェック(SK固定費マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_STYMD">有効開始日</param>
    ''' <param name="I_SYABAN">車番</param>
    Public Sub HaitaCheckSK(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String,
                          ByVal I_STYMD As String, ByVal I_SYABAN As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0008_SKKOTEIHI					")
        SQLStr.AppendLine(" WHERE                                       ")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND DATE_FORMAT(STYMD, '%Y/%m/%d') = DATE_FORMAT(@STYMD, '%Y/%m/%d')")
        SQLStr.AppendLine("   AND SYABAN  = @SYABAN                     ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20) '車番

                P_TORICODE.Value = I_TORICODE '取引先コード
                P_ORGCODE.Value = I_ORGCODE '部門コード
                P_STYMD.Value = CDate(I_STYMD) '有効開始日
                P_SYABAN.Value = I_SYABAN '車番

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0007Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0007Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0007Chk.Load(SQLdr)

                    If LNM0007Chk.Rows.Count > 0 Then
                        Dim LNM0007row As DataRow
                        LNM0007row = LNM0007Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0007row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0007row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0007D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 排他チェック(TNG固定費マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_STYMD">有効開始日</param>
    ''' <param name="I_SYABAN">車番</param>
    Public Sub HaitaCheckTNG(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String,
                          ByVal I_STYMD As String, ByVal I_SYABAN As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0009_TNGKOTEIHI					")
        SQLStr.AppendLine(" WHERE                                       ")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND DATE_FORMAT(STYMD, '%Y/%m/%d') = DATE_FORMAT(@STYMD, '%Y/%m/%d')")
        SQLStr.AppendLine("   AND SYABAN  = @SYABAN                     ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20) '車番

                P_TORICODE.Value = I_TORICODE '取引先コード
                P_ORGCODE.Value = I_ORGCODE '部門コード
                P_STYMD.Value = CDate(I_STYMD) '有効開始日
                P_SYABAN.Value = I_SYABAN '車番

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0007Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0007Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0007Chk.Load(SQLdr)

                    If LNM0007Chk.Rows.Count > 0 Then
                        Dim LNM0007row As DataRow
                        LNM0007row = LNM0007Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0007row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0007row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0007D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

End Class