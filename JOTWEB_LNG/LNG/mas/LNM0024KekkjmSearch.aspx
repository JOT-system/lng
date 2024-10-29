<%@ Page Title="LNM0024S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master"  CodeBehind="LNM0024KekkjmSearch.aspx.vb" Inherits="JOTWEB_LNG.LNM0024KekkjmSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0024WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:content id="LNM0024SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0024S.css")%>' rel="stylesheet" type="text/css" />
</asp:content>

<asp:Content ID="LNM0024S" ContentPlaceHolderID="contents1" runat="server">
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
                <a class="ef" id="WF_TORICODE" ondblclick="Field_DBclick('txtToriCode', <%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);" onchange="CodeName_OnChange('txtToriCode','hdnSelectTori','txtToriCode','lblToriCode','txtToriCode',<%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);">
                    <asp:TextBox ID="txtToriCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                    <asp:DropDownList ID="hdnSelectTori" runat="server" ></asp:DropDownList>
                </a>
                <a id="WF_INVOICECAMPCD_TEXT">
                    <asp:textbox ID="lblToriCode" runat="server" CssClass="WF_TEXT"></asp:textbox>
                </a>
            </div>
                
            <!-- 請求書提出部店 -->
            <div class="inputItem">
                <a id="WF_INVFILINGDEPT_LABEL" >請求書提出部店</a>
                <a class="ef" id="WF_INVFILINGDEPT" ondblclick="Field_DBclick('txtInvFilingDept', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('txtInvFilingDept');">
                    <asp:TextBox ID="txtInvFilingDept" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_INVFILINGDEPT_TEXT">
                    <asp:Label ID="lblInvFilingDept" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 請求書決済区分 -->
            <div class="inputItem">
                <a id="WF_INVKESAIKBN_LABEL" >請求書決済区分</a>
                <a><asp:TextBox ID="txtInvKesaiKbn" runat="server" CssClass="WF_TEXTBOX_CSS" onblur="MsgClear();" MaxLength="2"></asp:TextBox></a>
                <a id="WF_INVKESAIKBN_TEXT">
                    <asp:Label ID="lblInvKesaiKbn" runat="server" CssClass="WF_TEXT"></asp:Label>
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
    <!-- 受託人単一選択 -->
    <MSINC:multiselect runat="server" id="mspTrusteeSingle" />
    <!-- 荷主単一選択 -->
    <MSINC:multiselect runat="server" id="mspShipperSingle" />
    <!-- 取引先単一選択 -->
    <MSINC:multiselect runat="server" id="mspToriSingle" />
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
