<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0023WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0023WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                　<!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_DELDATAFLG" runat="server"></asp:TextBox>               <!-- 論理削除フラグ -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_SHIPPERCD" runat="server"></asp:TextBox>                <!-- 荷主コード -->
    <asp:TextBox ID="WF_SEL_SHIPPERCD2" runat="server"></asp:TextBox>               <!-- 荷主コード2 -->
    <asp:TextBox ID="WF_SEL_NAME" runat="server"></asp:TextBox>                     <!-- 荷主名称 -->
    <asp:TextBox ID="WF_SEL_NAMES" runat="server"></asp:TextBox>                    <!-- 荷主名称（短） -->
    <asp:TextBox ID="WF_SEL_NAMEKANA" runat="server"></asp:TextBox>                 <!-- 荷主カナ名称 -->
    <asp:TextBox ID="WF_SEL_NAMEKANAS" runat="server"></asp:TextBox>                <!-- 荷主カナ名称（短） -->
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
