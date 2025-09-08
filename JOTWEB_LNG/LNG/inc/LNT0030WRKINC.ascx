<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0030WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0030WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 一覧用（LNM0019：サーチャージ定義マスタ受渡し用） -->
    <asp:TextBox ID="WF_SEL_TARGETYMD_L" runat="server"></asp:TextBox>              <!-- 対象年月日 -->
    <asp:TextBox ID="WF_SEL_TORI_L" runat="server"></asp:TextBox>                   <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_ORG_L" runat="server"></asp:TextBox>                    <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_CHKDELDATAFLG_L" runat="server"></asp:TextBox>          <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>                <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_ORGCODE" runat="server"></asp:TextBox>                 <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_ORGNAME" runat="server"></asp:TextBox>                 <!-- 部門名称 -->
    <asp:TextBox ID="WF_SEL_KASANORGCODE" runat="server"></asp:TextBox>            <!-- 加算先部門コード -->
    <asp:TextBox ID="WF_SEL_KASANORGNAME" runat="server"></asp:TextBox>            <!-- 加算先部門名称 -->
    <asp:TextBox ID="WF_SEL_SURCHARGEPATTERNCODE" runat="server"></asp:TextBox>    <!-- サーチャージパターンコード -->
    <asp:TextBox ID="WF_SEL_SURCHARGEPATTERNNAME" runat="server"></asp:TextBox>    <!-- サーチャージパターン名 -->
    <asp:TextBox ID="WF_SEL_BILLINGCYCLE" runat="server"></asp:TextBox>            <!-- 請求サイクル -->
    <asp:TextBox ID="WF_SEL_BILLINGCYCLENAME" runat="server"></asp:TextBox>        <!-- 請求サイクル名 -->
    <asp:TextBox ID="WF_SEL_CALCMETHOD" runat="server"></asp:TextBox>              <!-- 距離算定方式 -->
    <asp:TextBox ID="WF_SEL_CALCMETHODNAME" runat="server"></asp:TextBox>          <!-- 距離算定方式名 -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>                   <!-- 有効開始日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>                  <!-- 有効終了日 -->
    <asp:TextBox ID="WF_SEL_DIESELPRICESITEID" runat="server"></asp:TextBox>       <!-- 実勢軽油価格参照先ID -->
    <asp:TextBox ID="WF_SEL_DIESELPRICESITENAME" runat="server"></asp:TextBox>     <!-- 実勢軽油価格参照先名 -->
    <asp:TextBox ID="WF_SEL_DIESELPRICESITEBRANCH" runat="server"></asp:TextBox>   <!-- 実勢軽油価格参照先ID枝番 -->
    <asp:TextBox ID="WF_SEL_DIESELPRICESITEKBNNAME" runat="server"></asp:TextBox>  <!-- 実勢軽油価格参照先区分名 -->
    <asp:TextBox ID="WF_SEL_DISPLAYNAME" runat="server"></asp:TextBox>             <!-- 実勢軽油価格参照先画面表示名 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                  <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
