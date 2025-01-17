<%@ Page Title="LNM0006S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master"  CodeBehind="LNM0006TankaSearch.aspx.vb" Inherits="JOTWEB_LNG.LNM0006TankaSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0006WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:content id="LNM0006SH" contentplaceholderid="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/style.css" />
    <link href='<%=ResolveUrl("~/LNG/css/LNM0006S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0006S.js")%>'></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/index.js"></script>
</asp:content>

<asp:Content ID="LNM0006S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="d-inline-flex align-items-center flex-column w-100">
        <div class="d-flex w-100 wrap">
            <!-- サイドメニュー -->
            <MSINC:leftmenu ID="leftmenu" runat="server" />
            <div class="w-100 contents">
                <nav style="--bs-breadcrumb-divider: '>';" aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item d-flex align-items-center gap-1"><span class="material-symbols-outlined">home</span><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('WF_ButtonBackToMenu');">TOP</a></li>
                        <li class="breadcrumb-item active" aria-current="page">単価マスタ（検索）</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title">単価マスタ検索</h2>
                    <div class="searchbox" id="searchbox">
                        <!-- ○ 固定項目 ○ -->
<%--                        <div class="actionButtonBox">
                            <div class="rightSide">
                                <input type="button" id="WF_ButtonSEARCH" class="btn-sticky" value="検索" onclick="ButtonClick('WF_ButtonSEARCH');" />
                                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
                            </div>
                        </div> <!-- End actionButtonBox -->--%>

                        <!-- ○ 変動項目 ○ -->
                        <div class="inputBox">
                            <!-- 会社コード -->
                            <div class="inputItem" style="display:none;">
                                <a id="WF_CAMPCODE_LABEL">
                                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社コード"></asp:Label>
                                </a>
                                <a id="WF_CAMPCODE">
                                    <asp:TextBox ID="TxtCampCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2" Enabled="false"></asp:TextBox>
                                </a>
                                <a id="WF_CAMPCODE_TEXT">
                                    <asp:Label ID="LblCampCodeName" runat="server" CssClass="WF_TEXT"></asp:Label>
                                </a>
                            </div>

                            <!-- 有効開始日 -->
                            <div class="inputItem">
                                <a id="WF_STYMD_LABEL">有効開始日</a>                
                                <div class="position-relative input-group calendar datetimepicker" data-target-input="nearest">
                                    <input type="text" id="WF_StYMDCode" runat="server" class="WF_TEXTBOX_CSS" data-input>
                                    <span class="input-group-text" data-toggle>
                                        <span class="material-symbols-outlined">calendar_month</span>
                                    </span>
                                </div>
                            </div>

                            <!-- 取引先コード -->
                            <div class="inputItem">
                                <a id="WF_TORI_LABEL" >取引先コード</a>
                                <a class="ef" id="WF_TORI" ondblclick="Field_DBclick('TxtTORICode', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtTORICode');">
                                    <asp:TextBox ID="TxtTORICode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                                </a>
                                <a id="WF_TORI_TEXT">
                                    <asp:Label ID="LblTORIName" runat="server" CssClass="WF_TEXT"></asp:Label>
                                </a>
                            </div>

                            <!-- 部門コード -->
                            <div class="inputItem">
                                <a id="WF_ORG_LABEL" >部門名称</a>
                                <asp:DropDownList ID="ddlSelectORG" runat="server" ClientIDMode="Predictable" CssClass="ddlSelectControl"/>
                            </div>

                            <!-- 論理削除フラグ -->
                            <div class="inputItem">
                                <a id="WF_DELDATAFLG">
                                    <asp:CheckBox ID="ChkDelDataFlg" runat="server" Text="削除行を含む" />
                                </a>
                            </div>
                        </div> <!-- End inputBox -->

                        <div class="actionButtonBox">
                            <div class="centerSide">
                                <%--<input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />--%>
                                <input type="button" id="WF_ButtonEND2" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
                                <input type="button" id="WF_ButtonSEARCH" class="btn-sticky btn-action" value="検索" onclick="ButtonClick('WF_ButtonSEARCH');" />
                            </div>
                        </div> <!-- End actionButtonBox -->

                    </div> <!-- End searchbox -->
                </div>
            </div>
        </div>
    </div>
    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- multiSelect レイアウト -->
    <!-- 取引先部門コード単一選択 -->
    <MSINC:multiselect runat="server" id="mspToriOrgCodeSingle" />

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
