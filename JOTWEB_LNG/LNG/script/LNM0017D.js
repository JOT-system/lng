// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //組織コード
    const TxtOrgCode = document.getElementById('TxtOrgCode');
    const TxtOrgCodecommonIcon = document.getElementById('TxtOrgCodecommonIcon');
    const TxtOrgCodeEvent = document.getElementById('TxtOrgCodeEvent');
    //大分類コード
    const TxtBigCTNCD = document.getElementById('TxtBigCTNCD');
    const TxtBigCTNCDcommonIcon = document.getElementById('TxtBigCTNCDcommonIcon');
    const TxtBigCTNCDEvent = document.getElementById('TxtBigCTNCDEvent');
    //中分類コード
    const TxtMiddleCTNCD = document.getElementById('TxtMiddleCTNCD');
    const TxtMiddleCTNCDcommonIcon = document.getElementById('TxtMiddleCTNCDcommonIcon');
    const TxtMiddleCTNCDEvent = document.getElementById('TxtMiddleCTNCDEvent');
    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //組織コード
        TxtOrgCode.readOnly = true;
        TxtOrgCodecommonIcon.style.display = "none";
        TxtOrgCodeEvent.disabled = "disabled";
        TxtOrgCodeEvent.ondblclick = "";
        //大分類コード
        TxtBigCTNCD.readOnly = true;
        TxtBigCTNCDcommonIcon.style.display = "none";
        TxtBigCTNCDEvent.disabled = "disabled";
        TxtBigCTNCDEvent.ondblclick = "";
        //中分類コード
        TxtMiddleCTNCD.readOnly = true;
        TxtMiddleCTNCDcommonIcon.style.display = "none";
        TxtMiddleCTNCDEvent.disabled = "disabled";
        TxtMiddleCTNCDEvent.ondblclick = "";
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



