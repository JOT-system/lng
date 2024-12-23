﻿// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    //〇 OT発送日報送信ボタン活性／非活性
    //非活性
    document.getElementById("WF_ButtonPAYF").disabled = "disabled";

    if (document.getElementById('TxtOrderNo').value === "") {
        document.getElementById("WF_ButtonINSERT").value = "登録";
    } else {
        document.getElementById("WF_ButtonINSERT").value = "更新";
    }

    //〇 更新ボタン活性／非活性
    let objDtabNo = document.getElementById("WF_DTAB_CHANGE_NO").value;
    if (document.getElementById('WF_MAPpermitcode').value === "TRUE") {
        document.getElementById("WF_ButtonINSERT").disabled = "";

        //〇タブ１
        if (objDtabNo === "0") {
            if (document.getElementById('WF_MAPButtonControl').value === "1"
                || document.getElementById('WF_MAPButtonControl').value === "3") {
                //非活性
                document.getElementById("WF_ButtonALLSELECT_TAB1").disabled = "disabled";
                document.getElementById("WF_ButtonSELECT_LIFTED_TAB1").disabled = "disabled";
                document.getElementById("WF_ButtonLINE_LIFTED_TAB1").disabled = "disabled";
                document.getElementById("WF_ButtonLINE_ADD_TAB1").disabled = "disabled";
                //document.getElementById("WF_ButtonCSV").disabled = "";
                document.getElementById("WF_ButtonFEECALC_TAB1").disabled = "disabled";
            }
            else {
                //活性
                document.getElementById("WF_ButtonALLSELECT_TAB1").disabled = "";
                document.getElementById("WF_ButtonSELECT_LIFTED_TAB1").disabled = "";
                document.getElementById("WF_ButtonLINE_LIFTED_TAB1").disabled = "";
                document.getElementById("WF_ButtonLINE_ADD_TAB1").disabled = "";
                //document.getElementById("WF_ButtonCSV").disabled = "";
                document.getElementById("WF_ButtonFEECALC_TAB1").disabled = "";
            }
        }

    } else {
        //非活性 
        document.getElementById("WF_ButtonINSERT").disabled = "disabled";
        //〇タブ１
        if (objDtabNo === "0") {
            document.getElementById("WF_ButtonALLSELECT_TAB1").disabled = "disabled";
            document.getElementById("WF_ButtonSELECT_LIFTED_TAB1").disabled = "disabled";
            document.getElementById("WF_ButtonLINE_LIFTED_TAB1").disabled = "disabled";
            document.getElementById("WF_ButtonLINE_ADD_TAB1").disabled = "disabled";
            document.getElementById("WF_ButtonFEECALC_TAB1").disabled = "disabled";
        }
    }

    // 上部 表示/非表示イベントバインド
    let showHideButtonObj = document.getElementById('hideHeader');
    if (showHideButtonObj !== null) {
        //クリックイベントのバインド
        showHideButtonObj.addEventListener('click',
            function () {
                hideHeader_click();
            });
        //ロード時は必ず上部 表示/非表示処理を行う
        showHideHeader();
    }

    /* 共通一覧のスクロールイベント紐づけ */
    /* 対象の一覧表IDを配列に格納 */
    let arrListId = new Array();
    if (typeof pnlListAreaId1 !== 'undefined') {
        arrListId.push(pnlListAreaId1);
    }
    if (typeof pnlListAreaId2 !== 'undefined') {
        arrListId.push(pnlListAreaId2);
    }
    if (typeof pnlListAreaId3 !== 'undefined') {
        arrListId.push(pnlListAreaId3);
    }
    if (typeof pnlListAreaId4 !== 'undefined') {
        arrListId.push(pnlListAreaId4);
    }
    /* 対象の一覧表IDをループ */
    for (let i = 0, len = arrListId.length; i < len; ++i) {
        let listObj = document.getElementById(arrListId[i]);
        // 対象の一覧表が未存在（レンダリングされていなければ）ならスキップ
        if (listObj === null) {
            continue;
        }
        // 一覧表のイベントバインド
        //bindListCommonEvents(arrListId[i], IsPostBack, true);
        //bindListCommonEvents(pnlListAreaId, IsPostBack);
        bindListCommonEvents(arrListId[i], IsPostBack, true, true, true, false);
        // テキストボックスEnter縦移動イベントバインド
        commonBindEnterToVerticalTabStep();
        // チェックボックス変更
        ChangeCheckBox(arrListId[i]);
        // チェックボックス変更(Light)
        ChangeCheckBoxLight(arrListId[i]);
    }
}


