<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0003WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0003WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">

    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>        <!-- 会社コード -->
    <asp:TextBox ID="WF_Check" runat="server"></asp:TextBox>               <!-- アコーディオン開閉 -->
    <asp:TextBox ID="WF_Memo" runat="server"></asp:TextBox>                <!-- メモ履歴表示切替 -->
    <asp:TextBox ID="WF_SEL_BIGCTN_LIST" runat="server"></asp:TextBox>     <!-- コンテナ種別一覧 -->
    <asp:TextBox ID="WF_SEL_ACCKBN_LIST" runat="server"></asp:TextBox>     <!-- 経理資産区分一覧 -->
    <asp:TextBox ID="WF_ADD12_LIST" runat="server"></asp:TextBox>          <!-- 扉形式一覧 -->
    <asp:TextBox ID="WF_ZYOGAI_LIST" runat="server"></asp:TextBox>         <!-- 除外リスト -->
    <asp:TextBox ID="WF_MODECHG_LIST" runat="server"></asp:TextBox>        <!-- 切替 -->

    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>

    <!-- 明細画面(タブ１)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB1TBL" runat="server"></asp:TextBox>
        <!-- 明細画面(タブ２)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB2TBL" runat="server"></asp:TextBox>
</div>
