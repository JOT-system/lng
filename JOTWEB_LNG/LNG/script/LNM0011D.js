// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //発駅コード
    const TxtDepStation = document.getElementById('TxtDepStation');
    const TxtDepStationcommonIcon = document.getElementById('TxtDepStationcommonIcon');
    const TxtDepStationEvent = document.getElementById('TxtDepStationEvent');
    //着駅コード
    const TxtArrStation = document.getElementById('TxtArrStation');
    const TxtArrStationcommonIcon = document.getElementById('TxtArrStationcommonIcon');
    const TxtArrStationEvent = document.getElementById('TxtArrStationEvent');
    //摘要年月日
    const TxtFromYmd = document.getElementById('TxtFromYmd');
    const TxtFromYmdcommonIcon = document.getElementById('TxtFromYmdcommonIcon');
    const TxtFromYmdEvent = document.getElementById('TxtFromYmdEvent');
    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //発駅コード
        TxtDepStation.readOnly = true;
        TxtDepStationcommonIcon.style.display = "none";
        TxtDepStationEvent.disabled = "disabled";
        TxtDepStationEvent.ondblclick = "";
        //着駅コード
        TxtArrStation.readOnly = true;
        TxtArrStationcommonIcon.style.display = "none";
        TxtArrStationEvent.disabled = "disabled";
        TxtArrStationEvent.ondblclick = "";
        //摘要年月日
        TxtFromYmd.readOnly = true;
        TxtFromYmd.style.backgroundColor = "#ECECEC"
        TxtFromYmdcommonIcon.style.display = "none";
        TxtFromYmdEvent.disabled = "disabled";
        TxtFromYmdEvent.ondblclick = "";
    };
};


