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
    //優先順位
    const TxtPriorityNo = document.getElementById('TxtPriorityNo');
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
        //優先順位
        TxtPriorityNo.readOnly = true;
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

