<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0024WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0024WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                 <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORICODE2" runat="server"></asp:TextBox>                <!-- 取引先コード2 -->
    <asp:TextBox ID="WF_SEL_INVFILINGDEPT" runat="server"></asp:TextBox>            <!-- 請求書提出部店 -->
    <asp:TextBox ID="WF_SEL_INVFILINGDEPT2" runat="server"></asp:TextBox>           <!-- 請求書提出部店2 -->
    <asp:TextBox ID="WF_SEL_INVKESAIKBN" runat="server"></asp:TextBox>              <!-- 請求書決済区分 -->
    <asp:TextBox ID="WF_SEL_INVKESAIKBN2" runat="server"></asp:TextBox>             <!-- 請求書決済区分2 -->
    <asp:TextBox ID="WF_SEL_TORINAME" runat="server"></asp:TextBox>                 <!-- 取引先名称 -->
    <asp:TextBox ID="WF_SEL_TORINAMES" runat="server"></asp:TextBox>                <!-- 取引先略称 -->
    <asp:TextBox ID="WF_SEL_TORINAMEKANA" runat="server"></asp:TextBox>             <!-- 取引先カナ名称 -->
    <asp:TextBox ID="WF_SEL_TORIDIVNAME" runat="server"></asp:TextBox>              <!-- 取引先部門名称 -->
    <asp:TextBox ID="WF_SEL_TORICHARGE" runat="server"></asp:TextBox>               <!-- 取引先担当者 -->
    <asp:TextBox ID="WF_SEL_TORIKBN" runat="server"></asp:TextBox>                  <!-- 取引先区分 -->
    <asp:TextBox ID="WF_SEL_POSTNUM1" runat="server"></asp:TextBox>                 <!-- 郵便番号（上） -->
    <asp:TextBox ID="WF_SEL_POSTNUM2" runat="server"></asp:TextBox>                 <!-- 郵便番号（下） -->
    <asp:TextBox ID="WF_SEL_ADDR1" runat="server"></asp:TextBox>                    <!-- 住所１ -->
    <asp:TextBox ID="WF_SEL_ADDR2" runat="server"></asp:TextBox>                    <!-- 住所２ -->
    <asp:TextBox ID="WF_SEL_ADDR3" runat="server"></asp:TextBox>                    <!-- 住所３ -->
    <asp:TextBox ID="WF_SEL_ADDR4" runat="server"></asp:TextBox>                    <!-- 住所４ -->
    <asp:TextBox ID="WF_SEL_TEL" runat="server"></asp:TextBox>                      <!-- 電話番号 -->
    <asp:TextBox ID="WF_SEL_FAX" runat="server"></asp:TextBox>                      <!-- ＦＡＸ番号 -->
    <asp:TextBox ID="WF_SEL_MAIL" runat="server"></asp:TextBox>                     <!-- メールアドレス -->
    <asp:TextBox ID="WF_SEL_PAYKBN" runat="server"></asp:TextBox>                   <!-- 請求支払区分 -->
    <asp:TextBox ID="WF_SEL_BANKCODE" runat="server"></asp:TextBox>                 <!-- 銀行コード -->
    <asp:TextBox ID="WF_SEL_BANKBRANCHCODE" runat="server"></asp:TextBox>           <!-- 支店コード -->
    <asp:TextBox ID="WF_SEL_ACCOUNTTYPE" runat="server"></asp:TextBox>              <!-- 口座種別 -->
    <asp:TextBox ID="WF_SEL_ACCOUNTNUMBER" runat="server"></asp:TextBox>            <!-- 口座番号 -->
    <asp:TextBox ID="WF_SEL_ACCOUNTNAME" runat="server"></asp:TextBox>              <!-- 口座名義 -->
    <asp:TextBox ID="WF_SEL_INACCOUNTCD" runat="server"></asp:TextBox>              <!-- 社内口座コード -->
    <asp:TextBox ID="WF_SEL_TAXCALCULATION" runat="server"></asp:TextBox>           <!-- 税計算区分 -->
    <asp:TextBox ID="WF_SEL_DEPOSITDAY" runat="server"></asp:TextBox>               <!-- 入金日 -->
    <asp:TextBox ID="WF_SEL_DEPOSITMONTHKBN" runat="server"></asp:TextBox>          <!-- 入金月区分 -->
    <asp:TextBox ID="WF_SEL_CLOSINGDAY" runat="server"></asp:TextBox>               <!-- 計上締日 -->
    <asp:TextBox ID="WF_SEL_ACCOUNTINGMONTH" runat="server"></asp:TextBox>          <!-- 計上月区分 -->
    <asp:TextBox ID="WF_SEL_SLIPDESCRIPTION1" runat="server"></asp:TextBox>         <!-- 伝票摘要１ -->
    <asp:TextBox ID="WF_SEL_SLIPDESCRIPTION2" runat="server"></asp:TextBox>         <!-- 伝票摘要２ -->
    <asp:TextBox ID="WF_SEL_NEXTMONTHUNSETTLEDKBN" runat="server"></asp:TextBox>    <!-- 運賃翌月未決済区分 -->

    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                  <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>                 <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>               <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_INITPGID" runat="server"></asp:TextBox>                 <!-- 登録プログラムＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                   <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                  <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>                <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_UPDPGID" runat="server"></asp:TextBox>                  <!-- 更新プログラムＩＤ -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>               <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_UPDTIMSTP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->
    <asp:TextBox ID="WF_SEL_DELDATAFLG" runat="server"></asp:TextBox>               <!-- 論理削除フラグ -->
</div>
