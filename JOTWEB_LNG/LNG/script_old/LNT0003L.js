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
    bindListCommonEvents(pnlDepStationList, IsPostBack, true, false, false, false);
    bindListCommonEvents(pnlMemoHistory, IsPostBack, true, false, false, false);

    commonBindNormalEnterToNextTabStep();
}
$(document).ready(function () {
    $("#contents1_ddlSelectStation").multiselect({
        menuHeight: 390,
        noneSelectedText: "★全駅",
        selectedText: "# 駅選択",
        autoopen: false,
        multiple: true,
        buttonWidth:180,

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
        buttonWidth:180,

        position: {
            my: 'center',
            at: 'center'
        }
    });
    //$("#contents1_ddlZyogaiKbnList").multiselect({
    //    menuHeight: 390,
    //    noneSelectedText: "",
    //    selectedText: "",
    //    autoopen: false,
    //    multiple: false,

    //    position: {
    //        my: 'center',
    //        at: 'center'
    //    }
        
    //});
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
        commonDispWait();
        document.forms[0].submit();
    }
}

/**
 * ロード時処理
 */
window.addEventListener('load', function () {

    /* アコーディオン開閉によるグリッド領域のサイズ変更 */
    gridSizeChange();

    /* リストボックスの表示値を更新 */
    changeListBoxValue();

    let ele = document.getElementById('pnlMemoHistoryWrapper');
    ele.style.visibility = document.getElementById('WF_Memo').value;

});

/**
 * グリッドサイズ変更処理
 */
function gridSizeChange() {

    /* アコーディオン開閉によるグリッド領域のサイズ変更 */
    /* グリッド領域以外の高さ 444px(#titlebox:46px, .Operation:45px, #pnlTitle:40px, .selectFrame:177px, .pnlTotalArea:120px, 余白:24px  )*/
    if (document.getElementById("chkAcdTotal").checked) {
        document.getElementById('pnlGridArea').setAttribute('style', 'height: calc(100vh - 452px)');
        document.getElementById('divDepStationListGrid').setAttribute('style', 'height: calc(100vh - 452px - 48px)');
        document.getElementById('pnlDepStationList').setAttribute('style', 'height: 485px');
    } else {
        document.getElementById('pnlGridArea').setAttribute('style', 'height: calc(100vh - 374px)');
        document.getElementById('divDepStationListGrid').setAttribute('style', 'height: calc(100vh - 374px - 48px)');
        document.getElementById('pnlDepStationList').setAttribute('style', 'height: 550px');
    }

}

/**
 * リストボックスの表示値を更新
 */
function changeListBoxValue() {

    let toItem = 'lbCONTSTATUSCONTSTATUS';
    let fromItem = 'txtpnlDepStationListCONTSTATUSAFT';
    let drCount = document.getElementById('pnlDepStationList_DR').firstElementChild.rows.length;

    for (let i = 1; i <= drCount; i++) {
        // オブジェクトの存在チェック(存在しない場合はスキップ)
        let objTarget = document.getElementById(fromItem + i)
        if (objTarget === null || objTarget === undefined) {
            return;
        }
        document.getElementById(toItem + i).value = document.getElementById(fromItem + i).value;
    }

}

/**
 * 表示順クリアボタンイベント
 * @param {string} sortClearObjId ソートクリア対象のオブジェクト
 */
function sortClear(sortClearObjId) {
    let sortClearObj = document.getElementById(sortClearObjId);
    var formId = document.forms[0].id;

    if (sortClearObj === null) {
        return;
    }

    /* ソート項目をクリア */
    document.getElementById('hdnListSortValue' + formId + "pnlDepStationList").value = "";

    document.getElementById("MF_SUBMIT").value = "TRUE";
    document.getElementById("WF_LIST_SORTING").value = "";
    sortClearObj.value = '1';
    commonDispWait();
    document.forms[0].submit();
}

/**
 *  『履歴を見る』ボタンクリックイベント
 * @param {object} obj TR(行)オブジェクト
 * @param {string} lineCnt 行No
 * @param {string} colName カラム名
 * @return {undefined} なし
 * @description グリッド内ボタン押下イベント
 */
function historyClick(obj, lineCnt, colName) {

    if (document.getElementById("WF_CTNFLG").value == "1"){
        return;
    } else {
        document.getElementById("WF_SelectedIndex").value = lineCnt;
        ButtonClick('WF_ButtonHISTORY');
    }
    document.getElementById("WF_CTNFLG").value = ""
}

