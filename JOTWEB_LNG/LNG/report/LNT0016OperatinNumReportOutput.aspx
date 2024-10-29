<%@ Page Title="LNT0016S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0016OperatinNumReportOutput.aspx.vb" Inherits="JOTWEB_LNG.LNT0016OperatinNumReportOutput" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0016WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>

<asp:Content id="LNT0016SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0016.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0016S.js")%>'></script>
</asp:Content>

<asp:Content ID="LNT0016S" ContentPlaceHolderID="contents1" runat="server">
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
            
            <!-- 年月 -->
            <asp:Panel ID="pnlDetailboxLine1" CssClass="detailboxLineLabel" runat="server">
                <span class="spanLeft"></span>
                <div class="divYM">
                    <a id="WF_YM_LABEL">年月</a>
                </div>
            </asp:Panel>
             <asp:Panel ID="pnlDetailboxLine2" CssClass="detailboxLineInput" runat="server">      
                <span class="spanLeft"></span>
                <a class="inputItem" id="WF_TARGETYM_AREA">
                    <asp:TextBox ID="txtDownloadMonth" class="txtDownloadMonth" runat="server" data-monthpicker="1"></asp:TextBox>
                </a>
            </asp:Panel>

            <!-- コンテナ種別 -->
            <div class="inputItem" id="WF_CTN_AREA">
                <a id="WF_CTN_LABEL">コンテナ種別</a>
                <a class="ef" id="WF_CTN" ondblclick="Field_DBclick('TxtCtnClass', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtCtnClass');">
                    <asp:TextBox ID="TxtCtnClass" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_ORG_TEXT">
                    <asp:Label ID="LblCtnClass" runat="server" CssClass="WF_TEXT"></asp:Label>
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
