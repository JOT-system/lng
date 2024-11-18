// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {
    //document.getElementById("rightb").style.visibility = "hidden";
    //左ナビゲーションのクリックイベントバインド
    bindLeftNaviClick('divLeftNav1', '1');
    bindLeftNaviClick('divLeftNav2', '2');
    bindLeftNaviClick('divLeftNav3', '3');
    bindLeftNaviClick('divLeftNav4', '4');
    bindLeftNaviClick('divLeftNav5', '5');
    bindLeftNaviClick('divLeftNav6', '6');
    bindLeftNaviClick('divLeftNav7', '7');
    bindLeftNaviClick('divLeftNav8', '8');
    bindLeftNaviClick('divLeftNav9', '9');
    bindLeftNaviClick('divLeftNav10', '10');
    bindLeftNaviClick('divLeftNav11', '11');
    bindLeftNaviClick('divLeftNav12', '12');
    bindLeftNaviClick('divLeftNav13', '13');
    bindLeftNaviClick('divLeftNav14', '14');
    bindLeftNaviClick('divLeftNav15', '15');

    //ガイダンス開閉のイベントバインド
    //    let guidanceButton = document.getElementById('guidanceOpenCloseButton');
    //    if (guidanceButton !== null) {
    //        bindShowCloseGuidance(guidanceButton);
    //    }
    // ポストバック時のスクロール位置復元
    let menuVscrollObj = document.getElementById('hdnPaneAreaVScroll');
    let menuPaneArea = document.querySelector('#Menuheaderbox > .menuMain');
    if (menuVscrollObj !== null) {
        if (menuPaneArea !== null) {
            if (menuVscrollObj.value !== '') {
                menuPaneArea.scrollTop = menuVscrollObj.value;
                menuVscrollObj.value = '';
            }

        }
    }
    //〆状況ペインの幅調整
    let closeBranchAll = document.querySelectorAll('.cycleBillingStatusDeptBranch > div');
    let closeBottom = document.querySelector('.cycleBillingStatusBottom');
    if (closeBranchAll !== null) {
        if (closeBottom !== null) {
            let branchSize = 0;
            for (let i = 0; i < closeBranchAll.length; i++) {
                let closeBranch = closeBranchAll[i];
                branchSize = branchSize + closeBranch.clientWidth;
            }
            closeBottom.style.width = branchSize + "px";
        }
    }

    //// 左ボックス
    //if (document.getElementById("WF_LeftboxOpen").value === "Open") {
    //    document.getElementById("LF_LEFTBOX").style.display = "block";
    //}

    //// 左ボックス拡張機能追加
    //addLeftBoxExtention(leftListExtentionTarget);

}
/**
 * 左ナビゲーションクリックイベントバインド
 * @return {undefined} なし
 */
function bindLeftNaviClick(strElementID, strID) {
    /* 左ナビ全体のDivを取得 */
    let leftNavObj = document.getElementById(strElementID);
    /* 左ナビ未描画なら終了 */
    if (leftNavObj === null) {
        return;
    }
    /* ラベルタグ（左ナビボタン風デザイン）のオブジェクトを取得 */
    let labelObjList = leftNavObj.querySelectorAll("div[data-hasnext='1'] > label");
    /* 左ナビボタンが描画されてなければそのまま終了 */
    if (labelObjList === null) {
        return;
    }
    if (labelObjList.length === 0) {
        return;
    }
    /* 左ナビボタンのループ */
    for (let i = 0; i < labelObjList.length; i++) {
        let targetLabel = labelObjList[i];
        let parentDiv = targetLabel.parentNode;
        let posicol = parentDiv.dataset.posicol;
        let rowline = parentDiv.dataset.rowline;
        // ダイアログを閉じるタイミングでフォーカスを合わせる
        targetLabel.addEventListener('click', (function (posicol, rowline) {
            return function () {
                let hdnPosiColObj = document.getElementById('hdnPosiCol');
                hdnPosiColObj.value = posicol;
                let hdnRowLineObj = document.getElementById('hdnRowLine');
                hdnRowLineObj.value = rowline;
                commonDispWait();
                ButtonClick('WF_ButtonLeftNavi' + strID); /* 共通サブミット処理、VB側ロード時のSelectケースで割り振らせる */
            };
        })(posicol, rowline), false);

    }
}
/**
 * 左ナビゲーションクリックイベントバインド
 * @param {Element} objButton 対象のボタンオブジェクト
 * @return {undefined} なし
 */
