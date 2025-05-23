<%@ Page Title="LNM0014D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0014SprateDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0014SprateDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0014WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0014DH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/style.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNM0014D.css")%>' rel="stylesheet" type="text/css" />
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/index.js"></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0014D.js")%>'></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
</asp:Content>
 
<asp:Content ID="LNM0014D" ContentPlaceHolderID="contents1" runat="server">
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
                        <li class="breadcrumb-item active"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNM0014L');">特別料金マスタ</a></li>
                        <li class="breadcrumb-item active" id="PAGE_NAME1" aria-current="page">特別料金マスタ（詳細）</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title" id="PAGE_NAME2">特別料金マスタ詳細</h2>
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


                             <p id="KEY_LINE_4">
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
                            <p id="KEY_LINE_5">
                                <!-- 取引先コード -->
                                <span>
                                    <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先名称" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_TORI" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_TORIChange');" />
                                    <asp:Label ID="WF_TORICODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>

                            <p id="KEY_LINE_7">
                                <!-- 部門コード -->
                                <span>
                                    <asp:Label ID="WF_ORGCODE_L" runat="server" Text="部門名称" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="WF_ORG" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_ORGChange');" />
                                    <asp:Label ID="WF_ORGCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>
                            <p id="KEY_LINE_9">
                                <!-- 加算先部門コード -->
                                <span>
                                    <asp:Label ID="WF_KASANORGCODE_L" runat="server" Text="加算先部門名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="WF_KASANORG" runat="server" class="form-select rounded-0" onchange="ButtonClick('WF_KASANORGChange');" />
                                    <asp:Label ID="WF_KASANORGCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>
                            <p id="KEY_LINE_11">
                                <!-- 届先名称 -->
                                <span>
                                    <asp:Label ID="WF_TODOKECODE_L" runat="server" Text="届先名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <a class="ef" id="WF_TODOKE" ondblclick="Field_DBclick('TxtTODOKECODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtTODOKECODE');">
                                        <asp:TextBox ID="TxtTODOKENAME" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="20"></asp:TextBox>
                                    </a>
                                    <a  style="display:none;">
                                        <asp:TextBox ID="TxtTODOKECODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>
                                    </a>
                                </span>
                            </p>

                            <asp:Panel ID="pnlYusouArea" runat="server">
                                <p id="YUSOU_LINE_1">
                                    <asp:Label ID="WF_YUSOU_L" runat="server" Text="●輸送費明細(その他明細)に出力する名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </p>
                               <p id="YUSOU_LINE_2">
                                    <span class="ef magnifier">
                                        <!-- グループ名 -->
                                        <asp:Label ID="WF_GROUPNAME_L" runat="server" Text="明細グループ名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        <a class="ef" id="WF_GROUP" ondblclick="Field_DBclick('TxtGROUPNAME', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtGROUPNAME');">
                                            <asp:TextBox ID="TxtGROUPNAME" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="100"></asp:TextBox>
                                        </a>
                                        <!-- グループソート順 -->
                                        <asp:Label ID="WF_GROUPSORTNO_L" runat="server" Text="グループソート順" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        <asp:TextBox ID="TxtGROUPSORTNO" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                                    </span>
                                </p>
                                <p id="YUSOU_LINE_3">
                                    <span class="ef magnifier">
                                        <!-- 明細名 -->
                                        <asp:Label ID="WF_DETAILNAME_L" runat="server" Text="明細名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        <a class="ef" id="WF_DETAIL" ondblclick="Field_DBclick('TxtDETAILNAME', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtDETAILNAME');">
                                            <asp:TextBox ID="TxtDETAILNAME" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="100"></asp:TextBox>
                                        </a>
                                       <!-- 明細ソート順 -->
                                       <asp:Label ID="WF_DETAILSORTNO_L" runat="server" Text="明細ソート順" CssClass="WF_TEXT_LEFT"></asp:Label>
                                       <asp:TextBox ID="TxtDETAILSORTNO" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                                    </span>
                                </p>
                            </asp:Panel>

                            <p id="KEY_LINE_14" style="display:none;">
                                <!-- グループID -->
                                <span>
                                    <asp:Label ID="WF_GROUPID_L" runat="server" Text="グループID" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtGROUPID" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                                </span>
                            </p>
 
                            <p id="KEY_LINE_17" style="display:none;">
                                <!-- 明細ID -->
                                <span>
                                    <asp:Label ID="WF_DETAILID_L" runat="server" Text="明細ID" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtDETAILID" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                                </span>
                            </p>

                            <p id="KEY_LINE_19">
                                <!-- 単価 -->
                                <span>
                                    <asp:Label ID="WF_TANKA_L" runat="server" Text="単価" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtTANKA" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="9"></asp:TextBox>
                                    <asp:Label ID="WF_TANKA_TEXT" runat="server" Text="円" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </span>
                            </p>
                            <p id="KEY_LINE_20">
                                <!-- 数量 -->
                                <span>
                                    <asp:Label ID="WF_QUANTITY_L" runat="server" Text="数量" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtQUANTITY" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="9"></asp:TextBox>
                                </span>
                            </p>
                            <p id="KEY_LINE_21">
                                <!-- 計算単位 -->
                                <span>
                                    <asp:Label ID="WF_CALCUNIT_L" runat="server" Text="計算単位" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="ddlSelectCALCUNIT" runat="server" class="form-select rounded-0"/>
                                </span>
                            </p>

                            <div id="RAD_LINE_29">
                                <!-- 表示フラグ -->
                                <asp:Label ID="WF_DISPLAYFLG_L" runat="server" Text="請求書表示" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:RadioButtonList ID="RadioDISPLAYFLG" runat="server" RepeatDirection="Horizontal" CssClass="WF_RADIO">
                                <asp:ListItem Value="1">表示する</asp:ListItem>
                                <asp:ListItem Value="0">表示しない</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <asp:Panel ID="pnlAssessmentArea" runat="server">
                                <p id="ASSESSMENT_LINE_1">
                                    <asp:Label ID="WF_ASSESSMENT_L" runat="server" Text="●請求書鑑分(別の請求書として出力したい場合)" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </p>
                                <div id="RAD_LINE_30">
                                    <!-- 鑑分けフラグ -->
                                        <asp:Label ID="WF_ASSESSMENTFLG_L" runat="server" Text="鑑分けフラグ" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        <asp:RadioButtonList ID="RadioASSESSMENTFLG" runat="server" RepeatDirection="Horizontal" CssClass="WF_RADIO">
                                        <asp:ListItem Value="1">する</asp:ListItem>
                                        <asp:ListItem Value="0">しない</asp:ListItem>
                                        </asp:RadioButtonList>
                                </div>
                                <p id="ASSESSMENT_CHK_LINE_2">
                                    <span>
                                        <asp:Label ID="WF_ATENA_CHANGE" runat="server" Text="宛名書変更" CssClass="WF_TEXT_LEFT"></asp:Label>
                                        <input type="checkbox" id="WF_ATENACHANGE"  onchange="ChkAtenaChange();" />
                                    </span>
                                </p>
                                <div id="AtenaChangeArea" style="display:none;">
                                    <p id="ASSESSMENT_LINE_3">
                                        <!-- 宛名会社名 -->
                                        <span class="ef magnifier">
                                            <asp:Label ID="WF_ATENACOMPANYNAME_L" runat="server" Text="宛名会社名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                            <select name="mae" id="ddlMAEKABU" class="form-select rounded-0" onchange="MaekabuChange();">
                                              <option value=""></option>
                                              <option value="前株">前株</option>
                                            </select>
                                            <select name="ato" id="ddlATOKABU" class="form-select rounded-0" onchange="AtokabuChange();">
                                              <option value=""></option>
                                              <option value="後株">後株</option>
                                            </select>

 <%--                                           <asp:DropDownList ID="ddlMAEKABU" runat="server" CssClass="form-select rounded-0" onchange="ButtonClick('WF_MAEKABUChange');" >
                                                <asp:ListItem Text="" Value=""></asp:ListItem>
                                                <asp:ListItem Text="前株" Value="前株"></asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:DropDownList ID="ddlATOKABU" runat="server" CssClass="form-select rounded-0" onchange="ButtonClick('WF_ATOKABUChange');" >
                                                <asp:ListItem Text="" Value=""></asp:ListItem>
                                                <asp:ListItem Text="後株" Value="後株"></asp:ListItem>
                                            </asp:DropDownList>--%>
                                        </span>
                                    </p>
                                    <p id="ASSESSMENT_LINE_4">
                                        <!-- 宛名会社名 -->
                                        <span>
                                            <asp:Label ID="WF_ATENACOMPANYNAME2_L" runat="server" Text="" CssClass="WF_TEXT_LEFT"></asp:Label>
                                            <asp:TextBox ID="TxtATENACOMPANYNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                        </span>
                                    </p>
                                    <p id="ASSESSMENT_LINE_5">
                                        <!-- 宛名会社部門名 -->
                                        <span>
                                            <asp:Label ID="WF_ATENACOMPANYDEVNAME_L" runat="server" Text="宛名会社部門名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                            <asp:TextBox ID="TxtATENACOMPANYDEVNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                        </span>
                                    </p>
                                    <p id="ASSESSMENT_LINE_6">
                                        <!-- 請求書発行部店名 -->
                                        <span>
                                            <asp:Label ID="WF_FROMORGNAME_L" runat="server" Text="請求書発行部店名" CssClass="WF_TEXT_LEFT"></asp:Label>
                                            <asp:TextBox ID="TxtFROMORGNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                        </span>
                                    </p>
                                </div>
                            </asp:Panel>

                            <div class="Operation">
                                    <div class="actionButtonBox">
                                        <div class="centerSide">
                                            <input type="button" id="WF_ButtonUPDATE" class="btn-sticky btn-action" value="更新" onclick="ButtonClick('WF_ButtonUPDATE');" />
                                            <input type="button" id="WF_ButtonCANCEL" class="btn-sticky" value="キャンセル"  onclick="ButtonClick('WF_ButtonCLEAR');" />
                                        </div>
                                    </div>
                            </div>


<div style="display:none;">
                            <p id="KEY_LINE_22">
                                <!-- 出荷地 -->
                                <span>
                                    <asp:Label ID="WF_DEPARTURE_L" runat="server" Text="出荷地" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtDEPARTURE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                </span>
                            </p>
                            <p id="KEY_LINE_23">
                                <!-- 走行距離 -->
                                <span>
                                    <asp:Label ID="WF_MILEAGE_L" runat="server" Text="走行距離" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtMILEAGE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                                </span>
                            </p>
                            <p id="KEY_LINE_24">
                                <!-- 輸送回数 -->
                                <span>
                                    <asp:Label ID="WF_SHIPPINGCOUNT_L" runat="server" Text="輸送回数" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSHIPPINGCOUNT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                                </span>
                            </p>
                            <p id="KEY_LINE_25">
                                <!-- 燃費 -->
                                <span>
                                    <asp:Label ID="WF_NENPI_L" runat="server" Text="燃費" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtNENPI" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                                </span>
                            </p>
                            <p id="KEY_LINE_26">
                                <!-- 実勢軽油価格 -->
                                <span>
                                    <asp:Label ID="WF_DIESELPRICECURRENT_L" runat="server" Text="実勢軽油価格" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtDIESELPRICECURRENT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                                </span>
                            </p>
                            <p id="KEY_LINE_27">
                                <!-- 基準経由価格 -->
                                <span>
                                    <asp:Label ID="WF_DIESELPRICESTANDARD_L" runat="server" Text="基準経由価格" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtDIESELPRICESTANDARD" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                                </span>
                            </p>
                            <p id="KEY_LINE_28">
                                <!-- 燃料使用量 -->
                                <span>
                                    <asp:Label ID="WF_DIESELCONSUMPTION_L" runat="server" Text="燃料使用量" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtDIESELCONSUMPTION" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                                </span>
                            </p>
                            <p id="KEY_LINE_34">
                                <!-- 明細区分 -->
                                <span>
                                    <asp:Label ID="WF_MEISAICATEGORYID_L" runat="server" Text="明細区分" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="ddlMEISAICATEGORYID" runat="server" CssClass="ddlSelectControl">
                                        <asp:ListItem Text="" Value=""></asp:ListItem>
                                        <asp:ListItem Text="請求追加明細(特別料金)" Value="1"></asp:ListItem>
                                        <asp:ListItem Text="サーチャージ" Value="2"></asp:ListItem>
                                    </asp:DropDownList>
                                </span>
                            </p>
                            <p id="KEY_LINE_35">
                                <!-- 備考1 -->
                                <span>
                                    <asp:Label ID="WF_BIKOU1_L" runat="server" Text="備考1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="100"></asp:TextBox>
                                </span>
                            </p>
                            <p id="KEY_LINE_36">
                                <!-- 備考2 -->
                                <span>
                                    <asp:Label ID="WF_BIKOU2_L" runat="server" Text="備考2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="100"></asp:TextBox>
                                </span>
                            </p>
                            <p id="KEY_LINE_37">
                                <!-- 備考3 -->
                                <span>
                                    <asp:Label ID="WF_BIKOU3_L" runat="server" Text="備考3" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="100"></asp:TextBox>
                                </span>
                            </p>
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
    <MSINC:multiselect runat="server" id="mspTodokeCodeSingle" />
    <!-- グループID単一選択 -->
    <MSINC:multiselect runat="server" id="mspGroupIdSingle" />
    <!-- 明細ID単一選択 -->
    <MSINC:multiselect runat="server" id="mspDetailIdSingle" />

    <!-- イベント用 -->
    <div style="display:none;">

        <!-- 入力不可制御項目 -->
        <input id="DisabledKeyItem" runat="server" value="" type="text" />
        <input id="DisabledKeyOrgCount" runat="server" value="" type="text" />
        <input id="DisabledKeyToriCount" runat="server" value="" type="text" />
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
        <!-- 権限 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
        <!-- 宛名書変更チェックボックス状態 -->
        <input id="WF_ATENACHKSTATUS" runat="server" value="" type="text" /> 
        <!-- 宛名書変更リストボックス選択状態 -->
        <input id="WF_ATENALISTSELECT" runat="server" value="" type="text" /> 

        <!-- 値保持 -->
        <input id="WF_TARGETYM_SAVE" runat="server" value="" type="text" />
        <input id="WF_TORI_SAVE" runat="server" value="" type="text" />
        <input id="WF_ORG_SAVE" runat="server" value="" type="text" />  
        <input id="WF_GROUPNAME_SAVE" runat="server" value="" type="text" />          
        <input id="WF_DETAILNAME_SAVE" runat="server" value="" type="text" />  

    </div>
 
</asp:Content>
