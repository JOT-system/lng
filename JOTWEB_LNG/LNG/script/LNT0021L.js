// 画面読み込み時処理
window.onload = function () {
    
    // スプレッドのフォーカス移動設定処理
    setKeyMap();
    
    //月選択バインド
    commonBindMonthPicker();
    BindMonthPicker();

    //決済条件spreadOnload
    KekkjSrcOnload();

    RemarkPopupOnload();

    HistoryPopupOnload();

    //表示している明細に応じ、ボタン活性制御
    var detail = document.getElementById('WF_DETAILFLG').value;
    if (detail == "0") {
        document.getElementById('WF_ButtonCALC').disabled = false;
        document.getElementById('WF_ButtonCONFIRM').disabled = false;
        document.getElementById('WF_ButtonUNCONFIRM').disabled = false;
        document.getElementById('WF_ButtonSAVE').disabled = false;
        document.getElementById('WF_ButtonREQUEST').disabled = false;
    } else {
        document.getElementById('WF_ButtonCALC').disabled = true;
        document.getElementById('WF_ButtonCONFIRM').disabled = true;
        document.getElementById('WF_ButtonUNCONFIRM').disabled = true;
        document.getElementById('WF_ButtonSAVE').disabled = true;
        document.getElementById('WF_ButtonREQUEST').disabled = true;
    };

    //原価計算ボタン活性制御 確定、未確定ボタン表示制御
    var calc = document.getElementById('WF_CONTUSER').value;
    if (calc == "1") {
        if (detail == "0") {
            document.getElementById('WF_ButtonCALC').disabled = false;
            document.getElementById('WF_ButtonCONFIRM').disabled = false;
            document.getElementById('WF_ButtonUNCONFIRM').disabled = false;
        };
    } else {
        document.getElementById('WF_ButtonCALC').disabled = true;
        document.getElementById('WF_ButtonCONFIRM').disabled = true;
        document.getElementById('WF_ButtonUNCONFIRM').disabled = true;
    };

    var confirm = document.getElementById('WF_GoodsSalesConfirm').value;
    if (confirm == "1") {
        document.getElementById('WF_ButtonCALC').disabled = true;
        document.getElementById('WF_ButtonSAVE').disabled = true;
        document.getElementById('WF_ButtonREQUEST').disabled = true;
    } else {
        if (detail == "0") {
            if (calc == "1") {
                document.getElementById('WF_ButtonCALC').disabled = false;
            };
            document.getElementById('WF_ButtonSAVE').disabled = false;
            document.getElementById('WF_ButtonREQUEST').disabled = false;
        };
    };

    //表示明細切り替え
    var detailflg = document.getElementById('WF_DETAILFLG').value;
    if (detailflg == "0") {
        document.getElementById('tblWrapper_Control').style.display = 'inline';
        document.getElementById('tblWrapper_Approval').style.display = 'none';
    } else {
        document.getElementById('tblWrapper_Control').style.display = 'none';
        document.getElementById('tblWrapper_Approval').style.display = 'inline';
    };

    $(document).ready(function () {
        $("#contents1_ddlControl").multiselect({
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
            buttonWidth: 150,

            position: {
                my: 'center',
                at: 'center'
            }
        });

        $("#contents1_ddlBigCtnCd").multiselect({
            menuHeight: 390,
            noneSelectedText: "★全選択",
            selectedText: "# 個選択",
            autoopen: false,
            multiple: true,
            buttonWidth: 120,

            position: {
                my: 'center',
                at: 'center'
            }
        });
    });

    // スクロール位置を復元 
    let divtbl_ctr = document.getElementById("tblWrapper_Control").getElementsByTagName("div");
    if (divtbl_ctr[0] !== null) {
        divtbl_ctr[0].scrollTop = document.getElementById("WF_ClickedScrollTop_ctr").value;
        divtbl_ctr[0].scrollLeft = document.getElementById("WF_ClickedScrollLeft_ctr").value;
    };
    let divtbl_app = document.getElementById("tblWrapper_Approval").getElementsByTagName("div");
    if (divtbl_app[0] !== null) {
        divtbl_app[0].scrollTop = document.getElementById("WF_ClickedScrollTop_app").value;
        divtbl_app[0].scrollLeft = document.getElementById("WF_ClickedScrollLeft_app").value;
    };

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
};

/**
 *  年月選択Pickerの表示イベントバインド
 * @return {undefined} なし
 * @description 
 */
