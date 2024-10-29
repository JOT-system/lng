Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 在庫データ
''' </summary>
''' <remarks>
''' <list type="number">
''' <item><description>リース明細画面データ用のキー</description></item>
''' </list>
''' </remarks>
Public Enum ZAIKO_DP
    CS_KEIJOYM                 '計上年月
    CS_CTNTYPE                 'コンテナ形式
    CS_CTNNO                   'コンテナ番号
    CS_INVOICEKEIJYOBRANCHCODE '計上支店
    CS_STATIONCODE             '現在駅
    CS_STOCKSTATUS             '在庫状態
    CS_STOCKREGISTRATIONDATE   '在庫登録日
    CS_EXCEPTIONDATE           '運用除外日
    CS_STOCKREGISTRATID        '在庫登録者
    CS_DISPOSALFLG             '在庫処分フラグ
    CS_INITYMD            '登録年月日
    CS_INITUSER           '登録ユーザーＩＤ
    CS_INITTERMID         '登録端末
    CS_INITPGID           '登録プログラムＩＤ
    CS_UPDYMD             '更新年月日
    CS_UPDUSER            '更新ユーザーＩＤ
    CS_UPDTERMID          '更新端末
    CS_UPDPGID            '更新プログラムＩＤ
End Enum

Public Class LNM0002WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNM0002S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNM0002L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNM0002D"       'MAPID(更新)
    Public Const MAPIDH As String = "LNM0002H"       'MAPID(履歴)
    'タイトル区分
    Public Const TITLEKBNS As String = "C"   'タイトル区分

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
        CTNTYPE   'コンテナ記号
        CTNNO   'コンテナ番号
        JURISDICTIONCD   '所管部コード
        ACCOUNTINGASSETSCD   '経理資産コード
        ACCOUNTINGASSETSKBN   '経理資産区分
        DUMMYKBN   'ダミー区分
        SPOTKBN   'スポット区分
        SPOTSTYMD   'スポット区分　開始年月日
        SPOTENDYMD   'スポット区分　終了年月日
        BIGCTNCD   '大分類コード
        MIDDLECTNCD   '中分類コード
        SMALLCTNCD   '小分類コード
        CONSTRUCTIONYM   '建造年月
        CTNMAKER   'コンテナメーカー
        FROZENMAKER   '冷凍機メーカー
        GROSSWEIGHT   '総重量
        CARGOWEIGHT   '荷重
        MYWEIGHT   '自重
        BOOKVALUE   '簿価商品価格
        OUTHEIGHT   '外寸・高さ
        OUTWIDTH   '外寸・幅
        OUTLENGTH   '外寸・長さ
        INHEIGHT   '内寸・高さ
        INWIDTH   '内寸・幅
        INLENGTH   '内寸・長さ
        WIFEHEIGHT   '妻入口・高さ
        WIFEWIDTH   '妻入口・幅
        SIDEHEIGHT   '側入口・高さ
        SIDEWIDTH   '側入口・幅
        FLOORAREA   '床面積
        INVOLUMEMARKING   '内容積・標記
        INVOLUMEACTUA   '内容積・実寸
        TRAINSCYCLEDAYS   '交番検査・ｻｲｸﾙ日数
        TRAINSBEFORERUNYMD   '交番検査・前回実施日
        TRAINSNEXTRUNYMD   '交番検査・次回実施日
        REGINSCYCLEDAYS   '定期検査・ｻｲｸﾙ月数
        REGINSCYCLEHOURMETER   '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
        REGINSBEFORERUNYMD   '定期検査・前回実施日
        REGINSNEXTRUNYMD   '定期検査・次回実施日
        REGINSHOURMETERYMD   '定期検査・ｱﾜﾒｰﾀ記載日
        REGINSHOURMETERTIME   '定期検査・ｱﾜﾒｰﾀ時間
        REGINSHOURMETERDSP   '定期検査・ｱﾜﾒｰﾀ表示桁
        OPERATIONSTYMD   '運用開始年月日
        OPERATIONENDYMD   '運用除外年月日
        RETIRMENTYMD   '除却年月日
        COMPKANKBN   '複合一貫区分
        SUPPLYFLG   '調達フラグ
        ADDITEM1   '付帯項目１
        ADDITEM2   '付帯項目２
        ADDITEM3   '付帯項目３
        ADDITEM4   '付帯項目４
        ADDITEM5   '付帯項目５
        ADDITEM6   '付帯項目６
        ADDITEM7   '付帯項目７
        ADDITEM8   '付帯項目８
        ADDITEM9   '付帯項目９
        ADDITEM10   '付帯項目１０
        ADDITEM11   '付帯項目１１
        ADDITEM12   '付帯項目１２
        ADDITEM13   '付帯項目１３
        ADDITEM14   '付帯項目１４
        ADDITEM15   '付帯項目１５
        ADDITEM16   '付帯項目１６
        ADDITEM17   '付帯項目１７
        ADDITEM18   '付帯項目１８
        ADDITEM19   '付帯項目１９
        ADDITEM20   '付帯項目２０
        ADDITEM21   '付帯項目２１
        ADDITEM22   '付帯項目２２
        ADDITEM23   '付帯項目２３
        ADDITEM24   '付帯項目２４
        ADDITEM25   '付帯項目２５
        ADDITEM26   '付帯項目２５
        ADDITEM27   '付帯項目２７
        ADDITEM28   '付帯項目２８
        ADDITEM29   '付帯項目２９
        ADDITEM30   '付帯項目３０
        ADDITEM31   '付帯項目３１
        ADDITEM32   '付帯項目３２
        ADDITEM33   '付帯項目３３
        ADDITEM34   '付帯項目３４
        ADDITEM35   '付帯項目３５
        ADDITEM36   '付帯項目３６
        ADDITEM37   '付帯項目３７
        ADDITEM38   '付帯項目３８
        ADDITEM39   '付帯項目３９
        ADDITEM40   '付帯項目４０
        ADDITEM41   '付帯項目４１
        ADDITEM42   '付帯項目４２
        ADDITEM43   '付帯項目４３
        ADDITEM44   '付帯項目４４
        ADDITEM45   '付帯項目４５
        ADDITEM46   '付帯項目４６
        ADDITEM47   '付帯項目４７
        ADDITEM48   '付帯項目４８
        ADDITEM49   '付帯項目４９
        ADDITEM50   '付帯項目５０
        FLOORMATERIAL   '床材質コード
    End Enum

    ''' <summary>
    ''' 変更履歴出力項目位置
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOL
        OPERATEKBNNAME   '操作区分
        MODIFYKBNNAME   '変更区分
        MODIFYYMD   '変更日時
        MODIFYUSER   '変更USER
        DELFLG   '削除フラグ
        CTNTYPE   'コンテナ記号
        CTNNO   'コンテナ番号
        JURISDICTIONCD   '所管部コード
        ACCOUNTINGASSETSCD   '経理資産コード
        ACCOUNTINGASSETSKBN   '経理資産区分
        DUMMYKBN   'ダミー区分
        SPOTKBN   'スポット区分
        SPOTSTYMD   'スポット区分　開始年月日
        SPOTENDYMD   'スポット区分　終了年月日
        BIGCTNCD   '大分類コード
        MIDDLECTNCD   '中分類コード
        SMALLCTNCD   '小分類コード
        CONSTRUCTIONYM   '建造年月
        CTNMAKER   'コンテナメーカー
        FROZENMAKER   '冷凍機メーカー
        GROSSWEIGHT   '総重量
        CARGOWEIGHT   '荷重
        MYWEIGHT   '自重
        BOOKVALUE   '簿価商品価格
        OUTHEIGHT   '外寸・高さ
        OUTWIDTH   '外寸・幅
        OUTLENGTH   '外寸・長さ
        INHEIGHT   '内寸・高さ
        INWIDTH   '内寸・幅
        INLENGTH   '内寸・長さ
        WIFEHEIGHT   '妻入口・高さ
        WIFEWIDTH   '妻入口・幅
        SIDEHEIGHT   '側入口・高さ
        SIDEWIDTH   '側入口・幅
        FLOORAREA   '床面積
        INVOLUMEMARKING   '内容積・標記
        INVOLUMEACTUA   '内容積・実寸
        TRAINSCYCLEDAYS   '交番検査・ｻｲｸﾙ日数
        TRAINSBEFORERUNYMD   '交番検査・前回実施日
        TRAINSNEXTRUNYMD   '交番検査・次回実施日
        REGINSCYCLEDAYS   '定期検査・ｻｲｸﾙ月数
        REGINSCYCLEHOURMETER   '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
        REGINSBEFORERUNYMD   '定期検査・前回実施日
        REGINSNEXTRUNYMD   '定期検査・次回実施日
        REGINSHOURMETERYMD   '定期検査・ｱﾜﾒｰﾀ記載日
        REGINSHOURMETERTIME   '定期検査・ｱﾜﾒｰﾀ時間
        REGINSHOURMETERDSP   '定期検査・ｱﾜﾒｰﾀ表示桁
        OPERATIONSTYMD   '運用開始年月日
        OPERATIONENDYMD   '運用除外年月日
        RETIRMENTYMD   '除却年月日
        COMPKANKBN   '複合一貫区分
        SUPPLYFLG   '調達フラグ
        ADDITEM1   '付帯項目１
        ADDITEM2   '付帯項目２
        ADDITEM3   '付帯項目３
        ADDITEM4   '付帯項目４
        ADDITEM5   '付帯項目５
        ADDITEM6   '付帯項目６
        ADDITEM7   '付帯項目７
        ADDITEM8   '付帯項目８
        ADDITEM9   '付帯項目９
        ADDITEM10   '付帯項目１０
        ADDITEM11   '付帯項目１１
        ADDITEM12   '付帯項目１２
        ADDITEM13   '付帯項目１３
        ADDITEM14   '付帯項目１４
        ADDITEM15   '付帯項目１５
        ADDITEM16   '付帯項目１６
        ADDITEM17   '付帯項目１７
        ADDITEM18   '付帯項目１８
        ADDITEM19   '付帯項目１９
        ADDITEM20   '付帯項目２０
        ADDITEM21   '付帯項目２１
        ADDITEM22   '付帯項目２２
        ADDITEM23   '付帯項目２３
        ADDITEM24   '付帯項目２４
        ADDITEM25   '付帯項目２５
        ADDITEM26   '付帯項目２５
        ADDITEM27   '付帯項目２７
        ADDITEM28   '付帯項目２８
        ADDITEM29   '付帯項目２９
        ADDITEM30   '付帯項目３０
        ADDITEM31   '付帯項目３１
        ADDITEM32   '付帯項目３２
        ADDITEM33   '付帯項目３３
        ADDITEM34   '付帯項目３４
        ADDITEM35   '付帯項目３５
        ADDITEM36   '付帯項目３６
        ADDITEM37   '付帯項目３７
        ADDITEM38   '付帯項目３８
        ADDITEM39   '付帯項目３９
        ADDITEM40   '付帯項目４０
        ADDITEM41   '付帯項目４１
        ADDITEM42   '付帯項目４２
        ADDITEM43   '付帯項目４３
        ADDITEM44   '付帯項目４４
        ADDITEM45   '付帯項目４５
        ADDITEM46   '付帯項目４６
        ADDITEM47   '付帯項目４７
        ADDITEM48   '付帯項目４８
        ADDITEM49   '付帯項目４９
        ADDITEM50   '付帯項目５０
        FLOORMATERIAL   '床材質コード
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
    ''' <remarks></remarks>
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
    ''' コンテナ記号・番号取得のパラメータ設定
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
    ''' 固定値マスタから一覧の取得設定
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_FIXCODE"></param>
    ''' <returns></returns>
    Function CreateFIXParam(ByVal I_COMPCODE As String, Optional ByVal I_FIXCODE As String = "") As Hashtable
        Dim WW_PrmData As New Hashtable
        WW_PrmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        WW_PrmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_FIXCODE
        CreateFIXParam = WW_PrmData
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
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="CTNTYPE">コンテナ記号</param>
    ''' <param name="CTNNO">コンテナ番号</param>
    ''' <param name="TIMESTAMP">タイムスタンプ</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                             ByRef CTNTYPE As String, ByRef CTNNO As String, ByRef TIMESTAMP As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     CTNTYPE                                 " _
            & "   , CTNNO                                   " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     LNG.LNM0002_RECONM                      " _
            & " WHERE                                       " _
            & "         CTNTYPE = @P1                       " _
            & "     AND CTNNO   = @P2                       "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 5)  'コンテナ記号
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 8)  'コンテナ番号

                PARA1.Value = CTNTYPE  'コンテナ記号
                PARA2.Value = CTNNO    'コンテナ番号

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0002Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0002Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0002Chk.Load(SQLdr)

                    If LNM0002Chk.Rows.Count > 0 Then
                        Dim LNM0002row As DataRow
                        LNM0002row = LNM0002Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0002row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0002row("UPDTIMSTP").ToString <> TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0002C HAITA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' DataTableの指定位置からString値を取得する
    ''' </summary>
    ''' <param name="objOutputData">DataTable</param>
    ''' <param name="nRow">行</param>
    ''' <param name="strCol">列</param>
    ''' <param name="strDefault">規定値</param>
    ''' <returns>取得データ</returns>
    ''' <remarks>値がDBNULLの場合は規定値が返却される</remarks>
    Public Shared Function GetStringValue(ByVal objOutputData As DataTable, ByVal nRow As Integer, ByVal strCol As String, Optional ByVal strDefault As String = "") As String
        Dim strRet As String = strDefault
        Dim objCell As Object = objOutputData.Rows(nRow)(strCol)

        If Not IsDBNull(objCell) Then
            strRet = objCell.ToString
        End If

        Return strRet
    End Function

    ''' <summary>
    ''' 在庫更新判定処理
    ''' </summary>
    ''' <param name="LNM0002row">テーブル1レコード</param>
    ''' <remarks>原価確定状態テーブルの計上年月を取得する</remarks>
    Public Shared Function GetZaikoUpdateHantei(LNM0002row As DataRow) As String

        Dim blnUpdKbn As String = ""

        '生存のみ
        If LNM0002row("DELFLG").ToString.Equals(C_DELETE_FLG.ALIVE) Then
            '除却年月日が空白の場合
            If LNM0002row("RETIRMENTYMD").ToString.Equals("") OrElse LNM0002row("RETIRMENTYMD").ToString.Equals(Date.MinValue.ToString) Then
                '運用除外年月日が存在する場合
                If Not LNM0002row("OPERATIONENDYMD").ToString.Equals("") Then
                    'キマークの場合
                    If (LNM0002row("ADDITEM2").ToString.Equals("47") _
                        AndAlso LNM0002row("ADDITEM10").ToString.Equals("00")) _
                    OrElse (LNM0002row("ADDITEM2").ToString.Equals("00") _
                            AndAlso LNM0002row("ADDITEM10").ToString.Equals("47")) Then
                        ' 在庫更新 (引合待)
                        blnUpdKbn = "1"
                    End If
                    If (LNM0002row("ADDITEM2").ToString.Equals("91") _
                        AndAlso LNM0002row("ADDITEM10").ToString.Equals("00")) _
                    OrElse (LNM0002row("ADDITEM2").ToString.Equals("00") _
                            AndAlso LNM0002row("ADDITEM10").ToString.Equals("91")) Then
                        ' 在庫更新 (営業外引合待)
                        blnUpdKbn = "2"
                    End If
                End If
            End If
        End If

        GetZaikoUpdateHantei = blnUpdKbn

    End Function

    ''' <summary>
    ''' 原価確定状態テーブル 計上年月取得処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="prmKakuteiFlg">計上年月</param>
    ''' <remarks>原価確定状態テーブルの計上年月を取得する</remarks>
    Public Shared Sub GetKeijyoYYYYMM(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                           ByVal prmKakuteiFlg As String,
                                           ByRef refKeijoYM As String)
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理

        refKeijoYM = ""

        With sqlText
            .AppendLine("SELECT TOP 1")
            .AppendLine("    KEIJOYM")
            .AppendLine("FROM")
            'メイン コンテナ在庫テーブル
            .AppendLine("     LNG.LNT0093_GOODSSALES_CONFIRM")
            '抽出条件
            .AppendLine(" WHERE")
            .AppendLine("     DELFLG = @DELFLG")
            .AppendLine(" AND CONFIRMSTATUS = @CONFIRMSTATUS")
            .AppendLine(" ORDER BY KEIJOYM DESC")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@CONFIRMSTATUS", prmKakuteiFlg)
            .Add("@DELFLG", C_DELETE_FLG.ALIVE)
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            refKeijoYM = GetStringValue(sqlRetSet, 0, "KEIJOYM")
        End If

    End Sub

    ''' <summary>
    ''' 駅コード取得処理(現況表)
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="prmCtnType">コンテナ記号</param>
    ''' <param name="prmCtnNo">コンテナ番号</param>
    ''' <param name="refCnt">取得件数</param>
    ''' <param name="refStationCd">駅コード</param>
    ''' <param name="refCtnStatus">コンテナ状態区分</param>
    ''' <remarks>現況表から着駅コードを取得する</remarks>
    Public Shared Sub GetStation(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                           ByVal prmCtnType As String, ByVal prmCtnNo As String,
                                           ByRef refCnt As Integer,
                                           ByRef refStationCd As String, ByRef refCtnStatus As String)
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理

        refCnt = 0
        refStationCd = ""
        refCtnStatus = ""

        With sqlText
            .AppendLine("SELECT TOP 1")
            .AppendLine("    ARRSTATION, CONTSTATUS")
            .AppendLine("FROM")
            'メイン 駅マスタ
            .AppendLine("     LNG.LNT0021_PRESENTSTATE")
            '抽出条件
            .AppendLine(" WHERE")
            .AppendLine("     CTNTYPE = @CTNTYPE")
            .AppendLine(" AND CTNNO   = @CTNNO")
            .AppendLine(" AND DELFLG  = @DELFLG")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@CTNTYPE", prmCtnType)
            .Add("@CTNNO", prmCtnNo)
            .Add("@DELFLG", C_DELETE_FLG.ALIVE)
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            refStationCd = GetStringValue(sqlRetSet, 0, "ARRSTATION")
            refCtnStatus = GetStringValue(sqlRetSet, 0, "CONTSTATUS")
        End If

        refCnt = sqlRetSet.Rows.Count

    End Sub

    ''' <summary>
    ''' 計上支店管轄支店取得処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="prmStationCd">駅コード</param>
    ''' <param name="refOrgCode">計上支店</param>
    ''' <param name="refGovernOrgCode">管轄支店</param>
    ''' <remarks>駅マスタから計上支店、管轄支店を取得する</remarks>
    Public Shared Sub GetOrgGovernCode(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                           ByVal prmStationCd As String,
                                           ByRef refOrgCode As String, ByRef refGovernOrgCode As String)
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理
        Dim intLeaseCnt As Integer = 0

        refOrgCode = ""
        refGovernOrgCode = ""

        With sqlText
            .AppendLine("SELECT TOP 1")
            .AppendLine("    ORGCODE, GOVERNORGCODE")
            .AppendLine("FROM")
            'メイン 駅マスタ
            .AppendLine("     COM.LNS0020_STATION")
            '抽出条件
            .AppendLine(" WHERE")
            .AppendLine("     STATION = @STATION")
            .AppendLine(" AND DELFLG = @DELFLG")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@STATION", prmStationCd)
            .Add("@DELFLG", C_DELETE_FLG.ALIVE)
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            refOrgCode = GetStringValue(sqlRetSet, 0, "ORGCODE")
            refGovernOrgCode = GetStringValue(sqlRetSet, 0, "GOVERNORGCODE")
        End If

    End Sub

    ''' <summary>
    ''' コンテナ在庫テーブル 件数、原価確定フラグ、請求書発行部手取得処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="prmKeijoYM">計上年月</param>
    ''' <param name="prmCtnType">コンテナ記号</param>
    ''' <param name="prmCtnNo">コンテナ番号</param>
    ''' <param name="refCnt">取得件数</param>
    ''' <param name="refCONFIRMFLG">原価確定フラグ</param>
    ''' <param name="refINVOICEORGCODE">請求書発行部店</param>
    ''' <param name="refDISPOSALFLG">在庫処分フラグ</param>
    ''' <remarks>コンテナ在庫テーブルの件数を取得する</remarks>
    Public Shared Sub GetCtnStockCnt(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                           ByVal prmKeijoYM As String,
                                           ByVal prmCtnType As String,
                                           ByVal prmCtnNo As String,
                                           ByRef refCnt As Integer,
                                           ByRef refCONFIRMFLG As String,
                                           ByRef refINVOICEORGCODE As String,
                                           ByRef refDISPOSALFLG As String)
        Dim sqlText As New StringBuilder()
        Dim sqlParam As New Hashtable
        Dim sqlRetSet As DataTable = Nothing
        Dim CS0050SESSION As New CS0050SESSION    'セッション情報操作処理

        refCnt = 0
        refCONFIRMFLG = ""

        With sqlText
            .AppendLine("SELECT ")
            .AppendLine("    CTNNO")
            .AppendLine("   ,CONFIRMFLG")
            .AppendLine("   ,INVOICEORGCODE")
            .AppendLine("   ,DISPOSALFLG")
            .AppendLine("FROM")
            'メイン コンテナ在庫テーブル
            .AppendLine("     LNG.LNT0089_CONTAINER_STOCK")
            '抽出条件
            .AppendLine(" WHERE")
            .AppendLine("     KEIJOYM = @KEIJOYM")
            .AppendLine(" AND CTNTYPE = @CTNTYPE")
            .AppendLine(" AND CTNNO = @CTNNO")
            .AppendLine(" AND DELFLG = @DELFLG")
        End With

        'パラメータ設定
        With sqlParam
            .Add("@KEIJOYM", prmKeijoYM)
            .Add("@CTNTYPE", prmCtnType)
            .Add("@CTNNO", prmCtnNo)
            .Add("@DELFLG", C_DELETE_FLG.ALIVE)
        End With

        'SQL実行
        CS0050SESSION.GetDataTable(sqlCon, sqlText.ToString, sqlParam, sqlRetSet, sqlTran)

        If sqlRetSet.Rows.Count > 0 Then
            refCONFIRMFLG = GetStringValue(sqlRetSet, 0, "CONFIRMFLG")
            refINVOICEORGCODE = GetStringValue(sqlRetSet, 0, "INVOICEORGCODE")
            refDISPOSALFLG = GetStringValue(sqlRetSet, 0, "DISPOSALFLG")
        End If

        refCnt = sqlRetSet.Rows.Count

    End Sub

    ''' <summary>
    ''' 現況表テーブル 更新処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htWKData">在庫データ</param>
    ''' <remarks>現況表テーブルを更新する</remarks>
    Public Shared Sub UpdatePresenttateData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                   ByVal htWKData As Hashtable)

        '◯在庫テーブル
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("UPDATE LNG.LNT0021_PRESENTSTATE ")
        sqlDetailStat.AppendLine("SET")
        sqlDetailStat.AppendLine("    CONTSTATUS      = @CONTSTATUS")      'コンテナ状態
        '更新情報
        sqlDetailStat.AppendLine("  , UPDYMD          = @UPDYMD")          '更新年月日
        sqlDetailStat.AppendLine("  , UPDUSER         = @UPDUSER")         '更新ユーザーＩＤ
        sqlDetailStat.AppendLine("  , UPDTERMID       = @UPDTERMID")       '更新端末
        sqlDetailStat.AppendLine("WHERE")
        sqlDetailStat.AppendLine("    CTNTYPE = @CTNTYPE")                 'コンテナ記号
        sqlDetailStat.AppendLine("AND CTNNO = @CTNNO")                     'コンテナ番号

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = htWKData(ZAIKO_DP.CS_CTNTYPE) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = htWKData(ZAIKO_DP.CS_CTNNO)          'コンテナ番号
                .Add("CONTSTATUS", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_STOCKSTATUS)) 'コンテナ状態
                '更新情報
                .Add("UPDYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_UPDYMD))        '更新年月日
                .Add("UPDUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_UPDUSER))      '更新ユーザーＩＤ
                .Add("UPDTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_UPDTERMID))  '更新端末
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' コンテナステータス履歴ファイル 登録処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htWKData">画面の明細データ</param>
    ''' <remarks>コンテナ在庫テーブルを登録する</remarks>
    Public Shared Sub InsertCtnStatusData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                            ByVal htWKData As Hashtable)

        Dim WW_DATENOW As DateTime = Date.Now

        '◯コンテナ在庫テーブル
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("INSERT INTO LNG.LNT0085_CTNSTATUS (")
        sqlDetailStat.AppendLine("    CTNTYPE")            'コンテナ形式
        sqlDetailStat.AppendLine("  , CTNNO")              'コンテナ番号
        sqlDetailStat.AppendLine("  , DATADATE")           'データ年月日
        sqlDetailStat.AppendLine("  , DATATIME")           'データ時刻
        sqlDetailStat.AppendLine("  , ARRSTATION")         '着駅コード
        sqlDetailStat.AppendLine("  , CONTSTATUS")         'コンテナ状態
        sqlDetailStat.AppendLine("  , DELFLG")             '削除フラグ
        sqlDetailStat.AppendLine("  , INITYMD")            '登録年月日
        sqlDetailStat.AppendLine("  , INITUSER")           '登録ユーザーＩＤ
        sqlDetailStat.AppendLine("  , INITTERMID")         '登録端末
        sqlDetailStat.AppendLine(")")
        sqlDetailStat.AppendLine(" VALUES(")
        sqlDetailStat.AppendLine("    @CTNTYPE")            'コンテナ形式
        sqlDetailStat.AppendLine("  , @CTNNO")              'コンテナ番号
        sqlDetailStat.AppendLine("  , @DATADATE")           'データ年月日
        sqlDetailStat.AppendLine("  , @DATATIME")           'データ時刻
        sqlDetailStat.AppendLine("  , @ARRSTATION")         '着駅コード
        sqlDetailStat.AppendLine("  , @CONTSTATUS")         'コンテナ状態
        sqlDetailStat.AppendLine("  , @DELFLG")             '削除フラグ
        sqlDetailStat.AppendLine("  , @INITYMD")            '登録年月日
        sqlDetailStat.AppendLine("  , @INITUSER")           '登録ユーザーＩＤ
        sqlDetailStat.AppendLine("  , @INITTERMID")         '登録端末
        sqlDetailStat.AppendLine(")")

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = htWKData(ZAIKO_DP.CS_CTNTYPE) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = htWKData(ZAIKO_DP.CS_CTNNO)          'コンテナ番号
                .Add("DATADATE", MySqlDbType.Date).Value = Now.Date                         'データ年月日
                .Add("DATATIME", MySqlDbType.DateTime).Value = WW_DATENOW                   'データ時刻
                .Add("ARRSTATION", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_STATIONCODE))      '現在駅
                .Add("CONTSTATUS", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_STOCKSTATUS)) '在庫状態
                '登録情報
                .Add("DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE                                            '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_INITYMD))         '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_INITUSER))       '登録ユーザーＩＤ
                .Add("INITTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_INITTERMID))   '登録端末
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' 在庫テーブル 物理削除処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htWKData">リース適用データ</param>
    ''' <remarks>在庫テーブルを更新する</remarks>
    Public Shared Sub DeleteCtnStockData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                   ByVal htWKData As Hashtable)

        '◯在庫テーブル
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("DELETE FROM LNG.LNT0089_CONTAINER_STOCK ")
        '条件
        sqlDetailStat.AppendLine("WHERE")
        sqlDetailStat.AppendLine("    KEIJOYM = @KEIJOYM")                 '計上年月
        sqlDetailStat.AppendLine("AND CTNTYPE = @CTNTYPE")                 'コンテナ記号
        sqlDetailStat.AppendLine("AND CTNNO = @CTNNO")                     'コンテナ番号

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("KEIJOYM", MySqlDbType.Int32).Value = htWKData(ZAIKO_DP.CS_KEIJOYM)      '計上年月
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = htWKData(ZAIKO_DP.CS_CTNTYPE) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = htWKData(ZAIKO_DP.CS_CTNNO)          'コンテナ番号
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 在庫テーブル 登録処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="htWKData">画面の明細データ</param>
    ''' <remarks>コンテナ在庫テーブルを登録する</remarks>
    Public Shared Sub InsertCtnStockData(ByVal sqlCon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                            ByVal htWKData As Hashtable)

        '◯コンテナ在庫テーブル
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("INSERT INTO LNG.LNT0089_CONTAINER_STOCK (")
        sqlDetailStat.AppendLine("    KEIJOYM")                 '計上年月
        sqlDetailStat.AppendLine("  , CTNTYPE")                 'コンテナ形式
        sqlDetailStat.AppendLine("  , CTNNO")                   'コンテナ番号
        sqlDetailStat.AppendLine("  , STOCKBRANCHCODE")         '在庫管理支店
        sqlDetailStat.AppendLine("  , BRANCHCODE")              '原価計上支店
        sqlDetailStat.AppendLine("  , STATIONCODE")             '現在駅
        sqlDetailStat.AppendLine("  , STOCKSTATUS")             '在庫状態
        sqlDetailStat.AppendLine("  , STOCKREGISTRATIONDATE")   '在庫登録日
        sqlDetailStat.AppendLine("  , EXCEPTIONDATE")           '運用除外日
        sqlDetailStat.AppendLine("  , STOCKREGISTRATID")        '在庫登録者
        sqlDetailStat.AppendLine("  , DISPOSALFLG")             '在庫処分フラグ
        sqlDetailStat.AppendLine("  , DELFLG")             '削除フラグ
        sqlDetailStat.AppendLine("  , INITYMD")            '登録年月日
        sqlDetailStat.AppendLine("  , INITUSER")           '登録ユーザーＩＤ
        sqlDetailStat.AppendLine("  , INITTERMID")         '登録端末
        sqlDetailStat.AppendLine("  , INITPGID")           '登録プログラムＩＤ
        sqlDetailStat.AppendLine(")")
        sqlDetailStat.AppendLine(" VALUES(")
        sqlDetailStat.AppendLine("    @KEIJOYM")                 '計上年月
        sqlDetailStat.AppendLine("  , @CTNTYPE")                 'コンテナ形式
        sqlDetailStat.AppendLine("  , @CTNNO")                   'コンテナ番号
        sqlDetailStat.AppendLine("  , @STOCKBRANCHCODE")         '在庫管理支店
        sqlDetailStat.AppendLine("  , @BRANCHCODE")              '原価計上支店
        sqlDetailStat.AppendLine("  , @STATIONCODE")             '現在駅
        sqlDetailStat.AppendLine("  , @STOCKSTATUS")             '在庫状態
        sqlDetailStat.AppendLine("  , @STOCKREGISTRATIONDATE")   '在庫登録日
        sqlDetailStat.AppendLine("  , @EXCEPTIONDATE")           '運用除外日
        sqlDetailStat.AppendLine("  , @STOCKREGISTRATID")        '在庫登録者
        sqlDetailStat.AppendLine("  , @DISPOSALFLG")             '在庫処分フラグ
        sqlDetailStat.AppendLine("  , @DELFLG")             '削除フラグ
        sqlDetailStat.AppendLine("  , @INITYMD")            '登録年月日
        sqlDetailStat.AppendLine("  , @INITUSER")           '登録ユーザーＩＤ
        sqlDetailStat.AppendLine("  , @INITTERMID")         '登録端末
        sqlDetailStat.AppendLine("  , @INITPGID")           '登録プログラムＩＤ
        sqlDetailStat.AppendLine(")")

        Using sqlDetailCmd As New MySqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("KEIJOYM", MySqlDbType.Int32).Value = htWKData(ZAIKO_DP.CS_KEIJOYM)      '計上年月
                .Add("CTNTYPE", MySqlDbType.VarChar).Value = htWKData(ZAIKO_DP.CS_CTNTYPE) 'コンテナ形式
                .Add("CTNNO", MySqlDbType.Int32).Value = htWKData(ZAIKO_DP.CS_CTNNO)          'コンテナ番号
                .Add("STOCKBRANCHCODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_INVOICEKEIJYOBRANCHCODE))   '在庫管理支店
                .Add("BRANCHCODE", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_INVOICEKEIJYOBRANCHCODE))   '原価計上支店
                .Add("STATIONCODE", MySqlDbType.Int32).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_STATIONCODE))      '現在駅
                .Add("STOCKSTATUS", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_STOCKSTATUS)) '在庫状態
                .Add("STOCKREGISTRATIONDATE", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_STOCKREGISTRATIONDATE)) '在庫登録日
                .Add("EXCEPTIONDATE", MySqlDbType.Date).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_EXCEPTIONDATE)) '運用除外日
                .Add("STOCKREGISTRATID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_STOCKREGISTRATID))  '在庫登録者
                .Add("DISPOSALFLG", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_DISPOSALFLG)) '在庫処分フラグ
                '登録情報
                .Add("DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE                                            '削除フラグ
                .Add("INITYMD", MySqlDbType.DateTime).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_INITYMD))         '登録年月日
                .Add("INITUSER", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_INITUSER))       '登録ユーザーＩＤ
                .Add("INITTERMID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_INITTERMID))   '登録端末
                .Add("INITPGID", MySqlDbType.VarChar).Value = CmnSetFmt.ObjToDbNull(htWKData(ZAIKO_DP.CS_INITPGID))       '登録プログラムＩＤ
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

End Class