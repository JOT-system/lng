<%@ Page Title="LNT0001C" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0001ZissekiIntake.aspx.vb" Inherits="JOTWEB_LNG.LNT0001ZissekiIntake" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightviewD" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightviewR" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0001TILESELECTORWRKINC.ascx" TagName="tilelist" TagPrefix="MSINC" %>

<asp:Content ID="LNT0001DH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/style.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNT0001D.css")%>' rel="stylesheet" type="text/css" />
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/index.js"></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/script/fixed_midashi.js")%>'></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0001D.js")%>'></script>
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
                        <li class="breadcrumb-item active"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNT0001L');">実績一覧</a></li>
                        <li class="breadcrumb-item active" aria-current="page">実績取込</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title">実績取込</h2>
                    <div class="Operation">
                        <div class="actionButtonBox">
                            <div class="rightSide">
                                <input type="button" id="WF_ButtonKintone" class="btn-sticky  btn-action" value="実績取込" onclick="ButtonClick('WF_ButtonKintone');" />
                                <input type="button" id="WF_ButtonZero" class="btn-sticky  btn-action" value="実績数量ゼロ" onclick="ButtonClick('WF_ButtonZero');" />
                                <input type="button" id="WF_ButtonNgPrint" class="btn-sticky  btn-action" value="実績不備データ" onclick="ButtonClick('WF_ButtonNgPrint');" />
                                <%--id="WF_ButtonEND"は、メニューへ、ログアウトボタンを追加するキーワードとなる--%>
                                <%--ここでは、ログアウトボタンを表示したくないためid="WF_ButtonEND2"とする--%>
                                <input type="button" id="WF_ButtonEND2"      class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" /> 
                            </div>
                        </div>
                    </div>
                    <div class="searchBar">
                        <!-- 作成日時 -->
                        <div id="actionTrigger" class="d-flex flex-wrap gap-3 w-100">
                            <div class="actionButtonBox">
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
                                    <div class="divItem">
                                        <div class="divDdlArea">
                                            <asp:TextBox ID="WF_TORI_L" class="txtlblbox" runat="server" ReadOnly="true" disabled="disabled" Text ="荷主"></asp:TextBox>
                                        </div>
                                        <a class="divDdlAreaLeft">
                                            <asp:ListBox ID="WF_TORI" runat="server" ClientIDMode="Predictable" CssClass="ddlSelectControl" SelectionMode="Multiple"></asp:ListBox>
                                        </a>
                                    </div>
                                </div>
                                <input type="button" id="WF_ButtonExtract" class="btn-sticky btn-search" value="検索" onclick="ButtonClick('WF_ButtonExtract');" />
                            </div>
                        </div>
                    </div>

                    <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
                </div>
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

        <input id="WF_TORIhdn" runat="server" value="" type="text" />
        <input id="WF_TORINAMEhdn" runat="server" value="" type="text" />
    </div>
</asp:Content>
