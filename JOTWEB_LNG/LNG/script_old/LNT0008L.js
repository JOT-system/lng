// 画面読み込み時処理
window.onload = function () {

    setKeyMap()

    //月選択バインド
    commonBindMonthPicker();

    //発受託人検索OnLoad用処理
    TrusteeSrcOnload()

    // 選択ボタン入力制御
    if (document.getElementById('WF_SpdListCnt').value > 0) {
        document.getElementById('WF_ButtonSPREAD_SEL_ALL').disabled = false;
        document.getElementById('WF_ButtonSPREAD_SEL_DEL').disabled = false;
        document.getElementById('WF_ButtonFIRST').disabled = false;
        document.getElementById('WF_ButtonPREVIOUS').disabled = false;
        document.getElementById('WF_ButtonNEXT').disabled = false;
        document.getElementById('WF_ButtonLASTT').disabled = false;
        document.getElementById('WF_ButtonPAGE').disabled = false;
    } else {
        document.getElementById('WF_ButtonSPREAD_SEL_ALL').disabled = true;
        document.getElementById('WF_ButtonSPREAD_SEL_DEL').disabled = true;
        document.getElementById('WF_ButtonFIRST').disabled = true;
        document.getElementById('WF_ButtonPREVIOUS').disabled = true;
        document.getElementById('WF_ButtonNEXT').disabled = true;
        document.getElementById('WF_ButtonLASTT').disabled = true;
        document.getElementById('WF_ButtonPAGE').disabled = true;
    }

    if (document.getElementById('WF_APPROVAL_FLG').value === "1") {
        document.getElementById("WF_ButtonDirect").value = "承認画面へ";
    } else {
        document.getElementById("WF_ButtonDirect").value = "ダイレクト修正";
    }
};

// ○左Box用処理（左Box表示/非表示切り替え）
function Spred_ButtonSel_click() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_SPREAD_ButtonSel";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// ボタン押下
function Sort_click(column) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_SPREAD_BtnClick";
        document.getElementById("WF_SortColumn").value = column;
        document.body.style.cursor = "wait";
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

 // ○発受託人検索ダイアログ ボタンクリックイベント
function DeptrusteeSrc_Click() {

    ButtonClick('WF_ButtonDeptrustee');
}

 // ○受託人検索ダイアログ『閉じる』ボタンクリックイベント
function TrusteeSrcCloseClick() {

    ButtonClick('WF_ButtonTrusteeSrcCLOSE');
}


function setKeyMap() {
    var s = document.getElementById(spid);
    var kcode;
    kcode = 13;

    s.AddKeyMap(kcode, false, false, false, "element.MoveToNextCell()");
    s.AddKeyMap(kcode, false, true, false, "element.MoveToPrevCell()");
}

