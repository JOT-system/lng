<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0021WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0021WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->

    <!-- ■共通 -->
    <!-- 更新データ(退避用) -->
     <asp:TextBox ID="WF_SEL_StockListTBL" runat="server"></asp:TextBox>

    <!-- MAPID退避(収入管理明細画面への遷移制御のため) -->
    <asp:TextBox ID="WF_SEL_MAPIDBACKUP" runat="server"></asp:TextBox>

    <!-- メモ画面表示制御 -->
    <asp:TextBox ID="WF_Memo" runat="server"></asp:TextBox>                <!-- メモ履歴表示切替 -->
    <asp:TextBox ID="WF_RemarkSrc" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_KekkjSrc" runat="server"></asp:TextBox>

</div>