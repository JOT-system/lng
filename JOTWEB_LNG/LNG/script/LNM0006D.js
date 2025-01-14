// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;

    //入力不可キー
    document.getElementById('TxtSelLineCNT').readOnly = true; 
    document.getElementById('TxtCampCode').readOnly = true;

    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") { 
        //取引先コード
        document.getElementById('TxtTORICODE').readOnly = true;
        document.getElementById('TxtTORICODEcommonIcon').style.display = "none";
        //取引先名称
        document.getElementById('TxtTORINAME').readOnly = true;
        //部門コード
        document.getElementById('TxtORGCODE').readOnly = true;
        document.getElementById('TxtORGCODEcommonIcon').style.display = "none";
        //部門名称
        document.getElementById('TxtORGNAME').readOnly = true;
        //加算先部門コード
        document.getElementById('TxtKASANORGCODE').readOnly = true;
        document.getElementById('TxtKASANORGCODEcommonIcon').style.display = "none";
        //加算先部門名称
        document.getElementById('TxtKASANORGNAME').readOnly = true;
        //届先コード
        document.getElementById('TxtTODOKECODE').readOnly = true;
        document.getElementById('TxtTODOKECODEcommonIcon').style.display = "none";
        //届先名称
        document.getElementById('TxtTODOKENAME').readOnly = true;
        //車号
        document.getElementById('TxtSYAGOU').readOnly = true;
    };

};

document.addEventListener("DOMContentLoaded", function () {
    // カレンダー表示
    document.querySelectorAll('.datetimepicker').forEach(picker => {
        flatpickr(picker, {
            wrap: true,
            dateFormat: 'Y/m/d',
            locale: 'ja',
            clickOpens: false,
            allowInput: true,
            monthSelectorType: 'static',
            //defaultDate: new Date() // 必要に応じてカスタマイズ
        });
    });
});