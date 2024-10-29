<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0023WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0023WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                  <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                　 <!-- 選択行 -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_TORICODE_S" runat="server"></asp:TextBox>                <!-- 支払先コード -->
    <asp:TextBox ID="WF_SEL_CLIENTCODE_S" runat="server"></asp:TextBox>              <!-- 顧客コード -->
    <asp:TextBox ID="WF_SEL_DELFLG_S" runat="server"></asp:TextBox>                  <!-- 削除フラグ -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                  <!-- 支払先コード -->
    <asp:TextBox ID="WF_SEL_CLIENTCODE" runat="server"></asp:TextBox>                <!-- 顧客コード -->
    <asp:TextBox ID="WF_SEL_INVOICENUMBER" runat="server"></asp:TextBox>             <!-- インボイス登録番号 -->
    <asp:TextBox ID="WF_SEL_CLIENTNAME" runat="server"></asp:TextBox>                <!-- 顧客名 -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>                  <!-- 会社名 -->
    <asp:TextBox ID="WF_SEL_TORIDIVNAME" runat="server"></asp:TextBox>               <!-- 部門名 -->
    <asp:TextBox ID="WF_SEL_PAYBANKCODE" runat="server"></asp:TextBox>               <!-- 振込先銀行コード -->
    <asp:TextBox ID="WF_SEL_PAYBANKNAME" runat="server"></asp:TextBox>               <!-- 振込先銀行名 -->
    <asp:TextBox ID="WF_SEL_PAYBANKNAMEKANA" runat="server"></asp:TextBox>           <!-- 振込先銀行名カナ -->
    <asp:TextBox ID="WF_SEL_PAYBANKBRANCHCODE" runat="server"></asp:TextBox>         <!-- 振込先支店コード -->
    <asp:TextBox ID="WF_SEL_PAYBANKBRANCHNAME" runat="server"></asp:TextBox>         <!-- 振込先支店名 -->
    <asp:TextBox ID="WF_SEL_PAYBANKBRANCHNAMEKANA" runat="server"></asp:TextBox>     <!-- 振込先支店名カナ -->
    <asp:TextBox ID="WF_SEL_PAYACCOUNTTYPENAME" runat="server"></asp:TextBox>        <!-- 預金種別 -->
    <asp:TextBox ID="WF_SEL_PAYACCOUNTTYPE" runat="server"></asp:TextBox>            <!-- 預金種別コード -->
    <asp:TextBox ID="WF_SEL_PAYACCOUNT" runat="server"></asp:TextBox>                <!-- 口座番号 -->
    <asp:TextBox ID="WF_SEL_PAYACCOUNTNAME" runat="server"></asp:TextBox>            <!-- 口座名義 -->
    <asp:TextBox ID="WF_SEL_PAYORBANKCODE" runat="server"></asp:TextBox>             <!-- 支払元銀行コード -->
    <asp:TextBox ID="WF_SEL_PAYTAXCALCUNIT" runat="server"></asp:TextBox>            <!-- 消費税計算処理区分 -->
    <asp:TextBox ID="WF_SEL_LINKSTATUS" runat="server"></asp:TextBox>                <!-- 連携状態区分 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                    <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
