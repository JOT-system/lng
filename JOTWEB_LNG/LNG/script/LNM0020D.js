// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;

    //入力不可キー
    document.getElementById('TxtSelLineCNT').readOnly = true; 
    document.getElementById('TxtCampCode').readOnly = true;


    //変更不可判断キーに値が入っていない場合
    if (DisabledKeyItem == "") {
        //名称を変更する
        document.getElementById('PAGE_NAME1').innerText = "軽油価格参照先マスタ（登録）";
        document.getElementById('PAGE_NAME2').innerText = "軽油価格参照先マスタ登録";
        document.getElementById('WF_ButtonUPDATE').value = "登録";
    } else {
        //値復元
        document.getElementById('WF_DEISELPRICESITENAME').value = document.getElementById('WF_DEISELPRICESITENAME_SAVE').value;
        document.getElementById('WF_DEISELPRICESITEID').value = document.getElementById('WF_DEISELPRICESITEID_SAVE').value; 
        document.getElementById('WF_DEISELPRICESITEKBNNAME').value = document.getElementById('WF_DEISELPRICESITEKBNNAME_SAVE').value; 
        document.getElementById('WF_DEISELPRICESITEBRANCH').value = document.getElementById('WF_DEISELPRICESITEBRANCH_SAVE').value; 

        document.getElementById('WF_DEISELPRICESITENAME').disabled = true; 
        document.getElementById('WF_DEISELPRICESITEID').disabled = true; 
        document.getElementById('WF_DEISELPRICESITEKBNNAME').disabled = true; 
        document.getElementById('WF_DEISELPRICESITEBRANCH').disabled = true; 

        document.getElementById('WF_DEISELPRICESITENAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_DEISELPRICESITEID').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_DEISELPRICESITEKBNNAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_DEISELPRICESITEBRANCH').style.backgroundColor = "#F2F2F2";
    }
};

function InitDisplay() {

    /* スクロール位置復元 */
    let panel = document.getElementById("detailkeybox");
    if (panel != null) {
        let top = Number(document.getElementById("WF_scrollY").value);
        panel.scrollTo(0, top);
    }
}

function saveTabScrollPosition() {
    let panel = document.getElementById("detailkeybox");
    if (panel != null) {
        document.getElementById("WF_scrollY").value = panel.scrollTop;
    }
}

document.addEventListener("DOMContentLoaded", function () {

    //現在時刻取得
    let wk = new Date();
    //2カ月前取得
    wk.setMonth(wk.getMonth() - 2);
    let wkminDate = wk.toLocaleDateString('en-CA');

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
            ],
            minDate: wkminDate

        });
    });

    /* scrollイベント発生時に表示タブのスクロール位置を保存する処理を追加 */
    var panel = document.getElementById("detailkeybox");
    if (panel != null) {
        panel.addEventListener('scroll', saveTabScrollPosition);
    }

});

