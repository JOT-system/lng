Option Strict On
Imports MySQL.Data.MySqlClient
''' <summary>
''' 共通マルチ/単体 選択コントロール
''' </summary>
Public Class GRC0002SELECTIONPOPUPWORKINC
    Inherits System.Web.UI.UserControl
    ''' <summary>
    ''' 当アイテム情報を保持するクラス
    ''' </summary>
    Private mControlItem As ControlItemData
    Private Const KEYHASHTABLEITM As String = "GRC0002KEY"
    Private Const CHKHASHTABLEITM As String = "GRC0002CHK"
    ''' <summary>
    ''' 画面選択モード(Multiple:複数選択/Single:単体選択)
    ''' </summary>
    ''' <returns></returns>
    Public Property SelectionMode As ListSelectionMode
        Get
            Return Me.mControlItem.SelectionMode
        End Get
        Set(value As ListSelectionMode)
            Me.mControlItem.SelectionMode = value
        End Set
    End Property
    ''' <summary>
    ''' 抽出SQLのプロパティ
    ''' </summary>
    ''' <returns></returns>
    Public Property SQL As String
        Get
            Return Me.mControlItem.SQL
        End Get
        Set(value As String)
            Me.mControlItem.SQL = value
        End Set
    End Property
    ''' <summary>
    ''' SQLパラメーター指定すれば抽出時に加えます(new GRC0002SELECTIONPOPUPWORKINC.SQLParamItem("パラメータ名","型","設定値")で追加
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLParam As List(Of SQLParamItem)
        Get
            Return Me.mControlItem.SQLParams
        End Get
        Set(value As List(Of SQLParamItem))
            Me.mControlItem.SQLParams = value
        End Set
    End Property
    ''' <summary>
    ''' SQLのユニークになるフィールドで除外するキーの値を設定
    ''' </summary>
    ''' <returns></returns>
    Public Property ExcludeKeys As List(Of String)
        Get
            Return Me.mControlItem.ExcludeKeys
        End Get
        Set(value As List(Of String))
            Me.mControlItem.ExcludeKeys = value
        End Set
    End Property
    ''' <summary>
    ''' 画面に表示するSQL取得結果のフィールドを定義
    ''' </summary>
    ''' <returns></returns>
    Public Property DispFieldList As List(Of DispFieldItem)
        Get
            Return Me.mControlItem.DispFieldList
        End Get
        Set(value As List(Of DispFieldItem))
            Me.mControlItem.DispFieldList = value
        End Set
    End Property
    ''' <summary>
    ''' SQLの抽出結果でユニークになるフィールド名を設定
    ''' </summary>
    ''' <returns></returns>
    Public Property KeyFieldName As String
        Get
            Return Me.mControlItem.KeyFieldName
        End Get
        Set(value As String)
            Me.mControlItem.KeyFieldName = value
        End Set
    End Property
    ''' <summary>
    ''' 当画面の追加ボタンの文言（デフォルト「追加」）
    ''' </summary>
    ''' <returns></returns>
    Public Property AddButtonDispName As String
        Get
            Return Me.mControlItem.AddButtonDispName
        End Get
        Set(value As String)
            Me.mControlItem.AddButtonDispName = value
        End Set
    End Property
    ''' <summary>
    ''' フィルタードロップダウンを表示するときに追加（キーフィルタ対象のフィールド名,表示文言）
    ''' </summary>
    ''' <returns></returns>
    Public Property FilterField As Dictionary(Of String, String)
        Get
            Return Me.mControlItem.FilterField
        End Get
        Set(value As Dictionary(Of String, String))
            Me.mControlItem.FilterField = value
        End Set
    End Property
    ''' <summary>
    ''' SelectionModeがSingleの時に選択した値を返却
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property SelectedSingleKey As String
        Get
            Return Me.hdnGrc0002SelectedUniqueKey.Value
        End Get
    End Property
    ''' <summary>
    ''' SelectionModeがSingleの時に選択したDBの1行の情報を返却
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property SelectedSingleItem As Hashtable
        Get
            Dim key As String = Me.hdnGrc0002SelectedUniqueKey.Value
            Dim qSelectItm = From itm In Me.mControlItem.ListData Where itm.Key = key Select itm.Value
            If qSelectItm.Any = False Then
                Return Nothing
            End If
            Return qSelectItm.FirstOrDefault
        End Get
    End Property
    ''' <summary>
    ''' SelectionModeがMultipleの時に選択されたキー一覧を取得
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property SelectedKeys As List(Of String)
        Get
            Dim qSelectedKeys = From itm In Me.mControlItem.ListData Where Convert.ToBoolean(itm.Value(CHKHASHTABLEITM)) = True Select itm.Key

            If qSelectedKeys.Any = False Then
                Return Nothing
            End If

            Return qSelectedKeys.ToList
        End Get
    End Property
    ''' <summary>
    ''' SelectionModeがMultipleの時に選択されたキーを元にDB取得結果を返却
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property SelectedItems As List(Of Hashtable)
        Get
            Me.CollectRepeaterCheckVal()
            Dim qSelectedItems = From itm In Me.mControlItem.ListData Where Convert.ToBoolean(itm.Value(CHKHASHTABLEITM)) = True Select itm.Value

            If qSelectedItems.Any = False Then
                Return Nothing
            End If

            Return qSelectedItems.ToList
        End Get
    End Property
    ''' <summary>
    ''' フィルタ表示度合を返却
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property FilterLevel As String
        Get
            If Me.mControlItem.FilterField Is Nothing OrElse Me.mControlItem.FilterField.Count = 0 Then
                Return "filterLevel0"
            ElseIf Me.mControlItem.FilterField.Count = 1 Then
                Return "filterLevel1"
            Else
                Return "filterLevel2"
            End If
        End Get
    End Property
    ''' <summary>
    ''' VB6でいうところのTagプロパティ(このユーザーコントロールクラスでは未使用）
    ''' 当ポップアップを使いまわす際、どの項目か印をつけておきたい場合に使用してください。
    ''' </summary>
    ''' <returns></returns>
    Public Property Tag As String
        Get
            Return Me.mControlItem.Tag
        End Get
        Set(value As String)
            Me.mControlItem.Tag = value
        End Set
    End Property

    ''' <summary>
    ''' ページ初期化
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub GRC0002SELECTIONPOPUPWORKINC_Init(sender As Object, e As EventArgs) Handles Me.Init

        Try
            If Me.IsPostBack = False Then
                'mControlItemの復元はLoadViewStateで行われる
                'ページ初期化ではViewStateの復元は行われないので
                'Loadイベント以降で行う事
                'ページInit → 当コントロールInit → ページLoad →　当コントロールロード
                Me.mControlItem = New ControlItemData
            End If
        Catch ex As Exception
            Throw
        Finally

        End Try
    End Sub


    ''' <summary>
    ''' ページロード時
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack = True Then
            '一覧のチェック状態を取得
            CollectRepeaterCheckVal()



            'リピーターの再設定
            If Me.hdnShowHideGrc0002.Value <> "0" Then
                SetRepeater()
            ElseIf Me.hdnShowHideGrc0002.Value = "0" Then
                Me.mControlItem.ListData = Nothing
                Me.repGrc0002ListHeader.DataSource = Nothing
                Me.repGrc0002ListHeader.DataBind()
                Me.repGrc0002SelectListRow.DataSource = Nothing
                Me.repGrc0002SelectListRow.DataBind()
            End If

        End If
    End Sub
    ''' <summary>
    ''' ポップアップ表示
    ''' </summary>
    Public Sub ShowPopUpList(Optional ByVal SearchKey As String = "")
        '必須プロパティを設定指定ない場合例外スロー
        If Me.mControlItem.SQL = "" OrElse
           Me.mControlItem.DispFieldList Is Nothing OrElse
           Me.mControlItem.DispFieldList.Count = 0 OrElse
           Me.mControlItem.KeyFieldName = "" Then
            'プロパティ設定不足の場合は例外をスロー
            Throw New Exception("必須のプロパティが設定されていません。GRC0002SELECTIONPOPUPWORKINC")
        End If
        'ポップアップを開いた直後のユーザー入力欄はクリア
        Me.hdnGrc0002SelectedUniqueKey.Value = ""
        Me.txtGrc0002TextSearch.Text = SearchKey
        Me.ddlGrc0002Filter1.Items.Clear()
        Me.ddlGrc0002Filter2.Items.Clear()
        Me.pnlGrc0002FilterDdl1.Visible = False
        Me.pnlGrc0002FilterDdl2.Visible = False

        '選択モードの設定
        If Me.mControlItem.SelectionMode = ListSelectionMode.Multiple Then
            Me.hdnShowHideGrc0002.Value = "1"
        Else
            Me.hdnShowHideGrc0002.Value = "2"
        End If
        'データを取得
        Me.mControlItem.ListData = Me.GetListData()

        'フィルタモード(3つ以上入れても無視)
        If Me.mControlItem.FilterField IsNot Nothing Then
            Dim idx = 0
            For Each dicItm In Me.mControlItem.FilterField
                If idx >= 2 Then
                    Exit For
                End If
                Dim fieldName = dicItm.Key
                Dim fieldText = dicItm.Value
                Dim qfilterList = From itm In Me.mControlItem.ListData.Values Where Convert.ToString(itm(fieldName)) <> "" Select listItm = Convert.ToString(itm(fieldName)) Group By listItm Into Group Select listItm

                Dim filterList As New List(Of String)
                If qfilterList.Any Then
                    filterList = qfilterList.ToList
                End If
                If idx = 0 Then
                    Me.ddlGrc0002Filter1.Items.Add(New ListItem("", ""))
                    For Each itm In filterList
                        Me.ddlGrc0002Filter1.Items.Add(New ListItem(itm, itm))
                    Next
                    Me.lblGrc0002FilterColName1.Text = dicItm.Value
                    Me.pnlGrc0002FilterDdl1.Visible = True
                Else
                    Me.ddlGrc0002Filter2.Items.Add(New ListItem("", ""))
                    For Each itm In filterList
                        Me.ddlGrc0002Filter2.Items.Add(New ListItem(itm, itm))
                    Next
                    Me.lblGrc0002FilterColName2.Text = dicItm.Value
                    Me.pnlGrc0002FilterDdl2.Visible = True
                End If

                idx = idx + 1
            Next


        End If

    End Sub
    ''' <summary>
    ''' リピーターのチェック状況を取得
    ''' </summary>
    Private Sub CollectRepeaterCheckVal()
        '複数選択以外はやる意味がないので終了
        If Me.hdnShowHideGrc0002.Value <> "1" Then
            Return
        End If
        'データ一覧が無ければやる意味が無いので終了
        If Me.repGrc0002SelectListRow.Items Is Nothing OrElse
           Me.repGrc0002SelectListRow.Items.Count = 0 Then
            Return
        End If
        'リピーターの行ループ
        Dim hdnKeyVal As HiddenField
        Dim chkObj As CheckBox
        For Each repItm As RepeaterItem In Me.repGrc0002SelectListRow.Items
            If Not (repItm.ItemType = ListItemType.AlternatingItem OrElse
                    repItm.ItemType = ListItemType.Item) Then
                Continue For
            End If

            hdnKeyVal = DirectCast(repItm.FindControl("hdnGrc0002KeyValue"), HiddenField)
            chkObj = DirectCast(repItm.FindControl("chkGrc0002InsideList"), CheckBox)

            If hdnKeyVal Is Nothing OrElse chkObj Is Nothing Then
                Continue For
            End If

            Dim keyVal As String = hdnKeyVal.Value
            Dim listItm As DispListItemData
            listItm = Nothing
            If Me.mControlItem.ListData.ContainsKey(keyVal) Then
                With Me.mControlItem.ListData(keyVal)
                    .Item(CHKHASHTABLEITM) = chkObj.Checked
                End With
            End If
        Next repItm


    End Sub
    ''' <summary>
    ''' 一覧表にデータ展開
    ''' </summary>
    Private Sub SetRepeater()
        If Me.hdnShowHideGrc0002.Value = "0" Then
            Return
        End If

        Dim dispList As New List(Of DispListRowItemData)
        Dim dispColItem As New List(Of DispListItemData)
        Dim dispItem As DispListItemData = Nothing
        '一覧表に展開(ヘッダー)
        Me.repGrc0002ListHeader.DataSource = Me.mControlItem.DispFieldList
        Me.repGrc0002ListHeader.DataBind()
        '一覧表に展開(データ)
        Dim isDispItem(2) As Boolean
        For Each item In Me.mControlItem.ListData
            isDispItem(0) = False '絞り込みドロップダウン1マッチ
            isDispItem(1) = False '絞り込みドロップダウン2マッチ
            isDispItem(2) = False 'ワード検索マッチ
            'フィルタドロップダウンとのマッチング
            If Me.mControlItem.FilterField IsNot Nothing AndAlso Me.mControlItem.FilterField.Count > 0 Then
                Dim idx As Integer = 0
                For Each filterItm In Me.mControlItem.FilterField
                    If idx >= 2 Then
                        Return
                    End If
                    Dim filterdValue As String = ""
                    If idx = 0 Then
                        If Me.ddlGrc0002Filter1.SelectedIndex >= 0 Then
                            filterdValue = Me.ddlGrc0002Filter1.SelectedValue
                        End If
                    Else
                        If Me.ddlGrc0002Filter2.SelectedIndex >= 0 Then
                            filterdValue = Me.ddlGrc0002Filter2.SelectedValue
                        End If
                    End If
                    If filterdValue = "" Then
                        isDispItem(idx) = True
                    ElseIf filterdValue = Convert.ToString(item.Value(filterItm.Key)) Then
                        isDispItem(idx) = True
                    End If
                    idx = idx + 1
                Next
            Else
                isDispItem(0) = True
                isDispItem(1) = True
            End If
            If Me.mControlItem.FilterField IsNot Nothing AndAlso Me.mControlItem.FilterField.Count = 1 Then
                isDispItem(1) = True
            End If
            '表示一覧の中でヒットする文字があれば絞り込み対象
            For Each fields In Me.mControlItem.DispFieldList
                If Me.txtGrc0002TextSearch.Text = "" Then
                    isDispItem(2) = True
                    Exit For
                Else
                    If Strings.StrConv(Convert.ToString(item.Value(fields.FieldName)), VbStrConv.Uppercase Or VbStrConv.Wide).Contains(Strings.StrConv(Me.txtGrc0002TextSearch.Text, VbStrConv.Uppercase Or VbStrConv.Wide)) Then
                        isDispItem(2) = True
                        Exit For
                    End If
                End If
            Next
            'フィルタ合致
            If isDispItem(0) AndAlso isDispItem(1) AndAlso isDispItem(2) Then
                Dim rowItm As DispListRowItemData
                rowItm = New DispListRowItemData
                dispColItem = New List(Of DispListItemData)
                'チェックボックス設定
                rowItm.ChkVal = DirectCast(item.Value(CHKHASHTABLEITM), Boolean)
                'キー値設定
                rowItm.KeyVal = DirectCast(item.Value(KEYHASHTABLEITM), String)
                For Each fields In Me.mControlItem.DispFieldList
                    dispItem = New DispListItemData
                    dispItem.FieldName = fields.FieldName
                    dispItem.Value = item.Value(fields.FieldName)
                    dispItem.IsFixed = fields.FixedCol
                    dispItem.TextAlign = fields.TextAlign
                    dispColItem.Add(dispItem)
                Next
                rowItm.ColList = dispColItem
                dispList.Add(rowItm)
            End If
        Next
        'データ部分のリピーターにバインド
        Me.repGrc0002SelectListRow.DataSource = dispList
        Me.repGrc0002SelectListRow.DataBind()
        '動的カラムのスタイル書き出し
        Dim styleStr As New StringBuilder
        Dim totalWidth As Integer = 0
        Dim totalFixWidth As Integer = 0
        If Me.hdnShowHideGrc0002.Value = "1" Then
            totalWidth = 50 'チェックボックスの長さ50
            totalFixWidth = 50
        End If
        styleStr.AppendLine("<style>")
        For Each fieldSetting In Me.mControlItem.DispFieldList
            styleStr.AppendFormat("#{0}_divGrc0002InputList div.{1} {{", Me.ID, fieldSetting.FieldName).AppendLine()
            styleStr.AppendFormat("    width:{0}px;", fieldSetting.Size).AppendLine()
            styleStr.AppendFormat("}}").AppendLine()
            styleStr.AppendFormat("#{0}_divGrc0002InputList div.{1}.data {{", Me.ID, fieldSetting.FieldName).AppendLine()
            styleStr.AppendFormat("    text-align:{0};", fieldSetting.TextAlign).AppendLine()
            styleStr.AppendFormat("}}").AppendLine()
            If IsNumeric(fieldSetting.Size) Then
                totalWidth = totalWidth + CInt(fieldSetting.Size)
                If fieldSetting.FixedCol Then
                    styleStr.AppendFormat("#{0}_divGrc0002InputList div.{1}.fix {{", Me.ID, fieldSetting.FieldName).AppendLine()
                    styleStr.AppendFormat("    left:{0}px;", totalFixWidth).AppendLine()
                    styleStr.AppendFormat("}}").AppendLine()
                    totalFixWidth = totalFixWidth + CInt(fieldSetting.Size)
                End If
            End If
        Next
        styleStr.AppendFormat("#{0}_divGrc0002InputList .grc0002selectheaderrow,", Me.ID).AppendLine()
        styleStr.AppendFormat("#{0}_divGrc0002InputList .grc0002selectdatarow {{", Me.ID).AppendLine()
        styleStr.AppendFormat(" width:{0}px;", totalWidth).AppendLine()
        styleStr.AppendFormat("}}").AppendLine()
        styleStr.AppendLine("</style>")
        Me.letGrc0002Style.Text = styleStr.ToString
    End Sub
    ''' <summary>
    ''' ポップアップ非表示
    ''' </summary>
    Public Sub HidePopUp()
        Me.hdnShowHideGrc0002.Value = "0"
    End Sub

    ''' <summary>
    ''' 検索条件初期設定
    ''' </summary>
    Public Sub ddlFilterInit(Optional ByVal textVal As String = "", Optional ByVal filter1Val As String = "", Optional ByVal filter2Val As String = "")
        '文字検索に設定値がある場合
        If textVal <> "" Then
            Me.txtGrc0002TextSearch.Text = textVal
        End If

        'フィルター1に設定値がある場合
        If filter1Val <> "" Then
            If Me.ddlGrc0002Filter1.Items.FindByText(filter1Val) IsNot Nothing Then
                Me.ddlGrc0002Filter1.SelectedValue = filter1Val
            End If
        End If

        'フィルター2に設定値がある場合
        If filter2Val <> "" Then
            If Me.ddlGrc0002Filter2.Items.FindByText(filter2Val) IsNot Nothing Then
                Me.ddlGrc0002Filter2.SelectedValue = filter2Val
            End If
        End If
    End Sub

    ''' <summary>
    ''' 全ての設定を初期状態に戻す
    ''' </summary>
    Public Sub InitPopUp()
        Me.hdnShowHideGrc0002.Value = "0"
        Me.hdnGrc0002SelectedUniqueKey.Value = ""
        Me.txtGrc0002TextSearch.Text = ""
        Me.ddlGrc0002Filter1.Items.Clear()
        Me.ddlGrc0002Filter2.Items.Clear()
        Me.pnlGrc0002FilterDdl1.Visible = False
        Me.pnlGrc0002FilterDdl2.Visible = False
        Me.mControlItem = New ControlItemData
        Me.repGrc0002ListHeader.DataSource = Nothing
        Me.repGrc0002ListHeader.DataBind()
        Me.repGrc0002SelectListRow.DataSource = Nothing
        Me.repGrc0002SelectListRow.DataBind()
    End Sub

    ''' <summary>
    ''' 当コントロールのSQL、SQLParamを元にデータを取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetListData() As Dictionary(Of String, Hashtable)
        Dim CS0050SESSION As New CS0050SESSION
        Dim retVal As New Dictionary(Of String, Hashtable)
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            MySqlConnection.ClearPool(SQLcon)
            Using sqlCmd As New MySqlCommand(Me.SQL, SQLcon)
                'SQLパラメータの追加

                If Me.SQLParam IsNot Nothing AndAlso Me.SQLParam.Count > 0 Then
                    With sqlCmd.Parameters
                        For Each sqlParamitm In Me.SQLParam
                            .Add(sqlParamitm.SqlParamName, sqlParamitm.SqlParamType).Value = sqlParamitm.SqlParamValue
                        Next
                    End With
                End If

                Dim retItem As Hashtable
                Dim val As String = ""
                Dim keyVal As String = ""
                Dim defaultChkVal As Boolean = False
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    '動的な取得結果のカラム名取得
                    Dim colList As New List(Of String)
                    For i = 0 To sqlDr.FieldCount - 1
                        colList.Add(sqlDr.GetName(i))
                    Next
                    '結果を読み込み
                    While sqlDr.Read
                        retItem = New Hashtable
                        'キー値の取得
                        keyVal = Convert.ToString(sqlDr(Me.KeyFieldName))
                        If Me.ExcludeKeys IsNot Nothing AndAlso
                           Me.ExcludeKeys.Contains(keyVal) Then
                            Continue While '除外キーが存在する場合スキップ
                        End If
                        retItem.Add(KEYHASHTABLEITM, keyVal)
                        'チェックボックスの状態設定
                        retItem.Add(CHKHASHTABLEITM, defaultChkVal)
                        '動的列の設定
                        For Each colName In colList
                            val = Convert.ToString(sqlDr(colName))
                            retItem.Add(colName, val)
                        Next
                        retVal.Add(keyVal, retItem)
                    End While
                End Using 'sqlDr
            End Using 'sqlCmd
        End Using 'SQLcon
        Return retVal
    End Function

    ''' <summary>
    ''' 当コントロールのプロパティ値を保持し圧縮したBase64形式に変換
    ''' </summary>
    ''' <returns></returns>
    Private Function GetThisControlItemValueToBase64(contData As ControlItemData) As String

        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim base64Str As String = ""
        Dim noConpressionByte As Byte()
        'クラスをシリアライズ
        Using ms As New IO.MemoryStream()
            formatter.Serialize(ms, contData)
            noConpressionByte = ms.ToArray
        End Using
        '圧縮シリアライズしたByteデータを圧縮し圧縮したByteデータをBase64に変換
        Using ms As New IO.MemoryStream(),
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress, True)
            ds.Write(noConpressionByte, 0, noConpressionByte.Length)
            ds.Close()
            Dim byteDat = ms.ToArray
            base64Str = Convert.ToBase64String(byteDat, 0, byteDat.Length, Base64FormattingOptions.None)
        End Using
        Return base64Str
    End Function
    ''' <summary>
    ''' Base64文字を当コントロールデータに置換
    ''' </summary>
    ''' <returns></returns>
    Private Function DecodeThisControlValues(base64String As String) As ControlItemData
        Dim retVal As ControlItemData
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim conmressedByte As Byte()
        conmressedByte = Convert.FromBase64String(base64String)
        '取得した文字をByte化し解凍、画面利用クラスに再格納
        Using inpMs As New IO.MemoryStream(conmressedByte),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            outMs.Position = 0
            retVal = DirectCast(formatter.Deserialize(outMs), ControlItemData)
        End Using
        Return retVal
    End Function


    ''' <summary>
    ''' 当コントロールで使用するデータ保持クラス
    ''' </summary>
    <Serializable>
    Public Class ControlItemData
        Public Sub New()
            Me.SelectionMode = ListSelectionMode.Multiple
            Me.SQL = ""
            Me.SQLParams = New List(Of SQLParamItem)
            Me.DispFieldList = New List(Of DispFieldItem)
            Me.KeyFieldName = ""
            Me.ListData = New Dictionary(Of String, Hashtable)
            Me.ExcludeKeys = New List(Of String)
            Me.AddButtonDispName = "追加"
            Me.FilterField = New Dictionary(Of String, String)
            Me.Tag = ""
        End Sub
        ''' <summary>
        ''' 追加ボタンの文言（デフォルト:追加)
        ''' </summary>
        ''' <returns></returns>
        Public Property AddButtonDispName As String
        ''' <summary>
        ''' 選択モード(Multiple:複数選択(デフォルト),Single:単体選択) 
        ''' </summary>
        ''' <returns></returns>
        Public Property SelectionMode As ListSelectionMode = ListSelectionMode.Multiple
        ''' <summary>
        ''' データ取得SQL(必須)
        ''' </summary>
        ''' <returns></returns>
        Public Property SQL As String
        ''' <summary>
        ''' SQLのパラメーター(任意[SQLにパラメータを使っているなら必須])(MySqlParameters.Add("@xxxxx",MySqlDbType.xxxx)でやっているパラメーター
        ''' </summary>
        ''' <returns></returns>
        Public Property SQLParams As List(Of SQLParamItem)

        ''' <summary>
        ''' 表示アイテム(必須)
        ''' </summary>
        ''' <returns></returns>
        Public Property DispFieldList As List(Of DispFieldItem)
        ''' <summary>
        ''' SQLでユニークになるフィールド名の指定(必須)
        ''' </summary>
        ''' <returns></returns>
        Public Property KeyFieldName As String
        ''' <summary>
        ''' SQLの抽出結果＋画面データ(返却)
        ''' </summary>
        ''' <returns></returns>
        Public Property ListData As Dictionary(Of String, Hashtable)
        ''' <summary>
        ''' 抽出結果より一覧から除外するキー値(任意)
        ''' </summary>
        ''' <returns></returns>
        Public Property ExcludeKeys As List(Of String)
        ''' <summary>
        ''' フィルタ設定フィールド(任意)(キー:フィルタを設定するフィールド,値:フィルタの文言)
        ''' </summary>
        ''' <returns></returns>
        Public Property FilterField As Dictionary(Of String, String)
        ''' <summary>
        ''' VB6でいうところのTagプロパティ(このユーザーコントロールクラスでは未使用）
        ''' 当ポップアップを使いまわす際、どのフィールドか印をつけておきたい場合に使用してください。
        ''' </summary>
        ''' <returns></returns>
        Public Property Tag As String
    End Class
    ''' <summary>
    ''' 画面表示フィールド定義
    ''' </summary>
    <Serializable>
    Public Class DispFieldItem
        ''' <summary>
        ''' 画面表示するSQLで設定したフィールド名
        ''' </summary>
        ''' <returns></returns>
        Public Property FieldName As String
        ''' <summary>
        ''' 項目名
        ''' </summary>
        ''' <returns></returns>
        Public Property DispName As String
        ''' <summary>
        ''' カラムサイズ(px)
        ''' </summary>
        ''' <returns></returns>
        Public Property Size As String
        ''' <summary>
        ''' 固定せる(True:固定,False:動的)
        ''' </summary>
        ''' <returns></returns>
        Public Property FixedCol As Boolean
        ''' <summary>
        ''' テキスト表示位置(Left,Center,Right)
        ''' </summary>
        ''' <returns></returns>
        Public Property TextAlign As String

        ''' <summary>
        ''' コンストラクタ(詳細設定版)
        ''' </summary>
        ''' <param name="FieldName">フィールド名</param>
        ''' <param name="DispName">表示名</param>
        ''' <param name="Size">カラム幅(単位px)</param>
        ''' <param name="textAlign">Left,Center,Right</param>
        ''' <param name="FixedCol">固定カラム</param>
        Public Sub New(fieldName As String, dispName As String, size As String, textAlign As String, fixedCol As Boolean)
            'どの引数のコンストラクタから来ても最終的にここに到達
            Me.FieldName = fieldName
            Me.DispName = dispName
            Me.FixedCol = fixedCol
            Me.TextAlign = textAlign

            If size = "" Then
                size = "100" 'ディクショナリにもなければ100(px)となる
                '引数のsizeが""の時はある程度フィールド名から推測して幅を決める
                '当ディクショナリに追加はご自由に
                'キー：フィールド名、値：サイズ(px)
                Dim FieldNameToColSize As New Dictionary(Of String, String) From
                    {{"CONTNUMBER", "140"}, {"DETENTIONNAME", "250"},
                     {"OTHERCOMP", "120"}}
                If FieldNameToColSize.ContainsKey(fieldName) Then
                    size = FieldNameToColSize(fieldName)
                End If
            End If
            Me.Size = size
        End Sub
        ''' <summary>
        ''' コンストラクタ(コンストラクタ固定セル無し版)
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispName">表示名</param>
        ''' <param name="size">カラム幅(単位px)</param>
        ''' <param name="fixedCol">固定カラム</param>
        Public Sub New(fieldName As String, dispName As String, size As String, fixedCol As Boolean)
            Me.New(fieldName, dispName, size, "left", fixedCol)
        End Sub
        ''' <summary>
        ''' コンストラクタ(コンストラクタ固定セル無し版)
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispName">表示名</param>
        ''' <param name="size">カラム幅(単位px)</param>
        Public Sub New(fieldName As String, dispName As String, size As String)
            Me.New(fieldName, dispName, size, False)
        End Sub
        ''' <summary>
        ''' コンストラクタ（サイズ未指定版 ）
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispName">表示名</param>
        ''' <param name="fixedCol">固定カラム</param>
        Public Sub New(fieldName As String, dispName As String, fixedCol As Boolean)
            Me.New(fieldName, dispName, "", fixedCol)
        End Sub
        ''' <summary>
        ''' コンストラクタ（サイズ未指定版）
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispName">表示名</param>
        ''' <param name="fixedCol">固定カラム</param>
        ''' <param name="textAlign">Left,Center,Right</param>
        Public Sub New(fieldName As String, dispName As String, fixedCol As Boolean, textAlign As String)
            Me.New(fieldName, dispName, "", textAlign, fixedCol)
        End Sub
        ''' <summary>
        ''' コンストラクタ（サイズ固定セル未指定版 サイズは100px固定）
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispName">表示名</param>
        Public Sub New(fieldName As String, dispName As String)
            Me.New(fieldName, dispName, False)
        End Sub
    End Class
    ''' <summary>
    ''' SQLのパラメータ変数設定
    ''' </summary>
    <Serializable>
    Public Class SQLParamItem
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New(sqlParamName As String, sqlParamType As MySqlDbType, sqlParamValue As String)
            Me.SqlParamName = sqlParamName
            Me.SqlParamType = sqlParamType
            Me.SqlParamValue = sqlParamValue
        End Sub

        ''' <summary>
        ''' SQLパラメーター名
        ''' </summary>
        ''' <returns></returns>
        Public Property SqlParamName As String
        ''' <summary>
        ''' SQLパラメーターの種類
        ''' </summary>
        ''' <returns></returns>
        Public Property SqlParamType As MySqlDbType
        ''' <summary>
        ''' SQLパラメーターに設定する値
        ''' </summary>
        ''' <returns></returns>
        Public Property SqlParamValue As Object
    End Class
    ''' <summary>
    ''' リピーターに貼付する行データ用クラス
    ''' </summary>
    Public Class DispListRowItemData
        ''' <summary>
        ''' ユニークキー
        ''' </summary>
        ''' <returns></returns>
        Public Property KeyVal As String
        ''' <summary>
        ''' チェックボックス選択状態
        ''' </summary>
        ''' <returns></returns>
        Public Property ChkVal As Boolean
        ''' <summary>
        ''' 動的列のクラスリスト
        ''' </summary>
        ''' <returns></returns>
        Public Property ColList As New List(Of DispListItemData)

    End Class
    ''' <summary>
    ''' リピーターに貼付する動的カラムデータ(大本はmControlItem)
    ''' </summary>
    Public Class DispListItemData
        ''' <summary>
        ''' フィールド名
        ''' </summary>
        ''' <returns></returns>
        Public Property FieldName As String
        ''' <summary>
        ''' テキスト幅
        ''' </summary>
        ''' <returns></returns>
        Public Property TextAlign As String
        ''' <summary>
        ''' 固定カラム
        ''' </summary>
        ''' <returns></returns>
        Public Property IsFixed As Boolean
        ''' <summary>
        ''' 値
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As Object

    End Class

#Region "ユーザーコントロールのプロパティ保持関連"
    ''' <summary>
    ''' ビューステート復元(当コントロールのプロパティーを保持)
    ''' </summary>
    ''' <param name="savedState"></param>
    Protected Overrides Sub LoadViewState(savedState As Object)
        Dim totalState As Object() = Nothing
        If savedState IsNot Nothing Then
            totalState = CType(savedState, Object())
            If totalState.Count <> 2 Then
                MyBase.LoadViewState(savedState)
            Else
                MyBase.LoadViewState(totalState(0))
                If totalState(1) IsNot Nothing Then
                    Me.mControlItem = DecodeThisControlValues(totalState(1).ToString)
                End If
            End If
        End If
    End Sub
    ''' <summary>
    ''' ビューステート保存(当コントロールのプロパティ保持)
    ''' </summary>
    ''' <returns></returns>
    Protected Overrides Function SaveViewState() As Object
        Dim baseState = MyBase.SaveViewState()
        Dim totalState(1) As Object
        totalState(0) = baseState
        totalState(1) = GetThisControlItemValueToBase64(Me.mControlItem)
        Return totalState
    End Function
#End Region
End Class