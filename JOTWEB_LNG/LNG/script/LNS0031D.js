// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //発駅コード
    const TxtBUDGETCLASS = document.getElementById('TxtBUDGETCLASS');
    const TxtBUDGETCLASScommonIcon = document.getElementById('TxtBUDGETCLASScommonIcon');
    const TxtBUDGETCLASSEvent = document.getElementById('TxtBUDGETCLASSEvent');

    //予算分類名称
    document.getElementById('TxtBUDGETCLASSNM').readOnly = true;

    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //発駅コード
        TxtBUDGETCLASS.readOnly = true;
        TxtBUDGETCLASScommonIcon.style.display = "none";
        TxtBUDGETCLASSEvent.disabled = "disabled";
        TxtBUDGETCLASSEvent.ondblclick = "";
    };
};


