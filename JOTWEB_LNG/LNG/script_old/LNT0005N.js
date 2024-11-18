//************************************************************
// 画面名称：リース申請登録　詳細画面
// 作成日：2024/09/10
// 作成者：杉元　孝行
// 更新日：2024/09/10
// 更新者：杉元　孝行
//
//修正履歴：
// 
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

    // 担当者・確認者制御フラグ
    const staffFlg = document.getElementById('WF_STAFF_FLG').value;
    // モード
    const Mode  = document.getElementById('WF_MODE').value;
    // 戻るボタン表示不可フラグ
    const endDisabledFlg = document.getElementById('WF_ENDBtnDisabledFlg').value;
    // コンテナ部フラグ
    const ctnFlg = document.getElementById('WF_CTN_FLG').value;
    // ログインステータス
    const loginStatus = document.getElementById('WF_LOGIN_STATUS').value;
    // 一覧から来たかのフラグ
    const applListFlg = document.getElementById('WF_ApplListFlg').value;

    // ボタンの入力制御 他はクライアント側で制御
    // 請求先、請求先部門　入力行のパネル
    var input_pnlDetailboxLine3 = document.getElementById("pnlDetailboxLine3").getElementsByClassName("btn-stickyDetail");
    // リース開始日　入力行のパネル
    var pnlLineInputLeaseDate = document.getElementById("pnlLineInputLeaseDate").getElementsByClassName("btn-stickyDetail");
    // コンテナ一覧　ボタンのパネル
    var pnlCtnListBtn = document.getElementById("pnlCtnListBtn").getElementsByClassName("btn-sticky");
    // 添付ファイル一覧　ボタンのパネル
    var pnlFileUploadBtn = document.getElementById("pnlFileUpload").getElementsByClassName("btn-sticky");

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

        // 添付ファイル一覧　ボタン行のパネル
        for (var i = 0; i < pnlFileUploadBtn.length; i++) {
            pnlFileUploadBtn[i].disabled = false;
        }
        
        if (document.getElementById('WF_APPL_STATUS').value == "" || document.getElementById('WF_APPL_STATUS').value == "0"){
            // 自動更新ボタン
            document.getElementById('WF_ButtonAUTOCALC').disabled = false;
            // コンテナ番号一括アップロード
            document.getElementById('WF_ButtonUPLOAD').disabled = false;
            // コンテナ追加ボタン
            document.getElementById('WF_ButtonCTNNO_ADD').disabled = false;
            // 一括請求ボタン
            document.getElementById('WF_ButtonINVOICEALL').disabled = false;
            // 契約形態ボタン
            document.getElementById('WF_ButtonCONTRACTMODE').disabled = false;
            // 改造費リース区分ボタン
            document.getElementById('WF_ButtonREMODELLEASEKBN').disabled = false;
            // スポット区分ボタン
            document.getElementById('WF_ButtonSPOTKBN').disabled = false;
            // 締日区分ボタン
            document.getElementById('WF_ButtonCLOSINGDAYKBN').disabled = false;
            // 行削除ボタン
            document.getElementById('WF_Button_LINE_DEL').disabled = false;
            // 全選択ボタン
            document.getElementById('WF_Button_SEL_ALL').disabled = false;
            // 全解除ボタン
            document.getElementById('WF_Button_SEL_CANCEL').disabled = false;

        } else {
            // 自動更新ボタン
            document.getElementById('WF_ButtonAUTOCALC').disabled = true;
            // コンテナ番号一括アップロード
            document.getElementById('WF_ButtonUPLOAD').disabled = true;
            // コンテナ追加ボタン
            document.getElementById('WF_ButtonCTNNO_ADD').disabled = true;
            // 一括請求ボタン
            document.getElementById('WF_ButtonINVOICEALL').disabled = true;
            // 契約形態ボタン
            document.getElementById('WF_ButtonCONTRACTMODE').disabled = true;
            // 改造費リース区分ボタン
            document.getElementById('WF_ButtonREMODELLEASEKBN').disabled = true;
            // スポット区分ボタン
            document.getElementById('WF_ButtonSPOTKBN').disabled = true;
            // 締日区分ボタン
            document.getElementById('WF_ButtonCLOSINGDAYKBN').disabled = true;
            // 行削除ボタン
            document.getElementById('WF_Button_LINE_DEL').disabled = true;
            // 全選択ボタン
            document.getElementById('WF_Button_SEL_ALL').disabled = true;
            // 全解除ボタン
            document.getElementById('WF_Button_SEL_CANCEL').disabled = true;

            if (ctnFlg == '1' && document.getElementById('WF_APPL_STATUS').value == "2"){
                // 全選択ボタン
                document.getElementById('WF_Button_SEL_ALL').disabled = false;
                // 全解除ボタン
                document.getElementById('WF_Button_SEL_CANCEL').disabled = false;
            }
        }

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

        // 添付ファイル一覧　ボタン行のパネル
        for (var i = 0; i < pnlFileUploadBtn.length; i++) {
            pnlFileUploadBtn[i].disabled = true;
        }

        // 自動更新ボタン
        document.getElementById('WF_ButtonAUTOCALC').disabled = true;
        // コンテナ番号一括アップロード
        document.getElementById('WF_ButtonUPLOAD').disabled = true;
        // 
        document.getElementById('btnFileSelect').disabled = true;
        // 一括請求ボタン
        document.getElementById('WF_ButtonINVOICEALL').disabled = true;
        // 契約形態ボタン
        document.getElementById('WF_ButtonCONTRACTMODE').disabled = true;
        // 改造費リース区分ボタン
        document.getElementById('WF_ButtonREMODELLEASEKBN').disabled = true;
        // 行削除ボタン
        document.getElementById('WF_Button_LINE_DEL').disabled = true;
        // 全選択ボタン
        document.getElementById('WF_Button_SEL_ALL').disabled = true;
        // 全解除ボタン
        document.getElementById('WF_Button_SEL_CANCEL').disabled = true;
    };

    // する／しないの表示設定
    // 自動更新
    btnDispChange('WF_ButtonAUTOCALC', 'WF_AutoCalc');
    // 一括請求
    btnDispChange('WF_ButtonINVOICEALL', 'WF_InvoiceAll');
    // 契約形態
    btnDispContractChange('WF_ButtonCONTRACTMODE', 'WF_ContractMODE');
    // 改造費リース区分
    btnDispRemodelLeaseKbnChange('WF_ButtonREMODELLEASEKBN', 'WF_RemodelLeaseKbn');
    // スポット区分
    btnSpotKbnDispChange('WF_ButtonSPOTKBN', 'WF_SpotKbn');
    // 締日区分
    btnClosingdayKbnDispChange('WF_ButtonCLOSINGDAYKBN', 'WF_ClosingdayKbn', 'txtClosingDate');

    // 保存ボタンの制御
    if ((loginStatus == '1')
    && ((document.getElementById('WF_APPL_STATUS').value == "" || document.getElementById('WF_APPL_STATUS').value == "0"))) {
        document.getElementById('WF_ButtonSAVE').style.display = 'inline';
        document.getElementById('btnOtherFileUpload').disabled = false;
    } else {
        document.getElementById('WF_ButtonSAVE').style.display = 'none';
        document.getElementById('btnOtherFileUpload').disabled = true;
    }

    // クリアボタンの制御
    if (applListFlg == '1') {
        document.getElementById('WF_ButtonCLEAR').style.display = 'none';
    } else {
        document.getElementById('WF_ButtonCLEAR').style.display = 'inline';
    }

    // 戻るボタンの制御
    if (endDisabledFlg == '1'){
        document.getElementById('WF_ButtonEND').style.display = 'none';
    } else {
        document.getElementById('WF_ButtonEND').style.display = 'inline';
    }

    // ファイナンス情報制御
    if (ctnFlg == '1'){
        document.getElementById('divFinalBulkInputField').style.display = 'inline';
    } else {
        document.getElementById('divFinalBulkInputField').style.display = 'none';
    }

    // 決済条件検索OnLoad用処理
    KekkjSrcOnload()

    // コンテナ一括入力フィールド 表示/非表示
    dispContainerBulkInputField('hdnContainerBulkInputField', 'divContainerBulkInputField')
    // コンテナ一括入力フィールド 表示/非表示イベントバインド
    bindShowCloseContainerBulkInputField();

    // ファイナンス情報一括入力フィールド 表示/非表示
    dispContainerBulkInputField('hdnFinalBulkInputField', 'divFinalBulkInputField')
    // ファイナンス情報一括入力フィールド 表示/非表示イベントバインド
    bindShowCloseFinalBulkInputField();    

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

    if (document.getElementById('WF_InvoiceInfo').value === "hidden" && document.getElementById('WF_LeaseFinal').value === "hidden"){
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
    if (Mode == "1" || Mode == "2") {
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

    ////処理
    //var spread = document.getElementById(spid);
    //var row = spread.GetActiveRow();
    //var col = spread.GetActiveCol();

    ////選択行を非表示項目にセット
    //hidRowIndex.value = row;

    //サブミット
    document.getElementById("MF_SUBMIT").value = "TRUE";
    document.getElementById('WF_ButtonClick').value = "WF_SpreadDBclickKekkjm";
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
    // tableヘッダ、列固定スクロールライブラリ開始
    FixedMidashi.create();
});

