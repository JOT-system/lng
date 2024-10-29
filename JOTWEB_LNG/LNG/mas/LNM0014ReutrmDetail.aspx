<%@ Page Title="LNM0014D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0014ReutrmDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0014ReutrmDetail"%>

<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0014WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0014DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0014D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0014D.js")%>'></script>
</asp:Content>

<asp:Content ID="LNM0014D" ContentPlaceHolderID="contents1" runat="server">
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
                <!-- 大分類コード -->
                <span>
                    <asp:Label ID="WF_BIGCTNCD_L" runat="server" Text="大分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="txtBigCtnCdEvent" ondblclick="Field_DBclick('txtBigCtnCd', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('txtBigCtnCd');">
                        <asp:TextBox ID="txtBigCtnCd" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="2"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblBigCtnCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_5">
                <!-- 中分類コード -->
                <span>
                    <asp:Label ID="WF_MIDDLECTNCD_L" runat="server" Text="中分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="txtMiddleCtnCdEvent" ondblclick="Field_DBclick('txtMiddleCtnCd', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('txtMiddleCtnCd');">
                        <asp:TextBox ID="txtMiddleCtnCd" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="2"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblMiddleCtnCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_6">
                <!-- 発駅コード -->
                <span>
                    <asp:Label ID="WF_DEPSTATION_L" runat="server" Text="発駅コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="txtDepStationEvent" ondblclick="Field_DBclick('txtDepStation', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('txtDepStation');">
                        <asp:TextBox ID="txtDepStation" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="6"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblDepStationCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_7">
                <!-- 発受託人コード -->
                <span>
                    <asp:Label ID="WF_DEPTRUSTEECD_L" runat="server" Text="発受託人コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="txtDepTrusteeCdEvent" ondblclick="Field_DBclick('txtDepTrusteeCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('txtDepTrusteeCd');">
                        <asp:TextBox ID="txtDepTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="5"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblDepTrusteeCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>
            <p id="KEY_LINE_8">
                <!-- 発受託人サブコード -->
                <span>
                    <asp:Label ID="WF_DEPTRUSTEESUBCD_L" runat="server" Text="発受託人サブコード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="txtDepTrusteeSubCdEvent" ondblclick="Field_DBclick('txtDepTrusteeSubCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('txtDepTrusteeSubCd');">
                        <asp:TextBox ID="txtDepTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="3"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblDepTrusteeSubCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>


            <p id="KEY_LINE_9">
                <!-- 優先順位 -->
                <span>
                    <asp:Label ID="WF_PRIORITYNO_L" runat="server" Text="優先順位" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <asp:TextBox ID="txtPriorityNo" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_10">

                <!-- 使用目的 -->
                <span class="colCodeOnly">
                    <asp:Label ID="WF_PURPOSE_L" runat="server" Text="使用目的" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="txtPurpose" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="42"></asp:TextBox>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_11">
                <!-- 着駅コード -->
                <span>
                    <asp:Label ID="WF_ARRSTATION_L" runat="server" Text="着駅コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <span id="txtArrStationEvent" ondblclick="Field_DBclick('txtArrStation', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('txtArrStation');">
                        <asp:TextBox ID="txtArrStation" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="6"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblArrStationName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_12">
                <!-- 着受託人コード -->
                <span>
                    <asp:Label ID="WF_ARRTRUSTEECD_L" runat="server" Text="着受託人コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <span ondblclick="Field_DBclick('txtArrTrusteeCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('txtArrTrusteeCd');">
                        <asp:TextBox ID="txtArrTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblArrTrusteeCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>
            <p id="KEY_LINE_13">
                <!-- 着受託人サブコード -->
                <span>
                    <asp:Label ID="WF_ARRTRUSTEESUBCD_L" runat="server" Text="着受託人サブコード" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <span ondblclick="Field_DBclick('txtArrTrusteeSubCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('txtArrTrusteeSubCd');">
                        <asp:TextBox ID="txtArrTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblArrTrusteeSubCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <asp:Panel ID="WF_EXCEPTION1_PANEL" runat="server">
                <asp:Label ID="WF_EXCEPTION1_L" runat="server" Text="【 特例置換項目（現行） 】" CssClass="WF_TEXT_LEFT"></asp:Label>
                <div id="detailkeybox1">

                    <p id="KEY_LINE_14">
                        <!-- 特例置換項目-現行開始適用日 -->
                        <span>
                            <asp:Label ID="WF_SPRCURSTYMD_L" runat="server" Text="開始適用日" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span ondblclick="Field_DBclick('txtSprCurStYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                                <asp:TextBox ID="txtSprCurStYmd" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                            </span>
                            <asp:Label ID="lblSprCurStYmd" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>

                        <!-- 特例置換項目-現行終了適用日 -->
                        <span>
                            <asp:Label ID="WF_SPRCURENDYMD_L" runat="server" Text="終了適用日" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span ondblclick="Field_DBclick('txtSprCurEndYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                                <asp:TextBox ID="txtSprCurEndYmd" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                            </span>
                            <asp:Label ID="lblSprCurEndYmd" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                    </p>
                    <p id="KEY_LINE_15">
                        <!-- 特例置換項目-現行発送料 -->
                        <span>
                            <asp:Label ID="WF_SPRCURSHIPFEE_L" runat="server" Text="発送料" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <asp:TextBox ID="txtSprCurShipFee" runat="server" CssClass="WF_TEXTBOX_CSS textbox_right" MaxLength="5"></asp:TextBox>
                        </span>
                    </p>
                    <p id="KEY_LINE_16">
                        <!-- 特例置換項目-現行到着料 -->
                        <span>
                            <asp:Label ID="WF_SPRCURARRIVEFEE_L" runat="server" Text="到着料" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <asp:TextBox ID="txtSprCurArriveFee" runat="server" CssClass="WF_TEXTBOX_CSS textbox_right" MaxLength="5"></asp:TextBox>
                        </span>
                    </p>
                    <p id="KEY_LINE_17">
                        <!-- 特例置換項目-現行端数処理区分1 -->
                        <span>
                            <asp:Label ID="WF_SPRCURROUNDKBN1_L" runat="server" Text="端数処理区分1" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span id="SPRCURROUNDKBN1" ondblclick="Field_DBclick('txtSprCurRoundKbn1', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('txtSprCurRoundKbn1');">
                                <asp:TextBox ID="txtSprCurRoundKbn1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                            </span>
                            <asp:Label ID="lblSprCurRoundKbn1Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                        <!-- 特例置換項目-現行端数処理区分2 -->
                        <span>
                            <asp:Label ID="WF_SPRCURROUNDKBN2_L" runat="server" Text="端数処理区分2" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span id="SPRCURROUNDKBN2" ondblclick="Field_DBclick('txtSprCurRoundKbn2', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('txtSprCurRoundKbn2');">
                                <asp:TextBox ID="txtSprCurRoundKbn2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                            </span>
                            <asp:Label ID="lblSprCurRoundKbn2Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                    </p>
                </div>
            </asp:Panel>

            <asp:Panel ID="WF_EXCEPTION2_PANEL" runat="server">
                <asp:Label ID="WF_EXCEPTION2_L" runat="server" Text="【 特例置換項目（次期） 】" CssClass="WF_TEXT_LEFT"></asp:Label>
                <div id="detailkeybox2">


                    <p id="KEY_LINE_18">
                        <!-- 特例置換項目-次期開始適用日 -->
                        <span>
                            <asp:Label ID="WF_SPRNEXTSTYMD_L" runat="server" Text="開始適用日" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span ondblclick="Field_DBclick('txtSprNextStYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                                <asp:TextBox ID="txtSprNextStYmd" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                            </span>
                            <asp:Label ID="lblSprNextStYmd" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>

                        <!-- 特例置換項目-次期終了適用日 -->
                        <span>
                            <asp:Label ID="WF_SPRNEXTENDYMD_L" runat="server" Text="終了適用日" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span ondblclick="Field_DBclick('txtSprNextEndYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                                <asp:TextBox ID="txtSprNextEndYmd" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                            </span>
                            <asp:Label ID="lblSprNextEndYmd" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                    </p>
                    <p id="KEY_LINE_19">
                        <!-- 特例置換項目-次期発送料 -->
                        <span>
                            <asp:Label ID="WF_SPRNEXTSHIPFEE_L" runat="server" Text="発送料" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <asp:TextBox ID="txtSprNextShipFee" runat="server" CssClass="WF_TEXTBOX_CSS textbox_right" MaxLength="5"></asp:TextBox>
                        </span>
                    </p>
                    <p id="KEY_LINE_20">
                        <!-- 特例置換項目-次期到着料 -->
                        <span>
                            <asp:Label ID="WF_SPRNEXTARRIVEFEE_L" runat="server" Text="到着料" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <asp:TextBox ID="txtSprNextArriveFee" runat="server" CssClass="WF_TEXTBOX_CSS textbox_right" MaxLength="5"></asp:TextBox>
                        </span>
                    </p>
                    <p id="KEY_LINE_21">
                        <!-- 特例置換項目-次期端数処理区分1 -->
                        <span>
                            <asp:Label ID="WF_SPRNEXTROUNDKBN1_L" runat="server" Text="端数処理区分1" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span id="SPRNEXTROUNDKBN1" ondblclick="Field_DBclick('txtSprNextRoundKbn1', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('txtSprNextRoundKbn1');">
                                <asp:TextBox ID="txtSprNextRoundKbn1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                            </span>
                            <asp:Label ID="lblSprNextRoundKbn1Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                        <!-- 特例置換項目-次期端数処理区分2 -->
                        <span>
                            <asp:Label ID="Label1" runat="server" Text="端数処理区分2" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span id="SPRNEXTROUNDKBN2" ondblclick="Field_DBclick('txtSprNextRoundKbn2', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('txtSprNextRoundKbn2');">
                                <asp:TextBox ID="txtSprNextRoundKbn2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                            </span>
                            <asp:Label ID="lblSprNextRoundKbn2Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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

        <!-- 縦スクロール位置 -->
        <input id="WF_scrollY" runat="server" value="0" type="text" />
    </div>

</asp:Content>
