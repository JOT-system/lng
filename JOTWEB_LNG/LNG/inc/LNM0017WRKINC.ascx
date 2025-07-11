<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0017WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0017WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 一覧用 -->
    <asp:TextBox ID="WF_SEL_TARGETYM_L" runat="server"></asp:TextBox>               <!-- 対象年月 -->
    <asp:TextBox ID="WF_SEL_TORI_L" runat="server"></asp:TextBox>                   <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_ORG_L" runat="server"></asp:TextBox>                    <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_SHABAN_FROM_L" runat="server"></asp:TextBox>            <!-- 車番FROM -->
    <asp:TextBox ID="WF_SEL_SHABAN_TO_L" runat="server"></asp:TextBox>              <!-- 車番TO -->
    <asp:TextBox ID="WF_SEL_SEASON_L" runat="server"></asp:TextBox>                 <!-- 季節料金 -->
    <asp:TextBox ID="WF_SEL_CHKDELDATAFLG_L" runat="server"></asp:TextBox>          <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_INPUTPAGE_L" runat="server"></asp:TextBox>              <!-- 入力ページ -->
    <asp:TextBox ID="WF_SEL_NOWPAGECNT_L" runat="server"></asp:TextBox>             <!-- 表示中ページ -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_ID" runat="server"></asp:TextBox>                       <!-- ＩＤ -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                 <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>                 <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_ORDERORGCODE" runat="server"></asp:TextBox>             <!-- 受注受付部署コード -->
    <asp:TextBox ID="WF_SEL_ORDERORGNAME" runat="server"></asp:TextBox>             <!-- 受注受付部署名称 -->
    <asp:TextBox ID="WF_SEL_ORDERORGCATEGORY" runat="server"></asp:TextBox>         <!-- 受注受付部署判定区分 -->
    <asp:TextBox ID="WF_SEL_ORDERORGCATEGORYNAME" runat="server"></asp:TextBox>     <!-- 受注受付部署判定区分名称 -->
    <asp:TextBox ID="WF_SEL_SHUKABASHO" runat="server"></asp:TextBox>               <!-- 出荷場所コード -->
    <asp:TextBox ID="WF_SEL_SHUKABASHONAME" runat="server"></asp:TextBox>           <!-- 出荷場所名称 -->
    <asp:TextBox ID="WF_SEL_SHUKABASHOCATEGORY" runat="server"></asp:TextBox>       <!-- 出荷場所判定区分 -->
    <asp:TextBox ID="WF_SEL_SHUKABASHOCATEGORYNAME" runat="server"></asp:TextBox>   <!-- 出荷場所判定区分名称 -->
    <asp:TextBox ID="WF_SEL_TODOKECODE" runat="server"></asp:TextBox>               <!-- 届先コード -->
    <asp:TextBox ID="WF_SEL_TODOKENAME" runat="server"></asp:TextBox>               <!-- 届先名称 -->
    <asp:TextBox ID="WF_SEL_TODOKECATEGORY" runat="server"></asp:TextBox>           <!-- 届先判定区分 -->
    <asp:TextBox ID="WF_SEL_TODOKECATEGORYNAME" runat="server"></asp:TextBox>       <!-- 届先判定区分名称 -->
    <asp:TextBox ID="WF_SEL_RANGECODE" runat="server"></asp:TextBox>                <!-- 休日範囲 -->
    <asp:TextBox ID="WF_SEL_GYOMUTANKNUMFROM" runat="server"></asp:TextBox>         <!-- 車番（開始） -->
    <asp:TextBox ID="WF_SEL_GYOMUTANKNUMTO" runat="server"></asp:TextBox>           <!-- 車番（終了） -->
    <asp:TextBox ID="WF_SEL_TANKA" runat="server"></asp:TextBox>                    <!-- 単価 -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
