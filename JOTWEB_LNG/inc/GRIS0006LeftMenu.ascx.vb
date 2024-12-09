Option Strict On
Imports MySql.Data.MySqlClient


''' <summary>
''' 左ボックス共通ユーザーコントロールクラス
''' </summary>
Public Class GRIS0006LeftMenu
    Inherits UserControl

    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力

    Private Const C_VSNAME_LEFTNAVIDATA_3 As String = "VS_MENU_LEFT_NAVI_3"
    Private Const C_VSNAME_LEFTNAVIDATA_4 As String = "VS_MENU_LEFT_NAVI_4"
    Private Const C_VSNAME_LEFTNAVIDATA_5 As String = "VS_MENU_LEFT_NAVI_5"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Dim O_RTN As String = C_MESSAGE_NO.NORMAL

        If IsPostBack Then
            Select Case DirectCast(Page.Master.FindControl("contents1").FindControl("WF_ButtonClick"), HtmlInputText).Value
                Case "WF_ButtonLeftNavi1"
                    BtnLeftNavi_Click(1)
                Case "WF_ButtonLeftNavi2"
                    BtnLeftNavi_Click(2)
                Case "WF_ButtonLeftNavi3"
                    BtnLeftNavi_Click(3)
                Case "WF_ButtonLeftNavi4"
                    BtnLeftNavi_Click(4)
                Case "WF_ButtonLeftNavi5"
                    BtnLeftNavi_Click(5)
            End Select
        Else
            '○ 初期化処理
            Initialize()

        End If
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        Dim menuButtonList3 As List(Of MenuItem) = Nothing      '受注管理(レンタル)
        Dim menuButtonList4 As List(Of MenuItem) = Nothing      '帳票(レンタル)
        Dim menuButtonList5 As List(Of MenuItem) = Nothing      'マスタ管理(レンタル)

        Using sqlCon As MySqlConnection = CS0050SESSION.getConnection
            sqlCon.Open()
            'メニューボタン情報の取得
            Try
                'menuButtonList3 = GetMenuItemList(sqlCon, "3")　'予算
                'menuButtonList4 = GetMenuItemList(sqlCon, "4")
                'menuButtonList5 = GetMenuItemList(sqlCon, "5")　'マスタ
                menuButtonList3 = GetMenuItemList(sqlCon, "3")　'予算
                menuButtonList4 = GetMenuItemList(sqlCon, "5")
                menuButtonList5 = GetMenuItemList(sqlCon, "4")　'マスタ


                ViewState(C_VSNAME_LEFTNAVIDATA_3) = menuButtonList3
                Me.repLeftNav3.DataSource = menuButtonList3
                Me.repLeftNav3.DataBind()
                ViewState(C_VSNAME_LEFTNAVIDATA_4) = menuButtonList4
                'Me.repLeftNav4.DataSource = menuButtonList4
                'Me.repLeftNav4.DataBind()
                ViewState(C_VSNAME_LEFTNAVIDATA_5) = menuButtonList5
                Me.repLeftNav5.DataSource = menuButtonList5
                Me.repLeftNav5.DataBind()

            Catch ex As Exception
                'Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0008_UPROFMAP SELECT")

                CS0011LOGWrite.INFSUBCLASS = "Main"
                CS0011LOGWrite.INFPOSI = "S0008_UPROFMAP SELECT"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()
                Return
            End Try

        End Using


    End Sub

    ''' <summary>
    ''' メニューボタン情報を取得する
    ''' </summary>
    ''' <returns></returns>
    Private Function GetMenuItemList(sqlCon As MySqlConnection, ByVal strTitleKbn As String, Optional ByVal strHeadKbn As Integer = 1) As List(Of MenuItem)
        Dim retItm As New List(Of MenuItem)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT A.POSICOL")
        sqlStat.AppendLine("      ,A.POSIROW AS ROWLINE")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.MAPID,''))      as MAPID")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.VARIANT,''))    as VARIANT")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.TITLENAMES,'')) as TITLE")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.MAPNAMES,''))   as NAMES")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.MAPNAMEL,''))   as NAMEL")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.REPORTFLG,''))   as REPORTFLG")
        sqlStat.AppendLine("      ,rtrim(coalesce(A.REPORTID,''))   as REPORTID")
        sqlStat.AppendLine("      ,rtrim(coalesce(B.URL,''))        as URL")
        sqlStat.AppendLine("  FROM      COM.LNS0009_PROFMMAP           A")
        sqlStat.AppendLine("  LEFT JOIN COM.LNS0007_URL                B")
        sqlStat.AppendLine("    ON B.MAPID    = A.MAPID")
        sqlStat.AppendLine("   AND B.STYMD   <= @STYMD")
        sqlStat.AppendLine("   AND B.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("   AND B.DELFLG  <> @DELFLG")
        sqlStat.AppendLine(" WHERE A.CAMPCODE = @CAMPCODE")
        sqlStat.AppendLine("   AND A.MAPIDP   = @MAPIDP")
        sqlStat.AppendLine("   AND A.VARIANTP = @VARIANTP")
        sqlStat.AppendLine("   AND A.TITLEKBN = @TITLEKBN")
        sqlStat.AppendLine("   AND A.STYMD   <= @STYMD")
        sqlStat.AppendLine("   AND A.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("   AND A.DELFLG  <> @DELFLG")
        sqlStat.AppendLine(" ORDER BY A.POSICOL,A.POSIROW")
        Using dt As New DataTable
            Using sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)

                With sqlCmd.Parameters
                    .Add("@CAMPCODE", MySqlDbType.VarChar, 20).Value = LM_COMPCODE.Value
                    .Add("@MAPIDP", MySqlDbType.VarChar, 50).Value = GRM00001WRKINC.MAPID
                    .Add("@VARIANTP", MySqlDbType.VarChar, 50).Value = LM_ROLE_MENU.Value
                    .Add("@STYMD", MySqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", MySqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    .Add("@TITLEKBN", MySqlDbType.VarChar, 1).Value = strTitleKbn
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        dt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    dt.Load(sqlDr)
                    sqlDr.Close()
                End Using 'sqlDr
            End Using 'sqlCmd
            '取得結果を元にメニューアイテムクラスに格納
            '上位リストのみを取得()
            Dim topLevelList = From dr As DataRow In dt Where dr("ROWLINE").Equals(strHeadKbn)
            Dim childItems As List(Of DataRow) = Nothing
            '上位回送のリストループROWLINEが"1"のみ
            For Each topLevelItm In topLevelList
                Dim posiCol As Integer = CInt(topLevelItm("POSICOL"))
                childItems = (From dr As DataRow In dt Where dr("POSICOL").Equals(posiCol) AndAlso dr("ROWLINE").Equals(4)).ToList

                Dim retTopLevelItm = New MenuItem
                retTopLevelItm.PosiCol = CInt(topLevelItm("POSICOL"))
                retTopLevelItm.RowLine = CInt(topLevelItm("ROWLINE"))
                retTopLevelItm.MapId = Convert.ToString(topLevelItm("MAPID"))
                retTopLevelItm.Variant = Convert.ToString(topLevelItm("VARIANT"))
                retTopLevelItm.Title = Convert.ToString(topLevelItm("TITLE"))
                retTopLevelItm.Names = Convert.ToString(topLevelItm("NAMES"))
                retTopLevelItm.Names = Convert.ToString(topLevelItm("NAMEL"))
                retTopLevelItm.Reportflg = Convert.ToString(topLevelItm("REPORTFLG"))
                retTopLevelItm.Reportid = Convert.ToString(topLevelItm("REPORTID"))
                retTopLevelItm.Url = Convert.ToString(topLevelItm("URL"))

                If childItems.Count = 0 Then
                    '子供を完全に持たない
                    '一応意味はないがコケると困るので
                    If retTopLevelItm.Url = "" Then
                        retTopLevelItm.Url = "~/LNG/ex/page_404.html"
                    End If

                ElseIf childItems.Count = 1 Then
                    With childItems(0)
                        If retTopLevelItm.MapId = "" Then
                            retTopLevelItm.MapId = Convert.ToString(.Item("MAPID"))
                        End If
                        If retTopLevelItm.Variant = "" Then
                            retTopLevelItm.Variant = Convert.ToString(.Item("VARIANT"))
                        End If
                        If retTopLevelItm.Title = "" Then
                            retTopLevelItm.Title = Convert.ToString(.Item("TITLE"))
                        End If
                        If retTopLevelItm.Names = "" Then
                            retTopLevelItm.Names = Convert.ToString(.Item("NAMES"))
                        End If
                        If retTopLevelItm.Namel = "" Then
                            retTopLevelItm.Namel = Convert.ToString(.Item("NAMEL"))
                        End If
                        If retTopLevelItm.Url = "" Then
                            retTopLevelItm.Url = Convert.ToString(.Item("URL"))
                        End If
                        If retTopLevelItm.Url = "" Then
                            retTopLevelItm.Url = "~/LNG/ex/page_404.html"
                        End If
                    End With
                Else
                    '名前が無ければ子供の先頭の名称を付与
                    With childItems(0)
                        If retTopLevelItm.Names = "" Then
                            retTopLevelItm.Names = Convert.ToString(.Item("NAMES"))
                        End If
                        If retTopLevelItm.Namel = "" Then
                            retTopLevelItm.Namel = Convert.ToString(.Item("NAMEL"))
                        End If
                    End With
                    For Each childItem In childItems
                        Dim retChildItm = New MenuItem
                        retChildItm.PosiCol = CInt(childItem("POSICOL"))
                        retChildItm.RowLine = CInt(childItem("ROWLINE"))
                        retChildItm.MapId = Convert.ToString(childItem("MAPID"))
                        retChildItm.Variant = Convert.ToString(childItem("VARIANT"))
                        retChildItm.Title = Convert.ToString(childItem("TITLE"))
                        retChildItm.Names = Convert.ToString(childItem("NAMES"))
                        retChildItm.Namel = Convert.ToString(childItem("NAMEL"))
                        retChildItm.Url = Convert.ToString(childItem("URL"))
                        If retChildItm.Url = "" Then
                            retChildItm.Url = "~/LNG/ex/page_404.html"
                        End If
                        retTopLevelItm.ChildMenuItem.Add(retChildItm)
                    Next childItem

                End If
                childItems = Nothing
                If retTopLevelItm.Names = "" Then
                    retTopLevelItm.Names = "　"
                End If

                'Dim keyName As String = MP0000Base.GetBase64Str(retTopLevelItm.Names)
                'Dim val As String = MP0000Base.LoadCookie(keyName, Me)
                Dim isOpen As Boolean = True
                'If val <> "" Then
                '    isOpen = Convert.ToBoolean(val)
                'End If
                retTopLevelItm.OpenChild = isOpen
                retItm.Add(retTopLevelItm)
            Next topLevelItm

        End Using 'dt
        Return retItm

    End Function

    Protected Sub BtnLeftNavi_Click(intID As Integer)
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap
        Dim strIDNAME As String = ""
        Select Case intID
            Case 3
                strIDNAME = C_VSNAME_LEFTNAVIDATA_3
            Case 4
                strIDNAME = C_VSNAME_LEFTNAVIDATA_4
            Case 5
                strIDNAME = C_VSNAME_LEFTNAVIDATA_5
        End Select
        Dim leftNaviList = DirectCast(ViewState(strIDNAME), List(Of MenuItem))
        'ありえないがメニュー表示リストが存在しない場合はそのまま終了
        If leftNaviList Is Nothing OrElse
           IsNumeric(Me.hdnPosiCol.Value) = False OrElse
           IsNumeric(Me.hdnRowLine.Value) = False Then
            Return
        End If
        Dim posiRow As Integer = CInt(Me.hdnRowLine.Value)
        Dim posiCol As Integer = CInt(Me.hdnPosiCol.Value)
        Dim rowLine As Integer = CInt(Me.hdnRowLine.Value)
        Me.hdnPosiCol.Value = ""
        Me.hdnRowLine.Value = ""
        Dim menuItm As MenuItem = Nothing
        Dim qMenuItm = From itm In leftNaviList Where itm.PosiCol = posiCol
        If rowLine = 1 Then
            menuItm = qMenuItm.FirstOrDefault
        Else
            If qMenuItm.Any Then
                menuItm = (From itm In qMenuItm(0).ChildMenuItem Where itm.RowLine = rowLine).FirstOrDefault
            End If
        End If
        'ありえないが選択したメニューアイテムが存在しない場合はそのまま終了
        If menuItm Is Nothing Then
            Return
        End If

        'ボタン押下時、画面遷移
        Server.Transfer(menuItm.Url)
    End Sub

    ''' <summary>
    ''' 画面表示用遷移ボタンアイテムクラス
    ''' </summary>
    <Serializable>
    Public Class MenuItem
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            Me.ChildMenuItem = New List(Of MenuItem)
            Me.OpenChild = False
        End Sub
        ''' <summary>
        ''' 列表示(PROFMAP:POSICOL)
        ''' </summary>
        ''' <returns></returns>
        Public Property PosiCol As Integer
        ''' <summary>
        ''' 行位置(PROFMAP:POSIROW) ⇒ 親クラスリストとして利用する場合は"1"のみ、子で再帰利用している箇所は"1"以外
        ''' </summary>
        ''' <returns></returns>
        Public Property RowLine As Integer
        ''' <summary>
        ''' 画面ＩＤ(PROFMAP:MAPID)
        ''' </summary>
        ''' <returns></returns>
        Public Property MapId As String
        ''' <summary>
        ''' 変数(PROFMAP:VARIANT)
        ''' </summary>
        ''' <returns></returns>
        Public Property [Variant] As String
        ''' <summary>
        ''' タイトル名称(PROFMAP:TITLENAMES)⇒左ナビのCSSクラス名として設定
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Title As String
        ''' <summary>
        ''' 画面名称（短）(PROFMAP:MAPNAMES) ⇒ ボタン名称に設定
        ''' </summary>
        ''' <returns></returns>
        Public Property Names As String
        ''' <summary>
        ''' 画面名称（長）(PROFMAP:MAPNAMEL) ⇒ 現状当プロパティに投入のみ未使用
        ''' </summary>
        ''' <returns></returns>
        Public Property Namel As String
        '''<summary>
        '''帳票フラグ (PROFMAP:REPORTFLG)
        '''</summary>
        '''<returns></returns>
        Public Property Reportflg As String
        '''<summary>
        '''帳票ID (PROFMAP:REPORTID)
        '''</summary>
        '''<returns></returns>
        Public Property Reportid As String
        ''' <summary>
        ''' URL（URLマスタ：URL）チルダ付き（アプリルート相対）の遷移URL
        ''' </summary>
        ''' <returns></returns>
        Public Property Url As String
        ''' <summary>
        ''' POSICOLが同一でROWLINが1以外の子データを格納
        ''' </summary>
        ''' <returns></returns>
        Public Property ChildMenuItem As List(Of MenuItem)
        ''' <summary>
        ''' 子要素の表示状態
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>現状未使用：ポストバック発生時に閉じてしまったら利用検討</remarks>
        Public Property OpenChild As Boolean = False

        ''' <summary>
        ''' 子要素を持っているか（デザイン判定用：▼表示判定）
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>ある程度「孫・ひ孫」対応できる構造だが現状「子」のみ</remarks>
        Public ReadOnly Property HasChild As Boolean
            Get
                If ChildMenuItem Is Nothing OrElse ChildMenuItem.Count = 0 Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property
        ''' <summary>
        ''' 次ページ遷移情報を持つか(True：次画面遷移あり、False：次画面遷移無し)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HasNextPageInfo As Boolean
            Get
                'MAPIDを持つか持たないかで判定
                If Me.MapId.Trim.Equals("") Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

    End Class

#Region "<< Property Accessor >>"
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    Public Property COMPCODE As String
        Get
            Return LM_COMPCODE.Value
        End Get
        Set(value As String)
            LM_COMPCODE.Value = value
        End Set
    End Property
    ''' <summary>
    ''' ロール
    ''' </summary>
    Public Property ROLEMENU As String
        Get
            Return LM_ROLE_MENU.Value
        End Get
        Set(value As String)
            LM_ROLE_MENU.Value = value
        End Set
    End Property
#End Region
End Class