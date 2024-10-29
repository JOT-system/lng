/**
 * OnLoad用処理（左右Box非表示）
 */
function InitDisplay() {
    // 全部消す
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value === "Open") {
        document.getElementById("LF_LEFTBOX").style.display = "block";
    }

    addLeftBoxExtention(leftListExtentionTarget);

    if (document.getElementById('WF_RightboxOpen').value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlDepartureArrivalDifference, IsPostBack, true, false, false, false);
}
$(document).ready(function () {
    $("#contents1_ddlSelectStation").multiselect({
        menuHeight: 390,
        noneSelectedText: "★全駅",
        selectedText: "# 駅選択",
        autoopen: false,
        multiple: true,

        position: {
            my: 'center',
            at: 'center'
        }
    });
});

/**
 * 左ナビゲーションクリックイベントバインド
 * @param {string} refreshMarkObjId リフレッシュフラグを格納するオブジェクト
 * @return {undefined} なし
 */
function refreshPane(refreshMarkObjId) {
    let refreshObj = document.getElementById(refreshMarkObjId);
    let menuVscrollObj = document.getElementById('hdnPaneAreaVScroll');
    let menuPaneArea = document.querySelector('#Menuheaderbox > .menuMain');

    if (refreshObj === null) {
        return;
    }

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        refreshObj.value = '1';
        document.forms[0].submit();
    }
}

/**
 * ロード時処理
 */
window.addEventListener('load', function () {


});

// ○左BOX用処理（TextBox変更時、名称取得）
function OrgTextBox_change(fieldNM) {

    let targetObj = document.getElementById(fieldNM);
    ConvartWideCharToNormal(targetObj);

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        //document.body.style.cursor = "wait";
        if (document.getElementById('TxtStationCode').value == '') {
            document.getElementById('TxtStationName').value = '';
        } else {
            document.getElementById("MF_SUBMIT").value = "TRUE";
            document.getElementById('WF_FIELD').value = fieldNM;
            document.getElementById('WF_ButtonClick').value = "WF_LeftBoxSelectClick";
            commonDispWait();
            document.forms[0].submit();
        }
    }
}
