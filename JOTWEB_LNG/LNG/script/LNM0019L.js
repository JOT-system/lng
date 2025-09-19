// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);

    //　届先複数選択
    $(document).ready(function () {
        $("#contents1_WF_TODOKE").multiselect({
            menuHeight: 390,
            noneSelectedText: "選択してください",
            selectedText: "# 個選択",
            autoopen: false,
            multiple: true,
            buttonWidth: 330,

            position: {
                my: 'center',
                at: 'center'
            }
        });
    });

}

// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //表示判断キー
    const VisibleKeyOrgCode = document.getElementById('VisibleKeyOrgCode').value;

    //情シス、高圧ガス以外の場合、変更履歴、パンくず(検索)をを非表示にする
    if (VisibleKeyOrgCode == "") {
        document.getElementById('WF_ButtonHISTORY').style.display = "none";
    }

    //削除行灰色表示
    f_DeleteRowGray()

    //ヘッダコメント編集
    f_CommentCtrlCol();

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
            monthSelectorType: 'static',
            //defaultDate: new Date() // 必要に応じてカスタマイズ
        });
    });
});

// ○削除行を灰色表示
function f_DeleteRowGray() {
    var objDL = document.getElementById("pnlListArea_DL").children[0];
    var objDR = document.getElementById("pnlListArea_DR").children[0];
    var Col = {
        DELFLG: 15 //削除フラグ
    };

    for (var i = 0; i < objDR.rows.length; i++) {
        //削除行の場合
        if (objDR.rows[i].cells[Col.DELFLG].innerHTML == "1") {
            objDL.rows[i].style.backgroundColor = "gray";
            objDR.rows[i].style.backgroundColor = "gray";
        }
    }
}

// ○ヘッダコメント編集
function f_CommentCtrlCol() {
    var Col = {
        SURCHARGEPATTERN: 2         //サーチャージパターン
        , CALCMETHOD: 4             //距離計算方式
        , DIESELPRICESITE: 5        //実績単価参照先
        , DIESELPRICEROUNDLEN: 6    //実勢単価端数処理（桁数）
    };

    //**********ヘッダ********************/
    //1つの列以外を非表示にして表示中の列幅を伸ばす
    var objTableHR = document.getElementById("pnlListArea_HR").children[0];

    //ID追加
    objTableHR.rows[0].cells[Col.SURCHARGEPATTERN].id = "surchargepattern";
    objTableHR.rows[0].cells[Col.CALCMETHOD].id = "calcmethod";
    objTableHR.rows[0].cells[Col.DIESELPRICESITE].id = "dieselpreicesite";
    objTableHR.rows[0].cells[Col.DIESELPRICEROUNDLEN].id = "dieselpreicesite";
    //タイトル(吹き出し)
    objTableHR.rows[0].cells[Col.SURCHARGEPATTERN].title = "【サーチャージパターンについて】\n" +
        "・荷主単位：車両や届先に関わらず、荷主単位でサーチャージ料金が定義されている場合を指します。\n" +
        "・届先単位：届先毎に輸送距離や基準単価等が定義されている場合を指します。\n" +
        "・車型単位：車型によって輸送距離や基準単価等が定義されている場合を指します。\n" +
        "・車腹単位：車腹によって輸送距離や基準単価等が定義されている場合を指します。\n" +
        "・車番単位：車番によって輸送距離や基準単価等が定義されている場合を指します。";
    //タイトル(吹き出し)
    objTableHR.rows[0].cells[Col.CALCMETHOD].title = "【距離計算方式について】\n" +
        "・距離定義による計算：\n" +
        "　協定書等で距離程が定義されており、輸送回数に応じて自動計算が行われるデータです。\n" +
        "・距離は実績値を画面に入力：\n" +
        "　実勢単価参照先が新たに生じた場合は、軽油価格参照先管理マスタで事前登録を行ってください。";
    //タイトル(吹き出し)
    objTableHR.rows[0].cells[Col.DIESELPRICESITE].title = "【実勢価格参照先について】\n" +
        "サーチャージ定義毎の、実勢単価の参照先名称を表示します。\n" +
        "実勢単価参照先が新たに生じた場合は、軽油価格参照先管理マスタで事前登録を行ってください。";
    //タイトル(吹き出し)
    objTableHR.rows[0].cells[Col.DIESELPRICEROUNDLEN].title = "【実勢単価端数処理について】\n" +
        "「小数点以下は、端数処理を行う桁数です。\n" +
        "第3位の場合、第3位に対して端数処理を行い、X.XXの桁数を意味します。";

}

//料金設定ボタン押下時
function BtnSurchargeFeeClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonFeeClick";
    document.forms[0].submit();
}

//実勢単価ボタン押下時
function BtnTankaClick(obj, lineCnt, fieldNM) {
    document.getElementById("WF_SelectedIndex").value = lineCnt
    document.getElementById("WF_ButtonClick").value = "WF_ButtonTankaClick";
    document.forms[0].submit();
}
