<%@ Page Title="LNT0017S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0017TsptQuantityReportOutput.aspx.vb" Inherits="JOTWEB_LNG.LNT0017TsptQuantityReportOutput" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0017WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>

<asp:Content id="LNT0017SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0017.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0017S.js")%>'></script>
</asp:Content>

<asp:Content ID="LNT0017S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonOUTPUT" class="btn-sticky" value="出力" onclick="ButtonClick('WF_ButtonOUTPUT');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">
            
            <!-- 年月日(開始） -->
            <div class="inputItem">
                <a id="WF_STYMD_LABEL">年月日（開始）</a>
            </div>
            <div class="inputItem">
                <a></a>
                <a class="ef" id="WF_STYMD" ondblclick="Field_DBclick('TxtStYMDCode', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtStYMDCode" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </a>
            </div>
            <!-- 年月日(終了） -->
            <div class="inputItem" id="WF_ENDYMD_AREA">
                <a id="WF_ENDYMD_LABEL">年月日（終了）</a>
                <a class="ef" id="WF_ENDYMD" ondblclick="Field_DBclick('TxtEndYMDCode', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtEndYMDCode" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </a>
            </div>
            
            <div class="inputItem">
                <a id="WF_BIGCTN_LABEL">出力大分類</a>
                <asp:DropDownList ID="ddlBigCtn" runat="server" ClientIDMode="Predictable" CssClass="ddlSelectBigCtn" OnChange="SelectDropDownList_OnChange('WF_SelectControl_OnChange')"></asp:DropDownList>
            </div>
        </div> <!-- End inputBox -->
    </div> <!-- End searchbox -->

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />
    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />
    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- 非表示項目 -->
    <asp:HiddenField ID="hdnBigCtn" runat="server" Visible="false" ClientIDMode="Predictable"  />
    <asp:HiddenField ID="hdnReport" runat="server" Visible="false" ClientIDMode="Predictable"  />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_PrintURL" runat="server" value="" type="text" />              <!-- Textbox Print URL -->
        <!-- ダウンロードボタン表示フラグ(非表示) -->
        <input id="WF_DownloadFlg" runat="server" value="" type="text" />
        <!-- Textbox Print URL -->
        <input id="WF_PrintURL1" runat="server" value="" type="text" />
        <input id="WF_PrintURL2" runat="server" value="" type="text" />
    </div>
</asp:Content>
