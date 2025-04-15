﻿<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0001WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0001WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>                 <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_YM" runat="server"></asp:TextBox>                　     <!-- 日付（年月） -->
    <asp:TextBox ID="WF_SEL_TAISHOYM" runat="server"></asp:TextBox>                 <!-- 対象年月 -->
    <asp:TextBox ID="WF_SEL_TORICODE" runat="server"></asp:TextBox>                 <!-- 取引先コード -->
    <asp:TextBox ID="WF_SEL_TORIINDEX" runat="server"></asp:TextBox>                <!-- 取引先選択位置 -->
    <asp:TextBox ID="WF_SEL_ORGCODE" runat="server"></asp:TextBox>                  <!-- 部署コード -->
    <asp:TextBox ID="WF_SEL_CONTROLTYPE" runat="server"></asp:TextBox>              <!-- 表示対象区分 -->
    
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>                   <!-- 更新データ(退避用) -->
</div>
