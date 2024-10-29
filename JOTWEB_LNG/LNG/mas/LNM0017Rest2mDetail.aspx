<%@ Page Title="LNM0017D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0017Rest2mDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0017Rest2mDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0017WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNM0017DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0017D.css")%>' rel="stylesheet" type="text/css"/>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0017D.js")%>'></script>
</asp:Content>
 
<asp:Content ID="LNM0017D" ContentPlaceHolderID="contents1" runat="server">
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

                <p id="KEY_LINE_11">
                    <!-- 使用目的 -->
                    <span class="colCodeOnly">
                        <asp:Label ID="WF_PURPOSE_L" runat="server" Text="使用目的" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPurpose" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="42"></asp:TextBox>
                    </span>
                </p>

                <asp:Panel ID="WF_EXCEPTION_PANEL" runat="server">
                    <asp:Label ID="WF_EXCEPTION_L" runat="server" Text="【 特例置換項目 】" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <div id="detailkeybox1">
                        <p id="KEY_LINE_12">
                            <!-- 使用料金額 -->
                            <span>
                                <asp:Label ID="WF_SPRUSEFEE_L" runat="server" Text="使用料金額" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprUseFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_13">
                            <!-- 使用料率 -->
                            <span>
                                <asp:Label ID="WF_SPRUSEFEERATE_L" runat="server" Text="使用料率" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprUseFeeRate" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_14">
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

                        <p id="KEY_LINE_15">
                            <!-- 使用料率加減額 -->
                            <span>
                                <asp:Label ID="WF_SPRUSEFEERATEADDSUB_L" runat="server" Text="使用料率加減額" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprUseFeeRateAddSub" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_16">
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
                        
                        <p id="KEY_LINE_17">
                            <!-- 使用料無料特認 -->
                            <span>
                                <asp:Label ID="WF_SPRUSEFREESPE_L" runat="server" Text="使用料無料特認" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprUseFreeSpe', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprUseFreeSpe');">
                                    <asp:TextBox ID="TxtSprUseFreeSpe" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprUseFreeSpeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                            </span>
                        </p>

                        <p id="KEY_LINE_18">
                            <!-- 通運負担回送運賃 -->
                            <span>
                                <asp:Label ID="WF_SPRNITTSUFREESENDFEE_L" runat="server" Text="通運負担回送運賃" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprNittsuFreeSendFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_19">
                            <!-- 運行管理料 -->
                            <span>
                                <asp:Label ID="WF_SPRMANAGEFEE_L" runat="server" Text="運行管理料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprManageFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_20">
                            <!-- 荷主負担運賃 -->
                            <span>
                                <asp:Label ID="WF_SPRSHIPBURDENFEE_L" runat="server" Text="荷主負担運賃" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprShipBurdenFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_21">
                            <!-- 発送料 -->
                            <span>
                                <asp:Label ID="WF_SPRSHIPFEE_L" runat="server" Text="発送料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprShipFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_22">
                            <!-- 到着料 -->
                            <span>
                                <asp:Label ID="WF_SPRARRIVEFEE_L" runat="server" Text="到着料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprArriveFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_23">
                            <!-- 集荷料 -->
                            <span>
                                <asp:Label ID="WF_SPRPICKUPFEE_L" runat="server" Text="集荷料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprPickUpFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_24">
                            <!-- 配達料 -->
                            <span>
                                <asp:Label ID="WF_SPRDELIVERYFEE_L" runat="server" Text="配達料" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprDeliveryFee" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_25">
                            <!-- その他1 -->
                            <span>
                                <asp:Label ID="WF_SPROTHER1_L" runat="server" Text="その他1" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprOther1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>

                        <p id="KEY_LINE_26">
                            <!-- その他2 -->
                            <span>
                                <asp:Label ID="WF_SPROTHER2_L" runat="server" Text="その他2" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="TxtSprOther2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                            </span>
                        </p>
                        
                        <p id="KEY_LINE_27">
                            <!-- 適合区分 -->
                            <span>
                                <asp:Label ID="WF_SPRFITKBN_L" runat="server" Text="適合区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <span ondblclick="Field_DBclick('TxtSprFitKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprFitKbn');">
                                    <asp:TextBox ID="TxtSprFitKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                                </span>
                                <asp:Label ID="LblSprFitKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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
