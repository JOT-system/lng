﻿<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNM0008WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNM0008WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                　<!-- 選択行 -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_DELDATAFLG" runat="server"></asp:TextBox>               <!-- 論理削除フラグ -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>                      <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_ORG2" runat="server"></asp:TextBox>                     <!-- 組織コード2 -->
    <asp:TextBox ID="WF_SEL_BIGCTNCD" runat="server"></asp:TextBox>                 <!-- 大分類コード -->
    <asp:TextBox ID="WF_SEL_BIGCTNCD2" runat="server"></asp:TextBox>                <!-- 大分類コード2 -->
    <asp:TextBox ID="WF_SEL_MIDDLECTNCD" runat="server"></asp:TextBox>              <!-- 中分類コード -->
    <asp:TextBox ID="WF_SEL_MIDDLECTNCD2" runat="server"></asp:TextBox>             <!-- 中分類コード2 -->
    <asp:TextBox ID="WF_SEL_PURPOSE" runat="server"></asp:TextBox>                  <!-- 使用目的 -->
    <asp:TextBox ID="WF_SEL_STACKFREEKBN" runat="server"></asp:TextBox>             <!-- 積空区分 -->
    <asp:TextBox ID="WF_SEL_SPRDEPTRUSTEECD" runat="server"></asp:TextBox>          <!-- 特例置換項目-発受託人コード -->
    <asp:TextBox ID="WF_SEL_SPRDEPTRUSTEESUBCD" runat="server"></asp:TextBox>       <!-- 特例置換項目-発受託人サブコード -->
    <asp:TextBox ID="WF_SEL_SPRDEPTRUSTEESUBZKBN" runat="server"></asp:TextBox>     <!-- 特例置換項目-発受託人サブゼロ変換区分 -->
    <asp:TextBox ID="WF_SEL_SPRDEPSHIPPERCD" runat="server"></asp:TextBox>          <!-- 特例置換項目-発荷主コード -->
    <asp:TextBox ID="WF_SEL_SPRARRTRUSTEECD" runat="server"></asp:TextBox>          <!-- 特例置換項目-着受託人コード -->
    <asp:TextBox ID="WF_SEL_SPRARRTRUSTEESUBCD" runat="server"></asp:TextBox>       <!-- 特例置換項目-着受託人サブ -->
    <asp:TextBox ID="WF_SEL_SPRARRTRUSTEESUBZKBN" runat="server"></asp:TextBox>     <!-- 特例置換項目-着受託人サブゼロ変換区分 -->
    <asp:TextBox ID="WF_SEL_SPRJRITEMCD" runat="server"></asp:TextBox>              <!-- 特例置換項目-ＪＲ品目コード -->
    <asp:TextBox ID="WF_SEL_SPRSTACKFREEKBN" runat="server"></asp:TextBox>          <!-- 特例置換項目-積空区分 -->
    <asp:TextBox ID="WF_SEL_SPRSTATUSKBN" runat="server"></asp:TextBox>             <!-- 特例置換項目-状態区分 -->
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