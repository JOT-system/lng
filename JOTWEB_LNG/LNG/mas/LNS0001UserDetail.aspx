<%@ Page Title="LNS0001D" Language="vb" AutoEventWireup="false" CodeBehind="LNS0001UserDetail.aspx.vb" Inherits="JOTWEB_LNG.LNS0001UserDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNS0001WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNS0001DH" ContentPlaceHolderID="head" runat="server">
    <link href='<%=ResolveUrl("~/LNG/css/LNS0001D.css")%>' rel="stylesheet" type="text/css" />
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNS0001D.js")%>'></script>
</asp:Content>
 
<asp:Content ID="LNS0001D" ContentPlaceHolderID="contents1" runat="server">
    <div class="d-inline-flex align-items-center flex-column w-100">
        <div class="d-flex w-100 wrap">
            <!-- サイドメニュー -->
            <MSINC:leftmenu ID="leftmenu" runat="server" />
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
                    <span class="ef">
                        <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtDelFlg', <%=LIST_BOX_CLASSIFICATION.LC_FIX_VALUE%>)" onchange="TextBox_change('TxtDelFlg');">
                            <asp:TextBox ID="TxtDelFlg" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="1"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblDelFlgName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 画面ＩＤ -->
                    <span class="ef" style="display:none;">
                        <asp:Label ID="WF_MAPID_L" runat="server" Text="画面ＩＤ" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtMapId" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        <asp:Label ID="WF_MAPID_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_3">
                    <!-- ユーザID -->
                    <span class="ef">
                        <asp:Label ID="WF_USERID_L" runat="server" Text="ユーザID" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtUserId" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_USERID_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 社員名（短） -->
                    <span class="ef">
                        <asp:Label ID="WF_STAFFNAMES_L" runat="server" Text="社員名（短）" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtStaffNameS" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_STAFFNAMES_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_4">
                    <!-- 社員名（長） -->
                    <span class="ef">
                        <asp:Label ID="WF_STAFFNAMEL_L" runat="server" Text="社員名（長）" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtStaffNameL" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="50"></asp:TextBox>
                        <asp:Label ID="WF_STAFFNAMEL_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 誤り回数 -->
                    <span class="ef">
                        <asp:Label ID="WF_MISSCNT_L" runat="server" Text="誤り回数" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtMissCNT" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="3"></asp:TextBox>
                        <asp:Label ID="WF_MISSCNT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_5">
                    <!-- パスワード -->
                    <span class="ef">
                        <asp:Label ID="WF_PASSWORD_L" runat="server" Text="パスワード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtPassword" runat="server" CssClass="WF_TEXTBOX_CSS" TextMode="Password" MaxLength="200"></asp:TextBox>
                        <asp:Label ID="WF_PASSWORD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                   </span>

                    <!-- パスワード有効期限 -->
                    <span class="ef">
                        <asp:Label ID="WF_PASSENDYMD_L" runat="server" Text="パスワード有効期限" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <asp:TextBox ID="TxtPassEndYMD" runat="server" CssClass="WF_TEXTBOX_CSS"  Enabled="false"></asp:TextBox>
                        <asp:Label ID="WF_PASSENDYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_6">
                    <!-- 開始年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_STYMD_L" runat="server" Text="開始年月日" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                            <asp:TextBox ID="TxtStYMD" runat="server" TextMode="Date" CssClass="TxtDate"></asp:TextBox>
                        <asp:Label ID="WF_STYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 終了年月日 -->
                    <span class="ef">
                        <asp:Label ID="WF_ENDYMD_L" runat="server" Text="終了年月日" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                            <asp:TextBox ID="TxtEndYMD" runat="server" TextMode="Date" CssClass="TxtDate"></asp:TextBox>
                        <asp:Label ID="WF_ENDYMD_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_7">
                    <!-- 会社コード -->
                    <span class="ef" style="display:none;">
                        <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社コード" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtCampCode" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="6" Enabled="false"></asp:TextBox>
                        <asp:Label ID="LblCampCodeName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 組織コード -->
                    <span class="ef">
                        <asp:Label ID="WF_ORG_L" runat="server" Text="組織コード" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtOrg', <%=LIST_BOX_CLASSIFICATION.LC_ORG%>);" onchange="TextBox_change('TxtOrg');">
                            <asp:TextBox ID="TxtOrg" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="6"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblOrgName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_8">
                    <!-- メールアドレス -->
                    <span class="ef colCodeOnly">
                        <asp:Label ID="WF_EMAIL_L" runat="server" Text="メールアドレス" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <asp:TextBox ID="TxtEMail" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="128"></asp:TextBox>
                    </span>
                </p>

                <p id="KEY_LINE_9">
                    <!-- メニュー表示制御ロール -->
                    <span class="ef" >
                        <asp:Label ID="WF_MENUROLE_L" runat="server" Text="メニュー表示制御ロール" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtMenuRole', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('TxtMenuRole');">
                            <asp:TextBox ID="TxtMenuRole" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblMenuRoleName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- 画面参照更新制御ロール -->
                    <span class="ef" >
                        <asp:Label ID="WF_MAPROLE_L" runat="server" Text="画面参照更新制御ロール"  CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtMapRole', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('TxtMapRole');">
                            <asp:TextBox ID="TxtMapRole" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblMapRoleName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_10">
                    <!-- 画面表示項目制御ロール -->
                    <span class="ef">
                        <asp:Label ID="WF_VIEWPROFID_L" runat="server" Text="画面表示項目制御ロール" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtViewProfId', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('TxtViewProfId');">
                            <asp:TextBox ID="TxtViewProfId" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>

                        <asp:Label ID="LblViewProfIdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

                    <!-- エクセル出力制御ロール -->
                    <span class="ef">
                        <asp:Label ID="WF_RPRTPROFID_L" runat="server" Text="エクセル出力制御ロール" CssClass="WF_TEXT_LEFT requiredMark"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtRprtProfId', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('TxtRprtProfId');">
                            <asp:TextBox ID="TxtRprtProfId" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblRprtProfIdName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>
                </p>

                <p id="KEY_LINE_11">
                    <!-- 画面初期値ロール -->
                    <span class="ef">
                        <asp:Label ID="WF_VARIANT_L" runat="server" Text="画面初期値ロール" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <asp:TextBox ID="TxtVariant" runat="server" CssClass="WF_TEXTBOX_CSS" MaxLength="20"></asp:TextBox>
                        <asp:Label ID="WF_VARIANT_TEXT" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>

