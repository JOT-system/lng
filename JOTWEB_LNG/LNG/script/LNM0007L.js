// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* スプレッドシートのダブルクリックイベント紐づけ */
    //var spread = document.getElementById("FpSpread1");

    //if (spread.addEventListener) {
    //    spread.addEventListener("dblclick", DblClick, false);
    //} else {
    //    spread.ondblclick = DblClick;
    //}

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);

}

// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //表示判断キー
    const VisibleKeyOrgCode = document.getElementById('VisibleKeyOrgCode').value;
    const VisibleKeyTohokuOrgCode = document.getElementById('VisibleKeyTohokuOrgCode').value;

    //情シス、高圧ガス以外の場合
    if (VisibleKeyOrgCode == "") {
        //変更履歴を非表示にする
        document.getElementById('WF_ButtonHISTORY').style.display = "none";
            //東北支店以外の場合
        if (VisibleKeyTohokuOrgCode == "") {
            //SK固定費タブ、TNG固定費タブを非表示にする
            document.getElementById('WF_ButtonTNGKOTEIHI').style.display = "none";
            document.getElementById('WF_ButtonSKKOTEIHI').style.display = "none";
        }
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

//document.addEventListener("DOMContentLoaded", function () {
//    // カレンダー表示
//    document.querySelectorAll('.datetimepicker').forEach(picker => {
//        flatpickr(picker, {
//            wrap: true,
//            dateFormat: 'Y/m/d',
//            locale: 'ja',
//            clickOpens: false,
//            allowInput: true,
//            monthSelectorType: 'static',
//            //defaultDate: new Date() // 必要に応じてカスタマイズ
//        });
//    });
//});

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
 * タブボタン押下時処理
 */
//function f_TabBtnClick() {
//    var VisibleKeyControlTable = document.getElementById('VisibleKeyControlTable').value;
//    var WF_ButtonKOTEIHI = document.getElementById('WF_ButtonKOTEIHI'); //固定費ボタン
//    var WF_ButtonSKKOTEIHI = document.getElementById('WF_ButtonSKKOTEIHI'); //TNG固定費ボタン
//    var WF_ButtonTNGKOTEIHI = document.getElementById('WF_ButtonTNGKOTEIHI'); //SK固定費ボタン

//    WF_ButtonKOTEIHI.style.background = "#D9D9D9";
//    WF_ButtonSKKOTEIHI.style.background = "#D9D9D9";
//    WF_ButtonTNGKOTEIHI.style.background = "#D9D9D9";

//    switch (VisibleKeyControlTable) {
//        case "LNM0007L": //固定費マスタ
//            WF_ButtonKOTEIHI.style.background = "#FFFFFF";
//            break;
//        case "LNM0007LSK": //SK固定費マスタ
//            WF_ButtonSKKOTEIHI.style.background = "#FFFFFF";
//            break;
//        case "LNM0007LTNG": //TNG固定費マスタ
//            WF_ButtonTNGKOTEIHI.style.background = "#FFFFFF";
//            break;
//    }
//}
