<%@ Page Title="LNM0023S" Language="vb" AutoEventWireup="false" CodeBehind="LNM0023ShipperSearch.aspx.vb" Inherits="JOTWEB_LNG.LNM0023ShipperSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0023WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>

<asp:Content id="LNM0023SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0023S.css")%>' rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="LNM0023S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonSEARCH" class="btn-sticky" value="検索" onclick="ButtonClick('WF_ButtonSEARCH');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 荷主コード -->
            <div class="inputItem">
                <a id="WF_SHIPPERCD_LABEL">荷主コード</a>
                <a ondblclick="Field_DBclick('TxtShipperCd', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPER%>);" onchange="TextBox_change('TxtShipperCd');">
                    <asp:TextBox ID="TxtShipperCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_DEPSTATION_TEXT">
                    <asp:Label ID="LblShipperCdName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 論理削除フラグ -->
            <div class="inputItem">
                <a id="WF_DELDATAFLG">
                    <asp:CheckBox ID="ChkDelDataFlg" runat="server" Text="削除行を含む" />
                </a>
            </div>
        </div> <!-- End inputBox -->
    </div> <!-- End searchbox -->

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />
    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />
    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
    </div>
</asp:Content>
