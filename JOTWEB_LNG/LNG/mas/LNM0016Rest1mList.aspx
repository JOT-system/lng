﻿<%@ Page Title="LNM0016L" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNM0016Rest1mList.aspx.vb" Inherits="JOTWEB_LNG.LNM0016Rest1mList" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0016WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNM0016LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0016L.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0016L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="LNM0016L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <!-- 一覧件数 -->
                        <asp:Label ID="ListCount" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonINSERT"   class="btn-sticky" value="追加"     onclick="ButtonClick('WF_ButtonINSERT');" />
                        <input type="button" id="WF_ButtonHISTORY"  class="btn-sticky" value="変更履歴" onclick="ButtonClick('WF_ButtonHISTORY');" />
                        <%--<input type="button" id="WF_ButtonDebug" class="btn-sticky" value="デバッグ" onclick="ButtonClick('WF_ButtonDebug');" />--%>
                        <asp:Label ID="WF_UPLOAD_LABEL" AssociatedControlID="WF_UPLOAD_BTN" runat="server" CssClass="btn-sticky" Text="ｱｯﾌﾟﾛｰﾄﾞ"> <asp:FileUpload ID="WF_UPLOAD_BTN" runat="server"  onchange="ButtonClick('WF_ButtonUPLOAD')"/>
                        </asp:Label>
                        <input type="button" id="WF_ButtonDOWNLOAD" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonDOWNLOAD');" />
                        <input type="button" id="WF_ButtonPRINT"    class="btn-sticky" value="一覧印刷" onclick="ButtonClick('WF_ButtonPRINT');" />
                        <input type="button" id="WF_ButtonEND"      class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                        <div id="WF_ButtonFIRST" class="firstPage" runat="server"                       onclick="ButtonClick('WF_ButtonFIRST');"></div>
                        <div id="WF_ButtonLAST" class="lastPage" runat="server"                         onclick="ButtonClick('WF_ButtonLAST');"></div>
                    </div>
                </div> <!-- End class=actionButtonBox -->

            </div> <!-- End class="Operation" -->
            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
        </div>

        <!-- rightbox レイアウト -->
        <MSINC:rightview ID="rightview" runat="server" />

        <!-- leftbox レイアウト -->
        <MSINC:leftview ID="leftview" runat="server" />

        <!-- Work レイアウト -->
        <MSINC:wrklist ID="work" runat="server" />

        <!-- イベント用 -->
        <div style="display:none;">
            <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
            <!-- GridView DBクリック-->
            <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>
            <!-- GridView表示位置フィールド -->

            <input id="WF_FIELD" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->
            <input id="WF_FIELD_REP" runat="server" value="" type="text" />
            <!-- Textbox(Repeater) DBクリックフィールド -->
            <input id="WF_SelectedIndex" runat="server" value="" type="text" />
            <!-- Textbox DBクリックフィールド -->

            <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
            <!-- LeftBox Mview切替 -->
            <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
            <!-- LeftBox 開閉 -->
            <input id="WF_RightViewChange" runat="server" value="" type="text" />
            <!-- Rightbox Mview切替 -->
            <input id="WF_RightboxOpen" runat="server" value="" type="text" />
            <!-- Rightbox 開閉 -->

            <input id="WF_PrintURL" runat="server" value="" type="text" />
            <!-- Textbox Print URL -->

            <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />
            <!-- 一覧・詳細画面切替用フラグ -->

            <input id="WF_ButtonClick" runat="server" value="" type="text" />
            <!-- ボタン押下 -->
            <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
            <!-- 権限 -->
        </div>
 
</asp:Content>
