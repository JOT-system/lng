<%@ Page Title="LNM0014S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master"  CodeBehind="LNM0014ReutrmSearch.aspx.vb" Inherits="JOTWEB_LNG.LNM0014ReutrmSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0014WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:content id="LNM0014SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0014S.css")%>' rel="stylesheet" type="text/css" />
</asp:content>

<asp:Content ID="LNM0014S" ContentPlaceHolderID="contents1" runat="server">
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

            <!-- 大分類コード -->
            <div class="inputItem">
                <a id="WF_BIGCTNCD_LABEL" >大分類コード</a>
                <a class="ef" id="WF_BIGCTNCD" ondblclick="Field_DBclick('txtBigCtnCd', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('txtBigCtnCd');">
                    <asp:TextBox ID="txtBigCtnCd" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_BIGCTNCD_TEXT">
                    <asp:Label ID="lblBigCtnCdName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 中分類コード -->
            <div class="inputItem">
                <a id="WF_MIDDLECTNCD_LABEL" >中分類コード</a>
                <a class="ef" id="WF_MIDDLECTNCD" ondblclick="Field_DBclick('txtMiddleCtnCd', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('txtMiddleCtnCd');">
                    <asp:TextBox ID="txtMiddleCtnCd" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_MIDDLECTNCD_TEXT">
                    <asp:Label ID="lblMiddleCtnCdName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 発駅コード -->
            <div class="inputItem">
                <a id="WF_DEPSTATION_LABEL" >発駅コード</a>
                <a class="ef" id="WF_DEPSTATION" ondblclick="Field_DBclick('txtDepStation',  <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('txtDepStation');">
                    <asp:TextBox ID="txtDepStation" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_DEPSTATION_TEXT">
                    <asp:Label ID="lblDepStationName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 発受託人コード -->
            <div class="inputItem">
                <a id="WF_DEPTRUSTEECD_LABEL" >発受託人コード</a>
                <a class="ef" id="WF_DEPTRUSTEECD" ondblclick="Field_DBclick('txtDepTrusteeCd',  <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('txtDepTrusteeCd');">
                    <asp:TextBox ID="txtDepTrusteeCd" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="5"></asp:TextBox>
                </a>
                <a id="WF_DEPTRUSTEECD_TEXT">
                    <asp:Label ID="lblDepTrusteeCdName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 発受託人サブコード -->
            <div class="inputItem">
                <a id="WF_DEPTRUSTEESUBCD_LABEL" >発受託人サブコード</a>
                <a class="ef" id="WF_DEPTRUSTEESUBCD" ondblclick="Field_DBclick('txtDepTrusteeSubCd',  <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('txtDepTrusteeSubCd');">
                    <asp:TextBox ID="txtDepTrusteeSubCd" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="3"></asp:TextBox>
                </a>
                <a id="WF_DEPTRUSTEESUBCD_TEXT">
                    <asp:Label ID="lblDepTrusteeSubCdName" runat="server" CssClass="WF_TEXT"></asp:Label>
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
