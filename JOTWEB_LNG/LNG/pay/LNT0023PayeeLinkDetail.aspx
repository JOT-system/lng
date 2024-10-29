<%@ Page Title="LNT0023D" Language="vb" AutoEventWireup="false" CodeBehind="LNT0023PayeeLinkDetail.aspx.vb" Inherits="JOTWEB_LNG.LNT0023PayeeLinkDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0023WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<asp:Content ID="LNT0023DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0023D.css")%>' rel="stylesheet" type="text/css"/>
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0023D.js")%>'></script>
</asp:Content>
 
<asp:Content ID="LNT0023D" ContentPlaceHolderID="contents1" runat="server">
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
                    <!-- 支払先コード -->
                    <span>
                        <asp:Label ID="WF_TORICODE_LABEL" runat="server" Text="支払先コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtToriCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10" onchange="TxtToriCodeOnchange()"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_5">
                    <!-- 顧客コード -->
                    <span>
                        <asp:Label ID="WF_CLIENTCODE_LABEL" runat="server" Text="顧客コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtClientCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="15" ReadOnly="true"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- インボイス登録番号 -->
                    <span>
                        <asp:Label ID="WF_INVOICENUMBER_LABEL" runat="server" Text="インボイス登録番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtInvoiceNumber" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="14"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 顧客名 -->
                    <span>
                        <asp:Label ID="WF_CLIENTNAME_LABEL" runat="server" Text="顧客名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtClientName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="32" width="600px" ReadOnly="true"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- 会社名 -->
                    <span>
                        <asp:Label ID="WF_TORINAME_LABEL" runat="server" Text="会社名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtToriName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="32" width="600px" onchange="TxtToriNameOnchange()"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- 部門名 -->
                    <span>
                        <asp:Label ID="WF_TORIDIVNAME_LABEL" runat="server" Text="部門名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtToriDivName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="32" width="600px" onchange="TxtToriNameOnchange()"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 振込先銀行コード -->
                    <span>
<%--                        <asp:Label ID="WF_PAYBANKCODE_LABEL" runat="server" Text="振込先銀行コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayBankCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>--%>
                        <asp:Label ID="WF_PAYBANKCODE_L" runat="server" Text="振込先銀行コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtPayBankCode', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('TxtPayBankCode');">
                            <asp:TextBox ID="TxtPayBankCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 振込先銀行名 -->
                    <span>
                        <asp:Label ID="WF_PAYBANKNAME_LABEL" runat="server" Text="振込先銀行名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayBankName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="30"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_12">
                    <!-- 振込先銀行名カナ -->
                    <span>
                        <asp:Label ID="WF_PAYBANKNAMEKANA_LABEL" runat="server" Text="振込先銀行名カナ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayBankNameKana" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="30"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_13">
                    <!-- 振込先支店コード -->
                    <span>
<%--                        <asp:Label ID="WF_PAYBANKBRANCHCODE_LABEL" runat="server" Text="振込先支店コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayBankBranchCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>--%>
                        <asp:Label ID="WF_PAYBANKBRANCHCODE_L" runat="server" Text="振込先支店コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtPayBankBranchCode', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('TxtPayBankBranchCode');">
                            <asp:TextBox ID="TxtPayBankBranchCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="3"></asp:TextBox>
                        </span>
                    </span>
                </p>

                <p id="KEY_LINE_14">
                    <!-- 振込先支店名 -->
                    <span>
                        <asp:Label ID="WF_PAYBANKBRANCHNAME_LABEL" runat="server" Text="振込先支店名" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayBankBranchName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="30"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_15">
                    <!-- 振込先支店名カナ -->
                    <span>
                        <asp:Label ID="WF_PAYBANKBRANCHNAMEKANA_LABEL" runat="server" Text="振込先支店名カナ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayBankBranchNameKana" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="30"></asp:TextBox>
                    </span>
                </p>

               <p id="KEY_LINE_16">
                    <!-- 預金種別 -->
                    <span>
                        <asp:Label ID="WF_PAYACCOUNTTYPENAME_LABEL" runat="server" Text="預金種別" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <%--<asp:TextBox ID="TxtPayAccountTypeName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>--%>
                         <asp:DropDownList ID="ddlPayAccountTypeName" runat="server" CssClass="ddlSelectControl" onchange="PayAccountTypeNameOnchange()">
                            <asp:ListItem Text="" Value=""></asp:ListItem>
                            <asp:ListItem Text="普通" Value="普通"></asp:ListItem>
                            <asp:ListItem Text="当座" Value="当座"></asp:ListItem>
                        </asp:DropDownList>
                    </span>
                </p>

                <p id="KEY_LINE_17">
                    <!-- 預金種別コード -->
                    <span>
                        <asp:Label ID="WF_PAYACCOUNTTYPE_LABEL" runat="server" Text="預金種別コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayAccountType" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_18">
                    <!-- 口座番号 -->
                    <span>
                        <asp:Label ID="WF_PAYACCOUNT_LABEL" runat="server" Text="口座番号" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayAccount" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="8"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_19">
                    <!-- 口座名義 -->
                    <span>
                        <asp:Label ID="WF_PAYACCOUNTNAME_LABEL" runat="server" Text="口座名義" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayAccountName" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="30"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_20">
                    <!-- 支払元銀行コード -->
                    <span>
<%--                        <asp:Label ID="WF_PAYORBANKCODE_LABEL" runat="server" Text="支払元銀行コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtPayorBankCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="4"></asp:TextBox>--%>
                        <asp:Label ID="WF_PAYORBANKCODE_L" runat="server" Text="支払元銀行コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtPayorBankCode', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('TxtPayorBankCode');">
                            <asp:TextBox ID="TxtPayorBankCode" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="4"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblPayorBankName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_21">
                    <!-- 消費税計算処理区分 -->
                    <span>
                        <asp:Label ID="WF_PAYTAXCALCUNIT_LABEL" runat="server" Text="消費税計算処理区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <%--<asp:TextBox ID="TxtPayTaxCalcUnit" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="10"></asp:TextBox>--%>
                         <asp:DropDownList ID="ddlPayTaxCalcUnit" runat="server" CssClass="ddlSelectControl">
                            <asp:ListItem Text="総額" Value="総額"></asp:ListItem>
                            <asp:ListItem Text="明細" Value="明細"></asp:ListItem>
                        </asp:DropDownList>
                    </span>
                </p>

                <p id="KEY_LINE_22">
                    <!-- 連携状態区分 -->
                    <span>
                        <asp:Label ID="WF_LINKSTATUS_LABEL" runat="server" Text="連携状態区分" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtLinkStatus" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="1"></asp:TextBox>
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
        <!-- 支払元銀行単一選択 -->
        <MSINC:multiselect runat="server" id="mspBankAccountSingle" />

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
