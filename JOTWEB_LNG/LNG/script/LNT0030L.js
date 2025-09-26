// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // テキストボックスEnter横移動イベントバインド(必ず使用有無変更「DispFormat」の後で行う)
    setTimeout(function () {
        // テキストボックスEnter横移動イベントバインド
        commonBindEnterToHorizontalTabStep();
    }, 100);

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

// ページのすべてのリソースが読み込まれた後に実行される
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);

    //オートコンプリートOFF
    autoCompleteOff()
    //画面入力項目制御
    inputItemCtrl()
    // チェックボックス
    ChangeCheckBox();
    //届先選択ボタン制御
    if (document.getElementById('WF_SURCHARGEPATTERNCODE').value == "01") {
        document.getElementById('WF_ButtonTODOKE').disabled = true;
    } else {
        document.getElementById('WF_ButtonTODOKE').disabled = false;
    }

    //必ず上記の、画面入力項目制御inputItemCtrl()の後に実行してね！
　　//※readonlyの設定を上記で行っているためその結果を処理したいため
    var targetTextBoxList = document.querySelectorAll("div[data-generated='1'] td[ondblclick] > input[type=text]");
    //対象のオブジェクトをループ
    for (let i = 0; i < targetTextBoxList.length; i++) {
        let inputObj = targetTextBoxList[i];
        let parentObj = inputObj.parentElement;

        // 対象オブジェクトが使用不可(または読み取り)の場合は
        // ダブルクリックをワークさせない
        let iconOnly = false;

        if (inputObj.disabled || (inputObj.readOnly && !inputObj.classList.contains('iconOnly'))) {
            parentObj.ondblclick = ""; /* 親要素のダブルクリックを排除 */
            inputObj.addEventListener('dblclick', function (e) {
                e.stopPropagation(); /* テキストボックスのダブルクリック伝達を抑止 */
            });
            inputObj.style.width = "100%";
            continue;
        }
    }
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

// ○テキストボックスのオートコンプリートをOFFにする
function autoCompleteOff() {
    var objDRTable = document.getElementById("pnlListArea_DR").children[0];

    for (var i = 0; i < objDRTable.rows.length; i++) {
        var row = objDRTable.rows[i];
        for (var j = 0; j < row.cells.length; j++) {
            var cell = row.cells[j];
            for (var k = 0; k < cell.childNodes.length; k++) {
                var child = cell.childNodes[k];
                if (child.tagName === "INPUT" && child.type === "text") {
                    child.setAttribute('autocomplete', 'off');
                }
            }
        }
    }
}
// ○画面入力項目制御（入力不可設定）
function inputItemCtrl() {
    var objDRTable = document.getElementById("pnlListArea_DR").children[0];

    for (var i = 0; i < objDRTable.rows.length; i++) {
        var j = i + 1;
        document.getElementById("txtpnlListAreaDIESELPRICECURRENT" + j).setAttribute('readonly', 'true');                   //実勢単価

        //サーチャージパターンの判定
        switch (document.getElementById("WF_SURCHARGEPATTERNCODE").value) {
            case '01':
                //荷主単位
                document.getElementById("txtpnlListAreaAVOCADOSHUKANAME" + j + "commonIcon").setAttribute('class', '');     //出荷場所（虫眼鏡）
                document.getElementById("txtpnlListAreaAVOCADOSHUKANAME" + j).setAttribute('readonly', 'true');             //出荷場所
                document.getElementById("txtpnlListAreaAVOCADOTODOKENAME" + j + "commonIcon").setAttribute('class', '');    //届先（虫眼鏡）
                document.getElementById("txtpnlListAreaAVOCADOTODOKENAME" + j).setAttribute('readonly', 'true');            //届先
                document.getElementById("lbSHAGATASHAGATA" + j).disabled = true;                                            //車型
                document.getElementById("txtpnlListAreaSHABARA" + j).setAttribute('readonly', 'true');                      //車腹
                document.getElementById("txtpnlListAreaSHABAN" + j).setAttribute('readonly', 'true');                       //車番
                break;
            case '02':
                //届先単位
                document.getElementById("lbSHAGATASHAGATA" + j).disabled = true;                                            //車型
                document.getElementById("txtpnlListAreaSHABARA" + j).setAttribute('readonly', 'true');                      //車腹
                document.getElementById("txtpnlListAreaSHABAN" + j).setAttribute('readonly', 'true');                       //車番
                break;
            case '03':
                //車型単位
                document.getElementById("txtpnlListAreaAVOCADOSHUKANAME" + j + "commonIcon").setAttribute('class', '');     //出荷場所（虫眼鏡）
                document.getElementById("txtpnlListAreaAVOCADOSHUKANAME" + j).setAttribute('readonly', 'true');             //出荷場所
                document.getElementById("txtpnlListAreaSHABARA" + j).setAttribute('readonly', 'true');                      //車腹
                document.getElementById("txtpnlListAreaSHABAN" + j).setAttribute('readonly', 'true');                       //車番
                break;
            case '04':
                //車腹単位
                document.getElementById("txtpnlListAreaAVOCADOSHUKANAME" + j + "commonIcon").setAttribute('class', '');     //出荷場所（虫眼鏡）
                document.getElementById("txtpnlListAreaAVOCADOSHUKANAME" + j).setAttribute('readonly', 'true');             //出荷場所
                document.getElementById("lbSHAGATASHAGATA" + j).disabled = true;                                            //車型
                document.getElementById("txtpnlListAreaSHABAN" + j).setAttribute('readonly', 'true');                       //車番
                break;
            case '05':
                //車番単位
                document.getElementById("txtpnlListAreaAVOCADOSHUKANAME" + j + "commonIcon").setAttribute('class', '');     //出荷場所（虫眼鏡）
                document.getElementById("txtpnlListAreaAVOCADOSHUKANAME" + j).setAttribute('readonly', 'true');             //出荷場所
                //document.getElementById("txtpnlListAreaAVOCADOTODOKENAME" + j + "commonIcon").setAttribute('class', '');    //届先（虫眼鏡）
                //document.getElementById("txtpnlListAreaAVOCADOTODOKENAME" + j).setAttribute('readonly', 'true');            //届先
                document.getElementById("lbSHAGATASHAGATA" + j).disabled = true;                                            //車型
                document.getElementById("txtpnlListAreaSHABARA" + j).setAttribute('readonly', 'true');                      //車腹
                break;
            default:
                break;
        }
    }
}

