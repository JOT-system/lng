<%@ Page Title="LNM0024D" Language="vb" AutoEventWireup="false" CodeBehind="LNM0024KekkjmDetail.aspx.vb" Inherits="JOTWEB_LNG.LNM0024KekkjmDetail"%>

<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNM0024WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNM0024DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNM0024D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNM0024D.js")%>'></script>
</asp:Content>

<asp:Content ID="LNM0024D" ContentPlaceHolderID="contents1" runat="server">
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
                <!-- 取引先コード -->
                <span>
                    <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="txtToriCodeEvent" ondblclick="Field_DBclick('txtToriCode', <%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);" onchange="CodeName_OnChange('txtToriCode','hdnSelectTori','txtToriCode','lblToriCodeName','txtToriCode',<%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);">
                        <asp:TextBox ID="txtToriCode" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="10"></asp:TextBox>
                    </span>
                    <a id="WF_PAYEENAME_TEXT">
                        <asp:textbox ID="lblToriCodeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL" Text="" ></asp:textbox>
                    </a>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_6">
                <!-- 請求書提出部店 -->
                <span>
                    <asp:Label ID="WF_INVFILINGDEPT_L" runat="server" Text="請求書提出部店" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="txtInvFilingDeptEvent" ondblclick="Field_DBclick('txtInvFilingDept', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="CodeName_OnChange('txtInvFilingDeptEvent','hdnSelectTori','txtInvFilingDept','txtInvFilingDeptEvent','txtInvFilingDeptEvent',<%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);">
                        <asp:TextBox ID="txtInvFilingDept" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="6"></asp:TextBox>
                        <asp:DropDownList ID="hdnSelectTori" runat="server" ></asp:DropDownList>
                    </span>
                    <asp:Label ID="lblInvFilingDeptName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_7">
                <!-- 請求書決済区分 -->
                <span>
                    <asp:Label ID="WF_INVKESAIKBN_L" runat="server" Text="請求書決済区分" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="txtInvKesaiKbn" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                    <asp:Label ID="lblInvKesaiKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_8">
                <!-- 取引先名称 -->
                <span>
                    <asp:Label ID="WF_TORINAME_L" runat="server" Text="取引先名称" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="txtToriName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                    <asp:Label ID="lblToriName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_9">
                <!-- 取引先略称 -->
                <span>
                    <asp:Label ID="WF_TORINAMES_L" runat="server" Text="取引先略称" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtToriNameS" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="25"></asp:TextBox>
                    <asp:Label ID="lblToriNameS" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_10">
                <!-- 取引先カナ名称 -->
                <span>
                    <asp:Label ID="WF_TORINAMEKANA_L" runat="server" Text="取引先カナ名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtToriNameKana" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                    <asp:Label ID="lblToriNameKana" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>


            <p id="KEY_LINE_11">
                <!-- 取引先部門名称 -->
                <span>
                    <asp:Label ID="WF_TORIDIVNAME_L" runat="server" Text="取引先部門名称" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtToriDivName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="25"></asp:TextBox>
                    <asp:Label ID="lblToriDivName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_12">
                <!-- 取引先担当者 -->
                <span>
                    <asp:Label ID="WF_TORICHARGE_L" runat="server" Text="取引先担当者" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtToriCharge" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                    <asp:Label ID="lblToriCharge" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_13">
                <!-- 取引先区分 -->
                <span>
                    <asp:Label ID="WF_TORIKBN_L" runat="server" Text="取引先区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtToriKbn" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                    <asp:Label ID="lblToriKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>


            <p id="KEY_LINE_14">
                <!-- 郵便番号（上） -->
                <span>
                    <asp:Label ID="WF_POSTNUM1_L" runat="server" Text="郵便番号（上）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtPostNum1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                    <asp:Label ID="lblPostNum1" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_15">
                <!-- 郵便番号（下） -->
                <span>
                    <asp:Label ID="WF_POSTNUM2_L" runat="server" Text="郵便番号（下）" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtPostNum2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                    <asp:Label ID="lblPostNum2" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>


            <p id="KEY_LINE_16">
                <!-- 住所１ -->
                <span>
                    <asp:Label ID="WF_ADDR1_L" runat="server" Text="住所１" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtAddr1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="60"></asp:TextBox>
                    <asp:Label ID="lblAddr1" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_17">
                <!-- 住所２ -->
                <span>
                    <asp:Label ID="WF_ADDR2_L" runat="server" Text="住所２" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtAddr2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="60"></asp:TextBox>
                    <asp:Label ID="lblAddr2" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_18">
                <!-- 住所３ -->
                <span>
                    <asp:Label ID="WF_ADDR3_L" runat="server" Text="住所３" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtAddr3" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="60"></asp:TextBox>
                    <asp:Label ID="lblAddr3" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_19">
                <!-- 住所４ -->
                <span>
                    <asp:Label ID="WF_ADDR4_L" runat="server" Text="住所４" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtAddr4" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="60"></asp:TextBox>
                    <asp:Label ID="lblAddr4" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>


            <p id="KEY_LINE_20">
                <!-- 電話番号 -->
                <span>
                    <asp:Label ID="WF_TEL_L" runat="server" Text="電話番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtTel" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="15"></asp:TextBox>
                    <asp:Label ID="lblTel" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_21">
                <!-- ＦＡＸ番号 -->
                <span>
                    <asp:Label ID="WF_FAX_L" runat="server" Text="ＦＡＸ番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtFax" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="15"></asp:TextBox>
                    <asp:Label ID="lblFax" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_22">
                <!-- メールアドレス -->
                <span>
                    <asp:Label ID="WF_MAIL_L" runat="server" Text="メールアドレス" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtMail" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="128"></asp:TextBox>
                    <asp:Label ID="lblMail" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_23">
                <!-- 銀行コード -->
                <span>
                    <asp:Label ID="WF_BANKCODE_L" runat="server" Text="銀行コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtBankCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                    <asp:Label ID="lblBankCode" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_24">
                <!-- 支店コード -->
                <span>
                    <asp:Label ID="WF_BANKBRANCHCODE_L" runat="server" Text="支店コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtBankBranchCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                    <asp:Label ID="lblBankBranchCode" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>


            <p id="KEY_LINE_25">
                <!-- 口座種別 -->
                <span>
                    <asp:Label ID="WF_ACCOUNTTYPE_L" runat="server" Text="口座種別" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <span id="txtAccountTypeEvent" ondblclick="Field_DBclick('txtAccountType', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('txtAccountType');">
                        <asp:TextBox ID="txtAccountType" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="1"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblAccountType" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>
            
            <p id="KEY_LINE_26">
                <!-- 口座番号 -->
                <span>
                    <asp:Label ID="WF_ACCOUNTNUMBER_L" runat="server" Text="口座番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtAccountNumber" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>
                    <asp:Label ID="lblAccountNumber" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_27">
                <!-- 口座名義 -->
                <span>
                    <asp:Label ID="WF_ACCOUNTNAME_L" runat="server" Text="口座名義" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtAccountName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="30"></asp:TextBox>
                    <asp:Label ID="lblAccountName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>


            <p id="KEY_LINE_28">
                <!-- 社内口座コード -->
                <span>
                    <asp:Label ID="WF_INACCOUNTCD_L" runat="server" Text="社内口座コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtInAccountCd" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>
                    <asp:Label ID="lblInAccountCd" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>
            

            <p id="KEY_LINE_29">
                <!-- 税計算区分 -->
                <span>
                    <asp:Label ID="WF_TAXCALCULATION_L" runat="server" Text="税計算区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtTaxcalculation" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                    <asp:Label ID="lblTaxcalculation" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>


            <p id="KEY_LINE_30">
                <!-- 入金日 -->
                <span>
                    <asp:Label ID="WF_DEPOSITDAY_L" runat="server" Text="入金日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtDepositDay" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                    <asp:Label ID="lblDepositDay" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_31">
                <!-- 入金月区分 -->
                <span>
                    <asp:Label ID="WF_DEPOSITMONTHKBN_L" runat="server" Text="入金月区分" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    <span id="txtDepositMonthKbnEvent" ondblclick="Field_DBclick('txtDepositMonthKbn', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('txtDepositMonthKbn');">
                        <asp:TextBox ID="txtDepositMonthKbn" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="1"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblDepositMonthKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>
            
            <p id="KEY_LINE_32">
                <!-- 計上締日 -->
                <span>
                    <asp:Label ID="WF_CLOSINGDAY_L" runat="server" Text="計上締日" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtClosingday" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="2"></asp:TextBox>
                    <asp:Label ID="lblClosingday" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_33">
                <!-- 計上月区分 -->
                <span>
                    <asp:Label ID="WF_ACCOUNTINGMONTH_L" runat="server" Text="計上月区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                    <span id="txtAccountingMonthEvent" ondblclick="Field_DBclick('txtAccountingMonth', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>);" onchange="TextBox_change('txtAccountingMonth');">
                        <asp:TextBox ID="txtAccountingMonth" runat="server" CssClass="WF_TEXTBOX_CSS disabledboxIcon" MaxLength="1"></asp:TextBox>
                    </span>
                    <asp:Label ID="lblAccountingMonthName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p>&nbsp;</p>

            <p id="KEY_LINE_34">
                <!-- 伝票摘要１ -->
                <span>
                    <asp:Label ID="WF_SLIPDESCRIPTION1_L" runat="server" Text="伝票摘要１" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtSlipDescription1" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="21"></asp:TextBox>
                    <asp:Label ID="lblSlipDescription1" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>

            <p id="KEY_LINE_35">
                <!-- 伝票摘要２ -->
                <span>
                    <asp:Label ID="WF_SLIPDESCRIPTION2_L" runat="server" Text="伝票摘要２" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtSlipDescription2" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="21"></asp:TextBox>
                    <asp:Label ID="lblSlipDescription2" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </span>
            </p>
            
            <p>&nbsp;</p>

            <p id="KEY_LINE_36">
                <!-- 運賃翌月未決済区分 -->
                <span>
                    <asp:Label ID="WF_NEXTMONTHUNSETTLEDKBN_L" runat="server" Text="運賃翌月未決済区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="txtNextMonthUnSettledKbn" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                    <asp:Label ID="lblNextMonthUnSettledKbnName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
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
    <!-- 受託人単一選択 -->
    <MSINC:multiselect runat="server" id="mspTrusteeSingle" />
    <!-- 荷主単一選択 -->
    <MSINC:multiselect runat="server" id="mspShipperSingle" />
    <!-- 取引先単一選択 -->
    <MSINC:multiselect runat="server" id="mspToriSingle" />
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
        <!-- スクロールバー保管用 -->
        <input id="WF_ClickedScrollTop" runat="server" value="0" type="text" />    
    </div>

</asp:Content>
