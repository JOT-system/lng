<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0009WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0009WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                   <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_APPLICABLE_YYYY" runat="server"></asp:TextBox>            <!-- 対象年月(年) -->
    <asp:TextBox ID="WF_SEL_APPLICABLE_MM" runat="server"></asp:TextBox>              <!-- 対象年月(月) -->
    <asp:TextBox ID="WF_SEL_KEIJOORGNAME" runat="server"></asp:TextBox>               <!-- 計上先組織名称 -->
    <asp:TextBox ID="WF_SEL_KEIJOORGCD" runat="server"></asp:TextBox>                 <!-- 計上先組織コード -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                   <!-- 請求取引先コード -->
    <asp:TextBox ID="WF_SEL_CTNTYPE" runat="server"></asp:TextBox>                    <!-- コンテナ記号 -->
    <asp:TextBox ID="WF_SEL_CTNNO" runat="server"></asp:TextBox>                      <!-- コンテナ番号 -->
    <asp:TextBox ID="WF_SEL_CTNTYPEANDNO" runat="server"></asp:TextBox>               <!-- コンテナ記号&番号 -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                    <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_LEASENO" runat="server"></asp:TextBox>                    <!-- リース登録番号 -->
    <asp:TextBox ID="WF_SEL_LEASESTARTYMD" runat="server"></asp:TextBox>              <!-- 契約開始日 -->
    <asp:TextBox ID="WF_SEL_LEASEYEARS" runat="server"></asp:TextBox>                 <!-- 契約期間 -->
    <asp:TextBox ID="WF_SEL_LEASEENDYMD" runat="server"></asp:TextBox>                <!-- 契約終了日 -->
    <asp:TextBox ID="WF_SEL_PURCHASEPRICE" runat="server"></asp:TextBox>              <!-- 購入価格 -->
    <asp:TextBox ID="WF_SEL_REMODELINGCOST" runat="server"></asp:TextBox>             <!-- 改造価格 -->
    <asp:TextBox ID="WF_SEL_SURVIVALRATE" runat="server"></asp:TextBox>               <!-- 残存率 -->
    <asp:TextBox ID="WF_SEL_MONTHLEASEFEE" runat="server"></asp:TextBox>              <!-- 月額リース料 -->
    <asp:TextBox ID="WF_SEL_DISPTBL" runat="server"></asp:TextBox>                    <!-- 照会データ(退避用) -->
    <asp:TextBox ID="WF_SEL_SPTBL" runat="server"></asp:TextBox>                      <!-- スプレッドシートデータ(退避用) -->
</div>