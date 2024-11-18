<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNS0020WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNS0020WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                  <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                　 <!-- 選択行 -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_ORGCODE_S" runat="server"></asp:TextBox>                 <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_STATION_S" runat="server"></asp:TextBox>                 <!-- 駅コード -->
    <asp:TextBox ID="WF_SEL_DELFLG_S" runat="server"></asp:TextBox>                  <!-- 削除フラグ -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_ORGCODE" runat="server"></asp:TextBox>                  <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_STATION" runat="server"></asp:TextBox>                  <!-- 駅コード -->
    <asp:TextBox ID="WF_SEL_NAME" runat="server"></asp:TextBox>                     <!-- 駅名称 -->
    <asp:TextBox ID="WF_SEL_NAMES" runat="server"></asp:TextBox>                    <!-- 駅名称（短） -->
    <asp:TextBox ID="WF_SEL_NAMEKANA" runat="server"></asp:TextBox>                 <!-- 駅カナ名称 -->
    <asp:TextBox ID="WF_SEL_NAMEKANAS" runat="server"></asp:TextBox>                <!-- 駅カナ名称（短） -->
    <asp:TextBox ID="WF_SEL_STATIONSELECTFLAG" runat="server"></asp:TextBox>        <!-- 駅選択対象フラグ -->
    <asp:TextBox ID="WF_SEL_GOVERNORGCODE" runat="server"></asp:TextBox>            <!-- 管轄組織コード -->
    <asp:TextBox ID="WF_SEL_CLASS01" runat="server"></asp:TextBox>                  <!-- 分類コード01 -->
    <asp:TextBox ID="WF_SEL_CLASS02" runat="server"></asp:TextBox>                  <!-- 分類コード02 -->
    <asp:TextBox ID="WF_SEL_CLASS03" runat="server"></asp:TextBox>                  <!-- 分類コード03 -->
    <asp:TextBox ID="WF_SEL_ISLANDFLG" runat="server"></asp:TextBox>                <!-- 離島フラグ -->
    <asp:TextBox ID="WF_SEL_TAIOU1" runat="server"></asp:TextBox>                   <!-- 対応C1 -->
    <asp:TextBox ID="WF_SEL_TAIOU2" runat="server"></asp:TextBox>                   <!-- 対応C2 -->
    <asp:TextBox ID="WF_SEL_TAIOU3" runat="server"></asp:TextBox>                   <!-- 対応C3 -->
    <asp:TextBox ID="WF_SEL_TAIOU4" runat="server"></asp:TextBox>                   <!-- 対応C4 -->
    <asp:TextBox ID="WF_SEL_TAIOU5" runat="server"></asp:TextBox>                   <!-- 対応C5 -->
    <asp:TextBox ID="WF_SEL_TAIOU6" runat="server"></asp:TextBox>                   <!-- 対応C6 -->
    <asp:TextBox ID="WF_SEL_TAIOU7" runat="server"></asp:TextBox>                   <!-- 対応C7 -->
    <asp:TextBox ID="WF_SEL_TAIOU8" runat="server"></asp:TextBox>                   <!-- 対応C8 -->
    <asp:TextBox ID="WF_SEL_TAIOU9" runat="server"></asp:TextBox>                   <!-- 対応C9 -->
    <asp:TextBox ID="WF_SEL_TAIOU10" runat="server"></asp:TextBox>                  <!-- 対応C10 -->
    <asp:TextBox ID="WF_SEL_BEFOREORGCODE" runat="server"></asp:TextBox>            <!-- 変換前組織コード -->
    <asp:TextBox ID="WF_SEL_REPRESENFLG" runat="server"></asp:TextBox>              <!-- 予算表示フラグ -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                    <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
