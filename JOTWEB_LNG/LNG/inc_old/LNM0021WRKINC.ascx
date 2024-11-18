<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0021WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0021WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>               <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                 <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_DELDATAFLG" runat="server"></asp:TextBox>             <!-- 論理削除フラグ -->
    <asp:TextBox ID="WF_SEL_ITEMCD" runat="server"></asp:TextBox>                 <!-- 品目コード -->
    <asp:TextBox ID="WF_SEL_ITEMCD2" runat="server"></asp:TextBox>                <!-- 品目コード2 -->
    <asp:TextBox ID="WF_SEL_NAME" runat="server"></asp:TextBox>                   <!-- 品目名称 -->
    <asp:TextBox ID="WF_SEL_NAMES" runat="server"></asp:TextBox>                  <!-- 品目名称(短) -->
    <asp:TextBox ID="WF_SEL_NAMEKANA" runat="server"></asp:TextBox>               <!-- 品目カナ名称 -->
    <asp:TextBox ID="WF_SEL_NAMEKANAS" runat="server"></asp:TextBox>              <!-- 品目カナ名称(短) -->
    <asp:TextBox ID="WF_SEL_SPBIGCATEGCD" runat="server"></asp:TextBox>           <!-- 特大分類コード -->
    <asp:TextBox ID="WF_SEL_BIGCATEGCD" runat="server"></asp:TextBox>             <!-- 大分類コード -->
    <asp:TextBox ID="WF_SEL_MIDDLECATEGCD" runat="server"></asp:TextBox>          <!-- 中大分類コード -->
    <asp:TextBox ID="WF_SEL_SMALLCATEGCD" runat="server"></asp:TextBox>           <!-- 小大分類コード -->
    <asp:TextBox ID="WF_SEL_DANGERKBN" runat="server"></asp:TextBox>              <!-- 危険品区分 -->
    <asp:TextBox ID="WF_SEL_LIGHTWTKBN" runat="server"></asp:TextBox>             <!-- 軽量品区分 -->
    <asp:TextBox ID="WF_SEL_VALUABLEKBN" runat="server"></asp:TextBox>            <!-- 貴重品区分 -->
    <asp:TextBox ID="WF_SEL_REFRIGERATIONFLG" runat="server"></asp:TextBox>       <!-- 冷蔵適合フラグ -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>               <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>             <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                 <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>              <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>             <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>              <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                 <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>  <!-- 詳細画面更新 -->
</div>
