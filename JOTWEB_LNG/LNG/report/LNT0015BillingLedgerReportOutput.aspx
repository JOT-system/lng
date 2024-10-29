<%@ Page Title="LNT0015S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0015BillingLedgerReportOutput.aspx.vb" Inherits="JOTWEB_LNG.LNT0015BillingLedgerReportOutput" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0015WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>

<asp:Content id="LNT0015SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0015.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0015S.js")%>'></script>
</asp:Content>

<asp:Content ID="LNT0015S" ContentPlaceHolderID="contents1" runat="server">
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

            <!-- 請求提出部店 -->
            <div class="inputItem" id="WF_ORG_AREA">
                <a></a>
                <a id="WF_ORG_LABEL">請求提出部店</a>
                <a id="WF_ORG_MASSEAGE">※支店は1支店選択または全支店選択</a>
            </div>
            <div id="WF_ORGCODE_ALL_SELECT">
                <MSINC:tilelist ID="WF_ORGCODE_ALL" runat="server" />
            </div>
            <div id="WF_ORGCODE_SELECT">
                <MSINC:tilelist ID="WF_ORGCODE" runat="server" />
            </div>
            
            <!-- 年月 -->
            <div class="inputItem" id="WF_DATE">
                <a id="WF_LABELDATE">年月</a>
                <a class="DDLYMArea" id="WFDATE">
                    <asp:TextBox ID="txtDownloadMonth" class="txtDownloadMonth" runat="server" data-monthpicker="1"></asp:TextBox>
                </a>
            </div>

            <!-- 請求書種類 -->
            <div class="inputItem" id="WF_INVOICETYPE">
                <a id="WF_INVOICE_TYPE">請求書種類</a>
            </div>
            <span class="spanLeft"></span>
                <div class="singleInput">
                    <!-- 選択ボタン -->
                    <div class="right-harf">
                        <MSINC:tilelist ID="WF_INVTYPE" runat="server"/>
                    </div>
                </div>
            
            <!-- 出力条件 -->
            <div class="inputItem" id="WF_ALL">
                <asp:CheckBox runat="server" ID="CHKALL"></asp:CheckBox>
                <a id="WF_ALLOUTPUT">　申請・承認していないものも含める</a>
                <a class="inputItem" id="WF_TARGETALL_AREA">
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
        <input id="WF_PrintURL3" runat="server" value="" type="text" />
        <input id="WF_PrintURL4" runat="server" value="" type="text" />
    </div>
</asp:Content>
