<%@ Page Title="LNM0002D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0002ReconmDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0002ReconmDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0002WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNM0002DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0002D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0002D.js")%>'></script>
</asp:Content>
 
<asp:Content ID="LNM0002D" ContentPlaceHolderID="contents1" runat="server">
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
                    <!-- コンテナ記号 -->
                    <span>
                        <asp:Label ID="WF_CTNTYPE_L" runat="server" Text="コンテナ記号" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span id="TxtCTNTypeEvent" ondblclick="Field_DBclick('TxtCTNType', <%=LIST_BOX_CLASSIFICATION.LC_RECONM%>);" onchange="TextBox_change('TxtCTNType');">
                            <asp:TextBox ID="TxtCTNType" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="5"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblCTNTypeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_5" class="flexible">
                    <!-- コンテナ番号 -->
                    <span>
                        <asp:Label ID="WF_CTNNO_L" runat="server" Text="コンテナ番号" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtCTNNo" runat="server" CssClass="WF_TEXTBOX_CSS"  onblur="MsgClear();" MaxLength="8"></asp:TextBox>
                        <asp:Label ID="LblCTNNoName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 所管部コード -->
                    <span>
                        <asp:Label ID="WF_JURISDICTIONCD_L" runat="server" Text="所管部コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtJurisdictionCD', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtJurisdictionCD');">
                            <asp:TextBox ID="TxtJurisdictionCD" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblJurisdictionCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 経理資産コード -->
                    <span>
                        <asp:Label ID="WF_ACCOUNTINGASSETSCD_L" runat="server" Text="経理資産コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAccountingAsSetCD', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAccountingAsSetCD');">
                            <asp:TextBox ID="TxtAccountingAsSetCD" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAccountingAsSetCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 経理資産区分 -->
                    <span>
                        <asp:Label ID="WF_ACCOUNTINGASSETSKBN_L" runat="server" Text="経理資産区分" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAccountingAsSetKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAccountingAsSetKbn');">
                            <asp:TextBox ID="TxtAccountingAsSetKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAccountingAsSetKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- ダミー区分 -->
                    <span>
                        <asp:Label ID="WF_DUMMYKBN_L" runat="server" Text="ダミー区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtDummyKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtDummyKbn');">
                            <asp:TextBox ID="TxtDummyKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDummyKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- スポット区分 -->
                    <span>
                        <asp:Label ID="WF_SPOTKBN_L" runat="server" Text="スポット区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtSpotKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSpotKbn');">
                            <asp:TextBox ID="TxtSpotKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblSpotKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="DATA_LINE_11" class="flexible">
                    <!-- スポット区分　開始年月日 -->
                    <span>
                        <asp:Label ID="WF_SPOTSTYMD_L" runat="server" Text="スポット区分　開始年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtSpotStYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="TxtSpotStYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="Label1" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- スポット区分　終了年月日 -->
                        <asp:Label ID="WF_ENDYMD_L" runat="server" Text="スポット区分　終了年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtSpotEndYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="TxtSpotEndYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="Label2" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_12">
                    <!-- 大分類コード -->
                    <span>
                        <asp:Label ID="WF_BIGCTNCD_L" runat="server" Text="大分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtBigCTNCD', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtBigCTNCD');">
                            <asp:TextBox ID="TxtBigCTNCD" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblBigCTNCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_13">
                    <!-- 中分類コード -->
                    <span>
                        <asp:Label ID="WF_MIDDLECTNCD_L" runat="server" Text="中分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtMiddleCTNCD', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtMiddleCTNCD');">
                            <asp:TextBox ID="TxtMiddleCTNCD" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblMiddleCTNCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_14">
                    <!-- 小分類コード -->
                    <span>
                        <asp:Label ID="WF_SMALLCTNCD_L" runat="server" Text="小分類コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtSmallCTNCD', <%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtSmallCTNCD');">
                            <asp:TextBox ID="TxtSmallCTNCD" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblSmallCTNCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_15" class="flexible">
                    <!-- 建造年月 -->
                    <span>
                        <asp:Label ID="WF_CONSTRUCTIONYM_L" runat="server" Text="建造年月" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtConstructionYM" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_16">
                    <!-- コンテナメーカー -->
                    <span>
                        <asp:Label ID="WF_CTNMAKER_L" runat="server" Text="コンテナメーカー" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtCTNMaker', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtCTNMaker');">
                            <asp:TextBox ID="TxtCTNMaker" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblCTNMakerName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_17">
                    <!-- 冷凍機メーカー -->
                    <span>
                        <asp:Label ID="WF_FROZENMAKER_L" runat="server" Text="冷凍機メーカー"  CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtFrozenMaker', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtFrozenMaker');">
                            <asp:TextBox ID="TxtFrozenMaker" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblFrozenMakerName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="DATA_LINE_18" class="flexible">
                    <!-- 総重量 -->
                    <span>
                        <asp:Label ID="WF_GROSSWEIGHT_L" runat="server" Text="総重量" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtGrossWeight" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                        <asp:Label ID="Label3" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 荷重 -->
                        <asp:Label ID="WF_CARGOWEIGHT_L" runat="server" Text="荷重" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtCargoWeight" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                        <asp:Label ID="Label4" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 自重 -->
                        <asp:Label ID="WF_MYWEIGHT_L" runat="server" Text="自重" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtMyWeight" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                    </span>
                </p>
                
                <p id="KEY_LINE_84" class="flexible">
                    <!-- 簿価商品価格 -->
                    <span>
                        <asp:Label ID="WF_BOOKVALUE_L" runat="server" Text="簿価商品価格" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtBookValue" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="9"></asp:TextBox>
                    </span>
                </p>

                <p id="DATA_LINE_19" class="flexible">
                    <!-- 外寸・高さ -->
                    <span>
                        <asp:Label ID="WF_OUTHEIGHT_L" runat="server" Text="外寸・高さ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOutHeight" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="Label6" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 外寸・幅 -->
                        <asp:Label ID="WF_OUTWIDTH_L" runat="server" Text="外寸・幅" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOutWidth" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="Label7" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 外寸・長さ -->
                        <asp:Label ID="WF_OUTLENGTH_L" runat="server" Text="外寸・長さ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtOutLength" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                    </span>
                </p>

                <p id="DATA_LINE_20" class="flexible">
                    <!-- 内寸・高さ -->
                    <span>
                        <asp:Label ID="WF_INHEIGHT_L" runat="server" Text="内寸・高さ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtInHeight" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="Label9" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 内寸・幅 -->
                        <asp:Label ID="WF_INWIDTH_L" runat="server" Text="内寸・幅" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtInWidth" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="Label10" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 内寸・長さ -->
                        <asp:Label ID="WF_INLENGTH_L" runat="server" Text="内寸・長さ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtInLength" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                    </span>
                </p>

                <p id="DATA_LINE_21" class="flexible">
                    <!-- 妻入口・高さ -->
                    <span>
                        <asp:Label ID="WF_WIFEHEIGHT_L" runat="server" Text="妻入口・高さ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtWifeHeight" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="Label12" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 妻入口・幅 -->
                        <asp:Label ID="WF_WIFEWIDTH_L" runat="server" Text="妻入口・幅" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtWifeWidth" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="Label13" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="DATA_LINE_22" class="flexible">
                    <!-- 側入口・高さ -->
                    <span>
                        <asp:Label ID="WF_SIDEHEIGHT_L" runat="server" Text="側入口・高さ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtSideHeight" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="Label14" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 側入口・幅 -->
                        <asp:Label ID="WF_SIDEWIDTH_L" runat="server" Text="側入口・幅" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtSideWidth" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="Label15" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_23" class="flexible">
                    <!-- 床面積 -->
                    <span>
                        <asp:Label ID="WF_FLOORAREA_L" runat="server" Text="床面積" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtFloorArea" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                    </span>
                </p>

                <p id="DATA_LINE_24" class="flexible">
                    <!-- 内容積・標記 -->
                    <span>
                        <asp:Label ID="WF_INVOLUMEMARKING_L" runat="server" Text="内容積・標記" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtInVolumeMarking" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                        <asp:Label ID="Label16" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 内容積・実寸 -->
                        <asp:Label ID="WF_INVOLUMEACTUA_L" runat="server" Text="内容積・実寸" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtInVolumeActua" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="7"></asp:TextBox>
                        <asp:Label ID="Label17" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_25" class="flexible">
                    <!-- 交番検査・ｻｲｸﾙ日数 -->
                    <span>
                        <asp:Label ID="WF_TRAINSCYCLEDAYS_L" runat="server" Text="交番検査・ｻｲｸﾙ日数" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtTrainsCycleDays" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                    </span>
                </p>

                <p id="DATA_LINE_26" class="flexible">
                    <!-- 交番検査・前回実施日 -->
                    <span>
                        <asp:Label ID="WF_TRAINSBEFORERUNYMD_L" runat="server" Text="交番検査・前回実施日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtTrainsBeforeRunYMD" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                        <asp:Label ID="Label18" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 交番検査・次回実施日 -->
                        <asp:Label ID="WF_TRAINSNEXTRUNYMD_L" runat="server" Text="交番検査・次回実施日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtTrainsNextRunYMD" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                        <asp:Label ID="Label19" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="DATA_LINE_27" class="flexible">
                    <!-- 定期検査・ｻｲｸﾙ月数 -->
                    <span>
                        <asp:Label ID="WF_REGINSCYCLEDAYS_L" runat="server" Text="定期検査・ｻｲｸﾙ月数" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtReginsCycleDays" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                        <asp:Label ID="Label20" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 定期検査・ｻｲｸﾙｱﾜﾒｰﾀ -->
                        <asp:Label ID="WF_REGINSCYCLEHOURMETER_L" runat="server" Text="定期検査・ｻｲｸﾙｱﾜﾒｰﾀ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtReginsCycleHourMeter" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                        <asp:Label ID="Label21" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="DATA_LINE_28" class="flexible">
                    <!-- 定期検査・前回実施日 -->
                    <span>
                        <asp:Label ID="WF_REGINSBEFORERUNYMD_L" runat="server" Text="定期検査・前回実施日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtReginsBeforeRunYMD" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                        <asp:Label ID="Label22" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 定期検査・次回実施日 -->
                        <asp:Label ID="WF_REGINSNEXTRUNYMD_L" runat="server" Text="定期検査・次回実施日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtReginsNextRunYMD" runat="server" CssClass="WF_TEXTBOX_CSS" Enabled="false"></asp:TextBox>
                        <asp:Label ID="Label23" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="DATA_LINE_29" class="flexible">
                    <!-- 定期検査・ｱﾜﾒｰﾀ記載日 -->
                    <span>
                        <asp:Label ID="WF_REGINSHOURMETERYMD_L" runat="server" Text="定期検査・ｱﾜﾒｰﾀ記載日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtReginsHourMeterYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="TxtReginsHourMeterYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="Label24" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 定期検査・ｱﾜﾒｰﾀ時間 -->
                        <asp:Label ID="WF_REGINSHOURMETERTIME_L" runat="server" Text="定期検査・ｱﾜﾒｰﾀ時間" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtReginsHourMeterTime" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="5"></asp:TextBox>
                        <asp:Label ID="Label25" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 定期検査・ｱﾜﾒｰﾀ表示桁 -->
                        <asp:Label ID="WF_REGINSHOURMETERDSP_L" runat="server" Text="定期検査・ｱﾜﾒｰﾀ表示桁" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtReginsHourMeterDSP" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                    </span>
                </p>

                <p id="DATA_LINE_30" class="flexible">
                    <!-- 運用開始年月日 -->
                    <span>
                        <asp:Label ID="WF_OPERATIONSTYMD_L" runat="server" Text="運用開始年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtOperationStYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="TxtOperationStYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="Label27" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>

                    <!-- 運用除外年月日 -->
                        <asp:Label ID="WF_OPERATIONENDYMD_L" runat="server" Text="運用除外年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtOperationEndYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="TxtOperationEndYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                        <asp:Label ID="Label28" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_31" class="flexible">
                    <!-- 除却年月日 -->
                    <span>
                        <asp:Label ID="WF_RETIRMENTYMD_L" runat="server" Text="除却年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtRetirmentYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>)">
                            <asp:TextBox ID="TxtRetirmentYMD" runat="server" CssClass="WF_TEXTBOX_CSS calendarIcon"></asp:TextBox>
                        </span>
                    </span>
                </p>

                <p id="KEY_LINE_32">
                    <!-- 複合一貫区分 -->
                    <span>
                        <asp:Label ID="WF_COMPKANKBN_L" runat="server" Text="複合一貫区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtCompKanKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtCompKanKbn');">
                            <asp:TextBox ID="TxtCompKanKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblCompKanKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="KEY_LINE_85">
                    <!-- 調達フラグ -->
                    <span>
                        <asp:Label ID="WF_SUPPLYFLG_L" runat="server" Text="調達フラグ" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtSupplyFLG', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtSupplyFLG');">
                            <asp:TextBox ID="TxtSupplyFLG" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblSupplyFLGName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_33">
                    <!-- 付帯項目１ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM1_L" runat="server" Text="付帯項目１(使用禁止)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem1', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem1');">
                            <asp:TextBox ID="TxtAddItem1" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem1Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_34">
                    <!-- 付帯項目２ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM2_L" runat="server" Text="付帯項目２(優先臨時表示)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem2', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem2');">
                            <asp:TextBox ID="TxtAddItem2" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem2Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_35">
                    <!-- 付帯項目３ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM3_L" runat="server" Text="付帯項目３(エンジンオーバーホー)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem3', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem3');">
                            <asp:TextBox ID="TxtAddItem3" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem3Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_36">
                    <!-- 付帯項目４ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM4_L" runat="server" Text="付帯項目４(重点整備対象)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem4', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem4');">
                            <asp:TextBox ID="TxtAddItem4" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem4Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_37">
                    <!-- 付帯項目５ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM5_L" runat="server" Text="付帯項目５(青函アンテナ交換対象)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem5', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem5');">
                            <asp:TextBox ID="TxtAddItem5" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem5Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_38">
                    <!-- 付帯項目６ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM6_L" runat="server" Text="付帯項目６(管外回送禁止)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem6', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem6');">
                            <asp:TextBox ID="TxtAddItem6" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem6Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_39">
                    <!-- 付帯項目７ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM7_L" runat="server" Text="付帯項目７(再塗装未実施)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem7', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem7');">
                            <asp:TextBox ID="TxtAddItem7" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem7Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_40">
                    <!-- 付帯項目８ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM8_L" runat="server" Text="付帯項目８(濡損防止対策未実施)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem8', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem8');">
                            <asp:TextBox ID="TxtAddItem8" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem8Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_41">
                    <!-- 付帯項目９ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM9_L" runat="server" Text="付帯項目９(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem9', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem9');">
                            <asp:TextBox ID="TxtAddItem9" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem9Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_42">
                    <!-- 付帯項目１０ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM10_L" runat="server" Text="付帯項目１０(基本表示)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem10', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem10');">
                            <asp:TextBox ID="TxtAddItem10" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem10Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_43">
                    <!-- 付帯項目１１ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM11_L" runat="server" Text="付帯項目１１(色・標記)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem11', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem11');">
                            <asp:TextBox ID="TxtAddItem11" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem11Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_44">
                    <!-- 付帯項目１２ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM12_L" runat="server" Text="付帯項目１２(扉配置)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem12', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem12');">
                            <asp:TextBox ID="TxtAddItem12" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem12Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_45">
                    <!-- 付帯項目１３ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM13_L" runat="server" Text="付帯項目１３(フォークポケット)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem13', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem13');">
                            <asp:TextBox ID="TxtAddItem13" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem13Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_46">
                    <!-- 付帯項目１４ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM14_L" runat="server" Text="付帯項目１４(隅金具)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem14', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem14');">
                            <asp:TextBox ID="TxtAddItem14" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem14Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_47">
                    <!-- 付帯項目１５ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM15_L" runat="server" Text="付帯項目１５(ラッシングリング)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem15', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem15');">
                            <asp:TextBox ID="TxtAddItem15" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem15Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_48">
                    <!-- 付帯項目１６ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM16_L" runat="server" Text="付帯項目１６(ジョロダーレール)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem16', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem16');">
                            <asp:TextBox ID="TxtAddItem16" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem16Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_49">
                    <!-- 付帯項目１７ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM17_L" runat="server" Text="付帯項目１７(側扉ヒンジ)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem17', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem17');">
                            <asp:TextBox ID="TxtAddItem17" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem17Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_50">
                    <!-- 付帯項目１８ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM18_L" runat="server" Text="付帯項目１８(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem18', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem18');">
                            <asp:TextBox ID="TxtAddItem18" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem18Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_51">
                    <!-- 付帯項目１９ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM19_L" runat="server" Text="付帯項目１９(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem19', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem19');">
                            <asp:TextBox ID="TxtAddItem19" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem19Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_52">
                    <!-- 付帯項目２０ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM20_L" runat="server" Text="付帯項目２０(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem20', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem20');">
                            <asp:TextBox ID="TxtAddItem20" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem20Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_53">
                    <!-- 付帯項目２１ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM21_L" runat="server" Text="付帯項目２１(通風装置)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem21', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem21');">
                            <asp:TextBox ID="TxtAddItem21" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem21Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_54">
                    <!-- 付帯項目２２ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM22_L" runat="server" Text="付帯項目２２(パレット積載)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem22', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem22');">
                            <asp:TextBox ID="TxtAddItem22" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem22Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_55">
                    <!-- 付帯項目２３ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM23_L" runat="server" Text="付帯項目２３(水抜き穴)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem23', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem23');">
                            <asp:TextBox ID="TxtAddItem23" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem23Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_56">
                    <!-- 付帯項目２４ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM24_L" runat="server" Text="付帯項目２４(エアリブ)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem24', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem24');">
                            <asp:TextBox ID="TxtAddItem24" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem24Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_57">
                    <!-- 付帯項目２５ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM25_L" runat="server" Text="付帯項目２５(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem25', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem25');">
                            <asp:TextBox ID="TxtAddItem25" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem25Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_58">
                    <!-- 付帯項目２６ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM26_L" runat="server" Text="付帯項目２６(鮮魚専用コンテナ)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem26', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem26');">
                            <asp:TextBox ID="TxtAddItem26" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem26Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_59">
                    <!-- 付帯項目２７ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM27_L" runat="server" Text="付帯項目２７(キャノン専用コンテナ)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem27', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem27');">
                            <asp:TextBox ID="TxtAddItem27" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem27Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_60">
                    <!-- 付帯項目２８ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM28_L" runat="server" Text="付帯項目２８(特別留置)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem28', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem28');">
                            <asp:TextBox ID="TxtAddItem28" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem28Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_61">
                    <!-- 付帯項目２９ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM29_L" runat="server" Text="付帯項目２９(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem29', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem29');">
                            <asp:TextBox ID="TxtAddItem29" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem29Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_62">
                    <!-- 付帯項目３０ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM30_L" runat="server" Text="付帯項目３０(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem30', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem30');">
                            <asp:TextBox ID="TxtAddItem30" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem30Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_63">
                    <!-- 付帯項目３１ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM31_L" runat="server" Text="付帯項目３１(冷凍機温度帯)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem31', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem31');">
                            <asp:TextBox ID="TxtAddItem31" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem31Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_64">
                    <!-- 付帯項目３２ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM32_L" runat="server" Text="付帯項目３２(遠隔監視制御装置)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem32', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem32');">
                            <asp:TextBox ID="TxtAddItem32" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem32Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_65">
                    <!-- 付帯項目３３ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM33_L" runat="server" Text="付帯項目３３(エンジン形式)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem33', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem33');">
                            <asp:TextBox ID="TxtAddItem33" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem33Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_66">
                    <!-- 付帯項目３４ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM34_L" runat="server" Text="付帯項目３４(燃料タンク容量（リットル）)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem34', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem34');">
                            <asp:TextBox ID="TxtAddItem34" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem34Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_67">
                    <!-- 付帯項目３５ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM35_L" runat="server" Text="付帯項目３５(モータ駆動)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem35', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem35');">
                            <asp:TextBox ID="TxtAddItem35" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem35Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_68">
                    <!-- 付帯項目３６ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM36_L" runat="server" Text="付帯項目３６(青函トンネル通過装置)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem36', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem36');">
                            <asp:TextBox ID="TxtAddItem36" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem36Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_69">
                    <!-- 付帯項目３７ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM37_L" runat="server" Text="付帯項目３７(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem37', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem37');">
                            <asp:TextBox ID="TxtAddItem37" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem37Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_70">
                    <!-- 付帯項目３８ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM38_L" runat="server" Text="付帯項目３８(北海道限定運用コンテナ)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem38', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem38');">
                            <asp:TextBox ID="TxtAddItem38" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem38Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_71">
                    <!-- 付帯項目３９ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM39_L" runat="server" Text="付帯項目３９(ヤマト専用コンテナ)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem39', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem39');">
                            <asp:TextBox ID="TxtAddItem39" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem39Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_72">
                    <!-- 付帯項目４０ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM40_L" runat="server" Text="付帯項目４０(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem40', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem40');">
                            <asp:TextBox ID="TxtAddItem40" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem40Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_73">
                    <!-- 付帯項目４１ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM41_L" runat="server" Text="付帯項目４１(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem41', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem41');">
                            <asp:TextBox ID="TxtAddItem41" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem41Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_74">
                    <!-- 付帯項目４２ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM42_L" runat="server" Text="付帯項目４２(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem42', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem42');">
                            <asp:TextBox ID="TxtAddItem42" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem42Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_75">
                    <!-- 付帯項目４３ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM43_L" runat="server" Text="付帯項目４３(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem43', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem43');">
                            <asp:TextBox ID="TxtAddItem43" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem43Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_76">
                    <!-- 付帯項目４４ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM44_L" runat="server" Text="付帯項目４４(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem44', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem44');">
                            <asp:TextBox ID="TxtAddItem44" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem44Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_77">
                    <!-- 付帯項目４５ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM45_L" runat="server" Text="付帯項目４５(発送通知)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem45', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem45');">
                            <asp:TextBox ID="TxtAddItem45" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem45Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_78">
                    <!-- 付帯項目４６ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM46_L" runat="server" Text="付帯項目４６(他者所有コンテナ)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem46', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem46');">
                            <asp:TextBox ID="TxtAddItem46" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem46Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_79">
                    <!-- 付帯項目４７ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM47_L" runat="server" Text="付帯項目４７(リース取得コンテナ)" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem47', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem47');">
                            <asp:TextBox ID="TxtAddItem47" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem47Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_80">
                    <!-- 付帯項目４８ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM48_L" runat="server" Text="付帯項目４８(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem48', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem48');">
                            <asp:TextBox ID="TxtAddItem48" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem48Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_81">
                    <!-- 付帯項目４９ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM49_L" runat="server" Text="付帯項目４９(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem49', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem49');">
                            <asp:TextBox ID="TxtAddItem49" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem49Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                <p id="KEY_LINE_82">
                    <!-- 付帯項目５０ -->
                    <span>
                        <asp:Label ID="WF_ADDITEM50_L" runat="server" Text="付帯項目５０(未使用)" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtAddItem50', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtAddItem50');">
                            <asp:TextBox ID="TxtAddItem50" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4" Enabled="false"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblAddItem50Name" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_83">
                    <!-- 床材質コード -->
                    <span>
                        <asp:Label ID="WF_FLOORMATERIAL_L" runat="server" Text="床材質コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtFloorMaterial', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtFloorMaterial');">
                            <asp:TextBox ID="TxtFloorMaterial" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblFloorMaterialName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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
            
            <!-- スクロールバー保管用 -->
            <input id="WF_ClickedScrollTop" runat="server" value="0" type="text" />    
        </div>
 
</asp:Content>
