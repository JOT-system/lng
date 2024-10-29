<%@ Page Title="LNT0001S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0001OrderSearch.aspx.vb" Inherits="JOTWEB_LNG.LNT0001OrderSearch" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content id="LNT0001SH" contentplaceholderid="head" runat="server">
    <!-- <link href='<%=ResolveUrl("~/LNG/css/LNT0001S.css")%>' rel="stylesheet" type="text/css" /> -->
    <!-- <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0001S.js")%>'></script> -->
</asp:Content>

<asp:Content ID="LNT0001S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonDO" class="btn-sticky" value="検索"  onclick="ButtonClick('WF_ButtonDO');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox2">
            <!-- 会社コード -->
            <div class="inputItem2" style="display:none;">
                <a>会社コード</a>
                <a class="ef" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 年月日(発送日)From -->
            <div class="inputItem2">
                <a id="WF_DATE_LABEL" class="requiredMark">発送日From</a>
                <a class="ef" id="WF_DATE" ondblclick="Field_DBclick('TxtDateStart', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtDateStart" runat="server" CssClass="calendarIcon" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a id="WF_DATE_SYMBOL"><span>&nbsp;～&nbsp;</span>発送日To</a>
                <!-- 年月日(発送日)To -->
                <a class="ef" id="WF_DATE_TO" ondblclick="Field_DBclick('TxtDateEnd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtDateEnd" runat="server" CssClass="calendarIcon" onblur="MsgClear();"></asp:TextBox>
                </a>
            </div>
            <!-- 所管部 -->
            <div class="inputItem2">
                <a id="WF_JURISDICTIONCD_LABEL">所管部</a>
                <a class="ef" id="WF_JURISDICTIONCD" ondblclick="Field_DBclick('TxtJurisdictionCd', <%=LIST_BOX_CLASSIFICATION.LC_JURISDICTION%>);" onchange="TextBox_change('TxtJurisdictionCd');">
                    <asp:TextBox ID="TxtJurisdictionCd" runat="server"  CssClass="boxIcon" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                </a>
                <a id="WF_JURISDICTIONCD_TEXT" >
                    <asp:Label ID="LblJurisdictionName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- JOT発店所 -->
            <div class="inputItem2">
                <a id="WF_JOTDEPBRANCHCD_LABEL">JOT発店所</a>
                <a class="ef" id="WF_JOTDEPBRANCHCD" ondblclick="Field_DBclick('TxtJotdepbranchCd', <%=LIST_BOX_CLASSIFICATION.LC_JOTDEPBRANCH%>);" onchange="TextBox_change('TxtJotdepbranchCd');">
                    <asp:TextBox ID="TxtJotdepbranchCd" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_JOTDEPBRANCHCD_TEXT" >
                    <asp:Label ID="LblJotdepbranchName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 積空区分 -->
            <div class="inputItem2">
                <a id="LblStackFreeKbn">積空区分</a><br/>&nbsp;
                <a id="WF_SW">
                    <asp:RadioButton ID="RdBStack" runat="server" GroupName="RdBStackFreeKbn" Text="&nbsp;&nbsp;積&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" />
                    <asp:RadioButton ID="RdBFree" runat="server" GroupName="RdBStackFreeKbn" Text="&nbsp;&nbsp;空&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" />
                </a>
            </div>
            <!-- 発駅コード -->
            <div class="inputItem2">
                <a id="WF_DEPSTATION_LABEL">発駅コード</a>
                <a class="ef" id="WF_DEPSTATION" ondblclick="Field_DBclick('TxtDepStation', <%=LIST_BOX_CLASSIFICATION.LC_DEPSTATION%>);" onchange="TextBox_change('TxtDepStation');">
                    <asp:TextBox ID="TxtDepStation" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_DEPSTATION_TEXT" >
                    <asp:Label ID="LblDepStation" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 発受託人 -->
            <div class="inputItem2">
                <a id="WF_DEPTRUSTEECD_LABEL">発受託人</a>
                <a class="ef" id="WF_DEPTRUSTEECD" ondblclick="Field_DBclick('TxtDepTrusteeCd', <%=LIST_BOX_CLASSIFICATION.LC_DEPTRUSTEECD%>);" onchange="TextBox_change('TxtDepTrusteeCd');">
                    <asp:TextBox ID="TxtDepTrusteeCd" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="5"></asp:TextBox>
                </a>
                <a id="WF_DEPTRUSTEECD_TEXT" >
                    <asp:Label ID="LblDepTrusteeCd" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- コンテナ記号 -->
            <div class="inputItem2">
                <a id="WF_CTNTYPE_LABEL">コンテナ記号</a>
                <a class="ef" id="WF_CTNTYPE" ondblclick="Field_DBclick('TxtCtnType', <%=LIST_BOX_CLASSIFICATION.LC_CTNTYPE%>);" onchange="TextBox_change('TxtCtnType');">
                    <asp:TextBox ID="TxtCtnType" runat="server"  CssClass="boxIcon" onblur="MsgClear();" MaxLength="5"></asp:TextBox>
                </a>
                <!-- コンテナ番号 -->
                <a id="WF_CTNNO_LABEL">&nbsp;&nbsp;&nbsp;&nbsp;コンテナ番号</a>
                <a class="ef" id="WF_CTNNO"  ondblclick="Field_DBclick('TxtCtnNo', <%=LIST_BOX_CLASSIFICATION.LC_CTNNO%>);" onchange="TextBox_change('TxtCtnNo');">
                    <asp:TextBox ID="TxtCtnNo" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="8"></asp:TextBox>
                </a>
            </div>
            <!-- 状態 -->
            <div class="inputItem2">
                <a id="WF_STATUS_LABEL">状態</a>
                <a class="ef" id="WF_STATUS" ondblclick="Field_DBclick('TxtStatus', <%=LIST_BOX_CLASSIFICATION.LC_ORDERSTATUS%>);" onchange="TextBox_change('TxtStatus');">
                    <asp:TextBox ID="TxtStatus" runat="server"  CssClass="boxIcon" onblur="MsgClear();" MaxLength="3"></asp:TextBox>
                </a>
                <a id="WF_STATUS_TEXT" >
                    <asp:Label ID="LblStatusName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 受注キャンセルフラグ -->
            <div class="inputItem2">
                <a id="WF_ORDERCANCELFLG">
                    <asp:CheckBox ID="ChkOrderCancelFlg" runat="server" Text="受注ｷｬﾝｾﾙを含む" />
                </a>
            </div>
            <!-- 対象外 -->
            <div class="inputItem2">
                <a id="WF_NOTSELFLG">
                    <asp:CheckBox ID="ChkNotSelFlg" runat="server" Text="対象外を含む" />
                </a>
            </div>
        </div>
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
    </div>
</asp:Content>

