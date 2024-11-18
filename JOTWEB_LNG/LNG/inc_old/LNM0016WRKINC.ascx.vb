Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0016WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNM0016S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNM0016L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNM0016D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0016H"       'MAPID(履歴)
    'タイトル区分
    Public Const TITLEKBNS As String = "5"   'タイトル区分

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
        ORGCODE   '組織コード
        ORGNAME   '組織名称
        BIGCTNCD   '大分類コード
        BIGCTNNM   '大分類名称
        MIDDLECTNCD   '中分類コード
        MIDDLECTNNM   '中分類名称
        DEPSTATION   '発駅コード
        DEPSTATIONNM   '発駅名称
        DEPTRUSTEECD   '発受託人コード
        DEPTRUSTEENM   '発受託人名称
        DEPTRUSTEESUBCD   '発受託人サブコード
        DEPTRUSTEESUBNM   '発受託人サブ名称
        PRIORITYNO   '優先順位
        PURPOSE   '使用目的
        SLCCTNTYPE   '選択比較項目-コンテナ記号
        SLCCTNSTNO   '選択比較項目-コンテナ番号（開始）
        SLCCTNENDNO   '選択比較項目-コンテナ番号（終了）
        SLCJRDEPBRANCHCD   '選択比較項目-ＪＲ発支社支店コード
        SLCDEPSHIPPERCD1   '選択比較項目-発荷主コード１
        SLCDEPSHIPPERCD2   '選択比較項目-発荷主コード２
        SLCDEPSHIPPERCD3   '選択比較項目-発荷主コード３
        SLCDEPSHIPPERCD4   '選択比較項目-発荷主コード４
        SLCDEPSHIPPERCD5   '選択比較項目-発荷主コード５
        SLCDEPSHIPPERCDCOND   '選択比較項目-発荷主ＣＤ比較条件
        SLCJRARRBRANCHCD   '選択比較項目-ＪＲ着支社支店コード
        SLCJRARRBRANCHCDCOND   '選択比較項目-ＪＲ着支社支店ＣＤ比較
        SLCJOTARRORGCODE   '選択比較項目-ＪＯＴ着組織コード
        SLCJOTARRORGCODECOND   '選択比較項目-ＪＯＴ着組織ＣＤ比較
        SLCARRSTATION1   '選択比較項目-着駅コード１
        SLCARRSTATION2   '選択比較項目-着駅コード２
        SLCARRSTATION3   '選択比較項目-着駅コード３
        SLCARRSTATION4   '選択比較項目-着駅コード４
        SLCARRSTATION5   '選択比較項目-着駅コード５
        SLCARRSTATION6   '選択比較項目-着駅コード６
        SLCARRSTATION7   '選択比較項目-着駅コード７
        SLCARRSTATION8   '選択比較項目-着駅コード８
        SLCARRSTATION9   '選択比較項目-着駅コード９
        SLCARRSTATION10   '選択比較項目-着駅コード１０
        SLCARRSTATIONCOND   '選択比較項目-着駅コード比較条件
        SLCARRTRUSTEECD   '選択比較項目-着受託人コード
        SLCARRTRUSTEECDCOND   '選択比較項目-着受託人ＣＤ比較条件
        SLCARRTRUSTEESUBCD   '選択比較項目-着受託人サブコード
        SLCARRTRUSTEESUBCDCOND   '選択比較項目-着受託人サブＣＤ比較
        SLCSTMD   '選択比較項目-開始月日
        SLCENDMD   '選択比較項目-終了月日
        SLCSTSHIPYMD   '選択比較項目-開始発送年月日
        SLCENDSHIPYMD   '選択比較項目-終了発送年月日
        SLCJRITEMCD1   '選択比較項目-ＪＲ品目コード１
        SLCJRITEMCD2   '選択比較項目-ＪＲ品目コード２
        SLCJRITEMCD3   '選択比較項目-ＪＲ品目コード３
        SLCJRITEMCD4   '選択比較項目-ＪＲ品目コード４
        SLCJRITEMCD5   '選択比較項目-ＪＲ品目コード５
        SLCJRITEMCD6   '選択比較項目-ＪＲ品目コード６
        SLCJRITEMCD7   '選択比較項目-ＪＲ品目コード７
        SLCJRITEMCD8   '選択比較項目-ＪＲ品目コード８
        SLCJRITEMCD9   '選択比較項目-ＪＲ品目コード９
        SLCJRITEMCD10   '選択比較項目-ＪＲ品目コード１０
        SLCJRITEMCDCOND   '選択比較項目-ＪＲ品目コード比較
        SPRUSEFEE   '特例置換項目-使用料金額
        SPRUSEFEERATE   '特例置換項目-使用料率
        SPRUSEFEERATEROUND   '特例置換項目-使用料率端数整理
        SPRUSEFEERATEADDSUB   '特例置換項目-使用料率加減額
        SPRUSEFEERATEADDSUBCOND   '特例置換項目-使用料率加減額端数整理
        SPRROUNDPOINTKBN   '特例置換項目-端数処理時点区分
        SPRUSEFREESPE   '特例置換項目-使用料無料特認
        SPRNITTSUFREESENDFEE   '特例置換項目-通運負担回送運賃
        SPRMANAGEFEE   '特例置換項目-運行管理料
        SPRSHIPBURDENFEE   '特例置換項目-荷主負担運賃
        SPRSHIPFEE   '特例置換項目-発送料
        SPRARRIVEFEE   '特例置換項目-到着料
        SPRPICKUPFEE   '特例置換項目-集荷料
        SPRDELIVERYFEE   '特例置換項目-配達料
        SPROTHER1   '特例置換項目-その他１
        SPROTHER2   '特例置換項目-その他２
        SPRFITKBN   '特例置換項目-適合区分
        SPRCONTRACTCD   '特例置換項目-契約コード
        BEFOREORGCODE   '変換前組織コード
        BEFORESLCJOTARRORGCODE   '変換前 選択比較項目-ＪＯＴ着組織コード
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
        ORGCODE   '組織コード
        BIGCTNCD   '大分類コード
        MIDDLECTNCD   '中分類コード
        DEPSTATION   '発駅コード
        DEPTRUSTEECD   '発受託人コード
        DEPTRUSTEESUBCD   '発受託人サブコード
        PRIORITYNO   '優先順位
        PURPOSE   '使用目的
        SLCCTNTYPE   '選択比較項目-コンテナ記号
        SLCCTNSTNO   '選択比較項目-コンテナ番号（開始）
        SLCCTNENDNO   '選択比較項目-コンテナ番号（終了）
        SLCJRDEPBRANCHCD   '選択比較項目-ＪＲ発支社支店コード
        SLCDEPSHIPPERCD1   '選択比較項目-発荷主コード１
        SLCDEPSHIPPERCD2   '選択比較項目-発荷主コード２
        SLCDEPSHIPPERCD3   '選択比較項目-発荷主コード３
        SLCDEPSHIPPERCD4   '選択比較項目-発荷主コード４
        SLCDEPSHIPPERCD5   '選択比較項目-発荷主コード５
        SLCDEPSHIPPERCDCOND   '選択比較項目-発荷主ＣＤ比較条件
        SLCJRARRBRANCHCD   '選択比較項目-ＪＲ着支社支店コード
        SLCJRARRBRANCHCDCOND   '選択比較項目-ＪＲ着支社支店ＣＤ比較
        SLCJOTARRORGCODE   '選択比較項目-ＪＯＴ着組織コード
        SLCJOTARRORGCODECOND   '選択比較項目-ＪＯＴ着組織ＣＤ比較
        SLCARRSTATION1   '選択比較項目-着駅コード１
        SLCARRSTATION2   '選択比較項目-着駅コード２
        SLCARRSTATION3   '選択比較項目-着駅コード３
        SLCARRSTATION4   '選択比較項目-着駅コード４
        SLCARRSTATION5   '選択比較項目-着駅コード５
        SLCARRSTATION6   '選択比較項目-着駅コード６
        SLCARRSTATION7   '選択比較項目-着駅コード７
        SLCARRSTATION8   '選択比較項目-着駅コード８
        SLCARRSTATION9   '選択比較項目-着駅コード９
        SLCARRSTATION10   '選択比較項目-着駅コード１０
        SLCARRSTATIONCOND   '選択比較項目-着駅コード比較条件
        SLCARRTRUSTEECD   '選択比較項目-着受託人コード
        SLCARRTRUSTEECDCOND   '選択比較項目-着受託人ＣＤ比較条件
        SLCARRTRUSTEESUBCD   '選択比較項目-着受託人サブコード
        SLCARRTRUSTEESUBCDCOND   '選択比較項目-着受託人サブＣＤ比較
        SLCSTMD   '選択比較項目-開始月日
        SLCENDMD   '選択比較項目-終了月日
        SLCSTSHIPYMD   '選択比較項目-開始発送年月日
        SLCENDSHIPYMD   '選択比較項目-終了発送年月日
        SLCJRITEMCD1   '選択比較項目-ＪＲ品目コード１
        SLCJRITEMCD2   '選択比較項目-ＪＲ品目コード２
        SLCJRITEMCD3   '選択比較項目-ＪＲ品目コード３
        SLCJRITEMCD4   '選択比較項目-ＪＲ品目コード４
        SLCJRITEMCD5   '選択比較項目-ＪＲ品目コード５
        SLCJRITEMCD6   '選択比較項目-ＪＲ品目コード６
        SLCJRITEMCD7   '選択比較項目-ＪＲ品目コード７
        SLCJRITEMCD8   '選択比較項目-ＪＲ品目コード８
        SLCJRITEMCD9   '選択比較項目-ＪＲ品目コード９
        SLCJRITEMCD10   '選択比較項目-ＪＲ品目コード１０
        SLCJRITEMCDCOND   '選択比較項目-ＪＲ品目コード比較
        SPRUSEFEE   '特例置換項目-使用料金額
        SPRUSEFEERATE   '特例置換項目-使用料率
        SPRUSEFEERATEROUND   '特例置換項目-使用料率端数整理
        SPRUSEFEERATEADDSUB   '特例置換項目-使用料率加減額
        SPRUSEFEERATEADDSUBCOND   '特例置換項目-使用料率加減額端数整理
        SPRROUNDPOINTKBN   '特例置換項目-端数処理時点区分
        SPRUSEFREESPE   '特例置換項目-使用料無料特認
        SPRNITTSUFREESENDFEE   '特例置換項目-通運負担回送運賃
        SPRMANAGEFEE   '特例置換項目-運行管理料
        SPRSHIPBURDENFEE   '特例置換項目-荷主負担運賃
        SPRSHIPFEE   '特例置換項目-発送料
        SPRARRIVEFEE   '特例置換項目-到着料
        SPRPICKUPFEE   '特例置換項目-集荷料
        SPRDELIVERYFEE   '特例置換項目-配達料
        SPROTHER1   '特例置換項目-その他１
        SPROTHER2   '特例置換項目-その他２
        SPRFITKBN   '特例置換項目-適合区分
        SPRCONTRACTCD   '特例置換項目-契約コード
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

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
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
    ''' コンテナ記号・番号取得パラメーター設定
    ''' </summary>
    ''' <param name="CNTENATYPE_FLG"></param>
    ''' <param name="I_CTNTYPE"></param>
    ''' <returns></returns>
    Public Function CreateContenaParam(ByVal CNTENATYPE_FLG As Integer, Optional ByVal I_CTNTYPE As String = "") As Hashtable

        Dim WW_PrmData As New Hashtable
        WW_PrmData.Item(C_PARAMETERS.LP_CTNTYPE) = I_CTNTYPE
        WW_PrmData.Item(C_PARAMETERS.LP_TYPEMODE) = CNTENATYPE_FLG

        CreateContenaParam = WW_PrmData

    End Function

    ''' <summary>
    ''' 受託人コード取得パラメーター設定
    ''' </summary>
    ''' <param name="CODETYPE_FLG"></param>
    ''' <param name="I_STATION"></param>
    ''' <param name="I_TRUSTEECD"></param>
    ''' <returns></returns>
    Function CreateTrusteeCdParam(ByVal CODETYPE_FLG As Integer, ByVal I_STATION As String, Optional ByVal I_TRUSTEECD As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = CODETYPE_FLG
        prmData.Item(C_PARAMETERS.LP_STATION) = I_STATION
        prmData.Item(C_PARAMETERS.LP_TRUSTEECD) = I_TRUSTEECD

        CreateTrusteeCdParam = prmData

    End Function

    ''' <summary>
    ''' 大中小分類コード取得のパラメータ設定
    ''' </summary>
    ''' <param name="I_BIGCTNCD"></param>
    ''' <param name="I_MIDDLECTNCD"></param>
    ''' <param name="CLASSTYPE_FLG"></param>
    ''' <returns></returns>
    Public Function CreateClassParam(ByVal CLASSTYPE_FLG As Integer, Optional ByVal I_BIGCTNCD As String = "", Optional ByVal I_MIDDLECTNCD As String = "") As Hashtable

        Dim WW_PrmData As New Hashtable
        WW_PrmData.Item(C_PARAMETERS.LP_TYPEMODE) = CLASSTYPE_FLG
        WW_PrmData.Item(C_PARAMETERS.LP_BIGCTNCD) = I_BIGCTNCD
        WW_PrmData.Item(C_PARAMETERS.LP_MIDDLECTNCD) = I_MIDDLECTNCD

        CreateClassParam = WW_PrmData

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
    ''' 駅コード取得のパラメータ設定
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_ORGCODE"></param>
    ''' <returns></returns>
    Function CreateStationParam(ByVal I_COMPCODE As String, Optional ByVal I_ORGCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_ORG) = I_ORGCODE
        CreateStationParam = prmData
    End Function

    ''' <summary>
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="ORGCODE">組織コード</param>
    ''' <param name="BIGCTNCD">大分類コード</param>
    ''' <param name="MIDDLECTNCD">中分類コード</param>
    ''' <param name="DEPSTATION">発駅コード</param>
    ''' <param name="DEPTRUSTEECD">発受託人コード</param>
    ''' <param name="DEPTRUSTEESUBCD">発受託人サブコード</param>
    ''' <param name="PRIORITYNO">優先順位</param>
    ''' <param name="TIMESTAMP">タイムスタンプ</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                             ByRef ORGCODE As String, ByRef BIGCTNCD As String,
                             ByRef MIDDLECTNCD As String, ByRef DEPSTATION As String,
                             ByRef DEPTRUSTEECD As String, ByRef DEPTRUSTEESUBCD As String,
                             ByRef PRIORITYNO As String, ByRef TIMESTAMP As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     ORGCODE                                 " _
            & "   , BIGCTNCD                                " _
            & "   , MIDDLECTNCD                             " _
            & "   , DEPSTATION                              " _
            & "   , DEPTRUSTEECD                            " _
            & "   , DEPTRUSTEESUBCD                         " _
            & "   , PRIORITYNO                              " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     LNG.LNM0016_REST1M                      " _
            & " WHERE                                       " _
            & "         ORGCODE          = @P1              " _
            & "     AND BIGCTNCD         = @P2              " _
            & "     AND MIDDLECTNCD      = @P3              " _
            & "     AND DEPSTATION       = @P4              " _
            & "     AND DEPTRUSTEECD     = @P5              " _
            & "     AND DEPTRUSTEESUBCD  = @P6              " _
            & "     AND PRIORITYNO       = @P7              "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6) '組織コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 2) '大分類コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 2) '中分類コード
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 6) '発駅コード
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar, 5) '発受託人コード
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@P6", MySqlDbType.VarChar, 3) '発受託人サブコード
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@P7", MySqlDbType.VarChar, 5) '優先順位

                PARA1.Value = ORGCODE          '組織コード
                PARA2.Value = BIGCTNCD         '大分類コード
                PARA3.Value = MIDDLECTNCD      '中分類コード
                PARA4.Value = DEPSTATION       '発駅コード
                PARA5.Value = DEPTRUSTEECD     '発受託人コード
                PARA6.Value = DEPTRUSTEESUBCD  '発受託人サブコード
                PARA7.Value = PRIORITYNO       '優先順位

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0016Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0016Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0016Chk.Load(SQLdr)

                    If LNM0016Chk.Rows.Count > 0 Then
                        Dim LNM0016row As DataRow
                        LNM0016row = LNM0016Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0016row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0016row("UPDTIMSTP").ToString <> TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0016C HAITA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

End Class