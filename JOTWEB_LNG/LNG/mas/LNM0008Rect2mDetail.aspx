<%@ Page Title="LNM0008D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0008Rect2mDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0008Rect2mDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0008WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNM0008DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0008D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0008D.js")%>'></script>
</asp:Content>
 
<asp:Content ID="LNM0008D" ContentPlaceHolderID="contents1" runat="server">
        <!-- 全体レイアウト　detailbox -->
        <div class="detailboxOnly" id="detailbox" >
            <div id="detailbuttonbox" class="detailbuttonbox">
                <div class="actionButtonBox">
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonUPDATE" class="btn-sticky" value="更新" onclick="ButtonClick('WF_ButtonUPDATE');" />
                        <input type="button" id="WF_ButtonCLEAR" class="btn-sticky" value="戻る"  onclick="ButtonClick('WF_ButtonCLEAR');" />
                    </div>
                </div>
            </div>

            <div id="detailkeybox">
                <p id="KEY_LINE_1" class="flexible">
                    <!-- 選択No -->
                    <span>
                        <asp:Label ID="WF_SEL_LINECNT_L" runat="server" Text="選択No" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:Label ID="LblSelLineCNT" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_2">
                    <!-- 削除フラグ -->
                    <span>
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtDelFlg', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('TxtDelFlg');">
                            <asp:TextBox ID="TxtDelFlg" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDelFlgName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_3" class="flexible">
                    <!-- 画面ＩＤ -->
                    <span style="display:none;">
                        <asp:Label ID="WF_MAPID_L" runat="server" Text="画面ＩＤ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtMapId" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 組織コード -->
                    <span class="ef">
                        <asp:Label ID="WF_ORG_L" runat="server" Text="組織コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span id="TxtOrgCodeEvent" ondblclick="Field_DBclick('TxtOrgCode', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtOrgCode');">
                            <asp:TextBox ID="TxtOrgCode" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblOrgName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 大分類コード -->
                    <span>
                        <asp:Label ID="WF_BIGCTNCD_L" runat="server" Text="大分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span id="TxtBigCTNCDEvent" ondblclick="Field_DBclick('TxtBigCTNCD', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtBigCTNCD');">
                            <asp:TextBox ID="TxtBigCTNCD" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblBigCTNCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 中分類コード -->
                    <span>
                        <asp:Label ID="WF_MIDDLECTNCD_L" runat="server" Text="中分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span id="TxtMiddleCTNCDEvent" ondblclick="Field_DBclick('TxtMiddleCTNCD', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtMiddleCTNCD');">
                            <asp:TextBox ID="TxtMiddleCTNCD" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblMiddleCTNCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 使用目的 -->
                    <span class="colCodeOnly">
                        <asp:Label ID="WF_PURPOSE_L" runat="server" Text="使用目的" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPurpose" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="42"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 積空区分 -->
                    <span>
                        <asp:Label ID="WF_STACKFREEKBN_L" runat="server" Text="積空区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtStackFreeKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtStackFreeKbn');">
                            <asp:TextBox ID="TxtStackFreeKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblStackFreeKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <asp:Panel ID="WF_EXCEPTION_PANEL" runat="server">
                    <asp:Label ID="WF_EXCEPTION_L" runat="server" Text="【 特例置換項目 】" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <div id="detailkeybox1">
                        <p id="KEY_LINE_9">
                            <!-- 特例置換項目-発受託人コード -->
                            <span>
                                <asp:Label ID="WF_SPRDEPTRUSTEECD_L" runat="server" Text="発受託人コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprDepTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                                <asp:Label ID="Label1" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                            <!-- 特例置換項目-発受託人サブコード -->
                            <span>
                                <asp:Label ID="WF_SPRDEPTRUSTEESUBCD_L" runat="server" Text="発受託人サブコード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprDepTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                                <asp:Label ID="Label2" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_10">
                            <!-- 特例置換項目-発受託人サブゼロ変換区分 -->
                            <span>
                                <asp:Label ID="WF_SPRDEPTRUSTEESUBZKBN_L" runat="server" Text="発受託人サブゼロ変換区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprDerTrusteeSubZKbn" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                                <asp:Label ID="LblSprDerTrusteeSubZKbn" Text="（ 1:発受託人サブをゼロに変換）" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_11">
                            <!-- 特例置換項目-発荷主コード -->
                            <span>
                                <asp:Label ID="WF_SPRDEPSHIPPERCD_L" runat="server" Text="発荷主コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprDepShipperCd', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPER%>);" onchange="TextBox_change('TxtSprDepShipperCd');">
                                    <asp:TextBox ID="TxtSprDepShipperCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprDepShipperCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_12">
                            <!-- 特例置換項目-着受託人コード -->
                            <span>
                                <asp:Label ID="WF_SPRARRTRUSTEECD_L" runat="server" Text="着受託人コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprArrTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                                <asp:Label ID="Label3" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                            <!-- 特例置換項目-着受託人サブコード -->
                            <span>
                                <asp:Label ID="WF_SPRARRTRUSTEESUBCD_L" runat="server" Text="着受託人サブコード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprArrTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                                <asp:Label ID="Label4" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_13">
                            <!-- 特例置換項目-着受託人サブゼロ変換区分 -->
                            <span>
                                <asp:Label ID="WF_SPRARRTRUSTEESUBZKBN_L" runat="server" Text="着受託人サブゼロ変換区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprArrTrusteeSubZKbn" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                                <asp:Label ID="LblSprArrTrusteeSubZKbn" Text="（ 1:着受託人サブをゼロに変換）" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_14">
                            <!-- 特例置換項目-ＪＲ品目コード -->
                            <span>
                                <asp:Label ID="WF_SPRJRITEMCD_L" runat="server" Text="ＪＲ品目コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprJRItemCd', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSprJRItemCd');">
                                    <asp:TextBox ID="TxtSprJRItemCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprJRItemCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_15">
                            <!-- 特例置換項目-積空区分 -->
                            <span>
                                <asp:Label ID="WF_SPRSTACKFREEKBN_L" runat="server" Text="積空区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprStackFreeKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprStackFreeKbn');">
                                    <asp:TextBox ID="TxtSprStackFreeKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprStackFreeKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_16">
                            <!-- 特例置換項目-状態区分 -->
                            <span>
                                <asp:Label ID="WF_SPRSTATUSKBN_L" runat="server" Text="状態区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprStatusKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprStatusKbn');">
                                    <asp:TextBox ID="TxtSprStatusKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprStatusKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                    </div>
                </asp:Panel>
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

            <!-- 縦スクロール位置 -->
            <input id="WF_scrollY" runat="server" value="0" type="text" />
        </div>
 
</asp:Content>