function BindMonthPicker() {
    let targetTextBoxes = document.querySelectorAll("input[type=text][data-monthpicker-2]");
    for (let i = 0; i < targetTextBoxes.length; i++) {
        targetTextBox = targetTextBoxes[i];
        targetTextId = targetTextBox.id;
        /* 対象のテキストをspanで括る */
        let spanWrapper = document.createElement('span');
        spanWrapper.classList.add('commonMonthWrapperPicker');
        targetTextBox.parentNode.insertBefore(spanWrapper, targetTextBox);
        spanWrapper.appendChild(targetTextBox);
        targetTextBox = document.getElementById(targetTextId);
        targetTextBox.addEventListener('click', (function (targetTextBox) {
            return function () {
                commonDispMonthPicker(targetTextBox);
            };
        })(targetTextBox), true);
    }
}

/**
 * LEFTBOX用処理カスタム
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} lineCnt     'DataTable対象行
 * @param {any} listNo      'リスト番号
 */
function FD_DBclick(fieldId,  lineCnt, listNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        // クリック位置取得
        var elem = document.getElementById(fieldId);
        var rect = elem.getBoundingClientRect();
        var toriname = document.getElementById(fieldId).value;
        document.getElementById("WF_saveTop").value = rect.top + rect.height;
        document.getElementById("WF_saveLeft").value = rect.left;

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;
        document.getElementById("WF_TORINAME").value = toriname;
        document.getElementById('WF_LeftMViewChange').value = listNo;
        document.getElementById('WF_LeftboxOpen').value = "Open";

        document.getElementById("WF_ButtonClick").value = "WF_Field_DBClick";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * LEFTBOX用処理カスタム
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} invoiceType '請求書種別
 * @param {any} lineCnt     'DataTable対象行
 * @param {any} listNo      'リスト番号
 */
function FD_Kekkjmclick(fieldId, lineCnt, listNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        // クリック位置取得
        var elem = document.getElementById(fieldId);
        var rect = elem.getBoundingClientRect();
        document.getElementById("WF_saveTop").value = rect.top + rect.height;
        document.getElementById("WF_saveLeft").value = rect.left;

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonKekkJ";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * LEFTBOX用処理カスタム
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} lineCnt     'DataTable対象行
 */
function FD_Txtchenge(fieldId, lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_LeftBoxSelectClick";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * LEFTBOX用処理カスタム
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} lineCnt     'DataTable対象行
 */
