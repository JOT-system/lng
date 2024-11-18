// 画面読み込み時処理
window.onload = function () {

    // 画面制御処理
    mapSetting()
    // スプレッドのフォーカス移動設定処理
    // setKeyMap()

    RentalPopupOnload()
    LeasePopupOnload()
    KekkjSrcOnload()
    TrusteeSrcOnload()
    CommentPopupOnload()
    MessagePopupOnload()
    messageSetdisplay()

    if (document.getElementById('WF_RENTALADD_FLG').value == "1") {
        invtypeEventRent();
    } else {
        FixEventRent();
    };
    
    if (document.getElementById('WF_LEASEADD_FLG').value == "1"){
        invtypeEventLease();
    } else {
        FixEventLease();
    };

};

// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {
    // スクロール位置を復元 
    if (document.getElementById("detailbox") !== null) {
        document.getElementById("detailbox").scrollTop = document.getElementById("WF_ClickedScrollTop").value;
    }

    let tbl_rental = document.getElementById("tblWrapper_Rental").getElementsByTagName("div");
    if (tbl_rental[0] !== null) {
        tbl_rental[0].scrollTop = document.getElementById("WF_ClickedScrollTop_rent").value;
        tbl_rental[0].scrollLeft = document.getElementById("WF_ClickedScrollLeft_rent").value;
    };

    let tbl_lease = document.getElementById("tblWrapper_Lease").getElementsByTagName("div");
    if (tbl_lease[0] !== null) {
        tbl_lease[0].scrollTop = document.getElementById("WF_ClickedScrollTop_les").value;
    };

    let wrapper_ctn = document.getElementById("tblWrapper_Container");
    if (wrapper_ctn !== null) {
        let tbl_container = document.getElementById("tblWrapper_Container").getElementsByTagName("div");
        if (tbl_container[0] !== null) {
            tbl_container[0].scrollTop = document.getElementById("WF_ClickedScrollTop_ctn").value;
        };
    };

    let tbl_history = document.getElementById("tblWrapper_History").getElementsByTagName("div");
    if (tbl_history[0] !== null) {
        tbl_history[0].scrollTop = document.getElementById("WF_ClickedScrollTop_hist").value;
        tbl_history[0].scrollLeft = document.getElementById("WF_ClickedScrollLeft_hist").value;
    };
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
        /* 表示位置を指定 */
        var rect = document.getElementById("LF_LEFTBOX").getBoundingClientRect();
        var objRect = document.getElementById(document.getElementById("WF_FIELD").value).getBoundingClientRect();
        /* オブジェクトの座標＋高さ＋検索BOXの高さがウインドウのビューポートの下端を超える場合は */
        /* オブジェクトの上に検索BOXを表示する */
        if ((objRect.top + objRect.height + rect.height) > window.innerHeight && (objRect.top - rect.height) > 0) {
            document.getElementById("LF_LEFTBOX").style.top = (objRect.top - rect.height) + "px";
        } else {
            /* 通常はオブジェクトの真下に表示する */
            document.getElementById("LF_LEFTBOX").style.top = (objRect.top + objRect.height) + "px";
        }
        document.getElementById("WF_saveLeft").value = 645;
       commonDispWait();
       document.forms[0].submit();
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

