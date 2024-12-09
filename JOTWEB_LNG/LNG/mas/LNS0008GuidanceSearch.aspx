<%@ Page Title="LNS0008S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNS0008GuidanceSearch.aspx.vb" Inherits="JOTWEB_LNG.LNS0008GuidanceSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNS0008WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNS0008SH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNS0008S.css")%>' rel="stylesheet" type="text/css" />
    <!--<script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNS0008S.js")%>'></script>-->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            // #contentsInnerの高さ取得
            let windowHeight = window.innerHeight;
            const headerHeight = 47;
            const breadcrumbHeight = 18;
            const spaceHeight = 8 + 16 + 16;
            let contentsInnerHeight = windowHeight - headerHeight - breadcrumbHeight - spaceHeight;
            document.getElementById("contentsInner").style.height = contentsInnerHeight + "px";

            // #fixed-tableの高さ取得
            const contentsTitleHeight = 44;
            let actionTriggerHeight = document.getElementById("actionTrigger").offsetHeight;
            const contentsSpaceHeight = 16 * 4;
            let fixedTableHeight = (contentsInnerHeight - contentsTitleHeight -actionTriggerHeight - contentsSpaceHeight) + "px";
            document.getElementById("fixed-table").style.height = fixedTableHeight;

            // カレンダー表示
            flatpickr('#datetimepicker1', {
                wrap: true,
                dateFormat: 'Y/m/d',
                locale : 'ja',
                clickOpens: false,
                allowInput: true,
                monthSelectorType: 'static',
                defaultDate: new Date()
            });
        });
    </script>
</asp:Content>

<asp:Content ID="LNS0008S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="d-inline-flex align-items-center flex-column w-100">
        <div class="d-flex w-100 wrap">
            <!-- サイドメニュー -->
            <MSINC:leftmenu ID="leftmenu" runat="server" />
            <!-- メイン画面（一覧） -->
            <div class="w-100 contents">
                <nav style="--bs-breadcrumb-divider: '>';" aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item d-flex align-items-center gap-1"><span class="material-symbols-outlined">home</span><a href="#">TOP</a></li>
                        <li class="breadcrumb-item active" aria-current="page">ガイダンス検索</li>
                    </ol>
                </nav>
                <h2 class="w-100 fs-5 fw-bold contents-title">ガイダンス検索</h2>
                <div class="searchbox" id="searchbox">
                    <!-- ○ 固定項目 ○ -->
                    <div class="actionButtonBox">
                        <div class="rightSide">
                            <input type="button" id="WF_ButtonSEARCH" class="btn-sticky" value="検索" onclick="ButtonClick('WF_ButtonSEARCH');" />
                            <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
                        </div>
                    </div> <!-- End actionButtonBox -->
                    <!-- ○ 変動項目 ○ -->
                    <div class="inputBox">

                        <!-- 掲載開始日 -->
                        <div class="inputItem">
                            <a id="WF_FROMYMD_LABEL">掲載開始日</a>
                            <a ondblclick="Field_DBclick('TxtFromYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                                <asp:TextBox  ID="TxtFromYmd" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10" ></asp:TextBox>
                            </a>
                        </div>

                        <!-- 掲載終了日 -->
                        <div class="inputItem">
                            <a id="WF_ENDYMD_LABEL">掲載終了日</a>
                            <a ondblclick="Field_DBclick('TxtEndYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                                <asp:TextBox  ID="TxtEndYmd" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                            </a>
                        </div>

                        <!-- 掲載フラグ -->
                        <div class="inputItem">
                            <a id="WF_Test">ガイダンスを表示する支店・営業所を選ぶ(複数チェックはいづれかに含まれるになります)</a>
                            <a>
                                <div class="grc0001Wrapper">
                                    <asp:CheckBoxList ID="ChklFlags" runat="server" ClientIDMode="Predictable" RepeatLayout="UnorderedList"></asp:CheckBoxList>
                                </div>
                            </a>
                        </div>

                        <!-- 論理削除フラグ -->
                        <div class="inputItem">
                            <a id="WF_DELDATAFLG">
                                <asp:CheckBox ID="ChkDelDataFlg" runat="server" Text="削除行を含む" />
                            </a>
                        </div>
                    </div> <!-- End inputBox -->
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
