﻿<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0002WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0002WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">

    <!-- 共通 -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_ORG" runat="server"></asp:TextBox>                      <!-- 組織コード -->
    <asp:TextBox ID="WF_SEL_LINECNT" runat="server"></asp:TextBox>                  <!-- 選択行 -->

    <asp:TextBox ID="WF_SEL_BIGCTN_LIST" runat="server"></asp:TextBox>              <!-- コンテナ種別一覧 -->

    <asp:TextBox ID="WF_SEL_ACCKBN_LIST" runat="server"></asp:TextBox>              <!-- 経理資産区分一覧 -->
    <asp:TextBox ID="WF_SEL_ADD12_LIST" runat="server"></asp:TextBox>               <!-- 扉形式一覧 -->

    <!-- 検索用 -->
    <asp:TextBox ID="WF_SEL_PLANTCODE" runat="server"></asp:TextBox>                <!-- 基地コード -->

    <!-- 登録・更新用 -->
    <asp:TextBox ID="WF_SEL_PLANTCODE2" runat="server"></asp:TextBox>               <!-- 基地コード -->
    <asp:TextBox ID="WF_SEL_PLANTNAME" runat="server"></asp:TextBox>                <!-- 基地名称 -->
    <asp:TextBox ID="WF_SEL_PLANTNAMEKANA" runat="server"></asp:TextBox>            <!-- 基地名称カナ -->
    <asp:TextBox ID="WF_SEL_SHIPPERCODE" runat="server"></asp:TextBox>              <!-- 荷主コード -->
    <asp:TextBox ID="WF_SEL_DELFLG" runat="server"></asp:TextBox>                   <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SEL_INITYMD" runat="server"></asp:TextBox>                  <!-- 登録年月日 -->
    <asp:TextBox ID="WF_SEL_INITUSER" runat="server"></asp:TextBox>                 <!-- 登録ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_INITTERMID" runat="server"></asp:TextBox>               <!-- 登録端末 -->
    <asp:TextBox ID="WF_SEL_UPDYMD" runat="server"></asp:TextBox>                   <!-- 更新年月日 -->
    <asp:TextBox ID="WF_SEL_UPDUSER" runat="server"></asp:TextBox>                  <!-- 更新ユーザーＩＤ -->
    <asp:TextBox ID="WF_SEL_UPDTERMID" runat="server"></asp:TextBox>                <!-- 更新端末 -->
    <asp:TextBox ID="WF_SEL_RECEIVEYMD" runat="server"></asp:TextBox>               <!-- 集信日時 -->
    <asp:TextBox ID="WF_SEL_TIMESTAMP" runat="server"></asp:TextBox>                <!-- タイムスタンプ -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->

    
    <!-- 現況表(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB1TBL" runat="server"></asp:TextBox>
    <!-- 計算表(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB2TBL" runat="server"></asp:TextBox>
    <!-- 収支表(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB3TBL" runat="server"></asp:TextBox>
    <!-- 発送個数累計表(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB4TBL" runat="server"></asp:TextBox>
    <!-- 在庫シミュレーション(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB5TBL" runat="server"></asp:TextBox>

</div>