// 画面制御処理
function mapSetting() {
    //// 担当者・確認者制御フラグ
    //const staffFlg = document.getElementById('WF_StaffFlg').value;

    //// 担当者の場合
    //if (staffFlg == '1') {
    //    // 確認依頼ボタンを表示
    //    document.getElementById('WF_ButtonRequest').style.display = 'inline'
    //    // 取り下げボタンを表示
    //    document.getElementById('WF_ButtonWithdrawn').style.display = 'inline'
    //    // 確認済みボタンを非表示
    //    document.getElementById('WF_ButtonConfirmed').style.display = 'none'
    //    // 差し戻しボタンを非表示
    //    document.getElementById('WF_ButtonRemand').style.display = 'none'
    //} else {
    //    // 確認依頼ボタンを非表示
    //    document.getElementById('WF_ButtonRequest').style.display = 'none'
    //    // 取り下げボタンを非表示
    //    document.getElementById('WF_ButtonWithdrawn').style.display = 'none'
    //    // 確認済みボタンを表示
    //    document.getElementById('WF_ButtonConfirmed').style.display = 'inline'
    //    // 差し戻しボタンを表示
    //    document.getElementById('WF_ButtonRemand').style.display = 'inline'
    //};

    // 清算データ存在フラグ
    var rentalTotal = document.getElementById('WF_RentalTotal').value;

    // 清算データが存在しない場合、表示制限
    if (rentalTotal == "0") {
        // 使用料料合計
        document.getElementById('divRental').style.display = 'none';
        document.getElementById('spanRental').style.display = 'none';
    };

    // リースデータ存在フラグ
    var leaseTotal = document.getElementById('WF_LeaseTotal').value;

    // リースが存在しない場合、表示制限
    if (leaseTotal == "0") {
        // リース料合計
        document.getElementById('divLease').style.display = 'none';
        document.getElementById('spanLease').style.display = 'none';
    };

    // 手書きデータ存在フラグ
    var WriteTotal = document.getElementById('WF_WriteTotal').value;

    // 手書きが存在しない場合、表示制限
    if (WriteTotal == "0") {
        // リース料合計
        document.getElementById('divWrite').style.display = 'none';
        document.getElementById('spanWrite').style.display = 'none';
    };

    // 売却在庫データ存在フラグ
    var CtnTotal = document.getElementById('WF_CtnTotal').value;

    // 売却在庫が存在しない場合、表示制限
    if (CtnTotal == "0") {
        // リース料合計
        document.getElementById('divCtn').style.display = 'none';
        document.getElementById('spanCtn').style.display = 'none';
    };

    // 使用料ポップアップ処理切り替えフラグ
    const RentAddFlg = document.getElementById('WF_RENTALADD_FLG').value;

    // 追加の場合
    if (RentAddFlg == '1') {
        // 追加ボタンを表示
        document.getElementById('btnAddRow_Rental').style.display = 'inline'
        // 修正ボタンを非表示
        document.getElementById('btnFixRow_Rental').style.display = 'none'
    } else if (RentAddFlg == "0") {
        // 追加ボタンを非表示
        document.getElementById('btnAddRow_Rental').style.display = 'none'
        // 修正ボタンを表示
        document.getElementById('btnFixRow_Rental').style.display = 'inline'
    };

    // リース料ポップアップ処理切り替えフラグ
    const LeaseAddFlg = document.getElementById('WF_LEASEADD_FLG').value;

    // 追加の場合
    if (LeaseAddFlg == '1') {
        // 追加ボタンを表示
        document.getElementById('btnAddRow_Lease').style.display = 'inline'
        // 修正ボタンを非表示
        document.getElementById('btnFixRow_Lease').style.display = 'none'
    } else if (LeaseAddFlg == "0") {
        // 追加ボタンを非表示
        document.getElementById('btnAddRow_Lease').style.display = 'none'
        // 修正ボタンを表示
        document.getElementById('btnFixRow_Lease').style.display = 'inline'
    };

    // 請求先切り替えフラグ
    const TrusteeFlg = document.getElementById('WF_TRUSTEE_FLG').value;

    // 追加の場合
    if (TrusteeFlg == '1') {
        // 請求先(検索)表示
        document.getElementById('WF_DEPTRUSTEE').style.display = 'inline'
        document.getElementById('TxtDeptrusteeName').style.display = 'inline'
        document.getElementById('TxtDeptrusteeCode').style.display = 'inline'
        document.getElementById('WF_kekkjm').style.display = 'inline'
        // 請求先(名称のみ)非表示
        document.getElementById('TxtToricode').style.display = 'none'
    } else if (TrusteeFlg == "0") {
        // 請求先(検索)非表示
        document.getElementById('WF_DEPTRUSTEE').style.display = 'none'
        document.getElementById('TxtDeptrusteeName').style.display = 'none'
        document.getElementById('TxtDeptrusteeCode').style.display = 'none'
        document.getElementById('WF_kekkjm').style.display = 'none'
        // 請求先(名称のみ)表示
        document.getElementById('TxtToricode').style.display = 'inline'
    };

    // 決裁条件切り替えフラグ
    const KessaiFlg = document.getElementById('WF_KESSAI_FLG').value;

    // 追加の場合
    if (KessaiFlg == '1') {
        
        document.getElementById('WF_lblBank').style.display = 'inline'
        document.getElementById('WF_lblBankBranch').style.display = 'inline'
        document.getElementById('WF_lblDepositDate').style.display = 'inline'
        document.getElementById('TxtBankNm').style.display = 'inline'
        document.getElementById('TxtBankBranchNm').style.display = 'inline'
        document.getElementById('TxtDepositDate').style.display = 'inline'
    } else if (KessaiFlg == "0") {
        
        document.getElementById('WF_lblBank').style.display = 'none'
        document.getElementById('WF_lblBankBranch').style.display = 'none'
        document.getElementById('WF_lblDepositDate').style.display = 'none'
        document.getElementById('TxtBankNm').style.display = 'none'
        document.getElementById('TxtBankBranchNm').style.display = 'none'
        document.getElementById('TxtDepositDate').style.display = 'none'
    };
}

