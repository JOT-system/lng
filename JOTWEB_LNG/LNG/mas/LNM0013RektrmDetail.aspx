<%@ Page Title="LNM0013D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0013RektrmDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0013RektrmDetail"%>

<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0013WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0013DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0013D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0013D.js")%>'></script>
</asp:Content>

<asp:Content ID="LNM0013D" ContentPlaceHolderID="contents1" runat="server">
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
                    <asp:Label ID="LblSelLineCNT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </span>
            </p>
            <p id="KEY_LINE_2">
                <!-- 削除フラグ -->
                <span>
                    <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span ondblclick="Field_DBclick('TxtDelFlg', <%=LIST_BOX_CLASSIFICATION.LC_DELFLG%>)" onchange="TextBox_change('TxtDelFlg');">
                        <asp:TextBox ID="TxtDelFlg" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblDelFlgName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_3" class="flexible">
                <!-- 画面ＩＤ -->
                <span style="display: none;">
                    <asp:Label ID="WF_MAPID_L" runat="server" Text="画面ＩＤ" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="TxtMapId" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </span>
            </p>

            <p id="KEY_LINE_4">
                <!-- 大分類コード -->
                <span>
                    <asp:Label ID="WF_BIGCTNCD_L" runat="server" Text="大分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="TxtBigCTNCDEvent" ondblclick="Field_DBclick('TxtBigCTNCD', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtBigCTNCD');">
                        <asp:TextBox ID="TxtBigCTNCD" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="2"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblBigCTNCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_5">
                <!-- 中分類コード -->
                <span>
                    <asp:Label ID="WF_MIDDLECTNCD_L" runat="server" Text="中分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="TxtMiddleCTNCDEvent" ondblclick="Field_DBclick('TxtMiddleCTNCD', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtMiddleCTNCD');">
                        <asp:TextBox ID="TxtMiddleCTNCD" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="2"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblMiddleCTNCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_6">
                <!-- 優先順位 -->
                <span>
                    <asp:Label ID="WF_PRIORITYNO_L" runat="server" Text="優先順位" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <asp:TextBox ID="TxtPriorityNO" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_7">
                <!-- 発駅コード -->
                <span>
                    <asp:Label ID="WF_DEPSTATION_L" runat="server" Text="発駅コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="TxtDepstationEvent" ondblclick="Field_DBclick('TxtDepstation', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtDepstation');">
                        <asp:TextBox ID="TxtDepstation" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="6"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblDepstationCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
                <!-- ＪＲ発支社支店コード -->
                <span>
                    <asp:Label ID="WF_JRDEPBRANCHCD_L" runat="server" Text="ＪＲ発支社支店コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="TxtJrDepBranchCDEvent" ondblclick="Field_DBclick('TxtJrDepBranchCD', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtJrDepBranchCD');">
                        <asp:TextBox ID="TxtJrDepBranchCD" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="5"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblJrDepBranchCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>
            <p id="KEY_LINE_9">
                <!-- 着駅コード -->
                <span>
                    <asp:Label ID="WF_ARRSTATION_L" runat="server" Text="着駅コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="TxtArrstationEvent" ondblclick="Field_DBclick('TxtArrstation', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtArrstation');">
                        <asp:TextBox ID="TxtArrstation" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="6"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblArrstationName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
                <!-- ＪＲ着支社支店コード -->
                <span>
                    <asp:Label ID="WF_JRARRBRANCHCD_L" runat="server" Text="ＪＲ着支社支店コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="TxtJrArrBranchCDEvent" ondblclick="Field_DBclick('TxtJrArrBranchCD', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtJrArrBranchCD');">
                        <asp:TextBox ID="TxtJrArrBranchCD" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="5"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblJrArrBranchCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_13">

                <!-- 使用目的 -->
                <span class="colCodeOnly">
                    <asp:Label ID="WF_PURPOSE_L" runat="server" Text="使用目的" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <asp:TextBox ID="TxtPurpose" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="42"></asp:TextBox>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_11">
                <!-- 発受託人コード -->
                <span>
                    <asp:Label ID="WF_DEPTRUSTEECD_L" runat="server" Text="発受託人コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <span ondblclick="Field_DBclick('TxtDepTrusteeCD', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtDepTrusteeCD');">
                        <asp:TextBox ID="TxtDepTrusteeCD" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblDepTrusteeCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
                <!-- 発受託人サブコード -->
                <span>
                    <asp:Label ID="WF_DEPTRUSTEESUBCD_L" runat="server" Text="発受託人サブコード" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <span ondblclick="Field_DBclick('TxtDepTrusteeSubCD', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtDepTrusteeSubCD');">
                        <asp:TextBox ID="TxtDepTrusteeSubCD" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblDepTrusteeSubCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_14">

                <!-- コンテナ記号 -->
                <span>
                    <asp:Label ID="WF_CTNTYPE_L" runat="server" Text="コンテナ記号" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <span id="TxtCTNTypeEvent" ondblclick="Field_DBclick('TxtCTNType', <%=LIST_BOX_CLASSIFICATION.LC_CTNTYPE%>);" onchange="TextBox_change('TxtCTNType');">
                        <asp:TextBox ID="TxtCTNType" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="5"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblCTNTypeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>
            <p id="KEY_LINE_15">
                <!-- コンテナ番号（開始） -->
                <span>
                    <asp:Label ID="WF_CTNStNO_L" runat="server" Text="コンテナ番号（開始）" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <span id="TxtCTNStNOEvent" ondblclick="Field_DBclick('TxtCTNStNO', <%=LIST_BOX_CLASSIFICATION.LC_CTNNO%>);" onchange="TextBox_change('TxtCTNStNO');">
                        <asp:TextBox ID="TxtCTNStNO" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="8"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblCTNStNOName" runat="server" Text="" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
                <!-- コンテナ番号（終了） -->
                <span>
                    <asp:Label ID="WF_CTNEndNO_L" runat="server" Text="コンテナ番号（終了）" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <span id="TxtCTNEndNOEvent" ondblclick="Field_DBclick('TxtCTNEndNO', <%=LIST_BOX_CLASSIFICATION.LC_CTNNO%>);" onchange="TextBox_change('TxtCTNEndNO');">
                        <asp:TextBox ID="TxtCTNEndNO" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="8"></asp:TextBox>
                    </span>
                    <asp:Label ID="LblCTNEndNOName" runat="server" Text="" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <asp:Panel ID="WF_EXCEPTION1_PANEL" runat="server">
                <asp:Label ID="WF_EXCEPTION1_L" runat="server" Text="【 特例置換項目（現行） 】" CssClass="WF_TEXT_LEFT"></asp:Label>
                <div id="detailkeybox1">

                    <p id="KEY_LINE_17">
                        <!-- 特例置換項目-現行開始適用日 -->
                        <span>
                            <asp:Label ID="WF_SPRCURSTYMD_L" runat="server" Text="開始適用日" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span ondblclick="Field_DBclick('TxtSprCurStYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                                <asp:TextBox ID="TxtSprCurStYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                            </span>
                            <asp:Label ID="LblSprCurStYMD" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>

                        <!-- 特例置換項目-現行終了適用日 -->
                        <span>
                            <asp:Label ID="WF_SPRCURENDYMD_L" runat="server" Text="終了適用日" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span ondblclick="Field_DBclick('TxtSprCurEndYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                                <asp:TextBox ID="TxtSprCurEndYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                            </span>
                            <asp:Label ID="LblSprCurEndYMD" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                    </p>
                    <p id="KEY_LINE_18">
                        <!-- 特例置換項目-現行適用率 -->
                        <span>
                            <asp:Label ID="WF_SPRCURAPPLYRATE_L" runat="server" Text="適用率" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <asp:TextBox ID="TxtSprCurApplyRate" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>
                            <asp:Label ID="LblSprCurApplyRateDummy" runat="server" Text="" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                    </p>
                    <p id="KEY_LINE_19">
                        <!-- 特例置換項目-現行端数処理区分1 -->
                        <span>
                            <asp:Label ID="WF_SPRCURROUNDKBN1_L" runat="server" Text="端数処理区分1" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span id="SPRCURROUNDKBN1" ondblclick="Field_DBclick('TxtSprCurRoundKbn1', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprCurRoundKbn1');">
                                <asp:TextBox ID="TxtSprCurRoundKbn1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                            </span>
                            <asp:Label ID="LblSprCurRoundKbn1Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                        <!-- 特例置換項目-現行端数処理区分2 -->
                        <span>
                            <asp:Label ID="WF_SPRCURROUNDKBN2_L" runat="server" Text="端数処理区分2" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span id="SPRCURROUNDKBN2" ondblclick="Field_DBclick('TxtSprCurRoundKbn2', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprCurRoundKbn2');">
                                <asp:TextBox ID="TxtSprCurRoundKbn2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                            </span>
                            <asp:Label ID="LblSprCurRoundKbn2Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                    </p>
                </div>
            </asp:Panel>

            <asp:Panel ID="WF_EXCEPTION2_PANEL" runat="server">
                <asp:Label ID="WF_EXCEPTION2_L" runat="server" Text="【 特例置換項目（次期） 】" CssClass="WF_TEXT_LEFT"></asp:Label>
                <div id="detailkeybox2">


                    <p id="KEY_LINE_20">
                        <!-- 特例置換項目-次期開始適用日 -->
                        <span>
                            <asp:Label ID="WF_SPRNEXTSTYMD_L" runat="server" Text="開始適用日" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span ondblclick="Field_DBclick('TxtSprNextStYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                                <asp:TextBox ID="TxtSprNextStYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                            </span>
                            <asp:Label ID="LblSprNextStYMD" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>

                        <!-- 特例置換項目-次期終了適用日 -->
                        <span>
                            <asp:Label ID="WF_SPRNEXTENDYMD_L" runat="server" Text="終了適用日" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span ondblclick="Field_DBclick('TxtSprNextEndYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                                <asp:TextBox ID="TxtSprNextEndYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                            </span>
                            <asp:Label ID="LblSprNextEndYMD" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                    </p>
                    <p id="KEY_LINE_21">
                        <!-- 特例置換項目-次期適用率 -->
                        <span>
                            <asp:Label ID="WF_SPRNEXTAPPLYRATE_L" runat="server" Text="適用率" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <asp:TextBox ID="TxtSprNextApplyRate" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>
                            <asp:Label ID="LblSprNextApplyRateDummy" runat="server" Text="" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                    </p>
                    <p id="KEY_LINE_22">
                        <!-- 特例置換項目-次期端数処理区分1 -->
                        <span>
                            <asp:Label ID="WF_SPRNEXTROUNDKBN1_L" runat="server" Text="端数処理区分1" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span id="SPRNEXTROUNDKBN1" ondblclick="Field_DBclick('TxtSprNextRoundKbn1', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprNextRoundKbn1');">
                                <asp:TextBox ID="TxtSprNextRoundKbn1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                            </span>
                            <asp:Label ID="LblSprNextRoundKbn1Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                        </span>
                        <!-- 特例置換項目-次期端数処理区分2 -->
                        <span>
                            <asp:Label ID="WF_SPRNEXTROUNDKBN2_L" runat="server" Text="端数処理区分2" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <span id="SPRNEXTROUNDKBN2" ondblclick="Field_DBclick('TxtSprNextRoundKbn2', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSprNextRoundKbn2');">
                                <asp:TextBox ID="TxtSprNextRoundKbn2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                            </span>
                            <asp:Label ID="LblSprNextRoundKbn2Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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