// ○チェックボックス変更
// 20200115(三宅弘)複数の一覧表に対応するように引数を加え対応しました
function ChangeCheckBox(listId) {
    var objDataLeftSide = document.getElementById(listId + "_DL");
    if (objDataLeftSide === null) {
        return;
    }
    var objTable = objDataLeftSide.children[0];

    var chkObjs = objTable.querySelectorAll("input[id^='chk" + listId + "OPERATION']");
    var spnObjs = objTable.querySelectorAll("span[id^='hchk" + listId + "OPERATION']");
    let objSelectIndex = document.getElementById("WF_SelectedIndex").value
    let objChkboxFlg = document.getElementById("WF_CheckBoxFLG").value
    for (let i = 0; i < chkObjs.length; i++) {

        if (chkObjs[i] !== null) {
            if (spnObjs[i].innerText === "on") {
                chkObjs[i].checked = true;
            } else {
                chkObjs[i].checked = false;
            }
            // チェックボックスをクリック、かつ選択した行の場合
            if (objChkboxFlg === "TRUE" && Number(objSelectIndex) === i + 1) {
                // フォーカスを当てる
                chkObjs[i].focus()
                // フォーカスを外す
                chkObjs[i].blur()
            }
        }
    }
}


// ○チェックボックス変更(Light)
function ChangeCheckBoxLight(listId) {
    var objDataLightSide = document.getElementById(listId + "_DR");
    if (objDataLightSide === null) {
        return;
    }
    var objLightTable = objDataLightSide.children[0];

    var chkObjsLight1 = objLightTable.querySelectorAll("input[id^='chk" + listId + "STACKINGFLG']");
    var spnObjsLight1 = objLightTable.querySelectorAll("span[id^='hchk" + listId + "STACKINGFLG']");
    var chkObjsLight2 = objLightTable.querySelectorAll("input[id^='chk" + listId + "FIRSTRETURNFLG']");
    var spnObjsLight2 = objLightTable.querySelectorAll("span[id^='hchk" + listId + "FIRSTRETURNFLG']");
    var chkObjsLight3 = objLightTable.querySelectorAll("input[id^='chk" + listId + "AFTERRETURNFLG']");
    var spnObjsLight3 = objLightTable.querySelectorAll("span[id^='hchk" + listId + "AFTERRETURNFLG']");
    var chkObjsLight4 = objLightTable.querySelectorAll("input[id^='chk" + listId + "OTTRANSPORTFLG']");
    var spnObjsLight4 = objLightTable.querySelectorAll("span[id^='hchk" + listId + "OTTRANSPORTFLG']");

    var chkObjsLight5 = objLightTable.querySelectorAll("input[id^='chk" + listId + "WHOLESALEFLG']");
    var spnObjsLight5 = objLightTable.querySelectorAll("span[id^='hchk" + listId + "WHOLESALEFLG']");
    var chkObjsLight6 = objLightTable.querySelectorAll("input[id^='chk" + listId + "INSPECTIONFLG']");
    var spnObjsLight6 = objLightTable.querySelectorAll("span[id^='hchk" + listId + "INSPECTIONFLG']");
    var chkObjsLight7 = objLightTable.querySelectorAll("input[id^='chk" + listId + "DETENTIONFLG']");
    var spnObjsLight7 = objLightTable.querySelectorAll("span[id^='hchk" + listId + "DETENTIONFLG']");

    for (let i = 0; i < chkObjsLight1.length; i++) {

        if (chkObjsLight1[i] !== null) {
            if (spnObjsLight1[i].innerText === "on") {
                chkObjsLight1[i].checked = true;
            } else {
                chkObjsLight1[i].checked = false;
            }
        }
    }

    for (let i = 0; i < chkObjsLight2.length; i++) {

        if (chkObjsLight2[i] !== null) {
            if (spnObjsLight2[i].innerText === "on") {
                chkObjsLight2[i].checked = true;
            } else {
                chkObjsLight2[i].checked = false;
            }
        }
    }

    for (let i = 0; i < chkObjsLight3.length; i++) {

        if (chkObjsLight3[i] !== null) {
            if (spnObjsLight3[i].innerText === "on") {
                chkObjsLight3[i].checked = true;
            } else {
                chkObjsLight3[i].checked = false;
            }
        }
    }

    for (let i = 0; i < chkObjsLight4.length; i++) {

        if (chkObjsLight4[i] !== null) {
            if (spnObjsLight4[i].innerText === "on") {
                chkObjsLight4[i].checked = true;
            } else {
                chkObjsLight4[i].checked = false;
            }
        }
    }

    for (let i = 0; i < chkObjsLight5.length; i++) {

        if (chkObjsLight5[i] !== null) {
            if (spnObjsLight5[i].innerText === "on") {
                chkObjsLight5[i].checked = true;
            } else {
                chkObjsLight5[i].checked = false;
            }
        }
    }

    for (let i = 0; i < chkObjsLight6.length; i++) {

        if (chkObjsLight6[i] !== null) {
            if (spnObjsLight6[i].innerText === "on") {
                chkObjsLight6[i].checked = true;
            } else {
                chkObjsLight6[i].checked = false;
            }
        }
    }

    for (let i = 0; i < chkObjsLight7.length; i++) {

        if (chkObjsLight7[i] !== null) {
            if (spnObjsLight7[i].innerText === "on") {
                chkObjsLight7[i].checked = true;
            } else {
                chkObjsLight7[i].checked = false;
            }
        }
    }

    //### 20201207 START 指摘票No248対応 ############################################################
    // 格上可否フラグ
    var chkObjsLight8 = objLightTable.querySelectorAll("input[id^='chk" + listId + "UPGRADEFLG']");
    var spnObjsLight8 = objLightTable.querySelectorAll("span[id^='hchk" + listId + "UPGRADEFLG']");
    for (let i = 0; i < chkObjsLight8.length; i++) {

        if (chkObjsLight8[i] !== null) {
            if (spnObjsLight8[i].innerText === "on") {
                chkObjsLight8[i].checked = true;
            } else {
                chkObjsLight8[i].checked = false;
            }
        }
    }
    //### 20201207 END   指摘票No248対応 ############################################################

    //### 20210120 START 指摘票No300対応 ############################################################
    // 格下可否フラグ
    var chkObjsLight9 = objLightTable.querySelectorAll("input[id^='chk" + listId + "DOWNGRADEFLG']");
    var spnObjsLight9 = objLightTable.querySelectorAll("span[id^='hchk" + listId + "DOWNGRADEFLG']");
    for (let i = 0; i < chkObjsLight9.length; i++) {

        if (chkObjsLight9[i] !== null) {
            if (spnObjsLight9[i].innerText === "on") {
                chkObjsLight9[i].checked = true;
            } else {
                chkObjsLight9[i].checked = false;
            }
        }
    }
    //### 20210120 END   指摘票No300対応 ############################################################
}


