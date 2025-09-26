// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // テキストボックスEnter縦移動イベントバインド(必ず使用有無変更「DispFormat」の後で行う)
    setTimeout(function () {
        // テキストボックスEnter横移動イベントバインド
        commonBindEnterToHorizontalTabStep();
    }, 100);

}

// ページのすべてのリソースが読み込まれた後に実行される
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //削除行灰色表示
    f_DeleteRowGray()

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);
}

/**
 * 最上部・最下部ボタン押下処理
 */
function SetCellActive(mode) {
    var row = 0;
    var col = 0;

    //最下部押下時、最終行をrowにセット
    if (mode == "B") {
        var row = parseInt(hidRowCount.value) - 1;
    }

    var spread = document.all("FpSpread1");
    spread.SetActiveCell(row, col);

    var cell = spread.GetCellByRowCol(row, col);
    var rowHeader = spread.all(spread.id + "_rowHeader");
    var colHeader = spread.all(spread.id + "_colHeader");
    var view = spread.all(spread.id + "_view");

    if (view == null)
        return;

    view.scrollTop = cell.offsetTop;
    view.scrollLeft = cell.offsetLeft;

    if (rowHeader != null) {
        rowHeader.scrollTop = view.scrollTop;
    }
    if (colHeader != null) {
        colHeader.scrollLeft = view.scrollLeft;
    }
}

