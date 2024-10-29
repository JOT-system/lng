<%@ Page Title="LNM0007D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0007Rect1mDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0007Rect1mDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0007WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0007DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0007D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0007D.js")%>'></script>
</asp:Content>
 
<asp:Content ID="LNM0007D" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
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
                    <!-- 発駅コード -->
                    <span>
                        <asp:Label ID="WF_DEPSTATION_LABEL" runat="server" Text="発駅コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span id="TxtDepStationEvent" ondblclick="Field_DBclick('TxtDepStation', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtDepStation');">
                            <asp:TextBox ID="TxtDepStation" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDepStationName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 発受託人コード -->
                    <span>
                        <asp:Label ID="WF_DEPTRUSTEECD_LABEL" runat="server" Text="発受託人コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span id="TxtDepTrusteeCdEvent" ondblclick="Field_DBclick('TxtDepTrusteeCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtDepTrusteeCd');">
                            <asp:TextBox ID="TxtDepTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="5"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDepTrusteeCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- 優先順位 -->
                    <span>
                        <asp:Label ID="WF_PRIORITYNO_L" runat="server" Text="優先順位" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtPriorityNo" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 使用目的 -->
                    <span class="colCodeOnly">
                        <asp:Label ID="WF_PURPOSE_L" runat="server" Text="使用目的" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPurpose" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="42"></asp:TextBox>
                    </span>
                </p>
                
                <asp:Panel ID="WF_SELECTITEM_PANEL" runat="server">
                    <asp:Label ID="WF_SELECTITEM_L" runat="server" Text="【 選択比較項目 】" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <div id="detailkeybox1">
                        <p id="KEY_LINE_11">
                            <!-- 小分類コード -->
                            <span>
                                <asp:Label ID="WF_SMALLCTNCD_L" runat="server" Text="小分類コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSmallCTNCD', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtSmallCTNCD');">
                                    <asp:TextBox ID="TxtSmallCTNCD" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSmallCTNCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_12">
                            <!-- コンテナ記号 -->
                            <span>
                                <asp:Label ID="WF_CTNTYPE_L" runat="server" Text="コンテナ記号" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtCTNType', <%=LIST_BOX_CLASSIFICATION.LC_RECONM%>);" onchange="TextBox_change('TxtCTNType');">
                                    <asp:TextBox ID="TxtCTNType" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblCTNTypeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_13">
                            <!-- コンテナ番号（開始） -->
                            <span>
                                <asp:Label ID="WF_CTNSTNO_L" runat="server" Text="コンテナ番号（開始）" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtCTNStNo', <%=LIST_BOX_CLASSIFICATION.LC_RECONM%>);" onchange="TextBox_change('TxtCTNStNo');">
                                    <asp:TextBox ID="TxtCTNStNo" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon"  onblur="MsgClear();" MaxLength="8"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblCTNStNoName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                            <!-- コンテナ番号（終了） -->
                            <span>
                                <asp:Label ID="WF_CTNENDNO_L" runat="server" Text="コンテナ番号（終了）" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtCTNEndNo', <%=LIST_BOX_CLASSIFICATION.LC_RECONM%>);" onchange="TextBox_change('TxtCTNEndNo');">
                                    <asp:TextBox ID="TxtCTNEndNo" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon"  onblur="MsgClear();" MaxLength="8"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblCTNEndNoName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_14">
                            <!-- 積空区分 -->
                            <span>
                                <asp:Label ID="WF_SLCSTACKFREEKBN_L" runat="server" Text="積空区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcStackFreeKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcStackFreeKbn');">
                                    <asp:TextBox ID="TxtSlcStackFreeKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcStackFreeKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_15">
                            <!-- 状態区分 -->
                            <span>
                                <asp:Label ID="WF_SLCSTATUSKBN_L" runat="server" Text="状態区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcStatusKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcStatusKbn');">
                                    <asp:TextBox ID="TxtSlcStatusKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcStatusKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_16">
                            <!-- 発受託人サブコード -->
                            <span>
                                <asp:Label ID="WF_SLCDEPTRUSTEESUBCD_LABEL" runat="server" Text="発受託人サブコード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcDepTrusteeSubCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtSlcDepTrusteeSubCd');">
                                    <asp:TextBox ID="TxtSlcDepTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcDepTrusteeSubCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_17">
                            <!-- 発荷主コード -->
                            <span>
                                <asp:Label ID="WF_SLCDEPSHIPPERCD_L" runat="server" Text="発荷主コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcDepShipperCd', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPER%>);" onchange="TextBox_change('TxtSlcDepShipperCd');">
                                    <asp:TextBox ID="TxtSlcDepShipperCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcDepShipperCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_18">
                            <!-- 着駅コード -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION_L" runat="server" Text="着駅コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation');">
                                    <asp:TextBox ID="TxtSlcArrStation" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStationName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_19">
                            <!-- 着受託人コード -->
                            <span>
                                <asp:Label ID="WF_SLCARRTRUSTEECD_L" runat="server" Text="着受託人コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrTrusteeCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtSlcArrTrusteeCd');">
                                    <asp:TextBox ID="TxtSlcArrTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrTrusteeCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_20">
                            <!-- 着受託人サブコード -->
                            <span>
                                <asp:Label ID="WF_SLCARRTRUSTEESUBCD_L" runat="server" Text="着受託人サブコード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrTrusteeSubCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtSlcArrTrusteeSubCd');">
                                    <asp:TextBox ID="TxtSlcArrTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrTrusteeSubCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_21">
                            <!-- ＪＲ品目コード -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD_L" runat="server" Text="ＪＲ品目コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJRItemCd', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJRItemCd');">
                                    <asp:TextBox ID="TxtSlcJRItemCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJRItemCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_22">
                            <!-- 集荷先電話番号 -->
                            <span>
                                <asp:Label ID="WF_SLCPICKUPTEL_L" runat="server" Text="集荷先電話番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSlcPickUpTel" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="12"></asp:TextBox>
                            </span>
                        </p>
                    </div>
                </asp:Panel>
                <asp:Panel ID="WF_EXCEPTION_PANEL" runat="server">
                    <asp:Label ID="WF_EXCEPTION_L" runat="server" Text="【 特例置換項目 】" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <div id="detailkeybox2">
                        <p id="KEY_LINE_23">
                            <!-- 発受託人コード -->
                            <span>
                                <asp:Label ID="WF_SPRDEPTRUSTEECD_L" runat="server" Text="発受託人コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprDepTrusteeCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtSprDepTrusteeCd');">
                                    <asp:TextBox ID="TxtSprDepTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprDepTrusteeCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_24">
                            <!-- 発受託人サブコード -->
                            <span>
                                <asp:Label ID="WF_SPRDEPTRUSTEESUBCD_L" runat="server" Text="発受託人サブコード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprDepTrusteeSubCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtSprDepTrusteeSubCd');">
                                    <asp:TextBox ID="TxtSprDepTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprDepTrusteeSubCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                            <!-- 発受託人サブゼロ変換区分 -->
                            <span>
                                <asp:Label ID="WF_SPRDEPTRUSTEESUBZKBN_L" runat="server" Text="発受託人サブゼロ変換区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprDerTrusteeSubZKbn" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                                <asp:Label ID="LblSprDerTrusteeSubZKbn" Text="（ 1:発受託人サブをゼロに変換）" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_26">
                            <!-- 発荷主コード -->
                            <span>
                                <asp:Label ID="WF_SPRDEPSHIPPERCD_L" runat="server" Text="発荷主コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprDepShipperCd', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPER%>);" onchange="TextBox_change('TxtSprDepShipperCd');">
                                    <asp:TextBox ID="TxtSprDepShipperCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprDepShipperCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_27">
                            <!-- 着受託人コード -->
                            <span>
                                <asp:Label ID="WF_SPRARRTRUSTEECD_L" runat="server" Text="着受託人コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprArrTrusteeCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtSprArrTrusteeCd');">
                                    <asp:TextBox ID="TxtSprArrTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprArrTrusteeCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_28">
                            <!-- 着受託人サブコード -->
                            <span>
                                <asp:Label ID="WF_SPRARRTRUSTEESUBCD_L" runat="server" Text="着受託人サブコード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprArrTrusteeSubCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtSprArrTrusteeSubCd');">
                                    <asp:TextBox ID="TxtSprArrTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprArrTrusteeSubCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                            <!-- 着受託人サブゼロ変換区分 -->
                            <span>
                                <asp:Label ID="WF_SPRARRTRUSTEESUBZKBN_L" runat="server" Text="着受託人サブゼロ変換区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprArrTrusteeSubZKbn" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                                <asp:Label ID="LblSprArrTrusteeSubZKbn" Text="（ 1:着受託人サブをゼロに変換）" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_30">
                            <!-- ＪＲ品目コード -->
                            <span>
                                <asp:Label ID="WF_SPRJRITEMCD_L" runat="server" Text="ＪＲ品目コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprJRItemCd', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSprJRItemCd');">
                                    <asp:TextBox ID="TxtSprJRItemCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprJRItemCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_31">
                            <!-- 積空区分 -->
                            <span>
                                <asp:Label ID="WF_SPRSTACKFREEKBN_L" runat="server" Text="積空区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprStackFreeKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprStackFreeKbn');">
                                    <asp:TextBox ID="TxtSprStackFreeKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprStackFreeKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_32">
                            <!-- 状態区分 -->
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

        <!-- multiSelect レイアウト -->
        <!-- 駅単一選択 -->
        <MSINC:multiselect runat="server" id="mspStationSingle" />

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
