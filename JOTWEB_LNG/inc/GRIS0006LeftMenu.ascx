<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRIS0006LeftMenu.ascx.vb" Inherits="JOTWEB_LNG.GRIS0006LeftMenu" %>

    <div class="bg-white shadow overflow-x-hidden overflow-y-auto flex-shrink-0 side-menu">
        <div class="side-menu-inner">
            <button type="button" class="d-flex align-items-center gap-2 w-100 border-0 border-bottom fw-bold px-2 side-menu-top"><span class="material-symbols-outlined">home</span>TOP<span class="material-symbols-outlined ms-auto">chevron_right</span></button>
            <div id="divLeftNav3">
                <asp:Repeater ID="repLeftNav3" runat="server" ClientIDMode="Predictable">
                    <HeaderTemplate>
                        <div class="border-bottom" >
                            <p class="d-flex align-items-center gap-2 fw-bold px-2 side-menu-chart"><span class="material-symbols-outlined">bar_chart_4_bars</span>分析</p>
                        </div>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <div class="d-flex align-items-center w-100 border-0 border-top fw-bold text-start pe-2 side-menu-button <%# DirectCast(Container.DataItem, MenuItem).Title %> <%# If(DirectCast(Container.DataItem, MenuItem).HasChild, "hasChild", "") %> " 
                            data-posicol='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).PosiCol) %>'
                            data-rowline='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).RowLine) %>'
                            data-hasnext='<%# if(Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).HasNextPageInfo), "1", "") %>'
                            >
                                
                            <asp:CheckBox ID="chkTopItem3" 
                                        runat="server"
                                        Text='<%# DirectCast(Container.DataItem, MenuItem).Names %>'
                                        Checked='<%# DirectCast(Container.DataItem, MenuItem).OpenChild %>' />
                                
                            <asp:Repeater ID="repLeftNavChild3" 
                                            runat="server" 
                                            DataSource='<%# DirectCast(Container.DataItem, MenuItem).ChildMenuItem %>'>
                                <HeaderTemplate>
                                    <div class="childMenu3" <%# "onclick='document.getElementById(""" & DirectCast(DirectCast(Container.Parent.Parent, RepeaterItem).FindControl("chkTopItem3"), CheckBox).ClientID & """).checked = !document.getElementById(""" & DirectCast(DirectCast(Container.Parent.Parent, RepeaterItem).FindControl("chkTopItem3"), CheckBox).ClientID & """).checked;'" %>>
                                </HeaderTemplate>  
                                <ItemTemplate>
                                    <div data-posicol='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).PosiCol) %>'
                                        data-rowline='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).RowLine) %>'
                                        data-hasnext='<%# if(Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).HasNextPageInfo), "1", "") %>'
                                        >
                                        <label><%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).Names) %></label>
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </div>
                                </FooterTemplate>
                            </asp:Repeater>
                            
                        </div>
                    </ItemTemplate>
                    <FooterTemplate>

                    </FooterTemplate>
                </asp:Repeater>
            </div>

            <div id="divLeftNav5">
                <asp:Repeater ID="repLeftNav5" runat="server" ClientIDMode="Predictable">
                    <HeaderTemplate>
                        <div class="border-bottom" >
                            <button type="button" class="d-flex align-items-center flex-start gap-2 w-100 border-0 fw-bold text-start pe-2 side-menu-accordion" data-bs-toggle="collapse" data-bs-target="#panelsStayOpen-master" aria-expanded="true" aria-controls="panelsStayOpen-master"><span class="material-symbols-outlined">edit_note</span>マスタ<span class="material-symbols-outlined ms-auto arrow-down">keyboard_arrow_down</span></button>
                        </div>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <div class="d-flex align-items-center w-100 border-0 border-top fw-bold text-start pe-2 side-menu-button <%# DirectCast(Container.DataItem, MenuItem).Title %> <%# If(DirectCast(Container.DataItem, MenuItem).HasChild, "hasChild", "") %> " 
                            data-posicol='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).PosiCol) %>'
                            data-rowline='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).RowLine) %>'
                            data-hasnext='<%# if(Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).HasNextPageInfo), "1", "") %>'
                            >
                                
                            <asp:CheckBox ID="chkTopItem5" 
                                        runat="server"
                                        Text='<%# DirectCast(Container.DataItem, MenuItem).Names %>'
                                        Checked='<%# DirectCast(Container.DataItem, MenuItem).OpenChild %>' />
                                
                            <asp:Repeater ID="repLeftNavChild5" 
                                            runat="server" 
                                            DataSource='<%# DirectCast(Container.DataItem, MenuItem).ChildMenuItem %>'>
                                <HeaderTemplate>
                                    <div class="childMenu3" <%# "onclick='document.getElementById(""" & DirectCast(DirectCast(Container.Parent.Parent, RepeaterItem).FindControl("chkTopItem5"), CheckBox).ClientID & """).checked = !document.getElementById(""" & DirectCast(DirectCast(Container.Parent.Parent, RepeaterItem).FindControl("chkTopItem5"), CheckBox).ClientID & """).checked;'" %>>
                                </HeaderTemplate>  
                                <ItemTemplate>
                                    <div data-posicol='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).PosiCol) %>'
                                        data-rowline='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).RowLine) %>'
                                        data-hasnext='<%# if(Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).HasNextPageInfo), "1", "") %>'
                                        >
                                        <label><%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).Names) %></label>
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </div>
                                </FooterTemplate>
                            </asp:Repeater>
                            
                        </div>
                    </ItemTemplate>
                    <FooterTemplate>

                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>

    <!-- イベント用 -->
    <div style="display:none;">
        <!-- 左ナビでクリックしたボタンにつきサーバー保持の遷移先情報を特定するためのキーを格納 -->
        <asp:HiddenField ID="hdnPosiCol" runat="server" Value="" />
        <asp:HiddenField ID="hdnRowLine" runat="server" Value="" /> 

        <asp:HiddenField ID="LM_COMPCODE" runat="server" />
        <asp:HiddenField ID="LM_ROLE_MENU" runat="server" />
    </div>

