Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0010WRKINC
    Inherits UserControl

    Public Const MAPIDL As String = "LNM0010L"       'MAPID(一覧)
    Public Const MAPIDLHA As String = "LNM0010LHA"   'MAPID(八戸特別料金一覧)
    Public Const MAPIDLEN As String = "LNM0010LEN"   'MAPID(ENEOS業務委託料一覧)
    Public Const MAPIDLTO As String = "LNM0010LTO"   'MAPID(東北電力車両別追加料金一覧)
    Public Const MAPIDLKG As String = "LNM0010LKG"   'MAPID(北海道ガス特別料金一覧)
    Public Const MAPIDLSKSP As String = "LNM0010LSKSP"   'MAPID(SK特別料金一覧)
    Public Const MAPIDLSKSU As String = "LNM0010LSKSU"   'MAPID(SK燃料サーチャージ一覧)

    Public Const MAPIDD As String = "LNM0010D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0010H"       'MAPID(履歴)
    Public Const MAPIDHHA As String = "LNM0010HHA"   'MAPID(八戸特別料金履歴)
    Public Const MAPIDHEN As String = "LNM0010HEN"   'MAPID(ENEOS業務委託料履歴)
    Public Const MAPIDHTO As String = "LNM0010HTO"   'MAPID(東北電力車両別追加料金履歴)
    Public Const MAPIDHKG As String = "LNM0010HKG"   'MAPID(北海道ガス特別料金履歴)
    Public Const MAPIDHSKSP As String = "LNM0010HSKSP"   'MAPID(SK特別料金履歴)
    Public Const MAPIDHSKSU As String = "LNM0010HSKSU"   'MAPID(SK燃料サーチャージ履歴)

    'タイトル区分
    Public Const TITLEKBNS As String = "C"   'タイトル区分

    Public Const MAX_ENDYMD As String = "2099/12/31"

    'テーブル名
    Public Const TBLHACHINOHESPRATE As String = "LNM0010_HACHINOHESPRATE"
    Public Const TBLENEOSCOMFEE As String = "LNM0011_ENEOSCOMFEE"
    Public Const TBLTOHOKUSPRATE As String = "LNM0012_TOHOKUSPRATE"
    Public Const TBLKGSPRATE As String = "LNM0013_KGSPRATE"
    Public Const TBLSKSPRATE As String = "LNM0014_SKSPRATE"
    Public Const TBLSKSURCHARGE As String = "LNM0015_SKSURCHARGE"

    Public Const TBLHACHINOHESPRATEHIST As String = "LNT0009_HACHINOHESPRATEHIST"
    Public Const TBLENEOSCOMFEEHIST As String = "LNT0010_ENEOSCOMFEEHIST"
    Public Const TBLTOHOKUSPRATEHIST As String = "LNT0011_TOHOKUSPRATEHIST"
    Public Const TBLKGSPRATEHIST As String = "LNT0012_KGSPRATEHIST"
    Public Const TBLSKSPRATEHIST As String = "LNT0013_SKSPRATEHIST"
    Public Const TBLSKSURCHARGEHIST As String = "LNT0014_SKSURCHARGEHIST"


    '組織コード
    Public Const ORGISHIKARI As String = "020104" 'EX石狩営業所
    Public Const ORGISHIKARINAME As String = "EX石狩営業所" 'EX石狩営業所
    '加算先部門コード
    Public Const KASANORGHOKKAIDO As String = "020101" 'EX 北海道支店
    Public Const KASANORGHOKKAIDONAME As String = "EX 北海道支店" 'EX 北海道支店

    'Public Const TBLKOTEIHIHIST As String = "LNT0006_KOTEIHIHIST"
    'Public Const TBLSKKOTEIHIHIST As String = "LNT0007_SKKOTEIHIHIST"
    'Public Const TBLTNGKOTEIHIHIST As String = "LNT0008_TNGKOTEIHIHIST"

    'ボタン名
    'Public Const BTNNAMETOHOKU As String = "東北電力固定費"

    Public Enum TableList
        八戸特別料金
        ENEOS業務委託料
        東北電力車両別追加料金
        北海道ガス特別料金
        SK特別料金
        SK燃料サーチャージ
    End Enum


    ''' <summary>
    ''' ファイルタイプ
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum FILETYPE
        EXCEL
        PDF
    End Enum

