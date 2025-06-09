<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0006WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0006WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <!-- 一覧用 -->
    <asp:TextBox ID="WF_SEL_TARGETYMD_L" runat="server"></asp:TextBox>              <!-- 対象年月日 -->
    <asp:TextBox ID="WF_SEL_TORI_L" runat="server"></asp:TextBox>                   <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_ORG_L" runat="server"></asp:TextBox>                    <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_TODOKE_L" runat="server"></asp:TextBox>                 <!-- 実績届先コード -->
    <asp:TextBox ID="WF_SEL_DEPARTURE_L" runat="server"></asp:TextBox>              <!-- 実績出荷場所コード -->
    <asp:TextBox ID="WF_SEL_SHABAN_FROM_L" runat="server"></asp:TextBox>            <!-- 車番FROM -->
    <asp:TextBox ID="WF_SEL_SHABAN_TO_L" runat="server"></asp:TextBox>              <!-- 車番TO -->
    <asp:TextBox ID="WF_SEL_CHKDELDATAFLG_L" runat="server"></asp:TextBox>          <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_INPUTPAGE_L" runat="server"></asp:TextBox>              <!-- 入力ページ -->
    <asp:TextBox ID="WF_SEL_NOWPAGECNT_L" runat="server"></asp:TextBox>             <!-- 表示中ページ -->

    <!-- 登録・更新用 -->
      <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>              <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>                <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_ORGCODE" runat="server"></asp:TextBox>                 <!-- 部門コード -->
    <asp:TextBox ID="WF_SEL_ORGNAME" runat="server"></asp:TextBox>                 <!-- 部門名称 -->
    <asp:TextBox ID="WF_SEL_KASANORGCODE" runat="server"></asp:TextBox>            <!-- 加算先部門コード -->
    <asp:TextBox ID="WF_SEL_KASANORGNAME" runat="server"></asp:TextBox>            <!-- 加算先部門名称 -->
    <asp:TextBox ID="WF_SEL_AVOCADOSHUKABASHO" runat="server"></asp:TextBox>       <!-- 実績出荷場所コード -->
    <asp:TextBox ID="WF_SEL_AVOCADOSHUKANAME" runat="server"></asp:TextBox>        <!-- 実績出荷場所名称 -->
    <asp:TextBox ID="WF_SEL_SHUKABASHO" runat="server"></asp:TextBox>              <!-- 変換後出荷場所コード -->
    <asp:TextBox ID="WF_SEL_SHUKANAME" runat="server"></asp:TextBox>               <!-- 変換後出荷場所名称 -->
    <asp:TextBox ID="WF_SEL_AVOCADOTODOKECODE" runat="server"></asp:TextBox>       <!-- 実績届先コード -->
    <asp:TextBox ID="WF_SEL_AVOCADOTODOKENAME" runat="server"></asp:TextBox>       <!-- 実績届先名称 -->
    <asp:TextBox ID="WF_SEL_TODOKECODE" runat="server"></asp:TextBox>              <!-- 変換後届先コード -->
    <asp:TextBox ID="WF_SEL_TODOKENAME" runat="server"></asp:TextBox>              <!-- 変換後届先名称 -->
    <asp:TextBox ID="WF_SEL_TANKNUMBER" runat="server"></asp:TextBox>              <!-- 陸事番号 -->
    <asp:TextBox ID="WF_SEL_SHABAN" runat="server"></asp:TextBox>                  <!-- 車番 -->
    <asp:TextBox ID="WF_SEL_STYMD" runat="server"></asp:TextBox>                   <!-- 有効開始日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>                  <!-- 有効終了日 -->
    <asp:TextBox ID="WF_SEL_BRANCHCODE" runat="server"></asp:TextBox>              <!-- 枝番 -->
    <asp:TextBox ID="WF_SEL_TANKAKBN" runat="server"></asp:TextBox>                <!-- 単価区分 -->
    <asp:TextBox ID="WF_SEL_MEMO" runat="server"></asp:TextBox>                    <!-- 単価用途 -->
    <asp:TextBox ID="WF_SEL_TANKA" runat="server"></asp:TextBox>                   <!-- 単価 -->
    <asp:TextBox ID="WF_SEL_CALCKBN" runat="server"></asp:TextBox>                 <!-- 計算区分 -->
    <asp:TextBox ID="WF_SEL_ROUNDTRIP" runat="server"></asp:TextBox>               <!-- 往復距離 -->
    <asp:TextBox ID="WF_SEL_TOLLFEE" runat="server"></asp:TextBox>                 <!-- 通行料 -->
    <asp:TextBox ID="WF_SEL_SYAGATA" runat="server"></asp:TextBox>                 <!-- 車型 -->
    <asp:TextBox ID="WF_SEL_SYAGATANAME" runat="server"></asp:TextBox>             <!-- 車型名 -->
    <asp:TextBox ID="WF_SEL_SYABARA" runat="server"></asp:TextBox>                 <!-- 車腹 -->
    <asp:TextBox ID="WF_SEL_BIKOU1" runat="server"></asp:TextBox>                  <!-- 備考1 -->
    <asp:TextBox ID="WF_SEL_BIKOU2" runat="server"></asp:TextBox>                  <!-- 備考2 -->
    <asp:TextBox ID="WF_SEL_BIKOU3" runat="server"></asp:TextBox>                  <!-- 備考3 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                  <!-- 削除フラグ -->

    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->

</div>
