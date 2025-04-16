<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0002WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0002WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <asp:TextBox ID="WF_HIST" runat="server"></asp:TextBox>                         <!-- 履歴表示切替 -->


    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                  <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_TARGETYM" runat="server"></asp:TextBox>                <!-- 対象年月 -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>                <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_ORGCODE" runat="server"></asp:TextBox>                 <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_ORGNAME" runat="server"></asp:TextBox>                 <!-- 部門名称 -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
