// 画面読み込み時処理
window.onload = function () {

    // 画面制御処理
    mapSetting()

    setKeyMap()
    
    //月選択バインド
    commonBindMonthPicker();
    
    BtnDownloadOnload()
    
    AcntLinkOnload()
    
};

// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* スプレッドシートのダブルクリックイベント紐づけ */
    var spread = document.getElementById("spdPayeeList");

    if (spread.addEventListener) {
        spread.addEventListener("dblclick", DblClick, false);
    } else {
        spread.ondblclick = DblClick;
    }

    //メモOnLoad用処理
    MemoOnload()
}

// ○ダウンロードボタン表示制御
function BtnDownloadOnload() {

    let ele = document.getElementById('WF_ButtonPAYEE_DOWNLOAD');
    ele.style.visibility = document.getElementById('WF_DOWNLOAD').value;
}

// ○担当者者表示制御
function RqstaffOnload() {

    let ele = document.getElementById('WF_RQSTAFF_LABEL');
    ele.style.visibility = document.getElementById('WF_TEXTRQSTAFF').value;
    let ele2 = document.getElementById('TxtRqstaffName');
    ele2.style.visibility = document.getElementById('WF_TEXTRQSTAFF').value;
}

// 画面制御処理
function mapSetting() {
    // 担当者・確認者制御フラグ
    const staffFlg = document.getElementById('WF_StaffFlg').value;

    // 担当者の場合
    if (staffFlg == '1') {
        // 申請ボタンを表示
        document.getElementById('WF_ButtonAPPLICATION').style.display = 'inline'
        // 承認ボタンを非表示
        document.getElementById('WF_ButtonAPPROVAL').style.display = 'none'
        // 取下ボタンを表示
        document.getElementById('WF_ButtonCANCEL').style.display = 'inline'
        // 却下ボタンを非表示
        document.getElementById('WF_ButtonREJECT').style.display = 'none'
    } else {
        // 申請ボタンを表示
        document.getElementById('WF_ButtonAPPLICATION').style.display = 'none'
        // 承認ボタンを非表示
        document.getElementById('WF_ButtonAPPROVAL').style.display = 'inline'
        // 取下ボタンを表示
        document.getElementById('WF_ButtonCANCEL').style.display = 'none'
        // 却下ボタンを非表示
        document.getElementById('WF_ButtonREJECT').style.display = 'inline'
    }

}

// ○スプレッドシート内ボタン押下処理
function Spred_ButtonSel_click(btn) {
    var sheet = document.getElementById("spdPayeeList");
    var row = sheet.GetActiveRow();
    var col = sheet.GetActiveCol();
    var elem = document.activeElement;
    var result1 = elem.id.indexOf('_');
    var result2 = elem.id.indexOf(',');

    if (result1 != -1 && result2 != -1) {
        var len1 = result1 + 1;
        var len2 = result2;
        var len3 = result2 + 1;
        var Substrrow = elem.id.substring(len1, len2);
        var Substrcol = elem.id.substring(len3);
        var Introw = Number(Substrrow);
        var Intcol = Number(Substrcol);
        var PageMaxRow = 20

        if (Introw >= PageMaxRow) {
            while (Introw >= PageMaxRow) {
                Introw -= PageMaxRow;
                if (Introw >= PageMaxRow) {
                    continue;
                } else {
                    break;
                };
            };
        };

        if (row == Introw && col == Intcol) {
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = btn;
                //document.body.style.cursor = "wait";
                commonDispWait();
                document.forms[0].submit();
            };
        };
    };
}

/**
 * スプレッドシート・ダブルクリック処理
 */
function DblClick(e) {
    var e = e || window.event;
    var s = e.target || e.srcElement;
    var p1 = s.parentNode || s.parentElement;
    var p2 = s.parentNode.parentNode || s.parentElement.parentElement;
    var p3 = s.parentNode.parentNode.parentNode || s.parentElement.parentElement.parentElement;

    //属性の取得
    p1atb = p1.getAttribute("spdPayeeList");
    p2atb = p2.getAttribute("spdPayeeList");
    p3atb = p3.getAttribute("spdPayeeList");

    //p1atbを設定
    var cl = p1atb

    //p1atbが存在しない場合
    if (cl == undefined) {
        //p2atbを設定
        cl = p2atb;
        //p2atbが存在しない場合
        if (cl == undefined) {
            //p3atbを設定
            cl = p3atb;
        }
    }

    //処理
    var spread = document.getElementById(spid);
    var row = spread.GetActiveRow();
    var col = spread.GetActiveCol();

    //選択行を非表示項目にセット
    hidRowIndex.value = row;

    //サブミット
    document.getElementById("MF_SUBMIT").value = "TRUE";
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclick";
    document.forms[0].submit();
}

function setKeyMap() {
    var s = document.getElementById(spid);
    var kcode;
    kcode = 13;

    s.AddKeyMap(kcode, false, false, false, "element.MoveToNextCell()");
    s.AddKeyMap(kcode, false, true, false, "element.MoveToPrevCell()");

    // 金額更新ボタン入力不可制御
    if (document.getElementById('WF_BtnDisabledFlg').value === "1") {
        document.getElementById('btnMemoOK').disabled = true;
    } else {
        document.getElementById('btnMemoOK').disabled = false;
    }
}

// ○メモOnLoad用処理
function MemoOnload() {

    let ele = document.getElementById('pnlMemoWrapper');
    ele.style.visibility = document.getElementById('WF_Memo').value;
}

// ○経理連携用処理
function AcntLinkOnload() {

  // 経理連携時ボタン制御
    if (document.getElementById('WF_SEL_ALL').value === "1"){
        document.getElementById('WF_ButtonSPREAD_SEL_ALL').disabled = true;
        document.getElementById('WF_ButtonSPREAD_SEL_DEL').disabled = true;
        document.getElementById('WF_ButtonAPPLICATION').disabled = true;
        document.getElementById('WF_ButtonAPPROVAL').disabled = true;
        document.getElementById('WF_ButtonCANCEL').disabled = true;
        document.getElementById('WF_ButtonREJECT').disabled = true;
        document.getElementById('WF_ButtonDraftPayment_COOP').disabled = true;
        document.getElementById('WF_ButtonPAYEE_COOP').disabled = true;
    } else {
        document.getElementById('WF_ButtonSPREAD_SEL_ALL').disabled = false;
        document.getElementById('WF_ButtonSPREAD_SEL_DEL').disabled = false;
        document.getElementById('WF_ButtonAPPLICATION').disabled = false;
        document.getElementById('WF_ButtonAPPROVAL').disabled = false;
        document.getElementById('WF_ButtonCANCEL').disabled = false;
        document.getElementById('WF_ButtonREJECT').disabled = false;
        document.getElementById('WF_ButtonDraftPayment_COOP').disabled = false;
        document.getElementById('WF_ButtonPAYEE_COOP').disabled = false;
    }
}

/**
 *  『メモ』ボタンクリックイベント
 * @param {object} obj TR(行)オブジェクト
 * @param {string} lineCnt 行No
 * @param {string} colName カラム名
 * @return {undefined} なし
 * @description グリッド内ボタン押下イベント
 */
function memoClick() {

    ButtonClick('WF_ButtonMEMO');
}

/**
 *  『メモ_決定』ボタンクリックイベント
 */
function memoOkClick() {

    ButtonClick('WF_ButtonMemoOK');
}

/**
 *  『メモ_キャンセル』ボタンクリックイベント
 */
function memoCancelClick() {

    ButtonClick('WF_ButtonMemoCANCEL');
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
