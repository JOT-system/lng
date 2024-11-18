// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //発駅コード
    const TxtDepStation = document.getElementById('TxtDepStation');
    const TxtDepStationcommonIcon = document.getElementById('TxtDepStationcommonIcon');
    const TxtDepStationEvent = document.getElementById('TxtDepStationEvent');
    //発受託人コード
    const TxtDepTrusteeCd = document.getElementById('TxtDepTrusteeCd');
    const TxtDepTrusteeCdcommonIcon = document.getElementById('TxtDepTrusteeCdcommonIcon');
    const TxtDepTrusteeCdEvent = document.getElementById('TxtDepTrusteeCdEvent');
    //発受託人サブコード
    const TxtDepTrusteeSubCd = document.getElementById('TxtDepTrusteeSubCd');
    const TxtDepTrusteeSubCdcommonIcon = document.getElementById('TxtDepTrusteeSubCdcommonIcon');
    const TxtDepTrusteeSubCdEvent = document.getElementById('TxtDepTrusteeSubCdEvent');
    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //発駅コード
        TxtDepStation.readOnly = true;
        TxtDepStationcommonIcon.style.display = "none";
        TxtDepStationEvent.disabled = "disabled";
        TxtDepStationEvent.ondblclick = "";
        //発受託人コード
        TxtDepTrusteeCd.readOnly = true;
        TxtDepTrusteeCdcommonIcon.style.display = "none";
        TxtDepTrusteeCdEvent.disabled = "disabled";
        TxtDepTrusteeCdEvent.ondblclick = "";
        //発受託人サブコード
        TxtDepTrusteeSubCd.readOnly = true;
        TxtDepTrusteeSubCdcommonIcon.style.display = "none";
        TxtDepTrusteeSubCdEvent.disabled = "disabled";
        TxtDepTrusteeSubCdEvent.ondblclick = "";
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


