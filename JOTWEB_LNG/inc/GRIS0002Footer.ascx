﻿<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRIS0002Footer.ascx.vb" Inherits="JOTWEB_LNG.GRIS0002Footer" %>

        <!-- 全体レイアウト　footerbox -->
        <div class="footerbox" id="footerbox" style="display:none;">
            <asp:Label ID="WF_MESSAGE" runat="server" Text="" CssClass="WF_MESSAGE" ondblclick="r_boxDisplay();"></asp:Label>
            <asp:Panel ID="WF_HELPIMG" runat="server" ondblclick="HelpDisplay();"></asp:Panel>
        </div>