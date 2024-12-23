﻿
<%@ Page Title="M10000" Language="vb" AutoEventWireup="true" CodeBehind="M10000LOGON.aspx.vb" Inherits="JOTWEB_LNG.M10000LOGON"  %>

<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %> 



<asp:Content ID="M10000H" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/css/M10000.css")%>"/>  

    <script type="text/javascript" src="<%=ResolveUrl("~/script/M10000.js")%>"></script>

    <!--  ここにGAのスクリプト記載予定 -->

</asp:Content> 

<asp:Content ID="M10000" ContentPlaceHolderID="contents1" runat="server">
       <!--  画像　-->        

        <!-- LOGON　TOPbox -->
        <div id="logonbox" class="logonbox" >
            <div id="logonkeybox" class="logonkeybox">
                <div id="Waku" class="Waku">
                    <div id="LogInImage" class="LogInImage">
                        <asp:Image ID="WF_LOGO" runat ="server" ImageUrl ="~/img/logo.png" alt=""/>
                    </div>
                    <p class="LINE_1">
                        <span>
                            ユーザーID
                            <asp:TextBox ID="UserID" runat="server" Width="300px"></asp:TextBox>
                        </span>
                    </p>
                    <p class="LINE_2">
                        <span>
                            パスワード
                            <asp:TextBox ID="PassWord" runat="server" Width="300px" TextMode="Password"></asp:TextBox>
                        </span>
                    </p>
                    <div class="Operation" >
                        <span>
                        <input type="button" id="OK" value="ログイン"  style="Width:300px" onclick="ButtonClick('WF_ButtonOK'); " />
                        </span>
                    </div>
                    <p class="LINE_3">
                    </p>
                 </div>   
            </div>
            <!-- ガイダンス表示エリア -->
            <div id="guidanceBoxWrapper" class="guidanceboxWrapper">
                <div id="guidanceBox" class="guidancebox">
                    <div id="guidanceTitle">お知らせ</div>
                    <asp:Repeater ID="repGuidance" runat="server">
                        <HeaderTemplate >
                            <ul class="ulGuidance">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <div class="titleLine">
                                    <div class="entryDate"><%# Eval("ENTRYDATE") %></div>
                                    <div class="title"><%# Eval("TITLE") %></div>
                                </div>
                                <div class="otherLine">
                                    <div class="naiyo"><%# Eval("NAIYOU") %></div>
                                    <div class="attachFile1"><a href='<%# ResolveUrl("~/LNG/mas/LNS0008GuidanceDownload.aspx") & "?id=" & JOTWEB_LNG.LNS0008WRKINC.GetParamString(Eval("GUIDANCENO"), "1") %>' target="_blank"><%# Eval("FILE1") %></a></div>
                                </div>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate>
                            </ul>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <div hidden="hidden">
            <input id="WF_ButtonClick" runat="server" value=""  type="text" />      <!-- ボタン押下 -->
            <asp:TextBox ID="WF_TERMID" runat="server"></asp:TextBox>               <!-- 端末ID　 -->
            <asp:TextBox ID="WF_TERMCAMP" runat="server"></asp:TextBox>             <!-- 端末会社　 -->
        </div>

</asp:Content> 