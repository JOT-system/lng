//************************************************************
// 画面名称：リース登録　詳細画面
// 作成日：2022/02/24
// 作成者：杉元　孝行
// 更新日：2024/04/11
// 更新者：杉元　孝行
//
//修正履歴：
// 2024/04/11 杉元孝行 スポットリース一括請求対応
// 2024/08/14 杉元孝行 スポット区分追加対応
// 2024/08/21 杉元孝行 契約終了日を締日とする契約への対応
//************************************************************

// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // スクロール位置を復元 
    if (document.getElementById("detailbox") !== null) {
        document.getElementById("detailbox").scrollTop = document.getElementById("WF_ClickedScrollTop").value;
    }
}

// 画面読み込み時処理
window.onload = function () {

    // モード
    const Mode  = document.getElementById('WF_MODE').value;

    // ボタンの入力制御 他はクライアント側で制御
    // 請求先、請求先部門　入力行のパネル
    var input_pnlDetailboxLine3 = document.getElementById("pnlDetailboxLine3").getElementsByClassName("btn-stickyDetail");
    // リース開始日　入力行のパネル
    var pnlLineInputLeaseDate = document.getElementById("pnlLineInputLeaseDate").getElementsByClassName("btn-stickyDetail");
    // コンテナ一覧　ボタンのパネル
    var pnlCtnListBtn = document.getElementById("pnlCtnListBtn").getElementsByClassName("btn-sticky");

    // 新規モード
    if (Mode == "1") {
        // 請求先、請求先部門　入力行のパネル
        for(var i=0; i < input_pnlDetailboxLine3.length; i++){
            input_pnlDetailboxLine3[i].disabled = false;
        }

        // リース開始日　入力行のパネル
        for(var i=0; i < pnlLineInputLeaseDate.length; i++){
            pnlLineInputLeaseDate[i].disabled = false;
        }

        // コンテナ一覧　ボタン行のパネル
        for (var i = 0; i < pnlCtnListBtn.length; i++) {
            pnlCtnListBtn[i].disabled = false;
        }

        // 自動更新ボタン
        document.getElementById('WF_ButtonAUTOCALC').disabled = false;
        // 追加コンテナ一括入力ボタン
        document.getElementById('WF_TAB3_BTN_CONTAINER_BULK_INPUT').disabled = false;
        // ファイナンス情報一括入力ボタン
        document.getElementById('WF_BTN_FINAL_BULK_INPUT').disabled = false;
        // コンテナ番号一括アップロード
        document.getElementById('WF_ButtonUPLOAD').disabled = false;
        // 2024/04/11 杉元孝行 スポットリース一括請求対応 ADD START
        // 一括請求ボタン
        document.getElementById('WF_ButtonINVOICEALL').disabled = false;
        // 2024/04/11 杉元孝行 スポットリース一括請求対応 ADD END
        // 2024/08/14 杉元孝行 スポット区分追加対応 ADD START
        // スポット区分ボタン
        document.getElementById('WF_ButtonSPOTKBN').disabled = false;
        // 2024/08/14 杉元孝行 スポット区分追加対応 ADD END
        // 2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD START
        // 締日区分ボタン
        document.getElementById('WF_ButtonCLOSINGDAYKBN').disabled = false;
        // 2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD END

    } else {
        // 請求先、請求先部門　入力行のパネル
        for(var i=0; i < input_pnlDetailboxLine3.length; i++){
            input_pnlDetailboxLine3[i].disabled = true;
        }

        // リース開始日　入力行のパネル
        for(var i=0; i < pnlLineInputLeaseDate.length; i++){
            pnlLineInputLeaseDate[i].disabled = true;
        }

        // コンテナ一覧　ボタン行のパネル
        for (var i = 0; i < pnlCtnListBtn.length; i++) {
            pnlCtnListBtn[i].disabled = true;
        }

        // 自動更新ボタン
        document.getElementById('WF_ButtonAUTOCALC').disabled = true;
        // 追加コンテナ一括入力ボタン
        document.getElementById('WF_TAB3_BTN_CONTAINER_BULK_INPUT').disabled = true;
        // ファイナンス情報一括入力ボタン
        document.getElementById('WF_BTN_FINAL_BULK_INPUT').disabled = true;
        // コンテナ番号一括アップロード
        document.getElementById('WF_ButtonUPLOAD').disabled = true;
        // 
        document.getElementById('btnFileSelect').disabled = true;
        // 2024/04/11 杉元孝行 スポットリース一括請求対応 ADD START
        // 一括請求ボタン
        document.getElementById('WF_ButtonINVOICEALL').disabled = true;
        // 2024/04/11 杉元孝行 スポットリース一括請求対応 ADD END
        // 2024/08/14 杉元孝行 スポット区分追加対応 ADD START
        // スポット区分ボタン
        document.getElementById('WF_ButtonSPOTKBN').disabled = true;
        // 2024/08/14 杉元孝行 スポット区分追加対応 ADD END
        // 2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD START
        // 締日区分ボタン
        document.getElementById('WF_ButtonCLOSINGDAYKBN').disabled = true;
        // 2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD END
    };

    // する／しないの表示設定
    // リース開始日 日割計算
    btnDispChange('WF_ButtonDAYCALCSTART', 'WF_DayCalcStart');
    // リース終了日 日割計算
    btnDispChange('WF_ButtonDAYCALCEND', 'WF_DayCalcEnd');
    // 自動更新
    btnDispChange('WF_ButtonAUTOCALC', 'WF_AutoCalc');
    // リース開始日 日割計算(請求情報)
    btnDispChange('WF_ButtonIVINFO_DAYCALCSTART', 'WF_IVInfoDayCalcStart');
    // リース終了日 日割計算(請求情報)
    btnDispChange('WF_ButtonIVINFO_DAYCALCEND', 'WF_IVInfoDayCalcEnd');
    // 自動更新(請求情報)
    btnDispChange('WF_ButtonIVINFO_AUTOCALC', 'WF_IVInfoAutoCalc');
    // 2024/04/11 杉元孝行 スポットリース一括請求対応 ADD START
    // 一括請求
    btnDispChange('WF_ButtonINVOICEALL', 'WF_InvoiceAll');
    // 一括請求(請求情報)
    btnDispChange('WF_ButtonIVINFO_INVOICEALL', 'WF_IVInfoInvoiceAll');
    // 2024/04/11 杉元孝行 スポットリース一括請求対応 ADD END
    // 2024/08/14 杉元孝行 スポット区分追加対応 ADD START
    // スポット区分
    btnSpotKbnDispChange('WF_ButtonSPOTKBN', 'WF_SpotKbn');
    // スポット区分(請求情報)
    btnSpotKbnDispChange('WF_ButtonIVINFO_SPOTKBN', 'WF_IVInfoSpotKbn');
    // 2024/08/14 杉元孝行 スポット区分追加対応 ADD END
    // 2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD START
    // 締日区分
    btnClosingdayKbnDispChange('WF_ButtonCLOSINGDAYKBN', 'WF_ClosingdayKbn', 'txtClosingDate');
    // 締日区分(請求情報)
    btnClosingdayKbnDispChange('WF_ButtonIVInfoCLOSINGDAYKBN', 'WF_IVInfoClosingdayKbn', 'txtIVInfoClosingDate');
    // 2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD END

    // 登録ボタン入力不可制御
    if (document.getElementById('WF_INSERTBtnDisabledFlg').value === "1"){
        document.getElementById('WF_ButtonINSERT').disabled = true;
    } else {
        document.getElementById('WF_ButtonINSERT').disabled = false;
    }

    // スプレッドのフォーカス移動設定処理
    setKeyMap()

    // 決済条件検索OnLoad用処理
    KekkjSrcOnload()

    // 明細画面OnLoad用処理
    LeaseDatalistOnload()

    // コンテナ検索OnLoad用処理
    ReconmSrcOnload()

    // ○請求情報画面OnLoad用処理
    InvoiceInputOnload()

    // ファイナンス画面OnLoad用処理
    LeaseFinalOnload()

    // 請求先検索OnLoad用処理
//    InvoiceSrcOnload()

    // コンテナ一括入力フィールド 表示/非表示
    dispContainerBulkInputField('hdnContainerBulkInputField', 'divContainerBulkInputField')
    // コンテナ一括入力フィールド 表示/非表示イベントバインド
    bindShowCloseContainerBulkInputField();

    // ファイナンス情報一括入力フィールド 表示/非表示
    dispContainerBulkInputField('hdnFinalBulkInputField', 'divFinalBulkInputField')
    // ファイナンス情報一括入力フィールド 表示/非表示イベントバインド
    bindShowCloseFinalBulkInputField();    

    // 2024/08/14 杉元孝行 スポット区分追加対応 ADD START
    // 契約形態ボタンクリック時
    listContraLNMode_onchange()
    // 契約形態ボタンクリック時(請求情報)
    listIVInfoContraLNMode_onchange()
    // 2024/08/14 杉元孝行 スポット区分追加対応 ADD END

    // 新規モード
    if (Mode == "1") {
        $(function () {
            $('#inpFileUpload').css({
                'position': 'absolute',
                'top': '-9999px'
            }).change(function () {
                var val = $(this).val();
                var path = val.replace(/\\/g, '/');
                var match = path.lastIndexOf('/');
                $('#txtFileName').css("display", "inline-block");
                $('#txtFileName').val(match !== -1 ? val.substring(match + 1) : val);
            });
            $('#txtFileName').bind('keyup, keydown, keypress', function () {
                return false;
            });
            $('#txtFileName, #btnFileSelect').click(function () {
                $('#inpFileUpload').trigger('click');
            });
        });
    } else {
        $(function () {
            $('#inpFileUpload').css({
                'position': 'absolute',
                'top': '-9999px'
            });
        });
    }
};

