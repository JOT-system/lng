<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0007WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0007WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->

    <!-- 検索条件項目用 -->
    <asp:TextBox ID="WF_SRC_KEIJOYM" runat="server"></asp:TextBox>                  <!-- 計上年月 -->
    <asp:TextBox ID="WF_SRC_TANTO_STORE" runat="server"></asp:TextBox>              <!-- 担当部店 -->
    <asp:TextBox ID="WF_SRC_INVOICECAMPNM" runat="server"></asp:TextBox>            <!-- 請求先名称 -->
    <asp:TextBox ID="WF_SRC_INVOICETYPE" runat="server"></asp:TextBox>              <!-- 請求書種類 -->
    <asp:TextBox ID="WF_SRC_STATUS" runat="server"></asp:TextBox>                   <!-- 申請状態 -->
    <asp:TextBox ID="WF_SRC_STATUS1" runat="server"></asp:TextBox>                  <!-- 申請状態 -->
    <asp:TextBox ID="WF_SRC_STATUS2" runat="server"></asp:TextBox>                  <!-- 申請状態 -->
    <asp:TextBox ID="WF_SRC_STATUS3" runat="server"></asp:TextBox>                  <!-- 申請状態 -->
    <asp:TextBox ID="WF_SRC_STATUS4" runat="server"></asp:TextBox>                  <!-- 申請状態 -->

    <!-- 明細画面遷移用 -->
    <asp:TextBox ID="WF_SEL_DEPOSITYMD" runat="server"></asp:TextBox>               <!-- 入金予定日 -->
    <asp:TextBox ID="WF_SEL_DEPOSITYMD_BEF" runat="server"></asp:TextBox>           <!-- 入金予定日(変更前)  -->
    <asp:TextBox ID="WF_SEL_KEIJOYM" runat="server"></asp:TextBox>                  <!-- 計上年月 -->
    <asp:TextBox ID="WF_SEL_TANTO_STORE" runat="server"></asp:TextBox>              <!-- 担当部店 -->
    <asp:TextBox ID="WF_SEL_TANTO_STORE_NAME" runat="server"></asp:TextBox>         <!-- 担当部店名 -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                 <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORICODE_BEF" runat="server"></asp:TextBox>             <!-- 取引先コード(変更前) -->
    <asp:TextBox ID="WF_SEL_CLOSESTATUS" runat="server"></asp:TextBox>              <!-- 締め状態 -->
    <asp:TextBox ID="WF_SEL_RQACKNOWLEDGER" runat="server"></asp:TextBox>           <!-- 確認者 -->
    <asp:TextBox ID="WF_SEL_INSERT" runat="server"></asp:TextBox>                   <!-- 新規登録処理切り替え用 -->
    <asp:TextBox ID="WF_SEL_INS_FLG" runat="server"></asp:TextBox>                  <!-- 新規登録データ選択 -->
    <asp:TextBox ID="WF_SEL_TAXCALCULATION" runat="server"></asp:TextBox>           <!-- 税計算区分 -->
    <asp:TextBox ID="WF_SEL_NODTL_FLG" runat="server"></asp:TextBox>                <!-- 明細無しフラグ -->
    <asp:TextBox ID="WF_SEL_CLOSE_STUTAS" runat="server"></asp:TextBox>             <!-- 経理連携状態 -->

    <!-- ■表示ページ保管用 -->
    <asp:TextBox ID="WF_SRC_PAGE" runat="server"></asp:TextBox>                     <!-- ページ数 -->

    <!-- ■権限ロール -->
    <asp:TextBox ID="WF_USER_UPD_ROLE" runat ="server"></asp:TextBox>               <!-- 更新権限 -->
    <asp:TextBox ID="WF_USER_REF_ROLE" runat ="server"></asp:TextBox>               <!-- 参照権限 -->

    <!-- ■レフトボックス用 -->
    <!-- 請求先 -->
    <asp:TextBox ID="WF_DEPTRUSTEENAME" runat="server"></asp:TextBox>
    <!-- 請求先サブ -->
    <asp:TextBox ID="WK_DEPPICKDELTRADERCODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WK_DEPPICKDELTRADERNAME" runat="server"></asp:TextBox>

    <!-- ■共通 -->
    <!-- 検索押下フラグ -->
    <asp:TextBox ID="WF_SEL_SEARCH" runat="server"></asp:TextBox>
    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>
    <!-- 更新データ(退避用) -->
     <asp:TextBox ID="WF_SEL_DetailListTBL" runat="server"></asp:TextBox>
     <asp:TextBox ID="WF_SEL_RentalListTBL" runat="server"></asp:TextBox>
     <asp:TextBox ID="WF_SEL_LeaseListTBL" runat="server"></asp:TextBox>
     <asp:TextBox ID="WF_SEL_ContainerListTBL" runat="server"></asp:TextBox>
     <asp:TextBox ID="WF_SEL_HistoryListTBL" runat="server"></asp:TextBox>

    <!-- MAPID退避(収入管理明細画面への遷移制御のため) -->
    <asp:TextBox ID="WF_SEL_MAPIDBACKUP" runat="server"></asp:TextBox>

    <!-- popup用 -->
    <asp:TextBox ID="WF_RENTAL_POPUP" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_LEASE_POPUP" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_KekkjSrc" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_TrusteeSrc" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_CommentSrc" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_MessageSrc" runat="server"></asp:TextBox>
    
    <!-- エラーメッセージ保管用 -->
    <asp:TextBox ID="WF_ERR_Message" runat="server"></asp:TextBox>              <!-- エラーメッセージ -->
</div>