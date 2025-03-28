﻿<%@ Page Title="LNM0007S" Language="vb" AutoEventWireup="false" CodeBehind="LNM0007Rect1mSearch.aspx.vb" Inherits="JOTWEB_LNG.LNM0007Rect1mSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0007WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content id="LNM0007SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0007S.css")%>' rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="LNM0007S" ContentPlaceHolderID="contents1" runat="server">
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

            <!-- 組織コード -->
            <div class="inputItem">
                <a id="WF_ORG_LABEL">組織コード</a>
                <a class="ef" id="WF_ORG" ondblclick="Field_DBclick('TxtOrgCode', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtOrgCode');">
                    <asp:TextBox ID="TxtOrgCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_ORG_TEXT">
                    <asp:Label ID="LblOrgName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 大分類コード -->
            <div class="inputItem">
                <a id="WF_BIGCTNCD_LABEL">大分類コード</a>
                <a class="ef" id="WF_BIGCTNCD" ondblclick="Field_DBclick('TxtBigCTNCD', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtBigCTNCD');">
                    <asp:TextBox ID="TxtBigCTNCD" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_BIGCTNCD_TEXT">
                    <asp:Label ID="LblBigCTNCDName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 中分類コード -->
            <div class="inputItem">
                <a id="WFMIDDLECTNCD_LABEL">中分類コード</a>
                <a class="ef" id="WF_MIDDLECTNCD" ondblclick="Field_DBclick('TxtMiddleCTNCD', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtMiddleCTNCD');">
                    <asp:TextBox ID="TxtMiddleCTNCD" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_MIDDLECTNCD_TEXT">
                    <asp:Label ID="LblMiddleCTNCDName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 発駅コード -->
            <div class="inputItem">
                <a id="WF_DEPSTATION_LABEL">発駅コード</a>
                <a class="ef" id="WF_DEPSTATION" ondblclick="Field_DBclick('TxtDepStation', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtDepStation');">
                    <asp:TextBox ID="TxtDepStation" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon"  onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_DEPSTATION_TEXT">
                    <asp:Label ID="LblDepStationName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 発受託人コード -->
            <div class="inputItem">
                <a id="WF_DEPTRUSTEECD_LABEL">発受託人コード</a>
                <a class="ef" id="WF_DEPTRUSTEECD" ondblclick="Field_DBclick('TxtDepTrusteeCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtDepTrusteeCd');">
                    <asp:TextBox ID="TxtDepTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon"  onblur="MsgClear();" MaxLength="5"></asp:TextBox>
                </a>
                <a id="WF_DEPTRUSTEECD_TEXT">
                    <asp:Label ID="LblDepTrusteeCdName" runat="server" CssClass="WF_TEXT"></asp:Label>
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

    <!-- multiSelect レイアウト -->
    <!-- 駅単一選択 -->
    <MSINC:multiselect runat="server" id="mspStationSingle" />

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
