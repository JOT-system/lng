// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);

}

// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //表示判断キー
    const VisibleKeyOrgCode = document.getElementById('VisibleKeyOrgCode').value;

    //情シス、高圧ガス以外の場合
    if (VisibleKeyOrgCode == "") {
        document.getElementById('WF_ButtonHISTORY').style.display = "none";
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

document.addEventListener("DOMContentLoaded", function () {
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
});

/**
 * ロード時処理
 */
window.addEventListener('load', function () {
    let ele = document.getElementById('pnlHISTWrapper');
    ele.style.visibility = document.getElementById('WF_HIST').value;
});

//追加ボタン押下時
function BtnAddClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonAddClick";
    document.forms[0].submit();
}

//出力ボタン押下時
function BtnOutputClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonOutClick";
    document.forms[0].submit();
}

//参照ボタン押下時
function BtnReferenceClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonRefClick";
    document.forms[0].submit();
}

//履歴画面の出力ボタン押下時
function BtnHistOutputClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonHistOutClick";
    document.forms[0].submit();
}