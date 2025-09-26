<%@ Page Title="LNM0017D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0017HolidayRateDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0017HolidayRateDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0017WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0017DH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/style.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNM0017D.css")%>' rel="stylesheet" type="text/css" />
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/index.js"></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0017D.js")%>'></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
</asp:Content>
 
<asp:Content ID="LNM0017D" ContentPlaceHolderID="contents1" runat="server">
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
                        <li class="breadcrumb-item active"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNM0017L');">休日割増単価マスタ</a></li>
                        <li class="breadcrumb-item active" id="PAGE_NAME1" aria-current="page">休日割増単価マスタ（詳細）</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title" id="PAGE_NAME2">休日割増単価マスタ詳細</h2>
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
                            <!-- ＩＤ -->
                            <span style="display:none;">
                                <asp:Label ID="WF_ID_L" runat="server" Text="ＩＤ" CssClass="WF_TEXT_RIGHT"></asp:Label>
                                <asp:TextBox ID="WF_ID" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                            </span>
                            <p id="TYPE_B_LINE_4">
                                <!-- 取引先コード -->
                                <span>
                                    <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先名称" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_TORICODE" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TORIChange');" />
                                    <asp:TextBox ID="WF_TORICODE_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_5">
                                <!-- 受注受付部署コード -->
                                <span>
                                    <asp:Label ID="WF_ORDERORGCODE_L" runat="server" Text="受注受付部署名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_ORDERORGCODE" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_ORGChange');" />
                                    <asp:TextBox ID="WF_ORDERORGCODE_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_6">
                                <!-- 受注受付部署判定区分 -->
                                <span>
                                    <asp:Label ID="WF_ORDERORGCATEGORY_L" runat="server" Text="受注受付部署判定区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_ORDERORGCATEGORY" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_ORDERORGCATEGORYChange');" />
                                    <asp:TextBox ID="WF_ORDERORGCATEGORY_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_7">
                                <!-- 出荷場所コード -->
<%--                                <span>
                                    <asp:Label ID="Label2" runat="server" Text="出荷場所名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <a class="ef" id="WF_SHUKABASHO_L" ondblclick="Field_DBclick('WF_SHUKABASHO_TEXT', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_SHUKABASHO_TEXT');">
                                        <asp:TextBox ID="WF_SHUKABASHO" runat="server" CssClass="boxIcon" onblur="MsgClear();" ></asp:TextBox>
                                    </a>
                                    <asp:TextBox ID="WF_SHUKABASHO_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>--%>
                                <span>
                                    <asp:Label ID="WF_SHUKABASHO_L" runat="server" Text="出荷場所名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_SHUKABASHO" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_SHUKABASHOChange');" />
                                    <asp:TextBox ID="WF_SHUKABASHO_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_8">
                                <!-- 出荷場所判定区分 -->
                                <span>
                                    <asp:Label ID="WF_SHUKABASHOCATEGORY_L" runat="server" Text="出荷場所判定区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_SHUKABASHOCATEGORY" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_SHUKABASHOCATEGORYChange');" />
                                    <asp:TextBox ID="WF_SHUKABASHOCATEGORY_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_9">
                                <!-- 届先コード -->
                                <span>
                                    <asp:Label ID="Label1" runat="server" Text="届先名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <a class="ef" id="WF_TODOKECODE_L" ondblclick="Field_DBclick('WF_TODOKECODE_TEXT', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('WF_TODOKECODE_TEXT');">
                                        <asp:TextBox ID="WF_TODOKECODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" autocomplete="off"></asp:TextBox>
                                    </a>
                                    <asp:TextBox ID="WF_TODOKECODE_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_10">
                                <!-- 届先判定区分 -->
                                <span>
                                    <asp:Label ID="WF_TODOKECATEGORY_L" runat="server" Text="届先判定区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_TODOKECATEGORY" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TODOKECATEGORYChange');"　 />
                                    <asp:TextBox ID="WF_TODOKECATEGORY_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_11">
                                <!-- 休日範囲 -->
                                <span>
                                    <asp:Label ID="WF_RANGECODE_L" runat="server" Text="休日範囲" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:CheckBoxList ID="WF_RANGECODE" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal" CssClass="checkboxlist-inline">
                                        <asp:ListItem Text="日曜" Value="1"></asp:ListItem>
                                        <asp:ListItem Text="祝日" Value="2"></asp:ListItem>
                                        <asp:ListItem Text="元日" Value="3"></asp:ListItem>
                                        <asp:ListItem Text="年末年始" Value="4"></asp:ListItem>
                                        <asp:ListItem Text="メーデー" Value="5"></asp:ListItem>
                                    </asp:CheckBoxList>
                                </span>
                            </p>
                            <p id="TYPE_F_LINE_12">
                                <!-- 車番（開始） 、車番（終了）-->
                                <span>
                                    <!-- 車番（開始） -->
                                    <asp:Label ID="WF_GYOMUTANKNUMFROM_L" runat="server" Text="車番（開始）" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="WF_GYOMUTANKNUMFROM" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="8"  autocomplete="off"></asp:TextBox>
                                    <asp:Label ID="WF_GYOMUTANKNUMFROM_TEXT" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <!-- 車番（終了） -->
                                    <asp:Label ID="WF_GYOMUTANKNUMTO_L" runat="server" Text="車番（終了）" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="WF_GYOMUTANKNUMTO" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="8"  autocomplete="off"></asp:TextBox>
                                    <asp:Label ID="WF_GYOMUTANKNUMTO_TEXT" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <p id="TYPE_F_LINE_13">
                                <!-- 単価-->
                                <span>
                                    <!-- 単価 -->
                                    <asp:Label ID="WF_TANKA_L" runat="server" Text="単価" CssClass="WF_TEXT_LEFT  requiredMark"></asp:Label>
                                    <asp:TextBox ID="WF_TANKA" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="8"  autocomplete="off"></asp:TextBox>
                                    <asp:Label ID="WF_TANKA_TEXT" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label>
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
        <input id="WF_TARGETYM_SAVE" runat="server" value="" type="text" />
        <input id="WF_TORICODE_SAVE" runat="server" value="" type="text" />
        <input id="WF_TORICODE_TEXT_SAVE" runat="server" value="" type="text" />
        <input id="WF_ORG_SAVE" runat="server" value="" type="text" />
        <input id="WF_SEASONKBN_SAVE" runat="server" value="" type="text" />

        <!-- 縦スクロール位置 -->
        <input id="WF_scrollY" runat="server" value="0" type="text" />

    </div>
 
</asp:Content>
