<%@ Page Title="LNT0020S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0020OperatIncomeReportOutput.aspx.vb" Inherits="JOTWEB_LNG.LNT0020OperatIncomeReportOutput" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0020WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>

<asp:Content id="LNT0020SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0020.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0020S.js")%>'></script>
</asp:Content>

<asp:Content ID="LNT0020S" ContentPlaceHolderID="contents1" runat="server">
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
            <!-- 帳票選択 -->
            <div class="inputItem">
                <a id="WF_TORICODE_LABEL">勘定科目</a>
                <asp:DropDownList ID="WF_REPORTNAME" runat="server" onchange="ButtonClick('WF_REPORTNAME');" class="DdlAccount"/>
            </div>
            <!-- 年度 -->
            <asp:Panel ID="WF_FISCALYEAR_F" runat="server" CssClass="inputItem">
                <a id="WF_FISCALYEAR_L">年度（期間種別に任意期間以外を選択した場合に入力してください）</a>
                <asp:TextBox ID="WF_FISCALYEAR" runat="server" onchange="ButtonClick('WF_FISCALYEAR');"  class="Year"/>
            </asp:Panel>
            <!-- 期間種別 -->
            <asp:Panel ID="WF_PERIODTYPE_F" runat="server" CssClass="inputItem">
                <a id="WF_PERIODTYPE_L">期間種別</a>
                <asp:DropDownList ID="WF_PERIODTYPE_DDL" runat="server" onchange="ButtonClick('WF_PERIODTYPE_DDL');" class="DdlAccount"/>
            </asp:Panel>
            <!-- 期間 -->
            <asp:Panel ID="WF_PERIOD_F" runat="server" CssClass="inputItem">
                <a id="WF_PERIOD_L">期間</a>
                <span class="ef" id="WF_FROM" ondblclick="Field_DBclick('WF_PERIOD_FROM', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="WF_PERIOD_FROM" runat="server" CssClass="calendarIcon" />
                </span>
                <asp:Label ID="WF_PERIOD" runat="server" Text="  ～  " ></asp:Label>
                <span class="ef" id="WF_TO" ondblclick="Field_DBclick('WF_PERIOD_TO', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="WF_PERIOD_TO" runat="server" CssClass="calendarIcon" />
                </span>
            </asp:Panel>
        </div> <!-- End inputBox -->
    </div> <!-- End searchbox -->

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />
    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />
    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- 非表示項目 -->
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
