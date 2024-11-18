// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* スプレッドシートのダブルクリックイベント紐づけ */
    var spread = document.getElementById("FpSpread1");

    if (spread.addEventListener) {
        spread.addEventListener("dblclick", DblClick, false);
    } else {
        spread.ondblclick = DblClick;
    }

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

    p1atb = p1.getAttribute("FpSpread");
    p2atb = p2.getAttribute("FpSpread");
    p3atb = p3.getAttribute("FpSpread");

    var cl = p1atb

    if (cl == undefined) {
        cl = p2atb;
        if (cl == undefined) {
            cl = p3atb;
        }
    }

    //詳細選択押下処理
    spGetvalue()
}

/**
 * 詳細選択押下処理
 */
function spGetvalue() {
    var spread = document.getElementById(spid);
    var row = spread.GetActiveRow();
    var col = spread.GetActiveCol();

    if (row == -1 || col == -1) {
        alert("セルを選択してください。");
        return
    }

    //選択行を非表示項目にセット
    hidRowIndex.value = row;

    //サブミット
    document.getElementById("MF_SUBMIT").value = "TRUE";
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclick";
    document.forms[0].submit();
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
    objPrintUrl = document.getElementById("WF_PrintURL3").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL4").value;
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