// ○チェックボックス選択
function SelectCheckBox(obj, lineCnt, fieldName) {

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        let chkObj = obj.querySelector("input");
        if (chkObj === null) {
            return;
        }
        if (chkObj.disabled === true) {
            return;
        }

        surfix = '';
        if (fieldName === 'STACKINGFLG') {
            surfix = 'STACKING'
        }
        if (fieldName === 'WHOLESALEFLG') {
            surfix = 'WHOLESALE'
        }
        if (fieldName === 'INSPECTIONFLG') {
            surfix = 'INSPECTION'
        }
        if (fieldName === 'DETENTIONFLG') {
            surfix = 'DETENTION'
        }
        if (fieldName === 'FIRSTRETURNFLG') {
            surfix = 'FIRSTRETURN'
        }
        if (fieldName === 'AFTERRETURNFLG') {
            surfix = 'AFTERRETURN'
        }
        if (fieldName === 'OTTRANSPORTFLG') {
            surfix = 'OTTRANSPORT'
        }
        //### 20201207 START 指摘票No248対応 ############################################################
        if (fieldName === 'UPGRADEFLG') {
            surfix = 'UPGRADE'
        }
        //### 20201207 END   指摘票No248対応 ############################################################
        //### 20210120 START 指摘票No300対応 ############################################################
        if (fieldName === 'DOWNGRADEFLG') {
            surfix = 'DOWNGRADE'
        }
        //### 20210120 END   指摘票No300対応 ############################################################

        document.getElementById("WF_SelectedIndex").value = lineCnt;
        document.getElementById("WF_ButtonClick").value = "WF_CheckBoxSELECT" + surfix;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }

}

