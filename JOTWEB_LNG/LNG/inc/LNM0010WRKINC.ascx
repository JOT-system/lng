<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0010WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0010WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_CONTROLTABLE" runat="server"></asp:TextBox>             <!-- 操作テーブル -->
    <asp:TextBox ID="WF_SEL_CONTROLTABLEHIST" runat="server"></asp:TextBox>         <!-- 操作テーブル(履歴) -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>              <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_RECOID" runat="server"></asp:TextBox>              <!-- レコードID -->
    <asp:TextBox ID="WF_SEL_RECONAME" runat="server"></asp:TextBox>              <!-- レコード名 -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>              <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>              <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_ORGCODE" runat="server"></asp:TextBox>              <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_ORGNAME" runat="server"></asp:TextBox>              <!-- 部門名称 -->
    <asp:TextBox ID="WF_SEL_KASANORGCODE" runat="server"></asp:TextBox>              <!-- 加算先部門コード -->
    <asp:TextBox ID="WF_SEL_KASANORGNAME" runat="server"></asp:TextBox>              <!-- 加算先部門名称 -->
    <asp:TextBox ID="WF_SEL_TODOKECODE" runat="server"></asp:TextBox>              <!-- 届先コード -->
    <asp:TextBox ID="WF_SEL_TODOKENAME" runat="server"></asp:TextBox>              <!-- 届先名称 -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>              <!-- 有効開始日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>              <!-- 有効終了日 -->
    <asp:TextBox ID="WF_SEL_KINGAKU" runat="server"></asp:TextBox>              <!-- 金額 -->
    <asp:TextBox ID="WF_SEL_SYABAN" runat="server"></asp:TextBox>              <!-- 車番 -->
    <asp:TextBox ID="WF_SEL_TAISHOYM" runat="server"></asp:TextBox>              <!-- 対象年月 -->
    <asp:TextBox ID="WF_SEL_ITEMID" runat="server"></asp:TextBox>              <!-- 大項目 -->
    <asp:TextBox ID="WF_SEL_ITEMNAME" runat="server"></asp:TextBox>              <!-- 項目名 -->
    <asp:TextBox ID="WF_SEL_SYABARA" runat="server"></asp:TextBox>              <!-- 車腹 -->
    <asp:TextBox ID="WF_SEL_KOTEIHI" runat="server"></asp:TextBox>              <!-- 固定費 -->
    <asp:TextBox ID="WF_SEL_TANKA" runat="server"></asp:TextBox>              <!-- 単価 -->
    <asp:TextBox ID="WF_SEL_KYORI" runat="server"></asp:TextBox>              <!-- 走行距離 -->
    <asp:TextBox ID="WF_SEL_KEIYU" runat="server"></asp:TextBox>              <!-- 実勢軽油価格 -->
    <asp:TextBox ID="WF_SEL_KIZYUN" runat="server"></asp:TextBox>              <!-- 基準価格 -->
    <asp:TextBox ID="WF_SEL_TANKASA" runat="server"></asp:TextBox>              <!-- 単価差 -->
    <asp:TextBox ID="WF_SEL_KAISU" runat="server"></asp:TextBox>              <!-- 輸送回数 -->
    <asp:TextBox ID="WF_SEL_COUNT" runat="server"></asp:TextBox>              <!-- 回数 -->
    <asp:TextBox ID="WF_SEL_FEE" runat="server"></asp:TextBox>              <!-- 料金 -->
    <asp:TextBox ID="WF_SEL_BIKOU" runat="server"></asp:TextBox>              <!-- 備考 -->
    <asp:TextBox ID="WF_SEL_USAGECHARGE" runat="server"></asp:TextBox>              <!-- 燃料使用量 -->
    <asp:TextBox ID="WF_SEL_SURCHARGE" runat="server"></asp:TextBox>              <!-- サーチャージ -->
    <asp:TextBox ID="WF_SEL_BIKOU1" runat="server"></asp:TextBox>              <!-- 備考1 -->
    <asp:TextBox ID="WF_SEL_BIKOU2" runat="server"></asp:TextBox>              <!-- 備考2 -->
    <asp:TextBox ID="WF_SEL_BIKOU3" runat="server"></asp:TextBox>              <!-- 備考3 -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

    <!-- 北海道ガス特別料金マスタ用 -->
    <asp:TextBox ID="WF_SEL_LISTCOUNT" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->

</div>
