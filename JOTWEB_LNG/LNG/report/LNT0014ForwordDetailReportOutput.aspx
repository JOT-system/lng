<%@ Page Title="LNT0014S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0014ForwordDetailReportOutput.aspx.vb" Inherits="JOTWEB_LNG.LNT0014ForwordDetailReportOutput" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0014WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>

<asp:Content id="LNT0014SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0014.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0014S.js")%>'></script>
</asp:Content>

<asp:Content ID="LNT0014S" ContentPlaceHolderID="contents1" runat="server">
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

            <!-- 対象支店 -->
            <div class="inputItem" id="WF_ORG_AREA">
                <a id="WF_ORG_LABEL">対象支店</a>
                <a class="ef" id="WF_ORG" ondblclick="Field_DBclick('TxtOrgCode', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtOrgCode');">
                    <asp:TextBox ID="TxtOrgCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_ORG_TEXT">
                    <asp:Label ID="LblOrgName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            
            <!-- 年月 -->
            <div class="inputItem" id="WF_DATE">
                <a id="WF_LABELDATE">計上年月</a>
                <div class="inputItem">
                <a></a>
                <a class ="ef" id ="WF_TARGETYM">
                    <asp:TextBox ID="TxtDownloadMonth" runat ="server" onblur="MsgClear();" MaxLength="7" data-monthpicker ="1"></asp:TextBox>
                </a>
                </div> 
            </div>

            <!-- 支払先 -->
            <div class="inputItem" id="WF_KEKKJM_PAY">
                <a id="WF_KEKKJM_LABELPAY">支払先</a>
                    <a class="ef" id="WF_Payment" ondblclick="Field_DBclick('TxtPayee', <%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);" onchange="CodeName_OnChange('TxtPayee','hdnSelectTori','TxtPayee','LblPayee','TxtPayee',<%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);">
                    <asp:TextBox ID="TxtPayee" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                    <asp:DropDownList ID="hdnSelectTori" runat="server" ></asp:DropDownList>
                </a>
                <a id="WF_ORG_TEXTPAY">
                    <asp:textbox ID="LblPayee" runat="server" CssClass="WF_TEXT" Text="" ></asp:textbox>
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
    <!-- 受託人単一選択 -->
    <MSINC:multiselect runat="server" id="mspTrusteeSingle" />
    <!-- 荷主単一選択 -->
    <MSINC:multiselect runat="server" id="mspShipperSingle" />
    <!-- 取引先単一選択 -->
    <MSINC:multiselect runat="server" id="mspToriSingle" />
    <!-- 駅単一選択 -->
    <MSINC:multiselect runat="server" id="mspStationSingle" />

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
    </div>
</asp:Content>
