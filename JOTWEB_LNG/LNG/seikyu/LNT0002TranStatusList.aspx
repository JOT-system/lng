<%@ Page Title="LNT0002L" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0002TranStatusList.aspx.vb" Inherits="JOTWEB_LNG.LNT0002TranStatusList" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview2" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNT0002LH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/style.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNT0002L.css")%>' rel="stylesheet" type="text/css" />
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@latest/dist/plugins/monthSelect/index.js"></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0002L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="LNT0002L" ContentPlaceHolderID="contents1" runat="server">
    <div class="d-inline-flex align-items-center flex-column w-100">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="d-flex w-100 wrap">
            <!-- サイドメニュー -->
            <MSINC:leftmenu ID="leftmenu" runat="server" />
            <div class="w-100 contents">
                <nav style="--bs-breadcrumb-divider: '>';" aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item d-flex align-items-center gap-1"><span class="material-symbols-outlined">home</span><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('WF_ButtonBackToMenu');">TOP</a></li>
                        <li class="breadcrumb-item active" aria-current="page">輸送費明細出力状況</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title">輸送費明細出力状況</h2>
                        <div class="Operation">
                            <div class="actionButtonBox">
                                <div class="leftSide">
                                    <!-- 対象年月 -->
                                    <div class="d-flex align-items-center gap-2 me-3">
                                        <strong class="flex-shrink-0">対象年月</strong>
                                        <b class="calendararea">
                                            <b class="position-relative input-group calendar datetimepicker" data-target-input="nearest">
                                                <input type="text" id="WF_TaishoYm" runat="server" class="WF_TEXTBOX_CSS" onchange="ButtonClick('WF_SelectCALENDARChange');" data-input>
                                                <span id="WF_StYMD_CALENDAR" class="input-group-text" data-toggle>
                                                    <span class="material-symbols-outlined">calendar_month</span>
                                                </span>
                                            </b>
                                        </b>
                                    </div>
                                </div>
                                <div class="rightSide">
                                    <%--<input type="button" id="WF_ButtonEND"      class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />--%>
                                    <input type="button" id="WF_ButtonEND2"      class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                                </div>
                            </div> <!-- End class=actionButtonBox -->
                        </div> <!-- End class="Operation" -->

                        <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
                </div>
                <div id="pnlHISTWrapper">     
                    <asp:Panel ID="pnlHISTArea" runat="server">
                        <asp:Label ID="WF_HISTTITLE" runat="server" Text="" CssClass="font18 PaddTop10"></asp:Label>
                        <div class="detailboxEX" >
                            <div class="detailboxEXSub" >
                                <asp:Panel ID="pnlHISTListArea" runat="server"></asp:Panel>
                            </div>                         
                        </div>   
                        <div class="Operation">
                            <span>
                                <input type="button" id="WF_ButtonCLOSE" class="btn-sticky" value="閉じる"  onclick="ButtonClick('WF_ButtonCLOSE');" />
                            </span>
                        </div>  
                    </asp:Panel>
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

    <!-- イベント用 -->
    <div style="display:none;">
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

        <asp:DropDownList ID="WF_TORI" runat="server" class="form-select rounded-0" />
        <asp:DropDownList ID="WF_TORIEXL" runat="server" class="form-select rounded-0" />
        <asp:DropDownList ID="WF_FILENAME" runat="server" class="form-select rounded-0" />
        <asp:DropDownList ID="WF_TORIORG" runat="server" class="form-select rounded-0" />
    </div>
 
</asp:Content>