// ○左Box用処理（左Box表示/非表示切り替え）
function ListField_DBclick(pnlList, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_GridDBclick').value = Line;
        document.getElementById('WF_FIELD').value = fieldNM;

        // 品目コード
        if (fieldNM === "ITEMCD") {
            document.getElementById('WF_LeftMViewChange').value = 9;
        }
        // 発駅コード
        else if (fieldNM === "DEPSTATION") {
            document.getElementById('WF_LeftMViewChange').value = 11;
        }
        // 着駅コード
        else if (fieldNM === "ARRSTATION") {
            document.getElementById('WF_LeftMViewChange').value = 13;
        }
        // 発受託人コード
        else if (fieldNM === "DEPTRUSTEECD") {
            document.getElementById('WF_LeftMViewChange').value = 23;
        }
        // 着受託人コード
        else if (fieldNM === "ARRTRUSTEECD") {
            document.getElementById('WF_LeftMViewChange').value = 27;
        }
        // 積空区分
        else if (fieldNM === "STACKFREEKBNCD") {
            document.getElementById('WF_LeftMViewChange').value = 35;
        }
        // 荷送人
        else if (fieldNM === "SHIPPERCD") {
            document.getElementById('WF_LeftMViewChange').value = 37;
        }
        // 日付
        else if (fieldNM === "PLANARRYMD") {
            document.getElementById('WF_LeftMViewChange').value = 2;
        }
        document.getElementById('WF_LeftboxOpen').value = "Open";
        document.getElementById('WF_ButtonClick').value = "WF_Field_DBClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// ○一覧用処理（チェンジイベント）
function ListField_Change(pnlList, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_GridDBclick').value = Line;
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_ButtonClick').value = "WF_ListChange";
        document.forms[0].submit();
    }
}
// 〇表示/非表示ボタンクリック時
function hideHeader_click() {
    let headerStateObj = document.getElementById('hdnDispHeaderItems');
    //表示/非表示のフラグ切替
    headerStateObj.value = Math.abs(Number(headerStateObj.value) - 1);
    //切替処理の実行
    showHideHeader();
} 
// 〇上部表示/非表示処理
function showHideHeader() {
    let headerStateObj = document.getElementById('hdnDispHeaderItems');
    let showHideButtonObj = document.getElementById('hideHeader');
    let headerObj = document.getElementById('headerDispArea');
    let detailBoxOjb = document.getElementById('detailbox');
    // 操作対象のオブジェクトが無い場合はそのまま終了
    if (headerStateObj === null) {
        return;
    }
    if (showHideButtonObj === null) {
        return;
    }
    if (headerObj === null) {
        return;
    }
    if (detailBoxOjb === null) {
        return;
    }
    // ヘッダーの表示/非表示切替
    showHideButtonObj.classList.remove('hideHeader');
    headerObj.classList.remove('hideHeader');
    if (headerStateObj.value === '0') {
        //ヘッダー非表示の場合(対象のCssクラスにhideHeader付与)
        showHideButtonObj.classList.add('hideHeader');
        headerObj.classList.add('hideHeader');
    }
    /* 下部の高さを定義 */
    let top = detailBoxOjb.offsetTop;
    let footer = 22.22;
    detailBoxOjb.style.height = "calc(100% - " + top + "px)";
    /* 一覧表の幅をヘッダー有無で可変にする為、ウィンドウのリサイズイベントを発火 */
    var resizeEvent = window.document.createEvent('UIEvents');
    resizeEvent.initUIEvent('resize', true, false, window, 0);
    window.dispatchEvent(resizeEvent);
}