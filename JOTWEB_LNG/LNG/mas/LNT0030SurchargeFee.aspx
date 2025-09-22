<%@ Page Title="LNT0030L" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0030SurchargeFee.aspx.vb" Inherits="JOTWEB_LNG.LNT0030SurchargeFee" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview2" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0030WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNT0030LH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/style.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNT0030L.css")%>' rel="stylesheet" type="text/css" />
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/index.js"></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0030L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="LNT0030L" ContentPlaceHolderID="contents1" runat="server">
    <div class="d-inline-flex align-items-center flex-column w-100">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="d-flex w-100 wrap">
            <!-- サイドメニュー -->
            <MSINC:leftmenu ID="leftmenu" runat="server" />
            <div class="w-100 contents" >
                <nav style="--bs-breadcrumb-divider: '>';" aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item d-flex align-items-center gap-1"><span class="material-symbols-outlined">home</span><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('WF_ButtonBackToMenu');">TOP</a></li>
                        <li class="breadcrumb-item active"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNM0019L');">サーチャージ定義マスタ</a></li>
                        <li class="breadcrumb-item active" id="PAGE_NAME1" aria-current="page">サーチャージ料金（登録）</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title">サーチャージ料金登録</h2>
                        <div class="Operation">
                            <div class="actionButtonBox">
                                <div class="d-flex align-items-center gap-2 me-3">
                                    <!-- 一覧件数 -->
                                    <asp:Label ID="ListCount" runat="server" CssClass="WF_TEXT_LEFT" style="width:90px"></asp:Label>
                                    <div class="d-flex align-items-center gap-2">
                                        <strong class="flex-shrink-0" style="width:90px">取　引　先：</strong>
                                        <asp:textBox ID="WF_TORINAME" runat="server" Text="取引先名" CssClass="WF_TEXT_LEFT" disabled="true"></asp:textBox>
                                        <asp:textBox ID="WF_TORICODE" runat="server" Text="取引先" CssClass="WF_TEXT_LEFT" style="display:none"></asp:textBox>
                                        <strong class="flex-shrink-0" style="width:100px">　　部　門：</strong>
                                        <asp:textBox ID="WF_ORGNAME" runat="server" Text="部門名" CssClass="WF_TEXT_LEFT" disabled="true"></asp:textBox>
                                        <asp:textBox ID="WF_ORGCODE" runat="server" Text="部門" CssClass="WF_TEXT_LEFT" style="display:none"></asp:textBox>
                                    </div>
                                </div>

                                <div class="rightSide">
                                    <%--<input type="button" id="WF_ButtonTANKA" class="btn-sticky" value="実勢単価登録へ" onclick="ButtonClick('WF_ButtonTANKA');" />--%>
                                    <asp:Label ID="WF_UPLOAD_LABEL" AssociatedControlID="WF_UPLOAD_BTN" runat="server" CssClass="btn-sticky btn-action" Text="ｱｯﾌﾟﾛｰﾄﾞ"> <asp:FileUpload ID="WF_UPLOAD_BTN" runat="server"  onchange="ButtonClick('WF_ButtonUPLOAD')"/>
                                    </asp:Label>
                                    <input type="button" id="WF_ButtonDOWNLOAD" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonDOWNLOAD');" />
                                    <input type="button" id="WF_ButtonEND2"      class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                                </div>
                            </div> <!-- End class=actionButtonBox -->
                        </div> <!-- End class="Operation" -->
                        <div class="searchBar">
                            <div id="actionTrigger" class="d-flex flex-wrap gap-3 w-100">
                                <div class="actionButtonBox">
                                    <!-- 有効年月日(開始） -->
                                    <div class="d-flex align-items-center gap-2 me-3">
                                        <strong class="flex-shrink-0">請求年月</strong>
                                        <div class="position-relative input-group calendar datetimepicker" data-target-input="nearest">
                                            <input type="text" id="WF_SeikyuYm" runat="server" class="WF_TEXTBOX_CSS" onchange="ButtonClick('WF_SelectCALENDARChange');" autocomplete="off" data-input >
                                            <span class="input-group-text" data-toggle >
                                                <span class="material-symbols-outlined">calendar_month</span>
                                            </span>
                                        </div>
                                    </div>
                                    <div class="d-flex align-items-center gap-2">
                                        <strong class="flex-shrink-0" style="width:90px">請求サイクル：</strong>
                                        <asp:textBox ID="WF_BILLINGCYCLENAME" runat="server" Text="請求サイクル名" CssClass="WF_TEXT_LEFT" disabled="true"></asp:textBox>
                                        <asp:textBox ID="WF_BILLINGCYCLE" runat="server" Text="請求サイクル" CssClass="WF_TEXT_LEFT" style="display:none"></asp:textBox>
                                        <strong class="flex-shrink-0" style="width:100px">ｻｰﾁｬｰｼﾞﾊﾟﾀｰﾝ：</strong>
                                        <asp:textBox ID="WF_SURCHARGEPATTERNNAME" runat="server" Text="サーチャージパターン名" CssClass="WF_TEXT_LEFT" disabled="true"></asp:textBox>
                                        <asp:textBox ID="WF_SURCHARGEPATTERNCODE" runat="server" Text="サーチャージパターン" CssClass="WF_TEXT_LEFT" style="display:none"></asp:textBox>
                                        <strong class="flex-shrink-0" style="width:100px">距離算定方式：</strong>
                                        <asp:textBox ID="WF_CALCMETHODNAME" runat="server" Text="距離算定方式名" CssClass="WF_TEXT_LEFT" disabled="true"></asp:textBox>
                                        <asp:textBox ID="WF_CALCMETHOD" runat="server" Text="距離算定方式" CssClass="WF_TEXT_LEFT" style="display:none"></asp:textBox>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="searchBar" >
                            <div class="actionButtonBox">
                                <div class="d-flex align-items-center gap-2 me-3 w-100">
                                    <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="行追加" onclick="BtnAddClick('WF_ButtonINSERT');" />
                                    <a class="ef" id="WF_TODOKECODE" >
                                        <input type="button" id="WF_ButtonTODOKE" class="btn-sticky" value="届先選択" onclick="Field_DBclick('WF_TODOKECODE', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" />
                                    </a>
                                    <%--<asp:TextBox ID="WF_TODOKECODE" runat="server" CssClass="boxIcon" onblur="MsgClear();" ></asp:TextBox>--%>
                                    <input type="button" id="WF_ButtonALLSELECT" class="btn-sticky" value="全選択" onclick  ="ButtonClick('WF_ButtonALLSELECT');" />
                                    <input type="button" id="WF_ButtonALLREJECT" class="btn-sticky" value="全解除" onclick ="ButtonClick('WF_ButtonALLREJECT');" />
                                    <input type="button" id="WF_ButtonUPDATE" class="btn-sticky btn-action" value="更新" onclick="ButtonClick('WF_ButtonUPDATE');" />
                                </div>
                                <div class="rightSide">
                                </div>
                            </div>
                        </div>

                        <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
                </div>
            </div>
        </div>
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview ID="rightview" runat="server" />
    <MSINC:rightview2 ID="rightview2" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview ID="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist ID="work" runat="server" />

    <!-- 届先コード単一選択 -->
    <MSINC:multiselect ID="mspTodokeCode" runat="server" />
    <!-- 出荷場所コード単一選択 -->
    <MSINC:multiselect ID="mspShukabasho" runat="server" />

    <!-- イベント用 -->
    <div style="display:none;">
        <!-- GridView DBクリック-->
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
        <!-- GridView表示位置フィールド -->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD" runat="server" value="" type="text" />
        <!-- Textbox(Repeater) DBクリックフィールド -->
        <input id="WF_FIELD_REP" runat="server" value="" type="text" />
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />
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
        <!-- フラグパラメタの保存 -->
        <input id="WF_FLGPARM" runat="server" value="" type="text" />
    </div>
 
</asp:Content>
