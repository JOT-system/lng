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
        document.getElementById('PAGE_NAME1').innerText = "特別料金マスタ（登録）";
        document.getElementById('PAGE_NAME2').innerText = "特別料金マスタ登録";
        document.getElementById('WF_ButtonUPDATE').value = "登録";
    } else {
        //値復元
        document.getElementById('WF_TARGETYM').value = document.getElementById('WF_TARGETYM_SAVE').value;
        document.getElementById('WF_TORI').value = document.getElementById('WF_TORI_SAVE').value;
        document.getElementById('WF_ORG').value = document.getElementById('WF_ORG_SAVE').value;
        document.getElementById('TxtGROUPNAME').value = document.getElementById('WF_GROUPNAME_SAVE').value;
        document.getElementById('TxtDETAILNAME').value = document.getElementById('WF_DETAILNAME_SAVE').value;

        document.getElementById('WF_TARGETYM').disabled = true;
        document.getElementById('WF_TORI').disabled = true;
        document.getElementById('WF_ORG').disabled = true;

        document.getElementById('TxtGROUPNAMEcommonIcon').style.display = "none";
        document.getElementById('TxtGROUPNAME').disabled = true;
        document.getElementById('TxtDETAILNAMEcommonIcon').style.display = "none";
        document.getElementById('TxtDETAILNAME').disabled = true;

        document.getElementById('WF_TARGETYM').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_TORI').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_ORG').style.backgroundColor = "#F2F2F2";
        document.getElementById('TxtGROUPNAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('TxtDETAILNAME').style.backgroundColor = "#F2F2F2";

    };

    //ポップアップメッセージ
    document.getElementById("pnlYusouArea").title = "明細グループ名\n" +
        "　・・・共通する明細名を束ねたい場合に使用してください。\n" +
        "　　　　束ねる必要が無い場合は、こちらを明細名としてご使用ください。\n" +
        "\n" +
        "明細名\n" +
        "　・・・グループで束ねたい場合に使用してください。\n" +
        "　　　　束ねる必要が無い場合はグループ名を使用し、こちらは入力不要です。"; 

    //チェックボックス状態復元
    if (document.getElementById('WF_ATENACHKSTATUS').value == "true") {
        document.getElementById('WF_ATENACHANGE').checked = true;
        document.getElementById('AtenaChangeArea').style.display = 'inline';
    } else {
        document.getElementById('WF_ATENACHANGE').checked = false;
        document.getElementById('AtenaChangeArea').style.display = 'none';
    }

    //リストボックス状態復元
    if (document.getElementById('WF_ATENALISTSELECT').value == "MAE") {
        document.getElementById("ddlMAEKABU").options[1].selected = true;
    }
    if (document.getElementById('WF_ATENALISTSELECT').value == "ATO") {
        document.getElementById("ddlATOKABU").options[1].selected = true;
    }
};

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
});


//宛名書変更チェックボックス変更時処理
function ChkAtenaChange() {

    //チェックボックス状態保持
    document.getElementById('WF_ATENACHKSTATUS').value = document.getElementById('WF_ATENACHANGE').checked;

    if (document.getElementById('WF_ATENACHANGE').checked) {
        document.getElementById('AtenaChangeArea').style.display = 'inline';
    } else {
        document.getElementById('AtenaChangeArea').style.display = 'none';
    }
}

//前株リストボックス変更時処理
function MaekabuChange() {
    let mae = document.getElementById("ddlMAEKABU");
    let ato = document.getElementById("ddlATOKABU");

    if (mae.options[1].selected == true && ato.options[1].selected == true) {
        ato.options[0].selected = true;
    }

    // リストボックス選択状態保持
    document.getElementById('WF_ATENALISTSELECT').value = "";
    if (mae.options[1].selected == true){
        document.getElementById('WF_ATENALISTSELECT').value = "MAE";
    }
    if (ato.options[1].selected == true) {
        document.getElementById('WF_ATENALISTSELECT').value = "ATO";
    }
}

//後株リストボックス変更時処理
function AtokabuChange() {
    let mae = document.getElementById("ddlMAEKABU");
    let ato = document.getElementById("ddlATOKABU");

    if (mae.options[1].selected == true && ato.options[1].selected == true) {
        mae.options[0].selected = true;
    }

    // リストボックス選択状態保持
    document.getElementById('WF_ATENALISTSELECT').value = "";
    if (mae.options[1].selected == true) {
        document.getElementById('WF_ATENALISTSELECT').value = "MAE";
    }
    if (ato.options[1].selected == true) {
        document.getElementById('WF_ATENALISTSELECT').value = "ATO";
    }
}