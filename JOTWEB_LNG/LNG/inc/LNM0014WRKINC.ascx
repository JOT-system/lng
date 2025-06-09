<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0014WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0014WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 一覧用 -->
    <asp:TextBox ID="WF_SEL_TARGETYM_L" runat="server"></asp:TextBox>               <!-- 対象年月 -->
    <asp:TextBox ID="WF_SEL_TORI_L" runat="server"></asp:TextBox>                   <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_ORG_L" runat="server"></asp:TextBox>                    <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_TODOKE_L" runat="server"></asp:TextBox>                 <!-- 届先コード -->
    <asp:TextBox ID="WF_SEL_DEPARTURE_L" runat="server"></asp:TextBox>              <!-- 出荷場所コード -->
    <asp:TextBox ID="WF_SEL_CHKDELDATAFLG_L" runat="server"></asp:TextBox>          <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_INPUTPAGE_L" runat="server"></asp:TextBox>              <!-- 入力ページ -->
    <asp:TextBox ID="WF_SEL_NOWPAGECNT_L" runat="server"></asp:TextBox>             <!-- 表示中ページ -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                  <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_TARGETYM" runat="server"></asp:TextBox>                <!-- 対象年月 -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>                <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_ORGCODE" runat="server"></asp:TextBox>                 <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_ORGNAME" runat="server"></asp:TextBox>                 <!-- 部門名称 -->
    <asp:TextBox ID="WF_SEL_KASANORGCODE" runat="server"></asp:TextBox>            <!-- 加算先部門コード -->
    <asp:TextBox ID="WF_SEL_KASANORGNAME" runat="server"></asp:TextBox>            <!-- 加算先部門名称 -->
    <asp:TextBox ID="WF_SEL_TODOKECODE" runat="server"></asp:TextBox>              <!-- 届先コード -->
    <asp:TextBox ID="WF_SEL_TODOKENAME" runat="server"></asp:TextBox>              <!-- 届先名称 -->
    <asp:TextBox ID="WF_SEL_GROUPSORTNO" runat="server"></asp:TextBox>             <!-- グループソート順 -->
    <asp:TextBox ID="WF_SEL_GROUPID" runat="server"></asp:TextBox>                 <!-- グループID -->
    <asp:TextBox ID="WF_SEL_GROUPNAME" runat="server"></asp:TextBox>               <!-- グループ名 -->
    <asp:TextBox ID="WF_SEL_DETAILSORTNO" runat="server"></asp:TextBox>            <!-- 明細ソート順 -->
    <asp:TextBox ID="WF_SEL_DETAILID" runat="server"></asp:TextBox>                <!-- 明細ID -->
    <asp:TextBox ID="WF_SEL_DETAILNAME" runat="server"></asp:TextBox>              <!-- 明細名 -->
    <asp:TextBox ID="WF_SEL_TANKA" runat="server"></asp:TextBox>                   <!-- 単価 -->
    <asp:TextBox ID="WF_SEL_QUANTITY" runat="server"></asp:TextBox>                <!-- 数量 -->
    <asp:TextBox ID="WF_SEL_CALCUNIT" runat="server"></asp:TextBox>                <!-- 計算単位 -->
    <asp:TextBox ID="WF_SEL_DEPARTURE" runat="server"></asp:TextBox>               <!-- 出荷地 -->
    <asp:TextBox ID="WF_SEL_MILEAGE" runat="server"></asp:TextBox>                 <!-- 走行距離 -->
    <asp:TextBox ID="WF_SEL_SHIPPINGCOUNT" runat="server"></asp:TextBox>           <!-- 輸送回数 -->
    <asp:TextBox ID="WF_SEL_NENPI" runat="server"></asp:TextBox>                   <!-- 燃費 -->
    <asp:TextBox ID="WF_SEL_DIESELPRICECURRENT" runat="server"></asp:TextBox>      <!-- 実勢軽油価格 -->
    <asp:TextBox ID="WF_SEL_DIESELPRICESTANDARD" runat="server"></asp:TextBox>     <!-- 基準経由価格 -->
    <asp:TextBox ID="WF_SEL_DIESELCONSUMPTION" runat="server"></asp:TextBox>       <!-- 燃料使用量 -->
    <asp:TextBox ID="WF_SEL_DISPLAYFLG" runat="server"></asp:TextBox>              <!-- 表示フラグ -->
    <asp:TextBox ID="WF_SEL_ASSESSMENTFLG" runat="server"></asp:TextBox>           <!-- 鑑分けフラグ -->
    <asp:TextBox ID="WF_SEL_ATENACOMPANYNAME" runat="server"></asp:TextBox>        <!-- 宛名会社名 -->
    <asp:TextBox ID="WF_SEL_ATENACOMPANYDEVNAME" runat="server"></asp:TextBox>     <!-- 宛名会社部門名 -->
    <asp:TextBox ID="WF_SEL_FROMORGNAME" runat="server"></asp:TextBox>             <!-- 請求書発行部店名 -->
    <asp:TextBox ID="WF_SEL_MEISAICATEGORYID" runat="server"></asp:TextBox>        <!-- 明細区分 -->
    <asp:TextBox ID="WF_SEL_BIKOU1" runat="server"></asp:TextBox>                  <!-- 備考1 -->
    <asp:TextBox ID="WF_SEL_BIKOU2" runat="server"></asp:TextBox>                  <!-- 備考2 -->
    <asp:TextBox ID="WF_SEL_BIKOU3" runat="server"></asp:TextBox>                  <!-- 備考3 -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
