''************************************************************
' ガイダンスマスタメンテダウンロード
' 作成日 2022/03/02
' 更新日 
' 作成者 名取
' 更新者 
'
' 修正履歴 : 2022/03/02 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient
''' <summary>
''' ガイダンスダウンロードクラス(画面は提供せずファイルストリームを転送する)
''' </summary>
Public Class LNS0008GuidanceDownload
    Inherits System.Web.UI.Page

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    ''' <summary>
    ''' 添付ファイルをダウンロード
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' パラメータが無ければ404
        If Request.Params Is Nothing OrElse Request.Params.Count = 0 _
           OrElse Not Request.Params.AllKeys.Contains("id") Then
            Response.Redirect("~/LNG/ex/page_404.html")
            Return
        End If
        Dim WW_ParamStr = Request.Params("id")
        Dim WW_DecParam As List(Of String) = LNS0008WRKINC.DecodeParamString(WW_ParamStr)
        Dim WW_FilePath As String = ""
        Dim WW_FileName As String = ""
        If WW_DecParam(2) = "0" Then
            WW_FilePath = GetFilePath(WW_DecParam(1))
            WW_FileName = WW_DecParam(1)
        Else
            WW_FilePath = GetFilePath(WW_DecParam(0), WW_DecParam(1))
            WW_FileName = IO.Path.GetFileName(WW_FilePath)
        End If

        If WW_FilePath = "" Then
            Response.Redirect("~/LNG/ex/page_404.html")
            Return
        End If

        Dim WW_Fileinfo = New IO.FileInfo(WW_FilePath)
        Dim WW_EncodeFileName As String = HttpUtility.UrlEncode(WW_FileName)
        WW_EncodeFileName = WW_EncodeFileName.Replace("+", "%20")
        Response.ContentType = "application/octet-stream"
        Response.AddHeader("Content-Disposition", String.Format("attachment;filename*=utf-8''{0}", WW_EncodeFileName))
        Response.AddHeader("Content-Length", WW_Fileinfo.Length.ToString())
        Response.WriteFile(WW_FilePath)
        Response.End()
    End Sub

    ''' <summary>
    ''' ファイルパス生成（作業フォルダ）
    ''' </summary>
    ''' <param name="WW_FileName"></param>
    ''' <returns></returns>
    Private Function GetFilePath(WW_FileName As String) As String
        Dim WW_GuidanceWorkDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, LNS0008WRKINC.GUIDANCEROOT, "USERWORKS", CS0050SESSION.USERID)
        If Not IO.Directory.Exists(WW_GuidanceWorkDir) Then
            Return ""
        End If
        Dim WW_RetFilePath As String = IO.Path.Combine(WW_GuidanceWorkDir, WW_FileName)
        If Not IO.File.Exists(WW_RetFilePath) Then
            Return ""
        End If
        Return WW_RetFilePath
    End Function

    ''' <summary>
    ''' ファイルパス生成（正式フォルダ）
    ''' </summary>
    ''' <param name="WW_GuidanceNo"></param>
    ''' <param name="WW_FileNo"></param>
    ''' <returns></returns>
    Private Function GetFilePath(WW_GuidanceNo As String, WW_FileNo As String) As String
        Dim WW_FileName As String = ""

        If WW_FileNo = "" Then
            Return ""
        End If
        Dim SQLstat As New StringBuilder
        SQLstat.AppendFormat("SELECT GD.FILE{0}", WW_FileNo).AppendLine()
        SQLstat.AppendLine("  FROM COM.LNS0008_GUIDANCE GD")
        SQLstat.AppendLine(" WHERE GD.GUIDANCENO = @GUIDANCENO ")
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection,
              SQLCmd As New MySqlCommand(SQLstat.ToString, SQLcon)
            SQLcon.Open()
            With SQLCmd.Parameters
                .Add("@GUIDANCENO", MySqlDbType.VarChar).Value = WW_GuidanceNo
            End With
            Dim WW_FileNameObj = SQLCmd.ExecuteScalar
            WW_FileName = Convert.ToString(WW_FileNameObj)
        End Using

        If WW_FileName = "" Then
            Return ""
        End If
        Dim WW_GuidanceDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, LNS0008WRKINC.GUIDANCEROOT, WW_GuidanceNo)
        Dim WW_FilePath As String = IO.Path.Combine(WW_GuidanceDir, WW_FileName)
        If IO.File.Exists(WW_FilePath) = False Then
            Return ""
        End If

        Return WW_FilePath
    End Function

End Class

