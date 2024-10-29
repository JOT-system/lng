<%@ Page Title="LNT0019D" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0019ClosingBranch.aspx.vb" Inherits="JOTWEB_LNG.LNT0019ClosingBranch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0019WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0001TILESELECTORWRKINC.ascx" TagName="tilelist" TagPrefix="MSINC"  %>

<%@ Register assembly="FarPoint.Web.SpreadJ" namespace="FarPoint.Web.Spread" tagprefix="FarPoint" %>

<asp:Content ID="LNT0019DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0019S.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0019S.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>

</asp:Content>

<asp:Content ID="LNT0019D" ContentPlaceHolderID="contents1" runat="server">
    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        </div>
    <!-- 全体レイアウト　detailbox -->
    <div  class="detailbox" id="detailbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <!-- ボタン -->
            <div class="rightSide">
                <!-- 右ボタン -->
                <input type="button" id="WF_ButtonEND" class="btn-sticky btn-back" value="戻る(「メニューへ」ボタン表示用)") onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div>
        <!-- １行目：空白行 -->
        <asp:Panel ID="Panel1" CssClass="detailboxLineBlank" runat="server"></asp:Panel>
            <!-- ２行目：ラベル -->
            <asp:Panel ID="Panel2" CssClass="detailboxLineLabel" runat="server">
                <div class="divKeijoYM">
                    <a id="WF_KEIJOYM_LABEL">計上年月</a>
                </div>
                <a class="inputItem" id="WF_TARGETYM_AREA">
                    <asp:TextBox ID="txtDownloadMonth" class="txtDownloadMonth" runat="server" data-monthpicker="1" data-monthpickerneedspostback="1"></asp:TextBox>
                </a>
            </asp:Panel>
        <!-- ３行目：空白行 -->
        <asp:Panel ID="Panel3" CssClass="detailboxLineBlank2" runat="server"></asp:Panel>

        <div class="panelFrame">
            <!-- ４行目：ラベル -->
                <asp:Panel ID="Panel4" CssClass="detailboxLineLabel" runat="server">
                    <div class="divBranch">
                        <a id="WF_BRANCH_LABEL">支店</a>
                    </div>
                    <div class="divBranch">
                        ステータス
                    </div>
                    <div class="divBranch">
                        <a id="WF_RENTAL_LABEL">レンタル使用料</a>
                    </div>
                    <div class="divBranch">
                        <a id="WF_LEASE_LABEL">リース料</a>
                    </div>
                    <div class="divBranch">
                        <a id="WF_WRITE_LABEL">手書き請求書</a>
                    </div>
                    <div class="divBranch">
                        <a id="WF_CTNSALE_LABEL">コンテナ売却</a>
                    </div>
                    <div class="divBranch">
                        <a id="WF_PAYMENT_LABEL">回送費</a>
                    </div>
                </asp:Panel>
            <!-- ５行目：空白行 -->
            <asp:Panel ID="Panel5" CssClass="detailboxLineBlank2line" runat="server">
                <p class="divBranch">北海道支店</p>
                <p class="divBranch">原価計算済み</p>
                <span class="spanLong"><!-- 空白 --></span>
                <div id="Hokkaido5" class="divCtnSale">
                    <asp:Label id="WF_CTNSALECALCULATION_HOKKAIDOSYO" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_CTNSALECALCULATION1">/</a>
                    <asp:Label id="WF_CTNSALECALCULATION_HOKKAIDOTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_CTNSALECALCULATION2" class="size">件</a>
                </div>
            </asp:Panel>
            <!-- ６行目：ラベル -->
                <asp:Panel ID="Panel6" CssClass="detailboxLineLabel" runat="server">
                    <div class="divBranch">
                        <a id="WF_HOKKAIDO_LABEL"><!-- 北海道支店 --><!-- 空白 --></a>
                    </div>
                    <p class="divBranch">承認済み</p>
                    <div id="Hokkaido0" class="divRental">
                        <asp:Label id="WF_RENTAL_HOKKAIDOSYO" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_RENTAL1" class="size">/</a>
                        <asp:Label id="WF_RENTAL_HOKKAIDOTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_RENTAL2" class="size">件</a>
                    </div>
                    <div id="Hokkaido1" class="divLease">
                        <asp:Label id="WF_LEASE_HOKKAIDOSYO" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_LEASE1" class="size">/</a>
                        <asp:Label id="WF_LEASE_HOKKAIDOTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_LEASE2" class="size">件</a>
                    </div>
                    <div id="Hokkaido2" class="divWrite">
                        <asp:Label id="WF_WRITE_HOKKAIDOSYO" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_WRITE1" class="size">/</a>
                        <asp:Label id="WF_WRITE_HOKKAIDOTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_WRITE2" class="size">件</a>
                    </div>
                    <div id="Hokkaido3" class="divCtnSale">
                        <asp:Label id="WF_CTNSALE_HOKKAIDOSYO" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_CTNSALE1">/</a>
                        <asp:Label id="WF_CTNSALE_HOKKAIDOTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_CTNSALE2" class="size">件</a>
                    </div>
                    <div id="Hokkaido4" class="divPayment">
                        <asp:Label id="WF_PAYMENT_HOKKAIDOSYO" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_PAYMENT1">/</a>
                        <asp:Label id="WF_PAYMENT_HOKKAIDOTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_HOKKAIDO_PAYMENT2" class="size">件</a>
                    </div>
                </asp:Panel>
            <!-- ７行目：空白行 -->
            <asp:Panel ID="Panel7" CssClass="detailboxLineBlank2line" runat="server">
                <p class="divBranch">東北支店</p>
                <p class="divBranch">原価計算済み</p>
                <span class="spanLong"><!-- 空白 --></span>
                <div id="Touhoku5" class="divCtnSale">
                    <asp:Label id="WF_CTNSALECALCULATION_TOUHOKUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_CTNSALECALCULATION1">/</a>
                    <asp:Label id="WF_CTNSALECALCULATION_TOUHOKUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_CTNSALECALCULATION2" class="size">件</a>
                </div>
            </asp:Panel>
            <!-- ８行目：ラベル -->
                <asp:Panel ID="Panel8" CssClass="detailboxLineLabel" runat="server">
                    <div class="divBranch">
                        <a id="WF_TOUHOKU_LABEL"><!-- 東北支店 --><!-- 空白 --></a>
                    </div>
                    <p class="divBranch">承認済み</p>
                    <div id="Touhoku0" class="divRental">
                        <asp:Label id="WF_RENTAL_TOUHOKUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_RENTAL1" class="size">/</a>
                        <asp:Label id="WF_RENTAL_TOUHOKUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_RENTAL2" class="size">件</a>
                    </div>
                    <div id="Touhoku1" class="divLease">
                        <asp:Label id="WF_LEASE_TOUHOKUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_LEASE1" class="size">/</a>
                        <asp:Label id="WF_LEASE_TOUHOKUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_LEASE2" class="size">件</a>
                    </div>
                    <div id="Touhoku2" class="divWrite">
                        <asp:Label id="WF_WRITE_TOUHOKUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_WRITE1" class="size">/</a>
                        <asp:Label id="WF_WRITE_TOUHOKUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_WRITE2" class="size">件</a>
                    </div>
                    <div id="Touhoku3" class="divCtnSale">
                        <asp:Label id="WF_CTNSALE_TOUHOKUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_CTNSALE1" class="size">/</a>
                        <asp:Label id="WF_CTNSALE_TOUHOKUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_CTNSALE2" class="size">件</a>
                    </div>
                    <div id="Touhoku4" class="divPayment">
                        <asp:Label id="WF_PAYMENT_TOUHOKUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_PAYMENT1" class="size">/</a>
                        <asp:Label id="WF_PAYMENT_TOUHOKUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TOUHOKU_PAYMENT2" class="size">件</a>
                    </div>
                </asp:Panel>
            <!-- ９行目：空白行 -->
            <asp:Panel ID="Panel9" CssClass="detailboxLineBlank2line" runat="server">
                <p class="divBranch">関東支店</p>
                <p class="divBranch">原価計算済み</p>
                <span class="spanLong"><!-- 空白 --></span>
                <div id="Kantou5" class="divCtnSale">
                    <asp:Label id="WF_CTNSALECALCULATION_KANTOUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_CTNSALECALCULATION1">/</a>
                    <asp:Label id="WF_CTNSALECALCULATION_KANTOUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_CTNSALECALCULATION2" class="size">件</a>
                </div>
            </asp:Panel>
            <!-- １０行目：ラベル -->
                <asp:Panel ID="Panel10" CssClass="detailboxLineLabel" runat="server">
                    <div class="divBranch">
                        <a id="WF_KANTOU_LABEL"><!-- 関東支店 --><!-- 空白 --></a>
                    </div>
                    <p class="divBranch">承認済み</p>
                    <div id="Kantou0" class="divRental">
                        <asp:Label id="WF_RENTAL_KANTOUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_RENTAL1" class="size">/</a>
                        <asp:Label id="WF_RENTAL_KANTOUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_RENTAL2" class="size">件</a>
                    </div>
                    <div id="Kantou1" class="divLease">
                        <asp:Label id="WF_LEASE_KANTOUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_LEASE1" class="size">/</a>
                        <asp:Label id="WF_LEASE_KANTOUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_LEASE2" class="size">件</a>
                    </div>
                    <div id="Kantou2" class="divWrite">
                        <asp:Label id="WF_WRITE_KANTOUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_WRITE1" class="size">/</a>
                        <asp:Label id="WF_WRITE_KANTOUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_WRITE2" class="size">件</a>
                    </div>
                    <div id="Kantou3" class="divCtnSale">
                        <asp:Label id="WF_CTNSALE_KANTOUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_CTNSALE1" class="size">/</a>
                        <asp:Label id="WF_CTNSALE_KANTOUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_CTNSALE2" class="size">件</a>
                    </div>
                    <div id="Kantou4" class="divPayment">
                        <asp:Label id="WF_PAYMENT_KANTOUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_PAYMENT1" class="size">/</a>
                        <asp:Label id="WF_PAYMENT_KANTOUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANTOU_PAYMENT2" class="size">件</a>
                    </div>
                </asp:Panel>
            <!-- １１行目：空白行 -->
            <asp:Panel ID="Panel11" CssClass="detailboxLineBlank2line" runat="server">
                <p class="divBranch">中部支店</p>
                <p class="divBranch">原価計算済み</p>
                <span class="spanLong"><!-- 空白 --></span>
                <div id="Tyubu5" class="divCtnSale">
                    <asp:Label id="WF_CTNSALECALCULATION_TYUBUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_CTNSALECALCULATION1">/</a>
                    <asp:Label id="WF_CTNSALECALCULATION_TYUBUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_CTNSALECALCULATION2" class="size">件</a>
                </div>
            </asp:Panel>
            <!-- １２行目：ラベル -->
                <asp:Panel ID="Panel12" CssClass="detailboxLineLabel" runat="server">
                    <div class="divBranch">
                        <a id="WF_TYUBU_LABEL"><!-- 中部支店 --><!-- 空白 --></a>
                    </div>
                    <p class="divBranch">承認済み</p>
                    <div id="Tyubu0" class="divRental">
                        <asp:Label id="WF_RENTAL_TYUBUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_RENTAL1" class="size">/</a>
                        <asp:Label id="WF_RENTAL_TYUBUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_RENTAL2" class="size">件</a>
                    </div>
                    <div id="Tyubu1" class="divLease">
                        <asp:Label id="WF_LEASE_TYUBUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_LEASE1" class="size">/</a>
                        <asp:Label id="WF_LEASE_TYUBUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_LEASE2" class="size">件</a>
                    </div>
                    <div id="Tyubu2" class="divWrite">
                        <asp:Label id="WF_WRITE_TYUBUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_WRITE1" class="size">/</a>
                        <asp:Label id="WF_WRITE_TYUBUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_WRITE2" class="size">件</a>
                    </div>
                    <div id="Tyubu3" class="divCtnSale">
                        <asp:Label id="WF_CTNSALE_TYUBUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_CTNSALE1" class="size">/</a>
                        <asp:Label id="WF_CTNSALE_TYUBUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_CTNSALE2" class="size">件</a>
                    </div>
                    <div id="Tyubu4" class="divPayment">
                        <asp:Label id="WF_PAYMENT_TYUBUSYO" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_PAYMENT1" class="size">/</a>
                        <asp:Label id="WF_PAYMENT_TYUBUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_TYUBU_PAYMENT2" class="size">件</a>
                    </div>
                </asp:Panel>
            <!-- １３行目：空白行 -->
            <asp:Panel ID="Panel13" CssClass="detailboxLineBlank2line" runat="server">
                <p class="divBranch">関西支店</p>
                <p class="divBranch">原価計算済み</p>
                <span class="spanLong"><!-- 空白 --></span>
                <div id="Kansai5" class="divCtnSale">
                    <asp:Label id="WF_CTNSALECALCULATION_KANSAISYO" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_CTNSALECALCULATION1">/</a>
                    <asp:Label id="WF_CTNSALECALCULATION_KANSAITOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_CTNSALECALCULATION2" class="size">件</a>
                </div>
            </asp:Panel>
            <!-- １４行目：ラベル -->
                <asp:Panel ID="Panel14" CssClass="detailboxLineLabel" runat="server">
                    <div class="divBranch">
                        <a id="WF_KANSAI_LABEL"><!-- 関西支店 --><!-- 空白 --></a>
                    </div>
                    <p class="divBranch">承認済み</p>
                    <div id="Kansai0" class="divRental">
                        <asp:Label id="WF_RENTAL_KANSAISYO" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_RENTAL1" class="size">/</a>
                        <asp:Label id="WF_RENTAL_KANSAITOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_RENTAL2" class="size">件</a>
                    </div>
                    <div id="Kansai1"  class="divLease">
                        <asp:Label id="WF_LEASE_KANSAISYO" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_LEASE1" class="size">/</a>
                        <asp:Label id="WF_LEASE_KANSAITOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_LEASE2" class="size">件</a>
                    </div>
                    <div id="Kansai2" class="divWrite">
                        <asp:Label id="WF_WRITE_KANSAISYO" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_WRITE1" class="size">/</a>
                        <asp:Label id="WF_WRITE_KANSAITOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_WRITE2" class="size">件</a>
                    </div>
                    <div id="Kansai3" class="divCtnSale">
                        <asp:Label id="WF_CTNSALE_KANSAISYO" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_CTNSALE1" class="size">/</a>
                        <asp:Label id="WF_CTNSALE_KANSAITOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_CTNSALE2" class="size">件</a>
                    </div>
                    <div id="Kansai4" class="divPayment">
                        <asp:Label id="WF_PAYMENT_KANSAISYO" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_PAYMENT1" class="size">/</a>
                        <asp:Label id="WF_PAYMENT_KANSAITOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KANSAI_PAYMENT2" class="size">件</a>
                    </div>
                </asp:Panel>
            <!-- １５行目：空白行 -->
            <asp:Panel ID="Panel15" CssClass="detailboxLineBlank2line" runat="server">
                <p class="divBranch">九州支店</p>
                <p class="divBranch">原価計算済み</p>
                <span class="spanLong"><!-- 空白 --></span>
                <div id="Kyusyu5" class="divCtnSale">
                    <asp:Label id="WF_CTNSALECALCULATION_KYUSYUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_CTNSALECALCULATION1">/</a>
                    <asp:Label id="WF_CTNSALECALCULATION_KYUSYUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_CTNSALECALCULATION2" class="size">件</a>
                </div>
            </asp:Panel>
            <!-- １６行目：ラベル -->
                <asp:Panel ID="Panel16" CssClass="detailboxLineLabel" runat="server">
                    <div class="divBranch">
                        <a id="WF_KYUUSYU_LABEL"><!-- 九州支店 --><!-- 空白 --></a>
                    </div>
                    <p class="divBranch">承認済み</p>
                    <div id="Kyusyu0" class="divRental">
                        <asp:Label id="WF_RENTAL_KYUSYUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_RENTAL1" class="size">/</a>
                        <asp:Label id="WF_RENTAL_KYUSYUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_RENTAL2" class="size">件</a>
                    </div>
                    <div id="Kyusyu1" class="divLease">
                        <asp:Label id="WF_LEASE_KYUSYUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_LEASE1" class="size">/</a>
                        <asp:Label id="WF_LEASE_KYUSYUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_LEASE2" class="size">件</a>
                    </div>
                    <div id="Kyusyu2" class="divWrite">
                        <asp:Label id="WF_WRITE_KYUSYUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_WRITE1" class="size">/</a>
                        <asp:Label id="WF_WRITE_KYUSYUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_WRITE2" class="size">件</a>
                    </div>
                    <div id="Kyusyu3" class="divCtnSale">
                        <asp:Label id="WF_CTNSALE_KYUSYUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_CTNSALE1" class="size">/</a>
                        <asp:Label id="WF_CTNSALE_KYUSYUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_CTNSALE2" class="size">件</a>
                    </div>
                    <div id="Kyusyu4" class="divPayment">
                        <asp:Label id="WF_PAYMENT_KYUSYUSYO" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_PAYMENT1" class="size">/</a>
                        <asp:Label id="WF_PAYMENT_KYUSYUTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_KYUSYU_PAYMENT2" class="size">件</a>
                    </div>
                </asp:Panel>
            <!-- １７行目：空白行 -->
            <asp:Panel ID="Panel17" CssClass="detailboxLineBlank2line" runat="server">
                <p class="divBranch">コンテナ部</p>
                <p class="divBranch">原価計算済み</p>
                <span class="spanLong"><!-- 空白 --></span>
                <div id="CTN5" class="divCtnSale">
                    <asp:Label id="WF_CTNSALECALCULATION_CTNSYO" runat="server" width="" text =""></asp:Label><a id="WF_CTN_CTNSALECALCULATION1">/</a>
                    <asp:Label id="WF_CTNSALECALCULATION_CTNTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_CTN_CTNSALECALCULATION2" class="size">件</a>
                </div>
            </asp:Panel>
            <!-- １８行目：ラベル -->
                <asp:Panel ID="Panel18" CssClass="detailboxLineLabel" runat="server">
                    <div class="divBranch">
                        <a id="WF_CTN_LABEL"><!-- コンテナ部 --><!-- 空白 --></a>
                    </div>
                    <p class="divBranch">承認済み</p>
                    <div id="CTN0" class="divRental">
                        <asp:Label id="WF_RENTAL_CTNSYO" runat="server" width="" text =""></asp:Label><a id="WF_CTN_RENTAL1" class="size">/</a>
                        <asp:Label id="WF_RENTAL_CTNTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_CTN_RENTAL2" class="size">件</a>
                    </div>
                    <div id="CTN1" class="divLease">
                        <asp:Label id="WF_LEASE_CTNSYO" runat="server" width="" text =""></asp:Label><a id="WF_CTN_LEASE1" class="size">/</a>
                        <asp:Label id="WF_LEASE_CTNTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_CTN_LEASE2" class="size">件</a>
                    </div>
                    <div id="CTN2" class="divWrite">
                        <asp:Label id="WF_WRITE_CTNSYO" runat="server" width="" text =""></asp:Label><a id="WF_CTN_WRITE1" class="size">/</a>
                        <asp:Label id="WF_WRITE_CTNTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_CTN_WRITE2" class="size">件</a>
                    </div>
                    <div id="CTN3" class="divCtnSale">
                        <asp:Label id="WF_CTNSALE_CTNSYO" runat="server" width="" text =""></asp:Label><a id="WF_CTN_CTNSALE1" class="size">/</a>
                        <asp:Label id="WF_CTNSALE_CTNTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_CTN_CTNSALE2" class="size">件</a>
                    </div>
                    <div id="CTN4" class="divPayment">
                        <asp:Label id="WF_PAYMENT_CTNSYO" runat="server" width="" text =""></asp:Label><a id="WF_CTN_PAYMENT1" class="size">/</a>
                        <asp:Label id="WF_PAYMENT_CTNTOTAL" runat="server" width="" text =""></asp:Label><a id="WF_CTN_PAYMENT2" class="size">件</a>
                    </div>
                </asp:Panel>
            </div>
            <!-- １９行目：空白行 -->
            <asp:Panel ID="Panel19" CssClass="detailboxLineBlank3" runat="server"></asp:Panel>
            <!-- ２０行目：経理連携 -->
                <asp:Panel ID="Panel20" CssClass="detailboxLineLabel" runat="server">
                    <div class="divCSVClose">
                        <a id="WF_CSV_LABEL">経理連携Excel</a>
                    </div>
                    <input type="button" id="WF_CSV_DL" class="downloadbtn-sticky" value="ダウンロード" onclick="ButtonClick('WF_CSV_DL');" />
                    <asp:Label id="WF_SHONIN_LABEL" runat="server"></asp:Label>
                </asp:Panel>
            <!-- ２１行目：空白行 -->
            <asp:Panel ID="Panel21" CssClass="detailboxLineBlank2" runat="server"></asp:Panel>
            <!-- ２２行目：締め確定 -->
                <asp:Panel ID="Panel22" CssClass="detailboxLineLabel" runat="server">
                    <div class="divCSVClose">
                        <a id="WF_CLOS_LABEL">締め確定</a>
                    </div>
                    <div class="singleInput">
                        <!-- 選択ボタン -->
                        <div class="right-harf">
                            <MSINC:tilelist ID="WF_CLOSTA" runat="server"/>
                        </div>
                    </div>
                    <asp:Label id="WF_GENKA_LABEL" runat="server"></asp:Label>
                </asp:Panel>
    </div>

    <!-- 非表示項目 -->
    <asp:HiddenField ID="hdnControl" runat="server" Visible="false" ClientIDMode="Predictable"  />

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD" runat="server" value="" type="text" />
        <!-- Textbox(Repeater) DBクリックフィールド -->
        <input id="WF_FIELD_REP" runat="server" value="" type="text" />
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />

        <!-- Textbox Print URL -->
        <input id="WF_PrintURL" runat="server" value="" type="text" />

        <!-- ボタン入力制御フラグ -->
        <input id="WF_CSVDLDisabledFlg" runat="server" value="" type="text" />

        <!-- 北海道使用料(非表示) -->
        <input id="WF_Hokkaido0" runat="server" value="" type="text" />
        <!-- 北海道リース(非表示) -->
        <input id="WF_Hokkaido1" runat="server" value="" type="text" />
        <!-- 北海道手書き(非表示) -->
        <input id="WF_Hokkaido2" runat="server" value="" type="text" />
        <!-- 北海道コンテナ売却(非表示) -->
        <input id="WF_Hokkaido3" runat="server" value="" type="text" />
        <!-- 北海道回送費(非表示) -->
        <input id="WF_Hokkaido4" runat="server" value="" type="text" />
        <!-- 北海道コンテナ売却(原価計算)(非表示) -->
        <input id="WF_Hokkaido5" runat="server" value="" type="text" />
        <!-- 東北使用料(非表示) -->
        <input id="WF_Touhoku0" runat="server" value="" type="text" />
        <!-- 東北リース(非表示) -->
        <input id="WF_Touhoku1" runat="server" value="" type="text" />
        <!-- 東北手書き(非表示) -->
        <input id="WF_Touhoku2" runat="server" value="" type="text" />
        <!-- 東北コンテナ売却(非表示) -->
        <input id="WF_Touhoku3" runat="server" value="" type="text" />
        <!-- 東北回送費(非表示) -->
        <input id="WF_Touhoku4" runat="server" value="" type="text" />
        <!-- 東北コンテナ売却(原価計算)(非表示) -->
        <input id="WF_Touhoku5" runat="server" value="" type="text" />
        <!-- 関東使用料(非表示) -->
        <input id="WF_Kantou0" runat="server" value="" type="text" />
        <!-- 関東リース(非表示) -->
        <input id="WF_Kantou1" runat="server" value="" type="text" />
        <!-- 関東手書き(非表示) -->
        <input id="WF_Kantou2" runat="server" value="" type="text" />
        <!-- 関東コンテナ売却(非表示) -->
        <input id="WF_Kantou3" runat="server" value="" type="text" />
        <!-- 関東回送費(非表示) -->
        <input id="WF_Kantou4" runat="server" value="" type="text" />
        <!-- 関東コンテナ売却(原価計算)(非表示) -->
        <input id="WF_Kantou5" runat="server" value="" type="text" />
        <!-- 中部使用料(非表示) -->
        <input id="WF_Tyubu0" runat="server" value="" type="text" />
        <!-- 中部リース(非表示) -->
        <input id="WF_Tyubu1" runat="server" value="" type="text" />
        <!-- 中部手書き(非表示) -->
        <input id="WF_Tyubu2" runat="server" value="" type="text" />
        <!-- 中部コンテナ売却(非表示) -->
        <input id="WF_Tyubu3" runat="server" value="" type="text" />
        <!-- 中部回送費(非表示) -->
        <input id="WF_Tyubu4" runat="server" value="" type="text" />
        <!-- 中部コンテナ売却(原価計算)(非表示) -->
        <input id="WF_Tyubu5" runat="server" value="" type="text" />
        <!-- 関西使用料(非表示) -->
        <input id="WF_Kansai0" runat="server" value="" type="text" />
        <!-- 関西リース(非表示) -->
        <input id="WF_Kansai1" runat="server" value="" type="text" />
        <!-- 関西手書き(非表示) -->
        <input id="WF_Kansai2" runat="server" value="" type="text" />
        <!-- 関西コンテナ売却(非表示) -->
        <input id="WF_Kansai3" runat="server" value="" type="text" />
        <!-- 関西回送費(非表示) -->
        <input id="WF_Kansai4" runat="server" value="" type="text" />
        <!-- 関西コンテナ売却(原価計算)(非表示) -->
        <input id="WF_Kansai5" runat="server" value="" type="text" />
        <!-- 九州使用料(非表示) -->
        <input id="WF_Kyusyu0" runat="server" value="" type="text" />
        <!-- 九州リース(非表示) -->
        <input id="WF_Kyusyu1" runat="server" value="" type="text" />
        <!-- 九州手書き(非表示) -->
        <input id="WF_Kyusyu2" runat="server" value="" type="text" />
        <!-- 九州コンテナ売却(非表示) -->
        <input id="WF_Kyusyu3" runat="server" value="" type="text" />
        <!-- 九州回送費(非表示) -->
        <input id="WF_Kyusyu4" runat="server" value="" type="text" />
        <!-- 九州コンテナ売却(原価計算)(非表示) -->
        <input id="WF_Kyusyu5" runat="server" value="" type="text" />
        <!-- コンテナ部使用料(非表示) -->
        <input id="WF_CTN0" runat="server" value="" type="text" />
        <!-- コンテナ部リース(非表示) -->
        <input id="WF_CTN1" runat="server" value="" type="text" />
        <!-- コンテナ部手書き(非表示) -->
        <input id="WF_CTN2" runat="server" value="" type="text" />
        <!-- コンテナ部コンテナ売却(非表示) -->
        <input id="WF_CTN3" runat="server" value="" type="text" />
        <!-- コンテナ部回送費(非表示) -->
        <input id="WF_CTN4" runat="server" value="" type="text" />
        <!-- コンテナ部コンテナ売却(原価計算)(非表示) -->
        <input id="WF_CTN5" runat="server" value="" type="text" />


        <!-- 経理締め確定(非表示) -->
        <input id="WF_CloseFLG" runat="server" value="" type="text" />

        <!-- 画面表示切替 -->
        <input id="WF_DISP" runat="server" value="" type="text" />
        <!-- LeftBox Mview切替 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
        <!-- LeftBox 開閉 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
        <!-- Rightbox Mview切替 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />
        <!-- Rightbox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />

        <!-- ボタン押下 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />
        <!-- 権限 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />

        <!-- Textbox Print URL -->
        <input id="WF_PrintURL1" runat="server" value="" type="text" />
        <input id="WF_PrintURL2" runat="server" value="" type="text" />

    </div>
</asp:Content>