//行追加ボタン押下時
function BtnAddClick(EventName) {
    //var objDRTable = document.getElementById("pnlListArea_DR").children[0];
    //var findFlg = false

    ////追加行があるか否か判定する（ADDFLG="1"のレコードがあるか）
    //for (var i = 0; i < objDRTable.rows.length; i++) {
    //    var j = i + 1;
    //    if (document.getElementById("txtpnlListAreaADDFLG" + j).value == "1") {
    //        findFlg = true
    //    }
    //}
    //if (findFlg == true) {
    //    //追加行があれば、SUNMITしない
    //    return;
    //}

    document.getElementById("WF_ButtonClick").value = EventName;
    document.forms[0].submit();
}

// ○左Box用処理（左Box表示/非表示切り替え）
function ListField_DBclick(pnlList, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_GridDBclick').value = Line;
        document.getElementById('WF_FIELD').value = fieldNM;
        if (fieldNM === "SEIKYUDATEFROM" || fieldNM === "SEIKYUDATETO") {
            document.getElementById('WF_LeftMViewChange').value = "2";    //2:カレンダーを意味する
        } else {
            document.getElementById('WF_LeftMViewChange').value = "";
        }
        //カーソル位置を保管(カレンダーの表示位置)
        var elem = document.getElementById("txtpnlListArea" + fieldNM + Line);
        var rect = elem.getBoundingClientRect();
        document.getElementById("WF_saveTop").value = rect.top + rect.height;       //Y軸
        document.getElementById("WF_saveLeft").value = rect.left;　                 //X軸

        //カレンダーのサイズ
        var elemCal = document.getElementById("LF_LEFTBOX");
        var leftboxWidth = 360;
        var leftboxHeight = 307;

        //画面のサイズ
        var windowWidth = window.innerWidth;
        var windowHeight = window.innerHeight;

        // ポップアップ（カレンダー）が画面の右端を超える場合の調整
        if (rect.left + leftboxWidth > windowWidth) {
            document.getElementById("WF_saveLeft").value = windowWidth - leftboxWidth;
        }

        // ポップアップ（カレンダー）が画面の下端を超える場合の調整
        if (rect.top + rect.height + leftboxHeight > windowHeight) {
            document.getElementById("WF_saveTop").value = rect.top + rect.height - (leftboxHeight + rect.height);
        }

        document.getElementById('WF_LeftboxOpen').value = "Open";
        document.getElementById('WF_ButtonClick').value = "WF_Field_DBClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}
// ○一覧用処理（チェンジイベント）
function ListField_Change(pnlList, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_GridDBclick').value = Line;
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_ButtonClick').value = "WF_ListChange";
        document.forms[0].submit();
    }
}
// ○チェックボックス変更
function ChangeCheckBox() {

    var objTable = document.getElementById("pnlListArea_DL").children[0];

    var chkObjs = objTable.querySelectorAll("input[id^='chkpnlListAreaOPERATIONCB']");
    var spnObjs = objTable.querySelectorAll("span[id^='hchkpnlListAreaOPERATIONCB']");

    for (let i = 0; i < chkObjs.length; i++) {

        if (chkObjs[i] !== null) {
            if (spnObjs[i].innerText === "on") {
                chkObjs[i].checked = true;
            } else {
                chkObjs[i].checked = false;
            }
        }
    }
}

// ○チェックボックス選択
function SelectCheckBox(obj, lineCnt) {

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        let chkObj = obj.querySelector("input");
        if (chkObj === null) {
            return;
        }
        if (chkObj.disabled === true) {
            return;
        }

        document.getElementById("WF_SelectedIndex").value = lineCnt;
        document.getElementById("WF_ButtonClick").value = "WF_CheckBoxSELECT";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }

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