#Region "一覧画面DLUL入出力項目位置設定"
    ''' <summary>
    ''' 入出力項目位置(八戸特別料金)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOLHA
        DELFLG   '削除フラグ
        RECOID   'レコードID
        RECONAME   'レコード名
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        KINGAKU   '金額
    End Enum
    ''' <summary>
    ''' 入出力項目位置(ENEOS業務委託料)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOLEN
        DELFLG   '削除フラグ
        RECOID   'レコードID
        RECONAME   'レコード名
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        KINGAKU   '金額
    End Enum
    ''' <summary>
    ''' 入出力項目位置(東北電力車両別追加料金)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOLTO
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
        KOTEIHI   '固定費
        KAISU   '回数
    End Enum
    ''' <summary>
    ''' 入出力項目位置(北海道ガス特別料金)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOLKG
        DELFLG   '削除フラグ
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        TODOKECODE   '届先コード
        TODOKENAME   '届先名称
        SYABAN   '車番
        TAISHOYM   '対象年月
        ITEMID   '大項目
        ITEMNAME   '項目名
        RECOID   'レコードID
        RECONAME   'レコード名
        TANKA   '単価
        COUNT   '回数
        FEE   '料金
        BIKOU   '備考
    End Enum
    ''' <summary>
    ''' 入出力項目位置(SK特別料金)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOLSKSP
        DELFLG   '削除フラグ
        RECOID   'レコードID
        RECONAME   'レコード名
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        TODOKECODE   '届先コード
        TODOKENAME   '届先名称
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        SYABARA   '車腹
        KOTEIHI   '固定費
        BIKOU1   '備考1
        BIKOU2   '備考2
        BIKOU3   '備考3
    End Enum
    ''' <summary>
    ''' 入出力項目位置(SK特別料金)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOLSKSU
        DELFLG   '削除フラグ
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        TODOKECODE   '届先コード
        TODOKENAME   '届先名称
        TAISHOYM   '対象年月
        KYORI   '走行距離
        KEIYU   '実勢軽油価格
        KIZYUN   '基準価格
        TANKASA   '単価差
        KAISU   '輸送回数
        USAGECHARGE   '燃料使用量
        SURCHARGE   'サーチャージ
        BIKOU1   '備考1
    End Enum
