'------------------------------------------------------------------------------
' <自動生成>
'     このコードはツールによって生成されました。
'
'     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
'     コードが再生成されるときに損失したりします。 
' </自動生成>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On


Partial Public Class LNT0022InspeLNManage
    
    '''<summary>
    '''WF_STATUS コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_STATUS As Global.System.Web.UI.WebControls.DropDownList
    
    '''<summary>
    '''WF_CTNTYPE コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_CTNTYPE As Global.System.Web.UI.WebControls.DropDownList
    
    '''<summary>
    '''WF_CTNNO コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_CTNNO As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''WF_STATION コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_STATION As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''WF_STATIONNAME コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_STATIONNAME As Global.System.Web.UI.WebControls.Label
    
    '''<summary>
    '''WF_ButtonDownload コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_ButtonDownload As Global.System.Web.UI.HtmlControls.HtmlInputButton
    
    '''<summary>
    '''WF_FileUpload コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_FileUpload As Global.System.Web.UI.WebControls.FileUpload
    
    '''<summary>
    '''pnlChangePage コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents pnlChangePage As Global.System.Web.UI.WebControls.Panel
    
    '''<summary>
    '''txtSelectPage コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents txtSelectPage As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''btnRefreshPage コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents btnRefreshPage As Global.System.Web.UI.WebControls.Button
    
    '''<summary>
    '''btnFirstPage コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents btnFirstPage As Global.System.Web.UI.WebControls.ImageButton
    
    '''<summary>
    '''btnBackPage コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents btnBackPage As Global.System.Web.UI.WebControls.ImageButton
    
    '''<summary>
    '''lblNowPage コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents lblNowPage As Global.System.Web.UI.WebControls.Label
    
    '''<summary>
    '''lblMaxPage コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents lblMaxPage As Global.System.Web.UI.WebControls.Label
    
    '''<summary>
    '''btnNextPage コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents btnNextPage As Global.System.Web.UI.WebControls.ImageButton
    
    '''<summary>
    '''btnLastPage コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents btnLastPage As Global.System.Web.UI.WebControls.ImageButton
    
    '''<summary>
    '''pnlNoData コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents pnlNoData As Global.System.Web.UI.WebControls.Panel
    
    '''<summary>
    '''gvLNT0022 コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents gvLNT0022 As Global.System.Web.UI.WebControls.GridView
    
    '''<summary>
    '''hdnShowPnlInspectDialog コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents hdnShowPnlInspectDialog As Global.System.Web.UI.WebControls.HiddenField
    
    '''<summary>
    '''pnlInspectDialogArea コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents pnlInspectDialogArea As Global.System.Web.UI.WebControls.Panel
    
    '''<summary>
    '''gvDialogHead コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents gvDialogHead As Global.System.Web.UI.WebControls.GridView
    
    '''<summary>
    '''gvDialogRegularInspects コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents gvDialogRegularInspects As Global.System.Web.UI.WebControls.GridView
    
    '''<summary>
    '''gvDialogAdditionInspects コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents gvDialogAdditionInspects As Global.System.Web.UI.WebControls.GridView
    
    '''<summary>
    '''txtValidateMessage コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents txtValidateMessage As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''WF_INSPECT_UPDATE コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_INSPECT_UPDATE As Global.System.Web.UI.WebControls.Button
    
    '''<summary>
    '''rightview コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents rightview As Global.JOTWEB_LNG.GRIS0004RightBox
    
    '''<summary>
    '''leftview コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents leftview As Global.JOTWEB_LNG.GRIS0005LeftBox
    
    '''<summary>
    '''work コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents work As Global.JOTWEB_LNG.LNT0022WRKINC
    
    '''<summary>
    '''WF_GridDBclick コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_GridDBclick As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''WF_GridPosition コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_GridPosition As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''WF_FIELD コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_FIELD As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_FIELD_REP コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_FIELD_REP As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_SelectedIndex コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_SelectedIndex As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_DelInspectRowIndex コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_DelInspectRowIndex As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_StationTable コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_StationTable As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_InspectCodes コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_InspectCodes As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_DISP コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_DISP As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_LeftMViewChange コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_LeftMViewChange As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_LeftboxOpen コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_LeftboxOpen As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_RightViewChange コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_RightViewChange As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_RightboxOpen コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_RightboxOpen As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_BOXChange コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_BOXChange As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_ButtonClick コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_ButtonClick As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_MAPpermitcode コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_MAPpermitcode As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_MAPButtonControl コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_MAPButtonControl As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_DTAB_CHANGE_NO コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_DTAB_CHANGE_NO As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''hdnDispHeaderItems コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents hdnDispHeaderItems As Global.System.Web.UI.WebControls.HiddenField
    
    '''<summary>
    '''WF_CheckBoxFLG コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_CheckBoxFLG As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_PrintURL1 コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_PrintURL1 As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_PrintURL2 コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_PrintURL2 As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_PrintURL3 コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_PrintURL3 As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_PrintURL4 コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_PrintURL4 As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''Master プロパティ。
    '''</summary>
    '''<remarks>
    '''自動生成されたプロパティ。
    '''</remarks>
    Public Shadows ReadOnly Property Master() As JOTWEB_LNG.LNGMasterPage
        Get
            Return CType(MyBase.Master,JOTWEB_LNG.LNGMasterPage)
        End Get
    End Property
End Class
