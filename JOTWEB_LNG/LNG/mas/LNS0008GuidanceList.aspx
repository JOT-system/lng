<%@ Page Title="LNS0008L" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNS0008GuidanceList.aspx.vb" Inherits="JOTWEB_LNG.LNS0008GuidanceList" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNS0008WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNS0008LH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNS0008L.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNS0008L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            // #contentsInnerの高さ取得
            let windowHeight = window.innerHeight;
            const headerHeight = 47;
            const breadcrumbHeight = 18;
            const spaceHeight = 8 + 16 + 16;
            let contentsInnerHeight = windowHeight - headerHeight - breadcrumbHeight - spaceHeight;
            document.getElementById("contentsInner").style.height = contentsInnerHeight + "px";

            // #fixed-tableの高さ取得
            const contentsTitleHeight = 44;
            let actionTriggerHeight = document.getElementById("actionTrigger").offsetHeight;
            const contentsSpaceHeight = 16 * 4;
            let fixedTableHeight = (contentsInnerHeight - contentsTitleHeight -actionTriggerHeight - contentsSpaceHeight) + "px";
            document.getElementById("fixed-table").style.height = fixedTableHeight;

            // カレンダー表示
            flatpickr('#datetimepicker1', {
                wrap: true,
                dateFormat: 'Y/m/d',
                locale : 'ja',
                clickOpens: false,
                allowInput: true,
                monthSelectorType: 'static',
                defaultDate: new Date()
            });
        });
    </script>
</asp:Content>
 
<asp:Content ID="LNS0008L" ContentPlaceHolderID="contents1" runat="server">
    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
    <!-- 全体レイアウト　headerbox -->
    <div class="d-inline-flex align-items-center flex-column w-100">
        <div class="d-flex w-100 wrap">
            <!-- サイドメニュー -->
            <MSINC:leftmenu ID="leftmenu" runat="server" />
            <!-- メイン画面（一覧） -->
            <div class="w-100 contents">
                <nav style="--bs-breadcrumb-divider: '>';" aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item d-flex align-items-center gap-1"><span class="material-symbols-outlined">home</span><a href="#">TOP</a></li>
                        <li class="breadcrumb-item active">ガイダンス検索</li>
                        <li class="breadcrumb-item active" aria-current="page">ガイダンス一覧</li>
                    </ol>
                </nav>
                <h2 class="w-100 fs-5 fw-bold contents-title">ガイダンス一覧</h2>
                <div class="headerboxOnly" id="headerbox">
                    <div class="Operation">
                        <div class="actionButtonBox">
                            <div class="leftSide">
                                <!-- 一覧件数 -->
                                <asp:Label ID="ListCount" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                            </div>
                            <div class="rightSide">
                                <input type="button" id="WF_ButtonINSERT"   class="btn-sticky" value="追加"     onclick="ButtonClick('WF_ButtonINSERT');" />
                                <input type="button" id="WF_ButtonEND"      class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                                <div id="WF_ButtonFIRST" class="firstPage" runat="server"                       onclick="ButtonClick('WF_ButtonFIRST');"></div>
                                <div id="WF_ButtonLAST" class="lastPage" runat="server"                         onclick="ButtonClick('WF_ButtonLAST');"></div>
                            </div>
                        </div> <!-- End class=actionButtonBox -->

                    </div> <!-- End class="Operation" -->
                    <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
                </div>
            </div>
        </div>
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