function bindShowCloseGuidance(objButton) {
    let menuBox = document.getElementById('Menuheaderbox');
    let guidanceAreaObj = document.getElementById('guidanceList');
    let guidanceWrapObj = document.getElementById('guidanceArea');
    let flag = getDispGuigance();
    if (flag === '1') {
        menuBox.classList.add('showGuidance');
    } else {
        guidanceAreaObj.style.display = 'none';
        guidanceWrapObj.style.height = '30px';
        objButton.classList.add('closeBtn');
    }
    objButton.addEventListener('click', (function (objButton, menuBox, guidanceAreaObj, guidanceWrapObj) {
        return function () {
            if (menuBox.classList.contains('showGuidance')) {
                menuBox.classList.remove('showGuidance');
                objButton.classList.add('closeBtn');
                guidanceAreaObj.style.display = 'none';
                guidanceWrapObj.style.height = '30px';
                setDispGuidance('0');
            } else {
                menuBox.classList.add('showGuidance');
                objButton.classList.remove('closeBtn');
                guidanceAreaObj.style.display = '';
                guidanceWrapObj.style.height = '';
                setDispGuidance('1');
            }
        };
    })(objButton, menuBox, guidanceAreaObj, guidanceWrapObj), true);
}

/**
 * 左ナビゲーションクリックイベントバインド
 * @param {Element} objButton 対象のボタンオブジェクト
 * @return {undefined} なし
 */
function bindShowCloseGuidanceBox(objButton) {
    let menuBox = document.getElementById('Menuheaderbox');
    let guidanceBoxObj = document.getElementById('guidanceBox');
    let guidanceBoxWrapObj = document.getElementById('guidanceBoxWrapper');
    let flag = getDispNews();
    if (flag === '1') {
        menuBox.classList.add('showGuidanceBox');
    } else {
        guidanceBoxObj.style.display = 'none';
        guidanceBoxWrapObj.style.height = '30px';
        objButton.classList.add('closeBtn');
    }

    objButton.addEventListener('click', (function (objButton, menuBox, guidanceBoxObj, guidanceBoxWrapObj) {
        return function () {
            if (menuBox.classList.contains('showGuidanceBox')) {
                menuBox.classList.remove('showGuidanceBox');
                objButton.classList.add('closeBtn');
                guidanceBoxObj.style.display = 'none';
                guidanceBoxWrapObj.style.height = '30px';
                setDispNews('0');
            } else {
                menuBox.classList.add('showGuidanceBox');
                objButton.classList.remove('closeBtn');
                guidanceBoxObj.style.display = '';
                guidanceBoxWrapObj.style.height = '';
                setDispNews('1');
            }
        };
    })(objButton, menuBox, guidanceBoxObj, guidanceBoxWrapObj), true);
}

/**
 * ローカルストレージよりガイダンスの表示/非表示設定を取得
 * @return {undefined} なし
 */
function getDispGuigance() {
    let dtm = localStorage.getItem("menu0001GuidanceSetDate");
    let flg = localStorage.getItem("menu0001GuidanceFlag");
    var dt = new Date();
    var y = dt.getFullYear();
    var m = ("00" + (dt.getMonth() + 1)).slice(-2);
    var d = ("00" + dt.getDate()).slice(-2);
    let currentDtm = y + m + d;
    if (dtm === null) {
        dtm = currentDtm;
        localStorage.setItem('menu0001GuidanceSetDate', dtm);
    }
    if (dtm === currentDtm) {
        if (flg === null) {
            flg = '1';
        }
    } else {
        flg = '1';
        localStorage.setItem('menu0001GuidanceSetDate', currentDtm);
        localStorage.setItem("menu0001GuidanceFlag", flg);
    }
    return flg;
}
/**
 * 左ナビゲーションクリックイベントバインド
 * @param {string} flag 設定するフラグ
 * @return {undefined} なし
 */
