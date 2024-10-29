<%@ Control Language="vb" AutoEventWireup="true" CodeBehind="GRC0002SELECTIONPOPUPWORKINC.ascx.vb" Inherits="JOTWEB_LNG.GRC0002SELECTIONPOPUPWORKINC" ClientIDMode="Predictable" %>
<div class="grc0002Wrapper">
    <!-- 共通複数選択/一覧選択コントロール -->
    <!-- 動的なスタイルを設定するためにリテラルコントロールでstyleタグを生成 -->    
    <asp:Literal ID="letGrc0002Style" runat="server" EnableViewState ="false" Mode="PassThrough" ></asp:Literal>    
    <div id='<%= Me.ID  %>_pnlGrc0002InputWrapper' class="pnlGrc0002InputWrapper <%= Me.FilterLevel %>" data-grc0002control="1" >
        <!-- hdnShowHideGrc0002 0:非表示 1:複数選択モード  2:単体選択モード -->
        <span data-shohidehiddenobj="1">
            <asp:HiddenField ID="hdnShowHideGrc0002" runat="server" value="0" ClientIDMode="Predictable" />
        </span>

        <div id='<%= Me.ID  %>_pnlGrc0002Input' class="pnlGrc0002Input" >
            <!-- ボタンエリア -->
            <div id='<%= Me.ID  %>_divGrc0002InputHeader' class="divGrc0002InputHeader" >
                <input type="button" id='<%= Me.ID  %>_btnGrc0002ConfirmAdd' class="btn-sticky btnGrc0002ConfirmAdd" value="<%= Me.AddButtonDispName %>"  onclick='ButtonClick(<%= """" & Me.ID & "ConfirmAdd""" %>);' />
                <input type="button" id='<%= Me.ID  %>_btnGrc0002InputCLOSE' class="btn-sticky btnGrc0002InputCLOSE" value="閉じる"  onclick="commonHideGrc0002InputCLOSEClick('<%= Me.hdnShowHideGrc0002.ClientID %>');" />
            </div>
            <!-- コンテンツエリア -->
            <div id='<%= Me.ID  %>_divGrc0002InputContents' class="divGrc0002InputContents" >
                <span data-uniquekeyhiddenobj="1">
                    <asp:HiddenField ID="hdnGrc0002SelectedUniqueKey" runat="server"  ClientIDMode="Predictable"  />
                </span>
                <div id='<%= Me.ID  %>_divGrc0002InputRemark' class="divGrc0002InputRemark" onchange='ButtonClick(<%= """" & Me.ID & "dummycall""" %>);';>
                    <div class="divGrc0002Filters">
                        <div class='divGrc0002FilterTextArea'>
                            <div>文字検索</div>
                            <div>
                                <asp:TextBox ID="txtGrc0002TextSearch" runat="server"></asp:TextBox>
                            </div>
                            <div>
                                ※全項目の部分一致フィルタ
                            </div>
                        </div>
                        <div class='divGrc0002FilterDdlArea <%= Me.FilterLevel %> <%= if(Me.lblGrc0002FilterColName1.Text = "", "emptyDdl1Text", "") %> <%= if(Me.lblGrc0002FilterColName2.Text = "", "emptyDdl2Text", "") %>'>
                            <asp:Panel ID="pnlGrc0002FilterDdl1" runat="server">
                                <div>
                                    <asp:Label ID="lblGrc0002FilterColName1" runat="server" Text=""></asp:Label>
                                </div>
                                <div>
                                    <asp:DropDownList ID="ddlGrc0002Filter1" runat="server"></asp:DropDownList>
                                </div>
                            </asp:Panel>
                            <asp:Panel ID="pnlGrc0002FilterDdl2" runat="server">
                                <div>
                                    <asp:Label ID="lblGrc0002FilterColName2" runat="server" Text=""></asp:Label>
                                </div>
                                <div>
                                    <asp:DropDownList ID="ddlGrc0002Filter2" runat="server"></asp:DropDownList>
                                </div>
                            </asp:Panel>
                        </div>
                    </div>
                </div>
                <div class='divGrc0002InputListWrapper'>
                    <div id='<%= Me.ID  %>_divGrc0002InputList' class="divGrc0002InputList <%= Me.FilterLevel %> ">
                        <!-- 表ヘッダー -->
                        <asp:Repeater ID="repGrc0002ListHeader" runat="server" ClientIDMode="Predictable">
                            <HeaderTemplate>
                                <div class="grc0002selectheaderrow">
                                    <div class='header grc0002chk fix'>選択</div>
                            </HeaderTemplate>
                            <ItemTemplate>
                                    <div class='header <%# Eval("FieldName") %> <%# if(Eval("FixedCol") = True, "fix", "") %>'><%# Eval("DispName") %></div>
                            </ItemTemplate>
                            <FooterTemplate>
                                </div>
                            </FooterTemplate>
                        </asp:Repeater>
                        <!-- 表データ -->
                        <asp:Repeater ID="repGrc0002SelectListRow" runat="server" ClientIDMode="Predictable">
                            <ItemTemplate>
                                <div class='grc0002selectdatarow <%#If(Eval("ChkVal") = True, "checkedrow", "") %>' onclick="commonGrc0002PopUpChangeSelect('<%# Eval("KeyVal") %>','<%= Me.ID  %>_pnlGrc0002InputWrapper','<%= Me.ID  %>',this);">
                                    <div class='data grc0002chk fix'>
                                        <asp:CheckBox ID="chkGrc0002InsideList" runat="server" CssClass='chkGrc0002InsideList' Checked='<%# Eval("ChkVal") %>' />
                                        <asp:HiddenField ID="hdnGrc0002KeyValue" runat="server" Value='<%# Eval("KeyVal") %>' />
                                    </div>
                                    <!-- 動的列データ -->
                                    <asp:Repeater ID="repGrc0002SelectListVCol" runat="server" ClientIDMode="Predictable" DataSource='<%# Eval("ColList") %>'>
                                        <ItemTemplate>
                                            <div class='data <%# Eval("FieldName") %> <%# if(Eval("IsFixed") = True, "fix", "") %> <%# Eval("TextAlign") %>'>
                                                <asp:Label ID="lblGrc0002Vcol" runat="server" Text='<%# Eval("Value") %>'></asp:Label>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>  
                            </ItemTemplate>
                        </asp:Repeater> 
                    </div> <!-- End divGrc0002InputList -->
                </div> <!-- End divGrc0002InputListWrapper -->
            </div> <!-- End _divGrc0002InputContents -->
        </div> <!-- End pnlGrc0002Input -->
    </div>
</div>