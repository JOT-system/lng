<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNS0008WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNS0008WRKINC" %>
<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_DELDATAFLG" runat="server"></asp:TextBox>               <!-- 論理削除フラグ -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>                      <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_GUIDANCENO" runat="server"></asp:TextBox>               <!-- ガイダンス№ -->
    <asp:TextBox ID="WF_SEL_GUIDANCENO2" runat="server"></asp:TextBox>              <!-- ガイダンス№2 -->
    <asp:TextBox ID="WF_SEL_FROMYMD" runat="server"></asp:TextBox>                  <!-- 掲載開始日 -->
    <asp:TextBox ID="WF_SEL_ENDYMD" runat="server"></asp:TextBox>                   <!-- 掲載終了日 -->
    <asp:TextBox ID="WF_SEL_TYPE" runat="server"></asp:TextBox>                     <!-- 種類 -->
    <asp:TextBox ID="WF_SEL_TITLE" runat="server"></asp:TextBox>                    <!-- タイトル -->
    <asp:TextBox ID="WF_SEL_DISPFLAGS_LIST" runat="server"></asp:TextBox>           <!-- 掲載フラグ -->
    <asp:TextBox ID="WF_SEL_NAIYOU" runat="server"></asp:TextBox>                   <!-- 内容 -->
    <asp:TextBox ID="WF_SEL_FILE1" runat="server"></asp:TextBox>                    <!-- 添付ファイル名１ -->
    <asp:TextBox ID="WF_SEL_FILE2" runat="server"></asp:TextBox>                    <!-- 添付ファイル名２ -->
    <asp:TextBox ID="WF_SEL_FILE3" runat="server"></asp:TextBox>                    <!-- 添付ファイル名３ -->
    <asp:TextBox ID="WF_SEL_FILE4" runat="server"></asp:TextBox>                    <!-- 添付ファイル名４ -->
    <asp:TextBox ID="WF_SEL_FILE5" runat="server"></asp:TextBox>                    <!-- 添付ファイル名５ -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                  <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>                 <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>               <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                   <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                  <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>                <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>               <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_USER_CAMPCODE" runat="server"></asp:TextBox>            <!-- ユーザー会社コード -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->
</div>
