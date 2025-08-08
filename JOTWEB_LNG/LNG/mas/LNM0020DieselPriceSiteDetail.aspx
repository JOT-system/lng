<%@ Page Title="LNM0020D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0020DieselPriceSiteDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0020DieselPriceSiteDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0020WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0020DH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/style.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNM0020D.css")%>' rel="stylesheet" type="text/css" />
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/index.js"></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0020D.js")%>'></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
</asp:Content>
 
<asp:Content ID="LNM0020D" ContentPlaceHolderID="contents1" runat="server">
    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
    <!-- 全体レイアウト　detailbox -->
    <div class="d-inline-flex align-items-center flex-column w-100">
        <div class="d-flex w-100 wrap">
            <!-- サイドメニュー -->
            <MSINC:leftmenu ID="leftmenu" runat="server" />
            <div class="w-100 contents">
                <nav style="--bs-breadcrumb-divider: '>';" aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item d-flex align-items-center gap-1"><span class="material-symbols-outlined">home</span><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('WF_ButtonBackToMenu');">TOP</a></li>
                        <li class="breadcrumb-item active"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNM0020L');">軽油価格参照先マスタ</a></li>
                        <li class="breadcrumb-item active" id="PAGE_NAME1" aria-current="page">軽油価格参照先マスタ（詳細）</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title" id="PAGE_NAME2">軽油価格参照先マスタ詳細</h2>
                    <div class="Operation">
                            <div class="actionButtonBox">
                                <div class="rightSide">
                                    <%--<input type="button" id="WF_ButtonUPDATE" class="btn-sticky btn-action" value="更新" onclick="ButtonClick('WF_ButtonUPDATE');" />--%>
                                    <input type="button" id="WF_ButtonCLEAR" class="btn-sticky" value="戻る"  onclick="ButtonClick('WF_ButtonCLEAR');" />
                                </div>
                            </div>
                    </div>

                        <div id="detailkeybox">
                            <!-- 画面ＩＤ -->
                            <span class="ef" style="display:none;">
                                <asp:Label ID="WF_MAPID_L" runat="server" Text="画面ＩＤ" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtMapId" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                                <asp:Label ID="WF_MAPID_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>

                            <p id="KEY_LINE_1" style="display:none;">
                                <!-- 選択No -->
                                <span>
                                    <asp:Label ID="WF_SEL_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSelLineCNT" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                                    <%--<asp:Label ID="LblSelLineCNT" runat="server" CssClass="WF_TEXT"></asp:Label>--%>
                                </span>
                            </p>
                            <div id="RAD_LINE_1">
                                <!-- 削除フラグ -->
                                <asp:Label ID="WF_DELFLG_L" runat="server" Text="有効/無効" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                <asp:RadioButtonList ID="RadioDELFLG" runat="server" RepeatDirection="Horizontal" CssClass="WF_RADIO">
                                <asp:ListItem Value="0">有効</asp:ListItem>
                                <asp:ListItem Value="1">無効(削除)</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <a  style="display:none;">
                            <!-- 会社コード -->
                            <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <asp:TextBox ID="TxtCampCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                            <asp:Label ID="LblCampCodeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </a>
                            <p id="TYPE_B_LINE_4">
                                <!-- 実勢軽油価格参照先ID -->
                                <span>
                                    <asp:Label ID="WF_DIESELPRICESITEID_L" runat="server" Text="参照先名" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:TextBox ID="WF_DIESELPRICESITENAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50" onBlur="ButtonClick('WF_DIESELPRICESITEChange');" ></asp:TextBox>
                                    <asp:TextBox ID="WF_DIESELPRICESITEID" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10" Enabled="false"></asp:TextBox>
                                </span>
                                <a style="display:none;">
                                    <asp:DropDownList ID="WF_DIESELPRICE" runat="server" class="form-select rounded-0" />
                                    <datalist id="WF_DIESELPRICE_DL" runat="server"></datalist>
                                </a>
                            </p>
                            <p id="TYPE_B_LINE_5">
                                <!-- 実勢軽油価格参照先ID枝番 -->
                                <span>
                                    <asp:Label ID="WF_DIESELPRICESITEBRANCH_L" runat="server" Text="区分名" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:TextBox ID="WF_DIESELPRICESITEKBNNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50" ></asp:TextBox>
                                    <asp:TextBox ID="WF_DIESELPRICESITEBRANCH" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_6">
                                <!-- 画面表示名称 -->
                                <span>
                                    <asp:Label ID="WF_DISPLAYNAME_L" runat="server" Text="画面表示名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="WF_DISPLAYNAME" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="true"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_8">
                                <!-- 実勢軽油価格参照先URL -->
                                <span>
                                    <asp:Label ID="WF_DIESELPRICESITEURL_L" runat="server" Text="サイトＵＲＬ" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="WF_DIESELPRICESITEURL" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="true"></asp:TextBox>
                                </span>
                            </p>
                            <div class="Operation">
                                <div class="actionButtonBox">
                                    <div class="centerSide">
                                        <input type="button" id="WF_ButtonUPDATE" class="btn-sticky btn-action" value="更新" onclick="ButtonClick('WF_ButtonUPDATE');" />
                                        <input type="button" id="WF_ButtonCANCEL" class="btn-sticky" value="キャンセル"  onclick="ButtonClick('WF_ButtonCLEAR');" />
                                    </div>
                                </div>
                            </div>

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

    <!-- multiSelect レイアウト -->
    <!-- 届先コード単一選択 -->
    <MSINC:multiselect runat="server" id="mspShukabashoSingle" />
    <!-- 届先コード単一選択 -->
    <MSINC:multiselect runat="server" id="mspTodokeCodeSingle" />

    <!-- イベント用 -->
    <div style="display:none;">

        <!-- 入力不可制御項目 -->
        <input id="DisabledKeyItem" runat="server" value="" type="text" />
        <input id="DisabledKeyOrgCount" runat="server" value="" type="text" />
        <input id="DisabledKeyToriCount" runat="server" value="" type="text" />

        <!-- GridView DBクリック-->
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
        <!-- GridView表示位置フィールド -->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>
            
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD" runat="server" value="" type="text" />
        <!-- Textbox(Repeater) DBクリックフィールド -->
        <input id="WF_FIELD_REP" runat="server" value="" type="text" />
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_SELectedIndex" runat="server" value="" type="text" />
            
        <!-- LeftBox Mview切替 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
        <!-- LeftBox 開閉 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
        <!-- Rightbox Mview切替 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />
        <!-- Rightbox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />
            
        <!-- Textbox Print URL -->
        <input id="WF_PrintURL" runat="server" value="" type="text" />
            
        <!-- 一覧・詳細画面切替用フラグ -->
        <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />
            
        <!-- ボタン押下 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />
        <!-- 前の遷移先 -->
        <input id="WF_BeforeMAPID" runat="server" value="" type="text" />
        <!-- 権限 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
        <!-- 値保持 -->
        <input id="WF_DIESELPRICESITENAME_SAVE" runat="server" value="" type="text" />
        <input id="WF_DIESELPRICESITEID_SAVE" runat="server" value="" type="text" />
        <input id="WF_DIESELPRICESITEKBNNAME_SAVE" runat="server" value="" type="text" />
        <input id="WF_DIESELPRICESITEBRANCH_SAVE" runat="server" value="" type="text" />

        <!-- 縦スクロール位置 -->
        <input id="WF_scrollY" runat="server" value="0" type="text" />

    </div>
 
</asp:Content>
