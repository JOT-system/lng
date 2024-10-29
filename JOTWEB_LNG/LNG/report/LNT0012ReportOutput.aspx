<%@ Page Title="LNT0012S" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0012ReportOutput.aspx.vb" Inherits="JOTWEB_LNG.LNT0012ReportOutput" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>
<%@ Import Namespace="JOTWEB_LNG.GRIS0003SRightBox" %>

<%@ Register Src="~/inc/GRIS0003SRightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0012WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/GRC0002SELECTIONPOPUPWORKINC.ascx" TagName="multiselect" TagPrefix="MSINC"  %>

<%@ Register src="../inc/GRC0001TILESELECTORWRKINC.ascx" tagname="tilelist" tagprefix="MSINC" %>

<asp:Content id="LNT0012SH" contentplaceholderid="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0012.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0012S.js")%>'></script>
</asp:Content>

<asp:Content ID="LNT0012S" ContentPlaceHolderID="contents1" runat="server">
    <!-- 全体レイアウト　searchbox -->
    <div class="searchbox" id="searchbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide"></div>
            <div class="rightSide">
                <input type="button" id="WF_ButtonOUTPUT" class="btn-sticky" value="出力" onclick="ButtonClick('WF_ButtonOUTPUT');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div> <!-- End actionButtonBox -->

        <!-- ○ 変動項目 ○ -->
        <div class="inputBox">

            <!-- 帳票 -->
            <div class="inputItem">
                <a id="WF_REPORT_LABEL">
                    <asp:Label ID="HeaderReport" runat="server" Text="帳票" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                </a>
                <a class="ef" id="WF_REPORT" ondblclick="Field_DBclick('TxtReportId', <%=LIST_BOX_CLASSIFICATION.LC_REPORT%>);" onchange="TextBox_change('TxtReportId');">
                    <asp:TextBox ID="TxtReportId" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="8"></asp:TextBox>
                </a>
                <a id="WF_REPORT_TEXT">
                    <asp:Label ID="TxtReportName" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
