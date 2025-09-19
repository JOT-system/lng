<%@ Page Title="LNM0019D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0019SurchargePatternDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0019SurchargePatternDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0019WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0019DH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNM0019D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0019D.js")%>'></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
</asp:Content>
 
<asp:Content ID="LNM0019D" ContentPlaceHolderID="contents1" runat="server">
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
                        <li class="breadcrumb-item active"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNM0019L');">サーチャージ定義マスタ</a></li>
                        <li class="breadcrumb-item active" id="PAGE_NAME1" aria-current="page">サーチャージ定義マスタ（詳細）</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title" id="PAGE_NAME2">サーチャージ定義マスタ詳細</h2>
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
                                            <input type="text" id="WF_StYMD" runat="server" class="WF_TEXTBOX_CSS" onchange="ButtonClick('WF_SelectCALENDARChange');" data-input>
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
                                    <asp:DropDownList ID="WF_TORINAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TORINAMEChange');" />
                                    <asp:TextBox ID="WF_TORICODE" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_2">
                                <!-- 部門コード -->
                                <span>
                                    <asp:Label ID="WF_ORG_L" runat="server" Text="担当部門名" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_ORGNAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_ORGNAMEChange');" />
                                    <asp:TextBox ID="WF_ORGCODE" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_3">
                                <!-- 加算先部門コード -->
                                <span>
                                    <asp:Label ID="WF_KASANORGCODE_L" runat="server" Text="加算先部門名" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_KASANORGNAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_KASANORGNAMEChange');" />
                                    <asp:TextBox ID="WF_KASANORGCODE" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_4">
                                <!-- 請求サイクル -->
                                <span>
                                    <asp:Label ID="WF_BILLINGCYCLE_L" runat="server" Text="請求サイクル" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_BILLINGCYCLENAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_BILLINGCYCLECNAMEhange');" />
                                    <asp:TextBox ID="WF_BILLINGCYCLE" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="display:none;"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_5">
                                <!-- サーチャージパターンコード -->
                                <span>
                                    <asp:Label ID="WF_SURCHARGEPATTERNCODE_L" runat="server" Text="サーチャージパターン" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_SURCHARGEPATTERNNAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_SURCHARGEPATTERNNAMEChange');" />
                                    <asp:TextBox ID="WF_SURCHARGEPATTERNCODE" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="display:none;"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_6">
                                <!-- 距離算定方式 -->
                                <span>
                                    <asp:Label ID="WF_CALCMETHOD_L" runat="server" Text="距離算定方式" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_CALCMETHODNAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_CALCMETHODNAMEChange');" />
                                    <asp:TextBox ID="WF_CALCMETHOD" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="display:none;"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_7">
                                <!-- 実勢軽油価格参照先名 -->
                                <span>
                                    <asp:Label ID="WF_DISPLAYNAME_L" runat="server" Text="実勢価格参照先" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_DISPLAYNAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_DISPLAYNAMEChange');" />
                                    <asp:TextBox ID="WF_DIESELPRICESITEID" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="display:none;"></asp:TextBox>
                                    <asp:TextBox ID="WF_DIESELPRICESITENAME" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="display:none;"></asp:TextBox>
                                    <asp:TextBox ID="WF_DIESELPRICESITEBRANCH" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="display:none;"></asp:TextBox>
                                    <asp:TextBox ID="WF_DIESELPRICESITEKBNNAME" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="display:none;"></asp:TextBox>
                                </span>
                            </p>

                            <p id="TYPE_C_LINE_8">
                                <span>
                                    <asp:Label ID="WF_DIESELPRICEROUNDING_L" runat="server" Text="実勢単価端数処理" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_9">
                                <!-- 実勢単価端数処理（桁数） -->
                                <span>
                                    <asp:Label ID="WF_DECIMALPOINT_L" runat="server" Text="　　　小数点以下" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:RadioButtonList ID="WF_DECIMALPOINT" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal" CssClass="checkboxlist-inline">
                                        <asp:ListItem Text="第１位" Value="1"></asp:ListItem>
                                        <asp:ListItem Text="第２位" Value="2"></asp:ListItem>
                                        <asp:ListItem Text="第３位" Value="3"></asp:ListItem>
                                    </asp:RadioButtonList>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_10">
                                <!-- 実勢単価端数処理（方式） -->
                                <span>
                                    <asp:Label ID="WF_DIESELPRICEROUNDMETHOD_L" runat="server" Text="　　　処理方式" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_DIESELPRICEROUNDMETHODNAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_DIESELPRICEROUNDMETHODChange');" />
                                    <asp:TextBox ID="WF_DIESELPRICEROUNDMETHOD" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="display:none;"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_11">
                                <span>
                                    <asp:Label ID="Label1" runat="server" Text="請求金額端数処理" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_12">
                                <!-- サーチャージ請求金額端数処理（方式） -->
                                <span>
                                    <asp:Label ID="WF_SURCHARGEROUNDMETHOD_L" runat="server" Text="　　　処理方式" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_SURCHARGEROUNDMETHODNAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_SURCHARGEROUNDMETHODChange');" />
                                    <asp:TextBox ID="WF_SURCHARGEROUNDMETHOD" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false" style="display:none;"></asp:TextBox>
                                </span>
                            </p>

                            <p id="TYPE_C_LINE_13">
                                <!-- 勘定科目コード -->
                                <span>
                                    <asp:Label ID="WF_ACCOUNT_L" runat="server" Text="勘定科目名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_ACCOUNTNAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_ACCOUNTChange');" />
                                    <asp:TextBox ID="WF_ACCOUNTCODE" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_C_LINE_14">
                                <!-- セグメントコード -->
                                <span>
                                    <asp:Label ID="WF_SEGMENT_L" runat="server" Text="セグメント名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_SEGMENTNAME" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_SEGMENTChange');" />
                                    <asp:TextBox ID="WF_SEGMENTCODE" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                                </span>
                            </p>
                            <p id="TYPE_K_LINE_1">
                                <span>
                                    <!-- 割合JOT -->
                                    <asp:Label ID="WF_JOTPERCENTAGE_L" runat="server" Text="割合JOT" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="WF_JOTPERCENTAGE" runat="server" CssClass="WF_TEXTBOX_RIGHT" MaxLength="6"></asp:TextBox>
                                    <asp:Label ID="WF_JOTPERCENTAGE_TEXT" runat="server" Text="%" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <!-- 割合ENEX -->
                                    <asp:Label ID="WF_ENEXPERCENTAGE_L" runat="server" Text="割合ENEX" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="WF_ENEXPERCENTAGE" runat="server" CssClass="WF_TEXTBOX_RIGHT" MaxLength="6"></asp:TextBox>
                                    <asp:Label ID="WF_ENEXPERCENTAGE_TEXT" runat="server" Text="%" CssClass="WF_TEXT_LEFT"></asp:Label>
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
        <!-- 値保持 -->
        <%--<input id="WF_STYMD_SAVE" runat="server" value="" type="text" />--%>
        <input id="WF_TORICODE_SAVE" runat="server" value="" type="text" />
        <input id="WF_TORINAME_SAVE" runat="server" value="" type="text" />
        <input id="WF_ORG_SAVE" runat="server" value="" type="text" />
        <input id="WF_ORGNAME_SAVE" runat="server" value="" type="text" />
        <input id="WF_KASANORG_SAVE" runat="server" value="" type="text" />
        <input id="WF_KASANORGNAME_SAVE" runat="server" value="" type="text" />
        <input id="WF_BILLINGCYCLE_SAVE" runat="server" value="" type="text" />
        <input id="WF_BILLINGCYCLENAME_SAVE" runat="server" value="" type="text" />
        <input id="WF_SURCHARGEPATTERNCODE_SAVE" runat="server" value="" type="text" />
        <input id="WF_SURCHARGEPATTERNNAME_SAVE" runat="server" value="" type="text" />

        <!-- 縦スクロール位置 -->
        <input id="WF_scrollY" runat="server" value="0" type="text" />
    </div>
 
</asp:Content>