// ドロップダウンリスト変更
function SelectDropDownList_OnChange(ddlIDClick) {
    var strTaisyoYm = document.getElementById('contents1_ddlSelectTaisyoYm').value;
    var strTaisyoYYYY = strTaisyoYm.substring(0, 4);
    var strTaisyoMM = strTaisyoYm.substring(4, 6);

    var dt = new Date(strTaisyoYYYY, strTaisyoMM, 0);

    //日（最終日）を取得する
    var lastDay = dt.getDate();

    document.getElementById('TxtDateStart').value = strTaisyoYYYY + '/' + strTaisyoMM + '/' + '01';
    document.getElementById('TxtDateEnd').value = strTaisyoYYYY + '/' + strTaisyoMM + '/' + lastDay;
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

$(document).ready(function () {
    $("#contents1_ddlContraLNModel").multiselect({
        menuHeight: 390,
        noneSelectedText: "★全選択",
        selectedText: "# 個選択",
        autoopen: false,
        multiple: true,
        buttonWidth: 150,

        position: {
            my: 'center',
            at: 'center'
        }
    });

    $("#contents1_ddlStackFreeKbn").multiselect({
        menuHeight: 390,
        noneSelectedText: "★全選択",
        selectedText: "# 個選択",
        autoopen: false,
        multiple: true,
        buttonWidth: 150,

        position: {
            my: 'center',
            at: 'center'
        }
    });
});

/**
 *  年月選択Pickerの表示イベントバインド
 * @return {undefined} なし
 * @description 
 */
function commonBindMonthPicker() {
    let targetTextBoxes = document.querySelectorAll("input[type=text][data-monthpicker]");
    for (let i = 0; i < targetTextBoxes.length; i++) {
        targetTextBox = targetTextBoxes[i];
        targetTextId = targetTextBox.id;
        /* 対象のテキストをspanで括る */
        let spanWrapper = document.createElement('span');
        spanWrapper.classList.add('commonMonthWrapperPicker');
        targetTextBox.parentNode.insertBefore(spanWrapper, targetTextBox);
        spanWrapper.appendChild(targetTextBox);
        targetTextBox = document.getElementById(targetTextId);
        targetTextBox.readOnly = 'true';
        targetTextBox.addEventListener('click', (function (targetTextBox) {
            return function () {
                commonDispMonthPicker(targetTextBox);
            };
        })(targetTextBox), true);
    }
}
/**
 *  年月選択Pickerの表示イベントバインド
 * @param {Element} targetTextBox 入力テキストID
 * @return {undefined} なし
 * @description 
 */
function commonDispMonthPicker(targetTextBox) {
    // 初期表示年月の設定
    let currentYear = (new Date()).getFullYear();
    let currentMonth = 0;
    if (targetTextBox.value !== '') {
        currentYear = targetTextBox.value.substring(0, 4);
        monthString = targetTextBox.value.substring(5);
        currentMonth = Number(monthString);
    }
    // MonthPickerIDの定義
    let monthPickerId = 'commonMonthList';
    // 既に表示されている場合は消す
    let findObj = document.getElementById(monthPickerId);
    if (findObj !== null) {
        findObj.parentNode.removeChild(findObj);
    }
    //DatePickerElementの生成
    let parentObj = targetTextBox.parentNode;
    let divNode = document.createElement('div');
    divNode.id = monthPickerId;

    divNode.classList.add('commonMonthListWrapper');

    let insideInnerHtml = "<div><div class='commonMonthListHeader' ><span id='spnCommonPrevYear' onclick='commonChangeMonthPickerYear(-1);' >＜</span><span id='spnCommonMonthPickerDispYear' data-dispyear='" + currentYear + "'></span><span id='spnCommonNextYear'  onclick='commonChangeMonthPickerYear(1);'>＞</span></div>";
    insideInnerHtml = insideInnerHtml + "<hr />";
    insideInnerHtml = insideInnerHtml +
        "<div id='divCommonMonthListBody' class='commonMonthPickerListBody' data-year='" + currentYear + "' data-month='" + currentMonth + "'>";
    for (let i = 1; i < 13; i++) {
        let selectedVal = '';
        if (currentMonth === i) {
            selectedVal = 'selected';
        }
        insideInnerHtml = insideInnerHtml + "<span id='commonMonthPickerTile" + i + "' data-dispyear='" + currentYear + "' data-month='" + i + "' class='monthTile " + selectedVal + "'></span>";
    }
    insideInnerHtml = insideInnerHtml +
        "</div></div>";
    divNode.innerHTML = insideInnerHtml;
    parentObj.appendChild(divNode);
    // 消込処理(年月ポップアップにフォーカスがなくなって１秒経過で消す)
    setTimeout(function () {
        commonDeleteMonthPicker();
    }, 1000); // 初期表示でポップアップに１秒フォーカスが当たらないと消す
    let monthPickerInsideDiv = document.getElementById(monthPickerId).querySelector('div');
    monthPickerInsideDiv.addEventListener('mouseout', (function () {
        return function () {
            setTimeout(function () {
                commonDeleteMonthPicker();
            }, 700); // 初期よりちょい短めに判定700msで消す
        };
    })(), true);
    // 月パネルクリックイベントバインド
    let monthListObj = document.getElementById('divCommonMonthListBody');
    let monthTiles = monthListObj.querySelectorAll('span');
    for (let i = 0; i < monthTiles.length; i++) {
        let monthTileObj = monthTiles[i];
        monthTileObj.addEventListener('click', (function (targetTextBox, monthTileObjId) {
            return function () {
                commonMonthPickerMonthClick(targetTextBox, monthTileObjId);
            };
        })(targetTextBox, monthTileObj.id), true);
    }
}
/**
 *  年月選択Pickerの消込
 * @return {undefined} なし
 * @description 
 */
function commonDeleteMonthPicker() {
    // MonthPickerIDの定義
    let monthPickerId = document.getElementById('commonMonthList');
    if (monthPickerId === null) {
        return;
    }
    let hasHover = monthPickerId.querySelector(':hover');
    if (hasHover === null) {
       monthPickerId.parentNode.removeChild(monthPickerId);
    }
}
/**
 *  年月選択Pickerの年移動ボタン押下時
 * @param {number} moveFlag 移動方向
 * @return {undefined} なし
 * @description 
 */
function commonChangeMonthPickerYear(moveFlag) {
    /* 年表示オブジェクトの取得 */
    let yearTitleObj = document.getElementById('spnCommonMonthPickerDispYear');
    if (yearTitleObj === null) {
        return;
    }
    /* 月表示タイルオブジェクトの取得 */
    let monthListObj = document.getElementById('divCommonMonthListBody');
    let monthTiles = monthListObj.querySelectorAll('span');
    /* 引数に応じ年数を移動 */
    let dispYear = Number(yearTitleObj.dataset.dispyear);
    dispYear = dispYear + moveFlag;
    /* 既設定（テキストボックスに設定済）の値を取得(選択年月ハイライト用 */
    let currentYear = Number(monthListObj.dataset.year);
    let currentMonth = Number(monthListObj.dataset.month);
    /* 表示年月の変更 */
    yearTitleObj.setAttribute('data-dispyear', dispYear);
    /* 月表示タイルオブジェクトのループ */
    for (let i = 0; i < monthTiles.length; i++) {
        let monthTileObj = monthTiles[i];
        let tileMonth = Number(monthTileObj.dataset.month);
        monthTileObj.classList.remove('selected');
        if (currentYear === dispYear && currentMonth === tileMonth) {
            monthTileObj.classList.add('selected');
        }
        monthTileObj.dataset.dispyear = dispYear;
    }
}
/**
 *  年月選択Pickerの月ボタン押下時
 * @param {Element} targetTextBox 年月格納テキストボックス
 * @param {String} monthTileObjId 押下したTileObjId
 * @return {undefined} なし
 * @description 
 */
function commonMonthPickerMonthClick(targetTextBox, monthTileObjId) {
    let tileObj = document.getElementById(monthTileObjId);
    let year = tileObj.dataset.dispyear;
    let month = tileObj.dataset.month;
    if (month.length === 1) {
        month = "0" + month;
    }
    targetTextBox.value = year + "/" + month + "";
    /* 年月ピッカーを画面より消す(削除) */
    let monthPickerId = document.getElementById('commonMonthList');
    if (monthPickerId === null) {
        return;
    }
    monthPickerId.parentNode.removeChild(monthPickerId);

    var strTaisyoYm = document.getElementById('txtDownloadMonth').value;
    var strTaisyoYYYY = strTaisyoYm.substring(0, 4);
    var strTaisyoMM = strTaisyoYm.substring(5, 7);
    var dt = new Date(strTaisyoYYYY, strTaisyoMM, 0);
    //日（最終日）を取得する
    var lastDay = dt.getDate();
    document.getElementById('TxtDateStart').value = strTaisyoYYYY + '/' + strTaisyoMM + '/' + '01';
    document.getElementById('TxtDateEnd').value = strTaisyoYYYY + '/' + strTaisyoMM + '/' + lastDay;

    let needsPostBack = targetTextBox.dataset.monthpickerneedspostback;
    if (typeof needsPostBack === "undefined") {
        return;
    }
    if (needsPostBack === '1') {
        ButtonClick(targetTextBox.id);
    }
}