// 〇する/しない切替ボタンクリック時
function btnDispChange(btnId, txtId) {
    if (document.getElementById(txtId).value === "1"){
        document.getElementById(btnId).value = "する";
    } else {
        document.getElementById(btnId).value = "しない";
    }
}

// 〇する/しない切替ボタンクリック時
function btnChange_click(btnId, txtId) {
    if (document.getElementById(btnId).value === "する") {
        document.getElementById(btnId).value = "しない";
        document.getElementById(txtId).value = "0";
    } else {
        document.getElementById(btnId).value = "する";
        document.getElementById(txtId).value = "1";
    }
}

// ○左Box用処理（左Box表示/非表示切り替え）
function Spred_Field_DBclick(fieldNM, tabNo) {

    var sheet = document.getElementById("spdCtnList");
    var row = sheet.GetActiveRow();
    var col = sheet.GetActiveCol();
    var elem = document.activeElement;
    var result1 = elem.id.indexOf('_');
    var result2 = elem.id.indexOf(',');

    if (result1 != -1 && result2 != -1) {
        if (document.getElementById('WF_InvoiceInfo').value === "hidden" && document.getElementById('WF_LeaseFinal').value === "hidden") {
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
    }
}

// スプレッドのフォーカス移動設定処理
function setKeyMap() {
    var s = document.getElementById(spid);
    var kcode;
    kcode = 13;

    s.AddKeyMap(kcode, false, false, false, "element.MoveToNextCell()");
    s.AddKeyMap(kcode, false, true, false, "element.MoveToPrevCell()");
}

// ドロップダウンリスト変更
function SelectDropDownList_OnChange(ddlIDClick) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = ddlIDClick;
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

// 確認ボタン押下
function Spred_ConfBtn_click(btnIDClick) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = btnIDClick;
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

// ○決済条件検索OnLoad用処理
function KekkjSrcOnload() {

    let ele = document.getElementById('pnlKekkjSrcWrapper');
    ele.style.visibility = document.getElementById('WF_KekkjSrc').value;

    /* スプレッドシートのダブルクリックイベント紐づけ */
    var spread = document.getElementById("spdKekkjm");

    if (spread.addEventListener) {
        spread.addEventListener("dblclick", DblClickKekkjm, false);
    } else {
        spread.ondblclick = DblClickKekkjm;
    }

}

/**
 *  決済条件検索ダイアログ ボタンクリックイベント
 */
function kekkjmSrc_Click() {
    // モード
    const Mode = document.getElementById('WF_MODE').value;

    // 新規モード
    if (Mode == "1") {
        ButtonClick('WF_ButtonKekkJ');
    }
}

/**
 *  決済条件検索ダイアログ ボタンクリックイベント
 */
function IVInfokekkjmSrc_Click() {
    // モード
    ButtonClick('WF_ButtonIVInfoKekkJ');
}

/**
 *  決済条件検索ダイアログ『閉じる』ボタンクリックイベント
 */
function KekkjSrcCloseClick() {

    ButtonClick('WF_ButtonKekkjSrcCLOSE');
}

/**
 * スプレッドシート・ダブルクリック処理
 */
function DblClickKekkjm(e) {
    var e = e || window.event;
    var s = e.target || e.srcElement;
    var p1 = s.parentNode || s.parentElement;
    var p2 = s.parentNode.parentNode || s.parentElement.parentElement;
    var p3 = s.parentNode.parentNode.parentNode || s.parentElement.parentElement.parentElement;

    //属性の取得
    p1atb = p1.getAttribute("spdKekkjm");
    p2atb = p2.getAttribute("spdKekkjm");
    p3atb = p3.getAttribute("spdKekkjm");

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
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclickKekkjm";
    commonDispWait();
    document.forms[0].submit();
}

// ○SPREAD用クリック処理
function Spred_ButtonSel_click() {
    var sheet = document.getElementById("spdCtnList");
    var row = sheet.GetActiveRow();
    var col = sheet.GetActiveCol();
    var elem = document.activeElement;
    var result1 = elem.id.indexOf('_');
    var result2 = elem.id.indexOf(',');

    if (result1 != -1 && result2 != -1) {
        if (document.getElementById('WF_InvoiceInfo').value === "hidden" && document.getElementById('WF_LeaseFinal').value === "hidden") {
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = "WF_SPREAD_ButtonSel";
                //document.body.style.cursor = "wait";
                commonDispWait();
                document.forms[0].submit();
            }
        }
    }
}

