// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //発組織コード
    const TxtJRDepBranchCode = document.getElementById('TxtJRDepBranchCode');
    const TxtJRDepBranchCodecommonIcon = document.getElementById('TxtJRDepBranchCodecommonIcon');
    const TxtJRDepBranchCodeEvent = document.getElementById('TxtJRDepBranchCodeEvent');
    //着組織コード
    const TxtJRArrBranchCode = document.getElementById('TxtJRArrBranchCode');
    const TxtJRArrBranchCodecommonIcon = document.getElementById('TxtJRArrBranchCodecommonIcon');
    const TxtJRArrBranchCodeEvent = document.getElementById('TxtJRArrBranchCodeEvent');
    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //発組織コード
        TxtJRDepBranchCode.readOnly = true;
        TxtJRDepBranchCodecommonIcon.style.display = "none";
        TxtJRDepBranchCodeEvent.disabled = "disabled";
        TxtJRDepBranchCodeEvent.ondblclick = "";
        //着組織コード
        TxtJRArrBranchCode.readOnly = true;
        TxtJRArrBranchCodecommonIcon.style.display = "none";
        TxtJRArrBranchCodeEvent.disabled = "disabled";
        TxtJRArrBranchCodeEvent.ondblclick = "";
    };
};