/**
 *  履歴一覧『閉じる』ボタンクリックイベント
 */
function historyCloseClick() {

    ButtonClick('WF_ButtonHistoryCLOSE');
}

/**
 *  『状態』リスト選択イベント
 * @param {object} obj TR(行)オブジェクト
 * @param {string} lineCnt 行No
 * @param {string} colName カラム名
 * @return {undefined} なし
 * @description グリッド内リストボックス選択イベント
 */
function statusSelect(obj, lineCnt, colName) {

    var objDataGrid = document.getElementById("pnlDepStationList_DR");

    if (objDataGrid === null) {
        return;
    }

    var objTable = objDataGrid.children[0];

    // 状態（リストボックス）を取得
    var selectObjs = objTable.querySelectorAll("select[id^='lbCONTSTATUSCONTSTATUS");
    // 状態（値（非表示））を取得
    var updateObjs = objTable.querySelectorAll("input[id^='txtpnlDepStationListCONTSTATUSAFT");

    // 選択値を非表示項目へ退避する
    if (selectObjs[lineCnt-1] !== null) {
        updateObjs[lineCnt-1].value = selectObjs[lineCnt-1].value;
    }
    /* テキストボックスの変更イベントを発火 */
    var evt = document.createEvent("HTMLEvents");
    evt.initEvent("change", false, true);
    updateObjs[lineCnt - 1].parentNode.dispatchEvent(evt);

}

/**
 * コンテナ種別・経理資産区分選択処理（再描画）
 */
function selectCheckBox() {

    //document.getElementById("MF_SUBMIT").value = "TRUE";
    //document.forms[0].submit();

}

// ○一括ダウンロード処理
function f_AccountingDownload() {

    var objPrintUrl = document.getElementById("WF_PrintURL1").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL2").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
}

/**
* ダウンロード処理
* @param {string} url ダウンロードURL
* @return {undefined} なし
* @description 
*/
function commonDownload(url) {
    if ('1' === url) {
        //IEの場合
        window.open(url + '?rnd=' + new Date().getTime(), "view", "_blank");
    } else {
        // IE以外の場合
        var fileName = url.substring(url.lastIndexOf('/') + 1);
        let nondecodeFileName = fileName;
        fileName = decodeURIComponent(fileName);
        // リンク（<a>要素）を生成し、JavaScriptからクリックする
        var link = document.createElement("a");
        // キャッシュされたファイルをダウンロード扱いしないためURLパラメータをダミーで付与
        if (url.indexOf('?') === -1) {
            link.href = url + '?rnd=' + new Date().getTime();
        } else {
            link.href = url;
        }         link.id = 'commondownloaddummylink';
        //link.download = fileName;
        link.setAttribute('download', nondecodeFileName);
        link.target = '_blank';
        link.innerText = 'dl';
        link.style.display = 'none';
        link.type = 'application/octet-stream';
        link.rel = 'noopener noreferrer';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
}

// ○左BOX用処理（TextBox変更時、名称取得）
function OrgTextBox_change(fieldNM) {

    let targetObj = document.getElementById(fieldNM);
    ConvartWideCharToNormal(targetObj);

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        //document.body.style.cursor = "wait";
        if (document.getElementById('TxtStationCode').value == ''){
            document.getElementById("LblStationName").textContent = "";
        } else {
            document.getElementById("MF_SUBMIT").value = "TRUE";
            document.getElementById('WF_FIELD').value = fieldNM;
            document.getElementById('WF_ButtonClick').value = "WF_LeftBoxSelectClick";
            commonDispWait();
            document.forms[0].submit();
        }
    }
}

// 全角→半角(数字用)
function CtnNumTextBox_change(fieldNM) {
    let targetObj = document.getElementById(fieldNM);
    ConvartWideCharToNormal(targetObj);
}

// 全角→半角(英数字)
function CtnEisuTextBox_change(fieldNM) {
    let targetObj = document.getElementById(fieldNM);
    targetObj.value = replaceFullToHalf(targetObj.value);
    // 大文字→小文字変換
    var str = targetObj.value ;
    str = str.toUpperCase();
    targetObj.value = str
}

// 
function OrgOnblur(){
    document.getElementById("WF_CTNFLG").value = "1";
}
