<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRIS0001Title.ascx.vb" Inherits="JOTWEB_LNG.GRIS0001Title" %>

        <!-- 全体レイアウト　titlebox -->
        <div class="titlebox" id="titlebox" runat="server">
            <div id="tblTitlebox">
                <div class="syriusLogo">
                    <asp:Label ID="WF_TITLETEXT" class="WF_TITLETEXT" runat="server" Text=""></asp:Label>
                </div>
                <ul class="wfArea">
                    <li>
                        <input type="button" id="WF_ButtonLogOut" class="btn-sticky" value="ログアウト" onclick="location.href='/M10000LOGON.aspx'">
                    </li>
                    <li>
                        <asp:Label ID="WF_TITLEID" class="WF_TITLEID" runat="server" Text=""></asp:Label>
                        <asp:Label ID="WF_USERNAME" class="WF_USERNAME" runat="server" Text=""></asp:Label>
                        <div hidden="hidden">
                           <%=If(Parent.Parent.FindControl("contents1").Page.Title = "M00001", "<span id='spnOpenNewTab' onclick='commonOpenNewTab(""" & ResolveUrl(Parent.Parent.FindControl("contents1").Page.Form.Page.AppRelativeVirtualPath) & """);return false;'>新しいタブを開く</span>", "&nbsp;") %>
                            <asp:Label ID="lblCommonHeaderLeftBottom" runat="server" Text=""></asp:Label>
                        </div>
                    </li>
                    <li>
                        <asp:Label ID="WF_TITLECAMP" class="WF_TITLECAMP" runat="server" Text=""></asp:Label>
                        <asp:Label ID="WF_TITLEDATE" class="WF_TITLEDATE" runat="server" Text=""></asp:Label>
                    </li>
                </ul>
                <!-- 右BOXアイコンは非表示 -->
                <div id="rightb" onclick="r_boxDisplayNonSubmit();" style="display:none">
                    <div id="divShowRightBox"></div>
                </div>
            </div>
            <%--                <img id="rightb" class="WF_rightboxSW" src="<%=ResolveUrl("~/img/right.png")%>" style="z-index:30" ondblclick="r_boxDisplay();" alt=""/>--%>
        </div>