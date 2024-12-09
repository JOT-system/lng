<%@ Page Title="LNS0001S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master"  CodeBehind="LNS0001UserSearch.aspx.vb" Inherits="JOTWEB_LNG.LNS0001UserSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNS0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="LNS0001SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNS0001S.css")%>' rel="stylesheet" type="text/css" />
</asp:content>

<asp:Content ID="LNS0001S" ContentPlaceHolderID="contents1" runat="server">
     <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="rightSide">
                <input type="button" id="WF_ButtonSEARCH" class="btn-sticky" value="検索" onclick="ButtonClick('WF_ButtonSEARCH');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            <!-- 会社コード -->
            <div class="inputItem">
                <a id="WF_CAMPCODE_LABEL">
                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社コード"></asp:Label>
                </a>
                <a id="WF_CAMPCODE">
                    <asp:TextBox ID="TxtCampCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6" Enabled="false"></asp:TextBox>
                </a>
                <a class="ef" id="WF_CAMPCODE_TEXT">
                    <asp:Label ID="LblCampCodeName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 有効年月日(開始） -->
            <div class="inputItem">
                <a id="WF_STYMD_LABEL">有効年月日（開始）</a>
                <a ondblclick="Field_DBclick('TxtStYMDCode', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox  ID="TxtStYMDCode" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10" ></asp:TextBox>
                </a>
                    <%--<asp:TextBox ID="TxtStYMDCode" runat="server" TextMode="Date" CssClass="TxtDate" onblur="MsgClear();" MaxLength="10"></asp:TextBox>--%>
            </div>
            <!-- 有効年月日(終了） -->
            <div class="inputItem">
                <a id="WF_ENDYMD_LABEL" >有効年月日（終了）</a>
                <a ondblclick="Field_DBclick('TxtEndYMDCode', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox  ID="TxtEndYMDCode" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10" ></asp:TextBox>
                </a>
                    <%--<asp:TextBox ID="TxtEndYMDCode" runat="server" TextMode="Date" CssClass="TxtDate" onblur="MsgClear();" MaxLength="10"></asp:TextBox>--%>
            </div>

            <!-- 組織コード -->
            <div class="inputItem">
                <a id="WF_ORG_LABEL" >組織コード</a>
                <a class="ef" id="WF_ORG" ondblclick="Field_DBclick('TxtOrgCode', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtOrgCode');">
                    <asp:TextBox ID="TxtOrgCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_ORG_TEXT">
                    <asp:Label ID="LblOrgName" runat="server" CssClass="WF_TEXT"></asp:Label>
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
