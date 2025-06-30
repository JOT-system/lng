// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);

    // カレンダー表示
    document.querySelectorAll('.datetimepicker').forEach(picker => {
        flatpickr(picker, {
            wrap: true,
            dateFormat: 'Y/m/d',
            locale: 'ja',
            clickOpens: false,
            allowInput: true,
            plugins: [
                new monthSelectPlugin({
                    shorthand: true, //defaults to false
                    dateFormat: "Y/m",
                    altFormat: "F Y", //defaults to "F Y"
                    theme: "light" // defaults to "light"
                })
            ]
        });
    });

}
// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';
}

/**
 * 全角→半角(英数記号)
 * @param {string} str 変換したい文字列
 * @return {string} 変換された文字列を返す
 */
function replaceFullToHalf(str) {
    return str.replace(/[！-～]/g, function (x) {
        return String.fromCharCode(x.charCodeAt(0) - 0xFEE0);
    });
}

/**
 * 数値入力チェック
 * @param {any} objid オブジェクトID
 * @param {Number} length 桁数
 */
function txtNumberChange(objid, length) {
    let txtObj = document.getElementById(objid);
    let numLen = Number(length);
    if (txtObj !== null) {
        if (String(txtObj.value).length == 0) {
            return true;
        }
        var inputVal = replaceFullToHalf(txtObj.value);
        if (inputVal.match(/^[0-9]+$/g) === null) {
            alert("数値以外は入力できません");
            txtObj.value = ""
            txtObj.focus();
            return false;
        }
        if (String(inputVal).length > numLen) {
            alert(length + "桁以上の数値は入力できません");
            txtObj.value = ""
            txtObj.focus();
            return false;
        }
        txtObj.value = Number(inputVal);
        return true;
    } else {
        return false;
    }
}

/**
 * 数値入力チェック（範囲チェック)
 * @param {any} objid
 * @param {Number} max
 */
function txtNumberChangeMax(objid, max) {
    let txtObj = document.getElementById(objid);
    let numMax = Number(max);
    if (txtObj !== null) {
        if (String(txtObj.value).length == 0) {
            return false;
        }
        var inputVal = replaceFullToHalf(txtObj.value);
        if (inputVal.match(/^[0-9]+$/g) === null) {
            alert("数値以外は入力できません");
            txtObj.value = ""
            txtObj.focus();
            return false;
        }
        if (Number(inputVal) < 1 || Number(inputVal) > numMax) {
            alert("1～" + max + "の範囲の数値を入力してください");
            txtObj.value = ""
            txtObj.focus();
            return false;
        }
        txtObj.value = Number(inputVal);
    } else {
        return false;
    }
}

/**
 * 頁変更ボタンクリック
 * @param {any} type 種別
 */
function btnPageClick(type) {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        //押下されたボタンを設定
        if (Number(type) === 1) {
            document.getElementById("WF_ButtonClick").value = "WF_btnFirstPage";
        } else if (Number(type) === 2) {
            document.getElementById("WF_ButtonClick").value = "WF_btnBackPage";
        } else if (Number(type) === 3) {
            document.getElementById("WF_ButtonClick").value = "WF_btnNextPage";
        } else if (Number(type) === 4) {
            document.getElementById("WF_ButtonClick").value = "WF_btnLastPage";
        } else if (Number(type) === 5) {
            let txtObj = document.getElementById("txtSelectPage");
            if (String(txtObj.value).length == 0) {
                document.getElementById("MF_SUBMIT").value = "FALSE";
                alert("移動する頁数が入力されていません。");
                txtObj.classList.add("error");
                return false;
            }
            document.getElementById("WF_ButtonClick").value = "WF_btnRefreshPage";
        }
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * ファイルアップロード処理
 */
function btnUploadFile() {
    document.getElementById("WF_FileUpload").click();
    return false;
}

// ○一括ダウンロード処理
function f_BulkDownload() {

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