// ○リース明細画面OnLoad用処理
function LeaseDatalistOnload() {

    let ele = document.getElementById('pnlLeaseDatalistWrapper');
    ele.style.visibility = document.getElementById('WF_LeaseDataList').value;
}

/**
 *  リース明細画面ダイアログ『閉じる』ボタンクリックイベント
 */
function LeaseDataListCloseClick() {

    ButtonClick('WF_ButtonLeaseDataListCLOSE');
}

// ○コンテナ検索OnLoad用処理
function ReconmSrcOnload() {

    let ele = document.getElementById('pnlReconmSrcWrapper');
    ele.style.visibility = document.getElementById('WF_ReconmList').value;

    /* スプレッドシートのダブルクリックイベント紐づけ */
    var spread = document.getElementById("spdReconm");

    if (spread.addEventListener) {
        spread.addEventListener("dblclick", DblClickReconm, false);
    } else {
        spread.ondblclick = DblClickReconm;
    }
}

/**
 *  コンテナ検索ダイアログ ボタンクリックイベント
 */
function reconmSrc_Click() {
    // モード
    const Mode = document.getElementById('WF_MODE').value;

    ButtonClick('WF_ButtonReconm');
}

/**
 *  コンテナ検索ダイアログ ボタンクリックイベント
 */
