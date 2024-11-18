// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

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
 * 行ダブルクリック(検査登録ダイアログ表示)処理
 * @param {any} lineCnt 行番号
 */
function rowDblClick(lineCnt) {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_ShowDialog";
        document.getElementById("WF_SelectedIndex").value = lineCnt;
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  検査登録ダイアログ『キャンセル』クリックイベント
 */
function btnInspectDialogCancelClick() {

    let ele = document.getElementById('pnlInspectDialogWrapper');
    let showFlg = this.document.getElementById('hdnShowPnlInspectDialog');
    if (ele != null && showFlg != null) {
        showFlg.value = '0';
        ele.style.visibility = 'hidden';
    }
    event.preventDefault();  //イベントをキャンセル
}

/**
 * 検査登録ダイアログ行 定期検査行追加
 */
function AddRegularInspectRow() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_RegularInspectRow_Add";
        // disabled解除
        $.each($('select[name*="INSPECTVENDOR"] > option[disabled]'), (index, node) => {
            node.removeAttribute('disabled');
        });
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * 検査登録ダイアログ行 追加検査行追加
 */
function AddAdditionInspectRow() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_AdditionInspectRow_Add";
        // disabled解除
        $.each($('select[name*="INSPECTVENDOR"] > option[disabled]'), (index, node) => {
            node.removeAttribute('disabled');
        });
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * 検査登録ダイアログ行 検査行削除
 * @param {any} lineCnt
 */
function DelInspectRow(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_InspectRow_Del";
        document.getElementById("WF_DelInspectRowIndex").value = lineCnt;
        // disabled解除
        $.each($('select[name*="INSPECTVENDOR"] > option[disabled]'), (index, node) => {
            node.removeAttribute('disabled');
        });
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * 検査登録ダイアログ 『更新』クリックイベント
 */
function btnInspectDialogUpdateClick() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_INSPECT_UPDATE";
        // disabled解除
        $.each($('select[name*="INSPECTVENDOR"] > option[disabled]'), (index, node) => {
            node.removeAttribute('disabled');
        });
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * 検査コード変更時、検査名書き換え処理
 * @param {any} objid
 */
function ChangeInspectName(objid) {
    let selObj = document.getElementById(objid);
    let txtObj = document.getElementById(String(objid).replace("INSPECTCODE", "INSPECTNAME"));
    let codesObj = document.getElementById("WF_InspectCodes");
    if (selObj !== null && txtObj !== null && codesObj !== null) {
        var codes = JSON.parse(codesObj.value);
        var i = 0;
        txtObj.value = "";
        for (var i = 0; i < codes.length; i++) {
            if (codes[i].code == selObj.value) {
                txtObj.value = codes[i].name;
                break;
            }
        }
    }
}

/**
 * 実施場所入力時、駅コードto駅名変換処理
 * @param {any} objid
 */
function ConvertStationName(objid) {
    let txtObj = document.getElementById(objid);
    let staObj = document.getElementById("WF_StationTable");
    if (txtObj !== null && staObj !== null) {
        if (String(txtObj.value).length == 0 || String(staObj.value).length == 0) {
            return;
        }
        var inputVal = replaceFullToHalf(txtObj.value);
        if (inputVal.match(/^[0-9]+$/g) === null) {
            return;
        }
        var staTable = JSON.parse(staObj.value);
        var i = 0;
        for (var i = 0; i < staTable.length; i++) {
            if (staTable[i].code == inputVal) {
                txtObj.value = staTable[i].name;
                break;
            }
        }
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

// ロード時処理
window.addEventListener('load', () => {

    // tableヘッダ、列固定スクロールライブラリ開始
    FixedMidashi.create();

    document.getElementById("txtSelectPage").classList.remove("error");

    // 検査登録ダイアログ表示/非表示処理
    ele = document.getElementById('pnlInspectDialogWrapper');
    showFlg = this.document.getElementById('hdnShowPnlInspectDialog');
    if (ele != null && showFlg != null) {
        if (showFlg.value === '0') {
            ele.style.visibility = 'hidden';
        } else {
            ele.style.visibility = 'visible';
        }
    }
    // Date項目 未入力の場合は文字色を変更して消す
    $.each($('input[type=date].nothing'), (index, datebox) => {
        datebox.style.color = (datebox.value) ? '#000000' : '#FFFFFF';
    });
    $.each($('input[type=date].caution'), (index, datebox) => {
        datebox.style.color = (datebox.value) ? '#000000' : '#FFFF66';
    });
    $.each($('input[type=date].warning'), (index, datebox) => {
        datebox.style.color = (datebox.value) ? '#FFFFFF' : '#FF3333';
    });
    $.each($('input[type=date].registed'), (index, datebox) => {
        datebox.style.color = (datebox.value) ? '#000000' : '#FCE4D6';
    });
    // Date項目 フォーカス取得時は入力用にいったん色を付ける
    $('input[type=date].nothing').focus(function (event) {
        this.style.color = '#000000';
    });
    $('input[type=date].caution').focus(function (event) {
        this.style.color = '#000000';
    });
    $('input[type=date].warning').focus(function (event) {
        this.style.color = '#FFFFFF';
    });
    $('input[type=date].registed').focus(function (event) {
        this.style.color = '#000000';
    });
    // Date項目 フォーカス喪失後に未入力の場合は文字色を変更して消す
    $('input[type=date].nothing').blur(function (event) {
        this.style.color = (this.value) ? '#000000' : '#FFFFFF';
    });
    $('input[type=date].caution').blur(function (event) {
        this.style.color = (this.value) ? '#000000' : '#FFFF66';
    });
    $('input[type=date].warning').blur(function (event) {
        this.style.color = (this.value) ? '#FFFFFF' : '#FF3333';
    });
    $('input[type=date].registed').blur(function (event) {
        this.style.color = (this.value) ? '#000000' : '#FCE4D6';
    });

    // 入力項目イベント付与
    // コンテナ番号(変更)
    $('input#WF_CTNNO').change(function (event) {
        // 数値入力チェック(桁数=6)OKならsubmitイベント
        if (txtNumberChange(this.id, 6) === true) {
            return ButtonClick('WF_CTNNO');
        }
    });
    // 駅コード(変更)
    $('input#WF_STATION').change(function (event) {
        // 数値入力チェック(桁数=6)OKならsubmitイベント
        if (txtNumberChange(this.id, 6) === true) {
            return ButtonClick('WF_STATION');
        }
    });
    // 頁番号(フォーカス消失)
    $('input#txtSelectPage').blur(function (event) {
        // 数値入力チェック(範囲=最大頁数)
        txtNumberChangeMax(this.id, Number($('span#lblMaxPage').get(0).innerText));
    });
    // 検査年(フォーカス消失)
    $('input[type=text][id*="INSPECTYEAR"]').blur(function (event) {
        // 数値入力チェック(桁数=4)
        txtNumberChange(this.id, 4);
    });
    // 検査コード(変更)
    $('select[id*="INSPECTCODE"]').change(function (event) {
        // 検査名書き換え
        ChangeInspectName(this.id);
    });
    // 実施場所(変更)
    $('input[type=text][id*="ENFORCEPLACE"]').change(function (event) {
        // 駅コード→名称書き換え
        ConvertStationName(this.id);
    });
});