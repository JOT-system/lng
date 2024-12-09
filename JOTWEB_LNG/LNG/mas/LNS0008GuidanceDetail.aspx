<%@ Page Title="LNS0008D" Language="vb" AutoEventWireup="false" CodeBehind="LNS0008GuidanceDetail.aspx.vb" Inherits="JOTWEB_LNG.LNS0008GuidanceDetail" %>
<%@ MasterType VirtualPath="~/LNG/LNGMasterPage.Master" %>

<%@ Import Namespace="JOTWEB_LNG.GRIS0005LeftBox" %>

<%@ Register Src="~/inc/GRIS0004RightBox.ascx" TagName="rightview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0005LeftBox.ascx" TagName="leftview" TagPrefix="MSINC" %>
<%@ Register Src="~/inc/GRIS0006LeftMenu.ascx" TagName="leftmenu" TagPrefix="MSINC" %>
<%@ Register Src="~/LNG/inc/LNS0008WRKINC.ascx" TagName="wrklist" TagPrefix="MSINC" %>

<asp:Content ID="LNS0008DH" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css"/>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
    <link href='<%=ResolveUrl("~/LNG/css/LNS0008D.css")%>' rel="stylesheet" type="text/css" />
    <script type="text/javascript" src='<%=ResolveUrl("~/LNG/script/LNS0008D.js")%>'></script>
    <script type="text/javascript">
        var IsPostBack = '<%=If(IsPostBack = True, "1", "0")%>';
        // 添付許可拡張子
        var acceptExtentions = ["xlsx", "docx", "pptx", "jpg", "png", "bmp", "zip", "gif", "csv", "txt", "pdf", "lzh"];
        var acceptExtentionsStr = "許可ファイル種類(" + acceptExtentions.join(',') + ")";
        // ガイドライン用のUploadハンドラー
        var handlerUrl = '<%=ResolveUrl("~/LNG/inc/LNS0008FILEUPLOAD.ashx")%>';
    </script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/ja.js"></script>
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            // #contentsInnerの高さ取得
            let windowHeight = window.innerHeight;
            const headerHeight = 47;
            const breadcrumbHeight = 18;
            const spaceHeight = 8 + 16 + 16;
            let contentsInnerHeight = windowHeight - headerHeight - breadcrumbHeight - spaceHeight;
            document.getElementById("contentsInner").style.height = contentsInnerHeight + "px";

            // #fixed-tableの高さ取得
            const contentsTitleHeight = 44;
            let actionTriggerHeight = document.getElementById("actionTrigger").offsetHeight;
            const contentsSpaceHeight = 16 * 4;
            let fixedTableHeight = (contentsInnerHeight - contentsTitleHeight -actionTriggerHeight - contentsSpaceHeight) + "px";
            document.getElementById("fixed-table").style.height = fixedTableHeight;

            // カレンダー表示
            flatpickr('#datetimepicker1', {
                wrap: true,
                dateFormat: 'Y/m/d',
                locale : 'ja',
                clickOpens: false,
                allowInput: true,
                monthSelectorType: 'static',
                defaultDate: new Date()
            });
        });
    </script>
</asp:Content>
 