<%--            <div class="divLbl">
                <a id="WF_REPORT_LABEL">帳票</a>
            </div>
            <span class="span1"></span>
            <div class="divDdlArea">
                <div class="divDdlAreaLeft">
                    <asp:DropDownList ID="ddlReportId" runat="server" ClientIDMode="Predictable" CssClass="ddlSelectReport" OnChange="SelectDropDownList_OnChange('WF_SelectReportId_OnChange')"></asp:DropDownList>
                </div>
            </div>--%>

            <!-- 対象支店 -->
            <div class="inputItem" id="WF_ORG_AREA">
                <a></a>
                <a id="WF_ORG_LABEL">対象支店</a>
                <a id="WF_ORG_MASSEAGE">※支店は1支店選択または全支店選択</a>
                <a id="WF_ORG_MASSEAGE2">※支店は1支店選択または複数支店選択</a>
            </div>
            <div id="WF_ORGCODE_SELECT_DDL" class="inputItem">
                <a id="WF_ORG_LABEL2">対象支店</a>
                <a class ="ef">
                    <asp:DropDownList ID="ddlSelectOffice" runat="server" ClientIDMode="Predictable"></asp:DropDownList>
                </a>
            </div>
            <div id="WF_ORGCODE_LEASE_SELECT_DDL" class="inputItem">
                <a id="WF_ORG_LABEL3">対象支店</a>
                <a class ="ef">
                    <asp:DropDownList ID="ddlSelectLeaseOffice" runat="server" ClientIDMode="Predictable"></asp:DropDownList>
                </a>
            </div>
            <div id="WF_ORGCODE_ALL_SELECT">
                <MSINC:tilelist ID="WF_ORGCODE_ALL" runat="server" />
            </div>
            <div id="WF_ORGCODE_SELECT">
                <MSINC:tilelist ID="WF_ORGCODE" runat="server" />
            </div>
            <div id="WF_ORGCODE_MULTIPLE_SELECT">
                <MSINC:tilelist ID="WF_ORGCODE_MULTIPLE" runat="server" />
            </div>
            <!-- 年月日(開始） -->
            <div id="WF_STYMD_TITLE" class="inputItem">
                <a id="WF_STYMD_LABEL">年月日（開始）</a>
                <a id="WF_SELECDATE_LABEL">対象日付</a>
            </div>
            <div id="WF_STYMD_TEXT" class="inputItem">
                <a></a>
                <a class="ef" id="WF_STYMD" ondblclick="Field_DBclick('TxtStYMDCode', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtStYMDCode" runat="server" CssClass="calendarIcon"  onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </a>
            </div>
            <!-- 年月日(終了） -->
            <div class="inputItem" id="WF_ENDYMD_AREA">
                <a id="WF_ENDYMD_LABEL">年月日（終了）</a>
                <a class="ef" id="WF_ENDYMD" ondblclick="Field_DBclick('TxtEndYMDCode', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtEndYMDCode" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </a>
            </div>
            <!-- 対象年月 -->
            <div id="WF_STYM_TITLE_AREA" class="inputItem2">
                <a id="WF_TARGETYM_LABEL">対象年月</a>
                <a id="WF_BILLINGYM_LABEL">請求年月</a>
                <a id="WF_BILLINGFROMYM_LABEL">請求年月(開始)</a>
            </div>
            <div id="WF_STYM_TEXT" class="inputItem">
                <a></a>
                <a class ="ef" id ="WF_TARGETYM">
                    <asp:TextBox ID="TxtDownloadMonth" runat ="server" onblur="MsgClear();" MaxLength="7" data-monthpicker ="1"></asp:TextBox>
                </a>
                <a id="WF_BILLING_MASSEAGE">
                    <asp:Label ID="LblBillingMasseage" runat="server" CssClass="WF_TEXT">※指定した場合、発送年月日よりも優先されます</asp:Label>
                </a>
            </div> 
            <!-- 請求年月(終了) -->
            <div class="inputItem" id ="WF_BILLINGTOYM_AREA">
                <a id="WF_BILLINGTOYM_LABEL">請求年月(終了)</a>
                <a class ="ef" id ="WF_BILLINGTOYM">
                    <asp:TextBox ID="TxtBllingToYM" runat ="server" onblur="MsgClear();" MaxLength="7" data-monthpicker ="1"></asp:TextBox>
                </a>
            </div>
            
            <!-- 発送日FROM -->
            <div id="WF_SHIPYMDFROM_AREA" class="inputItem">
                <a id="WF_SHIPYMDFROM_LABEL">発送年月日FROM</a>
                <a class="ef" id="WF_SHIPYMDFROM" ondblclick="Field_DBclick('TxtShipYMDFrom', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtShipYMDFrom" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </a>
                <a id="WF_SHIPYMDFROM_TEXT">
                    <asp:Label ID="LblShipYMDForm" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 発送日TO -->
            <div id="WF_SHIPYMDTO_AREA" class="inputItem">
                <a id="WF_SHIPYMDTO_LABEL">発送年月日TO</a>
                <a class="ef" id="WF_SHIPYMDTO" ondblclick="Field_DBclick('TxtShipYMDTo', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                    <asp:TextBox ID="TxtShipYMDTo" runat="server" CssClass="calendarIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                </a>
                <a id="WF_SHIPYMDTO_TEXT">
                    <asp:Label ID="LblShipYMDTo" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 発駅 -->
            <div class="inputItem">
                <a id="WF_DEPSTATION_LABEL">発駅</a>
                <a id="WF_NOWSTATION_LABEL">現在駅</a>
            </div>
            <div class="inputItem" id="WF_STA_AREA">
                <a></a>
                <a class="ef" id="WF_DEPSTA" ondblclick="Field_DBclick('TxtStaCode', <%=LIST_BOX_CLASSIFICATION.LC_STATION%>);" onchange="TextBox_change('TxtStaCode');">
                    <asp:TextBox ID="TxtStaCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="6"></asp:TextBox>
                </a>
                <a id="WF_DEPSTATION_TXT">
                    <asp:Label ID="LblStaName" runat="server" CssClass="WF_TEXT"></asp:Label>
               </a>
            </div>

            <!-- コンテナ留置先一覧 -->
            <div class="inputItem" id="WF_PRT0003_AREA">
                <a id="WF_MODE_LABEL">
                    <asp:Label ID="HeaderMode" runat="server" Text="処理" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                </a>
                <a class="ef" id="WF_MODE" ondblclick="Field_DBclick('TxtMode', <%=LIST_BOX_CLASSIFICATION.LC_MODE%>);"onchange="TextBox_change('TxtMode');">
                    <asp:TextBox ID="TxtMode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </a>
                <a id="WF_MODE_TEXT">
                    <asp:Label ID="LblMode" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- コンテナ動静表 -->
            <div id="WF_PRT0005_AREA">
                <!-- コンテナ記号 -->
                <div class="inputItem">
                    <a id="WF_CTNTYPE_LABEL">コンテナ記号</a>
                    <a class="ef" id="WF_CTNTYPE" ondblclick="Field_DBclick('TxtCtnType',<%=LIST_BOX_CLASSIFICATION.LC_CTNTYPE%>);">
                        <asp:TextBox ID="TxtCtnType" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="5"></asp:TextBox>
                    </a>
                </div>
                <div class="inputItem">
                    <!-- コンテナ番号(FROM) -->
                    <a id="WF_STCTNNO_LABEL">コンテナ番号(開始)</a>
                    <a class="ef" id="WF_STCTNNO" ondblclick="Field_DBclick('TxtStCtnNo',<%=LIST_BOX_CLASSIFICATION.LC_CTNNO%>);">
                        <asp:TextBox ID="TxtStCtnNo" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="8"></asp:TextBox>
                    </a>
                </div>
                <div class="inputItem">
                    <!-- コンテナ番号(TO) -->
                    <a id="WF_ENDCTNNO_LABEL">コンテナ番号(終了)</a>
                    <a class="ef" id="WF_ENDCTNNO" ondblclick="Field_DBclick('TxtEndCtnNo',<%=LIST_BOX_CLASSIFICATION.LC_CTNNO%>);">
                        <asp:TextBox ID="TxtEndCtnNo" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="8"></asp:TextBox>
                    </a>
                </div>
                <!-- 経理資産区分 -->
                <div class="inputItem">
                    <a id="WF_ACCOUNTINGASSETSKBN_L">経理資産区分</a>
                    <asp:DropDownList ID="WF_ACCOUNTINGASSETSKBN_DDL" runat="server" />
                </div>
<%--                    <a id="WF_JURISDICTION_LABEL">
                        <asp:Label ID="HeaderJurisdiction" runat="server" Text="経理資産区分" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    </a>
                    <a class="ef" id="WF_JURISDICTION" ondblclick="Field_DBclick('TxtJurisdiction',<%=LIST_BOX_CLASSIFICATION.LC_ACCOUNTINGASSETSKBN%>);" onchange="TextBox_change('TxtJurisdiction');">
                        <asp:TextBox ID="TxtJurisdiction" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                    </a>
                    <a id="WF_JURISDICTION_TEXT">
                        <asp:Label ID="LblJurisdiction" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
                <!-- 検索区分 -->
                <div class="inputItem">
                    <a id="WF_SEARCH_LABEL">
                        <asp:Label ID="HeaderSearch" runat="server" Text="検索区分" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    </a>
                    <a class="ef" id="WF_SEARCH" ondblclick="Field_DBclick('TxtSearch',<%=LIST_BOX_CLASSIFICATION.LC_SEARCH%>);" onchange="TextBox_change('TxtSearch');">
                        <asp:TextBox ID="TxtSearch" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                    </a>
                    <a id="WF_SEARCH_TEXT">
                        <asp:Label ID="LblSearch" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>--%>
                <!-- 停滞日数 -->
                <div class="inputItem">
                    <a id="WF_STAGNATION_LABEL">停滞日数</a>
                    <a class="ef" id="WF_STAGNATION">
                        <asp:TextBox ID="TxtStagnation" runat="server" onblur="MsgClear();" MaxLength="4"></asp:TextBox>
                    </a>
                </div>
            </div>

            <!-- 発駅・通運別合計表 -->
            <div id="WF_PRT0006_AREA">
                <!-- 発受託人 -->
                <div class="inputItem">
                    <a id="WF_DEPTRUSTEE_LABEL">発受託人</a>
                    <a class="ef" id="WF_DEPTRUSTEE" ondblclick="Field_DBclick('TxtDepTrustee',<%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtDepTrustee');">
                        <asp:TextBox ID="TxtDepTrustee" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="5"></asp:TextBox>
                    </a>
                    <a id="WF_DEPTRUSTEE_TEXT">
                        <asp:Label ID="LblDepTrustee" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
                <!-- 発受託人サブ -->
                <div class="inputItem">
                    <a id="WF_DEPTRUSTEESUB_LABEL">発受託人サブ</a>
                    <a class="ef" id="WF_DEPTRUSTEESUB" ondblclick="Field_DBclick('TxtDepTrusteeSub',<%=LIST_BOX_CLASSIFICATION.LC_REKEJM%>);" onchange="TextBox_change('TxtDepTrusteeSub');">
                        <asp:TextBox ID="TxtDepTrusteeSub" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="3"></asp:TextBox>
                    </a>
                    <a id="WF_DEPTRUSTEESUB_TEXT">
                        <asp:Label ID="LblDepTrusteeSub" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
                <!-- 並び順 -->
                <div class="inputItem">
                    <a id="WF_SORT_LABEL">
                        <asp:Label ID="HeaderSort" runat="server" Text="並び順" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </a>
                    <a class="ef" id="WF_SORT" ondblclick="Field_DBclick('TxtSort',<%=LIST_BOX_CLASSIFICATION.LC_SORT%>);" onchange="TextBox_change('TxtSort');">
                        <asp:TextBox ID="TxtSort" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                    </a>
                    <a id="WF_SORT_TEXT">
                        <asp:Label ID="LblSort" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
                <!-- 受託人指定 -->
                <div class="inputItem">
                    <a id="WF_TRUSTEE_LABEL">受託人指定</a>
                    <a class="ef" id="WF_TRUSTEE" ondblclick="Field_DBclick('TxtTrustee',<%=LIST_BOX_CLASSIFICATION.LC_TRUSTEEKBN%>);" onchange="TextBox_change('TxtTrustee');">
                        <asp:TextBox ID="TxtTrustee" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                    </a>
                    <a id="WF_TRUSTEE_TEXT">
                        <asp:Label ID="LblTrustee" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
                <!-- 加減額表示指定 -->
                <div class="inputItem">
                    <a id="WF_ADDSUB_LABEL">加減額表示指定</a>
                    <a class="ef" id="WF_ADDSUB" ondblclick="Field_DBclick('TxtAddSub',<%=LIST_BOX_CLASSIFICATION.LC_ADDSUBKBN%>);" onchange="TextBox_change('TxtAddSub');">
                        <asp:TextBox ID="TxtAddSub" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                    </a>
                    <a id="WF_ADDSUB_TEXT">
                        <asp:Label ID="LblAddSub" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
            </div>

            <!-- リース料明細チェックリスト -->
            <div class="inputItem" id="WF_PRT0007_AREA">
                <a id="WF_BRANCHBASE_LABEL">処理</a>
                <a class="ef" id="WF_BRANCHBAS" ondblclick="Field_DBclick('TxtBranchBase', <%=LIST_BOX_CLASSIFICATION.LC_BRANCHBASE%>);" onchange="TextBox_change('TxtBranchBase');">
                    <asp:TextBox ID="TxtBranchBase" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                </a>
                <a id="WF_BRANCHBAS_TEXT">
                    <asp:Label ID="LblBranchBase" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>

            <!-- 支店間流動表(金額) -->
            <div id="WF_PRT0008_AREA">
                <!-- 発着ベース -->
                <div class="inputItem">
                    <a id="WF_DEPARRBASE_LABEL">
                        <asp:Label ID="HeaderDepArrBase" runat="server" Text="ベース" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    </a>
                    <a class="ef" id="WF_DEPARRBASE" ondblclick="Field_DBclick('TxtDepArrBase',<%=LIST_BOX_CLASSIFICATION.LC_DEPARRBASE%>);" onchange="TextBox_change('TxtDepArrBase');">
                        <asp:TextBox ID="TxtDepArrBase" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                    </a>
                    <a id="WF_DEPARRBASE_TEXT">
                        <asp:Label ID="LblDepArrBase" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
                <!-- 積空区分 -->
                <div class="inputItem" id="WF_STACKFREE_AREA">
                    <a id="WF_STACKFREE_LABEL">
                        <asp:Label ID="HeaderStackFree" runat="server" Text="積空区分" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    </a>
                    <a class="ef" id="WF_STACKFREE" ondblclick="Field_DBclick('TxtStackFree',<%=LIST_BOX_CLASSIFICATION.LC_STACKFREE%>);" onchange="TextBox_change('TxtStackFree');">
                        <asp:TextBox ID="TxtStackFree" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                    </a>
                    <a id="WF_STACKFREE_TEXT">
                        <asp:Label ID="LblStackFree" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
                <!-- 入れ替え -->
                <div class="inputItem" id="WF_REPLACE_AREA">
                    <a id="WF_REPLACE_LABEL">
                        <asp:Label ID="HeaderReplace" runat="server" Text="最新マスタで入れ替え" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    </a>
                    <a class="ef" id="WF_REPLACE" ondblclick="Field_DBclick('TxtReplace',<%=LIST_BOX_CLASSIFICATION.LC_REPLACE%>);" onchange="TextBox_change('TxtReplace');">
                        <asp:TextBox ID="TxtReplace" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="1"></asp:TextBox>
                    </a>
                    <a id="WF_REPLACE_TEXT">
                        <asp:Label ID="LblReplace" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
                <!-- 大分類 -->
                <div class="inputItem">
                    <a id="WF_BIGCTNCDE_LABEL">大分類</a>
                    <a class="ef" id="WF_BIGCTNCD" ondblclick="Field_DBclick('TxtBigCtnCd',<%=LIST_BOX_CLASSIFICATION.LC_CLASS%>);" onchange="TextBox_change('TxtBigCtnCd');">
                        <asp:TextBox ID="TxtBigCtnCd" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                    </a>
                    <a id="WF_BIGCTNCD_TEXT">
                        <asp:Label ID="LblBigCtnCd" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
                <!-- 出力設定 -->
                <div class="inputItem">
                    <a id="WF_REPORTESETTING_LABEL">
                        <asp:Label ID="Label1" runat="server" Text="出力設定" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                    </a>
                    <a class="ef" id="WF_REPORTSETTING" ondblclick="Field_DBclick('TxtReportSetting',<%=LIST_BOX_CLASSIFICATION.LC_REPORTSETTING%>);" onchange="TextBox_change('TxtReportSetting');">
                        <asp:TextBox ID="TxtReportSetting" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="2"></asp:TextBox>
                    </a>
                    <a id="WF_REPORTESETTING_TEXT">
                        <asp:Label ID="LblReportSetting" runat="server" CssClass="WF_TEXT"></asp:Label>
                    </a>
                </div>
            </div>
            <!-- レンタルコンテナ回送費明細 -->
            <div id="WF_PRT0012_AREA">
                <!-- 支払先 -->
                <div class="inputItem">
                    <a id="WF_PAYEECODE_LABEL">支払先</a>
                    <a class="ef" id="WF_PAYEE" ondblclick="Field_DBclick('TxtPayeeCode',<%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);" onchange="CodeName_OnChange('TxtPayeeCode','hdnSelectTori','TxtPayeeCode','LblPayeeName','TxtPayeeCode',<%=LIST_BOX_CLASSIFICATION.LC_KEKKJM%>);">
                        <asp:TextBox ID="TxtPayeeCode" runat="server" CssClass="boxIcon" onblur="MsgClear();" MaxLength="10"></asp:TextBox>
                        <asp:DropDownList ID="hdnSelectTori" runat="server" ></asp:DropDownList>
                    </a>
                    <a id="WF_PAYEENAME_TEXT">
                        <asp:TextBox ID="LblPayeeName" runat="server" CssClass="WF_TEXT" Text="" ></asp:TextBox>
                    </a>
                </div>
            </div>
            <!-- 勘定科目別・計上店別営業収入計上一覧(全勘定科目) -->
            <div id="WF_PRT0014_AREA">
                <!-- 年度指定 -->
                <div class="inputItem">
                    <a id="WF_FISCALYEAR_L">年度（期間種別に任意期間以外を選択した場合に入力してください）</a>
                    <asp:TextBox ID="WF_FISCALYEAR" runat="server" onchange="ButtonClick('WF_FISCALYEAR');" />
                </div>
                <!-- 期間種別 -->
                <div class="inputItem">
                    <a id="WF_PERIODTYPE_L">期間種別</a>
                    <asp:DropDownList ID="WF_PERIODTYPE_DDL" runat="server" onchange="ButtonClick('WF_PERIODTYPE_DDL');" />
                </div>
                <!-- 期間 -->
                <div class="inputItem">
                    <a id="WF_PERIOD_L">期間</a>
                    <span class="ef" id="WF_FROM" ondblclick="Field_DBclick('WF_PERIOD_FROM', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="WF_PERIOD_FROM" runat="server" CssClass="calendarIcon" MaxLength="10"/>
                    </span>
                    <asp:Label ID="WF_PERIOD" runat="server" Text="～" ></asp:Label>
                    <span class="ef" id="WF_TO" ondblclick="Field_DBclick('WF_PERIOD_TO', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="WF_PERIOD_TO" runat="server" CssClass="calendarIcon" MaxLength="10"/>
                    </span>
                </div>
            </div>
            
            <!-- 使用料明細表 -->
            <div id="WF_PRT0016_AREA">
                <!-- 請求書種類 -->
                <div class="inputItem" id="WF_INVOICETYPE">
                    <a id="WF_INVOICE_TYPE">明細種類</a>
                </div>
                <span class="spanLeft"></span>
                    <div class="singleInput">
                        <!-- 選択ボタン -->
                        <div class="right-harf">
                            <MSINC:tilelist ID="WF_INVTYPE" runat="server"/>
                        </div>
                    </div>
            </div>
            
        </div> <!-- End inputBox -->
    </div> <!-- End searchbox -->

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />
    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />
    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />
    
    <!-- multiSelect レイアウト -->
    <!-- 受託人単一選択 -->
    <MSINC:multiselect runat="server" id="mspTrusteeSingle" />
    <!-- 荷主単一選択 -->
    <MSINC:multiselect runat="server" id="mspShipperSingle" />
    <!-- 取引先単一選択 -->
    <MSINC:multiselect runat="server" id="mspToriSingle" />
    <!-- 駅単一選択 -->
    <MSINC:multiselect runat="server" id="mspStationSingle" />

    <!-- 非表示項目 -->
    <asp:HiddenField ID="hdnReport" runat="server" Visible="false" ClientIDMode="Predictable"  />

    <!-- イベント用 -->
    <div hidden="hidden">
        <input id="WF_FIELD" runat="server" value="" type="text" />                 <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />         <!-- Textbox DBクリックフィールド -->
        <input id="WF_LeftboxOpen" runat="server" value="" type="text" />           <!-- LeftBox 開閉 -->
        <input id="WF_RightboxOpen" runat="server" value="" type="text" />          <!-- Rightbox 開閉 -->
        <input id="WF_LeftMViewChange" runat="server" value="" type="text" />       <!-- LeftBox Mview切替 -->
        <input id="WF_ButtonClick" runat="server" value="" type="text" />           <!-- ボタン押下 -->
        <input id="WF_PrintURL" runat="server" value="" type="text" />              <!-- Textbox Print URL -->
        <!-- スクロールバー保管用 -->
        <input id="WF_ClickedScrollTop" runat="server" value="0" type="text" />   
        <input id="WF_PsyeeName" runat="server" value="0" type="text" />    
    </div>
</asp:Content>
