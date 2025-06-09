<%@ Page Title="LNM0007D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0007KoteihiDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0007KoteihiDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0007WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0007DH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/style.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNM0007D.css")%>' rel="stylesheet" type="text/css" />
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/index.js"></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0007D.js")%>'></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
</asp:Content>
 
<asp:Content ID="LNM0007D" ContentPlaceHolderID="contents1" runat="server">
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
                        <li class="breadcrumb-item active"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNM0007L');">固定費マスタ</a></li>
                        <li class="breadcrumb-item active" id="PAGE_NAME1" aria-current="page">固定費マスタ（詳細）</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title" id="PAGE_NAME2">固定費マスタ詳細</h2>
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
                            <p id="TYPE_A_LINE_4">
                                <!-- 対象年月 -->
                                <span>
                                    <asp:Label ID="WF_TARGETYM_L" runat="server" Text="対象年月" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <b class="calendararea">
                                        <b class="position-relative input-group calendar datetimepicker" data-target-input="nearest">
                                            <input type="text" id="WF_TARGETYM" runat="server" class="WF_TEXTBOX_CSS" onchange="ButtonClick('WF_SelectCALENDARChange');" data-input>
                                            <span id="WF_StYMD_CALENDAR" class="input-group-text" data-toggle>
                                                <span class="material-symbols-outlined">calendar_month</span>
                                            </span>
                                        </b>
                                    </b>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_5">
                                <!-- 取引先コード -->
                                <span>
                                    <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先名称" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:TextBox ID="WF_TORINAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50" onBlur="ButtonClick('WF_TORIChange');" ></asp:TextBox>
                                    <asp:TextBox ID="WF_TORICODE_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                                </span>
                                <a style="display:none;">
                                    <asp:DropDownList ID="WF_TORI" runat="server" class="form-select rounded-0" />
                                    <datalist id="WF_TORI_DL" runat="server"></datalist>
                                </a>
                            </p>
                            <p id="TYPE_B_LINE_7">
                                <!-- 部門コード -->
                                <span>
                                    <asp:Label ID="WF_ORGCODE_L" runat="server" Text="担当部門名" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_ORG" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_ORGChange');" />
                                    <asp:TextBox ID="WF_ORGCODE_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_B_LINE_8">
                                <!-- 加算先部門コード -->
                                <span>
                                    <asp:Label ID="WF_KASANORGCODE_L" runat="server" Text="加算先部門名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_KASANORG" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_KASANORGChange');" />
                                    <asp:TextBox ID="WF_KASANORGCODE_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>

                            <p id="TYPE_C_LINE_1">
                                <span>
                                    <!-- 陸事番号 -->
                                    <asp:Label ID="WF_RIKUBAN_L" runat="server" Text="陸事番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtRIKUBAN" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                                    <!-- 車番 -->
                                    <asp:Label ID="WF_SYABAN_L" runat="server" Text="業務車番" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:TextBox ID="TxtSYABAN" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                                    <!-- 車腹 -->
                                    <asp:Label ID="WF_SYABARA_L" runat="server" Text="車腹" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSYABARA" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                                    <asp:Label ID="WF_SYABARA_TEXT" runat="server" Text="ｔ" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <p id="TYPE_D_LINE_9">
                                <span>
                                    <!-- 車型 -->
                                    <asp:Label ID="WF_SYAGATA_L" runat="server" Text="車型名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_SYAGATA" runat="server" class="form-select rounded-0" onchange="f_syagata()">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                        <asp:ListItem Text="単車" Value="1"></asp:ListItem>
                                        <asp:ListItem Text="トレーラ" Value="2"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:TextBox ID="WF_SYAGATA_CODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                             <p id="TYPE_D_LINE_16">
                                <!-- 季節料金判定区分 -->
                                <span>
                                    <asp:Label ID="WF_SEASONKBN_L" runat="server" Text="季節料金判定" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_SEASONKBN" runat="server" class="form-select rounded-0" onchange="f_seasonkbn()">
                                        <asp:ListItem Text="通年" Value="0"></asp:ListItem>
                                        <asp:ListItem Text="夏季料金" Value="1"></asp:ListItem>
                                        <asp:ListItem Text="冬季料金" Value="2"></asp:ListItem>
                                    </asp:DropDownList>

                                </span>
                            </p>
                             <p id="TYPE_E_LINE_17">
                                <!-- 季節料金判定開始月日、季節料金判定終了月日 -->
                                <span>
                                    <asp:Label ID="WF_SEASONSTART_L" runat="server" Text="季節料金判定開始月日<br>(MMDD形式)" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSEASONSTART" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                                    <asp:Label ID="WF_SEASONEND_L" runat="server" Text="季節料金判定終了月日<br>(MMDD形式)" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSEASONEND" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_F_LINE_19">
                                <!-- 固定費(月額) 、固定費(日額)-->
                                <span>
                                    <!-- 月額固定費 -->
                                    <asp:Label ID="WF_KOTEIHIM_L" runat="server" Text="固定費(月額)" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtKOTEIHIM" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="8"></asp:TextBox>
                                    <asp:Label ID="WF_KOTEIHIM_TEXT" runat="server" Text="円" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <!-- 日額固定費 -->
                                    <asp:Label ID="WF_KOTEIHID_L" runat="server" Text="固定費(日額)" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtKOTEIHID" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="8"></asp:TextBox>
                                    <asp:Label ID="WF_KOTEIHID_TEXT" runat="server" Text="円" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <p id="TYPE_G_LINE_20">
                                <!-- 回数 、減額費用-->
                                <span>
                                    <asp:Label ID="WF_KAISU_L" runat="server" Text="回数" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtKAISU" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"　Enabled="false"></asp:TextBox>
                                    <asp:Label ID="WF_GENGAKU_L" runat="server" Text="減額費用" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtGENGAKU" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10" Enabled="false"></asp:TextBox>
                                    <asp:Label ID="WF_GENGAKU_TEXT" runat="server" Text="円" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <p id="TYPE_H_LINE_23">
                                <!-- 請求額 -->
                                <span>
                                    <asp:Label ID="WF_AMOUNT_L" runat="server" Text="請求金額" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtAMOUNT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10" Enabled="false"></asp:TextBox>
                                    <asp:Label ID="TxtAMOUNT_TEXT" runat="server" Text="円" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <p id="TYPE_I_LINE_24">
                                <!-- 備考1 -->
                                <span>
                                    <asp:Label ID="WF_BIKOU1_L" runat="server" Text="備考1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_I_LINE_25">
                                <!-- 備考2 -->
                                <span>
                                    <asp:Label ID="WF_BIKOU2_L" runat="server" Text="備考2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_I_LINE_26">
                                <!-- 備考3 -->
                                <span>
                                    <asp:Label ID="WF_BIKOU3_L" runat="server" Text="備考3" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
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
        <input id="WF_TORINAME_SAVE" runat="server" value="" type="text" />
        <input id="WF_TORICODE_TEXT_SAVE" runat="server" value="" type="text" />
        <input id="WF_ORG_SAVE" runat="server" value="" type="text" />
        <input id="WF_SEASONKBN_SAVE" runat="server" value="" type="text" />

    </div>
 
</asp:Content>
