// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeySystem = document.getElementById('DisabledKeySystem').value;

    //情報システム部以外でログインした場合会社コードを入力不可にする
    if (DisabledKeySystem == "") {
        document.getElementById('TxtCampCode').readOnly = true;
        document.getElementById('TxtCampCodecommonIcon').style.display = "none";
    };
};

document.addEventListener("DOMContentLoaded", function () {
    // #contentsInnerの高さ取得
    let windowHeight = window.innerHeight;
    const headerHeight = 47;
    const breadcrumbHeight = 18;
    const spaceHeight = 8 + 8 + 16;
    let contentsInnerHeight = windowHeight - headerHeight - breadcrumbHeight - spaceHeight;
    document.getElementById("contentsInner").style.height = contentsInnerHeight + "px";

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