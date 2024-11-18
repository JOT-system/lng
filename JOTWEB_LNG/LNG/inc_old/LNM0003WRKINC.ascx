<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0003WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0003WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                　<!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_DELDATAFLG" runat="server"></asp:TextBox>               <!-- 論理削除フラグ -->
    <asp:TextBox ID="WF_SEL_DEPSTATION" runat="server"></asp:TextBox>               <!-- 発駅コード -->
    <asp:TextBox ID="WF_SEL_DEPSTATION2" runat="server"></asp:TextBox>              <!-- 発駅コード2-->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEECD" runat="server"></asp:TextBox>             <!-- 発受託人コード -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEECD2" runat="server"></asp:TextBox>            <!-- 発受託人コード2 -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEESUBCD" runat="server"></asp:TextBox>          <!-- 発受託人サブコード -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEESUBCD2" runat="server"></asp:TextBox>         <!-- 発受託人サブコード2 -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEENM" runat="server"></asp:TextBox>             <!-- 発受託人名称 -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEESUBNM" runat="server"></asp:TextBox>          <!-- 発受託人サブ名称 -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEESUBKANA" runat="server"></asp:TextBox>        <!-- 発受託人名称（カナ） -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                 <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_ELIGIBLEINVOICENUMBER" runat="server"></asp:TextBox>    <!-- 適格請求書登録番号 -->
    <asp:TextBox ID="WF_SEL_INVKEIJYOBRANCHCD" runat="server"></asp:TextBox>        <!-- 請求項目 計上店コード -->
    <asp:TextBox ID="WF_SEL_INVCYCL" runat="server"></asp:TextBox>                  <!-- 請求項目 請求サイクル -->
    <asp:TextBox ID="WF_SEL_INVFILINGDEPT" runat="server"></asp:TextBox>            <!-- 請求項目 請求書提出部店 -->
    <asp:TextBox ID="WF_SEL_INVKESAIKBN" runat="server"></asp:TextBox>              <!-- 請求項目 請求書決済区分 -->
    <asp:TextBox ID="WF_SEL_INVSUBCD" runat="server"></asp:TextBox>                 <!-- 請求項目 請求書細分コード -->
    <asp:TextBox ID="WF_SEL_PAYKEIJYOBRANCHCD" runat="server"></asp:TextBox>        <!-- 支払項目 費用計上店コード -->
    <asp:TextBox ID="WF_SEL_PAYFILINGBRANCH" runat="server"></asp:TextBox>          <!-- 支払項目 支払書提出支店 -->
    <asp:TextBox ID="WF_SEL_TAXCALCUNIT" runat="server"></asp:TextBox>              <!-- 支払項目 消費税計算単位 -->
    <asp:TextBox ID="WF_SEL_PAYKESAIKBN" runat="server"></asp:TextBox>              <!-- 支払項目 決済区分 -->
    <asp:TextBox ID="WF_SEL_PAYBANKCD" runat="server"></asp:TextBox>                <!-- 支払項目 銀行コード -->
    <asp:TextBox ID="WF_SEL_PAYBANKBRANCHCD" runat="server"></asp:TextBox>          <!-- 支払項目 銀行支店コード -->
    <asp:TextBox ID="WF_SEL_PAYACCOUNTTYPE" runat="server"></asp:TextBox>           <!-- 支払項目 口座種別 -->
    <asp:TextBox ID="WF_SEL_PAYACCOUNTNO" runat="server"></asp:TextBox>             <!-- 支払項目 口座番号 -->
    <asp:TextBox ID="WF_SEL_PAYACCOUNTNM" runat="server"></asp:TextBox>             <!-- 支払項目 口座名義人 -->
    <asp:TextBox ID="WF_SEL_PAYTEKIYO" runat="server"></asp:TextBox>                <!-- 支払項目 支払摘要 -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                  <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>                 <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>               <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                   <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                  <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>                <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>               <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->
</div>
