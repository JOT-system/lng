// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* スプレッドシートのダブルクリックイベント紐づけ */
    //var spread = document.getElementById("FpSpread1");

    //if (spread.addEventListener) {
    //    spread.addEventListener("dblclick", DblClick, false);
    //} else {
    //    spread.ondblclick = DblClick;
    //}

}

// ページのすべてのリソースが読み込まれた後に実行される
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //表示判断キー
    const VisibleKeyOrgCode = document.getElementById('VisibleKeyOrgCode').value;

    //情シス、高圧ガス以外の場合
    if (VisibleKeyOrgCode == "") {
        //変更履歴を非表示にする
        document.getElementById('WF_ButtonHISTORY').style.display = "none";
    }

    //削除行灰色表示
    f_DeleteRowGray()

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);
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


// ○削除行を灰色表示
function f_DeleteRowGray() {
    var objTable = document.getElementById("pnlListArea_DR").children[0];
    var Col = {
        DELFLG: 6 //削除フラグ
    };

    for (var i = 0; i < objTable.rows.length; i++) {
        //削除行の場合
        if (objTable.rows[i].cells[Col.DELFLG].innerHTML == "1") {
            objTable.rows[i].style.backgroundColor = "gray";
        }
    }
}