/**
 * LEFTBOX用処理カスタム
 * @param {any} fieldId     '入力フィールドのClientID
 * @param {any} lineCnt     'DataTable対象行
 * @param {any} listNo      'リスト番号
 */
function FD_DBclick(fieldId,  lineCnt, listNo) {
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
 * @param {any} invoiceType '請求書種別
 * @param {any} lineCnt     'DataTable対象行
 * @param {any} listNo      'リスト番号
 */
function FD_Kekkjmclick(fieldId, lineCnt, listNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        // クリック位置取得
        var elem = document.getElementById(fieldId);
        var rect = elem.getBoundingClientRect();
        document.getElementById("WF_saveTop").value = rect.top + rect.height;
        document.getElementById("WF_saveLeft").value = rect.left;

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldId;
        document.getElementById('WF_LINECNT').value = lineCnt;

        document.getElementById("WF_ButtonClick").value = "WF_ButtonKekkJView";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

// リース開始日変更処理（TextBox変更時、名称取得）
function TextStart_change(fieldNM) {

    // 更新期間'
    var ddl = document.getElementById('contents1_ddlSelectUpdPeriod');
    var selectedValue = ddl.options[ddl.selectedIndex].value;

    var startYMD = document.getElementById(fieldNM).value;
    if (document.getElementById('TxtLeaseEndYMD').value == "") {
        var dtStartYMD = toDate(startYMD, '-');

        if (selectedValue == "") {
            // 1年後
            dtStartYMD.setFullYear(dtStartYMD.getFullYear() + 1);
        } else {
            // 何月後
            dtStartYMD.setMonth(dtStartYMD.getMonth() + parseInt(selectedValue));
        }
        // -1日
        dtStartYMD.setDate(dtStartYMD.getDate() - 1);
        document.getElementById('TxtLeaseEndYMD').value = getDateToString(dtStartYMD);
    }
}

// 文字列から日付へ変換
// str: 日付文字列（yyyy-MM-dd, yyyy/MM/dd）
// delim: 区切り文字（"-", "/"など）
function toDate(str, delim) {
    var arr = str.split(delim)
    return new Date(arr[0], arr[1] - 1, arr[2]);
}

// 日付から文字列へ変換
function getDateToString(dt) {
    var y = dt.getFullYear();
    var m = ("00" + (dt.getMonth() + 1)).slice(-2);
    var d = ("00" + dt.getDate()).slice(-2);
    var result = y + "-" + m + "-" + d;
    return result;
}
/**
 * ファイルアップロード
 * @return {undefined} なし
 * @param {string} callerObjId 呼出し元のオブジェクトId
 */
function fileUploadToSubmit(callerObjId) {
    let fileObj = document.getElementById(callerObjId);
    if (fileObj === null) {
        return;
    }
    if (fileObj.files.length === 0) {
        return;
    } else {
        ButtonClick(callerObjId);
    }
}

/**
 * 添付ファイル削除
 * @return {undefined} なし
 * @param {string} attachKey 添付ファイルユニークキー
 */
function deleteAttachment(attachKey) {
    let attachKeyObj = document.getElementById('hdnAttachmentKey');
    if (attachKeyObj === null) {
        return;
    }

    attachKeyObj.value = attachKey;
    ButtonClick('deleteAttachment');
}

// 〇契約形態 切替ボタンクリック時
function btnDispContractChange(btnId, txtId) {
    if (document.getElementById(txtId).value === "2"){
        document.getElementById(btnId).value = "ファイナンス";
    } else if (document.getElementById(txtId).value === "3") {
        document.getElementById(btnId).value = "オペレーティング";
    } else {
        // txtID = 1 の場合
        document.getElementById(btnId).value = "開示対象外";
    }
}

// 〇契約形態 切替ボタンクリック時
function btnContractChange_click(btnId, txtId) {
    // 一括請求ボタン
    document.getElementById('WF_ButtonINVOICEALL').disabled = true;

    if (document.getElementById(txtId).value === "1") {
        document.getElementById(btnId).value = "ファイナンス";
        document.getElementById(txtId).value = "2";
    } else if (document.getElementById(txtId).value === "2") {
        document.getElementById(btnId).value = "オペレーティング";
        document.getElementById(txtId).value = "3";
    } else {
        // txtID = 3 の場合
        document.getElementById(btnId).value = "開示対象外";
        document.getElementById(txtId).value = "1";
        // 一括請求ボタン
        document.getElementById('WF_ButtonINVOICEALL').disabled = false;
    }
}

// 〇改造費リース区分 切替ボタンクリック時
function btnDispRemodelLeaseKbnChange(btnId, txtId) {
    if (document.getElementById(txtId).value === "1"){
        document.getElementById(btnId).value = "改造費リース区分";
    } else {
        document.getElementById(btnId).value = "しない";
    }
}

// 〇改造費リース区分 切替ボタンクリック時
function btnRemodelLeaseKbChange_click(btnId, txtId) {
    if (document.getElementById(txtId).value === "1") {
        document.getElementById(btnId).value = "しない";
        document.getElementById(txtId).value = "0";
    } else {
        document.getElementById(btnId).value = "改造費リース区分";
        document.getElementById(txtId).value = "1";
    }
}

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

// 〇締日/契約終了日切替ボタンクリック時
function btnClosingdayKbnDispChange(btnId, txtId, txtIDDay) {
    if (document.getElementById(txtId).value === "1"){
        document.getElementById(btnId).value = "契約終了日";
        document.getElementById(txtIDDay).disabled = true;
        document.getElementById(txtIDDay).value = "";
    } else {
        document.getElementById(btnId).value = "締日";
        if (document.getElementById('WF_APPL_STATUS').value == "" || document.getElementById('WF_APPL_STATUS').value == "0") {
            document.getElementById(txtIDDay).disabled = false;
        }
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