// ○行追加OnLoad用処理
function RentalPopupOnload() {

    let ele = document.getElementById('pnlRentalAddAmountSrcWrapper');
    ele.style.visibility = document.getElementById('WF_RENTAL_POPUP').value;
}

/**
 *  行追加ダイアログ『閉じる』ボタンクリックイベント
 */
function RentalPopupCloseClick() {

    ButtonClick('WF_ButtonRentalPopupCLOSE');
}

// ○行追加OnLoad用処理
function LeasePopupOnload() {

    let ele = document.getElementById('pnlLeaseAddAmountSrcWrapper');
    ele.style.visibility = document.getElementById('WF_LEASE_POPUP').value;
}

/**
 *  行追加ダイアログ『閉じる』ボタンクリックイベント
 */
function LeasePopupCloseClick() {

    ButtonClick('WF_ButtonLeasePopupCLOSE');
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
 *  決済条件検索ダイアログ ボタンクリックイベント
 */
function kekkjmSrc_Click() {

    ButtonClick('WF_ButtonKekkJ');
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
    var spread = document.getElementById("spdKekkjm");
    var row = spread.GetActiveRow();
    var col = spread.GetActiveCol();

    //選択行を非表示項目にセット
    hidRowIndex.value = row;

    if (row == -1 || col == -1) {
        return
    }

    //サブミット
    document.getElementById("MF_SUBMIT").value = "TRUE";
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclickKekkjm";
    commonDispWait();
    document.forms[0].submit();
}

/**
 *  発受託人検索ダイアログ ボタンクリックイベント
 */
function DeptrusteeSrc_Click() {

    ButtonClick('WF_ButtonDeptrustee');
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

    if (row == -1 || col == -1) {
        return
    }

    //処理
    var spread = document.getElementById("spdTrustee");
    var row = spread.GetActiveRow();
    var col = spread.GetActiveCol();

    //選択行を非表示項目にセット
    hidRowIndex.value = row;

    //サブミット
    document.getElementById("MF_SUBMIT").value = "TRUE";
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclickTrustee";
    commonDispWait();
    document.forms[0].submit();
}

// ○摘要OnLoad用処理
function CommentPopupOnload() {

    let ele = document.getElementById('pnlCommentRichTextWrapper');
    ele.style.visibility = document.getElementById('WF_CommentSrc').value;
}

/**
 *  摘要ダイアログ『OK』ボタンクリックイベント
 */
function CommentSrcOKClick() {

    ButtonClick('WF_ButtonCommentSrcOK');
}

/**
 *  摘要ダイアログ『閉じる』ボタンクリックイベント
 */
function CommentSrcCloseClick() {

    ButtonClick('WF_ButtonCommentSrcCLOSE');
}

// ○通信欄OnLoad用処理
function MessagePopupOnload() {

    let ele = document.getElementById('pnlMessageRichTextWrapper');
    ele.style.visibility = document.getElementById('WF_MessageSrc').value;
}

/**
 *  通信欄ダイアログ『OK』ボタンクリックイベント
 */
function MessageSrcOKClick() {

    ButtonClick('WF_ButtonMessageSrcOK');
}

/**
 *  通信欄ダイアログ『閉じる』ボタンクリックイベント
 */
function MessageSrcCloseClick() {

    ButtonClick('WF_ButtonMessageSrcCLOSE');
}

function ddlSelectAmountType_OnChange(ddlIDClick) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_SelectAmountType_OnChange";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

function ddlSelectAmountType_Lease_OnChange(ddlIDClick) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_SelectAmountType_Lease_OnChange";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

function ddlTaxKbn_OnChange() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_TaxKbn_OnChange";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

function ddlTaxKbn_Lease_OnChange() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_TaxKbn_Lease_OnChange";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

/**
 *  使用料グリッドダブルクリックイベント
 * @param {any} lineCnt     'DataTable対象行
 */
function ListDbClick_Rental(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_SpreadDBclick_Rent";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  リースグリッドダブルクリックイベント
 * @param {any} lineCnt     'DataTable対象行
 */
function ListDbClick_Lease(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_SpreadDBclick_Lease";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  使用料行削除ボタンクリックイベント
 * @param {any} lineCnt     'DataTable対象行
 */
function DeleteBtnClick_Rental(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_deleteButton_Rental";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  リース行削除ボタンクリックイベント
 * @param {any} lineCnt     'DataTable対象行
 */
function DeleteBtnClick_Lease(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_deleteButton_Lease";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  使用料ドロップダウン変更イベント
 * @param {any} lineCnt     'DataTable対象行
 */
function Ddbchanged_Rental(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "cmbbxchanged_rental";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  リースドロップダウン変更イベント
 * @param {any} lineCnt     'DataTable対象行
 */
function Ddbchanged_Lease(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "cmbbxchanged_lease";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  ドラフト連携ボタン押下イベント
 * @param {any} lineCnt     'DataTable対象行
 */
function DraftRenkeiBtnClick(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonDraftRENKEI";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  申請ボタン押下イベント
 * @param {any} lineCnt     'DataTable対象行
 */
function RequestBtnClick(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonRequest";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  取下ボタン押下イベント
 * @param {any} lineCnt     'DataTable対象行
 */
function WithdrawnBtnClick(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonWithdrawn";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  承認ボタン押下イベント
 * @param {any} lineCnt     'DataTable対象行
 */
function ConfirmedBtnClick(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonConfirmed";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  却下ボタン押下イベント
 * @param {any} lineCnt     'DataTable対象行
 */
function RemandBtnClick(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonRemand";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 *  正式連携ボタン押下イベント
 * @param {any} lineCnt     'DataTable対象行
 */
function RenkeiBtnClick(lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonRENKEI";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * LEFTBOX用処理カスタム
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} lineCnt     'DataTable対象行
 * @param {any} listNo      'リスト番号
 */
function FD_DBclick(fieldId, lineCnt, listNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        // クリック位置取得
        var elem = document.getElementById(fieldId);
        var rect = elem.getBoundingClientRect();
        document.getElementById("WF_saveTop").value = rect.top + rect.height;
        document.getElementById("WF_saveLeft").value = rect.left;

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;
        document.getElementById('WF_LeftMViewChange').value = listNo;
        document.getElementById('WF_LeftboxOpen').value = "Open";

        document.getElementById("WF_ButtonClick").value = "WF_Field_DBClick";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * LEFTBOX用処理カスタム
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} lineCnt     'DataTable対象行
 */
function FD_Commentclick(fieldId, lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        // クリック位置取得
        var elem = document.getElementById(fieldId);
        var rect = elem.getBoundingClientRect();
        document.getElementById("WF_saveTop").value = rect.top + rect.height;
        document.getElementById("WF_saveLeft").value = rect.left;

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonCOMMENT";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * LEFTBOX用処理カスタム
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} lineCnt     'DataTable対象行
 */
function FD_Messageclick(fieldId, lineCnt) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        // クリック位置取得
        var elem = document.getElementById(fieldId);
        var rect = elem.getBoundingClientRect();
        document.getElementById("WF_saveTop").value = rect.top + rect.height;
        document.getElementById("WF_saveLeft").value = rect.left;

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonMESSAGE";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

/**
 * レンタル単価・個数変更時
 */
function txtunitprice_OnChange() {
    var unitprice = document.getElementById("txtunitprice");
    var quantity = document.getElementById("Txtquantityrental");
    var fee = document.getElementById("txtfee");

    //全角→半角変換
    unitprice.value = unitprice.value.replace(/[！-～]/g,
        function (tmpStr) {
            // 文字コードをシフト
            return String.fromCharCode(tmpStr.charCodeAt(0) - 0xFEE0);
        }
    );
    quantity.value = quantity.value.replace(/[！-～]/g,
        function (tmpStr) {
            // 文字コードをシフト
            return String.fromCharCode(tmpStr.charCodeAt(0) - 0xFEE0);
        }
    );

    //数値変換
    var unitprice_Num = Number(unitprice.value);
    var quantity_Num = Number(quantity.value); 

    if (isNaN(unitprice_Num)) {
        fee.value = "0"
    } else {
        if (unitprice_Num == 0) {
            fee.value = "0"
        } else {
            if (isNaN(quantity_Num)) {
                fee.value = unitprice_Num
            } else {
                if (quantity_Num == 0) {
                    fee.value = unitprice_Num
                } else {
                    fee.value = unitprice_Num * quantity_Num
                }
            }
        }
    }
}

/**
 * リース単価・個数変更時
 */
function txtunitprice_lease_OnChange() {
    var unitprice = document.getElementById("txtunitprice_Lease");
    var quantity = document.getElementById("Txtquantitylease");
    var fee = document.getElementById("txtfee_Lease");

    //全角→半角変換
    unitprice.value = unitprice.value.replace(/[！-～]/g,
        function (tmpStr) {
            // 文字コードをシフト
            return String.fromCharCode(tmpStr.charCodeAt(0) - 0xFEE0);
        }
    );
    quantity.value = quantity.value.replace(/[！-～]/g,
        function (tmpStr) {
            // 文字コードをシフト
            return String.fromCharCode(tmpStr.charCodeAt(0) - 0xFEE0);
        }
    );

    //数値変換
    var unitprice_Num = Number(unitprice.value);
    var quantity_Num = Number(quantity.value); 

    if (isNaN(unitprice_Num)) {
        fee.value = "0"
    } else {
        if (unitprice_Num == 0) {
            fee.value = "0"
        } else {
            if (isNaN(quantity_Num)) {
                fee.value = unitprice_Num
            } else {
                if (quantity_Num == 0) {
                    fee.value = unitprice_Num
                } else {
                    fee.value = unitprice_Num * quantity_Num
                }
            }
        }
    }
}

// ○一括ダウンロード処理
function f_draftInvoiceDownload() {
    var objPrintUrl = document.getElementById("WF_PrintURL1").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL2").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL3").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL4").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    return false;
}

/**
 * ダウンロード処理
 * @param {string} url ダウンロードURL
 * @return {undefined} なし
 * @description 
 */
function commonDownload(url) {
    if ('1' === url) {
        //IEの場合
        window.open(url + '?rnd=' + new Date().getTime(), "view", "_blank");
    } else {
        // IE以外の場合
        var fileName = url.substring(url.lastIndexOf('/') + 1);
        let nondecodeFileName = fileName;
        fileName = decodeURIComponent(fileName);
        // リンク（<a>要素）を生成し、JavaScriptからクリックする
        var link = document.createElement("a");
        // キャッシュされたファイルをダウンロード扱いしないためURLパラメータをダミーで付与
        if (url.indexOf('?') === -1) {
            link.href = url + '?rnd=' + new Date().getTime();
        } else {
            link.href = url;
        }

        link.id = 'commondownloaddummylink';
        //link.download = fileName;
        link.setAttribute('download', nondecodeFileName);
        link.target = '_blank';
        link.innerText = 'dl';
        link.style.display = 'none';
        link.type = 'application/octet-stream';
        link.rel = 'noopener noreferrer';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
}

function saveScrollPosition() {
    let detailbox = document.getElementById("detailbox");
    if (detailbox !== null) {
        document.getElementById("WF_ClickedScrollTop").value = detailbox.scrollTop;
    }
}

function saveScrollPosition_Rent() {
    let tbl_rental = document.getElementById("tblWrapper_Rental").getElementsByTagName("div")
    if (tbl_rental !== null) {
        document.getElementById("WF_ClickedScrollTop_rent").value = tbl_rental[0].scrollTop;
        document.getElementById("WF_ClickedScrollLeft_rent").value = tbl_rental[0].scrollLeft;
    }
}

function saveScrollPosition_Les() {
    let tbl_les = document.getElementById("tblWrapper_Lease").getElementsByTagName("div")
    if (tbl_les !== null) {
        document.getElementById("WF_ClickedScrollTop_les").value = tbl_les[0].scrollTop;
    }
}

function saveScrollPosition_Ctn() {
    let tbl_ctn = document.getElementById("tblWrapper_Container").getElementsByTagName("div")
    if (tbl_ctn !== null) {
        document.getElementById("WF_ClickedScrollTop_ctn").value = tbl_ctn[0].scrollTop;
    }
}

function saveScrollPosition_Hist() {
    let tbl_hist = document.getElementById("tblWrapper_History").getElementsByTagName("div")
    if (tbl_hist !== null) {
        document.getElementById("WF_ClickedScrollTop_hist").value = tbl_hist[0].scrollTop;
        document.getElementById("WF_ClickedScrollLeft_hist").value = tbl_hist[0].scrollLeft;
    }
}

function messageSetdisplay() {
    let ele_rental = document.getElementById('divmessagerental');
    let message_rental = document.getElementById('lblmessage_rental').value;
    let ele_lease = document.getElementById('divmessagelease');
    let message_lease = document.getElementById('lblmessage_lease').value;
    if (message_rental == "") {
        ele_rental.style.display = 'none';
    }
    if (message_lease == "") {
        ele_lease.style.display = 'none';
    }
}

function invtypeEventRent() {
    let ele_rental = document.getElementById('WF_INVTYPE_DTL_RENT_chklGrc0001SelectionBox_0').checked;
     if (ele_rental == true) {
         document.getElementById('Txtquantityrental').disabled = "disabled";
         document.getElementById('Txtunitrental').disabled = "disabled";
         document.getElementById('Txtquantityrental').value = "";
         document.getElementById('Txtunitrental').value = "";
     } else {
         document.getElementById('Txtquantityrental').disabled = "";
         document.getElementById('Txtunitrental').disabled = "";
    }
    let ele_lease = document.getElementById('WF_INVTYPE_DTL_RENT_chklGrc0001SelectionBox_1').checked;
    if (ele_lease == true) {
        document.getElementById('Txtquantityrental').disabled = "disabled";
        document.getElementById('Txtunitrental').disabled = "disabled";
        document.getElementById('Txtquantityrental').value = "";
        document.getElementById('Txtunitrental').value = "";
    } else {
        document.getElementById('Txtquantityrental').disabled = "";
        document.getElementById('Txtunitrental').disabled = "";
    }
    let ele_write = document.getElementById('WF_INVTYPE_DTL_RENT_chklGrc0001SelectionBox_2').checked;
    if (ele_write == true) {
        document.getElementById('quantityspanrental').style.display = "inline";
        document.getElementById('quantitydivrental').style.display = "inline";
        document.getElementById('unitspanrental').style.display = "inline";
        document.getElementById('unitdivrental').style.display = "inline";
        document.getElementById('quantityspanrental2').style.display = "inline";
        document.getElementById('WF_QUANTITY_RENTAL').style.display = "inline";
        document.getElementById('unitspanrental2').style.display = "inline";
        document.getElementById('WF_UNIT_RENTAL').style.display = "inline";
        document.getElementById('Txtquantityrental').disabled = "";
        document.getElementById('Txtunitrental').disabled = "";
    } else {
        document.getElementById('quantityspanrental').style.display = "none";
        document.getElementById('quantitydivrental').style.display = "none";
        document.getElementById('unitspanrental').style.display = "none";
        document.getElementById('unitdivrental').style.display = "none";
        document.getElementById('quantityspanrental2').style.display = "none";
        document.getElementById('WF_QUANTITY_RENTAL').style.display = "none";
        document.getElementById('unitspanrental2').style.display = "none";
        document.getElementById('WF_UNIT_RENTAL').style.display = "none";
        document.getElementById('Txtquantityrental').disabled = "disabled";
        document.getElementById('Txtunitrental').disabled = "disabled";

        var unitprice = document.getElementById("txtunitprice");
        var fee = document.getElementById("txtfee");
        var unitprice_Num = Number(unitprice.value);

        if (isNaN(unitprice_Num)) {
            fee.value = "0"
        } else {
            fee.value = unitprice_Num
        }
    }
 }

function FixEventRent() {
    let ele_rental = document.getElementById('WF_INVTYPE_DTL_RENT_chklGrc0001SelectionBox_0').checked;
    if (ele_rental == true) {
        document.getElementById('Txtquantityrental').disabled = "disabled";
        document.getElementById('Txtunitrental').disabled = "disabled";
    } else {
        document.getElementById('Txtquantityrental').disabled = "";
        document.getElementById('Txtunitrental').disabled = "";
    }
    let ele_lease = document.getElementById('WF_INVTYPE_DTL_RENT_chklGrc0001SelectionBox_1').checked;
    if (ele_lease == true) {
        document.getElementById('Txtquantityrental').disabled = "disabled";
        document.getElementById('Txtunitrental').disabled = "disabled";
    } else {
        document.getElementById('Txtquantityrental').disabled = "";
        document.getElementById('Txtunitrental').disabled = "";
    }
    let ele_write = document.getElementById('WF_INVTYPE_DTL_RENT_chklGrc0001SelectionBox_2').checked;
    if (ele_write == true) {
        document.getElementById('quantityspanrental').style.display = "inline";
        document.getElementById('quantitydivrental').style.display = "inline";
        document.getElementById('unitspanrental').style.display = "inline";
        document.getElementById('unitdivrental').style.display = "inline";
        document.getElementById('quantityspanrental2').style.display = "inline";
        document.getElementById('WF_QUANTITY_RENTAL').style.display = "inline";
        document.getElementById('unitspanrental2').style.display = "inline";
        document.getElementById('WF_UNIT_RENTAL').style.display = "inline";
        document.getElementById('Txtquantityrental').disabled = "";
        document.getElementById('Txtunitrental').disabled = "";
    } else {
        document.getElementById('quantityspanrental').style.display = "none";
        document.getElementById('quantitydivrental').style.display = "none";
        document.getElementById('unitspanrental').style.display = "none";
        document.getElementById('unitdivrental').style.display = "none";
        document.getElementById('quantityspanrental2').style.display = "none";
        document.getElementById('WF_QUANTITY_RENTAL').style.display = "none";
        document.getElementById('unitspanrental2').style.display = "none";
        document.getElementById('WF_UNIT_RENTAL').style.display = "none";
        document.getElementById('Txtquantityrental').disabled = "disabled";
        document.getElementById('Txtunitrental').disabled = "disabled";
    }
}

function invtypeEventLease() {
    let ele_rental = document.getElementById('WF_INVTYPE_DTL_LES_chklGrc0001SelectionBox_0').checked;
     if (ele_rental == true) {
         document.getElementById('Txtquantitylease').disabled = "disabled";
         document.getElementById('Txtunitlease').disabled = "disabled";
         document.getElementById('Txtquantitylease').value = "";
         document.getElementById('Txtunitlease').value = "";
     } else {
         document.getElementById('Txtquantitylease').disabled = "";
         document.getElementById('Txtunitlease').disabled = "";
    }
    let ele_lease = document.getElementById('WF_INVTYPE_DTL_LES_chklGrc0001SelectionBox_1').checked;
    if (ele_lease == true) {
        document.getElementById('Txtquantitylease').disabled = "disabled";
        document.getElementById('Txtunitlease').disabled = "disabled";
        document.getElementById('Txtquantitylease').value = "";
        document.getElementById('Txtunitlease').value = "";
    } else {
        document.getElementById('Txtquantitylease').disabled = "";
        document.getElementById('Txtunitlease').disabled = "";
    }
    let ele_write = document.getElementById('WF_INVTYPE_DTL_LES_chklGrc0001SelectionBox_2').checked;
    if (ele_write == true) {
        document.getElementById('quantityspanlease').style.display = "inline";
        document.getElementById('quantitydivlease').style.display = "inline";
        document.getElementById('unitspanlease').style.display = "inline";
        document.getElementById('unitdivlease').style.display = "inline";
        document.getElementById('quantityspanlease2').style.display = "inline";
        document.getElementById('WF_QUANTITY_LEASE').style.display = "inline";
        document.getElementById('unitspanlease2').style.display = "inline";
        document.getElementById('WF_UNIT_LEASE').style.display = "inline";
        document.getElementById('Txtquantitylease').disabled = "";
        document.getElementById('Txtunitlease').disabled = "";
    } else {
        document.getElementById('quantityspanlease').style.display = "none";
        document.getElementById('quantitydivlease').style.display = "none";
        document.getElementById('unitspanlease').style.display = "none";
        document.getElementById('unitdivlease').style.display = "none";
        document.getElementById('quantityspanlease2').style.display = "none";
        document.getElementById('WF_QUANTITY_LEASE').style.display = "none";
        document.getElementById('unitspanlease2').style.display = "none";
        document.getElementById('WF_UNIT_LEASE').style.display = "none";
        document.getElementById('Txtquantitylease').disabled = "disabled";
        document.getElementById('Txtunitlease').disabled = "disabled";

        var unitprice = document.getElementById("txtunitprice_Lease");
        var fee = document.getElementById("txtfee_Lease");
        var unitprice_Num = Number(unitprice.value);

        if (isNaN(unitprice_Num)) {
            fee.value = "0"
        } else {
            fee.value = unitprice_Num
        }
    }
 }

function FixEventLease() {
    let ele_rental = document.getElementById('WF_INVTYPE_DTL_LES_chklGrc0001SelectionBox_0').checked;
    if (ele_rental == true) {
        document.getElementById('Txtquantitylease').disabled = "disabled";
        document.getElementById('Txtunitlease').disabled = "disabled";
    } else {
        document.getElementById('Txtquantitylease').disabled = "";
        document.getElementById('Txtunitlease').disabled = "";
    }
    let ele_lease = document.getElementById('WF_INVTYPE_DTL_LES_chklGrc0001SelectionBox_1').checked;
    if (ele_lease == true) {
        document.getElementById('Txtquantitylease').disabled = "disabled";
        document.getElementById('Txtunitlease').disabled = "disabled";
    } else {
        document.getElementById('Txtquantitylease').disabled = "";
        document.getElementById('Txtunitlease').disabled = "";
    }
    let ele_write = document.getElementById('WF_INVTYPE_DTL_LES_chklGrc0001SelectionBox_2').checked;
    if (ele_write == true) {
        document.getElementById('quantityspanlease').style.display = "inline";
        document.getElementById('quantitydivlease').style.display = "inline";
        document.getElementById('unitspanlease').style.display = "inline";
        document.getElementById('unitdivlease').style.display = "inline";
        document.getElementById('quantityspanlease2').style.display = "inline";
        document.getElementById('WF_QUANTITY_LEASE').style.display = "inline";
        document.getElementById('unitspanlease2').style.display = "inline";
        document.getElementById('WF_UNIT_LEASE').style.display = "inline";
        document.getElementById('Txtquantitylease').disabled = "";
        document.getElementById('Txtunitlease').disabled = "";
    } else {
        document.getElementById('quantityspanlease').style.display = "none";
        document.getElementById('quantitydivlease').style.display = "none";
        document.getElementById('unitspanlease').style.display = "none";
        document.getElementById('unitdivlease').style.display = "none";
        document.getElementById('quantityspanlease2').style.display = "none";
        document.getElementById('WF_QUANTITY_LEASE').style.display = "none";
        document.getElementById('unitspanlease2').style.display = "none";
        document.getElementById('WF_UNIT_LEASE').style.display = "none";
        document.getElementById('Txtquantitylease').disabled = "disabled";
        document.getElementById('Txtunitlease').disabled = "disabled";
    }
}

window.addEventListener("DOMContentLoaded", () => {
    // 全体スクロールイベントに、ポジション記録処理を付与する
    document.getElementById("detailbox").addEventListener('scroll', saveScrollPosition);

    let divlist_rent = document.getElementById("tblWrapper_Rental").getElementsByTagName("div");
    divlist_rent[0].addEventListener('scroll', saveScrollPosition_Rent);

    let divlist_les = document.getElementById("tblWrapper_Lease").getElementsByTagName("div");
    divlist_les[0].addEventListener('scroll', saveScrollPosition_Les);

    let wrapper_ctn = document.getElementById("tblWrapper_Container");
    if (wrapper_ctn !== null) {
        let divlist_ctn = document.getElementById("tblWrapper_Container").getElementsByTagName("div");
        divlist_ctn[0].addEventListener('scroll', saveScrollPosition_Ctn);
    };

    let divlist_hist = document.getElementById("tblWrapper_History").getElementsByTagName("div");
    divlist_hist[0].addEventListener('scroll', saveScrollPosition_Hist);
});