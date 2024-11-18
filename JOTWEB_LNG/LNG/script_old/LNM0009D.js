// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //キロ程
    const TxtKiro = document.getElementById('TxtKiro');
    const TxtKirocommonIcon = document.getElementById('TxtKirocommonIcon');
    const TxtKiroEvent = document.getElementById('TxtKiroEvent');
    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //キロ程
        TxtKiro.readOnly = true;
        TxtKirocommonIcon.style.display = "none";
        TxtKiroEvent.disabled = "disabled";
        TxtKiroEvent.ondblclick = "";
    };
};


