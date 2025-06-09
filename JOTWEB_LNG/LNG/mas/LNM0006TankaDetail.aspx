<%@ Page Title="LNM0006D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0006TankaDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0006TankaDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0006WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0006DH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNM0006D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0006D.js")%>'></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
</asp:Content>
 
<asp:Content ID="LNM0006D" ContentPlaceHolderID="contents1" runat="server">
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
                        <li class="breadcrumb-item active" id="PAGE_SEARCH"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNM0006S');">単価マスタ（検索）</a></li>
                        <li class="breadcrumb-item active"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNM0006L');">単価マスタ（一覧）</a></li>
                        <li class="breadcrumb-item active" id="PAGE_NAME1" aria-current="page">単価マスタ（詳細）</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title" id="PAGE_NAME2">単価マスタ詳細</h2>
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
                            <div id="TYPE_A_LINE_1">
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

                            <p id="TYPE_B_LINE_1">
                                <span class="ef">
                                    <!-- 有効開始日 -->
                                    <asp:Label ID="WF_STYMD_L" runat="server" Text="有効開始日" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <b class="calendararea">
                                        <b class="position-relative input-group calendar datetimepicker" data-target-input="nearest">
                                            <input type="text" id="WF_StYMD" runat="server" class="WF_TEXTBOX_CSS" data-input>
                                            <span id="WF_StYMD_CALENDAR" class="input-group-text" data-toggle>
                                                <span class="material-symbols-outlined">calendar_month</span>
                                            </span>
                                        </b>
                                    </b>
                                    <!-- 有効終了日 -->
                                    <asp:Label ID="WF_ENDYMD_L" runat="server" Text="有効終了日" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <b class="calendararea">
                                        <b class="position-relative input-group calendar datetimepicker" data-target-input="nearest">
                                            <input type="text" id="WF_EndYMD" runat="server" class="WF_TEXTBOX_CSS" data-input>
                                            <span id="WF_ENDYMD_CALENDAR" class="input-group-text" data-toggle>
                                                <span class="material-symbols-outlined">calendar_month</span>
                                            </span>
                                        </b>
                                    </b>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_1">
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
                            <p id="TYPE_C_LINE_2">
                                <!-- 部門コード -->
                                <span>
                                    <asp:Label ID="Label1" runat="server" Text="担当部門名" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_ORG" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_ORGChange');" />
                                    <asp:TextBox ID="WF_ORGCODE_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_3">
                                <!-- 加算先部門コード -->
                                <span>
                                    <asp:Label ID="WF_KASANORGCODE_L" runat="server" Text="加算先部門名" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_KASANORG" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_KASANORGChange');" />
                                    <asp:TextBox ID="WF_KASANORGCODE_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_4">
                                <!-- 出荷場所 -->
                                <span>
                                    <asp:Label ID="WF_AVOCADOSHUKABASHO_L" runat="server" Text="出荷場所名" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:TextBox ID="WF_AVOCADOSHUKANAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20" onBlur="ButtonClick('WF_AVOCADOSHUKAChange');" ></asp:TextBox>
                                    <asp:TextBox ID="WF_AVOCADOSHUKABASHO_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>
                                </span>
                                <a style="display:none;">
                                    <asp:DropDownList ID="WF_AVOCADOSHUKA" runat="server" class="form-select rounded-0" />
                                    <datalist id="WF_AVOCADOSHUKA_DL" runat="server"></datalist>
                                </a>
                            </p>
                            <p id="TYPE_G_LINE_1">
                                <span>
                                    <input type="checkbox" id="WF_SHUKACHANGE"  onchange="ChkShukaChange();" />
                                    <asp:Label ID="WF_SHUKA_CHANGE" runat="server" Text="輸送費明細上でマスタとは異なる出荷場所名を使用している場合" CssClass="WF_TEXT_LEFT"></asp:Label>

                                </span>
                            </p>
                            <div id="ShukaChangeArea" style="display:none;">
                                <p id="TYPE_H_LINE_1">
                                    <!-- 変換後出荷場所名 -->
                                    <span>
                                        <asp:Label ID="WF_SHUKANAME_L" runat="server" Text="変換後出荷場所名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        <asp:TextBox ID="TxtSHUKANAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                                    </span>
                                </p>
                                <p id="TYPE_I_LINE_1">
                                    <!-- 変換後出荷コード -->
                                    <span>
                                        <asp:Label ID="WF_SHUKABASHO_L" runat="server" Text="変換後出荷コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        <asp:TextBox ID="TxtSHUKABASHO" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>
                                    </span>
                                </p>
                            </div>
                            <p id="TYPE_J_LINE_1">
                                <span>
                                    <asp:Label ID="WF_AVOCADOTODOKECODE_L" runat="server" Text="届先名" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:TextBox ID="WF_AVOCADOTODOKENAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20" onBlur="ButtonClick('WF_AVOCADOTODOKEChange');" ></asp:TextBox>
                                    <asp:TextBox ID="WF_AVOCADOTODOKECODE_TEXT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>
                                </span>
                                <a style="display:none;">
                                    <asp:DropDownList ID="WF_AVOCADOTODOKE" runat="server" class="form-select rounded-0" />
                                    <datalist id="WF_AVOCADOTODOKE_DL" runat="server"></datalist>
                                </a>
                            </p>
                            <p id="TYPE_K_LINE_1">
                                <span>
                                    <input type="checkbox" id="WF_TODOKECHANGE"  onchange="ChkTodokeChange();" />
                                    <asp:Label ID="WF_TODOKE_CHANGE" runat="server" Text="輸送費明細上でマスタとは異なる届先名を使用している場合" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <div id="TodokeChangeArea" style="display:none;">
                                <p id="TYPE_L_LINE_1">
                                    <!-- 変換後届先名 -->
                                    <span>
                                        <asp:Label ID="WF_TODOKENAME_L" runat="server" Text="変換後届先名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        <asp:TextBox ID="TxtTODOKENAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                                    </span>
                                </p>
                                <p id="TYPE_M_LINE_1">
                                    <!-- 変換後届先コード -->
                                    <span>
                                        <asp:Label ID="WF_TODOKECODE_L" runat="server" Text="変換後届先コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        <asp:TextBox ID="TxtTODOKECODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>
                                    </span>
                                </p>
                            </div>
                            <p id="TYPE_N_LINE_1">
                                <span>
                                    <!-- 陸事番号 -->
                                    <asp:Label ID="WF_TANKNUMBER_L" runat="server" Text="陸事番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtTANKNUMBER" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                                    <!-- 車番 -->
                                    <asp:Label ID="WF_SHABAN_L" runat="server" Text="業務車番" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSHABAN" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                                    <!-- 車腹 -->
                                    <asp:Label ID="WF_SYABARA_L" runat="server" Text="車腹" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSYABARA" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                                    <asp:Label ID="WF_SYABARA_TEXT" runat="server" Text="ｔ" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <p id="TYPE_O_LINE_1">
                                <span>
                                    <!-- 車型 -->
                                    <asp:Label ID="WF_SYAGATA_L" runat="server" Text="車型名" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_SYAGATA" runat="server" class="form-select rounded-0" onchange="f_syagata()">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                        <asp:ListItem Text="単車" Value="1"></asp:ListItem>
                                        <asp:ListItem Text="トレーラ" Value="2"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:Label ID="WF_SYAGATA_CODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>
                            <p id="TYPE_P_LINE_1">
                                <span>
                                    <!-- 単価 -->
                                    <asp:Label ID="WF_TANKA_L" runat="server" Text="単価" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtTANKA" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                                    <asp:Label ID="WF_TANKA_TEXT" runat="server" Text="円" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <!-- 計算区分 -->
                                    <asp:Label ID="WF_CALCKBN_L" runat="server" Text="計算区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="ddlSelectCALCKBN" runat="server" class="form-select rounded-0"/>
                                </span>
                            </p>
                            <p id="TYPE_Q_LINE_1">
                                <span>
                                    <!-- 往復距離 -->
                                    <asp:Label ID="WF_ROUNDTRIP_L" runat="server" Text="往復距離" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtROUNDTRIP" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                                    <asp:Label ID="WF_ROUNDTRIP_TEXT" runat="server" Text="Km" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <!-- 通行料 -->
                                    <asp:Label ID="WF_TOLLFEE_L" runat="server" Text="通行料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtTOLLFEE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="8"></asp:TextBox>
                                    <asp:Label ID="WF_TOLLFEE_TEXT" runat="server" Text="円" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <div id="TYPE_R_LINE_1">
                                <!-- 単価区分 -->
                                <asp:Label ID="WF_TANKAKBN_L" runat="server" Text="単価区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:RadioButtonList ID="RadioTANKAKBN" runat="server" RepeatDirection="Horizontal" CssClass="WF_RADIO2">
                                <asp:ListItem Value="0">通常</asp:ListItem>
                                <asp:ListItem Value="1">調整</asp:ListItem>
                                </asp:RadioButtonList>
                                <!-- 単価用途 -->
                                <asp:Label ID="WF_MEMO_L" runat="server" Text="単価用途" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtMEMO" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                            </div>
                            <p id="TYPE_S_LINE_1">
                                <span>
                                    <!-- 枝番 -->
                                    <asp:Label ID="WF_BRANCHCODE_L" runat="server" Text="枝番" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBRANCHCODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_T_LINE_1">
                                <span>
                                    <!-- 備考1 -->
                                    <asp:Label ID="WF_BIKOU1_L" runat="server" Text="備考1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                    <asp:Label ID="WF_BIKOU1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                                </span>
                            </p>
                            <p id="TYPE_T_LINE_2">
                                <span>
                                    <!-- 備考2 -->
                                    <asp:Label ID="WF_BIKOU2_L" runat="server" Text="備考2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                    <asp:Label ID="WF_BIKOU2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>
                            <p id="TYPE_T_LINE_3">
                                <span class="ef">
                                    <!-- 備考3 -->
                                    <asp:Label ID="WF_BIKOU3_L" runat="server" Text="備考3" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                    <asp:Label ID="WF_BIKOU3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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
        <!-- 表示制御項目 -->
        <input id="VisibleKeyOrgCode" runat="server" value="" type="text" />

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
        <!-- 自動作成した有効終了日 -->
        <input id="WF_AUTOENDYMD" runat="server" value="" type="text" />
        <!-- 出荷場所チェックボックス状態 -->
        <input id="WF_SHUKACHKSTATUS" runat="server" value="" type="text" />
        <!-- 届先場所チェックボックス状態 -->
        <input id="WF_TODOKECHKSTATUS" runat="server" value="" type="text" />
        <!-- 値保持 -->
        <%--<input id="WF_STYMD_SAVE" runat="server" value="" type="text" />--%>
        <input id="WF_TORINAME_SAVE" runat="server" value="" type="text" />
        <input id="WF_TORICODE_TEXT_SAVE" runat="server" value="" type="text" />
        <input id="WF_ORG_SAVE" runat="server" value="" type="text" />
        <input id="WF_KASANORG_SAVE" runat="server" value="" type="text" />
        <input id="WF_AVOCADOSHUKANAME_SAVE" runat="server" value="" type="text" />
        <input id="WF_AVOCADOSHUKABASHO_TEXT_SAVE" runat="server" value="" type="text" />
        <input id="WF_AVOCADOTODOKENAME_SAVE" runat="server" value="" type="text" />
        <input id="WF_AVOCADOTODOKECODE_TEXT_SAVE" runat="server" value="" type="text" />
        <input id="WF_SHABAN_SAVE" runat="server" value="" type="text" />
        <input id="WF_BRANCHCODE_SAVE" runat="server" value="" type="text" />
        <input id="WF_SYAGATA_SAVE" runat="server" value="" type="text" />
        <input id="WF_SYABARA_SAVE" runat="server" value="" type="text" />

    </div>
 
</asp:Content>
