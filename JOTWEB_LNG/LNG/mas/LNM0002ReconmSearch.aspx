<%@ Page Title="LNM0002S" Language="vb" AutoEventWireup="false" CodeBehind="LNM0002ReconmSearch.aspx.vb" Inherits="JOTWEB_LNG.LNM0002ReconmSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>

<asp:Content id="LNM0002SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0002S.css")%>' rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="LNM0002S" ContentPlaceHolderID="contents1" runat="server">
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
            <!-- コンテナ記号 -->
            <div class="inputItem">
                <a id="WF_CTNTYPE_LABEL">
                    <asp:Label ID="WF_CTNTYPE_L" class="requiredMark" runat="server" Text="コンテナ記号"></asp:Label>
                </a>
                <a class="ef" id="WF_CTNTYPE" ondblclick="Field_DBclick('TxtCTNType', <%=LIST_BOX_CLASSIFICATION.LC_RECONM%>);" onchange="TextBox_change('TxtCTNType');">
                    <asp:TextBox ID="TxtCTNType" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon"  onblur="MsgClear();" MaxLength="5"></asp:TextBox>
                </a>
                <a id="WF_CTNTYPE_TEXT" style="display:none;">
                    <asp:Label ID="LblCTNTypeName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- コンテナ番号 -->
            <div class="inputItem">
                <a id="WF_CTNNO_LABEL">
                    <asp:Label ID="WF_CTNNO_L" runat="server" Text="コンテナ番号"></asp:Label>
                </a>
                <a class="ef" id="WF_CTNNO" ondblclick="Field_DBclick('TxtCTNNo', <%=LIST_BOX_CLASSIFICATION.LC_RECONM%>);" onchange="TextBox_change('TxtCTNNo');">
                    <asp:TextBox ID="TxtCTNNo" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon"  onblur="MsgClear();" MaxLength="8"></asp:TextBox>
                </a>
                <a id="WF_CTNNO_TEXT" style="display:none;">
                    <asp:Label ID="LblCTNNoName" runat="server" CssClass="WF_TEXT"></asp:Label>
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