<asp:Content ID="LNS0008D" ContentPlaceHolderID="contents1" runat="server">
        <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
        <!-- 全体レイアウト　detailbox -->
    <div class="d-inline-flex align-items-center flex-column w-100">
        <div class="d-flex w-100 wrap">
            <!-- サイドメニュー -->
            <MSINC:leftmenu ID="leftmenu" runat="server" />
            <!-- メイン画面（一覧） -->
            <div class="w-100 contents">
                <nav style="--bs-breadcrumb-divider: '>';" aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item d-flex align-items-center gap-1"><span class="material-symbols-outlined">home</span><a href="#">TOP</a></li>
                        <li class="breadcrumb-item active">ガイダンス検索</li>
                        <li class="breadcrumb-item active">ガイダンス一覧</li>
                        <li class="breadcrumb-item active" aria-current="page">ガイダン登録</li>
                    </ol>
                </nav>
                <h2 class="w-100 fs-5 fw-bold contents-title">ガイダンス一覧</h2>
                <div class="detailboxOnly" id="detailbox" >
                    <div id="detailbuttonbox" class="detailbuttonbox">
                        <div class="actionButtonBox">
                            <div class="rightSide">
                                <input type="button" id="WF_ButtonUPDATE" runat="server" class="btn-sticky" value="更新" onclick="ButtonClick('WF_ButtonUPDATE');" />
                                <input type="button" id="WF_ButtonCLEAR" runat="server" class="btn-sticky" value="戻る"  onclick="ButtonClick('WF_ButtonCLEAR');" />
                                <input type="button" id="WF_ButtonBackToMenu" runat="server" class="btn-sticky" value="メニューへ" onclick="ButtonClick('WF_ButtonBackToMenu');" />
                            </div>
                        </div>
                    </div>
                    <table class="input">
                        <colgroup>
                            <col /><col /><col /><col />
                        </colgroup>
                        <tbody>
                            <tr>
                                <th>ガイダンス登録日</th>
                                <td>
                                    <asp:Label ID="LblGuidanceEntryDate" runat="server"></asp:Label>
                                </td>
                                <th><span class='requiredMark'>種類</span></th>
                                <td>
                                    <div class="grc0001Wrapper type">
                                        <asp:RadioButtonList ID="RblType" runat="server"  ClientIDMode="Predictable" RepeatLayout="UnorderedList"></asp:RadioButtonList>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <th><span class='requiredMark'>掲載開始日</span></th>
                                <td>
                                    <a ondblclick="Field_DBclick('TxtFromYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                                        <asp:TextBox ID="TxtFromYmd" CssClass="calendarIcon" runat="server"></asp:TextBox>
                                    </a>
                                </td>
                                <th><span class='requiredMark'>掲載終了日</span></th>
                                <td>
                                    <a ondblclick="Field_DBclick('TxtEndYmd', <%=LIST_BOX_CLASSIFICATION.LC_CALENDAR%>);">
                                        <asp:TextBox ID="TxtEndYmd" CssClass="calendarIcon" runat="server"></asp:TextBox>
                                    </a>
                                </td>
                            </tr>
                            <tr>
                                <th><span class='requiredMark'>タイトル</span></th>
                                <td colspan="3"><asp:TextBox ID="TxtTitle" runat="server"></asp:TextBox></td>
                            </tr>
                            <tr>
                                <th class="top">対象</th>
                                <td colspan="3">
                                    <div class="grc0001Wrapper flags">
                                        <asp:CheckBoxList ID="ChklFlags" runat="server"  ClientIDMode="Predictable" RepeatLayout="UnorderedList"></asp:CheckBoxList>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <th class="top">内容</th>
                                <td colspan="3">
                                    <asp:TextBox ID="TxtNaiyou" runat="server" TextMode="MultiLine"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <th class="top">添付</th>
                                <td class="attachmentCell" colspan="3">
                                    <div id="divAttachmentArea" class="fileDrag">
                                        <div id="uploadLine" class="uploadLine">
                                            <asp:FileUpload ID="fupAttachment" runat="server"/>
                                            <input type="button" id="btnFileUpload" class="btn-sticky" value="ファイル追加" />
                                            <span id="uploadLineText">ボタンクリック、またはここにファイルをドラッグ＆ドロップ</span>
                                            <hr/>
                                        </div>
                                        <asp:Repeater ID="RepAttachments" runat="server" ClientIDMode="Predictable">
                                            <ItemTemplate>
                                                <div><span class="delAttachment" title="削除" onclick='setDeleteFileName("<%# Eval("FileName") %>");ButtonClick("WF_ButtonDELETE");'>×</span>
                                                    <span><a href='<%# ResolveUrl("~/LNG/mas/LNS0008GuidanceDownload.aspx") & "?id=" & JOTWEB_LNG.LNS0008WRKINC.GetParamString("", Eval("FileName"), "0") %>' target="_blank"><%# Eval("FileName") %></a></span>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
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

            <!-- ファイル名一覧 -->
            <input id="WF_FILENAMELIST" runat="server" value="" type="text" />
            <!-- 削除ファイル名 -->
            <input id="WF_DELETEFILENAME" runat="server" value="" type="text" />
        </div>
 
</asp:Content>