<%--                    <!-- 承認権限ロール -->
                    <span class="ef">
                        <asp:Label ID="WF_APPROVALID_L" runat="server" Text="承認権限ロール" CssClass="WF_TEXT_LEFT"></asp:Label>
                        <span ondblclick="Field_DBclick('TxtApproValid', <%=LIST_BOX_CLASSIFICATION.LC_ROLE%>);" onchange="TextBox_change('TxtApproValid');">
                            <asp:TextBox ID="TxtApproValid" runat="server" CssClass="WF_TEXTBOX_CSS boxIcon" MaxLength="20"></asp:TextBox>
                        </span>
                        <asp:Label ID="LblApproValidName" runat="server" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                    </span>--%>
                </p>

            </div>
        </div>

        <!-- 期間重複調整子画面 -->
        <div id="pnlOverlapPeriodsWrapper">
            <asp:Panel ID="pnlOverlapPeriodsContents" runat="server">
                <!-- メッセージ部 -->
                <div id="pnlOverlapPeriodsMessageArea">
                    <div id="pnlOverlapPeriodsMessage_1" runat="server">
                        <asp:Label ID="pnlLabel1" runat="server" Text="指定した期間内に有効中のデータが存在しました。" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </div>
                    <div id="pnlOverlapPeriodsMessage_2" runat="server">
                        <asp:Label ID="pnlLabel2" runat="server" Text="有効期間が重複してしまう為、調整を行ってください。" CssClass="WF_TEXT_LEFT"></asp:Label>
                    </div>
                </div>
                <!-- 入力部 -->
                <div id="pnlOverlapPeriodsArea">
                    <!-- 登録済前回期間 調整前 -->
                    <div id="pnlOverlapPeriodsLabelArea_AdjustLast">
                        <span>
                            <asp:Label ID="pnlLabel3" runat="server" Text="調整前　　　　" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>
                        <!-- 登録済前回期間 調整前-開始年月日 -->
                        <span>
                            <span></span>
                            <asp:Label ID="pnlTxtAdjustLastStYMD" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>
                        <!-- 登録済前回期間 調整前-終了年月日 -->
                        <span>
                            <span></span>
                            <asp:Label ID="pnlTxtAdjustLastEndYMD" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>
                    </div>
                    <!-- 登録済前回期間 -->
                    <div id="pnlOverlapPeriodsArea_Last">
                        <span>
                            <asp:Label ID="pnlLabel6" runat="server" Text="登録済前回期間" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>
                        <!-- 登録済前回期間-開始年月日 -->
                        <span>
                            <asp:Label ID="pnlLabel7" runat="server" Text="開始年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <asp:TextBox ID="pnlTxtLastStYMD" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        </span>
                        <!-- 登録済前回期間-終了年月日 -->
                        <span>
                            <asp:Label ID="pnlLabel8" runat="server" Text="終了年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="pnlTxtLastEndYMD" runat="server" TextMode="Date" CssClass="TxtDate"></asp:TextBox>
                        </span>
                    </div>
                    
                    <!-- 今回入力期間 -->
                    <div id="pnlOverlapPeriodsArea_Input">
                        <span>
                            <asp:Label ID="pnlLabel9" runat="server" Text="今回入力期間　" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>
                        <!-- 今回入力期間-開始年月日 -->
                        <span>
                            <asp:Label ID="pnlLabel10" runat="server" Text="開始年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="pnlTxtInputStYMD" runat="server" TextMode="Date" CssClass="TxtDate"></asp:TextBox>
                        </span>
                        <!-- 今回入力期間-終了年月日 -->
                        <span>
                            <asp:Label ID="pnlLabel11" runat="server" Text="終了年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="pnlTxtInputEndYMD" runat="server" TextMode="Date" CssClass="TxtDate"></asp:TextBox>
                        </span>
                    </div>
                    
                    <!-- 登録済次回期間 調整前 -->
                    <div id="pnlOverlapPeriodsLabelArea_AdjustNext">
                        <span>
                            <asp:Label ID="pnlLabel12" runat="server" Text="調整前　　　　" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>
                        <!-- 登録済次回期間 調整前-開始年月日 -->
                        <span>
                            <span></span>
                            <asp:Label ID="pnlTxtAdjustNextStYMD" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>
                        <!-- 登録済次回期間 調整前-終了年月日 -->
                        <span>
                            <span></span>
                            <asp:Label ID="pnlTxtAdjustNextEndYMD" runat="server" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>
                    </div>
                    <!-- 登録済次回期間 -->
                    <div id="pnlOverlapPeriodsArea_Next">
                        <span>
                            <asp:Label ID="pnlLabel15" runat="server" Text="登録済次回期間" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>
                        <!-- 登録済次回期間-開始年月日 -->
                        <span>
                            <asp:Label ID="pnlLabel16" runat="server" Text="開始年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                                <asp:TextBox ID="pnlTxtNextStYMD" runat="server" TextMode="Date" CssClass="TxtDate"></asp:TextBox>
                        </span>
                        <!-- 登録済次回期間-終了年月日 -->
                        <span>
                            <asp:Label ID="pnlLabel17" runat="server" Text="終了年月日" CssClass="WF_TEXT_LEFT"></asp:Label>
                            <asp:TextBox ID="pnlTxtNextEndYMD" runat="server" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                        </span>
                    </div>
                </div>
                <!-- ボタン部 -->
                <div id="pnlOverlapPeriodsButton" runat="server">
                    <input type="button" id="btnCommonOk" class="pnlOverlapPeriodsTitle input" value="更新"  onclick="OverlapPeriodsSrcUpdateClick();" />
                    <input type="button" id="btnCommonCancel" class="pnlOverlapPeriodsTitle input" value="キャンセル"  onclick="OverlapPeriodsSrcCloseClick();" />
                </div>
            </asp:Panel>
        </div>
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
            <input id="DisabledKeyItemUserId" runat="server" value="" type="text" />
            <input id="DisabledKeyItemPass" runat="server" value="" type="text" />

            <!-- 期間重複子画面制御項目 -->
            <input id="WF_OverlapPeriodsSrc" runat="server" value="" type="text" />
            <!--   前回情報制御項目 -->
            <input id="VisibleKey_OverlapPeriodsLast" runat="server" value="" type="text" />
            <!--   次回情報制御項目 -->
            <input id="VisibleKey_OverlapPeriodsNext" runat="server" value="" type="text" />
            <!--   今回入力制御項目 -->
            <input id="DisabledKey_OverlapPeriodsInput_Start" runat="server" value="" type="text" />
            <input id="DisabledKey_OverlapPeriodsInput_End" runat="server" value="" type="text" />

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
