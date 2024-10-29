<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0011WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0011WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                  <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                　 <!-- 選択行 -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_DEPSTATION_S" runat="server"></asp:TextBox>              <!-- 発駅コード -->
    <asp:TextBox ID="WF_SEL_ARRSTATION_S" runat="server"></asp:TextBox>              <!-- 着駅コード -->
    <asp:TextBox ID="WF_SEL_DELFLG_S" runat="server"></asp:TextBox>                  <!-- 削除フラグ -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_DEPSTATION" runat="server"></asp:TextBox>                <!-- 発駅コード -->
    <asp:TextBox ID="WF_SEL_ARRSTATION" runat="server"></asp:TextBox>                <!-- 着駅コード -->
    <asp:TextBox ID="WF_SEL_FROMYMD" runat="server"></asp:TextBox>                   <!-- 摘要年月日 -->
    <asp:TextBox ID="WF_SEL_KIRO" runat="server"></asp:TextBox>                      <!-- キロ程 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                    <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
