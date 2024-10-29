<%@ Page Title="LNT0001L" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0001OrderList.aspx.vb" Inherits="JOTWEB_LNG.LNT0001OrderList" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0001TILESELECTORWRKINC.ascx" TagName="tilelist" TagPrefix="MSINC"  %>

<asp:Content ID="LNT0001LH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0001L.css")%>' rel="stylesheet" type="text/css" /> 
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0001L.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId = '<%=Me.pnlListArea.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        //共通ポップアップボタン名
        var customPopUpOkButtonName = 'ﾀﾞｳﾝﾛｰﾄﾞ';
    </script>
</asp:Content>
 
<asp:Content ID="LNT0001L" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　headerbox -->
        <div class="headerboxOnly" id="headerbox">
            <div class="Operation">
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <!-- 会社 -->
                        <asp:Label ID="WF_SEL_CAMPCODE" runat="server" Text="会社" Visible="false"></asp:Label>
                        <asp:Label ID="WF_SEL_CAMPNAME" runat="server" CssClass="WF_TEXT_LEFT" Visible="false"></asp:Label>

                        <!-- 運用部署 -->
                        <asp:Label ID="WF_SELUORG_L" runat="server" Text="運用部署" Visible="false"></asp:Label>
                        <asp:Label ID="WF_SELUORG_TEXT" runat="server" CssClass="WF_TEXT_LEFT" Visible="false"></asp:Label>

                        <asp:Label ID="WF_SEL_CNT_TITLE" runat="server" Text="検索件数：" Visible="true"></asp:Label>
                        <asp:Label ID="WF_SEL_CNT" runat="server" Text="" Visible="true"></asp:Label>
                        <asp:Label ID="WF_SEL_KEN" runat="server" Text="件" Visible="true"></asp:Label>
                        <!-- 左ボタン -->
                        <div style="display:none;">
                        <input type="button" id="WF_ButtonALLSELECT" class="btn-sticky" value="全選択"  onclick="ButtonClick('WF_ButtonALLSELECT');" />
                        <input type="button" id="WF_ButtonSELECT_LIFTED" class="btn-sticky" value="選択解除"  onclick="ButtonClick('WF_ButtonSELECT_LIFTED');" />
                        <input type="button" id="WF_ButtonORDER_CANCEL" class="btn-sticky" value="キャンセル"  onclick="ButtonClick('WF_ButtonORDER_CANCEL');" />←選択した受注のキャンセル
                        </div>
                    </div>
                    <div class="rightSide">
                        <!-- 右ボタン -->
                        <input type="button" id="WF_ButtonDetailDownload" class="btn-sticky" value="明細ﾀﾞｳﾝﾛｰﾄﾞ" style="width:7em;"  onclick="ButtonClick('WF_ButtonDetailDownload');" />
                        <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="受注新規作成" style="width:7em;"  onclick="ButtonClick('WF_ButtonINSERT');" />
                        <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る"   onclick="ButtonClick('WF_ButtonEND');" />
                        <!-- 先頭行・末尾行ボタンを表示させる場合は divの括りを無くして WF_ButtonXXXを外だしにすれば出ます -->
                        <div style="display:none;">
                            <div id="WF_ButtonFIRST" class="firstPage" runat="server"                    onclick="ButtonClick('WF_ButtonFIRST');"></div>
                            <div id="WF_ButtonLAST" class="lastPage" runat="server"                      onclick="ButtonClick('WF_ButtonLAST');"></div>
                        </div>
                    </div>
                </div>
            </div>
            <asp:Panel ID="pnlListArea" runat="server"></asp:Panel>
        </div>

        <!-- rightbox レイアウト -->
        <MSINC:rightview ID="rightview" runat="server" />

        <!-- leftbox レイアウト -->
        <MSINC:leftview ID="leftview" runat="server" />

        <!-- Work レイアウト -->
        <MSINC:wrklist ID="work" runat="server" />

        <!-- イベント用 -->
        <div hidden="hidden">
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
            <!-- ボタン権限 -->
            <!-- 0 : 石油部/情報システム部  -->
            <!-- 1 : 東北支店/仙台  -->
            <!-- 2 : 関東支店/五井/甲子/袖ヶ浦/根岸  -->
            <!-- 3 : 中部支店/四日市/三重塩浜  -->
            <input id="WF_BUTTONpermitcode" runat="server" value="" type="text" />
            <input id="WF_BUTTONofficecode" runat="server" value="" type="text" />
        </div>
 
</asp:Content>