#End Region
#Region "変更履歴画面DL出力項目位置設定"
    ''' <summary>
    ''' 変更履歴出力項目位置(八戸特別料金)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOLHA
        OPERATEKBNNAME    '操作区分
        MODIFYKBNNAME   '変更区分
        MODIFYYMD   '変更日時
        MODIFYUSER   '変更USER
        DELFLG   '削除フラグ
        RECOID   'レコードID
        RECONAME   'レコード名
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        KINGAKU   '金額
    End Enum
    ''' <summary>
    ''' 変更履歴出力項目位置(ENEOS業務委託料)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOLEN
        OPERATEKBNNAME    '操作区分
        MODIFYKBNNAME   '変更区分
        MODIFYYMD   '変更日時
        MODIFYUSER   '変更USER
        DELFLG   '削除フラグ
        RECOID   'レコードID
        RECONAME   'レコード名
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        KINGAKU   '金額
    End Enum
    ''' <summary>
    ''' 変更履歴出力項目位置(東北電力車両別追加料金)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOLTO
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
        KOTEIHI   '固定費
        KAISU   '回数
    End Enum
    ''' <summary>
    ''' 変更履歴出力項目位置(北海道ガス特別料金)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOLKG
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
        TODOKECODE   '届先コード
        TODOKENAME   '届先名称
        SYABAN   '車番
        TAISHOYM   '対象年月
        ITEMID   '大項目
        ITEMNAME   '項目名
        RECOID   'レコードID
        RECONAME   'レコード名
        TANKA   '単価
        COUNT   '回数
        FEE   '料金
        BIKOU   '備考
    End Enum
    ''' <summary>
    ''' 変更履歴出力項目位置(SK特別料金)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOLSKSP
        OPERATEKBNNAME    '操作区分
        MODIFYKBNNAME   '変更区分
        MODIFYYMD   '変更日時
        MODIFYUSER   '変更USER
        DELFLG   '削除フラグ
        RECOID   'レコードID
        RECONAME   'レコード名
        TORICODE   '取引先コード
        TORINAME   '取引先名称
        ORGCODE   '部門コード
        ORGNAME   '部門名称
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名称
        TODOKECODE   '届先コード
        TODOKENAME   '届先名称
        STYMD   '有効開始日
        ENDYMD   '有効終了日
        SYABARA   '車腹
        KOTEIHI   '固定費
        BIKOU1   '備考1
        BIKOU2   '備考2
        BIKOU3   '備考3
    End Enum
    ''' <summary>
    ''' 変更履歴出力項目位置(SK特別料金)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOLSKSU
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
        TODOKECODE   '届先コード
        TODOKENAME   '届先名称
        TAISHOYM   '対象年月
        KYORI   '走行距離
        KEIYU   '実勢軽油価格
        KIZYUN   '基準価格
        TANKASA   '単価差
        KAISU   '輸送回数
        USAGECHARGE   '燃料使用量
        SURCHARGE   'サーチャージ
        BIKOU1   '備考1
    End Enum
