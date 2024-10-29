// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //荷主コード
    const TxtItemCd = document.getElementById('TxtItemCd');
    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //荷主コード
        TxtItemCd.readOnly = true;
    };
};


