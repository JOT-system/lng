<%@ Page Title="LNT0022C" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0022InspectManage.aspx.vb" Inherits="JOTWEB_LNG.LNT0022InspeLNManage" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0022WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0001TILESELECTORWRKINC.ascx" TagName="tilelist" TagPrefix="MSINC" %>

<asp:Content ID="LNT0022CH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0022C.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/script/fixed_midashi.js")%>'></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0022C.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="LNT0022C" ContentPlaceHolderID="contents1" runat="server">
    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
    <div id="detailbuttonbox" class="detailbuttonbox">
        <div class="actionButtonBox">
            <div class="rightSide">
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="前の画面に戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div>
    </div>
    <!-- 全体レイアウト(header) -->
    <div class="headerbox" id="headerbox">
        <div class="actionButtonBox">
            <div class="leftSide">
                <div id="WF_SELECT_ITEM_AREA">
                    <span>状態</span>
                    <asp:DropDownList ID="WF_STATUS" runat="server" onchange="ButtonClick('WF_STATUS');" />
                    <span>記号</span>
                    <asp:DropDownList ID="WF_CTNTYPE" runat="server" onchange="ButtonClick('WF_CTNTYPE');" />
                    <span>番号</span>
                    <asp:TextBox ID="WF_CTNNO" runat="server" MaxLength="6" />
                    <span>駅コード</span>
                    <asp:TextBox ID="WF_STATION" runat="server" MaxLength="6" />
                    <asp:Label ID="WF_STATIONNAME" runat="server" />
                </div>
            </div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonInit" class="btn-sticky" value="初期化" onclick="ButtonClick('WF_ButtonInit');" />
                <input type="button" id="WF_ButtonPrint" class="btn-sticky" value="帳票出力" onclick="return false;" disabled />
                <input type="button" id="WF_ButtonDownload" runat="server" class="btn-sticky" value="ダウンロード" onclick="ButtonClick('WF_Download');" />
                <input type="button" id="WF_ButtonUpload" class="btn-sticky" value="アップロード" onclick="btnUploadFile();" />
                <asp:FileUpload ID="WF_FileUpload" runat="server" onchange="ButtonClick('WF_FileUpload');" />
            </div>
        </div>
    </div>
    <!-- 全体レイアウト(detailbox) -->
    <div class="detailbox" id="detailbox">
        <!-- 頁 -->
        <asp:Panel ID="pnlChangePage" runat="server">
            <asp:TextBox ID="txtSelectPage" runat="server" Text="" CssClass="font16" />
            <asp:Button ID="btnRefreshPage" runat="server" Text="頁へ"
                UseSubmitBehavior="False" CssClass="btn-sticky"
                OnClientClick="return btnPageClick(5);" />
            <div class="arrowFirstPage">
                <asp:ImageButton ID="btnFirstPage" runat="server" CssClass=""
                    ImageUrl="../img/iconSkipBack.png" UseSubmitBehavior="False"
                    OnClientClick="return btnPageClick(1);" />
            </div>
            <div class="arrowPreviousPage">
                <asp:ImageButton ID="btnBackPage" runat="server" CssClass=""
                    ImageUrl="../img/iconArrowBack.png" UseSubmitBehavior="False"
                    OnClientClick="return btnPageClick(2);" />
            </div>
            <div>
                <asp:Label ID="lblNowPage" runat="server" CssClass="font16" />
                <span class="font18">/</span>
                <asp:Label ID="lblMaxPage" runat="server" CssClass="font16" />
            </div>
            <div class="arrowNextPage">
                <asp:ImageButton ID="btnNextPage" runat="server" CssClass=""
                    ImageUrl="../img/iconArrowNext.png" UseSubmitBehavior="False"
                    OnClientClick="return btnPageClick(3);" />
            </div>
            <div class="arrowLastPage">
                <asp:ImageButton ID="btnLastPage" runat="server"
                    ImageUrl="../img/iconSkipNext.png" UseSubmitBehavior="False"
                    OnClientClick="return btnPageClick(4);" />
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlNoData" runat="server" Visible="false">
            <span class="font18">条件に該当するコンテナはありません</span>
        </asp:Panel>
        <div class="tblWrapper">
            <asp:GridView ID="gvLNT0022" runat="server" CssClass="tbl ctnlist" _fixedhead="rows:1; cols:7; div-auto-size: none;"
                AllowPaging="false" AutoGenerateColumns="false" ShowHeader="true" GridLines="None"
                ShowHeaderWhenEmpty="false" Visible="true" ShowFooter="false" ClientIDMode="Predictable"
                OnPreRender="gvLNT0022_PreRender" OnDataBound="gvLNT0022_DataBound">
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>記号</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "CTNTYPE") %>
                            <asp:HiddenField ID="LINECNT" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "LINECNT") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>番号</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "CTNNO") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>コンテナ番号</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "CONTNUM") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>製造年月</HeaderTemplate>
                        <ItemTemplate>
                            <%# CDate(DataBinder.Eval(Container.DataItem, "CONTRUCTIONYM")).ToString("yyyy年MM月") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>現在駅</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "ARRSTATIONNAME") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>交番検査<BR>前回実施日</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "TRAINSBEFORERUNYMD") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>交番検査<BR>次回実施日</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "TRAINSNEXTRUNYMD") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>4年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR4_AFTER") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>4年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR4_INSPECTYMD") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>4年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR4_INSPECTCODE") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>4年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR4_INSPECTNAME") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>4年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR4_ENFORCEPLACE") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>4年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR4_INSPECTVENDORNAME") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>8年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR8_AFTER") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>8年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR8_INSPECTYMD") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>8年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR8_INSPECTCODE") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>8年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR8_INSPECTNAME") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>8年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR8_ENFORCEPLACE") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>8年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR8_INSPECTVENDORNAME") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>12年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR12_AFTER") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>12年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR12_INSPECTYMD") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>12年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR12_INSPECTCODE") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>12年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR12_INSPECTNAME") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>12年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR12_ENFORCEPLACE") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>12年点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "YEAR12_INSPECTVENDORNAME") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>追加点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "ADD_YEAR") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>追加点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "ADD_INSPECTYMD") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>追加点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "ADD_INSPECTCODE") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>追加点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "ADD_INSPECTNAME") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>追加点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "ADD_ENFORCEPLACE") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>追加点検</HeaderTemplate>
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container.DataItem, "ADD_INSPECTVENDORNAME") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <!-- 検査登録ダイアログ -->
    <div id="pnlInspectDialogWrapper">
        <asp:HiddenField ID="hdnShowPnlInspectDialog" runat="server" Value="0" />
        <asp:Panel ID="pnlInspectDialogArea" runat="server">
            <div id="divInspectDialogHead">
                <div class="title1">検査登録</div>
            </div>
            <div id="divInspectDialogBody">
                <div class="tblDialogHeadWrapper">
                    <asp:GridView ID="gvDialogHead" runat="server" CssClass="tbl"
                        AllowPaging="false" AutoGenerateColumns="false" ShowHeader="true" GridLines="None"
                        ShowHeaderWhenEmpty="false" Visible="true" ShowFooter="false" ClientIDMode="Predictable"
                        OnDataBound="gvDialogHead_DataBound">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>記号</HeaderTemplate>
                                <ItemTemplate>
                                    <%# DataBinder.Eval(Container.DataItem, "CTNTYPE") %>
                                    <asp:HiddenField ID="LINECNT" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "LINECNT") %>' />
                                </ItemTemplate>
                                <HeaderStyle CssClass="w69px" />
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>番号</HeaderTemplate>
                                <ItemTemplate>
                                    <%# DataBinder.Eval(Container.DataItem, "CTNNO") %>
                                </ItemTemplate>
                                <HeaderStyle CssClass="w70px" />
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>コンテナ番号</HeaderTemplate>
                                <ItemTemplate>
                                    <%# DataBinder.Eval(Container.DataItem, "CONTNUM") %>
                                </ItemTemplate>
                                <HeaderStyle CssClass="w110px" />
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>製造年月</HeaderTemplate>
                                <ItemTemplate>
                                    <%# CDate(DataBinder.Eval(Container.DataItem, "CONTRUCTIONYM")).ToString("yyyy年MM月") %>
                                </ItemTemplate>
                                <HeaderStyle CssClass="w100px" />
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>現在駅</HeaderTemplate>
                                <ItemTemplate>
                                    <%# DataBinder.Eval(Container.DataItem, "ARRSTATIONNAME") %>
                                </ItemTemplate>
                                <HeaderStyle CssClass="w100px" />
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>交番検査<BR>前回実施日</HeaderTemplate>
                                <ItemTemplate>
                                    <%# DataBinder.Eval(Container.DataItem, "TRAINSBEFORERUNYMD") %>
                                </ItemTemplate>
                                <HeaderStyle CssClass="w100px" />
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>交番検査<BR>次回実施日</HeaderTemplate>
                                <ItemTemplate>
                                    <%# DataBinder.Eval(Container.DataItem, "TRAINSNEXTRUNYMD") %>
                                </ItemTemplate>
                                <HeaderStyle CssClass="w100px" />
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <div class="tblDialogRegularInspectsWrapper">
                    <asp:GridView ID="gvDialogRegularInspects" runat="server" CssClass="tbl inspect"
                        AllowPaging="false" AutoGenerateColumns="false" ShowHeader="true" GridLines="None"
                        ShowHeaderWhenEmpty="false" Visible="true" ShowFooter="false" ClientIDMode="Predictable"
                        OnPreRender="gvDialogRegularInspects_PreRender"
                        OnDataBound="gvDialogRegularInspects_DataBound">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate></HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="BTN_ADD" runat="server" Text="＋" UseSubmitBehavior="false" />
                                </ItemTemplate>
                                <HeaderStyle CssClass="hidden" />
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>定期検査</HeaderTemplate>
                                <ItemTemplate>
                                    <%# DataBinder.Eval(Container.DataItem, "INSPECTSEQ") %>
                                    <asp:HiddenField ID="LINECNT" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "LINECNT") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate></HeaderTemplate>
                                <ItemTemplate>
                                    <%# DataBinder.Eval(Container.DataItem, "INSPECTYEAR") %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>検査日</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox ID="INSPECTYMD" runat="server" TextMode="Date"
                                        Text='<%# DataBinder.Eval(Container.DataItem, "INSPECTYMD") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>種別</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:DropDownList ID="INSPECTCODE" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>種別名</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox ID="INSPECTNAME" runat="server" MaxLength="20"
                                        Text='<%# DataBinder.Eval(Container.DataItem, "INSPECTNAME") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>実施場所</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox ID="ENFORCEPLACE" runat="server" MaxLength="20"
                                        Text='<%# DataBinder.Eval(Container.DataItem, "ENFORCEPLACE") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>点検修理者</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:DropDownList ID="INSPECTVENDOR" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate></HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="BTN_DEL" runat="server" Text="－" UseSubmitBehavior="false" />
                                </ItemTemplate>
                                <HeaderStyle CssClass="hidden" />
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <div class="tblDialogAdditionInspectsWrapper">
                    <asp:GridView ID="gvDialogAdditionInspects" runat="server" CssClass="tbl inspect"
                        AllowPaging="false" AutoGenerateColumns="false" ShowHeader="true" GridLines="None"
                        ShowHeaderWhenEmpty="false" Visible="true" ShowFooter="false" ClientIDMode="Predictable"
                        OnPreRender="gvDialogAdditionInspects_PreRender"
                        OnDataBound="gvDialogAdditionInspects_DataBound">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate></HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="BTN_ADD" runat="server" Text="＋" UseSubmitBehavior="false" />
                                </ItemTemplate>
                                <HeaderStyle CssClass="hidden" />
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>追加検査</HeaderTemplate>
                                <ItemTemplate>
                                    <%# DataBinder.Eval(Container.DataItem, "R_INSPECTSEQ") %>
                                    <asp:HiddenField ID="LINECNT" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "LINECNT") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate></HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox ID="INSPECTYEAR" runat="server" MaxLength="4"
                                        Text='<%# If(CInt(DataBinder.Eval(Container.DataItem, "INSPECTYEAR")) = 0, "", DataBinder.Eval(Container.DataItem, "INSPECTYEAR")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>検査日</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox ID="INSPECTYMD" runat="server" TextMode="Date"
                                        Text='<%# DataBinder.Eval(Container.DataItem, "INSPECTYMD") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>種別</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:DropDownList ID="INSPECTCODE" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>種別名</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox ID="INSPECTNAME" runat="server" MaxLength="20"
                                        Text='<%# DataBinder.Eval(Container.DataItem, "INSPECTNAME") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>実施場所</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:TextBox ID="ENFORCEPLACE" runat="server" MaxLength="20"
                                        Text='<%# DataBinder.Eval(Container.DataItem, "ENFORCEPLACE") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>点検修理者</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:DropDownList ID="INSPECTVENDOR" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate></HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="BTN_DEL" runat="server" Text="－" UseSubmitBehavior="false" />
                                </ItemTemplate>
                                <HeaderStyle CssClass="hidden" />
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <div id="divValidateMessage">
                    <asp:TextBox ID="txtValidateMessage" runat="server" TextMode="MultiLine"
                        Rows="3" Width="100%" ReadOnly="true" />
                </div>
            </div>
            <div id="divInspectDialogFooter">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <asp:Button ID="WF_INSPECT_UPDATE" runat="server" CssClass="btn-sticky" Text="更新"
                            UseSubmitBehavior="False" OnClientClick="btnInspectDialogUpdateClick();" />
                        <input type="button" class="btn-sticky" value="キャンセル"
                            onclick="btnInspectDialogCancelClick();" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <!-- GridView DBクリック-->
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" />
        <!-- GridView表示位置フィールド -->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server" />

        <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD" runat="server" value="" type="text" />
        <!-- Textbox(Repeater) DBクリックフィールド -->
        <input id="WF_FIELD_REP" runat="server" value="" type="text" />
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_DelInspectRowIndex" runat="server" value="" type="text" />
        <!-- Textbox 駅マスタ(json) -->
        <input id="WF_StationTable" runat="server" value="" type="text" />
        <!-- Textbox 検査コード(json) -->
        <input id="WF_InspectCodes" runat="server" value="" type="text" />

        <!-- 画面表示切替 -->
        <input id="WF_DISP" runat="server" value="" type="text" />
        <!-- LeftBox Mview切替 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
        <!-- LeftBox 開閉 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
        <!-- Rightbox Mview切替 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />
        <!-- Rightbox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />

        <!-- 一覧・詳細画面切替用フラグ -->
        <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />

        <!-- ボタン押下 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />
        <!-- 権限 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
        <!-- 画面ボタン制御 -->
        <input id="WF_MAPButtonControl" runat="server" value="0" type="text" />
        <!-- DetailBox Mview切替 -->
        <input id="WF_DTAB_CHANGE_NO" runat="server" value="" type="text"/>
        <!-- ヘッダーを表示するか保持、"1"(表示:初期値),"0"(非表示)  -->
        <asp:HiddenField ID="hdnDispHeaderItems" runat="server" Value="1" />
        <!-- 選択(チェックボックス)押下フラグ(True:有効, False：無効) -->
        <input id="WF_CheckBoxFLG" runat="server" value="" type="text" />
        <!-- Textbox Print URL -->
        <input id="WF_PrintURL1" runat="server" value="" type="text" />
        <input id="WF_PrintURL2" runat="server" value="" type="text" />
        <input id="WF_PrintURL3" runat="server" value="" type="text" />
        <input id="WF_PrintURL4" runat="server" value="" type="text" />
    </div>
</asp:Content>
