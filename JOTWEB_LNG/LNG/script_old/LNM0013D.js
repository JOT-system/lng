// 画面読み込み時処理
window.onload = function () {
    const DisabledKeyItem  = document.getElementById('DisabledKeyItem').value;

    const TxtBigCTNCD = document.getElementById('TxtBigCTNCD');
    const TxtMiddleCTNCD = document.getElementById('TxtMiddleCTNCD');
    const TxtPriorityNO = document.getElementById('TxtPriorityNO');
    const TxtDepstation = document.getElementById('TxtDepstation');
    const TxtJrDepBranchCD = document.getElementById('TxtJrDepBranchCD');
    const TxtArrstation = document.getElementById('TxtArrstation');
    const TxtJrArrBranchCD = document.getElementById('TxtJrArrBranchCD');

    const TxtBigCTNCDEvent = document.getElementById('TxtBigCTNCDEvent');
    const TxtMiddleCTNCDEvent = document.getElementById('TxtMiddleCTNCDEvent');
    const TxtPriorityNOEvent = document.getElementById('TxtPriorityNOEvent');
    const TxtDepstationEvent = document.getElementById('TxtDepstationEvent');
    const TxtJrDepBranchCDEvent = document.getElementById('TxtJrDepBranchCDEvent');
    const TxtArrstationEvent = document.getElementById('TxtArrstationEvent');
    const TxtJrArrBranchCDEvent = document.getElementById('TxtJrArrBranchCDEvent');

    const TxtBigCTNCDcommonIcon = document.getElementById('TxtBigCTNCDcommonIcon');
    const TxtMiddleCTNCDcommonIcon = document.getElementById('TxtMiddleCTNCDcommonIcon');
    const TxtPriorityNOcommonIcon = document.getElementById('TxtPriorityNOcommonIcon');
    const TxtDepstationcommonIcon = document.getElementById('TxtDepstationcommonIcon');
    const TxtJrDepBranchCDcommonIcon = document.getElementById('TxtJrDepBranchCDcommonIcon');
    const TxtArrstationcommonIcon = document.getElementById('TxtArrstationcommonIcon');
    const TxtJrArrBranchCDcommonIcon = document.getElementById('TxtJrArrBranchCDcommonIcon');
    //一意項目に値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        TxtBigCTNCD.readOnly = true;
        TxtBigCTNCDcommonIcon.style.display = "none";
        TxtBigCTNCDEvent.disabled = "disabled";
        TxtBigCTNCDEvent.ondblclick = "";

        TxtMiddleCTNCD.readOnly = true;
        TxtMiddleCTNCDcommonIcon.style.display = "none";
        TxtMiddleCTNCDEvent.disabled = "disabled";
        TxtMiddleCTNCDEvent.ondblclick = "";

        TxtPriorityNO.readOnly = true;

        TxtDepstation.readOnly = true;
        TxtDepstationcommonIcon.style.display = "none";
        TxtDepstationEvent.disabled = "disabled";
        TxtDepstationEvent.ondblclick = "";

        TxtJrDepBranchCD.readOnly = true;
        TxtJrDepBranchCDcommonIcon.style.display = "none";
        TxtJrDepBranchCDEvent.disabled = "disabled";
        TxtJrDepBranchCDEvent.ondblclick = "";

        TxtArrstation.readOnly = true;
        TxtArrstationcommonIcon.style.display = "none";
        TxtArrstationEvent.disabled = "disabled";
        TxtArrstationEvent.ondblclick = "";

        TxtJrArrBranchCD.readOnly = true;
        TxtJrArrBranchCDcommonIcon.style.display = "none";
        TxtJrArrBranchCDEvent.disabled = "disabled";
        TxtJrArrBranchCDEvent.ondblclick = "";
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


