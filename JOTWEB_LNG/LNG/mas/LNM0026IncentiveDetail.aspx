<%@ Page Title="LNM0026D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0026IncentiveDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0026IncentiveDetail"%>

<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0026WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNM0026DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0026D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0026D.js")%>'></script>
</asp:Content>

<asp:Content ID="LNM0026D" ContentPlaceHolderID="contents1" runat="server">
    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
    <!-- 全体レイアウト　detailbox -->
    <div class="detailboxOnly" id="detailbox">
        <div id="detailbuttonbox" class="detailbuttonbox">
            <div class="actionButtonBox">
                <div class="rightSide">
                    <input type="button" id="WF_ButtonUPDATE" class="btn-sticky" value="更新" onclick="ButtonClick('WF_ButtonUPDATE');" />
                    <input type="button" id="WF_ButtonCLEAR" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonCLEAR');" />
                </div>
            </div>
        </div>

        <div id="detailkeybox">
            <p id="KEY_LINE_1" class="flexible">
                <!-- 選択No -->
                <span>
                    <asp:Label ID="WF_SEL_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:Label ID="lblSelLineCNT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </span>
            </p>
            <p id="KEY_LINE_2">
                <!-- 削除フラグ -->
                <span>
                    <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span ondblclick="Field_DBclick('txtDelFlg', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('txtDelFlg');">
                        <asp:TextBox ID="txtDelFlg" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblDelFlgName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_3" class="flexible">
                <!-- 画面ＩＤ -->
                <span style="display: none;">
                    <asp:Label ID="WF_MAPID_L" runat="server" Text="画面ＩＤ" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="txtMapId" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </span>
            </p>

            <p id="KEY_LINE_4">
                <!-- 取引先コード -->
                <span>
                    <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="txtToriCodeEvent" ondblclick="Field_DBclick('txtToriCode', <%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);" onchange="TextBox_change('txtToriCode');">
                        <asp:TextBox ID="txtToriCode" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="10"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblToriCodeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_5">
                <!-- 発駅コード -->
                <span>
                    <asp:Label ID="WF_DEPSTATION_L" runat="server" Text="発駅コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="txtDepStationEvent" ondblclick="Field_DBclick('txtDepStation', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('txtDepStation');">
                        <asp:TextBox ID="txtDepStation" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="6"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblDepStationName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_6">
                <!-- ボリュームインセンティブ料金 -->
                <span>
                    <asp:Label ID="WF_VOLINCENTAMO_L" runat="server" Text="ボリュームインセンティブ料金" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtVolIncentAmo" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="9"></asp:TextBox>
                    <asp:Label ID="lblVolIncentAmo" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

        </div>
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview ID="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview ID="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist ID="work" runat="server" />

    <!-- イベント用 -->
    <div style="display: none;">

        <!-- 入力不可制御項目 -->
        <input id="DisabledKeyItem" runat="server" value="" type="text" />

        <!-- GridView DBクリック-->
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
        <!-- GridView表示位置フィールド -->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>

        <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD" runat="server" value="" type="text" />
        <!-- Textbox(Repeater) DBクリックフィールド -->
        <input id="WF_FIELD_REP" runat="server" value="" type="text" />
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_SELectedIndex" runat="server" value="" type="text" />

        <!-- LeftBox Mview切替 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />
        <!-- LeftBox 開閉 -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />
        <!-- Rightbox Mview切替 -->
        <input id="WF_RightViewChange" runat="server" value="" type="text" />
        <!-- Rightbox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />

        <!-- Textbox Print URL -->
        <input id="WF_PrintURL" runat="server" value="" type="text" />

        <!-- 一覧・詳細画面切替用フラグ -->
        <input id="WF_BOXChange" runat="server" value="headerbox" type="text" />

        <!-- ボタン押下 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />
        <!-- 権限 -->
        <input id="WF_MAPpermitcode" runat="server" value="" type="text" />
    </div>

</asp:Content>
