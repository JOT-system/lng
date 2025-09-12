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
        document.getElementById('PAGE_NAME1').innerText = "固定費マスタ（登録）";
        document.getElementById('PAGE_NAME2').innerText = "固定費マスタ登録";
        document.getElementById('WF_ButtonUPDATE').value = "登録";
    } else {
        //値復元
        document.getElementById('WF_TARGETYM').value = document.getElementById('WF_TARGETYM_SAVE').value;
        document.getElementById('WF_TORINAME').value = document.getElementById('WF_TORINAME_SAVE').value;
        document.getElementById('WF_TORICODE_TEXT').value = document.getElementById('WF_TORICODE_TEXT_SAVE').value; 
        document.getElementById('WF_ORG').value = document.getElementById('WF_ORG_SAVE').value;
        document.getElementById('WF_SEASONKBN').value = document.getElementById('WF_SEASONKBN_SAVE').value;

        document.getElementById('WF_TARGETYM').disabled = true;
        document.getElementById('WF_TORINAME').disabled = true; 
        document.getElementById('WF_TORICODE_TEXT').disabled = true; 
        document.getElementById('WF_ORG').disabled = true;
        document.getElementById('WF_SEASONKBN').disabled = true;

        document.getElementById('WF_TARGETYM').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_TORINAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_TORICODE_TEXT').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_ORG').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_SEASONKBN').backgroundColor = "#F2F2F2";
    }

    //ポップアップメッセージ
    let wkmessage1 = "JOT手数料として収受する割合(JOT収入分)をパーセンテージで入力してください。\n" +
        "JOTとENEXの割合は、合計100%となるようにしてください。";
    document.getElementById("WF_JOTPERCENTAGE_L").title = wkmessage1;

    let wkmessage2 = "ENEXへ支払う割合(ENEX収入分)をパーセンテージで入力してください。\n" +
        "JOTとENEXの割合は、合計100%となるようにしてください。";
    document.getElementById("WF_ENEXPERCENTAGE_L").title = wkmessage2;

    f_syagata();
    f_seasonkbn();
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

//車型リストボックス変更時
function f_syagata() {
    document.getElementById('WF_SYAGATA_CODE_TEXT').value = document.getElementById('WF_SYAGATA').value;
}
//季節料金判定区分リストボックス変更時
function f_seasonkbn() {
    switch (document.getElementById('WF_SEASONKBN').value) {
        case "0": //通年
            document.getElementById('TxtSEASONSTART').value = "";
            document.getElementById('TxtSEASONEND').value = "";
            document.getElementById('TxtSEASONSTART').readOnly = true;
            document.getElementById('TxtSEASONEND').readOnly = true;
            document.getElementById('TxtSEASONSTART').style.backgroundColor = "#F2F2F2";
            document.getElementById('TxtSEASONEND').style.backgroundColor = "#F2F2F2";
            break;
        case "1": //夏季料金
            document.getElementById('TxtSEASONSTART').readOnly = false;
            document.getElementById('TxtSEASONEND').readOnly = false;
            document.getElementById('TxtSEASONSTART').style.backgroundColor = "#FFFFFF";
            document.getElementById('TxtSEASONEND').style.backgroundColor = "#FFFFFF";
            break;
        case "2": //冬季料金
            document.getElementById('TxtSEASONSTART').readOnly = false;
            document.getElementById('TxtSEASONEND').readOnly = false;
            document.getElementById('TxtSEASONSTART').style.backgroundColor = "#FFFFFF";
            document.getElementById('TxtSEASONEND').style.backgroundColor = "#FFFFFF";
            break;
    }
}

function reAmount() {
    // 月額固定費
    let koteihiM = +document.getElementById("TxtKOTEIHIM").value;   // 数値に変換
    document.getElementById("TxtKOTEIHIM").value = koteihiM;
    // 調整額
    let gengaku = +document.getElementById("TxtGENGAKU").value;     // 数値に変換
    document.getElementById("TxtGENGAKU").value = gengaku;
    // 月額固定費 + 調整額
    let amount = parseFloat(koteihiM) + parseFloat(gengaku);
    document.getElementById("TxtAMOUNT").value = amount
}