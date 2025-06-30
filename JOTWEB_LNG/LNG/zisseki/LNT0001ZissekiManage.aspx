<%@ Page Title="LNT0001L" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0001ZissekiManage.aspx.vb" Inherits="JOTWEB_LNG.LNT0001ZissekiManage" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightviewD" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightviewR" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0001TILESELECTORWRKINC.ascx" TagName="tilelist" TagPrefix="MSINC" %>

<asp:Content ID="LNT0001CH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/style.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNT0001L.css")%>' rel="stylesheet" type="text/css" />
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/index.js"></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/script/fixed_midashi.js")%>'></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0001L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="LNT0001C" ContentPlaceHolderID="contents1" runat="server">
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
                        <li class="breadcrumb-item d-flex align-items-center gap-1"><span class="material-symbols-outlined">home</span><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('WF_ButtonBackToMenu');">TOP</a></li>
                        <li class="breadcrumb-item active" aria-current="page">実績一覧</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title">実績一覧</h2>
                    <div class="Operation">
                        <div class="actionButtonBox">
                            <div class="rightSide">
                                <input type="button" id="WF_ButtonZisseki" class="btn-sticky btn-action" value="実績取込" onclick="ButtonClick('WF_ButtonZisseki');" />
                                <input type="button" id="WF_ButtonInvoice" class="btn-sticky btn-action" value="請求書出力" onclick="ButtonClick('WF_ButtonInvoice');" />
                            </div>
                        </div>
                    </div>
                    <div class="searchBar">
                        <!-- 作成日時 -->
                        <div id="actionTrigger" class="d-flex flex-wrap gap-3 w-100">
                            <div class="actionButtonBox">
                                <div class="d-flex align-items-center gap-2 me-3">
                                    <!-- 一覧件数 -->
                                    <asp:Label ID="ListCount" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </div>
                                <!-- 有効年月日(開始） -->
                                <div class="d-flex align-items-center gap-2 me-3">
                                    <strong class="flex-shrink-0">対象年月</strong>
                                    <div class="position-relative input-group calendar datetimepicker" data-target-input="nearest">
                                        <input type="text" id="WF_TaishoYm" runat="server" class="WF_TEXTBOX_CSS" data-input >
                                        <span class="input-group-text" data-toggle >
                                            <span class="material-symbols-outlined">calendar_month</span>
                                        </span>
                                    </div>
                                </div>
                                <div class="d-flex align-items-center gap-2">
                                    <strong class="flex-shrink-0">荷主</strong>
                                    <asp:DropDownList ID="WF_TORI" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TORI');" />
                                </div>
                                <input type="button" id="WF_ButtonExtract" class="btn-sticky btn-search" value="検索" onclick="ButtonClick('WF_ButtonExtract');" />
                                <div class="rightSide">
                                    <div id="WF_ButtonFIRST" class="firstPage" runat="server"                       onclick="ButtonClick('WF_ButtonFIRST');"></div>
                                    <div id="WF_ButtonLAST" class="lastPage" runat="server"                         onclick="ButtonClick('WF_ButtonLAST');"></div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- 一覧表示（共通） -->
                    <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
                </div>

                <!-- ページネーション -->
<%--                <div class="d-flex justify-content-center mt-3" aria-label="Page navigation">
                    <ul class="pagination">
                        <li class="page-item disabled">
                            <a class="page-link page-link-first" href="#" aria-label="First">
                                <span aria-hidden="true"></span>
                            </a>
                        </li>
                        <li class="page-item disabled">
                            <a class="page-link page-link-previous" href="#" aria-label="Previous">
                                <span aria-hidden="true"></span>
                            </a>
                        </li>
                        <li class="page-item active"><a class="page-link" href="#">1</a></li>
                        <li class="page-item"><a class="page-link" href="#">2</a></li>
                        <li class="page-item"><a class="page-link" href="#">3</a></li>
                        <li class="page-item d-flex align-items-end">…</li>
                        <li class="page-item"><a class="page-link" href="#">10</a></li>
                        <li class="page-item">
                            <a class="page-link page-link-next" href="#" aria-label="Next">
                                <span aria-hidden="true"></span>
                            </a>
                        </li>
                        <li class="page-item">
                            <a class="page-link page-link-last" href="#" aria-label="Last">
                                <span aria-hidden="true"></span>
                            </a>
                        </li>
                    </ul>
                </div>--%>
            </div>
        </div>

        </div>
        <!-- rightbox レイアウト -->
        <MSINC:rightviewD ID="rightviewD" runat="server" />
        <MSINC:rightviewR ID="rightviewR" runat="server" />

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
