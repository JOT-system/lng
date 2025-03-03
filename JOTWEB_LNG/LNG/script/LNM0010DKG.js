// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';
    //非表示判断キー
    const WF_OPERATION = document.getElementById('WF_OPERATION').value;
    if (WF_OPERATION == "Insert") {
        document.getElementById('UPDITEM').style.display = "none";
    } else {
        document.getElementById('INSITEM').style.display = "none";
    }

    //大項目項目名非表示判定
    for (var item = 1; item <= 20; item++) {
        if (document.getElementById('WF_SEL_ITEM_' + ("00" + item).slice(-2) + '_NAME').innerText == "") {
            document.getElementById('LARGECATEGORY_LINE_' + ("00" + item).slice(-2)).style.display = "none";
        }
    }
    //レコード欄非表示判定
    for (var item = 1; item <= 20; item++) {
        for (var reco = 1; reco <= 20; reco++) {
            if (document.getElementById('WF_SEL_ITEM_' + ("00" + item).slice(-2) + '_RECONAME_' + ("00" + reco).slice(-2)).innerText == "") {
                document.getElementById('RECO_LINE_ITEM_' + ("00" + item).slice(-2) + '_' + ("00" + reco).slice(-2)).style.display = "none";
                document.getElementById('DEL_LINE_ITEM_' + ("00" + item).slice(-2) + '_' + ("00" + reco).slice(-2)).style.display = "none";
                document.getElementById('WF_SEL_ITEM_' + ("00" + item).slice(-2) + '_DELFLG_' + ("00" + reco).slice(-2)).style.display = "none";
            }
        }
    }
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
            plugins: [
                new monthSelectPlugin({
                    shorthand: true, //defaults to false
                    dateFormat: "Y/m",
                    altFormat: "F Y", //defaults to "F Y"
                    theme: "light" // defaults to "light"
                })
            ]
        });
    });
});

//単価、回数テキストボックスフォーカスアウト時、料金計算
function txtOnblur() {
    let tanka = 0; //単価
    let count = 0; //回数
    let fee = 0; //料金
    let feesummary = 0; //料金サマリ

    for (var item = 1; item <= 20; item++) {
        feesummary = 0
        for (var reco = 1; reco <= 20; reco++) {
            //console.log('WF_SEL_ITEM_' + ("00" + item).slice(-2) + '_RECONAME_' + ("00" + reco).slice(-2))
            //単価取得
            tanka = document.getElementById('WF_SEL_ITEM_' + ("00" + item).slice(-2) + '_TANKA_' + ("00" + reco).slice(-2)).value
            if (tanka == "") {
                tanka = 0;
            } 
            //回数取得
            count = document.getElementById('WF_SEL_ITEM_' + ("00" + item).slice(-2) + '_COUNT_' + ("00" + reco).slice(-2)).value
            if (count == "") {
                count = 0;
            } 
            //料金計算
            fee = tanka * count;
            document.getElementById('WF_SEL_ITEM_' + ("00" + item).slice(-2) + '_FEE_' + ("00" + reco).slice(-2)).innerText = fee;

            //料金サマリ加算
            feesummary = feesummary + fee;
        }
        //料金サマリ
        document.getElementById('WF_SEL_ITEM_' + ("00" + item).slice(-2) + '_FEESUMMARY').innerText = feesummary;
    }
}

