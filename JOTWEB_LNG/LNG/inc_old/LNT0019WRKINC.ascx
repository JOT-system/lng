<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0019WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0019WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->

    <!-- 検索条件項目用 -->
    <asp:TextBox ID="WF_SRC_KEIJOYM" runat="server"></asp:TextBox>                  <!-- 支払年月 -->
    <asp:TextBox ID="WF_SRC_TANTO_STORE" runat="server"></asp:TextBox>              <!-- 担当部店 -->
    <asp:TextBox ID="WF_SRC_PAYMENTCD" runat="server"></asp:TextBox>                <!-- 支払先コード -->

    <!-- 明細画面遷移用 -->
    <asp:TextBox ID="WF_SEL_DEPOSITYMD" runat="server"></asp:TextBox>               <!-- 支払予定日 -->
    <asp:TextBox ID="WF_SEL_KEIJOYM" runat="server"></asp:TextBox>                  <!-- 支払年月 -->
    <asp:TextBox ID="WF_SEL_TANTO_STORE" runat="server"></asp:TextBox>              <!-- 担当部店 -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                 <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_DEPOSITMONTHKBN" runat="server"></asp:TextBox>          <!-- 計上月区分 -->
    <asp:TextBox ID="WF_SEL_CLOSINGDAY" runat="server"></asp:TextBox>               <!-- 計上締日 -->
    <asp:TextBox ID="WF_SEL_PAYMENTNUMBER" runat="server"></asp:TextBox>            <!-- 支払番号 -->
    <asp:TextBox ID="WF_SEL_PAYMENTLINK" runat="server"></asp:TextBox>              <!-- 支払連携状態 -->
    <asp:TextBox ID="WF_SEL_REQUESTSTATUS" runat="server"></asp:TextBox>            <!-- 支払申請状態 -->
    <asp:TextBox ID="WF_SEL_RQSTAFF" runat="server"></asp:TextBox>                  <!-- 担当ユーザーID -->
    <asp:TextBox ID="WF_SEL_RQACKNOWLEDGER" runat="server"></asp:TextBox>           <!-- 確認ユーザーID -->

    <!-- ■レフトボックス用 -->
    <!-- 支払先 -->
    <asp:TextBox ID="WF_DEPTRUSTEECODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_DEPTRUSTEENAME" runat="server"></asp:TextBox>
    <!-- 支払先サブ -->
    <asp:TextBox ID="WK_DEPPICKDELTRADERCODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WK_DEPPICKDELTRADERNAME" runat="server"></asp:TextBox>

    <!-- ■共通 -->
    <!-- 検索押下フラグ -->
    <asp:TextBox ID="WF_SEL_SEARCH" runat="server"></asp:TextBox>
    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>

    <!-- MAPID退避(収入管理明細画面への遷移制御のため) -->
    <asp:TextBox ID="WF_SEL_MAPIDBACKUP" runat="server"></asp:TextBox>

    <!-- メモ画面表示制御 -->
    <asp:TextBox ID="WF_Memo" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_KAGENGAKU" runat="server"></asp:TextBox>

</div>