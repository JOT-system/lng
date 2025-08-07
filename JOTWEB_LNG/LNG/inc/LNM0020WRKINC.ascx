<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0020WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0020WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 一覧用 -->
    <asp:TextBox ID="WF_SEL_DEISELPRICESITEID_L" runat="server"></asp:TextBox>      <!-- 実勢軽油価格参照先ID -->
    <asp:TextBox ID="WF_SEL_DEISELPRICESITEBRANCH_L" runat="server"></asp:TextBox>  <!-- 実勢軽油価格参照先ID枝番 -->
    <asp:TextBox ID="WF_SEL_DEISELPRICESITENAME_L" runat="server"></asp:TextBox>    <!-- 実勢軽油価格参照先名 -->
    <asp:TextBox ID="WF_SEL_DEISELPRICESITEKBNNAME_L" runat="server"></asp:TextBox> <!-- 実勢軽油価格参照先区分名 -->
    <asp:TextBox ID="WF_SEL_DISPLAYNAME_L" runat="server"></asp:TextBox>            <!-- 画面表示名称 -->
    <asp:TextBox ID="WF_SEL_DEISELPRICESITEURL_L" runat="server"></asp:TextBox>     <!-- 実勢軽油価格参照先URL -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_DEISELPRICESITEID" runat="server"></asp:TextBox>        <!-- 実勢軽油価格参照先ID -->
    <asp:TextBox ID="WF_SEL_DEISELPRICESITEBRANCH" runat="server"></asp:TextBox>    <!-- 実勢軽油価格参照先ID枝番 -->
    <asp:TextBox ID="WF_SEL_DEISELPRICESITENAME" runat="server"></asp:TextBox>      <!-- 実勢軽油価格参照先名 -->
    <asp:TextBox ID="WF_SEL_DEISELPRICESITEKBNNAME" runat="server"></asp:TextBox>   <!-- 実勢軽油価格参照先区分名 -->
    <asp:TextBox ID="WF_SEL_DISPLAYNAME" runat="server"></asp:TextBox>              <!-- 画面表示名称 -->
    <asp:TextBox ID="WF_SEL_DEISELPRICESITEURL" runat="server"></asp:TextBox>       <!-- 実勢軽油価格参照先URL -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
