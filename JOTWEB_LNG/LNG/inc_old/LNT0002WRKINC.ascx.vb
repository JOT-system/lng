Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0002WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNT0002S"       'MAPID(条件)
    Public Const MAPIDL As String = "LNT0002L"       'MAPID(実行)
    Public Const MAPIDC As String = "LNT0002C"       'MAPID(更新)

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    '' <summary>
    '' 固定値マスタから一覧の取得
    '' </summary>
    '' <param name="COMPCODE"></param>
    '' <param name="FIXCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Function CreateFIXParam(ByVal I_COMPCODE As String, Optional ByVal I_FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_FIXCODE
        CreateFIXParam = prmData
    End Function

    '' <summary>
    '' 運用部署パラメーター
    '' </summary>
    '' <param name="I_COMPCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Public Function CreateORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        Return prmData
    End Function

    ''' <summary>
    ''' コンテナ種別の初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetNewDisplayFlags() As List(Of DisplayFlag)
        Dim retVal As New List(Of DisplayFlag)
        retVal.Add(New DisplayFlag(True, "通風", "BIGCTN00", 0, "05"))
        retVal.Add(New DisplayFlag(True, "冷蔵", "BIGCTN01", 1, "10"))
        retVal.Add(New DisplayFlag(True, "ｽｰﾊﾟｰUR", "BIGCTN02", 2, "11"))
        retVal.Add(New DisplayFlag(True, "冷凍", "BIGCTN03", 3, "15"))
        retVal.Add(New DisplayFlag(True, "L10屯", "BIGCTN04", 4, "20"))
        retVal.Add(New DisplayFlag(True, "ウイング", "BIGCTN05", 5, "25"))
        retVal.Add(New DisplayFlag(True, "有蓋", "BIGCTN06", 6, "30"))
        retVal.Add(New DisplayFlag(True, "無蓋", "BIGCTN07", 7, "35"))
        Return retVal
    End Function

    ''' <summary>
    ''' 経理資産区分の初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetNewDisplayFlags2() As List(Of DisplayFlag)
        Dim retVal As New List(Of DisplayFlag)
        retVal.Add(New DisplayFlag(True, "レンタル", "ACCKBN00", 0, "01"))
        retVal.Add(New DisplayFlag(False, "リース", "ACCKBN01", 1, "02"))
        Return retVal
    End Function

    ''' <summary>
    ''' 扉形式の初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetNewDisplayFlags3() As List(Of DisplayFlag)
        Dim retVal As New List(Of DisplayFlag)
        retVal.Add(New DisplayFlag(True, "L　字", "ADDITEM01", 0, "01"))
        retVal.Add(New DisplayFlag(True, "両開き", "ADDITEM02", 1, "02"))
        Return retVal
    End Function

    ''' <summary>
    ''' 駅マスタから一覧の取得
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
    ''' リストアイテムを受け渡し用にエンコードする
    ''' </summary>
    ''' <param name="dispFlags"></param>
    ''' <returns></returns>
    Public Function EncodeDisplayFlags(dispFlags As List(Of DisplayFlag)) As String
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim base64Str As String = ""
        Dim noCompressionByte As Byte()
        'クラスをシリアライズ
        Using ms As New IO.MemoryStream()
            formatter.Serialize(ms, dispFlags)
            noCompressionByte = ms.ToArray
        End Using

        '圧縮シリアライズしたByteデータを圧縮し圧縮したByteデータをBase64に変換
        Using ms As New IO.MemoryStream(),
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress, True)
            ds.Write(noCompressionByte, 0, noCompressionByte.Length)
            ds.Close()
            Dim byteDat = ms.ToArray
            base64Str = Convert.ToBase64String(byteDat, 0, byteDat.Length, Base64FormattingOptions.None)
        End Using
        Return base64Str
    End Function

    ''' <summary>
    ''' リストアイテムを受け渡し用にエンコードする
    ''' </summary>
    ''' <param name="base64Str">base64エンコードした文字列</param>
    ''' <returns></returns>
    Public Function DecodeDisplayFlags(base64Str As String) As List(Of DisplayFlag)
        Dim retVal As List(Of DisplayFlag)
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim compressedByte As Byte()
        compressedByte = Convert.FromBase64String(base64Str)
        '取得した文字をByte化し解凍、画面利用クラスに再格納
        Using inpMs As New IO.MemoryStream(compressedByte),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            outMs.Position = 0
            retVal = DirectCast(formatter.Deserialize(outMs), List(Of DisplayFlag))
        End Using
        Return retVal
    End Function

    ''' <summary>
    ''' チェックボックスの状態をフラグリストに設定
    ''' </summary>
    ''' <param name="chklObj"></param>
    ''' <param name="dispFlags"></param>
    ''' <returns></returns>
    Public Function SetSelectedDispFlags(chklObj As CheckBoxList, dispFlags As List(Of DisplayFlag)) As List(Of DisplayFlag)

        Dim chkFieldNames As New List(Of String)
        Dim qSelectedChk = From chkitm In chklObj.Items.Cast(Of ListItem) Where chkitm.Selected Select chkitm.Value

        If qSelectedChk.Any Then
            chkFieldNames = qSelectedChk.ToList
        End If

        Dim retObj = dispFlags

        For Each retItm In retObj
            retItm.Checked = False
            If chkFieldNames.Contains(retItm.FieldName) Then
                retItm.Checked = True
            End If
        Next

        Return retObj

    End Function

    ''' <summary>
    ''' コンテナ種別関連クラス
    ''' </summary>
    <Serializable>
    Public Class DisplayFlag
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="checked">チェック</param>
        ''' <param name="dispName">画面表示名</param>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispOrder">並び順</param>
        Public Sub New(checked As Boolean, dispName As String, fieldName As String, dispOrder As Integer, selectCode As String)
            Me.Checked = checked
            Me.DispName = dispName
            Me.FieldName = fieldName
            Me.DispOrder = dispOrder
            Me.selectCode = selectCode
        End Sub
        ''' <summary>
        ''' 表示名
        ''' </summary>
        ''' <returns></returns>
        Public Property DispName As String
        ''' <summary>
        ''' 対象フィールド
        ''' </summary>
        ''' <returns></returns>
        Public Property FieldName As String
        ''' <summary>
        ''' 表示順
        ''' </summary>
        ''' <returns></returns>
        Public Property DispOrder As Integer
        ''' <summary>
        ''' 表示グループ（仮）
        ''' </summary>
        ''' <returns></returns>
        Public Property Group As String = ""
        ''' <summary>
        ''' 選択フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property Checked As Boolean
        ''' <summary>
        ''' 選択コード
        ''' </summary>
        ''' <returns></returns>
        Public Property selectCode As String
    End Class

End Class
