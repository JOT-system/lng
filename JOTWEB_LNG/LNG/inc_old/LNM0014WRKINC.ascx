﻿<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0014WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0014WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>           <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>            <!-- 選択行 -->

    <asp:TextBox ID="WF_SEL_BIGCTNCD" runat="server"></asp:TextBox>           <!-- 大分類コード -->
    <asp:TextBox ID="WF_SEL_BIGCTNCD2" runat="server"></asp:TextBox>          <!-- 大分類コード2 -->
    <asp:TextBox ID="WF_SEL_MIDDLECTNCD" runat="server"></asp:TextBox>        <!-- 中分類コード -->
    <asp:TextBox ID="WF_SEL_MIDDLECTNCD2" runat="server"></asp:TextBox>       <!-- 中分類コード2 -->
    <asp:TextBox ID="WF_SEL_DEPSTATION" runat="server"></asp:TextBox>         <!-- 発駅コード -->
    <asp:TextBox ID="WF_SEL_DEPSTATION2" runat="server"></asp:TextBox>        <!-- 発駅コード2 -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEECD" runat="server"></asp:TextBox>       <!-- 発受託人コード -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEECD2" runat="server"></asp:TextBox>      <!-- 発受託人コード2 -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEESUBCD" runat="server"></asp:TextBox>    <!-- 発受託人サブコード -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEESUBCD2" runat="server"></asp:TextBox>   <!-- 発受託人サブコード2 -->
    <asp:TextBox ID="WF_SEL_PRIORITYNO" runat="server"></asp:TextBox>         <!-- 優先順位 -->
    <asp:TextBox ID="WF_SEL_PURPOSE" runat="server"></asp:TextBox>            <!-- 使用目的 -->
    <asp:TextBox ID="WF_SEL_ARRSTATION" runat="server"></asp:TextBox>         <!-- 着駅コード -->
    <asp:TextBox ID="WF_SEL_ARRTRUSTEECD" runat="server"></asp:TextBox>       <!-- 着受託人コード -->
    <asp:TextBox ID="WF_SEL_ARRTRUSTEESUBCD" runat="server"></asp:TextBox>    <!-- 着受託人サブコード -->
    <asp:TextBox ID="WF_SEL_SPRCURSTYMD" runat="server"></asp:TextBox>        <!-- 特例置換項目-現行開始適用日 -->
    <asp:TextBox ID="WF_SEL_SPRCURENDYMD" runat="server"></asp:TextBox>       <!-- 特例置換項目-現行終了適用日 -->
    <asp:TextBox ID="WF_SEL_SPRCURSHIPFEE" runat="server"></asp:TextBox>      <!-- 特例置換項目-現行発送料 -->
    <asp:TextBox ID="WF_SEL_SPRCURARRIVEFEE" runat="server"></asp:TextBox>    <!-- 特例置換項目-現行到着料 -->
    <asp:TextBox ID="WF_SEL_SPRCURROUNDKBN" runat="server"></asp:TextBox>     <!-- 特例置換項目-現行端数処理区分 -->
    <asp:TextBox ID="WF_SEL_SPRCURROUNDKBN1" runat="server"></asp:TextBox>    <!-- 特例置換項目-現行端数処理区分1 -->
    <asp:TextBox ID="WF_SEL_SPRCURROUNDKBN2" runat="server"></asp:TextBox>    <!-- 特例置換項目-現行端数処理区分2 -->
    <asp:TextBox ID="WF_SEL_SPRNEXTSTYMD" runat="server"></asp:TextBox>       <!-- 特例置換項目-次期開始適用日 -->
    <asp:TextBox ID="WF_SEL_SPRNEXTENDYMD" runat="server"></asp:TextBox>      <!-- 特例置換項目-次期終了適用日 -->
    <asp:TextBox ID="WF_SEL_SPRNEXTSHIPFEE" runat="server"></asp:TextBox>     <!-- 特例置換項目-次期発送料 -->
    <asp:TextBox ID="WF_SEL_SPRNEXTARRIVEFEE" runat="server"></asp:TextBox>   <!-- 特例置換項目-次期到着料 -->
    <asp:TextBox ID="WF_SEL_SPRNEXTROUNDKBN" runat="server"></asp:TextBox>    <!-- 特例置換項目-次期端数処理区分 -->
    <asp:TextBox ID="WF_SEL_SPRNEXTROUNDKBN1" runat="server"></asp:TextBox>   <!-- 特例置換項目-次期端数処理区分1 -->
    <asp:TextBox ID="WF_SEL_SPRNEXTROUNDKBN2" runat="server"></asp:TextBox>   <!-- 特例置換項目-次期端数処理区分2 -->

    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>             <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>            <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>           <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>         <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_INITPGID" runat="server"></asp:TextBox>           <!-- 登録プログラムＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>             <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>            <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>          <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_UPDPGID" runat="server"></asp:TextBox>            <!-- 更新プログラムＩＤ -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>         <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_UPDTIMSTP" runat="server"></asp:TextBox>          <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_DETAIL_UPDATE_MESSAGE" runat="server"></asp:TextBox>    <!-- 詳細画面更新 -->
    <asp:TextBox ID="WF_SEL_DELDATAFLG" runat="server"></asp:TextBox>               <!-- 論理削除フラグ -->
</div>
