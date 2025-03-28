﻿
function InitDisplay() {
    /* 読み取りモードではない場合ファイルアップロード関連イベントバインド */
    if (!document.forms[0].hasAttribute('REFONLY')) {
        bindDragFileEvent();
        bindUploadButton();
        let fileUpLineOjb = document.getElementById('uploadLineText');
        fileUpLineOjb.title = acceptExtentionsStr;
    }
    let fileNameListObj = document.getElementById("WF_FILENAMELIST");
    if (fileNameListObj !== null) {
        fileNameListObj.text = '';
    }
}
/* ドラッグイベントのバインド */
function bindDragFileEvent() {
    let attachAreaObj = document.getElementById('divAttachmentArea');
    attachAreaObj.addEventListener('dragstart', function (event) { dragEventCancel(event); }, false);
    attachAreaObj.addEventListener('dragenter', function (event) { dragEventEnter(event); }, false);
    attachAreaObj.addEventListener('dragover', function (event) { dragOverEvent(event); }, false);
    attachAreaObj.addEventListener('dragleave', function (event) { dragEventLeave(event); }, false);
    attachAreaObj.addEventListener('drag', function (event) { dragEventCancel(event); }, false);
    attachAreaObj.addEventListener('drop', function () {
        return function (event) {
            let attachAreaObj = document.getElementById('divAttachmentArea');
            //alert('まだ未実装です！');
            attachAreaObj.classList.remove('dragging');
            dropEvent(event, acceptExtentions);
        };
    }(), false);
}
/**
 * ドロップ処理（処理抑止）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function dragOverEvent(event) {
    //event.preventDefault();  //イベントをキャンセル
    event.preventDefault();
    event.dataTransfer.dropEffect = 'copy'; //ドラッグする文言を変更 CHROMEのみワーク
}
/**
 * ドロップ処理（処理抑止）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function dragEventCancel(event) {
    event.preventDefault();  //イベントをキャンセル
}
/**
 * ドロップ処理（処理抑止）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function dragEventEnter(event) {
    let attachAreaObj = document.getElementById('divAttachmentArea');
    event.preventDefault();  //イベントをキャンセル
    attachAreaObj;
    attachAreaObj.classList.add('dragging');
}
/**
 * ドロップ処理（処理抑止）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function dragEventLeave(event) {
    let attachAreaObj = document.getElementById('divAttachmentArea');
    event.preventDefault();  //イベントをキャンセル
    attachAreaObj.classList.remove('dragging');

}
/**
 * ドロップ処理（ドラッグドロップ入力）
 * @param {Event} e ドラッグイベントオブジェクト
 * @param {Array} acceptExtentions 許可拡張子配列(未設定時は全対象)
 * @return {undefined} なし
 * @description
 */
