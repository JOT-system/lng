<%@ Page Title="LNM0003D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0003RekejmDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0003RekejmDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0003WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0003DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0003D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0003D.js")%>'></script>
</asp:Content>
 
<asp:Content ID="LNM0003D" ContentPlaceHolderID="contents1" runat="server">
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
                    <!-- 発駅コード -->
                    <span>
                        <asp:Label ID="WF_DEPSTATION_LABEL" runat="server" Text="発駅コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span id="TxtDepStationEvent" ondblclick="Field_DBclick('TxtDepStation', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtDepStation');">
                            <asp:TextBox ID="TxtDepStation" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDepStationName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_5" class="flexible">
                    <!-- 発受託人コード -->
                    <span>
                        <asp:Label ID="WF_DEPTRUSTEECD_LABEL" runat="server" Text="発受託人コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span id="TxtDepTrusteeCdEvent" ondblclick="Field_DBclick('TxtDepTrusteeCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtDepTrusteeCd');">
                            <asp:TextBox ID="TxtDepTrusteeCd" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="5"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDepTrusteeCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL" style="display:none;"></asp:Label>
                    </span>
                    <!-- 発受託人名称 -->
                    <span>
                        <asp:Label ID="WF_DEPTRUSTEENM_LABEL" runat="server" Text="発受託人名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtDepTrusteeNm" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="32"></asp:TextBox>
                    </span>
                    <!-- 発受託人名称（カナ） -->
                    <span>
                        <asp:Label ID="WF_DEPTRUSTEESUBKANA_LABEL" runat="server" Text="発受託人名称（カナ）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtDepTrusteeSubKana" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_6" class="flexible">
                    <!-- 発受託人サブコード -->
                    <span>
                        <asp:Label ID="WF_DEPTRUSTEESUBCD_LABEL" runat="server" Text="発受託人サブコード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span id="TxtDepTrusteeSubCdEvent" ondblclick="Field_DBclick('TxtDepTrusteeSubCd', <%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtDepTrusteeSubCd');">
                            <asp:TextBox ID="TxtDepTrusteeSubCd" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="3"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDepTrusteeSubCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL" style="display:none;"></asp:Label>
                    </span>
                    <!-- 発受託人サブ名称 -->
                    <span>
                        <asp:Label ID="WF_DEPTRUSTEESUBNM_LABEL" runat="server" Text="発受託人サブ名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtDepTrusteeSubNm" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="18"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_7" class="flexible">
                    <!-- 取引先コード -->
                    <span>
                        <asp:Label ID="WF_TORICODE_LABEL" runat="server" Text="取引先コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtToriCode', <%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);" onchange="TextBox_change('TxtToriCode');">
                            <asp:TextBox ID="TxtToriCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="10"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblToriCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>
                
                <p id="KEY_LINE_8" class="flexible">
                    <!-- 適格請求書登録番号 -->
                        <span>
                        <asp:Label ID="WF_ELIGIBLEINVOICENUMBER_LABEL" runat="server" Text="適格請求書登録番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtInvNo" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- 請求項目 計上店コード -->
                    <span>
                        <asp:Label ID="WF_INVKEIJYOBRANCHCD_LABEL" runat="server" Text="請求項目 計上店コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtInvKeijyoBranchCd', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtInvKeijyoBranchCd');">
                            <asp:TextBox ID="TxtInvKeijyoBranchCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblInvKeijyoBranchCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 請求項目 請求サイクル -->
                    <span>
                        <asp:Label ID="WF_INVCYCL_LABEL" runat="server" Text="請求項目 請求サイクル" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtInvCycl', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtInvCycl');">
                            <asp:TextBox ID="TxtInvCycl" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblInvCyclName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 請求項目 請求書提出部店 -->
                    <span>
                        <asp:Label ID="WF_INVFILINGDEPT_LABEL" runat="server" Text="請求項目 請求書提出部店" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtInvFilingDept', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtInvFilingDept');">
                            <asp:TextBox ID="TxtInvFilingDept" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblInvFilingDeptName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_12">
                    <!-- 請求項目 請求書決済区分 -->
                    <span>
                        <asp:Label ID="WF_INVKESAIKBN_LABEL" runat="server" Text="請求項目 請求書決済区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtInvKesaiKbn', <%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);" onchange="TextBox_change('TxtInvKesaiKbn');">
                            <asp:TextBox ID="TxtInvKesaiKbn" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblInvKesaiKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_13">
                    <!-- 請求項目 請求書細分コード -->
                    <span>
                        <asp:Label ID="WF_INVSUBCD_LABEL" runat="server" Text="請求項目 請求書細分コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtInvSubCd', <%=LIST_BOX_CLASSIFICATION.LC_KEKSBM%>);" onchange="TextBox_change('TxtInvSubCd');">
                            <asp:TextBox ID="TxtInvSubCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblInvSubCdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_14">
                    <!-- 請求項目 請求摘要 -->
                    <span class="colCodeOnly">
                        <asp:Label ID="WF_INVTEKIYO_LABEL" runat="server" Text="請求項目 請求摘要" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtInvTekiyo" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="42" ReadOnly="true"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_15">
                    <!-- 支払項目 費用計上店コード -->
                    <span>
                        <asp:Label ID="WF_PAYKEIJYOBRANCHCD_LABEL" runat="server" Text="支払項目 費用計上店コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtPayKeijyoBranchCd', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtPayKeijyoBranchCd');">
                            <asp:TextBox ID="TxtPayKeijyoBranchCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblPayKeijyoBranchCDName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_16">
                    <!-- 支払項目 支払書提出支店 -->
                    <span>
                        <asp:Label ID="WF_PAYFILINGBRANCH_LABEL" runat="server" Text="支払項目 支払書提出支店" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtPayFilingBranch', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtPayFilingBranch');">
                            <asp:TextBox ID="TxtPayFilingBranch" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblPayFilingBranchName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_17">
                    <!-- 支払項目 消費税計算単位 -->
                    <span>
                        <asp:Label ID="WF_TAXCALCUNIT_LABEL" runat="server" Text="支払項目 消費税計算単位" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtTaxCalcUnit', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('TxtTaxCalcUnit');">
                            <asp:TextBox ID="TxtTaxCalcUnit" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="2"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblTaxCalcUnitName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_18">
                    <!-- 支払項目 決済区分 -->
                    <span>
                        <asp:Label ID="WF_PAYKESAIKBN_LABEL" runat="server" Text="支払項目 決済区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayKesaiKbn" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_19" class="flexible">
                    <!-- 支払項目 銀行コード -->
                    <span>
                        <asp:Label ID="WF_PAYBANKCD_LABEL" runat="server" Text="支払項目 銀行コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtPayBankCd', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('TxtPayBankCd');">
                            <asp:TextBox ID="TxtPayBankCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblPayBankCd" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                    <!-- 支払項目 銀行支店コード -->
                    <span>
                        <asp:Label ID="WF_PAYBANKBRANCHCD_LABEL" runat="server" Text="支払項目 銀行支店コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtPayBankBranchCd', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('TxtPayBankBranchCd');">
                            <asp:TextBox ID="TxtPayBankBranchCd" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblPayBankBranchCd" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_20">
                    <!-- 支払項目 口座種別 -->
                    <span>
                        <asp:Label ID="WF_PAYACCOUNTTYPE_LABEL" runat="server" Text="支払項目 口座種別" CssClass="WF_TEXT_LEFT"></asp:Label>
                         <asp:DropDownList ID="ddlPayAccountTypeName" runat="server" CssClass="ddlSelectControl" onchange="PayAccountTypeNameOnchange()">
                            <asp:ListItem Text="" Value="0"></asp:ListItem>
                            <asp:ListItem Text="普通" Value="1"></asp:ListItem>
                            <asp:ListItem Text="当座" Value="2"></asp:ListItem>
                        </asp:DropDownList>
                    </span>
                </p>

                <p id="KEY_LINE_21">
                    <!-- 支払項目 口座番号 -->
                    <span>
                        <asp:Label ID="WF_PAYACCOUNTNO_LABEL" runat="server" Text="支払項目 口座番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayAccountNo" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="8"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_22">
                    <!-- 支払項目 口座名義人 -->
                    <span>
                        <asp:Label ID="WF_PAYACCOUNTNM_LABEL" runat="server" Text="支払項目 口座名義人" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayAccountNm" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="30"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_23">
                    <!-- 支払項目 支払摘要 -->
                    <span class="colCodeOnly">
                        <asp:Label ID="WF_PAYTEKIYO_LABEL" runat="server" Text="支払項目 支払摘要" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayTekiyo" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="42"></asp:TextBox>
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

        <!-- multiSelect レイアウト -->
        <!-- 銀行コード単一選択 -->
        <MSINC:multiselect runat="server" id="mspBankCodeSingle" />
        <!-- 支店コード単一選択 -->
        <MSINC:multiselect runat="server" id="mspBankBranchCodeSingle" />
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
