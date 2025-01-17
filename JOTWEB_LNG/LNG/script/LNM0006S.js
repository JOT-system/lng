// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';
}

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