function UpdreconmSrc_Click() {
    // モード
    ButtonClick('WF_ButtonUpdReconm');
}

/**
 *  コンテナ検索ダイアログ『閉じる』ボタンクリックイベント
 */
function ReconmSrcCloseClick() {

    ButtonClick('WF_ButtonReconmSrcCLOSE');
}

// ○ファイナンス情報画面OnLoad用処理
function LeaseFinalOnload() {

    let ele = document.getElementById('pnlLeaseFinalSrcWrapper');
    ele.style.visibility = document.getElementById('WF_LeaseFinal').value;
}

/**
 *  ファイナンス情報画面ダイアログ『閉じる』ボタンクリックイベント
 */
function LeaseFinalSrcCloseClick() {

    ButtonClick('WF_ButtonLeaseFinalCLOSE');
}

// ○請求情報画面OnLoad用処理
function InvoiceInputOnload() {

    let ele = document.getElementById('pnlInvoiceInfoSrcWrapper');
    ele.style.visibility = document.getElementById('WF_InvoiceInfo').value;
}

/**
 *  請求情報画面ダイアログ『閉じる』ボタンクリックイベント
 */
function InvoiceInputSrcCloseClick() {

    ButtonClick('WF_ButtonInvoiceInputCLOSE');
}

/**
 * スプレッドシート・ダブルクリック処理
 */
function DblClickReconm(e) {
    var e = e || window.event;
    var s = e.target || e.srcElement;
    var p1 = s.parentNode || s.parentElement;
    var p2 = s.parentNode.parentNode || s.parentElement.parentElement;
    var p3 = s.parentNode.parentNode.parentNode || s.parentElement.parentElement.parentElement;

    //属性の取得
    p1atb = p1.getAttribute("spdReconm");
    p2atb = p2.getAttribute("spdReconm");
    p3atb = p3.getAttribute("spdReconm");

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
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclickReconm";
    commonDispWait();
    document.forms[0].submit();
}

// ○請求先検索OnLoad用処理
function InvoiceSrcOnload() {

    let ele = document.getElementById('pnlInvoiceSrcWrapper');
    ele.style.visibility = document.getElementById('WF_InvoiceSrc').value;

    /* スプレッドシートのダブルクリックイベント紐づけ */
    var spread = document.getElementById("spdInvoice");

    if (spread.addEventListener) {
        spread.addEventListener("dblclick", DblClickInvoice, false);
    } else {
        spread.ondblclick = DblClickInvoice;
    }

}

