<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0007WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0007WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 一覧用 -->
    <asp:TextBox ID="WF_SEL_TARGETYM_L" runat="server"></asp:TextBox>              <!-- 対象年月 -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>              <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>              <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_ORGCODE" runat="server"></asp:TextBox>              <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_ORGNAME" runat="server"></asp:TextBox>              <!-- 部門名称 -->
    <asp:TextBox ID="WF_SEL_KASANORGCODE" runat="server"></asp:TextBox>              <!-- 加算先部門コード -->
    <asp:TextBox ID="WF_SEL_KASANORGNAME" runat="server"></asp:TextBox>              <!-- 加算先部門名称 -->
    <asp:TextBox ID="WF_SEL_TARGETYM" runat="server"></asp:TextBox>              <!-- 対象年月 -->
    <asp:TextBox ID="WF_SEL_SYABAN" runat="server"></asp:TextBox>              <!-- 車番 -->
    <asp:TextBox ID="WF_SEL_RIKUBAN" runat="server"></asp:TextBox>              <!-- 陸事番号 -->
    <asp:TextBox ID="WF_SEL_SYAGATA" runat="server"></asp:TextBox>              <!-- 車型 -->
    <asp:TextBox ID="WF_SEL_SYAGATANAME" runat="server"></asp:TextBox>              <!-- 車型名 -->
    <asp:TextBox ID="WF_SEL_SYABARA" runat="server"></asp:TextBox>              <!-- 車腹 -->
    <asp:TextBox ID="WF_SEL_SEASONKBN" runat="server"></asp:TextBox>              <!-- 季節料金判定区分 -->
    <asp:TextBox ID="WF_SEL_SEASONSTART" runat="server"></asp:TextBox>              <!-- 季節料金判定開始月日 -->
    <asp:TextBox ID="WF_SEL_SEASONEND" runat="server"></asp:TextBox>              <!-- 季節料金判定終了月日 -->
    <asp:TextBox ID="WF_SEL_KOTEIHIM" runat="server"></asp:TextBox>              <!-- 固定費(月額) -->
    <asp:TextBox ID="WF_SEL_KOTEIHID" runat="server"></asp:TextBox>              <!-- 固定費(日額) -->
    <asp:TextBox ID="WF_SEL_KAISU" runat="server"></asp:TextBox>              <!-- 回数 -->
    <asp:TextBox ID="WF_SEL_GENGAKU" runat="server"></asp:TextBox>              <!-- 減額費用 -->
    <asp:TextBox ID="WF_SEL_AMOUNT" runat="server"></asp:TextBox>              <!-- 請求額 -->
    <asp:TextBox ID="WF_SEL_BIKOU1" runat="server"></asp:TextBox>              <!-- 備考1 -->
    <asp:TextBox ID="WF_SEL_BIKOU2" runat="server"></asp:TextBox>              <!-- 備考2 -->
    <asp:TextBox ID="WF_SEL_BIKOU3" runat="server"></asp:TextBox>              <!-- 備考3 -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
