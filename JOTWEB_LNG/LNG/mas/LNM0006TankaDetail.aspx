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
                                    <input type="button" id="WF_ButtonUPDATE" class="btn-sticky btn-action" value="更新" onclick="ButtonClick('WF_ButtonUPDATE');" />
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

                            <p id="TANKA_LINE_1">
                                <!-- 選択No -->
                                <span>
                                    <asp:Label ID="WF_SEL_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSelLineCNT" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                                    <%--<asp:Label ID="LblSelLineCNT" runat="server" CssClass="WF_TEXT"></asp:Label>--%>
                                </span>
                            </p>
                            <p id="TANKA_LINE_2">
                                <span class="ef magnifier">
                                    <!-- 削除フラグ -->
                                    <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="ddlDELFLG" runat="server" CssClass="ddlSelectControl">
                                        <asp:ListItem Text="有効" Value="0"></asp:ListItem>
                                        <asp:ListItem Text="削除" Value="1"></asp:ListItem>
                                    </asp:DropDownList>
                                    <a  style="display:none;">
                                    <!-- 会社コード -->
                                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtCampCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                                    <asp:Label ID="LblCampCodeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                    </a>
                                </span>


                            </p>
                            <p id="TANKA_LINE_3">
                                <span class="ef magnifier">
                                    <!-- 取引先コード -->
                                    <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <%--<asp:TextBox ID="TxtTORICODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>--%>
                                    <a class="ef" id="WF_TORI" ondblclick="Field_DBclick('TxtTORICODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtTORICODE');">
                                        <asp:TextBox ID="TxtTORICODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                                    </a>
                                    <asp:Label ID="WF_TORICODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                    <!-- 取引先名称 -->
                                    <asp:Label ID="WF_TORINAME_L" runat="server" Text="取引先名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtTORINAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                                    <asp:Label ID="WF_TORINAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>
                            <p id="TANKA_LINE_4">
                                <span class="ef magnifier">
                                    <!-- 部門コード -->
                                    <asp:Label ID="WF_ORGCODE_L" runat="server" Text="部門コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <asp:DropDownList ID="ddlSelectORG" runat="server" ClientIDMode="Predictable" CssClass="ddlSelectControl" onchange="ButtonClick('WF_ORGChange');"/>
                                </span>
                            </p>
                            <p id="TANKA_LINE_5">
                                <span class="ef magnifier">
                                    <!-- 加算先部門コード -->
                                    <asp:Label ID="WF_KASANORGCODE_L" runat="server" Text="加算先部門コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <%--<asp:TextBox ID="TxtKASANORGCODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>--%>
                                    <a class="ef" id="WF_KASANORG" ondblclick="Field_DBclick('TxtKASANORGCODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtKASANORGCODE');">
                                        <asp:TextBox ID="TxtKASANORGCODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                                    </a>
                                    <asp:Label ID="WF_KASANORGCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                    <!-- 加算先部門名称 -->
                                    <asp:Label ID="WF_KASANORGNAME_L" runat="server" Text="加算先部門名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtKASANORGNAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                                 <asp:Label ID="WF_KASANORGNAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>

                            <p id="TANKA_LINE_6">
                                <span class="ef magnifier">
                                    <!-- 届先コード -->
                                    <asp:Label ID="WF_TODOKECODE_L" runat="server" Text="届先コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                                    <%--<asp:TextBox ID="TxtTODOKECODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>--%>
                                    <a class="ef" id="WF_TODOKE" ondblclick="Field_DBclick('TxtTODOKECODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtTODOKECODE');">
                                        <asp:TextBox ID="TxtTODOKECODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                                    </a>
                                    <asp:Label ID="WF_TODOKECODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                    <!-- 届先名称 -->
                                    <asp:Label ID="WF_TODOKENAME_L" runat="server" Text="届先名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtTODOKENAME" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                                    <asp:Label ID="WF_TODOKENAME_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>

                            <p id="TANKA_LINE_ENDYMD_ANNOTATION">
                                <asp:Label ID="Label32" runat="server" Text="※有効終了日が未入力の場合は「2099/12/31」が設定されます。" CssClass="WF_TEXT_LEFT" style="color:red"></asp:Label>
                            </p>
                            <p id="TANKA_LINE_7">
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
                                    <asp:Label ID="WF_STYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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
                                    <asp:Label ID="WF_ENDYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>
                            <p id="TANKA_LINE_8">
                                <span class="ef">
                                    <!-- 単価 -->
                                    <asp:Label ID="WF_TANKA_L" runat="server" Text="単価" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtTANKA" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                                    <asp:Label ID="WF_TANKA_TEXT" runat="server" Text="円" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <!-- 枝番 -->
                                    <a  style="display:none;">
                                    <asp:Label ID="WF_BRANCHCODE_L" runat="server" Text="枝番" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBRANCHCODE" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                                    <asp:Label ID="WF_BRANCHCODE_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                    </a>
                                </span>
                            </p>
                            <p id="TANKA_LINE_9">
                                <span class="ef">
                                    <!-- 車型 -->
                                    <asp:Label ID="WF_SYAGATA_L" runat="server" Text="車型" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:DropDownList ID="ddlSelectSYAGATA" runat="server" ClientIDMode="Predictable" CssClass="ddlSelectControl"/>
                                    <asp:Label ID="WF_SYAGATA_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                    <!-- 車号 -->
                                    <asp:Label ID="WF_SYAGOU_L" runat="server" Text="車号" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSYAGOU" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                                    <asp:Label ID="WF_SYAGOU_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>
                            <p id="TANKA_LINE_10">
                                <span class="ef">
                                    <!-- 車腹 -->
                                    <asp:Label ID="WF_SYABARA_L" runat="server" Text="車腹" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSYABARA" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                                    <asp:Label ID="WF_SYABARA_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                    <!-- 種別 -->
                                    <asp:Label ID="WF_SYUBETSU_L" runat="server" Text="種別" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtSYUBETSU" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                                    <asp:Label ID="WF_SYUBETSU_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>
                            <p id="TANKA_LINE_11">
                                <span class="ef">
                                    <!-- 備考1 -->
                                    <asp:Label ID="WF_BIKOU1_L" runat="server" Text="備考1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                    <asp:Label ID="WF_BIKOU1_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                    <!-- 備考2 -->
                                    <asp:Label ID="WF_BIKOU2_L" runat="server" Text="備考2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                    <asp:Label ID="WF_BIKOU2_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>
                            <p id="TANKA_LINE_12">
                                <span class="ef">
                                    <!-- 備考3 -->
                                    <asp:Label ID="WF_BIKOU3_L" runat="server" Text="備考3" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    <asp:TextBox ID="TxtBIKOU3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                                    <asp:Label ID="WF_BIKOU3_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                                </span>
                            </p>
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
    <!-- 取引先コード単一選択 -->
    <MSINC:multiselect runat="server" id="mspToriCodeSingle" />
    <!-- 加算先部門コード単一選択 -->
    <MSINC:multiselect runat="server" id="mspKasanOrgCodeSingle" />
    <!-- 届先コード単一選択 -->
    <MSINC:multiselect runat="server" id="mspTodokeCodeSingle" />

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
        <!-- 前の遷移先 -->
        <input id="WF_BeforeMAPID" runat="server" value="" type="text" />
        <!-- 権限 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
        <!-- 自動作成した有効終了日 -->
        <input id="WF_AUTOENDYMD" runat="server" value="" type="text" />
    </div>
 
</asp:Content>
