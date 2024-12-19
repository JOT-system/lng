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
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNT0001D.css")%>' rel="stylesheet" type="text/css" />
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
                        <li class="breadcrumb-item active">実績管理</li>
                        <li class="breadcrumb-item active" aria-current="page">実績取込</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title">実績取込</h2>
                    <div class="Operation">
                        <div class="actionButtonBox">
                            <!-- 作成日時 -->
                            <div id="actionTrigger" class="d-flex flex-wrap gap-3 mt-3">
                                <div class="d-flex align-items-center gap-2 me-3">
                                    <strong class="flex-shrink-0">対象年月</strong>
                                    <div id="datetimepicker1" class="position-relative input-group calendar" data-target-input="nearest">
                                        <a ondblclick="Field_DBclick('WF_TaishoYm', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                                            <asp:TextBox  ID="WF_TaishoYm" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10" ></asp:TextBox>
                                        </a>
                                    </div>
                                </div>
                                <div class="d-flex align-items-center gap-2">
                                    <strong class="flex-shrink-0">荷主</strong>
                                    <asp:DropDownList ID="WF_TORI" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TORI');" />
                                </div>
                            </div>
                            <div class="rightSide">
                                <input type="button" id="WF_ButtonExtract" class="btn-sticky" value="絞り込み" onclick="ButtonClick('WF_ButtonExtract');" />
                                <input type="button" id="WF_ButtonKintone" class="btn-sticky" value="実績取込" onclick="ButtonClick('WF_ButtonKintone');" />
                                <input type="button" id="WF_ButtonZero" class="btn-sticky" value="実績数量ゼロ" onclick="ButtonClick('WF_ButtonZero');" />
                                <%--戻るボタンは、メニューへ、ログアウトボタンを追加するキーワードとして必要なので非表示とする--%>
                                <input type="button" id="WF_ButtonEND"      class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" /> 
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
    </div>
</asp:Content>
