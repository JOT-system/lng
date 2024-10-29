// 画面読み込み時処理
window.onload = function () {

    //月選択バインド
    commonBindMonthPicker();
};

// ○SPREAD用クリック処理
function Spred_ButtonSel_click() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_SPREAD_ButtonSel";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}
