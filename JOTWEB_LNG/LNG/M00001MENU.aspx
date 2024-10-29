<%@ Page Title="M00001" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="M00001MENU.aspx.vb" Inherits="JOTWEB_LNG.M00001MENU" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %> 

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC"%>
<%@ register src="~/LNG/inc/GRM00001WRKINC.ascx" tagname="work" tagprefix="LSINC" %>

<asp:Content ID="MC0001H" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/LNG/css/M00001.css")%>"/>
    <script type="text/javascript" src="<%=ResolveUrl("~/LNG/script/M00001.js")%>"></script>
</asp:Content>
<asp:Content ID="MC0001" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　Menuheaderbox -->
    <div  class="Menuheaderbox" id="Menuheaderbox">
        <div class="menuHead">
            <!-- ガイダンスエリア -->
            <div id="guidanceArea" class="guidance" runat="server">
                <div id="guidanceList">
                    <p class="ttl">ガイダンス１</p>
                    <asp:Repeater ID="repGuidance" runat="server" ClientIDMode="Predictable">
                        <HeaderTemplate>
                            <table class="guidanceTable">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <!--<td class="guidanceType"><div class='<%# Eval("TYPE") %>'></div></td>-->
                                <td class="entryDate"><%# Eval("ENTRYDATE") %></td>
                                <td>
                                    <a class="title" href="#" onclick="ButtonClick('WF_ButtonShowGuidance<%# Eval("GUIDANCENO") %>'); return false;"><%# Eval("TITLE") %></a>
                                    <p class="naiyo"><%# Eval("NAIYOU") %></p>
                                    <a class="attachFile1" href='<%# ResolveUrl("~/LNG/mas/LNS0008GuidanceDownload.aspx") & "?id=" & JOTWEB_LNG.LNS0008WRKINC.GetParamString(Eval("GUIDANCENO"), "1") %>' target="_blank"><%# Eval("FILE1") %></a>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            <%#If(DirectCast(DirectCast(Container.Parent, Repeater).DataSource, System.Data.DataTable).Rows.Count = 0,
                                                                                                                                                                    "<tr><td class='empty'>ガイダンスはありません</td></tr>",
                                                                                                                                                                    "") %>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
                <div id="guidanceOpenCloseWrapper">
                    <div id="guidanceOpenClose">
                        <span id="guidanceOpenCloseButton"></span>
                    </div>
                </div>
            </div>
            <!-- お知らせエリア -->
            <div id="guidanceBoxWrapper" class="guidanceboxWrapper" runat="server">
                <div id="guidanceBox" class="guidancebox">
                    <div id="guidanceTitle">お知らせ</div>
                    <asp:Repeater ID="repGuidance1" runat="server">
                        <HeaderTemplate >
                            <ul class="ulGuidance">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <div class="titleLine">
                                    <div class="title"><%# Eval("TITLE") %></div>
                                </div>
                                <div class="otherLine">
                                    <div class="naiyo"><%# Eval("NAIYOU") %></div>
                                </div>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate>
                            </ul>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
                <div id="guidanceBoxOpenCloseWrapper">
                    <div id="guidanceBoxOpenClose">
                        <span id="guidanceBoxOpenCloseButton"></span>
                    </div>
                </div>
            </div>
        </div>
        <div class="menuMain2">
            <!-- メインメニュー１ -->
            <div class="menuFrame">
                <label>分析</label>
                <!--分類１ -->
                <div id="divLeftNav3" class= "sideMenu">
                    <asp:Repeater ID="repLeftNav3" runat="server" ClientIDMode="Predictable">
                        <HeaderTemplate>
                            <div class="lblMenu4Title" >
                                <label>実績</label>
                            </div>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <div class="parentMenu4 <%# DirectCast(Container.DataItem, MenuItem).Title %> <%# If(DirectCast(Container.DataItem, MenuItem).HasChild, "hasChild", "") %> " 
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
                <!--分類２ -->
                <div id="divLeftNav4" class= "sideMenu">
                    <asp:Repeater ID="repLeftNav4" runat="server" ClientIDMode="Predictable">
                        <HeaderTemplate>
                            <div class="lblMenu4Title" >
                                <label>予算</label>
                            </div>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <div class="parentMenu4 <%# DirectCast(Container.DataItem, MenuItem).Title %> <%# If(DirectCast(Container.DataItem, MenuItem).HasChild, "hasChild", "") %> " 
                                data-posicol='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).PosiCol) %>'
                                data-rowline='<%# Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).RowLine) %>'
                                data-hasnext='<%# if(Server.HtmlEncode(DirectCast(Container.DataItem, MenuItem).HasNextPageInfo), "1", "") %>'
                                >
                                
                                <asp:CheckBox ID="chkTopItem4" 
                                            runat="server"
                                            Text='<%# DirectCast(Container.DataItem, MenuItem).Names %>'
                                            Checked='<%# DirectCast(Container.DataItem, MenuItem).OpenChild %>' />
                                
                                <asp:Repeater ID="repLeftNavChild4" 
                                                runat="server" 
                                                DataSource='<%# DirectCast(Container.DataItem, MenuItem).ChildMenuItem %>'>
                                    <HeaderTemplate>
                                        <div class="childMenu4" <%# "onclick='document.getElementById(""" & DirectCast(DirectCast(Container.Parent.Parent, RepeaterItem).FindControl("chkTopItem4"), CheckBox).ClientID & """).checked = !document.getElementById(""" & DirectCast(DirectCast(Container.Parent.Parent, RepeaterItem).FindControl("chkTopItem4"), CheckBox).ClientID & """).checked;'" %>>
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
            <!-- メインメニュー２ -->
            <div class="menuFrame">
                <label>システム管理</label>
                <div id="divLeftNav5" class= "sideMenu">
                    <asp:Repeater ID="repLeftNav5" runat="server" ClientIDMode="Predictable">
                        <HeaderTemplate>
                            <div class="lblMenu4Title" >
                                <label>マスタ管理</label>
                            </div>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <div class="parentMenu4 <%# DirectCast(Container.DataItem, MenuItem).Title %> <%# If(DirectCast(Container.DataItem, MenuItem).HasChild, "hasChild", "") %> " 
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
        <!-- ***** ボタン押下 ***** -->
        <a hidden="hidden">
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />
            <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
            <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
            <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
            <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
            <input id="WF_SelectChangeDdl" runat="server" value=""  type="text" />      <!-- DDL変更 -->
            <!-- 左ナビでクリックしたボタンにつきサーバー保持の遷移先情報を特定するためのキーを格納 -->
            <asp:HiddenField ID="hdnPosiCol" runat="server" Value="" />
            <asp:HiddenField ID="hdnRowLine" runat="server" Value="" /> 
            <asp:HiddenField ID="WF_HdnGuidanceUrl" visible="false" runat="server" />
            <asp:HiddenField ID="hdnPaneAreaVScroll" runat="server"  />

            <!-- Textbox Print URL -->
            <input id="WF_PrintURL01" runat="server" value="" type="text" />
            <input id="WF_PrintURL02" runat="server" value="" type="text" />
            <input id="WF_PrintURL03" runat="server" value="" type="text" />
            <input id="WF_PrintURL04" runat="server" value="" type="text" />
            <input id="WF_PrintURL05" runat="server" value="" type="text" />
            <input id="WF_PrintURL06" runat="server" value="" type="text" />
            <input id="WF_PrintURL07" runat="server" value="" type="text" />
            <input id="WF_PrintURL08" runat="server" value="" type="text" />
            <input id="WF_PrintURL09" runat="server" value="" type="text" />
            <input id="WF_PrintURL10" runat="server" value="" type="text" />
            <input id="WF_PrintURL11" runat="server" value="" type="text" />
            <input id="WF_PrintURL12" runat="server" value="" type="text" />
            <input id="WF_PrintURL13" runat="server" value="" type="text" />
            <input id="WF_PrintURL14" runat="server" value="" type="text" />
            <input id="WF_PrintURL15" runat="server" value="" type="text" />
        </a>
    </div>
    <!-- Work レイアウト -->
    <LSINC:work id="work" runat="server" />
    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- 非表示項目 -->
    <div hidden="hidden">
        <!-- ロールID -->
        <input id="WF_ApprovalId" runat="server" value="" type="text" />
    </div>
</asp:Content>

