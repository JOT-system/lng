<%@ Page Title="LNM0021D" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNM0021ItemDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0021ItemDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0021WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNM0003DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0021D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0021D.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>
 
<asp:Content ID="LNM0021D" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox">
            <div id="detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_UPDATE" class="btn-sticky" value="更新" onclick="ButtonClick('WF_UPDATE');" />
                        <input type="button" id="WF_CLEAR" class="btn-sticky" value="戻る"  onclick="ButtonClick('WF_CLEAR');" />
                    </div>
                </div>
            </div>

            <div id="detailkeybox">

                <p id="KEY_LINE_1">
                    <!-- 選択No -->
                    <span>
                        <asp:Label ID="WF_SEL_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:Label ID="LblSelLineCNT" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_2">
                    <!-- 削除フラグ -->
                    <span class="ef" id="WF_DELFLG">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtDelFlg', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('TxtDelFlg');">
                            <asp:TextBox ID="TxtDelFlg" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDelFlgName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_3">
                    <!-- 品目コード -->
                    <span>
                        <asp:Label ID="WF_ITEMCD_L" runat="server" Text="品目コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtItemCd" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>
                    </span>
                </p>
                <p id="KEY_LINE_4">
                    <!-- 品目名称 -->
                    <span>
                        <asp:Label ID="WF_ITEMNAME_L" runat="server" Text="品目名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtName" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <!-- 画面レイアウト位置調整用ラベル -->
                        <asp:Label ID="Label1" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 品目名称（短） -->
                    <span>
                        <asp:Label ID="WF_ITEMNAMES_L" runat="server" Text="品目名称（短）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtNames" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="20"></asp:TextBox>
                        <!-- 画面レイアウト位置調整用ラベル -->
                        <asp:Label ID="Label2" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_5">
                    <!-- 品目カナ名称 -->
                    <span>
                        <asp:Label ID="WF_ITEMNAMEKANA_L" runat="server" Text="品目カナ名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtNameKana" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="50"></asp:TextBox>
                        <!-- 画面レイアウト位置調整用ラベル -->
                        <asp:Label ID="Label3" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 品目カナ名称（短） -->
                    <span>
                        <asp:Label ID="WF_ITEMNAMEKANAS_L" runat="server" Text="品目カナ名称（短）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtNameKanas" runat="server" CssClass="WF_TEXTBOX_CSS " MaxLength="20"></asp:TextBox>
                        <!-- 画面レイアウト位置調整用ラベル -->
                        <asp:Label ID="Label4" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 特大分類コード -->
                    <span>
                        <asp:Label ID="WF_SPBIGCATEGCD_L" runat="server" Text="特大分類コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtSpBigCategCd', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSpBigCategCd');">
                            <asp:TextBox ID="TxtSpBigCategCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblSpBigCategCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 大分類コード -->
                    <span>
                        <asp:Label ID="WF_BIGCATEGCD_L" runat="server" Text="大分類コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtBigCategCd', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtBigCategCd');">
                            <asp:TextBox ID="TxtBigCategCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblBigCategCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 中分類コード -->
                    <span>
                        <asp:Label ID="WF_MIDDLECATEGCD_L" runat="server" Text="中分類コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtMiddleCategCd', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtMiddleCategCd');">
                            <asp:TextBox ID="TxtMiddleCategCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblMiddleCategCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- 小分類コード -->
                    <span>
                        <asp:Label ID="WF_SMALLCATEGCD_L" runat="server" Text="小分類コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtSmallCategCd', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSmallCategCd');">
                            <asp:TextBox ID="TxtSmallCategCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblSmallCategCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 危険品区分 -->
                    <span>
                        <asp:Label ID="WF_DANGERKBN_L" runat="server" Text="危険品区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtDangerKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtDangerKbn');">
                            <asp:TextBox ID="TxtDangerKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDangerKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 軽量品区分 -->
                    <span>
                        <asp:Label ID="WF_LIGHTWTKBN_L" runat="server" Text="軽量品区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtLightWtKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtLightWtKbn');">
                            <asp:TextBox ID="TxtLightWtKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblLightWtKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_12">
                    <!-- 貴重品区分 -->
                    <span>
                        <asp:Label ID="WF_VALUABLEKBN_L" runat="server" Text="貴重品区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtValuableKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtValuableKbn');">
                            <asp:TextBox ID="TxtValuableKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblValuableKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_13">
                    <!-- 冷蔵適合フラグ -->
                    <span>
                        <asp:Label ID="WF_REFRIGERATIONFLG_L" runat="server" Text="冷蔵適合フラグ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtRefrigerationFlg', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtRefrigerationFlg');">
                            <asp:TextBox ID="TxtRefrigerationFlg" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblRefrigerationFlgName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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
        <div style="display:none;">

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