/**
 *  請求先検索ダイアログ ボタンクリックイベント
 */
function InvoiceSrc_Click() {
    // モード
    const Mode = document.getElementById('WF_MODE').value;

    // 新規モード
    if (Mode == "1") {
        ButtonClick('WF_ButtonInvoice');
    }
}

/**
 *  請求先検索ダイアログ ボタンクリックイベント
 */
function UpdInvoiceSrc_Click() {
    // モード
    ButtonClick('WF_ButtonUpdInvoice');
}

/**
 *  請求先検索ダイアログ『閉じる』ボタンクリックイベント
 */
function InvoiceSrcCloseClick() {

    ButtonClick('WF_ButtonInvoiceSrcCLOSE');
}

/**
 * スプレッドシート・ダブルクリック処理
 */
function DblClickInvoice(e) {
    var e = e || window.event;
    var s = e.target || e.srcElement;
    var p1 = s.parentNode || s.parentElement;
    var p2 = s.parentNode.parentNode || s.parentElement.parentElement;
    var p3 = s.parentNode.parentNode.parentNode || s.parentElement.parentElement.parentElement;

    //属性の取得
    p1atb = p1.getAttribute("spdInvoiceSrc");
    p2atb = p2.getAttribute("spdInvoiceSrc");
    p3atb = p3.getAttribute("spdInvoiceSrc");

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
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclickInvoice";
    commonDispWait();
    document.forms[0].submit();
}

/**
 * コンテナ一括入力フィールド表示・非表示クリックイベントバインド
 */
function bindShowCloseContainerBulkInputField() {

    let buttonObjs = [document.getElementById("spnShowHideContainerBulkInputField")];

    let dispValues = [document.getElementById("hdnContainerBulkInputField")];

    let classSetObjs = [document.getElementById("divContainerBulkInputField")];

    for (let i = 0; i < buttonObjs.length; i++) {
        let buttonObj = buttonObjs[i];
        let dispValue = dispValues[i];
        let classSetObj = classSetObjs[i];

        if (buttonObj === null) {
            return;
        }
        buttonObj.addEventListener('click', (function (buttonObj, dispValue, classSetObj) {
            return function () {
                if (dispValue.value === "0") {
                    dispValue.value = "1";
                } else {
                    dispValue.value = "0";
                }
                dispContainerBulkInputField(dispValue.id, classSetObj.id);
            };
        })(buttonObj, dispValue, classSetObj), true);
    }
}

/**
 * ファイナンス情報一括入力フィールド表示・非表示クリックイベントバインド
 */
function bindShowCloseFinalBulkInputField() {

    let buttonObjs = [document.getElementById("spnShowHideFinalBulkInputField")];

    let dispValues = [document.getElementById("hdnFinalBulkInputField")];

    let classSetObjs = [document.getElementById("divFinalBulkInputField")];

    for (let i = 0; i < buttonObjs.length; i++) {
        let buttonObj = buttonObjs[i];
        let dispValue = dispValues[i];
        let classSetObj = classSetObjs[i];

        if (buttonObj === null) {
            return;
        }
        buttonObj.addEventListener('click', (function (buttonObj, dispValue, classSetObj) {
            return function () {
                if (dispValue.value === "0") {
                    dispValue.value = "1";
                } else {
                    dispValue.value = "0";
                }
                dispContainerBulkInputField(dispValue.id, classSetObj.id);
            };
        })(buttonObj, dispValue, classSetObj), true);
    }
}

/**
 * コンテナ一括入力フィールド表示・非表示制御
 * @param {string} hdnId 表示非表示を保持するHiddenタグのID
 * @param {string} settingclass 表示非表示Cssクラスを設定するID
 * @return {undefined} なし
 */
function dispContainerBulkInputField(hdnId, settingclass) {
    let dispValue = document.getElementById(hdnId);
    let searchArea = document.getElementById(settingclass);
    if (dispValue === null) {
        return;
    }
    searchArea.classList.remove("show");
    searchArea.classList.remove("hide");
    if (dispValue.value === "0") {
        searchArea.classList.add("show");
    } else {
        searchArea.classList.add("hide");
    }
}

