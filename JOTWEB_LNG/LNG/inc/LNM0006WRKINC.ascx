<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0006WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0006WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_DELFLG_S" runat="server"></asp:TextBox>                <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_STYMD_S" runat="server"></asp:TextBox>                 <!-- 有効開始日 -->
    <asp:TextBox ID="WF_SEL_TORINAME_S" runat="server"></asp:TextBox>              <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_ORGCODE_S" runat="server"></asp:TextBox>               <!-- 部門コード -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>                <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_ORGCODE" runat="server"></asp:TextBox>                 <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_ORGNAME" runat="server"></asp:TextBox>                 <!-- 部門名称 -->
    <asp:TextBox ID="WF_SEL_KASANORGCODE" runat="server"></asp:TextBox>            <!-- 加算先部門コード -->
    <asp:TextBox ID="WF_SEL_KASANORGNAME" runat="server"></asp:TextBox>            <!-- 加算先部門名称 -->
    <asp:TextBox ID="WF_SEL_TODOKECODE" runat="server"></asp:TextBox>              <!-- 届先コード -->
    <asp:TextBox ID="WF_SEL_TODOKENAME" runat="server"></asp:TextBox>              <!-- 届先名称 -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>                   <!-- 有効開始日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>                  <!-- 有効終了日 -->
    <asp:TextBox ID="WF_SEL_BRANCHCODE" runat="server"></asp:TextBox>              <!-- 枝番 -->
    <asp:TextBox ID="WF_SEL_TANKA" runat="server"></asp:TextBox>                   <!-- 単価 -->
    <asp:TextBox ID="WF_SEL_SYAGATA" runat="server"></asp:TextBox>                 <!-- 車型 -->
    <asp:TextBox ID="WF_SEL_SYAGOU" runat="server"></asp:TextBox>                  <!-- 車号 -->
    <asp:TextBox ID="WF_SEL_SYABARA" runat="server"></asp:TextBox>                 <!-- 車腹 -->
    <asp:TextBox ID="WF_SEL_SYUBETSU" runat="server"></asp:TextBox>                <!-- 種別 -->
    <asp:TextBox ID="WF_SEL_BIKOU1" runat="server"></asp:TextBox>                  <!-- 備考1 -->
    <asp:TextBox ID="WF_SEL_BIKOU2" runat="server"></asp:TextBox>                  <!-- 備考2 -->
    <asp:TextBox ID="WF_SEL_BIKOU3" runat="server"></asp:TextBox>                  <!-- 備考3 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                  <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
