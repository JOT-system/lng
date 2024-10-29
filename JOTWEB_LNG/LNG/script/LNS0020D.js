// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //駅コード
    const TxtSTATION = document.getElementById('TxtSTATION');
    const TxtSTATIONcommonIcon = document.getElementById('TxtSTATIONcommonIcon');
    const TxtSTATIONEvent = document.getElementById('TxtSTATIONEvent');

    //変換前組織コード
    document.getElementById('TxtBEFOREORGCODE').readOnly = true;

    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //駅コード
        TxtSTATION.readOnly = true;
        TxtSTATIONcommonIcon.style.display = "none";
        TxtSTATIONEvent.disabled = "disabled";
        TxtSTATIONEvent.ondblclick = "";
    }
}

function InitDisplay() {

    /* スクロール位置復元 */
    let panel = document.getElementById("divContensbox");
    if (panel != null) {
        let top = Number(document.getElementById("WF_scrollY").value);
        panel.scrollTo(0, top);
    }
}

function saveTabScrollPosition() {
    let panel = document.getElementById("divContensbox");
    if (panel != null) {
        document.getElementById("WF_scrollY").value = panel.scrollTop;
    }
}

window.addEventListener("DOMContentLoaded", () => {

    /* scrollイベント発生時に表示タブのスクロール位置を保存する処理を追加 */
    var panel = document.getElementById("divContensbox");
    if (panel != null) {
        panel.addEventListener('scroll', saveTabScrollPosition);
    }
});



