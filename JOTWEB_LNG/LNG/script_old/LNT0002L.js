// 画面読み込み時処理
window.onload = function () {

    // スクロール位置を復元 
    if (document.getElementById("pnlAllGridArea") !== null) {
        document.getElementById("pnlAllGridArea").scrollTop = document.getElementById("WF_ClickedScrollTop").value;
    }

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen") !== null) {
        if (document.getElementById("WF_LeftboxOpen").value === "Open") {
            document.getElementById("LF_LEFTBOX").style.display = "block";
            /* 表示位置を指定 */
            var rect = document.getElementById("LF_LEFTBOX").getBoundingClientRect();
            var objRect = document.getElementById(document.getElementById("WF_FIELD").value).getBoundingClientRect();
            /* オブジェクトの座標＋高さ＋検索BOXの高さがウインドウのビューポートの下端を超える場合は */
            /* オブジェクトの上に検索BOXを表示する */
            if ((objRect.top + objRect.height + rect.height) > window.innerHeight && (objRect.top - rect.height) > 0) {
                document.getElementById("LF_LEFTBOX").style.top = (objRect.top - rect.height) + "px";
            } else {
                /* 通常はオブジェクトの真下に表示する */
                document.getElementById("LF_LEFTBOX").style.top = (objRect.top + objRect.height) + "px";
            }
            /* オブジェクトの左端＋検索BOXの右端がウインドウのビューポートの右端を超える場合は */
            /* 超えた分だけ検索BOXのX座標を左にずらす */
            if ((objRect.left + rect.width) > window.innerWidth) {
                var correctX = window.innerWidth - (objRect.left + rect.width);
                /* 左ナビゲーションメニュー(250px) + マージンに重ならないようにする */
                if ((objRect.left + correctX) > 257) {
                    document.getElementById("LF_LEFTBOX").style.left = (objRect.left + correctX) + "px";
                } else {
                    document.getElementById("LF_LEFTBOX").style.left = "257px";
                }

            } else {
                /* 通常はオブジェクトの左端に検索BOXの左端を合わせる */
                document.getElementById("LF_LEFTBOX").style.left = objRect.left + "px";
            }
        }
    }

    // 一覧の内容を取得(右側のリスト)
    let trlDLst = document.getElementById("pnlShipmentsList_DL").getElementsByTagName("tr");
    let trlDRst = document.getElementById("pnlShipmentsList_DR").getElementsByTagName("tr");
    for (let i = 0; i < trlDLst.length; i++) {
        var chkStatus = trlDLst[i].getElementsByTagName("td")[1].innerText;

        if (chkStatus === "支店計") {
            trlDLst[i - 2].style.backgroundColor = "#0f4493";
            trlDLst[i - 2].style.color = "white";
            trlDLst[i - 1].style.backgroundColor = "#0f4493";
            trlDLst[i - 1].style.color = "white";
            trlDLst[i].style.backgroundColor = "#0f4493";
            trlDLst[i].style.color = "white";


            trlDRst[i - 2].style.backgroundColor = "#0f4493";
            trlDRst[i - 2].style.fontWeight = "bold";
            trlDRst[i - 2].style.color = "white";
            trlDRst[i - 1].style.backgroundColor = "#0f4493";
            trlDRst[i - 1].style.fontWeight = "bold";
            trlDRst[i - 1].style.color = "white";
            trlDRst[i].style.backgroundColor = "#0f4493";
            trlDRst[i].style.fontWeight = "bold";
            trlDRst[i].style.color = "white";
        }
    }
};

function saveScrollPosition() {
    let detailbox = document.getElementById("pnlAllGridArea");
    if (detailbox !== null) {
        document.getElementById("WF_ClickedScrollTop").value = detailbox.scrollTop;
    }
}

window.addEventListener("DOMContentLoaded", () => {
    // 全体スクロールイベントに、ポジション記録処理を付与する
    document.getElementById("pnlAllGridArea").addEventListener('scroll', saveScrollPosition);
});

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
    bindListCommonEvents(pnlShipmentsList, IsPostBack, true, false, false, false);
    bindListCommonEvents(pnlCalculationList, IsPostBack, true, false, false, false);
    bindListCommonEvents(pnlIncomeExpenditureList, IsPostBack, true, false, false, false);
    bindListCommonEvents(pnlStockSimulationList, IsPostBack, true, false, false, false);
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
    $("#contents1_ddlSelectChklFlags").multiselect({
        menuHeight: 390,
        noneSelectedText: "★全選択",
        selectedText: "# 個選択",
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

        /* スクロール位置を保持 */
        saveScroll();

        document.forms[0].submit();
    }
}

/**
 * ロード時処理
 */
window.addEventListener('load', function () {

    /* スクロール位置を復元 */
    loadScroll();

    // 駅未指定時、在庫シミュレーションは非表示
    if (document.getElementById('contents1_ddlSelectStation').value == 'ALL') {
        document.getElementById('pnlStockSimulation').setAttribute('style', 'display:none');
    }

});

/**
 * スクロール位置保持処理
 */
function saveScroll() {

    /* 共通部品から戻った際の規定値(173)の場合、保持しない */
    if (pnlAllGridArea.scrollTop != 173) {
        hidScroll.value = pnlAllGridArea.scrollTop;
    }

}

/**
 * スクロール位置復元処理
 */
function loadScroll() {
    pnlAllGridArea.scrollTop = hidScroll.value;
}

/**
 * 上下ボタン押下時処理
 */
function setPosition(mode) {

    var pnlPos = "";
    var elemtop = 0;

    if (mode == "UP") {
        switch (hidPosition.value) {
            case "0":
                pnlPos = "pnlSituationList";
                elemtop = 0;
                hidPosition.value = "0";
                break;
            case "1":
                pnlPos = "pnlSituationList";
                elemtop = 0;
                hidPosition.value = "0";
                break;
            case "2":
                pnlPos = "pnlIncomeExpenditure";
                elemtop = 204;
                hidPosition.value = "1";
                break;
            case "3":
                pnlPos = "pnlShippingTotal";
                elemtop = 578;
                hidPosition.value = "2";
                break;
        }
    }else {
        switch (hidPosition.value) {
            case "0":
                pnlPos = "pnlIncomeExpenditure";
                elemtop = 204;
                hidPosition.value = "1"
                break;
            case "1":
                pnlPos = "pnlShippingTotal";
                elemtop = 578;
                hidPosition.value = "2"
                break;
            case "2":
                pnlPos = "pnlStockSimulation";
                elemtop = 1034;
                hidPosition.value = "3"
                break;
            case "3":
                pnlPos = "pnlStockSimulation";
                elemtop = 1034;
                hidPosition.value = "3"
                break;
        }
    }

    //var element = document.getElementById(pnlPos);
    //var rect = element.getBoundingClientRect();
    //var elemtop = rect.top + window.pageYOffset;

    pnlAllGridArea.scrollTop = elemtop;
}

function onKeyDownEvent(e) {

    // Enter(13)が押されたら次のIndexへフォーカス移動
    //if (e.keyCode == 13) {
    //    var event = e.Event("keyup")
    //    event.keyCode = 9;
    //    e.trigger(event);
    //    return false;
    //}

}

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