function dropEvent(e, acceptExtentions) {
    e.preventDefault();
    commonDispWait();
    // ********************************
    // フッターボックスのオブジェクト取得
    // ********************************
    let footerMsg = document.getElementById("WF_MESSAGE");
    // ********************************
    // ファイル名格納用リスト
    // ********************************
    let fileNameListObj = document.getElementById("WF_FILENAMELIST");
    let fileNameList = [];
    if (fileNameListObj !== null) {
        fileNameListObj.text = '';
    }
    // ********************************
    // メッセージの取得
    // ********************************
    let messageList = new Array(6);
    let stMsgObj = document.getElementById('hdnUploadMessage01');
    messageList[0] = '';
    if (stMsgObj !== null) {
        messageList[0] = stMsgObj.value;
    }
    for (let i = 1; i < 6; i++) {
        var tmpObj = document.getElementById('hdnUploadError0' + i);
        if (tmpObj !== null) {
            messageList[i] = tmpObj.value;
        } else {
            messageList[i] = '';
        }
    }
    footerMsg.textContent = messageList[0];
    footerMsg.removeAttribute("class");
    footerMsg.classList.add('INFORMATION');
    // ドラッグされたファイル情報を取得
    var files = e.dataTransfer.files;

    // 送信用FormData オブジェクトを用意
    var fd = new FormData();
    // 許可拡張子の正規表現文字生成
    let regString = "";
    if (acceptExtentions === null) {
        // acceptExtentionsがない場合は拡張子制限なし
        regString = "^.*$";
    } else {
        // 許可拡張子を元に正規表現文字を生成
        for (let i = 0; i < acceptExtentions.length; i++) {
            if (regString === '') {
                regString = '^.*\.' + acceptExtentions[i] + '$';
            } else {
                regString = regString + '|' + '^.*\.' + acceptExtentions[i] + '$';
            }
        }
    }
    // 正規表現オブジェクトの生成
    let reg = new RegExp(regString);

    for (let i = 0; i < files.length; i++) {
        if (files[i].name.toLowerCase().match(reg)) {
            fd.append("files", files[i]);
            fileNameList.push({ FileName: files[i].name });
        } else {
            footerMsg.textContent = "不許可ファイルの種類です。";
            footerMsg.removeAttribute("class");
            footerMsg.classList.add('ABNORMAL');
            commonHideWait();
            return;
        }
    }

    // XMLHttpRequest オブジェクトを作成
    let xhr = new XMLHttpRequest();

    // ドロップファイルによりURL変更
    // 「POST メソッド」「接続先 URL」を指定
    xhr.open("POST", handlerUrl, false);

    // イベント設定
    // ⇒XHR 送信正常で実行されるイベント
    xhr.onload = function (e) {
        if (e.currentTarget.status === 200) {
            let fileNameListObj = document.getElementById("WF_FILENAMELIST");
            document.forms[0].submit();                             //aspx起動
        } else {
            footerMsg.textContent = messageList[1];
            footerMsg.removeAttribute("class");
            footerMsg.classList.add('ABNORMAL');
            commonHideWait();
        }
    };

    // ⇒XHR 送信ERRで実行されるイベント
    xhr.onerror = function (e) {
        footerMsg.textContent = messageList[1];
        footerMsg.removeAttribute("class");
        footerMsg.classList.add('ABNORMAL');
        commonHideWait();
    };

    // ⇒XHR 通信中止すると実行されるイベント
    xhr.onabort = function (e) {
        footerMsg.textContent = messageList[2];
        footerMsg.removeAttribute("class");
        footerMsg.classList.add('ABNORMAL');
        commonHideWait();
    };

    // ⇒送信中にタイムアウトエラーが発生すると実行されるイベント
    xhr.ontimeout = function (e) {
        footerMsg.textContent = messageList[3];
        footerMsg.removeAttribute("class");
        footerMsg.classList.add('ABNORMAL');
        commonHideWait();
    };
    if (fileNameListObj !== null) {
        fileNameListObj.value = JSON.stringify(fileNameList);
    }
    // 「送信データ」を指定、XHR 通信を開始する
    xhr.send(fd);
}
/**
 * アップロードボタンイベント（ファイル選択後にD&Dと同じ動作をさせる）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function bindUploadButton() {
    let attachAreaObj = document.getElementById('divAttachmentArea');
    let uploadLineObj = document.getElementById('uploadLine');
    let btnUploadObj = document.getElementById('btnFileUpload');
    let fupObj = document.createElement('input');
    let fupAttachment = document.getElementById('fupAttachment');
    fupAttachment.multiple = 'multiple';
    fupObj.type = 'file';
    fupObj.style.display = 'none';
    fupObj.multiple = 'multiple';
    fupObj.id = 'fupUpload';
    uploadLineObj.appendChild(fupObj);
    fupObj = document.getElementById('fupUpload');


    btnUploadObj.onclick = (function (fupObj) {
        return function () {
            fupObj.click();
        };
    })(fupObj);

    fupObj.onchange = (function (dropBoxId, fupObj) {
        return function () {
            if (fupObj.files.length > 0) {
                var dropObj = document.getElementById(dropBoxId);
                if (dropObj !== null) {
                    //対象のドロップイベントを選択したファイルをもとに発火
                    // file = uploadFileObj.files[0];  
                    var rect = dropObj.getBoundingClientRect(),
                        x = rect.left + (rect.width >> 1),
                        y = rect.top + (rect.height >> 1);
                    var data = { files: fupObj.files };

                    ['dragenter', 'dragover', 'drop'].forEach(function (name) {
                        var event = document.createEvent('MouseEvent');
                        event.initMouseEvent(name, !0, !0, window, 0, 0, 0, x, y, !1, !1, !1, !1, 0, null);
                        event.dataTransfer = data;
                        dropObj.dispatchEvent(event);
                    });
                    fupObj.value = '';
                }
            }
        };
    })(attachAreaObj.id, fupObj);
}
/**
 * 削除ファイル名設定
 * @param {String} fileName 削除対象ファイル名
 * @return {undefined} なし
 * @description
 */
function setDeleteFileName(fileName) {
    let delFileNameObj = document.getElementById('WF_DELETEFILENAME');
    delFileNameObj.value = fileName;
}


