<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0005WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0005WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- ■リース登録用 画面 登録・更新用 -->
    <!-- 請求先 -->
    <asp:TextBox ID="WF_UPD_INVOICE_CODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_UPD_INVOICE_NAME" runat="server"></asp:TextBox>
    <!-- 請求先部門 -->
    <asp:TextBox ID="WF_UPD_INVOICEDEP_CODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_UPD_INVOICEDEP_NAME" runat="server"></asp:TextBox>
    <!-- 請求書出力先 -->
    <asp:TextBox ID="WF_UPD_INVOICEOUTPUT_CODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_UPD_INVOICEOUTPUT_NAME" runat="server"></asp:TextBox>
    <!-- 計上先 -->
    <asp:TextBox ID="WF_UPD_KEIJO_CODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_UPD_KEIJO_NAME" runat="server"></asp:TextBox>
    <!-- 日割端数処理１ -->
    <asp:TextBox ID="WF_UPD_DAYLYHASUU1_CODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_UPD_DAYLYHASUU1_NAME" runat="server"></asp:TextBox>
    <!-- 日割端数処理２ -->
    <asp:TextBox ID="WF_UPD_DAYLYHASUU2_CODE" runat="server"></asp:TextBox>
    <asp:TextBox ID="WF_UPD_DAYLYHASUU2_NAME" runat="server"></asp:TextBox>

    <!-- ■リース一覧用 -->
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>
    <!-- 運用部署 -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>
    <!-- 計上年月 -->
    <asp:TextBox ID="WF_SEL_KEIJOYM" runat="server"></asp:TextBox>
    <!-- 請求先コード -->
    <asp:TextBox ID="WF_SEL_INVOICECODE" runat="server"></asp:TextBox>
    <!-- 請求先名称 -->
    <asp:TextBox ID="WF_SEL_INVOICENAME" runat="server"></asp:TextBox>
    <!-- 請求書出力先 -->
    <asp:TextBox ID="WF_SEL_INVOICEOUTPUT" runat="server"></asp:TextBox>
    <!-- 計上先 -->
    <asp:TextBox ID="WF_SEL_KEIJO" runat="server"></asp:TextBox>
    <!-- 契約形態 -->
    <asp:TextBox ID="WF_SEL_CONTRALNMODELIST" runat="server"></asp:TextBox>
    <!-- コンテナ記号 -->
    <asp:TextBox ID="WF_SEL_CTNTYPE" runat="server"></asp:TextBox>
    <!-- コンテナ番号 -->
    <asp:TextBox ID="WF_SEL_CTNNO" runat="server"></asp:TextBox>
    <!-- 状況 無効を含む -->
    <asp:TextBox ID="WF_SEL_CHKINVALIDONFLG" runat="server"></asp:TextBox>
    <!-- 状況 終了を含む-->
    <asp:TextBox ID="WF_SEL_CHKENDONFLG" runat="server"></asp:TextBox>

    <!-- リース登録番号 -->
    <asp:TextBox ID="WF_SELROW_LEASENO" runat="server"></asp:TextBox>
    <!-- 取引先コード -->
    <asp:TextBox ID="WF_SELROW_TORICODE" runat="server"></asp:TextBox>

    <!-- ■共通 -->
    <!-- 作成フラグ -->
    <asp:TextBox ID="WF_SEL_MODE" runat="server"></asp:TextBox>
    <!-- 検索押下フラグ -->
    <asp:TextBox ID="WF_SEL_SEARCH" runat="server"></asp:TextBox>
    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>
    <!-- 新規ボタン押下フラグ -->
    <asp:TextBox ID="WF_NEWBTN_FLG" runat="server"></asp:TextBox>
    <!-- MAPID退避(リース明細画面への遷移制御のため) -->
    <asp:TextBox ID="WF_SEL_MAPIDBACKUP" runat="server"></asp:TextBox>
    <!-- 決済条件検索画面表示切替 -->
    <asp:TextBox ID="WF_KekkjSrc" runat="server"></asp:TextBox>
    <!-- リース明細画面表示切替 -->
    <asp:TextBox ID="WF_LeaseDataList" runat="server"></asp:TextBox>
    <!-- コンテナ検索画面表示切替 -->
    <asp:TextBox ID="WF_ReconmList" runat="server"></asp:TextBox>
    <!-- ファイナンス情報画面表示切替 -->
    <asp:TextBox ID="WF_LeaseFinal" runat="server"></asp:TextBox>
    <!-- 請求情報画面表示切替 -->
    <asp:TextBox ID="WF_InvoiceInfo" runat="server"></asp:TextBox>
    <!-- 請求先検索画面表示切替 -->
    <asp:TextBox ID="WF_InvoiceSrc" runat="server"></asp:TextBox>
    <!-- コンテナ一覧検索 -->
    <asp:TextBox ID="WF_CtnListFlg" runat="server"></asp:TextBox>
</div>