#End Region

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
        SQLStr.AppendLine("     LNG.VIW0004_SPRATE VIW0004                                                                     ")
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
        SQLStr.AppendLine("      ON  VIW0004.ORGCODE = LNS0005.CODE                                                             ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("    VIW0004.TABLEID = @TABLEID                                                                       ")
        SQLStr.AppendLine(" ORDER BY                                                                       ")
        SQLStr.AppendLine("     VIW0004.TORICODE                                                           ")

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
        SQLStr.AppendLine("     LNG.LNM0010_TANKA")

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
        SQLStr.AppendLine("     LNG.LNM0010_TANKA")

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
    ''' ID取得(大項目)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_IDht">大項目格納HT</param>
    Public Sub CODEIDGetITEM(ByVal SQLcon As MySqlConnection,
                                   ByRef O_IDht As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       ITEMID AS ITEMID")
        SQLStr.AppendLine("      ,RTRIM(ITEMNAME) AS ITEMNAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0013_KGSPRATE")

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
                    '大項目、項目名格納
                    If Not O_IDht.ContainsKey(WW_Row("ITEMID")) Then
                        O_IDht.Add(WW_Row("ITEMNAME"), WW_Row("ITEMID"))
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
        SQLStr.AppendLine("     LNG.LNM0010_TANKA")

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
    ''' レコードID生成
    ''' </summary>
    ''' <param name="I_TABLEID">対象テーブル</param>
    Public Shared Function GenerateRECOID(ByVal I_TABLEID As String) As String
        GenerateRECOID = ""

        Dim CS0050Session As New CS0050SESSION

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        'SQLStr.AppendLine("       MAX(RECOID) AS RECOID")
        SQLStr.AppendLine("       MAX(LPAD(RECOID, 5, '0')) AS RECOID ")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.VIW0004_SPRATE")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("        TABLEID  = @TABLEID                 ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@TABLEID", MySqlDbType.VarChar).Value = I_TABLEID
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        Return ""
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If WW_Tbl.Rows.Count >= 1 Then
                        Return (CInt(WW_Tbl.Rows(0)("RECOID")) + 1).ToString
                    Else
                        Return "1"
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try
    End Function

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
            Case MAPIDLHA '八戸特別料金マスタ
                SQLStr.Append("     LNG.LNM0010_HACHINOHESPRATE             ")
            Case MAPIDLEN 'ENEOS業務委託料マスタ
                SQLStr.Append("     LNG.LNM0011_ENEOSCOMFEE           ")
            Case MAPIDLTO '東北電力車両別追加料金マスタ
                SQLStr.Append("     LNG.LNM0012_TOHOKUSPRATE          ")
            Case MAPIDLSKSP 'SK特別料金マスタ
                SQLStr.Append("     LNG.LNM0014_SKSPRATE          ")
        End Select
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("       DELFLG  = '0'             ")
        Select Case WW_CONTROLTABLE
            Case MAPIDLHA '八戸特別料金マスタ
                SQLStr.AppendLine("   AND RECOID  = @RECOID             ")
                SQLStr.AppendLine("   AND TORICODE  = @TORICODE                 ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
            Case MAPIDLEN 'ENEOS業務委託料マスタ
                SQLStr.AppendLine("   AND RECOID  = @RECOID             ")
                SQLStr.AppendLine("   AND TORICODE  = @TORICODE                 ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
            Case MAPIDLTO '東北電力車両別追加料金マスタ
                SQLStr.AppendLine("   AND TORICODE  = @TORICODE                 ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
                SQLStr.AppendLine("   AND SYABAN  = @SYABAN             ")
            Case MAPIDLSKSP 'SK特別料金マスタ
                SQLStr.AppendLine("   AND RECOID  = @RECOID             ")
                SQLStr.AppendLine("   AND TORICODE  = @TORICODE                 ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        End Select

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Select Case WW_CONTROLTABLE
                    Case MAPIDLHA '八戸特別料金マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                    Case MAPIDLEN 'ENEOS業務委託料マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                    Case MAPIDLTO '東北電力車両別追加料金マスタ
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20) '車番

                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_SYABAN.Value = WW_ROW("SYABAN") '車番
                    Case MAPIDLSKSP 'SK特別料金マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                End Select

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

#Region "排他チェック"

    ''' <summary>
    ''' 排他チェック(八戸特別料金マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_RECOID">レコードID</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_STYMD">有効開始日</param>
    Public Sub HaitaCheckHACHINOHESPRATE(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_RECOID As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String,
                          ByVal I_STYMD As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0010_HACHINOHESPRATE             ")
        SQLStr.AppendLine(" WHERE                                       ")
        SQLStr.AppendLine("       RECOID  = @RECOID                     ")
        SQLStr.AppendLine("   AND TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND DATE_FORMAT(STYMD, '%Y/%m/%d') = DATE_FORMAT(@STYMD, '%Y/%m/%d')")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10) 'レコードID
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                P_RECOID.Value = I_RECOID 'レコードID
                P_TORICODE.Value = I_TORICODE '取引先コード
                P_ORGCODE.Value = I_ORGCODE '部門コード
                P_STYMD.Value = CDate(I_STYMD) '有効開始日

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0010Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0010Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0010Chk.Load(SQLdr)

                    If LNM0010Chk.Rows.Count > 0 Then
                        Dim LNM0010row As DataRow
                        LNM0010row = LNM0010Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0010row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0010row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0010D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 排他チェック(ENEOS業務委託料マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_RECOID">レコードID</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_STYMD">有効開始日</param>
    Public Sub HaitaCheckENEOSCOMFEE(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_RECOID As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String,
                          ByVal I_STYMD As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0011_ENEOSCOMFEE                     ")
        SQLStr.AppendLine(" WHERE                                       ")
        SQLStr.AppendLine("       RECOID  = @RECOID                     ")
        SQLStr.AppendLine("   AND TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND DATE_FORMAT(STYMD, '%Y/%m/%d') = DATE_FORMAT(@STYMD, '%Y/%m/%d')")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10) 'レコードID
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                P_RECOID.Value = I_RECOID 'レコードID
                P_TORICODE.Value = I_TORICODE '取引先コード
                P_ORGCODE.Value = I_ORGCODE '部門コード
                P_STYMD.Value = CDate(I_STYMD) '有効開始日

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0010Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0010Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0010Chk.Load(SQLdr)

                    If LNM0010Chk.Rows.Count > 0 Then
                        Dim LNM0010row As DataRow
                        LNM0010row = LNM0010Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0010row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0010row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0010D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 排他チェック(東北電力車両別追加料金マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_STYMD">有効開始日</param>
    ''' <param name="I_SYABAN">車番</param>
    Public Sub HaitaCheckTOHOKUSPRATE(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String,
                          ByVal I_STYMD As String,
                          ByVal I_SYABAN As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0012_TOHOKUSPRATE                     ")
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

                    Dim LNM0010Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0010Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0010Chk.Load(SQLdr)

                    If LNM0010Chk.Rows.Count > 0 Then
                        Dim LNM0010row As DataRow
                        LNM0010row = LNM0010Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0010row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0010row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0010D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 排他チェック(北海道ガス特別料金マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_TAISHOYM">対象年月</param>
    Public Sub HaitaCheckKGSPRATE(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String,
                          ByVal I_TAISHOYM As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0013_KGSPRATE					")
        SQLStr.AppendLine(" WHERE                                       ")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND TAISHOYM = @TAISHOYM                  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6) '対象年月

                P_TORICODE.Value = I_TORICODE '取引先コード
                P_ORGCODE.Value = I_ORGCODE '部門コード
                P_TAISHOYM.Value = Replace(I_TAISHOYM, "/", "") '対象年月

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0010Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0010Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0010Chk.Load(SQLdr)

                    If LNM0010Chk.Rows.Count > 0 Then
                        Dim LNM0010row As DataRow
                        LNM0010row = LNM0010Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0010row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0010row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0010D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 排他チェック(SK特別料金マスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_RECOID">レコードID</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_STYMD">有効開始日</param>
    Public Sub HaitaCheckSKSPRATE(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_RECOID As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String,
                          ByVal I_STYMD As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0014_SKSPRATE                     ")
        SQLStr.AppendLine(" WHERE                                       ")
        SQLStr.AppendLine("       RECOID  = @RECOID                     ")
        SQLStr.AppendLine("   AND TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND DATE_FORMAT(STYMD, '%Y/%m/%d') = DATE_FORMAT(@STYMD, '%Y/%m/%d')")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10) 'レコードID
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                P_RECOID.Value = I_RECOID 'レコードID
                P_TORICODE.Value = I_TORICODE '取引先コード
                P_ORGCODE.Value = I_ORGCODE '部門コード
                P_STYMD.Value = CDate(I_STYMD) '有効開始日

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0010Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0010Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0010Chk.Load(SQLdr)

                    If LNM0010Chk.Rows.Count > 0 Then
                        Dim LNM0010row As DataRow
                        LNM0010row = LNM0010Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0010row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0010row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0010D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 排他チェック(SK燃料サーチャージマスタ)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_TAISHOYM">対象年月</param>
    Public Sub HaitaCheckSKSURCHARGE(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String,
                          ByVal I_TAISHOYM As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                      ")
        SQLStr.AppendLine("    UPDTIMSTP                                ")
        SQLStr.AppendLine(" FROM                                        ")
        SQLStr.AppendLine("     LNG.LNM0015_SKSURCHARGE					")
        SQLStr.AppendLine(" WHERE                                       ")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND TAISHOYM = @TAISHOYM                  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6) '対象年月

                P_TORICODE.Value = I_TORICODE '取引先コード
                P_ORGCODE.Value = I_ORGCODE '部門コード
                P_TAISHOYM.Value = Replace(I_TAISHOYM, "/", "") '対象年月

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0010Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0010Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0010Chk.Load(SQLdr)

                    If LNM0010Chk.Rows.Count > 0 Then
                        Dim LNM0010row As DataRow
                        LNM0010row = LNM0010Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0010row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0010row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0010D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

#End Region

End Class