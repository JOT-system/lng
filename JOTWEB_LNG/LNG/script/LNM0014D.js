// 画面読み込み時処理
window.onload = function () {
    const DisabledKeyItem  = document.getElementById('DisabledKeyItem').value;

    const txtBigCtnCd = document.getElementById('txtBigCtnCd');
    const txtMiddleCtnCd = document.getElementById('txtMiddleCtnCd');
    const txtDepStation = document.getElementById('txtDepStation');
    const txtDepTrusteeCd = document.getElementById('txtDepTrusteeCd');
    const txtDepTrusteeSubCd = document.getElementById('txtDepTrusteeSubCd');
    const txtPriorityNo = document.getElementById('txtPriorityNo');

    const txtBigCtnCdEvent = document.getElementById('txtBigCtnCdEvent');
    const txtMiddleCtnCdEvent = document.getElementById('txtMiddleCtnCdEvent');
    const txtDepStationEvent = document.getElementById('txtDepStationEvent');
    const txtDepTrusteeCdEvent = document.getElementById('txtDepTrusteeCdEvent');
    const txtDepTrusteeSubCdEvent = document.getElementById('txtDepTrusteeSubCdEvent');

    const txtBigCtnCdcommonIcon = document.getElementById('txtBigCtnCdcommonIcon');
    const txtMiddleCtnCdcommonIcon = document.getElementById('txtMiddleCtnCdcommonIcon');
    const txtDepStationcommonIcon = document.getElementById('txtDepStationcommonIcon');
    const txtDepTrusteeCdcommonIcon = document.getElementById('txtDepTrusteeCdcommonIcon');
    const txtDepTrusteeSubCdcommonIcon = document.getElementById('txtDepTrusteeSubCdcommonIcon');

    //一意項目に値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {

        txtBigCtnCd.readOnly = true;
        txtBigCtnCdcommonIcon.style.display = "none";
        txtBigCtnCdEvent.disabled = "disabled";
        txtBigCtnCdEvent.ondblclick = "";

        txtMiddleCtnCd.readOnly = true;
        txtMiddleCtnCdcommonIcon.style.display = "none";
        txtMiddleCtnCdEvent.disabled = "disabled";
        txtMiddleCtnCdEvent.ondblclick = "";

        txtDepStation.readOnly = true;
        txtDepStationcommonIcon.style.display = "none";
        txtDepStationEvent.disabled = "disabled";
        txtDepStationEvent.ondblclick = "";

        txtDepTrusteeCd.readOnly = true;
        txtDepTrusteeCdcommonIcon.style.display = "none";
        txtDepTrusteeCdEvent.disabled = "disabled";
        txtDepTrusteeCdEvent.ondblclick = "";

        txtDepTrusteeSubCd.readOnly = true;
        txtDepTrusteeSubCdcommonIcon.style.display = "none";
        txtDepTrusteeSubCdEvent.disabled = "disabled";
        txtDepTrusteeSubCdEvent.ondblclick = "";

        txtPriorityNo.readOnly = true;

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


