// 画面読み込み時処理
window.onload = function () {

    // 画面制御処理
    mapSetting()

    setKeyMap()

    AcntLinkOnload() 

    commonBindMonthPicker()

    $(document).ready(function () {
        $("#contents1_ddlInvType").multiselect({
            menuHeight: 390,
            noneSelectedText: "★全選択",
            selectedText: "# 個選択",
            autoopen: false,
            multiple: true,
            buttonWidth: 125,

            position: {
                my: 'center',
                at: 'center'
            }
        });

        $("#contents1_ddlStatus").multiselect({
            menuHeight: 390,
            noneSelectedText: "★全選択",
            selectedText: "# 個選択",
            autoopen: false,
            multiple: true,
            buttonWidth: 145,

            position: {
                my: 'center',
                at: 'center'
            }
        });
    });
    
};

// ロード時処理
window.addEventListener('load', () => {

    document.getElementById("txtSelectPage").classList.remove("error");

    // 頁番号(フォーカス消失)
    $('input#txtSelectPage').blur(function (event) {
        // 数値入力チェック(範囲=最大頁数)
        txtNumberChangeMax(this.id, Number($('span#lblMaxPage').get(0).innerText));
    });
    
    // tableヘッダ、列固定スクロールライブラリ開始
    FixedMidashi.create();
    
});

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

function setKeyMap() {

    // 承認者制御フラグ
    const ApprovalUserFlg = document.getElementById('WF_ApprovalUserFlg').value;
    // 承認者の場合
    if (ApprovalUserFlg == '0') {
        // 担当者ラベルを表示
        document.getElementById('WF_RQSTAFF_LABEL').style.display = 'none'
        // 担当者入力ボックスを表示
        document.getElementById('WF_RQSTAFF').style.display = 'none'
        // 担当者スパン2を表示
        document.getElementById('stafflabels2').style.display = 'none'
        // 担当者スパン3を表示
        document.getElementById('stafflabels3').style.display = 'none'
        // 担当者スパン5を表示
        document.getElementById('stafflabels5').style.display = 'none'
        // 担当者スパン6を表示
        document.getElementById('stafflabels6').style.display = 'inline'
        // 担当者スパン7を表示
        document.getElementById('stafflabels7').style.display = 'none'
        // 担当者スパン8を表示
        document.getElementById('stafflabels8').style.display = 'inline'
        // 担当者スパン9を表示
        document.getElementById('stafflabels9').style.display = 'inline'
    } else {
        // 担当者ラベルを表示
        document.getElementById('WF_RQSTAFF_LABEL').style.display = 'inline'
        // 担当者入力ボックスを表示
        document.getElementById('WF_RQSTAFF').style.display = 'inline'
        // 担当者スパン2を表示
        document.getElementById('stafflabels2').style.display = 'inline'
        // 担当者スパン3を表示
        document.getElementById('stafflabels3').style.display = 'inline'
        // 担当者スパン5を表示
        document.getElementById('stafflabels5').style.display = 'inline'
        // 担当者スパン6を表示
        document.getElementById('stafflabels6').style.display = 'none'
        // 担当者スパン7を表示
        document.getElementById('stafflabels7').style.display = 'inline'
        // 担当者スパン8を表示
        document.getElementById('stafflabels8').style.display = 'none'
        // 担当者スパン9を表示
        document.getElementById('stafflabels9').style.display = 'none'
    }
}

// ○経理連携用処理
function AcntLinkOnload() {

  // 経理連携時ボタン制御
    if (document.getElementById('WF_SEL_ALL').value === "1"){
        document.getElementById('WF_ButtonDRAFTINVBULK_COOP').disabled = true;
        document.getElementById('WF_ButtonINVBULK_COOP').disabled = true;
        document.getElementById('WF_ButtonINSERT').disabled = true;
        document.getElementById('WF_ButtonSPREAD_SEL_ALL').disabled = true;
        document.getElementById('WF_ButtonSPREAD_SEL_DEL').disabled = true;
        document.getElementById('WF_ButtonAPPLICATION').disabled = true;
        document.getElementById('WF_ButtonAPPROVAL').disabled = true;
        document.getElementById('WF_ButtonCANCEL').disabled = true;
        document.getElementById('WF_ButtonREJECT').disabled = true;
    } else {
        document.getElementById('WF_ButtonDRAFTINVBULK_COOP').disabled = false;
        document.getElementById('WF_ButtonINVBULK_COOP').disabled = false;
        document.getElementById('WF_ButtonINSERT').disabled = false;
        document.getElementById('WF_ButtonSPREAD_SEL_ALL').disabled = false;
        document.getElementById('WF_ButtonSPREAD_SEL_DEL').disabled = false;
        document.getElementById('WF_ButtonAPPLICATION').disabled = false;
        document.getElementById('WF_ButtonAPPROVAL').disabled = false;
        document.getElementById('WF_ButtonCANCEL').disabled = false;
        document.getElementById('WF_ButtonREJECT').disabled = false;
    }
}

// 画面制御処理
function mapSetting() {
    // ダウンロードボタン表示フラグ
    const downloadFlg = document.getElementById('WF_DownloadFlg').value;

    // 情報システム部の場合
    if (downloadFlg == '1') {
        // ダウンロードボタンを表示
        document.getElementById('WF_ButtonDOWNLOAD').style.display = 'inline'
    } else {
        // ダンロードボタンを非表示
        document.getElementById('WF_ButtonDOWNLOAD').style.display = 'none'
    }

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

/**
 *  グリッドダブルクリックイベント
 * @param {any} lineCnt     'DataTable対象行
 */
function ListDbClick(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_SpreadDBclick";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  ドラフト版連携ボタンクリックイベント
 * @param {any} invtype     '請求書種類
 * @param {any} lineCnt     'DataTable対象行
 */
function DraftRenkeiBtnClick(lineCnt, invtype) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;
        document.getElementById('WF_INVTYPE').value = invtype;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonDRAFT_BTN";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  正式版連携ボタンクリックイベント
 * @param {any} invtype     '請求書種類
 * @param {any} lineCnt     'DataTable対象行
 */
function RenkeiBtnClick(lineCnt, invtype) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;
        document.getElementById('WF_INVTYPE').value = invtype;

        document.getElementById("WF_ButtonClick").value = "WF_Button_BTN";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
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
    objPrintUrl = document.getElementById("WF_PrintURL5").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL6").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL7").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL8").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL9").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL10").value;
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

