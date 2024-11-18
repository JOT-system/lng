// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //変動要因コード
    const TxtVARIABLEFACTORCODE = document.getElementById('TxtVARIABLEFACTORCODE');
    const TxtVARIABLEFACTORCODEcommonIcon = document.getElementById('TxtVARIABLEFACTORCODEcommonIcon');
    const TxtVARIABLEFACTORCODEEvent = document.getElementById('TxtVARIABLEFACTORCODEEvent');
    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //変動要因コード
        TxtVARIABLEFACTORCODE.readOnly = true;
        TxtVARIABLEFACTORCODEcommonIcon.style.display = "none";
        TxtVARIABLEFACTORCODEEvent.disabled = "disabled";
        TxtVARIABLEFACTORCODEEvent.ondblclick = "";
    };
};


