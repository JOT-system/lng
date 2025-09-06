// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);

}

// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //削除行灰色表示
    f_DeleteRowGray()

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
            findFlg = true
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
