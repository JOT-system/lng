<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0008WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0008WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>
    <!-- 運用部署 -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>

    <!-- 検索条件項目用 -->
    <asp:TextBox ID="WF_SRC_CONTRALNMODE" runat="server"></asp:TextBox>             <!-- 状況 -->
    <asp:TextBox ID="WF_SRC_TAISYOYM" runat="server"></asp:TextBox>                 <!-- 対象年月 -->
    <asp:TextBox ID="WF_SRC_JOTORGCODE" runat="server"></asp:TextBox>               <!-- JOT発組織コード -->
    <asp:TextBox ID="WF_SRC_JOTORGNAME" runat="server"></asp:TextBox>               <!-- JOT発組織名称 -->
    <asp:TextBox ID="WF_SRC_DEPTCODE" runat="server"></asp:TextBox>                 <!-- 担当部店コード -->
    <asp:TextBox ID="WF_SRC_DEPTNAME" runat="server"></asp:TextBox>                 <!-- 担当部店名称 -->
    <asp:TextBox ID="WF_SRC_INVDEPTCODE" runat="server"></asp:TextBox>              <!-- 請求書提出部店コード -->
    <asp:TextBox ID="WF_SRC_INVDEPTNAME" runat="server"></asp:TextBox>              <!-- 請求書提出部店名称 -->
    <asp:TextBox ID="WF_SRC_TORICODE" runat="server"></asp:TextBox>                 <!-- 請求先コード -->
    <asp:TextBox ID="WF_SRC_TORINAME" runat="server"></asp:TextBox>                 <!-- 請求先名称 -->
    <asp:TextBox ID="WF_SRC_DEPSTATIONCODE" runat="server"></asp:TextBox>           <!-- 発駅コード -->
    <asp:TextBox ID="WF_SRC_DEPSTATIONNAME" runat="server"></asp:TextBox>           <!-- 発駅名称 -->
    <asp:TextBox ID="WF_SRC_ARRSTATIONCODE" runat="server"></asp:TextBox>           <!-- 着駅コード -->
    <asp:TextBox ID="WF_SRC_ARRSTATIONNAME" runat="server"></asp:TextBox>           <!-- 着駅名称 -->
    <asp:TextBox ID="WF_SRC_DEPTRUSTEECODE" runat="server"></asp:TextBox>           <!-- 発受託人コード -->
    <asp:TextBox ID="WF_SRC_DEPTRUSTEENAME" runat="server"></asp:TextBox>           <!-- 発受託人名称 -->
    <asp:TextBox ID="WF_SRC_DATESTART" runat="server"></asp:TextBox>                <!-- 発送年月日From -->
    <asp:TextBox ID="WF_SRC_DATEEND" runat="server"></asp:TextBox>                  <!-- 発送年月日To -->
    <asp:TextBox ID="WF_SRC_CTNTYPE" runat="server"></asp:TextBox>                  <!-- コンテナ記号 -->
    <asp:TextBox ID="WF_SRC_CTNNO" runat="server"></asp:TextBox>                    <!-- コンテナ番号 -->
    <asp:TextBox ID="WF_SRC_STACKFREEKBN" runat="server"></asp:TextBox>             <!-- 状態 -->
    <asp:TextBox ID="WF_SRC_PAGENO" runat="server"></asp:TextBox>                   <!-- ページ番号 -->
    <asp:TextBox ID="WF_SRC_SHIPPERCD" runat="server"></asp:TextBox>                <!-- 荷主コード -->
    <asp:TextBox ID="WF_SRC_SHIPPERNM" runat="server"></asp:TextBox>                <!-- 荷主名 -->

    <!-- ■共通 -->
    <!-- 作成フラグ -->
    <asp:TextBox ID="WF_SEL_MODE" runat="server"></asp:TextBox>
    <!-- 検索押下フラグ -->
    <asp:TextBox ID="WF_SEL_SEARCH" runat="server"></asp:TextBox>
    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>
    <!-- 過去フラグ -->
    <asp:TextBox ID="WF_SEL_KAKOFLG" runat="server"></asp:TextBox>
    <!-- 登録フラグ -->
    <asp:TextBox ID="WF_SEL_INSERT" runat="server"></asp:TextBox>

    <!-- ■選択したデータ -->
    <asp:TextBox ID="WF_SEL_SHIPYMD"    runat ="server"></asp:TextBox>   <!-- 発送日 -->
    <asp:TextBox ID="WF_SEL_CTNTYPE"    runat ="server"></asp:TextBox>   <!-- コンテナ記号 -->
    <asp:TextBox ID="WF_SEL_CTNNO"      runat ="server"></asp:TextBox>   <!-- コンテナ番号 -->
    <asp:TextBox ID="WF_SEL_SAMEDAYCNT" runat ="server"></asp:TextBox>   <!-- 同日内回数 -->
    <asp:TextBox ID="WF_SEL_CTNLINENO"  runat ="server"></asp:TextBox>   <!-- 行番 -->

    <!-- ■権限ロール -->
    <asp:TextBox ID="WF_USER_UPD_ROLE" runat ="server"></asp:TextBox>   <!-- 更新権限 -->
    <asp:TextBox ID="WF_USER_REF_ROLE" runat ="server"></asp:TextBox>   <!-- 参照権限 -->

    <!-- MAPID退避(リース明細画面への遷移制御のため) -->
    <asp:TextBox ID="WF_SEL_MAPIDBACKUP" runat="server"></asp:TextBox>

    <!-- 受託人検索用 -->
    <asp:TextBox ID="WF_TrusteeSrc" runat="server"></asp:TextBox>     <!-- 受託人検索画面表示切替 -->
    <asp:TextBox ID="WF_ActiveCol" runat="server"></asp:TextBox>      <!-- 受託人検索表示_列 -->

</div>