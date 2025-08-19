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
        document.getElementById('PAGE_NAME1').innerText = "サーチャージ定義マスタ（登録）";
        document.getElementById('PAGE_NAME2').innerText = "サーチャージ定義マスタ登録";
        document.getElementById('WF_ButtonUPDATE').value = "登録";
    } else {
        //値復元
        document.getElementById('WF_TORICODE').value = document.getElementById('WF_TORICODE_SAVE').value;
        SetDropDownList(document.getElementById('WF_TORINAME'), document.getElementById('WF_TORINAME_SAVE').value);
        document.getElementById('WF_ORGCODE').value = document.getElementById('WF_ORG_SAVE').value;
        SetDropDownList(document.getElementById('WF_ORGNAME'), document.getElementById('WF_ORGNAME_SAVE').value);
        document.getElementById('WF_KASANORGCODE').value = document.getElementById('WF_KASANORG_SAVE').value;
        SetDropDownList(document.getElementById('WF_KASANORGNAME'), document.getElementById('WF_KASANORGNAME_SAVE').value);
        document.getElementById('WF_BILLINGCYCLE').value = document.getElementById('WF_BILLINGCYCLE_SAVE').value;
        SetDropDownList(document.getElementById('WF_BILLINGCYCLENAME'), document.getElementById('WF_BILLINGCYCLENAME_SAVE').value);
        document.getElementById('WF_SURCHARGEPATTERNCODE').value = document.getElementById('WF_SURCHARGEPATTERNCODE_SAVE').value;
        SetDropDownList(document.getElementById('WF_SURCHARGEPATTERNNAME'), document.getElementById('WF_SURCHARGEPATTERNNAME_SAVE').value);

        document.getElementById('WF_TORICODE').disabled = true;
        document.getElementById('WF_TORINAME').disabled = true;
        document.getElementById('WF_ORGCODE').disabled = true;
        document.getElementById('WF_ORGNAME').disabled = true;
        document.getElementById('WF_KASANORGCODE').disabled = true;
        document.getElementById('WF_KASANORGNAME').disabled = true;
        document.getElementById('WF_BILLINGCYCLE').disabled = true;
        document.getElementById('WF_BILLINGCYCLENAME').disabled = true;
        document.getElementById('WF_SURCHARGEPATTERNCODE').disabled = true;
        document.getElementById('WF_SURCHARGEPATTERNNAME').disabled = true;

        document.getElementById('WF_TORICODE').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_TORINAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_ORGCODE').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_ORGNAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_KASANORGCODE').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_KASANORGNAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_BILLINGCYCLE').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_BILLINGCYCLENAME').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_SURCHARGEPATTERNCODE').style.backgroundColor = "#F2F2F2";
        document.getElementById('WF_SURCHARGEPATTERNNAME').style.backgroundColor = "#F2F2F2";
    }

    //ポップアップメッセージ
    let wkmessage1 = "【請求サイクルについて】 \n" +
        "年度内に何度請求を行うかを指定します。\n" +
        "請求書発行年月や料金計算期間は、別画面(サーチャージ料金マスタ)で設定します。"; 
    document.getElementById("WF_BILLINGCYCLE_L").title = wkmessage1;

    let wkmessage2 = "【サーチャージパターンについて】 \n" +
        "・荷主単位：車両や届先に関わらず、荷主単位でサーチャージ料金が定義されている場合に使用します。\n" +
        "・届先単位：輸送距離や基準単価等が、届先(及び出荷場所)毎に定義されている場合に使用します。\n" +
        "・車型単位：車型によって基準単価等が異なる場合に使用します。届先を条件に含めることも可能です。\n" +
        "・車腹単位：車腹によって基準単価等が異なる場合に使用します。届先を条件に含めることも可能です。\n" +
        "・車番単位：車番によって基準単価等が異なる場合に使用します。最小単位です。" 
    document.getElementById("WF_SURCHARGEPATTERNCODE_L").title = wkmessage2;

    let wkmessage3 = "【距離計算方式について】 \n" +
        "・事前定義：\n" +
        "　協定書等で、ラウンド毎の輸送距離が定義されている場合に選択してください。\n" +
        "　サーチャージ料金マスタで、輸送距離の登録が必要です。\n" +
        "・画面入力：\n" +
        "　輸送距離の実績値に応じてサーチャージ精算を行う場合、こちらを選択してください。\n" 
        "　サーチャージ請求書発行時、輸送距離を入力する必要があります。"; 
    document.getElementById("WF_CALCMETHOD_L").title = wkmessage3;

    let wkmessage4 = "【実勢価格参照先について】\n" +
        "軽油実勢価格の参照先を選択します。\n" +
        "実勢価格参照先の新規登録・変更が必要な場合は\n" +
        "「軽油価格参照先管理マスタ」でメンテナンスを行ってください。"
    document.getElementById("WF_DISPLAYNAME_L").title = wkmessage4;
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

function SetDropDownList(dropdown_name, save_name) {
    const dropdown = dropdown_name;
    for (let i = 0; i < dropdown.options.length; i++) {
        if (dropdown.options[i].text === save_name) {
            dropdown.selectedIndex = i;
            break;
        }
    }
}