function setDispGuidance(flag) {
    localStorage.setItem("menu0001GuidanceFlag", flag);
}

/**
 * ローカルストレージよりお知らせの表示/非表示設定を取得
 * @return {undefined} なし
 */
function getDispNews() {
    let dtm = localStorage.getItem("newsSetDate");
    let flg = localStorage.getItem("newsFlag");
    var dt = new Date();
    var y = dt.getFullYear();
    var m = ("00" + (dt.getMonth() + 1)).slice(-2);
    var d = ("00" + dt.getDate()).slice(-2);
    let currentDtm = y + m + d;
    if (dtm === null) {
        dtm = currentDtm;
        localStorage.setItem('newsSetDate', dtm);
    }
    if (dtm === currentDtm) {
        if (flg === null) {
            flg = '1';
        }
    } else {
        flg = '1';
        localStorage.setItem('newsSetDate', currentDtm);
        localStorage.setItem("newsFlag", flg);
    }
    return flg;
}
/**
 * 左ナビゲーションクリックイベントバインド
 * @param {string} flag 設定するフラグ
 * @return {undefined} なし
 */
function setDispNews(flag) {
    localStorage.setItem("newsFlag", flag);
}

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
        if (menuVscrollObj !== null) {
            if (menuPaneArea !== null) {
                menuVscrollObj.value = menuPaneArea.scrollTop;
            }
        }
        document.forms[0].submit();
    }

}
function downloadPaneData(dlButtonId) {
    let downLoadMarkObj = document.getElementById(dlButtonId);
    if (downLoadMarkObj === null) {
        return;
    }
    let dlMarkObj = document.querySelector("#" + dlButtonId + " + input[type=hidden]");
    let menuVscrollObj = document.getElementById('hdnPaneAreaVScroll');
    let menuPaneArea = document.querySelector('#Menuheaderbox > .menuMain');

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        if (menuVscrollObj !== null) {
            if (menuPaneArea !== null) {
                menuVscrollObj.value = menuPaneArea.scrollTop;
            }
        }

        setTimeout(function () {
            dlMarkObj.value = '';
            downLoadMarkObj.disabled = false;
            document.getElementById("MF_SUBMIT").value = "FALSE";
        }, 2000);
        dlMarkObj.value = '1';
        downLoadMarkObj.disabled = true;
        document.forms[0].submit();
    }
}

/* 
 * ○ドロップダウンリスト選択変更
 */
function selectChangeDdl(ddl) {
    /* サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT */
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        /* 選択値を取得 */
        let idx = document.getElementById(ddl).selectedIndex;
        document.getElementById(ddl + "_LaIdx").value = idx;
        /* 押下されたボタンを設定 */
        document.getElementById("WF_SelectChangeDdl").value = ddl;
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * ロード時処理(共通処理により、カレンダーアイコン付きTextBoxの幅がcalc(100% + 1px)に補正されるのを指定幅に戻す)
 */
window.addEventListener('load', function () {
    /* 帳票条件エリアのカレンダーアイコン付きテキストボックスのstyleを削除 */
    let queryString = "#reportDLAreaPane #reportConditionArea input[type=text].calendarIcon"
    var targetTextBoxList = document.querySelectorAll(queryString);
    if (targetTextBoxList != null) {
        for (let i = 0; i < targetTextBoxList.length; i++) {
            let inputObj = targetTextBoxList[i];
            inputObj.removeAttribute('style')
        }
    }
});
// ○一括ダウンロード処理
function f_ExcelDownload() {
    var objPrintUrl = document.getElementById("WF_PrintURL01").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL02").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL03").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL04").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL05").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL06").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL07").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL08").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL09").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL10").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL11").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL12").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL13").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL14").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL15").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    return false;
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
        }

        link.id = 'commondownloaddummylink';
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
