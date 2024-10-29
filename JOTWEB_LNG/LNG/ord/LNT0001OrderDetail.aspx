<%@ Page Title="LNT0001D" Language="vb" AutoEventWireup="false" MasterPageFile="~/LNG/LNGMasterPage.Master" CodeBehind="LNT0001OrderDetail.aspx.vb" Inherits="JOTWEB_LNG.LNT0001OrderDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNT0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNT0001DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNT0001D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNT0001D.js")%>'></script>
    <script type="text/javascript">
        var pnlListAreaId1 = '<%=Me.pnlListArea1.ClientID%>';
        var pnlListAreaId2 = '<%=Me.pnlListArea2.ClientID%>';
        var pnlListAreaId3 = '<%=Me.pnlListArea3.ClientID%>';
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
    </script>
</asp:Content>

<asp:Content ID="LNT0001D" ContentPlaceHolderID="contents1" runat="server">
    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
    <!-- 全体レイアウト　headerbox -->
    <div class="headerbox" id="headerbox">
        <!-- ○ 固定項目 ○ -->
        <div class="actionButtonBox">
            <div class="leftSide">
            </div>
            <div class="rightSide">
                <!-- ボタン -->
                <input type="button" id="WF_ButtonDetailDownload" class="btn-sticky" value="ﾀﾞｳﾝﾛｰﾄﾞ" onclick="ButtonClick('WF_ButtonDetailDownload');" />
                <input type="button" id="WF_ButtonPAYF" class="btn-sticky" value="精算ﾌｧｲﾙ作成" onclick="ButtonClick('WF_ButtonPAYF');" />
                <input type="button" id="WF_ButtonINSERT" class="btn-sticky" value="登録" onclick="ButtonClick('WF_ButtonINSERT');" />
                <input type="button" id="WF_ButtonEND" class="btn-sticky" value="戻る" onclick="ButtonClick('WF_ButtonEND');" />
            </div>
        </div>
        <div style="display:none;" data-comment="わからないので退避">
            <!-- 会社コード -->
            <div style="display:none">
                <a>会社コード</a>
                <a class="ef" ondblclick="Field_DBclick('WF_CAMPCODE', <%=LIST_BOX_CLASSIFICATION.LC_COMPANY%>);" onchange="TextBox_change('WF_CAMPCODE');">
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
            <!-- 運用部署 -->
            <div style="display:none">
                <a>運用部署</a>

                <a class="ef" ondblclick="Field_DBclick('WF_UORG', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('WF_UORG');">
                    <asp:TextBox ID="WF_UORG" runat="server" onblur="MsgClear();"></asp:TextBox>
                </a>
                <a>
                    <asp:Label ID="WF_UORG_TEXT" runat="server" CssClass="WF_TEXT"></asp:Label>
                </a>
            </div>
        </div>
        <!-- ○ 変動項目 ○ -->
        <div id="headerDispArea"> <!-- このdivで括られた領域を表示非表示する -->
            <asp:Panel ID="pnlHeaderInput" CssClass="headerInput" runat="server">
                <!-- ■　１行目　■ -->
                <!-- ■　発送日　■ -->
                <span class="left">
                    <a id="WF_PLANDEPYMD_LABEL" class="requiredMark">発送日</a>
                    <a class="ef" id="WF_PLANDEPYMD" ondblclick="Field_DBclick('TxtPlanDepYMD', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                        <asp:TextBox ID="TxtPlanDepYMD" runat="server" ReadOnly="true" CssClass="calendarIcon iconOnly" onblur="MsgClear();"></asp:TextBox>
                    </a>
                </span>
                <!-- コンテナ記号 -->
                <span class="left">              
                    <a id="WF_CTNTYPE_LABEL" class="requiredMark">コンテナ記号</a>
                    <a class="ef" id="WF_CTNTYPE" ondblclick="Field_DBclick('TxtCtnType', <%=LIST_BOX_CLASSIFICATION.LC_CTNTYPE%>);" onchange="TextBox_change('TxtCtnType');">
                        <asp:TextBox ID="TxtCtnType" runat="server" ReadOnly="true" onblur="MsgClear();" CssClass="boxIcon iconOnly" MaxLength="8"></asp:TextBox>
                        <asp:TextBox ID="TxtCtnTypeCode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
           　　 <!-- コンテナ番号 -->
                <span class="left">
                    <a id="WF_CTNNO_LABEL" class="requiredMark">コンテナ番号</a>
                    <a class="ef" id="WF_CTNNO" ondblclick="Field_DBclick('TxtCtnNo', <%=LIST_BOX_CLASSIFICATION.LC_CTNNO%>);" onchange="TextBox_change('TxtCtnNo');">
                        <asp:TextBox ID="TxtCtnNo" runat="server" onblur="MsgClear();" ReadOnly="true" CssClass="boxIcon iconOnly" MaxLength="5"></asp:TextBox>
                        <asp:TextBox ID="TxtCtnNoCode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <span></span>
                <!-- ■　２行目　■ -->
                <!-- ■　オーダー№　■ -->
                <span class="left">
                    <a id="WF_ORDERNO_LABEL" class="ef">オーダー№</a>
                    <a class="ef" id="WF_ORDERNO">
                        <asp:TextBox ID="TxtOrderNo" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　ステータス　■ -->
                <span class="left">
                    <a id="WF_ORDERSTATUS_LABEL">ステータス</a>
                    <a class="ef" id="ORDERSTATUS">
                        <asp:TextBox ID="TxtOrderStatus" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                    </a>
                </span>
                <span></span> 
                <span></span> 
                <span></span> 
                <!-- ■　３行目　■ -->
                <!-- ■　大分類　■ -->
                <span class="left">
                    <a id="WF_BIGCTN_LABEL" class="ef">大分類</a>
                    <a id="WF_BIGCTN">
                        <asp:TextBox ID="TxtBigCtnName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtBigCtnCode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　中分類　■ -->
                <span class="left">
                    <a id="WF_MIDDLECTN_LABEL" class="ef">中分類</a>
                    <a id="WF_MIDDLECTN">
                        <asp:TextBox ID="TxtMiddleCtnName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtMiddleCtnCode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　小分類　■ -->
                <span class="left">
                    <a id="WF_SMALLCTN_LABEL" class="ef">小分類</a>
                    <a id="WF_SMALLCTN">
                        <asp:TextBox ID="TxtSmallCtnName" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtSmallCtnCode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <span></span>
                <!-- ■　４行目　■ -->
                <!-- ■　125キロ賃率　■ -->
                <span class="left">
                    <a id="WF_RENTRATE125_LABEL" class="ef">125キロ賃率</a>
                    <a id="WF_RENTRATE125">
                        <asp:TextBox ID="TxtRentRate125" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtRentRate125Code" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　現行／次期　■ -->
                <span class="left">
                    <a id="WF_TUNNEXT125_LABEL" class="ef">現行／次期</a>
                    <a id="WF_TUNNEXT125">
                        <asp:TextBox ID="TxtTunNext125" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtTunNext125Code" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <span></span>
                <span></span>
                <span></span>
                <!-- ■　５行目　■ -->
                <!-- ■　端数金額基準　■ -->
                <span class="left">
                    <a id="WF_ROUNDFEE_LABEL" class="ef">端数金額基準</a>
                    <a id="WF_ROUNDFEE">
                        <asp:TextBox ID="TxtRoundFee" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtRoundFeeCode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　端数区分金額以上　■ -->
                <span class="left">
                    <a id="WF_ROUNDKBNGE_LABEL" class="ef">端数区分金額以上</a>
                    <a id="WF_ROUNDKBNGE">
                        <asp:TextBox ID="TxtRoundKbnGE" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtRoundKbnGECode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <!-- ■　端数区分金額未満　■ -->
                <span class="left">
                    <a id="WF_ROUNDKBNLT_LABEL" class="ef">端数区分金額未満</a>
                    <a id="WF_ROUNDKBNLT">
                        <asp:TextBox ID="TxtRoundKbnLT" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtRoundKbnLTCode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
                <span class="left">
                    <a id="WF_ROUNDTUNNEXT_LABEL" class="ef">現行／次期</a>
                    <a id="WF_ROUNDTUNNEXT">
                        <asp:TextBox ID="TxtRoundTunNext" runat="server" onblur="MsgClear();" Enabled="false"></asp:TextBox>
                        <asp:TextBox ID="TxtRoundTunNextCode" runat="server" onblur="MsgClear();" Visible="false"></asp:TextBox>
                    </a>
                </span>
            </asp:Panel>
        </div>
    </div>

    <!-- 全体レイアウト　detailbox -->
    <div  class="detailbox" id="detailbox">
        <!-- タブボックス -->
        <div id="tabBox">
            <div class="leftSide">
                <!-- ■　Dタブ　■ -->
                <asp:Label ID="WF_Dtab01" runat="server" Text="明細データ" data-itemelm="tab" onclick="DtabChange('0')" ></asp:Label>
                <asp:Label ID="WF_Dtab02" runat="server" Text="使用料金" data-itemelm="tab" onclick="DtabChange('1')" ></asp:Label>
                <asp:Label ID="WF_Dtab03" runat="server" Text="使用料金判定" data-itemelm="tab" onclick="DtabChange('2')"></asp:Label>
            </div>
            <div class="rightSide">
                <span id="hideHeader">
                </span>
            </div>
        </div>

        <asp:MultiView ID="WF_DetailMView" runat="server">
            <!-- ■ Tab No1　明細データ　■ -->
            <asp:View ID="WF_DView1" runat="server" >
                <!-- ボタン -->
                <div class="actionButtonBox">
                    <div class="leftSide">
                        <input type="button" id="WF_ButtonALLSELECT_TAB1" class="btn-sticky" value="全選択" onclick="ButtonClick('WF_ButtonALLSELECT_TAB1');" />
                        <input type="button" id="WF_ButtonSELECT_LIFTED_TAB1" class="btn-sticky" value="選択解除"  onclick="ButtonClick('WF_ButtonSELECT_LIFTED_TAB1');" />
                        <input type="button" id="WF_ButtonLINE_LIFTED_TAB1" class="btn-sticky" value="行削除"  onclick="ButtonClick('WF_ButtonLINE_LIFTED_TAB1');" />
                        <input type="button" id="WF_ButtonLINE_ADD_TAB1" class="btn-sticky" value="行追加"  onclick="ButtonClick('WF_ButtonLINE_ADD_TAB1');" />
                    </div>
                    <div class="rightSide">
                        <input type="button" id="WF_ButtonFEECALC_TAB1" class="btn-sticky" value="料金計算"  onclick="ButtonClick('WF_ButtonFEECALC_TAB1');" />
                    </div>
                </div>
                <!-- 一覧レイアウト -->
                <asp:panel id="pnlListArea1" runat="server" ></asp:panel>
            </asp:View>

            <!-- ■ Tab No2　精算予定ファイル　■ -->
            <asp:View ID="WF_DView2" runat="server">
                <div style="display:none">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>                   
                    <div class="rightSide">
                    </div>                    
                </div>
                </div>
                <div class="actionButtonBox"></div>
            <div class="inputItem2">
                <a id="WF_AUTO_FLG">
                    <asp:CheckBox ID="ChkAutoFlg" runat="server" Text="自動計算" />
                </a>
            </div>
                <!-- 一覧レイアウト -->
                <asp:panel id="pnlListArea2" runat="server" ></asp:panel>
            </asp:View>

            <!-- ■ Tab No3　使用料金判定　■ -->
            <asp:View ID="WF_DView3" runat="server">
                <!-- ボタン -->
                <div style="display:none">
                <div class="actionButtonBox">
                    <div class="leftSide">
                    </div>
                    <div class="rightSide">
                    </div>
                </div>
                </div>
                <div class="actionButtonBox"></div>
                <!-- 一覧レイアウト -->
                <asp:panel id="pnlListArea3" runat="server" ></asp:panel>
            </asp:View>
        </asp:MultiView>
        <!-- <div class="detailBottom"></div> -->
    </div>

    <!-- rightbox レイアウト -->
    <MSINC:rightview id="rightview" runat="server" />

    <!-- leftbox レイアウト -->
    <MSINC:leftview id="leftview" runat="server" />

    <!-- Work レイアウト -->
    <MSINC:wrklist id="work" runat="server" />

    <!-- イベント用 -->
    <div hidden="hidden">
        <!-- GridView DBクリック-->
        <asp:TextBox ID="WF_GridDBclick" Text="" runat="server"></asp:TextBox>
        <!-- GridView表示位置フィールド -->
        <asp:TextBox ID="WF_GridPosition" Text="" runat="server"></asp:TextBox>

        <!-- Textbox DBクリックフィールド -->
        <input id="WF_FIELD" runat="server" value="" type="text" />
        <!-- Textbox(Repeater) DBクリックフィールド -->
        <input id="WF_FIELD_REP" runat="server" value="" type="text" />
        <!-- Textbox DBクリックフィールド -->
        <input id="WF_SelectedIndex" runat="server" value="" type="text" />

        <!-- 画面表示切替 -->
        <input id="WF_DISP" runat="server" value="" type="text" />
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
        <!-- 新規・更新切替用フラグ -->
        <input id="WF_CREATEFLG" runat="server" value="" type="text" />
        <!-- 貨車連結切替用フラグ -->
        <input id="WF_CREATELINKFLG" runat="server" value="" type="text" />
        <!-- OT発送日報フラグ -->
        <input id="WF_OTLINKAGEFLG" runat="server" value="" type="text" />
        <!-- 向け先(一部)訂正フラグ -->
        <input id="WF_CORRECTIONTANKFLG" runat="server" value="" type="text" />
        <!-- 実績日訂正フラグ -->
        <input id="WF_CORRECTIONDATEFLG" runat="server" value="" type="text" />
        <!-- 一括フラグ -->
        <input id="WF_BULKFLG" runat="server" value="" type="text" />
        <!-- 手配連絡フラグ -->
        <input id="WF_CONTACTFLG" runat="server" value="" type="text" />
        <!-- 結果受理フラグ -->
        <input id="WF_RESULTFLG" runat="server" value="" type="text" />
        <!-- 託送指示フラグ -->
        <input id="WF_DELIVERYFLG" runat="server" value="" type="text" />
        <%-- 20210412 START 根岸営業所対応(竜王81列車) --%>
        <!-- 先返しフラグ -->
        <input id="WF_FIRSTRETURNFLG" runat="server" value="" type="text" />
        <%-- 20210412 END   根岸営業所対応(竜王81列車) --%>
        <!-- 使用受注オーダー可否フラグ -->
        <input id="WF_USEORDERFLG" runat="server" value="" type="text" />
        <!-- 画面ボタン制御 -->
        <input id="WF_MAPButtonControl" runat="server" value="0" type="text" />
        <!-- DetailBox Mview切替 -->
        <input id="WF_DTAB_CHANGE_NO" runat="server" value="" type="text"/>
        <!-- ヘッダーを表示するか保持、"1"(表示:初期値),"0"(非表示)  -->
        <asp:HiddenField ID="hdnDispHeaderItems" runat="server" Value="1" />
        <!-- 油種数登録ボタン押下フラグ(True:有効, False：無効) -->
        <input id="WF_ButtonInsertFLG" runat="server" value="" type="text" />
        <!-- 選択(チェックボックス)押下フラグ(True:有効, False：無効) -->
        <input id="WF_CheckBoxFLG" runat="server" value="" type="text" />
    </div>
</asp:Content>