// セル値を変更したときに発生
function Spred_dataChanged(event) {
    var row = event.spread.GetActiveRow();
    var col = event.spread.GetActiveCol();

    // 契約形態が変更された場合
    if (col === 8) {
        // ポストバックデータを更新
        event.spread.UpdatePostbackData();
        if (document.getElementById("MF_SUBMIT").value === "FALSE") {
            document.getElementById("MF_SUBMIT").value = "TRUE";
            document.getElementById("WF_ButtonClick").value = "Spred_dataChanged";
            //document.body.style.cursor = "wait";
            commonDispWait();
            document.forms[0].submit();
        }
    }
}

function saveScrollPosition() {
    let detailbox = document.getElementById("detailbox");
    if (detailbox !== null) {
        document.getElementById("WF_ClickedScrollTop").value = detailbox.scrollTop;
    }
}

window.addEventListener("DOMContentLoaded", () => {
    // 全体スクロールイベントに、ポジション記録処理を付与する
    document.getElementById("detailbox").addEventListener('scroll', saveScrollPosition);
});

// 2024/08/14 杉元孝行 スポット区分追加対応 ADD START
// 〇スポット/しない切替ボタンクリック時
function btnSpotKbnDispChange(btnId, txtId) {
    if (document.getElementById(txtId).value === "1"){
        document.getElementById(btnId).value = "スポット";
    } else {
        document.getElementById(btnId).value = "なし";
    }
}

// 〇スポット/しない切替ボタンクリック時
function btnSpotKbnChange_click(btnId, txtId) {
    if (document.getElementById(btnId).value === "スポット") {
        document.getElementById(btnId).value = "なし";
        document.getElementById(txtId).value = "0";
    } else {
        document.getElementById(btnId).value = "スポット";
        document.getElementById(txtId).value = "1";
    }
}

// 契約形態ボタンクリック時
function listContraLNMode_onchange() {

    // 要素を取得
    let ele_rental = document.getElementById('WF_CONTRALNMODELIST_chklGrc0001SelectionBox_0').checked;
    // チェックされているか判定
    if (ele_rental == true) {
        // スポット区分
        document.getElementById('WF_ButtonSPOTKBN').disabled = false;
    } else {
        // スポット区分
        document.getElementById('WF_ButtonSPOTKBN').disabled = true;
        // スポット区分
        document.getElementById('WF_SpotKbn').value = "0";
        // スポット区分ボタン
        btnSpotKbnDispChange('WF_ButtonSPOTKBN', 'WF_SpotKbn');
    }

}

// 契約形態ボタンクリック時(請求情報)
function listIVInfoContraLNMode_onchange() {

    // 要素を取得
    let ele_rental = document.getElementById('WF_IVINFO_CONTRALNMODELIST_chklGrc0001SelectionBox_0').checked;
    // チェックされているか判定
    if (ele_rental == true) {
        // スポット区分
        document.getElementById('WF_ButtonIVINFO_SPOTKBN').disabled = false;
    } else {
        // スポット区分
        document.getElementById('WF_ButtonIVINFO_SPOTKBN').disabled = true;
        // スポット区分
        document.getElementById('WF_IVInfoSpotKbn').value = "0";
        // スポット区分ボタン
        btnSpotKbnDispChange('WF_ButtonIVINFO_SPOTKBN', 'WF_IVInfoSpotKbn');
    }

}
// 2024/08/14 杉元孝行 スポット区分追加対応 ADD END

// 2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD START
// 〇締日/契約終了日切替ボタンクリック時
function btnClosingdayKbnDispChange(btnId, txtId, txtIDDay) {
    if (document.getElementById(txtId).value === "1"){
        document.getElementById(btnId).value = "契約終了日";
        document.getElementById(txtIDDay).disabled = true;
        document.getElementById(txtIDDay).value = "";
    } else {
        document.getElementById(btnId).value = "締日";
        document.getElementById(txtIDDay).disabled = false;
    }
}

// 〇締日/契約終了日切替ボタンクリック時
function btnClosingdayKbnChange_click(btnId, txtId, txtIDDay) {
    if (document.getElementById(btnId).value === "契約終了日") {
        document.getElementById(btnId).value = "締日";
        document.getElementById(txtId).value = "0";
        document.getElementById(txtIDDay).disabled = false;
    } else {
        document.getElementById(btnId).value = "契約終了日";
        document.getElementById(txtId).value = "1";
        document.getElementById(txtIDDay).disabled = true;
        document.getElementById(txtIDDay).value = "";
    }
}
// 2024/08/21 杉元孝行 契約終了日を締日とする契約への対応 ADD END
