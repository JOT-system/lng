<%@ Page Title="LNS0001S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master"  CodeBehind="LNS0001UserSearch.aspx.vb" Inherits="JOTWEB_LNG.LNS0001UserSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNS0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:content id="LNS0001SH" contentplaceholderid="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNS0001S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNS0001S.js")%>'></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
</asp:content>

<asp:Content ID="LNS0001S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="d-inline-flex align-items-center flex-column w-100">
        <div class="d-flex w-100 wrap">
            <!-- サイドメニュー -->
            <MSINC:leftmenu ID="leftmenu" runat="server" />
            <div class="w-100 contents">
                <nav style="--bs-breadcrumb-divider: '>';" aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item d-flex align-items-center gap-1"><span class="material-symbols-outlined">home</span><a href="#">TOP</a></li>
                        <li class="breadcrumb-item active">ユーザーマスタ</li>
                        <li class="breadcrumb-item active" aria-current="page">ユーザーマスタ検索</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title">ユーザーマスタ検索</h2>
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
                            <div class="inputItem">
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
                            <!-- 有効年月日(開始） -->
                            <div class="inputItem">
                                <a id="WF_STYMD_LABEL">有効年月日（開始）</a>                
                                <div class="position-relative input-group calendar datetimepicker" data-target-input="nearest">
                                    <input type="text" id="WF_StYMDCode" runat="server" class="WF_TEXTBOX_CSS" data-input>
                                    <span class="input-group-text" data-toggle>
                                        <span class="material-symbols-outlined">calendar_month</span>
                                    </span>
                                </div>
                            </div>

                            <!-- 有効年月日(終了） -->
                            <div class="inputItem">
                                <a id="WF_ENDYMD_LABEL" >有効年月日（終了）</a>            
                                <div class="position-relative input-group calendar datetimepicker" data-target-input="nearest">
                                    <input type="text" id="WF_EndYMDCode" runat="server" class="WF_TEXTBOX_CSS" data-input>
                                    <span class="input-group-text" data-toggle>
                                        <span class="material-symbols-outlined">calendar_month</span>
                                    </span>
                                </div>
                            </div>

                            <!-- 組織コード -->
                            <div class="inputItem">
                                <a id="WF_ORG_LABEL" >組織コード</a>
                                <a class="ef" id="WF_ORG" ondblclick="Field_DBclick('TxtOrgCode', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtOrgCode');">
                                    <asp:TextBox ID="TxtOrgCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                                </a>
                                <a id="WF_ORG_TEXT">
                                    <asp:Label ID="LblOrgName" runat="server" CssClass="WF_TEXT"></asp:Label>
                                </a>
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
