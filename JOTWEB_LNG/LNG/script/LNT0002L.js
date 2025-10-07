// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

}

// ページのすべてのリソースが読み込まれた後に実行される
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //表示判断キー
    const VisibleKeyOrgCode = document.getElementById('VisibleKeyOrgCode').value;

    //請求調整列編集
    f_InvoiceCtrlCol();

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
    commonDispWait();
    document.forms[0].submit();
}

//出力ボタン押下時
function BtnOutputClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonOutClick";
    commonDispWait();
    document.forms[0].submit();
}

//出力（共通）ボタン押下時
function BtnComOutputClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonComOutClick";
    commonDispWait();
    document.forms[0].submit();
}

//参照ボタン押下時
function BtnReferenceClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonRefClick";
    commonDispWait();
    document.forms[0].submit();
}

//履歴画面の出力ボタン押下時
function BtnHistOutputClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonHistOutClick";
    commonDispWait();
    document.forms[0].submit();
}

// ○請求調整列編集
function f_InvoiceCtrlCol() {
    var Col = {
        SPRATE: 4 //特別料金列
        , TANKA: 5 //単価調整列
        , KOTEIHI: 6 //固定費調整列
        , SURCHARGE: 7 //サーチャージ列
    };

    //**********ヘッダ********************/
    //1つの列以外を非表示にして表示中の列幅を伸ばす
    var objTableHR = document.getElementById("pnlListArea_HR").children[0];

    objTableHR.rows[0].cells[Col.TANKA].style.display = "none";
    objTableHR.rows[0].cells[Col.KOTEIHI].style.display = "none";
    objTableHR.rows[0].cells[Col.SURCHARGE].style.display = "none";

    var objNewWidth = Number(objTableHR.rows[0].cells[Col.SPRATE].style.width.replace("px","")) +
        Number(objTableHR.rows[0].cells[Col.TANKA].style.width.replace("px", "")) +
        Number(objTableHR.rows[0].cells[Col.KOTEIHI].style.width.replace("px", "")) +
        Number(objTableHR.rows[0].cells[Col.SURCHARGE].style.width.replace("px", "")) + "px";

    objTableHR.rows[0].cells[Col.SPRATE].style.width = objNewWidth
   
    //表示名称
    objTableHR.rows[0].cells[Col.SPRATE].innerHTML = "　請求調整対象";
    //ID追加
    objTableHR.rows[0].cells[Col.SPRATE].id = "seikyuchosei";
    //タイトル(吹き出し)
    objTableHR.rows[0].cells[Col.SPRATE].title = "入力が行われている場合、アイコンの色が変わります。\n" +
        "[凡例]\n" +
        "特・・・特別料金\n" +
        "単・・・単価調整\n" +
        "固・・・固定費調整\n" +
        "サ・・・サーチャージ"; 

    //**********データ行********************/
    var objTableDR = document.getElementById("pnlListArea_DR").children[0];
    for (var i = 0; i < objTableDR.rows.length; i++) {

        //特別料金
        if (objTableDR.rows[i].cells[Col.SPRATE].innerHTML != "") {
            objTableDR.rows[i].cells[Col.SPRATE].style.backgroundColor = "#44B3E1";
            objTableDR.rows[i].cells[Col.SPRATE].style.color = "#FFFFFF";
        } 

        //単価調整
        if (objTableDR.rows[i].cells[Col.TANKA].innerHTML != "") {
            objTableDR.rows[i].cells[Col.TANKA].style.backgroundColor = "#44B3E1";
            objTableDR.rows[i].cells[Col.TANKA].style.color = "#FFFFFF";
        } 

        //固定費調整
        if (objTableDR.rows[i].cells[Col.KOTEIHI].innerHTML != "") {
            objTableDR.rows[i].cells[Col.KOTEIHI].style.backgroundColor = "#44B3E1";
            objTableDR.rows[i].cells[Col.KOTEIHI].style.color = "#FFFFFF";
        } 

        //サーチャージ
        if (objTableDR.rows[i].cells[Col.SURCHARGE].innerHTML != "") {
            objTableDR.rows[i].cells[Col.SURCHARGE].style.backgroundColor = "#44B3E1";
            objTableDR.rows[i].cells[Col.SURCHARGE].style.color = "#FFFFFF";
        } 

        objTableDR.rows[i].cells[Col.SPRATE].innerHTML = "特";
        objTableDR.rows[i].cells[Col.SPRATE].title = "";

        objTableDR.rows[i].cells[Col.TANKA].innerHTML = "単";
        objTableDR.rows[i].cells[Col.TANKA].title = "";

        objTableDR.rows[i].cells[Col.KOTEIHI].innerHTML = "固";
        objTableDR.rows[i].cells[Col.KOTEIHI].title = "";

        objTableDR.rows[i].cells[Col.SURCHARGE].innerHTML = "サ";
        objTableDR.rows[i].cells[Col.SURCHARGE].title = "";

    }
}