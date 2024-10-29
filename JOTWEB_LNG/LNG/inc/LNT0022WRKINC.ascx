<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0022WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0022WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>

    <!-- 検索フィルタ -->
    <!-- コンテナ記号 -->
    <asp:TextBox ID="WF_SEL_CTNTYPE" runat="server"></asp:TextBox>
    <!-- コンテナ番号 -->
    <asp:TextBox ID="WF_SEL_CTNNO" runat="server"></asp:TextBox>
    <!-- フリーワード検索用 -->
    <asp:TextBox ID="WF_SEL_FREE" runat="server"></asp:TextBox>

    <!-- 更新データ退避用 -->
    <!-- コンテナマスタ -->
    <asp:TextBox ID="WF_SEL_INP_CONM_TBL" runat="server"></asp:TextBox>
    <!-- コンテナ検査日Ｔ -->
    <asp:TextBox ID="WF_SEL_INP_CONINS_TBL" runat="server"></asp:TextBox>
    <!-- 検査登録ダイアログ一時保存用 -->
    <asp:TextBox ID="WF_SEL_INP_DIALOG_TBL" runat="server"></asp:TextBox>
</div>
