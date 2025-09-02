<%@ Page Title="LNT0031L" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0031DieselPriceHist.aspx.vb" Inherits="JOTWEB_LNG.LNT0031DieselPriceHist" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview2" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0031WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNT0031LH" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNT0031L.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0031L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="LNT0031L" ContentPlaceHolderID="contents1" runat="server">
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
                        <li class="breadcrumb-item active"><a style="cursor: pointer;text-decoration:underline" onclick="ButtonClick('LNM0019L');">サーチャージ定義マスタ</a></li>
                        <li class="breadcrumb-item active" id="PAGE_NAME1" aria-current="page">実勢単価履歴（登録）</li>
                    </ol>
                </nav>
                <div id="contentsInner" class="border bg-white px-3 py-3 overflow-hidden contents-inner">
                    <h2 class="w-100 fs-5 fw-bold contents-title">実勢単価（軽油価格）履歴登録</h2>
                        <div class="Operation">
                            <div class="actionButtonBox">
                                <div class="rightSide">
                                    <%--<input type="button" id="WF_ButtonDebug" class="btn-sticky" value="デバッグ" onclick="ButtonClick('WF_ButtonDebug');" />--%>
                                    <asp:Label ID="WF_UPLOAD_LABEL" AssociatedControlID="WF_UPLOAD_BTN" runat="server" CssClass="btn-sticky btn-action" Text="ｱｯﾌﾟﾛｰﾄﾞ"> <asp:FileUpload ID="WF_UPLOAD_BTN" runat="server"  onchange="ButtonClick('WF_ButtonUPLOAD')"/>
                                    </asp:Label>
                                    <%--<input type="button" id="WF_ButtonHISTORY"  class="btn-sticky" value="変更履歴" onclick="ButtonClick('WF_ButtonHISTORY');" />--%>
                                    <input type="button" id="WF_ButtonDOWNLOAD" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonDOWNLOAD');" />
                                    <%--<input type="button" id="WF_ButtonPRINT"    class="btn-sticky" value="一覧印刷" onclick="ButtonClick('WF_ButtonPRINT');" />--%>
                                    <%--<input type="button" id="WF_ButtonEND"      class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />--%>
                                    <input type="button" id="WF_ButtonEND2"      class="btn-sticky" value="戻る"     onclick="ButtonClick('WF_ButtonEND');" />
                                    <%--<div id="WF_ButtonFIRST" class="firstPage" runat="server"                       onclick="ButtonClick('WF_ButtonFIRST');"></div>--%>
                                    <%--<div id="WF_ButtonLAST" class="lastPage" runat="server"                         onclick="ButtonClick('WF_ButtonLAST');"></div>--%>
                                </div>
                            </div> <!-- End class=actionButtonBox -->
                        </div> <!-- End class="Operation" -->
                        <div class="searchBar">
                            <div id="actionTrigger" class="d-flex flex-wrap gap-3 w-100">
                                <div class="actionButtonBox">
                                    <div class="d-flex align-items-center gap-2 me-3">
                                        <!-- 一覧件数 -->
                                        <asp:Label ID="ListCount" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                                    </div>
                                    <div class="d-flex align-items-center gap-2">
                                        <strong class="flex-shrink-0">軽油価格参照先：</strong>
                                        <asp:textBox ID="WF_DIESELPRICESITENAME" runat="server" Text="軽油価格参照先名" CssClass="WF_TEXT_LEFT" disabled="true"></asp:textBox>
                                        <asp:textBox ID="WF_DIESELPRICESITEKBNNAME" runat="server" Text="軽油価格参照先区分名" CssClass="WF_TEXT_LEFT" disabled="true"></asp:textBox>
                                    </div>

                                    <div class="Operation" style="width:170px" >
                                        <div class="actionButtonBox">
                                            <div class="centerSide">
                                                <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="行追加" onclick="BtnAddClick('WF_ButtonINSERT');" />
                                            </div>
                                        </div>
                                    </div>
                                     <div class="Operation">
                                        <div class="actionButtonBox">
                                           <div class="rightSide">
                                                <input type="button" id="WF_ButtonUPDATE" class="btn-sticky btn-action" value="更新" onclick="ButtonClick('WF_ButtonUPDATE');" />
                                            </div>
                                        </div>
                                    </div>
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
