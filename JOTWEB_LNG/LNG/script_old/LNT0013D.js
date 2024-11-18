// 画面読み込み時処理
window.onload = function () {
    
    LeaseFinalOnload()
    
    BtnDownloadOnload()
    
    AcntLinkOnload()
    
    TrusteeSrcOnload()
    
    mapSetting()
    
};

// ○左Box用処理（左Box表示/非表示切り替え）
function Spred_Field_DBclick(fieldNM, tabNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
       document.getElementById("MF_SUBMIT").value = "TRUE";
       document.getElementById('WF_FIELD').value = fieldNM;
       document.getElementById('WF_LeftMViewChange').value = tabNo;
       document.getElementById('WF_LeftboxOpen').value = "Open";

       document.getElementById("WF_ButtonClick").value = "WF_SPREAD_BtnClick";
       //document.body.style.cursor = "wait";
       commonDispWait();
       document.getElementById("WF_saveLeft").value = 0;
       document.forms[0].submit();
    }
}

// ○経理連携用処理
function AcntLinkOnload() {

  // 経理連携時ボタン制御
    if ((document.getElementById('WF_BUTTON_ROWADD').value === "1") ||
       (document.getElementById('WF_UPDATEBtnDisabledFlg').value === "1")){
        document.getElementById('WF_Row_Add').disabled = true;
    } else {
        document.getElementById('WF_Row_Add').disabled = false;
    }
  // 経理連携時ボタン制御
    if ((document.getElementById('WF_BUTTON_SAVE').value === "1") ||
       (document.getElementById('WF_UPDATEBtnDisabledFlg').value === "1")){
        document.getElementById('WF_ButtonSave').disabled = true;
    } else {
        document.getElementById('WF_ButtonSave').disabled = false;
    }
}

// ○行追加OnLoad用処理
function LeaseFinalOnload() {

    let ele = document.getElementById('pnlLeaseFinalSrcWrapper');
    ele.style.visibility = document.getElementById('WF_KAGENGAKU').value;
}

// ○ダウンロードボタン表示制御
function BtnDownloadOnload() {

    let ele = document.getElementById('WF_ButtonDownload');
    ele.style.visibility = document.getElementById('WF_DOWNLOAD').value;
}

/**
 *  行追加ダイアログ『閉じる』ボタンクリックイベント
 */
function LeaseFinalSrcCloseClick() {

    ButtonClick('WF_ButtonLeaseFinalCLOSE');
}

// ○スプレッドシート内ボタン押下処理
function Spred_ButtonSel_click(btn) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = btn;
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}
// ○受託人検索OnLoad用処理
function TrusteeSrcOnload() {

    let ele = document.getElementById('pnlTrusteeSrcWrapper');
    ele.style.visibility = document.getElementById('WF_TrusteeSrc').value;

    /* スプレッドシートのダブルクリックイベント紐づけ */
    var spread = document.getElementById("spdTrustee");

    if (spread.addEventListener) {
        spread.addEventListener("dblclick", DblClickTrustee, false);
    } else {
        spread.ondblclick = DblClickTrustee;
    }

}

/**
 *  発受託人検索ダイアログ ボタンクリックイベント
 */
function DeptrusteeSrc_Click() {

    ButtonClick('WF_ButtonDeptrustee');
}

/**
 *  着受託人検索ダイアログ ボタンクリックイベント
 */
function ArrtrusteeSrc_Click() {

    ButtonClick('WF_ButtonArrtrustee');
}

/**
 *  受託人検索ダイアログ『閉じる』ボタンクリックイベント
 */
function TrusteeSrcCloseClick() {

    ButtonClick('WF_ButtonTrusteeSrcCLOSE');
}

/**
 * スプレッドシート・ダブルクリック処理
 */
function DblClickTrustee(e) {
    var e = e || window.event;
    var s = e.target || e.srcElement;
    var p1 = s.parentNode || s.parentElement;
    var p2 = s.parentNode.parentNode || s.parentElement.parentElement;
    var p3 = s.parentNode.parentNode.parentNode || s.parentElement.parentElement.parentElement;

    //属性の取得
    p1atb = p1.getAttribute("spdTrustee");
    p2atb = p2.getAttribute("spdTrustee");
    p3atb = p3.getAttribute("spdTrustee");

    //p1atbを設定
    var cl = p1atb

    //p1atbが存在しない場合
    if (cl == undefined) {
        //p2atbを設定
        cl = p2atb;
        //p2atbが存在しない場合
        if (cl == undefined) {
            //p3atbを設定
            cl = p3atb;
        }
    }

    //処理
    var spread = document.getElementById(spid);
    var row = spread.GetActiveRow();
    var col = spread.GetActiveCol();

    //選択行を非表示項目にセット
    hidRowIndex.value = row;

    //サブミット
    document.getElementById("MF_SUBMIT").value = "TRUE";
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclickTrustee";
    document.forms[0].submit();
}
// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* スプレッドシートのダブルクリックイベント紐づけ */
    var spread = document.getElementById("spdPaymentList");

    if (spread.addEventListener) {
        spread.addEventListener("dblclick", DblClick_Pay, false);
    } else {
        spread.ondblclick = DblClick_Pay;
    };

};

/**
 * 使用料スプレッドシート・ダブルクリック処理
 */
function DblClick_Pay(e) {
    var e = e || window.event;
    var s = e.target || e.srcElement;
    var p1 = s.parentNode || s.parentElement;
    var p2 = s.parentNode.parentNode || s.parentElement.parentElement;
    var p3 = s.parentNode.parentNode.parentNode || s.parentElement.parentElement.parentElement;

    //属性の取得
    p1atb = p1.getAttribute("spdPaymentList");
    p2atb = p2.getAttribute("spdPaymentList");
    p3atb = p3.getAttribute("spdPaymentList");

    //p1atbを設定
    var cl = p1atb

    //p1atbが存在しない場合
    if (cl == undefined) {
        //p2atbを設定
        cl = p2atb;
        //p2atbが存在しない場合
        if (cl == undefined) {
            //p3atbを設定
            cl = p3atb;
        }
    }

    //処理
    var spread = document.getElementById("spdPaymentList");
    var row = spread.GetActiveRow();
    var col = spread.GetActiveCol();

    //選択行を非表示項目にセット
    //hidRowIndex.value = row;

    if (row == -1 || col == -1) {
        return
    }

    //サブミット
    document.getElementById("MF_SUBMIT").value = "TRUE";
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclick_Pay";
    document.forms[0].submit();
}

// 画面制御処理
function mapSetting() {

    // ポップアップ処理切り替えフラグ
    const PayAddFlg = document.getElementById('WF_PAYMENTADD_FLG').value;

    // 追加の場合
    if (PayAddFlg == '1') {
        // 追加ボタンを表示
        document.getElementById('btnLeaseFinalSrcSERRCH').style.display = 'inline'
        // 修正ボタンを非表示
        document.getElementById('btnFinalSrcSERRCH').style.display = 'none'
    } else if (PayAddFlg == "0") {
        // 追加ボタンを非表示
        document.getElementById('btnLeaseFinalSrcSERRCH').style.display = 'none'
        // 修正ボタンを表示
        document.getElementById('btnFinalSrcSERRCH').style.display = 'inline'
    };
}
