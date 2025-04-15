<%@ Page Title="LNT0001AJ" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0001ZissekiAjustMap.aspx.vb" Inherits="JOTWEB_LNG.LNT0001ZissekiAjustMap_aspx" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0001TILESELECTORWRKINC.ascx" TagName="tilelist" TagPrefix="MSINC" %>

<asp:Content ID="LNT0001AJH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNT0001AJ.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/script/fixed_midashi.js")%>'></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0001AJ.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
<asp:Content ID="LNT0001AJ" ContentPlaceHolderID="contents1" runat="server">
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
                        <li class="breadcrumb-item active"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNT0001L');">実績管理</a></li>
                        <li class="breadcrumb-item active" aria-current="page">調整画面</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title">実績単価調整画面</h2>
                    <div class="headerboxOnly" id="headerbox">
                        <div class="Operation">
                            <div class="actionButtonBox">
                                <!-- 対象年月 -->
                                <div id="actionTrigger" class="d-flex flex-wrap gap-3">
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
                                        <div style="display:none;">
                                        <strong class="flex-shrink-0">荷主</strong>
                                        <asp:DropDownList ID="WF_TORI" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TORI');" />
                                        <asp:DropDownList ID="WF_TORIEXL" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TORI');" />
                                        <asp:DropDownList ID="WF_FILENAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TORI');" />
                                        <asp:DropDownList ID="WF_TORIORG" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TORI');" />
                                        </div>
                                    </div>
                                </div>
                                <div class="rightSide">
                                    <input type="button" id="WF_ButtonUPDATE" class="btn-sticky btn-action" value="保存" onclick="ButtonClick('WF_ButtonUPDATE');" />
                                    <input type="button" id="WF_ButtonCLEAR" class="btn-sticky" value="戻る"  onclick="ButtonClick('WF_ButtonCLEAR');" />
                                    <%--戻るボタンは、メニューへ、ログアウトボタンを追加するキーワードとして必要なので非表示とする--%>
                                    <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" hidden="hidden"/> 
                                </div>
                            </div> <!-- End class=actionButtonBox -->
                        </div> <!-- End class="Operation" -->

                        <div id="tab1" class="tabBox">
                            <div class="d-flex align-items-center gap-2">
                                <strong class="flex-shrink-0">対象</strong>
                                <asp:DropDownList ID="WF_TARGETTABLE" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TARGETTABLEChange');" />
                            </div>
                        </div>
                        <%-- 特別料金(検索エリア) --%>
<%--                        <asp:Panel ID="pnlSpecialFEEArea" runat="server">
                        </asp:Panel>--%>
                        <%-- 単価調整(検索エリア) --%>
                        <asp:Panel ID="pnlPriceArea" runat="server">
                            <div id="tab2" class="tabBox">
                                <div class="d-flex align-items-center gap-2">
    <%--                                <strong class="flex-shrink-0">対象</strong>
                                    <asp:DropDownList ID="WF_TARGETTABLE" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TARGETTABLEChange');" />--%>
                                    <strong class="flex-shrink-0">届先</strong>
    <%--                                <asp:DropDownList ID="ddlTODOKE" runat="server" class="form-select rounded-0" />--%>
                                    <div class="divItem">
                                        <a class="divDdlAreaLeft">
                                            <asp:ListBox ID="ddlTODOKE" runat="server" ClientIDMode="Predictable" CssClass="ddlSelectControl" SelectionMode="Multiple"></asp:ListBox>
                                        </a>
                                    </div>
                                    <strong class="flex-shrink-0">　範囲</strong>
                                    <asp:DropDownList ID="ddlDayFirst" runat="server" class="form-select rounded-0" />
                                    <strong class="flex-shrink-0">～</strong>
                                    <asp:DropDownList ID="ddlDayEnd" runat="server" class="form-select rounded-0" />
                                    <strong class="flex-shrink-0">　陸事番号</strong>
    <%--                                <asp:DropDownList ID="ddlTANKNUMBER" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TANKNUMBERChange');" />--%>
                                    <div class="divItem">
                                        <a class="divDdlAreaLeft">
                                            <asp:ListBox ID="ddlTANKNUMBER" runat="server" ClientIDMode="Predictable" CssClass="ddlSelectControl" SelectionMode="Multiple"></asp:ListBox>
                                        </a>
                                    </div>
                                    <strong class="flex-shrink-0">　業務車番</strong>
    <%--                                <asp:DropDownList ID="ddlGYOMUTANKNUM" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_GYOMUTANKNUMChange');" />--%>
                                    <div class="divItem">
                                        <a class="divDdlAreaLeft">
                                            <asp:ListBox ID="ddlGYOMUTANKNUM" runat="server" ClientIDMode="Predictable" CssClass="ddlSelectControl" SelectionMode="Multiple"></asp:ListBox>
                                        </a>
                                    </div>
                                    <input type="button" id="WF_ButtonSearch" class="btn-sticky btn-search" value="検索" onclick="ButtonClick('WF_ButtonSearch');" />
                                    <input type="button" id="WF_ButtonRelease" class="btn-sticky" value="解除"  onclick="ButtonClick('WF_ButtonRelease');" />
                                </div>
                            </div>
                        </asp:Panel>
                        <%-- 固定費調整(検索エリア) --%>
<%--                        <asp:Panel ID="pnlFixedCostsArea" runat="server">
                        </asp:Panel>--%>
                        <%-- サーチャージ(検索エリア) --%>
<%--                        <asp:Panel ID="pnlSurchargeArea" runat="server">
                        </asp:Panel>--%>
                        <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
                    </div>
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

        <input id="WF_TODOKECODEhdn" runat="server" value="" type="text" />
        <input id="WF_TODOKENAMEhdn" runat="server" value="" type="text" />
        <input id="WF_TANKNUMBERhdn" runat="server" value="" type="text" />
        <input id="WF_GYOMUTANKNOhdn" runat="server" value="" type="text" />

    </div>
</asp:Content>