function FD_Ddlchenge(fieldId, lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ddlInvOrg_OnChange";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

// ○メモOnLoad用処理
function RemarkPopupOnload() {

    let ele = document.getElementById('pnlRemarkRichTextWrapper');
    ele.style.visibility = document.getElementById('WF_RemarkSrc').value;
}

// ○メモ履歴OnLoad用処理
function HistoryPopupOnload() {

    let ele = document.getElementById('pnlMemoHistoryWrapper');
    ele.style.visibility = document.getElementById('WF_Memo').value;
}

/**
 * LEFTBOX用処理カスタム
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} lineCnt     'DataTable対象行
 * @param {any} listNo      'リスト番号
 */
function FD_Remarkclick(fieldId, lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonRemark";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  メモダイアログ『OK』ボタンクリックイベント
 */
function RemarkSrcOKClick() {

    ButtonClick('WF_ButtonRemarkSrcOK');
}

/**
 *  メモダイアログ『閉じる』ボタンクリックイベント
 */
function RemarkSrcCloseClick() {

    ButtonClick('WF_ButtonRemarkSrcCLOSE');
}

/**
 *  『履歴を見る』ボタンクリックイベント
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} lineCnt     'DataTable対象行
 * @param {any} listNo      'リスト番号
 */
function FD_Historyclick(fieldId, lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonHISTORY";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  履歴一覧『閉じる』ボタンクリックイベント
 */
function historyCloseClick() {

    ButtonClick('WF_ButtonHistoryCLOSE');
}

/**
 *  『訂正』ボタンクリックイベント
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} lineCnt     'DataTable対象行
 * @param {any} listNo      'リスト番号
 */
function FD_Fixclick(fieldId, lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonFIX";
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

// スプレッドのフォーカス移動設定処理
function setKeyMap() {
    
// ダウンロードボタン入力不可制御
//if (document.getElementById('WF_CSVDLDisabledFlg').value === "1"){
//    document.getElementById('WF_CSV_DL').disabled = true;
//} else {
//    document.getElementById('WF_CSV_DL').disabled = false;
//}
}

// ○決済条件検索OnLoad用処理
function KekkjSrcOnload() {

    let ele = document.getElementById('pnlKekkjSrcWrapper');
    ele.style.visibility = document.getElementById('WF_KekkjSrc').value;

    /* スプレッドシートのダブルクリックイベント紐づけ */
    var spread = document.getElementById("spdKekkjm");

    if (spread.addEventListener) {
        spread.addEventListener("dblclick", DblClickKekkjm, false);
    } else {
        spread.ondblclick = DblClickKekkjm;
    }

}

/**
 *  決済条件検索ダイアログ ボタンクリックイベント
 */
function kekkjmSrc_Click() {

    ButtonClick('WF_ButtonKekkJ');
}

/**
 *  決済条件検索ダイアログ『閉じる』ボタンクリックイベント
 */
function KekkjSrcCloseClick() {

    ButtonClick('WF_ButtonKekkjSrcCLOSE');
}

/**
 * スプレッドシート・ダブルクリック処理
 */
function DblClickKekkjm(e) {
    var e = e || window.event;
    var s = e.target || e.srcElement;
    var p1 = s.parentNode || s.parentElement;
    var p2 = s.parentNode.parentNode || s.parentElement.parentElement;
    var p3 = s.parentNode.parentNode.parentNode || s.parentElement.parentElement.parentElement;

    //属性の取得
    p1atb = p1.getAttribute("spdKekkjm");
    p2atb = p2.getAttribute("spdKekkjm");
    p3atb = p3.getAttribute("spdKekkjm");

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
    var spread = document.getElementById("spdKekkjm");
    var row = spread.GetActiveRow();
    var col = spread.GetActiveCol();

    //選択行を非表示項目にセット
    hidRowIndex.value = row;

    if (row == -1 || col == -1) {
        return
    }

    //サブミット
    document.getElementById("MF_SUBMIT").value = "TRUE";
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclickKekkjm";
    commonDispWait();
    document.forms[0].submit();
}

// ロード時処理
window.addEventListener('load', () => {

    // tableヘッダ、列固定スクロールライブラリ開始
    FixedMidashi.create();

});

function saveScrollPosition_Control() {
    let detailbox = document.getElementById("tblWrapper_Control").getElementsByTagName("div")
    if (detailbox !== null) {
        document.getElementById("WF_ClickedScrollTop_ctr").value = detailbox[0].scrollTop;
        document.getElementById("WF_ClickedScrollLeft_ctr").value = detailbox[0].scrollLeft;
    }
}

function saveScrollPosition_Approval() {
    let detailbox = document.getElementById("tblWrapper_Approval").getElementsByTagName("div")
    if (detailbox !== null) {
        document.getElementById("WF_ClickedScrollTop_app").value = detailbox[0].scrollTop;
        document.getElementById("WF_ClickedScrollLeft_app").value = detailbox[0].scrollLeft;
    }
}

window.addEventListener("DOMContentLoaded", () => {
    // 全体スクロールイベントに、ポジション記録処理を付与する
    let divlist_ctr = document.getElementById("tblWrapper_Control").getElementsByTagName("div");
    divlist_ctr[0].addEventListener('scroll', saveScrollPosition_Control);

    let divlist_app = document.getElementById("tblWrapper_Approval").getElementsByTagName("div");
    divlist_app[0].addEventListener('scroll', saveScrollPosition_Approval);
});


/**
 * コード、名称手入力検索
 * @return {undefined} なし
 * @param {string} val      入力項目ID
 * @param {string} list     データ保持リストID
 * @param {string} code     取得値設定コード
 * @param {string} name     取得値設定名称
 * @param {string} clear1   変更時クリア項目
 * @param {string} srcid    検索子画面呼び出し用
 * @param {string} tabNo    検索子画面呼び出し用
 * @param {string} linecnt  行数
 * @description 入力された内容から、コード、名称を取得
 */
function ToriCodeName_OnChange(val, list, code, name, clear1, srcid, tabNo, linecnt) {
    var strToriVal = document.getElementById(val).value;
    var strToriList = document.getElementById(list);

    // 未入力の場合、処理を行わない
    if (strToriVal == "") {
        document.getElementById(code).value = "";
        document.getElementById(name).value = "";
        document.getElementById(clear1).value = "";
        return;
    }

    var count = 0;
    var flg = 0;
    var rtnCode = "";
    var rtnName = "";

    // コード、名称を検索
    for (var i = 0; i < strToriList.length; i++) {

        // コード検索
        if (strToriList.options[i].value.indexOf(strToriVal) > -1) {
            //件数カウント、フラグを立てる
            count += 1;
            flg = 1;
        }

        // 部分一致しているデータを設定（最初に見つかった１件のみ取得）
        if (flg == 1 && count == 1) {
            rtnCode = strToriList.options[i].value;
            rtnName = strToriList.options[i].textContent;
        }

        // フラグクリア
        flg = 0;
    }

    // 1件のみ取得の場合、設定
    if (count == 1) {
        document.getElementById(code).value = rtnCode;
        document.getElementById(name).value = rtnName;
        document.getElementById(clear1).value = "";
    } else {
        rtnCode = "仮登録";
        rtnName = strToriVal;
        document.getElementById(code).value = rtnCode;
        document.getElementById(name).value = rtnName;
        document.getElementById(clear1).value = "";
    }
}