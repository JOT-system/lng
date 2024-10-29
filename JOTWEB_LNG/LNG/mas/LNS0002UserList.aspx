<%@ Page Title="LNS0002L" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNS0002UserList.aspx.vb" Inherits="JOTWEB_LNG.LNS0002UserList" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNS0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNS0002LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNS0002L.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNS0002L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>

    <!-- ファイル選択ダイアログ用 -->
    <script type="text/javascript">
     $(function() {
         $('#inpFileUpload').css({
             'position': 'absolute',
             'top': '-9999px'
         }).change(function() {
             var val = $(this).val();
             var path = val.replace(/\\/g, '/');
             var match = path.lastIndexOf('/');
        $('#txtFileName').css("display","inline-block");
             $('#txtFileName').val(match !== -1 ? val.substring(match + 1) : val);
         });
         $('#txtFileName').bind('keyup, keydown, keypress', function() {
             return false;
         });
         $('#txtFileName, #btnFileSelect').click(function() {
             $('#inpFileUpload').trigger('click');
         });
     });
    </script>
</asp:Content>
 
<asp:Content ID="LNS0002L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <!-- アップロードエリア -->
                        <!-- <div class="divUploadArea"> -->
                            <!-- 一覧件数 -->
                        <!-- <asp:Label ID="ListCount" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <div id="btnFileSelect">ファイルを選択</div>
                            <asp:FileUpload ID="inpFileUpload" runat="server" />
                            <asp:TextBox ID="txtFileName" runat="server" placeholder="選択されていません。" ReadOnly="true"></asp:TextBox>
                            <input type="button" id="WF_ButtonUPLOAD"   class="btn-sticky" value="ｱｯﾌﾟﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonUPLOAD');" />
                        </div> -->
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonINSERT"   class="btn-sticky" value="追加"     onclick="ButtonClick('WF_ButtonINSERT');" />
                        <!-- <asp:Button          id="WF_ButtonDL"       class="btn-sticky" text="ﾀﾞｳﾝﾛｰﾄﾞ"  onclick="WF_ButtonDOWNLOAD_Click" runat="server" />
                        <asp:Button          id="WF_ButtonPDF"      class="btn-sticky" text="一覧印刷"  onclick="WF_ButtonPDF_Click" runat="server" /> -->
                        <input type="button" id="WF_ButtonSEL"      class="btn-sticky" value="詳細選択" onclick="spGetvalue();" />
                        <input type="button" id="WF_ButtonEND"      class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                        <div id="WF_ButtonFIRST" class="firstPage" runat="server"                       onclick="ButtonClick('WF_ButtonFIRST');"></div>
                        <div id="WF_ButtonLAST" class="lastPage" runat="server"                         onclick="ButtonClick('WF_ButtonLAST');"></div>
                    </div>
                </div> <!-- End class=actionButtonBox -->
            </div> <!-- End class="Operation" -->

            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>

        </div>

        <!-- rightbox レイアウト -->
        <MSINC:rightview ID="rightview" runat="server" />

        <!-- leftbox レイアウト -->
        <MSINC:leftview ID="leftview" runat="server" />

        <!-- Work レイアウト -->
        <MSINC:wrklist ID="work" runat="server" />

        <!-- イベント用 -->
        <div style="display:none;">
            <!-- GridView DBクリック-->
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
            <!-- GridView表示位置フィールド -->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>
            <!-- Textbox DBクリックフィールド -->
            <input id="WF_FIELD" runat="server" value="" type="text" />
            <!-- Textbox(Repeater) DBクリックフィールド -->
            <input id="WF_FIELD_REP" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->
            <input id="WF_SelectedIndex" runat="server" value="" type="text" />
            <!-- LeftBox Mview切替 -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
            <!-- LeftBox 開閉 -->
            <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
            <!-- Rightbox Mview切替 -->
            <input id="WF_RightViewChange" runat="server" value="" type="text" />
            <!-- Rightbox 開閉 -->
            <input id="WF_RightboxOpen" runat="server" value="" type="text" />
            <!-- Textbox Print URL -->
            <input id="WF_PrintURL" runat="server" value="" type="text" />
            <!-- 一覧・詳細画面切替用フラグ -->
            <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />
            <!-- ボタン押下 -->
            <input id="WF_ButtonClick" runat="server" value="" type="text" />
            <!-- 権限 -->
            <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
        </div>
 
</asp:Content>
