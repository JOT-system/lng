// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;

    //入力不可キー
    document.getElementById('TxtSelLineCNT').readOnly = true; 
    document.getElementById('TxtCampCode').readOnly = true;

    //情シス、高圧ガス以外の場合、パンくず(検索)をを非表示にする
    if (VisibleKeyOrgCode == "") {
        document.getElementById('PAGE_SEARCH').style.display = "none";
    }

    //変更不可判断キーに値が入っていない場合
    if (DisabledKeyItem == "") {
        //名称を変更する
        document.getElementById('PAGE_NAME1').innerText = "単価マスタ（登録）";
        document.getElementById('PAGE_NAME2').innerText = "単価マスタ登録";
        document.getElementById('WF_ButtonUPDATE').value = "登録";
    } else {
        //値復元
        //document.getElementById('WF_StYMD').value = document.getElementById('WF_STYMD_SAVE').value;
        document.getElementById('WF_TORINAME').value = document.getElementById('WF_TORINAME_SAVE').value;
        document.getElementById('WF_TORICODE_TEXT').value = document.getElementById('WF_TORICODE_TEXT_SAVE').value;
        document.getElementById('WF_ORG').value = document.getElementById('WF_ORG_SAVE').value;
        document.getElementById('WF_KASANORG').value = document.getElementById('WF_KASANORG_SAVE').value;
        document.getElementById('WF_AVOCADOSHUKANAME').value = document.getElementById('WF_AVOCADOSHUKANAME_SAVE').value;
        document.getElementById('WF_AVOCADOSHUKABASHO_TEXT').value = document.getElementById('WF_AVOCADOSHUKABASHO_TEXT_SAVE').value;
        document.getElementById('WF_AVOCADOTODOKENAME').value = document.getElementById('WF_AVOCADOTODOKENAME_SAVE').value;
        document.getElementById('WF_AVOCADOTODOKECODE_TEXT').value = document.getElementById('WF_AVOCADOTODOKECODE_TEXT_SAVE').value;
        document.getElementById('TxtSHABAN').value = document.getElementById('WF_SHABAN_SAVE').value;
        document.getElementById('TxtBRANCHCODE').value = document.getElementById('WF_BRANCHCODE_SAVE').value;
        document.getElementById('WF_SYAGATA').value = document.getElementById('WF_SYAGATA_SAVE').value;
        document.getElementById('TxtSYABARA').value = document.getElementById('WF_SYABARA_SAVE').value;

        //document.getElementById('WF_StYMD').disabled = true;
        document.getElementById('WF_TORINAME').disabled = true;
        document.getElementById('WF_TORICODE_TEXT').disabled = true;
        document.getElementById('WF_ORG').disabled = true;
        document.getElementById('WF_KASANORG').disabled = true;
        document.getElementById('WF_AVOCADOSHUKANAME').disabled = true;
        document.getElementById('WF_AVOCADOSHUKABASHO_TEXT').disabled = true;
        document.getElementById('WF_AVOCADOTODOKENAME').disabled = true;
        document.getElementById('WF_AVOCADOTODOKECODE_TEXT').disabled = true;
        document.getElementById('TxtSHABAN').disabled = true;
        document.getElementById('TxtBRANCHCODE').disabled = true;
        document.getElementById('WF_SYAGATA').disabled = true;
        document.getElementById('TxtSYABARA').disabled = true;

        //document.getElementById('WF_StYMD').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_TORINAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_TORICODE_TEXT').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_ORG').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_KASANORG').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_AVOCADOSHUKANAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_AVOCADOSHUKABASHO_TEXT').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_AVOCADOTODOKENAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_AVOCADOTODOKECODE_TEXT').style.backgroundColor = "#F2F2F2";
        document.getElementById('TxtSHABAN').style.backgroundColor = "#F2F2F2";
        document.getElementById('TxtBRANCHCODE').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_SYAGATA').style.backgroundColor = "#F2F2F2";
        document.getElementById('TxtSYABARA').style.backgroundColor = "#F2F2F2";
    }

    //ポップアップメッセージ
    let wkmessage1 = "シーエナジー/エルネスのように、輸送費明細上で独自のコードや出荷場所名・届先名を\n" +
        "使用している場合に入力する項目です。\n" +
        "AVOCADOのマスタそのままで良い場合は、未入力で問題ありません。"; 
    document.getElementById("WF_SHUKA_CHANGE").title = wkmessage1;
    document.getElementById("WF_TODOKE_CHANGE").title = wkmessage1;

    let wkmessage2 = "単価区分：\n" +
        "2回転時や、何らかの条件で単価が異なる場合が生じる場合は「調整」を選択してください。\n" +
        "単価変動が無い場合は、通常のままで問題ありません。\n" +
        "\n" +
        "単価用途：\n" +
        "単価調整画面で単価を調整する際の選択肢として表示します。\n" +
        String.fromCharCode(34) + "2回転時単価" + String.fromCharCode(34) + "など、識別しやすい文字列の登録を推奨します。"; 
    document.getElementById("WF_MEMO_L").title = wkmessage2;

    let wkmessage3 = "JOT手数料として収受する割合(JOT収入分)をパーセンテージで入力してください。\n" +
        "JOTとENEXの割合は、合計100%となるようにしてください。"; 
    document.getElementById("WF_JOTPERCENTAGE_L").title = wkmessage3;

    let wkmessage4 = "ENEXへ支払う割合(ENEX収入分)をパーセンテージで入力してください。\n" +
        "JOTとENEXの割合は、合計100%となるようにしてください。";
    document.getElementById("WF_ENEXPERCENTAGE_L").title = wkmessage4;


    //チェックボックス状態復元(出荷場所)
    if (document.getElementById('WF_SHUKACHKSTATUS').value == "true") {
        document.getElementById('WF_SHUKACHANGE').checked = true;
        document.getElementById('ShukaChangeArea').style.display = 'inline';
    } else {
        document.getElementById('WF_SHUKACHANGE').checked = false;
        document.getElementById('ShukaChangeArea').style.display = 'none';
    }
    //チェックボックス状態復元(届先場所)
    if (document.getElementById('WF_TODOKECHKSTATUS').value == "true") {
        document.getElementById('WF_TODOKECHANGE').checked = true;
        document.getElementById('TodokeChangeArea').style.display = 'inline';
    } else {
        document.getElementById('WF_TODOKECHANGE').checked = false;
        document.getElementById('TodokeChangeArea').style.display = 'none';
    }

    f_syagata();
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

    /* scrollイベント発生時に表示タブのスクロール位置を保存する処理を追加 */
    var panel = document.getElementById("detailkeybox");
    if (panel != null) {
        panel.addEventListener('scroll', saveTabScrollPosition);
    }

});

//出荷場所チェックボックス変更時処理
function ChkShukaChange() {

    //チェックボックス状態保持
    document.getElementById('WF_SHUKACHKSTATUS').value = document.getElementById('WF_SHUKACHANGE').checked;

    if (document.getElementById('WF_SHUKACHANGE').checked) {
        document.getElementById('ShukaChangeArea').style.display = 'inline';
    } else {
        document.getElementById('ShukaChangeArea').style.display = 'none';
    }
}

//届先変更チェックボックス変更時処理
function ChkTodokeChange() {

    //チェックボックス状態保持
    document.getElementById('WF_TODOKECHKSTATUS').value = document.getElementById('WF_TODOKECHANGE').checked;

    if (document.getElementById('WF_TODOKECHANGE').checked) {
        document.getElementById('TodokeChangeArea').style.display = 'inline';
    } else {
        document.getElementById('TodokeChangeArea').style.display = 'none';
    }
}

//車型リストボックス変更時
function f_syagata() {
    document.getElementById('WF_SYAGATA_CODE_TEXT').innerHTML = document.getElementById('WF_SYAGATA').value;
}