// ○ロック行を灰色表示
function f_DeleteRowGray() {
    var objDRTable = document.getElementById("pnlListArea_DR").children[0];
    //var Col = {
    //    LOCKFLG: 13 //ロックフラグ
    //};

    for (var i = 0; i < objDRTable.rows.length; i++) {
        //ロック行の場合
        var j = i + 1;
        if (document.getElementById("txtpnlListAreaLOCKFLG" + j).value == "1") {
            objDRTable.rows[i].style.backgroundColor = "gray";

            document.getElementById("txtpnlListAreaTARGETYEAR" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE1" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE2" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE3" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE4" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE5" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE6" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE7" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE8" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE9" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE10" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE11" + j).disabled = true;
            document.getElementById("txtpnlListAreaDIESELPRICE12" + j).disabled = true;
        }
        //オートコンプリートを無効にする
        document.getElementById("txtpnlListAreaTARGETYEAR" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE1" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE2" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE3" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE4" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE5" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE6" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE7" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE8" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE9" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE10" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE11" + j).setAttribute('autocomplete', 'off');
        document.getElementById("txtpnlListAreaDIESELPRICE12" + j).setAttribute('autocomplete', 'off');
    }
}


//行追加ボタン押下時
function BtnAddClick(EventName) {
    var objDRTable = document.getElementById("pnlListArea_DR").children[0];
    var findFlg = false

    //追加行があるか否か判定する（ADDFLG="1"のレコードがあるか）
    for (var i = 0; i < objDRTable.rows.length; i++) {
        var j = i + 1;
        if (document.getElementById("txtpnlListAreaADDFLG" + j).value == "1") {
            if (document.getElementById("txtpnlListAreaTARGETYEAR" + j).value == "") {
                findFlg = true
            } else {
                //追加行に入力（対象年）がある場合、追加フラグをリセット"0"
                document.getElementById("txtpnlListAreaADDFLG" + j).value = "0"
            }
        }
    }
    if (findFlg == true) {
        //追加行があれば、SUNMITしない
        return;
    }
    document.getElementById("WF_ButtonClick").value = EventName;
    document.forms[0].submit();
}

//ロックボタン押下時
function BtnLockClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    if (document.getElementById("btnLock" + lineCnt).outerHTML.indexOf('unlockkey') > -1)  {
        //非ロックの場合、ロック
        document.getElementById("WF_ButtonClick").value = "WF_ButtonLockClick";
    } else {
        //ロックの場合、非ロック
        document.getElementById("WF_ButtonClick").value = "WF_ButtonUnLockClick";
    }

    //追加データのロックボタンの場合、強制的に更新処理に上書しデータチェック＆更新処理とする
    if (document.getElementById("txtpnlListAreaADDFLG" + lineCnt).value == "1") {
        //更新処理を行う
        document.getElementById("WF_ButtonClick").value = "WF_ButtonUPDATE";
    }

    document.forms[0].submit();
}
//削除ボタン押下時
function BtnDelClick(obj, lineCnt, fieldNM) {

    if (document.getElementById("txtpnlListAreaLOCKFLG" + lineCnt).value == "1") {
        return;
    }
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonDelClick";
    document.forms[0].submit();
}
var commonKeyEnterProgress = false; // これは関数(function)外部に設定(グローバルスコープの変数です)

/**
 *  リストテーブルのEnterキーで横のテキストにタブを移すイベントバインド
 * @return {undefined} なし
 * @description 
 */
function commonBindEnterToHorizontalTabStep() {
    let generatedTables = document.querySelectorAll("div[data-generated='1']");
    if (generatedTables === null) {
        return;
    }
    if (generatedTables.length === 0) {
        return;
    }
    let focusObjKey = document.forms[0].id + "ListFocusObjId";
    if (sessionStorage.getItem(focusObjKey) !== null) {
        if (IsPostBack === undefined) {
            sessionStorage.removeItem(focusObjKey);
        }
        if (IsPostBack === '1') {
            focusObjId = sessionStorage.getItem(focusObjKey);
            setTimeout(function () {
                document.getElementById(focusObjId).focus();
                sessionStorage.removeItem(focusObjKey);
            }, 10);
        } else {
            sessionStorage.removeItem(focusObjKey);
        }

    }
    for (let i = 0, len = generatedTables.length; i < len; ++i) {
        let generatedTable = generatedTables[i];
        let panelId = generatedTable.id;
        //生成したテーブルオブジェクトのテキストボックス確認
        let textBoxes = generatedTable.querySelectorAll('input[type=text]:not([disabled]):not([disabled=""])');
        //テキストボックスが無ければ次の描画されたリストテーブルへ
        if (textBoxes === null) {
            continue;
        }

        // テキストボックスのループ
        for (let j = 0; j < textBoxes.length; j++) {
            let textBox = textBoxes[j];
            let lineCnt = textBox.attributes.getNamedItem("rownum").value;
            let fieldName = textBox.id.substring(("txt" + panelId).length);
            fieldName = fieldName.substring(0, fieldName.length - lineCnt.length);
            let nextTextFieldName = fieldName;
            if (textBoxes.length === j + 1) {
                // 最後のテキストボックスは先頭のフィールド
                nextTextFieldName = textBoxes[0].id.substring(("txt" + panelId).length);
                nextTextFieldName = nextTextFieldName.substring(0, nextTextFieldName.length - lineCnt.length);
            } else if (textBoxes.length > j + 1) {
                nextTextFieldName = textBoxes[j + 1].id.substring(("txt" + panelId).length);
                nextTextFieldName = nextTextFieldName.substring(0, nextTextFieldName.length - lineCnt.length);
            }

            textBox.dataset.fieldName = fieldName;
            textBox.dataset.nextTextFieldName = nextTextFieldName;
            textBox.addEventListener('keypress', (function (textBox, panelId) {
                return function () {
                    if (event.key === 'Enter') {
                        if (commonKeyEnterProgress === false) {
                            commonKeyEnterProgress = true; //Enter連打抑止
                            commonListEnterToHorizontalTabStep(textBox, panelId);
                            return setTimeout(function () {
                                commonKeyEnterProgress = false;　///Enter連打抑止
                            }, 10); // 5ミリ秒だと連打でフォーカスパニックになったので10ミリ秒に
                        }
                    }
                };
            })(textBox, panelId), true);
        }
    }
}
/**
 *  リストテーブルのEnterキーで横のテキストにタブを移すイベント
 * @param {Node} textBox テキストボックス
 * @param {string} panelId テキストボックス
 * @return {undefined} なし
 * @description 
 */
function commonListEnterToHorizontalTabStep(textBox, panelId) {
    let curLineCnt = Number(textBox.attributes.getNamedItem("rownum").value);
    let fieldName = textBox.dataset.fieldName;
    let nextTextFieldName = textBox.dataset.nextTextFieldName;
    let found = false;
    let focusNode;
    let maxLineCnt = 999; // 無限ループ抑止用の最大LineCntインクリメント
    let targetObjPrefix = "txt" + panelId + nextTextFieldName;
    while (found === false) {
        //curLineCnt = curLineCnt + 1;
        let targetObj = targetObjPrefix + curLineCnt;
        focusNode = document.getElementById(targetObj);
        if (focusNode !== null) {
            found = true;
        } else {
            curLineCnt = curLineCnt + 1;

            targetObjPrefix = "txt" + panelId + nextTextFieldName;
        }

        // 無限ループ抑止
        if (maxLineCnt === curLineCnt) {
            found = true;
        }
    }
    //onchangeイベント（postbackする）を見つけてセッション変数にフォーカス先を保持する（load時にセッション変数からフォーカス先を取得させる）
    //注意）T9では、trタグでonchangeしているため（1行毎、全てのテキスト）判定を止める！！
    //      T9以外で利用する場合、対応が必要かも？
    //var parentNodeObj = textBox.parentNode;
    //if (parentNodeObj.hasAttribute('onchange')) {

    var focusObjKey = document.forms[0].id + "ListFocusObjId";
    sessionStorage.setItem(focusObjKey, focusNode.id);
    //}
    //var retValue = sessionStorage.getItem(forcusObjKey);
    //if (retValue === null) {
    //    retValue = '';
    //}
    focusNode.focus();
    return;
}