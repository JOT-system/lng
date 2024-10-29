<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0017WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0017WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                  <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                　 <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                    <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_DELDATAFLG" runat="server"></asp:TextBox>                <!-- 論理削除フラグ -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>                       <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_ORG2" runat="server"></asp:TextBox>                      <!-- 組織コード2 -->
    <asp:TextBox ID="WF_SEL_BIGCTNCD" runat="server"></asp:TextBox>                  <!-- 大分類コード -->
    <asp:TextBox ID="WF_SEL_BIGCTNCD2" runat="server"></asp:TextBox>                 <!-- 大分類コード2 -->
    <asp:TextBox ID="WF_SEL_MIDDLECTNCD" runat="server"></asp:TextBox>               <!-- 中分類コード -->
    <asp:TextBox ID="WF_SEL_MIDDLECTNCD2" runat="server"></asp:TextBox>              <!-- 中分類コード2 -->
    <asp:TextBox ID="WF_SEL_PURPOSE" runat="server"></asp:TextBox>                   <!-- 使用目的 -->
    <asp:TextBox ID="WF_SEL_SPRUSEFEE" runat="server"></asp:TextBox>                 <!-- 特例置換項目-使用料金額 -->
    <asp:TextBox ID="WF_SEL_SPRUSEFEERATE" runat="server"></asp:TextBox>             <!-- 特例置換項目-使用料率 -->
    <asp:TextBox ID="WF_SEL_SPRUSEFEERATEROUND" runat="server"></asp:TextBox>        <!-- 特例置換項目-使用料率端数整理 -->
    <asp:TextBox ID="WF_SEL_SPRUSEFEERATEROUND1" runat="server"></asp:TextBox>       <!-- 特例置換項目-使用料率端数整理 -->
    <asp:TextBox ID="WF_SEL_SPRUSEFEERATEROUND2" runat="server"></asp:TextBox>       <!-- 特例置換項目-使用料率端数整理 -->
    <asp:TextBox ID="WF_SEL_SPRUSEFEERATEADDSUB" runat="server"></asp:TextBox>       <!-- 特例置換項目-使用料率加減額 -->
    <asp:TextBox ID="WF_SEL_SPRUSEFEERATEADDSUBCOND" runat="server"></asp:TextBox>   <!-- 特例置換項目-使用料率加減額端数整理 -->
    <asp:TextBox ID="WF_SEL_SPRUSEFEERATEADDSUBCOND1" runat="server"></asp:TextBox>  <!-- 特例置換項目-使用料率加減額端数整理 -->
    <asp:TextBox ID="WF_SEL_SPRUSEFEERATEADDSUBCOND2" runat="server"></asp:TextBox>  <!-- 特例置換項目-使用料率加減額端数整理 -->
    <asp:TextBox ID="WF_SEL_SPRROUNDPOINTKBN" runat="server"></asp:TextBox>          <!-- 特例置換項目-端数処理時点区分 -->
    <asp:TextBox ID="WF_SEL_SPRUSEFREESPE" runat="server"></asp:TextBox>             <!-- 特例置換項目-使用料無料特認 -->
    <asp:TextBox ID="WF_SEL_SPRNITTSUFREESENDFEE" runat="server"></asp:TextBox>      <!-- 特例置換項目-通運負担回送運賃 -->
    <asp:TextBox ID="WF_SEL_SPRMANAGEFEE" runat="server"></asp:TextBox>              <!-- 特例置換項目-運行管理料 -->
    <asp:TextBox ID="WF_SEL_SPRSHIPBURDENFEE" runat="server"></asp:TextBox>          <!-- 特例置換項目-荷主負担運賃 -->
    <asp:TextBox ID="WF_SEL_SPRSHIPFEE" runat="server"></asp:TextBox>                <!-- 特例置換項目-発送料 -->
    <asp:TextBox ID="WF_SEL_SPRARRIVEFEE" runat="server"></asp:TextBox>              <!-- 特例置換項目-到着料 -->
    <asp:TextBox ID="WF_SEL_SPRPICKUPFEE" runat="server"></asp:TextBox>              <!-- 特例置換項目-集荷料 -->
    <asp:TextBox ID="WF_SEL_SPRDELIVERYFEE" runat="server"></asp:TextBox>            <!-- 特例置換項目-配達料 -->
    <asp:TextBox ID="WF_SEL_SPROTHER1" runat="server"></asp:TextBox>                 <!-- 特例置換項目-その他１ -->
    <asp:TextBox ID="WF_SEL_SPROTHER2" runat="server"></asp:TextBox>                 <!-- 特例置換項目-その他２ -->
    <asp:TextBox ID="WF_SEL_SPRFITKBN" runat="server"></asp:TextBox>                 <!-- 特例置換項目-適合区分 -->
    <asp:TextBox ID="WF_SEL_SPRCONTRACTCD" runat="server"></asp:TextBox>             <!-- 特例置換項目-契約コード -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                   <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>                  <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>                <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                    <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                   <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>                 <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>                <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                 <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>     <!-- 詳細画面更新 -->
</div>
