<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0015WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0015WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                  <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                　 <!-- 選択行 -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_JRDEPBRANCHCD_S" runat="server"></asp:TextBox>          <!-- 発組織コード -->
    <asp:TextBox ID="WF_SEL_JRARRBRANCHCD_S" runat="server"></asp:TextBox>          <!-- 着組織コード -->
    <asp:TextBox ID="WF_SEL_DELFLG_S" runat="server"></asp:TextBox>                 <!-- 削除フラグ -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_JRDEPBRANCHCD" runat="server"></asp:TextBox>            <!-- 発組織コード -->
    <asp:TextBox ID="WF_SEL_JRARRBRANCHCD" runat="server"></asp:TextBox>            <!-- 着組織コード -->
    <asp:TextBox ID="WF_SEL_USEFEERATE" runat="server"></asp:TextBox>               <!-- 使用料率 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
