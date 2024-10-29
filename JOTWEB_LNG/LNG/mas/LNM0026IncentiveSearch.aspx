<%@ Page Title="LNM0026S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master"  CodeBehind="LNM0026IncentiveSearch.aspx.vb" Inherits="JOTWEB_LNG.LNM0026IncentiveSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0026WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="LNM0026SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0026S.css")%>' rel="stylesheet" type="text/css" />
</asp:content>

<asp:Content ID="LNM0026S" ContentPlaceHolderID="contents1" runat="server">
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

            <!-- 取引先コード -->
            <div class="inputItem">
                <a id="WF_TORICODE_LABEL" >取引先コード</a>
                <a class="ef" id="WF_TORICODE" ondblclick="Field_DBclick('txtToriCode', <%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);" onchange="TextBox_change('txtToriCode');">
                    <asp:TextBox ID="txtToriCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </a>
                <a id="WF_TORICODE_TEXT">
                    <asp:Label ID="lblToriCodeName" runat="server" CssClass="WF_TEXT"></asp:Label>
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

            <!-- 論理削除フラグ -->
            <div class="inputItem">
                <a id="WF_DELDATAFLG">
                    <asp:CheckBox ID="ChkDelDataFlg" runat="server" Text="　削除行を含む" />
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
