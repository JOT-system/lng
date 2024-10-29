// 画面読み込み時処理
window.onload = function () {
    const DisabledKeyItem  = document.getElementById('DisabledKeyItem').value;

    const txtToriCode = document.getElementById('txtToriCode');
    const txtDepStation = document.getElementById('txtDepStation');

    const txtToriCodeEvent = document.getElementById('txtToriCodeEvent');
    const txtDepStationEvent = document.getElementById('txtDepStationEvent');
 
    const txtToriCodecommonIcon = document.getElementById('txtToriCodecommonIcon');
    const txtDepStationcommonIcon = document.getElementById('txtDepStationcommonIcon');

    //一意項目に値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {

        txtToriCode.readOnly = true;
        txtToriCodecommonIcon.style.display = "none";
        txtToriCodeEvent.disabled = "disabled";
        txtToriCodeEvent.ondblclick = "";

        txtDepStation.readOnly = true;
        txtDepStationcommonIcon.style.display = "none";
        txtDepStationEvent.disabled = "disabled";
        txtDepStationEvent.ondblclick = "";

    };
};


