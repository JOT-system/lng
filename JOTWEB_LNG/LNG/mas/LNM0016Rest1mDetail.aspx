<%@ Page Title="LNM0016D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0016Rest1mDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0016Rest1mDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0016WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0016DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0016D.css")%>' rel="stylesheet" type="text/css"/>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0016D.js")%>'></script>
</asp:Content>
 
<asp:Content ID="LNM0016D" ContentPlaceHolderID="contents1" runat="server">
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
                <p id="KEY_LINE_1">
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

                <p id="KEY_LINE_3">
                    <!-- 画面ＩＤ -->
                    <span style="display:none;">
                        <asp:Label ID="WF_MAPID_L" runat="server" Text="画面ＩＤ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtMapId" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 組織コード -->
                    <span>
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
                    <!-- 発受託人サブコード -->
                    <span>
                        <asp:Label ID="WF_DEPTRUSTEESUBCD_LABEL" runat="server" Text="発受託人サブコード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span id="TxtDepTrusteeSubCdEvent" ondblclick="Field_DBclick('TxtDepTrusteeSubCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtDepTrusteeSubCd');">
                            <asp:TextBox ID="TxtDepTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="3"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDepTrusteeSubCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 優先順位 -->
                    <span>
                        <asp:Label ID="WF_PRIORITYNO_L" runat="server" Text="優先順位" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtPriorityNo" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 使用目的 -->
                    <span class="colCodeOnly">
                        <asp:Label ID="WF_PURPOSE_L" runat="server" Text="使用目的" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPurpose" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="42"></asp:TextBox>
                    </span>
                </p>
                
                <asp:Panel ID="WF_SELECTITEM_PANEL" runat="server">
                    <asp:Label ID="WF_SELECTITEM_L" runat="server" Text="【 選択比較項目 】" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <div id="detailkeybox1">
                        
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
                            <!-- ＪＲ発支社支店コード -->
                            <span>
                                <asp:Label ID="WF_SLCJRDEPBRANCHCD_L" runat="server" Text="ＪＲ発支社支店コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrDepBranchCd', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcJrDepBranchCd');">
                                    <asp:TextBox ID="TxtSlcJrDepBranchCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrDepBranchCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_15">
                            <!-- 発荷主コード1 -->
                            <span>
                                <asp:Label ID="WF_SLCDEPSHIPPERCD1_L" runat="server" Text="発荷主コード1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcDepShipperCd1', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPER%>);" onchange="TextBox_change('TxtSlcDepShipperCd1');">
                                    <asp:TextBox ID="TxtSlcDepShipperCd1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcDepShipperCd1Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>

                            <!-- 発荷主ＣＤ比較条件 -->
                            <span>
                                <asp:Label ID="WF_SLCDEPSHIPPERCDCOND_L" runat="server" Text="発荷主ＣＤ比較条件" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcDepShipperCdCond', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcDepShipperCdCond');">
                                    <asp:TextBox ID="TxtSlcDepShipperCdCond" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcDepShipperCdCondName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_16">
                            <!-- 発荷主コード2 -->
                            <span>
                                <asp:Label ID="WF_SLCDEPSHIPPERCD2_L" runat="server" Text="発荷主コード2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcDepShipperCd2', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPER%>);" onchange="TextBox_change('TxtSlcDepShipperCd2');">
                                    <asp:TextBox ID="TxtSlcDepShipperCd2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcDepShipperCd2Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_17">
                            <!-- 発荷主コード3 -->
                            <span>
                                <asp:Label ID="WF_SLCDEPSHIPPERCD3_L" runat="server" Text="発荷主コード3" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcDepShipperCd3', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPER%>);" onchange="TextBox_change('TxtSlcDepShipperCd3');">
                                    <asp:TextBox ID="TxtSlcDepShipperCd3" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcDepShipperCd3Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_18">
                            <!-- 発荷主コード4 -->
                            <span>
                                <asp:Label ID="WF_SLCDEPSHIPPERCD4_L" runat="server" Text="発荷主コード4" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcDepShipperCd4', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPER%>);" onchange="TextBox_change('TxtSlcDepShipperCd4');">
                                    <asp:TextBox ID="TxtSlcDepShipperCd4" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcDepShipperCd4Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_19">
                            <!-- 発荷主コード5 -->
                            <span>
                                <asp:Label ID="WF_SLCDEPSHIPPERCD5_L" runat="server" Text="発荷主コード5" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcDepShipperCd5', <%=LIST_BOX_CLASSIFICATION.LC_SHIPPER%>);" onchange="TextBox_change('TxtSlcDepShipperCd5');">
                                    <asp:TextBox ID="TxtSlcDepShipperCd5" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcDepShipperCd5Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_20">
                            <!-- ＪＲ着支社支店コード -->
                            <span>
                                <asp:Label ID="WF_SLCJRARRBRANCHCD_L" runat="server" Text="ＪＲ着支社支店コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrArrBranchCd', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcJrArrBranchCd');">
                                    <asp:TextBox ID="TxtSlcJrArrBranchCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrArrBranchCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>

                            <!-- ＪＲ着支社支店ＣＤ比較 -->
                            <span>
                                <asp:Label ID="WF_SLCJRARRBRANCHCDCOND_L" runat="server" Text="ＪＲ着支社支店ＣＤ比較" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrArrBranchCdCond', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcJrArrBranchCdCond');">
                                    <asp:TextBox ID="TxtSlcJrArrBranchCdCond" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrArrBranchCdCondName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_21">
                            <!-- ＪＯＴ着組織コード -->
                            <span>
                                <asp:Label ID="WF_SLCJOTARRORGCODE_L" runat="server" Text="ＪＯＴ着組織コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJotArrOrgCode', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtSlcJotArrOrgCode');">
                                    <asp:TextBox ID="TxtSlcJotArrOrgCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJotArrOrgCodeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>

                            <!-- ＪＯＴ着組織ＣＤ比較 -->
                            <span>
                                <asp:Label ID="WF_SLCJOTARRORGCODECOND_L" runat="server" Text="ＪＯＴ着組織ＣＤ比較" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJotArrOrgCodeCond', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcJotArrOrgCodeCond');">
                                    <asp:TextBox ID="TxtSlcJotArrOrgCodeCond" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJotArrOrgCodeCondName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_22">
                            <!-- 着駅コード1 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION1_L" runat="server" Text="着駅コード1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation1', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation1');">
                                    <asp:TextBox ID="TxtSlcArrStation1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStation1Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>

                            <!-- 着駅コード比較条件 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATIONCOND_L" runat="server" Text="着駅コード比較条件" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStationCond', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcArrStationCond');">
                                    <asp:TextBox ID="TxtSlcArrStationCond" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStationCondName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_23">
                            <!-- 着駅コード2 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION2_L" runat="server" Text="着駅コード2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation2', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation2');">
                                    <asp:TextBox ID="TxtSlcArrStation2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStation2Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_24">
                            <!-- 着駅コード3 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION3_L" runat="server" Text="着駅コード3" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation3', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation3');">
                                    <asp:TextBox ID="TxtSlcArrStation3" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStation3Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_25">
                            <!-- 着駅コード4 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION4_L" runat="server" Text="着駅コード4" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation4', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation4');">
                                    <asp:TextBox ID="TxtSlcArrStation4" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStation4Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_26">
                            <!-- 着駅コード5 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION5_L" runat="server" Text="着駅コード5" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation5', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation5');">
                                    <asp:TextBox ID="TxtSlcArrStation5" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStation5Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_27">
                            <!-- 着駅コード6 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION6_L" runat="server" Text="着駅コード6" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation6', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation6');">
                                    <asp:TextBox ID="TxtSlcArrStation6" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStation6Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_28">
                            <!-- 着駅コード7 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION7_L" runat="server" Text="着駅コード7" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation7', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation7');">
                                    <asp:TextBox ID="TxtSlcArrStation7" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStation7Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_29">
                            <!-- 着駅コード8 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION8_L" runat="server" Text="着駅コード8" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation8', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation8');">
                                    <asp:TextBox ID="TxtSlcArrStation8" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStation8Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_30">
                            <!-- 着駅コード9 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION9_L" runat="server" Text="着駅コード9" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation9', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation9');">
                                    <asp:TextBox ID="TxtSlcArrStation9" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStation9Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_31">
                            <!-- 着駅コード10 -->
                            <span>
                                <asp:Label ID="WF_SLCARRSTATION10_L" runat="server" Text="着駅コード10" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrStation10', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtSlcArrStation10');">
                                    <asp:TextBox ID="TxtSlcArrStation10" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrStation10Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_32">
                            <!-- 着受託人コード -->
                            <span>
                                <asp:Label ID="WF_SLCARRTRUSTEECD_L" runat="server" Text="着受託人コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrTrusteeCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtSlcArrTrusteeCd');">
                                    <asp:TextBox ID="TxtSlcArrTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrTrusteeCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>

                            <!-- 着受託人ＣＤ比較条件 -->
                            <span>
                                <asp:Label ID="WF_SLCARRTRUSTEECDCOND_L" runat="server" Text="着受託人ＣＤ比較条件" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrTrusteeCdCond', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcArrTrusteeCdCond');">
                                    <asp:TextBox ID="TxtSlcArrTrusteeCdCond" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrTrusteeCdCondName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_33">
                            <!-- 着受託人サブコード -->
                            <span>
                                <asp:Label ID="WF_SLCARRTRUSTEESUBCD_L" runat="server" Text="着受託人サブコード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrTrusteeSubCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtSlcArrTrusteeSubCd');">
                                    <asp:TextBox ID="TxtSlcArrTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrTrusteeSubCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>

                            <!-- 着受託人サブＣＤ比較 -->
                            <span>
                                <asp:Label ID="WF_SLCARRTRUSTEESUBCDCOND_L" runat="server" Text="着受託人サブＣＤ比較" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcArrTrusteeSubCdCond', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcArrTrusteeSubCdCond');">
                                    <asp:TextBox ID="TxtSlcArrTrusteeSubCdCond" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcArrTrusteeSubCdCondName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_34">
                            <!-- 開始月日 -->
                            <span>
                                <asp:Label ID="WF_SLCSTMD_L" runat="server" Text="開始月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSlcStMD" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                                <!-- フォーマット合わせ用 -->
                                <asp:Label ID="Label1" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>

                            <!-- 終了月日 -->
                            <span>
                                <asp:Label ID="WF_SLCENDMD_L" runat="server" Text="終了月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSlcEndMD" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                                <!-- フォーマット合わせ用 -->
                                <asp:Label ID="Label2" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_35">
                            <!-- 開始発送年月日 -->
                            <span>
                                <asp:Label ID="WF_SLCSTSHIPYMD_L" runat="server" Text="開始発送年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcStShipMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                                    <asp:TextBox ID="TxtSlcStShipMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                                </span>
                                <!-- フォーマット合わせ用 -->
                                <asp:Label ID="Label3" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                            
                            <!-- 終了発送年月日 -->
                            <span>
                                <asp:Label ID="WF_ENDYMD_L" runat="server" Text="終了発送年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcEndShipMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                                    <asp:TextBox ID="TxtSlcEndShipMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                                </span>
                                <!-- フォーマット合わせ用 -->
                                <asp:Label ID="Label4" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_43">
                            <!-- ＪＲ品目コード1 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD1_L" runat="server" Text="ＪＲ品目コード1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCd1', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJrItemCd1');">
                                    <asp:TextBox ID="TxtSlcJrItemCd1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCd1Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>

                            <!-- ＪＲ品目コード比較 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCDCOND_L" runat="server" Text="ＪＲ品目コード比較" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCdCond', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSlcJrItemCdCond');">
                                    <asp:TextBox ID="TxtSlcJrItemCdCond" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCdCondName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_44">
                            <!-- ＪＲ品目コード2 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD2_L" runat="server" Text="ＪＲ品目コード2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCd2', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJrItemCd2');">
                                    <asp:TextBox ID="TxtSlcJrItemCd2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCd2Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_45">
                            <!-- ＪＲ品目コード3 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD3_L" runat="server" Text="ＪＲ品目コード3" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCd3', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJrItemCd3');">
                                    <asp:TextBox ID="TxtSlcJrItemCd3" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCd3Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_46">
                            <!-- ＪＲ品目コード4 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD4_L" runat="server" Text="ＪＲ品目コード4" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCd4', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJrItemCd4');">
                                    <asp:TextBox ID="TxtSlcJrItemCd4" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCd4Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_47">
                            <!-- ＪＲ品目コード5 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD5_L" runat="server" Text="ＪＲ品目コード5" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCd5', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJrItemCd5');">
                                    <asp:TextBox ID="TxtSlcJrItemCd5" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCd5Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_48">
                            <!-- ＪＲ品目コード6 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD6_L" runat="server" Text="ＪＲ品目コード6" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCd6', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJrItemCd6');">
                                    <asp:TextBox ID="TxtSlcJrItemCd6" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCd6Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_49">
                            <!-- ＪＲ品目コード7 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD7_L" runat="server" Text="ＪＲ品目コード7" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCd7', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJrItemCd7');">
                                    <asp:TextBox ID="TxtSlcJrItemCd7" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCd7Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_50">
                            <!-- ＪＲ品目コード8 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD8_L" runat="server" Text="ＪＲ品目コード8" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCd8', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJrItemCd8');">
                                    <asp:TextBox ID="TxtSlcJrItemCd8" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCd8Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_51">
                            <!-- ＪＲ品目コード9 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD9_L" runat="server" Text="ＪＲ品目コード9" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCd9', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJrItemCd9');">
                                    <asp:TextBox ID="TxtSlcJrItemCd9" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCd9Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_52">
                            <!-- ＪＲ品目コード10 -->
                            <span>
                                <asp:Label ID="WF_SLCJRITEMCD10_L" runat="server" Text="ＪＲ品目コード10" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSlcJrItemCd10', <%=LIST_BOX_CLASSIFICATION.LC_ITEM%>);" onchange="TextBox_change('TxtSlcJrItemCd10');">
                                    <asp:TextBox ID="TxtSlcJrItemCd10" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSlcJrItemCd10Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                    </div>
                </asp:Panel>
                <asp:Panel ID="WF_EXCEPTION_PANEL" runat="server">
                    <asp:Label ID="WF_EXCEPTION_L" runat="server" Text="【 特例置換項目 】" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <div id="detailkeybox2">
                        <p id="KEY_LINE_53">
                            <!-- 使用料金額 -->
                            <span>
                                <asp:Label ID="WF_SPRUSEFEE_L" runat="server" Text="使用料金額" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprUseFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7" style></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_54">
                            <!-- 使用料率 -->
                            <span>
                                <asp:Label ID="WF_SPRUSEFEERATE_L" runat="server" Text="使用料率" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprUseFeeRate" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_55">
                            <!-- 使用料率端数整理 -->
                            <span>
                                <asp:Label ID="WF_SPRUSEFEERATEROUND1_L" runat="server" Text="使用料率端数整理1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprUseFeeRateRound1', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprUseFeeRateRound1');">
                                    <asp:TextBox ID="TxtSprUseFeeRateRound1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprUseFeeRateRound1Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                            <span>
                                <asp:Label ID="WF_SPRUSEFEERATEROUND2_L" runat="server" Text="使用料率端数整理2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprUseFeeRateRound2', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprUseFeeRateRound2');">
                                    <asp:TextBox ID="TxtSprUseFeeRateRound2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprUseFeeRateRound2Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_56">
                            <!-- 使用料率加減額 -->
                            <span>
                                <asp:Label ID="WF_SPRUSEFEERATEADDSUB_L" runat="server" Text="使用料率加減額" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprUseFeeRateAddSub" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_57">
                            <!-- 使用料率加減額端数整理 -->
                            <span>
                                <asp:Label ID="WF_SPRUSEFEERATEADDSUBCOND1_L" runat="server" Text="使用料率加減額端数整理1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprUseFeeRateAddSubCond1', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprUseFeeRateAddSubCond1');">
                                    <asp:TextBox ID="TxtSprUseFeeRateAddSubCond1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprUseFeeRateAddSubCond1Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                            <span>
                                <asp:Label ID="WF_SPRUSEFEERATEADDSUBCOND2_L" runat="server" Text="使用料率加減額端数整理2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprUseFeeRateAddSubCond2', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprUseFeeRateAddSubCond2');">
                                    <asp:TextBox ID="TxtSprUseFeeRateAddSubCond2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprUseFeeRateAddSubCond2Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_58">
                            <!-- 端数処理時点区分 -->
                            <span>
                                <asp:Label ID="WF_SPRROUNDPOINTKBN_L" runat="server" Text="端数処理時点区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprRoundPointKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprRoundPointKbn');">
                                    <asp:TextBox ID="TxtSprRoundPointKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprRoundPointKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_59">
                            <!-- 使用料無料特認 -->
                            <span>
                                <asp:Label ID="WF_SPRUSEFREESPE_L" runat="server" Text="使用料無料特認" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprUseFreeSpe', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprUseFreeSpe');">
                                    <asp:TextBox ID="TxtSprUseFreeSpe" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprUseFreeSpeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_60">
                            <!-- 通運負担回送運賃 -->
                            <span>
                                <asp:Label ID="WF_SPRNITTSUFREESENDFEE_L" runat="server" Text="通運負担回送運賃" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprNittsuFreeSendFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_61">
                            <!-- 運行管理料 -->
                            <span>
                                <asp:Label ID="WF_SPRMANAGEFEE_L" runat="server" Text="運行管理料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprManageFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_62">
                            <!-- 荷主負担運賃 -->
                            <span>
                                <asp:Label ID="WF_SPRSHIPBURDENFEE_L" runat="server" Text="荷主負担運賃" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprShipBurdenFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_63">
                            <!-- 発送料 -->
                            <span>
                                <asp:Label ID="WF_SPRSHIPFEE_L" runat="server" Text="発送料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprShipFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_64">
                            <!-- 到着料 -->
                            <span>
                                <asp:Label ID="WF_SPRARRIVEFEE_L" runat="server" Text="到着料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprArriveFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_65">
                            <!-- 集荷料 -->
                            <span>
                                <asp:Label ID="WF_SPRPICKUPFEE_L" runat="server" Text="集荷料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprPickUpFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_66">
                            <!-- 配達料 -->
                            <span>
                                <asp:Label ID="WF_SPRDELIVERYFEE_L" runat="server" Text="配達料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprDeliveryFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_67">
                            <!-- その他1 -->
                            <span>
                                <asp:Label ID="WF_SPROTHER1_L" runat="server" Text="その他1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprOther1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_68">
                            <!-- その他2 -->
                            <span>
                                <asp:Label ID="WF_SPROTHER2_L" runat="server" Text="その他2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprOther2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_69">
                            <!-- 適合区分 -->
                            <span>
                                <asp:Label ID="WF_SPRFITKBN_L" runat="server" Text="適合区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprFitKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprFitKbn');">
                                    <asp:TextBox ID="TxtSprFitKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprFitKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_70">
                            <!-- 契約コード -->
                            <span>
                                <asp:Label ID="WF_SPRCONTRACTCD_L" runat="server" Text="契約コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprContractCd" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
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
