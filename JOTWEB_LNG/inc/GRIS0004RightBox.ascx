<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="GRIS0004RightBox.ascx.vb" Inherits="JOTWEB_LNG.GRIS0004RightBox" %>

        <!-- 全体レイアウト　rightbox -->
        <div class="rightbox" id="RF_RIGHTBOX">
            <div id="RF_ERR_MEMO">
                <a>
                    <span style="position:relative;left:0.9em;top:1.2em;font-size:1em;">
                        <asp:Label runat="server" Text="エラー詳細"></asp:Label>
                        <!-- 　非表示　 --> 
                        <%--<asp:RadioButton ID="RF_RIGHT_SW1" runat="server" GroupName="rightbox" Text=" エラー詳細表示" Width="9em" Onclick="rightboxChange('0')" />--%>
                        <%--<asp:RadioButton ID="RF_RIGHT_SW2" runat="server" GroupName="rightbox" Text=" メモ表示" Width="9em" Onclick="rightboxChange('1')" />--%>
                    </span>
                    <input type="button" id="RF_RIGHTBOX_CLOSEBTN" value="×" style="position: relative; left: 17.1em; top: 1.2em; width: 25px; font-size: 1em; text-align: center; border: solid 1px #2bb6c1; color: #2bb6c1;" onclick="r_boxDisplayNonSubmit();" />
                </a>
                <br/>

                <asp:MultiView ID="RF_RIGHTVIEW" runat="server">
                    <!-- 　エラー　 --> 
                    <asp:View id="RF_VIEW1" runat="server" >
                        <a id="RF_RIGHTBOX_ERROR_REPORT">
                            <span id="RF_ERROR_REPORT" style="position:relative;left:1em;top:1.5em;" >
<%--                            <asp:TextBox ID="RF_ERR_REPORT" runat="server" Width="23.6em" Height="16.9em" TextMode="MultiLine" ReadOnly="true"></asp:TextBox>--%>
                                <asp:TextBox ID="RF_ERR_REPORT" runat="server" Width="330px" Height="495px" TextMode="MultiLine" ReadOnly="true"></asp:TextBox>
                            </span>
                            <br />
                        </a>
                    </asp:View>
                    <!-- 　メモ　 -->
                    <asp:View id="RF_VIEW2" runat="server">
                        <a id="RF_RIGHTBOX_MEMO">
                            <span id="RF_MEMOTITLE" style="position:relative;left:1em;top:2em;" onchange="MEMOChange()">
                            <asp:TextBox ID="RF_MEMO" runat="server" Width="28.4em" Height="16.9em" CssClass="WF_MEMO" TextMode="MultiLine"></asp:TextBox>
                            </span><br />
                        </a>
                    </asp:View>
                </asp:MultiView>
            </div>
            <!-- 　非表示　 --> 
            <div id="RF_REPORT_LIST" hidden="hidden">
                <span style="position:relative;left:1em;top:2.0em;display:none">印刷・インポート設定</span><br />

                <span style="position:relative;left:1em;top:2.0em;">
                    <asp:ListBox ID="RF_REPORTID" runat="server" Width="28.4em" Height="15em" style="border: 2px solid blue;background-color: #ccffff;"></asp:ListBox>
                </span>
            </div>
            
            <div id="RF_HIDDEN_LIS">
                <asp:HiddenField ID="RF_COMPCODE" runat="server" />
                <asp:HiddenField ID="RF_MAPID_REPORT" runat="server" />
                <asp:HiddenField ID="RF_MAPID_MEMO" runat="server" />
                <asp:HiddenField ID="RF_PROFID" runat="server" />
                <asp:HiddenField ID="RF_MAPVARI" runat="server" />
            </div>
        </div>
