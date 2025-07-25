﻿/**
 * @fileoverview システム共通JavaScript処理
 */
var commonKeyEnterProgress = false;
/**
 * ロード時処理
 * @param {object} なし
 * @return {undefined} なし
 */
window.addEventListener('load', function () {

    // CHPMasterPageのPage_Load処理で
    // commonDispWaitが実行されているため
    // 画面ロード完了後にwaitを解除する
    setTimeout(() => { commonHideWait(); }, 0);

    //左ナビゲーションのクリックイベントバインド
    //bindLeftNaviClick();
    //ガイダンス開閉のイベントバインド
    let guidanceButton = document.getElementById('guidanceOpenCloseButton');
    if (guidanceButton !== null) {
        bindShowCloseGuidance(guidanceButton);
    }
    //お知らせ開閉のイベントバインド
    let guidanceBoxCloseButton = document.getElementById('guidanceBoxOpenCloseButton');
    if (guidanceBoxCloseButton !== null) {
        bindShowCloseGuidanceBox(guidanceBoxCloseButton);
    }

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen") !== null) {
        if (document.getElementById("WF_LeftboxOpen").value === "Open") {
            document.getElementById("LF_LEFTBOX").style.display = "block";
            var spdVal = document.getElementById("WF_FIELD").value.substr(0, 3)
            if (spdVal === "spd") {
                /* 通常はオブジェクトの真下に表示する */
                document.getElementById("LF_LEFTBOX").style.top = document.getElementById("WF_saveTop").value + "px";;
                document.getElementById("LF_LEFTBOX").style.left = document.getElementById("WF_saveLeft").value + "px";;
            } else {
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
                /* オブジェクトの左端＋検索BOXの右端がウインドウのビューポートの右端を超える場合は */
                /* 超えた分だけ検索BOXのX座標を左にずらす */
                if ((objRect.left + rect.width) > window.innerWidth) {
                    var correctX = window.innerWidth - (objRect.left + rect.width);
                    /* 左ナビゲーションメニュー(250px) + マージンに重ならないようにする */
                    if ((objRect.left + correctX) > 257) {
                        document.getElementById("LF_LEFTBOX").style.left = (objRect.left + correctX) + "px";
                    } else {
                        document.getElementById("LF_LEFTBOX").style.left = "257px";
                    }

                } else {
                    /* 通常はオブジェクトの左端に検索BOXの左端を合わせる */
                    document.getElementById("LF_LEFTBOX").style.left = objRect.left + "px";
                }
            }
        }
    }

    // 左ボックス拡張機能追加
    if (typeof leftListExtentionTarget !== 'undefined') {
        addLeftBoxExtention(leftListExtentionTarget);
    }

    // 右ボックス
    if (document.getElementById("RF_RIGHTBOX") !== null) {
        document.getElementById("RF_RIGHTBOX").style.width = "0em";
    }

    /* エラーレポートの内容が設定されている場合 */
    if (document.getElementById("RF_ERR_REPORT") !== null) {
        if (String(document.getElementById("RF_ERR_REPORT").value).length > 0) {
            document.getElementById("rightb").style.display = "block";
        }
    }

    if (document.getElementById("WF_RightboxOpen") !== null) {
        if (document.getElementById("WF_RightboxOpen").value === "Open") {
            document.getElementById("RF_RIGHTBOX").style.width = "25em";
            document.getElementById("rightb").style.display = "none";
            document.getElementById("rightb").style.backgroundImage = "url('../img/guidanceIconE.png')"
        }
        if (document.getElementById("WF_RightboxOpen").value === "OpenI") {
            document.getElementById("RF_RIGHTBOX").style.width = "25em";
            document.getElementById("rightb").style.display = "none";
            document.getElementById("rightb").style.backgroundImage = "url('../img/guidanceIconI.png')"
        }
        if (document.getElementById("WF_RightboxOpen").value === "OpenW") {
            document.getElementById("RF_RIGHTBOX").style.width = "25em";
            document.getElementById("rightb").style.display = "none";
            document.getElementById("rightb").style.backgroundImage = "url('../img/guidanceIconW.png')"
        }
    }

    /* ブラウザ戻るボタンの禁止(無反応化) */
    if (window.history && window.history.pushState) {
        history.pushState('nohb', null, '');
        window.addEventListener('popstate', function (e) {
            if (!e.state) {
                //. もう一度履歴を操作して終了
                history.pushState('nohb', null, '');
                return;
            }
        });
    }
    //ポップアップ背面を使用不可に変更
    var popUpObj = document.getElementById('pnlCommonMessageWrapper');
    if (popUpObj !== null) {
        if (popUpObj.style.display !== 'none') {
            // 現在のフォーカスをポップアップに移動
            if (document.activeElement !== null) {
                document.activeElement.blur();
            }
            commonDisableModalBg(popUpObj.id);
            popUpObj.focus();
        }
    }

    //再度メニューのイベント
    bindLeftNaviClick('divLeftNav1', '1');
    bindLeftNaviClick('divLeftNav2', '2');
    bindLeftNaviClick('divLeftNav3', '3');
    bindLeftNaviClick('divLeftNav4', '4');
    bindLeftNaviClick('divLeftNav5', '5');
    bindLeftNaviClick('divLeftNav6', '6');
    bindLeftNaviClick('divLeftNav7', '7');
    bindLeftNaviClick('divLeftNav8', '8');
    bindLeftNaviClick('divLeftNav9', '9');
    bindLeftNaviClick('divLeftNav10', '10');
    bindLeftNaviClick('divLeftNav11', '11');
    bindLeftNaviClick('divLeftNav12', '12');
    bindLeftNaviClick('divLeftNav13', '13');
    bindLeftNaviClick('divLeftNav14', '14');
    bindLeftNaviClick('divLeftNav15', '15');

});
/**
 * 左ナビゲーションクリックイベントバインド
 * @return {undefined} なし
 */
function bindLeftNaviClick(strElementID, strID) {
    /* 左ナビ全体のDivを取得 */
    let leftNavObj = document.getElementById(strElementID);
    /* 左ナビ未描画なら終了 */
    if (leftNavObj === null) {
        return;
    }
    /* ラベルタグ（左ナビボタン風デザイン）のオブジェクトを取得 */
    let labelObjList = leftNavObj.querySelectorAll("div[data-hasnext='1'] > label");
    /* 左ナビボタンが描画されてなければそのまま終了 */
    if (labelObjList === null) {
        return;
    }
    if (labelObjList.length === 0) {
        return;
    }
    /* 左ナビボタンのループ */
    for (let i = 0; i < labelObjList.length; i++) {
        let targetLabel = labelObjList[i];
        let parentDiv = targetLabel.parentNode;
        let posicol = parentDiv.dataset.posicol;
        let rowline = parentDiv.dataset.rowline;
        // ダイアログを閉じるタイミングでフォーカスを合わせる
        targetLabel.addEventListener('click', (function (posicol, rowline) {
            return function () {
                let hdnPosiColObj = document.getElementById('hdnPosiCol');
                hdnPosiColObj.value = posicol;
                let hdnRowLineObj = document.getElementById('hdnRowLine');
                hdnRowLineObj.value = rowline;
                commonDispWait();
                ButtonClick('WF_ButtonLeftNavi' + strID); /* 共通サブミット処理、VB側ロード時のSelectケースで割り振らせる */
            };
        })(posicol, rowline), false);

    }
}
/**
 * DOM読み込み完了時時処理(ロードより先に実行される)
 * @param {object} なし
 * @return {undefined} なし
 */
window.addEventListener('DOMContentLoaded', function () {
    // 画面初期処理(個別のInitDisplay関数未定義の場合はスキップ)
    if (typeof InitDisplay === 'function') {
        InitDisplay();
    }
    //フッターメッセージ除去
    let msgBoxObj = document.getElementById('pnlCommonMessageWrapper');
    if (msgBoxObj !== null) {
        if (msgBoxObj.style.display !== 'none') {
            document.getElementById("WF_MESSAGE").innerText = "";
        }
    }
    /* テキストボックスフォーカスがあった時点で選択 */
    var texboxObjList = document.querySelectorAll("input[type='text'],input[type='number'],input[type='password']");
    for (let i = 0; i < texboxObjList.length; i++) {
        texboxObjList[i].addEventListener('focus', function () {
            // Edgeの場合はディレイをかけてテキストボックス全選択
            if (navigator.userAgent.match(/Edge\/(13|14|15|16|17|18)/)) {
                let tergetItemId = this.id;
                return setTimeout(function () {
                    document.getElementById(tergetItemId).select();
                }, 10);
            }
            this.select();
        });
    }
    //フォーカス合わせ
    let leftView = document.getElementById('LF_LEFTBOX');
    if (leftView !== null) {
        if (leftView.style.display !== 'block') {
            let saveKey = document.title + "currentItemId";
            var tergetItemId = sessionStorage.getItem(saveKey);
            if (tergetItemId !== null) {
                let varItem = document.getElementById(tergetItemId);
                if (varItem !== null) {
                    //IEだと通常のフォーカスメソッドだけだと機能しないためタイマーで稼働させる
                    //IEを無視するなら「varItem.focus();」で良い
                    setTimeout(function () {
                        document.getElementById(tergetItemId).focus();
                        var divContensboxObj = document.getElementById("divContensbox");
                        if (divContensboxObj !== null) {
                            let saveScrollKey = document.title + "contentsXpos";
                            let contentsScrollX = sessionStorage.getItem(saveScrollKey);
                            divContensboxObj.scrollLeft = contentsScrollX;
                            sessionStorage.removeItem(saveScrollKey);
                        }
                    }, 10);
                }
            }
            sessionStorage.removeItem(saveKey);
        }
        var divContensboxObj = document.getElementById("divContensbox");
        if (divContensboxObj !== null) {
            let saveScrollKey = document.title + "contentsXpos";
            let contentsScrollX = sessionStorage.getItem(saveScrollKey);
            divContensboxObj.scrollLeft = contentsScrollX;
        }
    }
    // 確認ウィンドウ
    ConfirmWindow();
    /* ******************************** */
    /* 虫眼鏡・検索のオブジェクトを付与 */
    /* ******************************** */
    // 対象オブジェクトの検索(inputタグのclass属性に'boxIcon'または'calendarIcon'が設定されているもの)
    let queryString = "input.boxIcon,input.calendarIcon,input.disabledboxIcon,input.disabledcalendarIcon";
    // 暫定（日付をやるならvb側をいじる）グリッド内のテキストボックス(グリッド内のtdにダブルクリックイベントがあるテキストボックス)
    queryString = queryString + ",div[data-generated='1'] td[ondblclick] > input[type=text]";
    queryString = queryString + ",div[data-generated='1'] td[ondblclick] > input[type=number]";
    // リッチテキスト用
    queryString = queryString + ",div[data-generated='1'] td > input[type=text][id^='txt'][id*='REMARK']";

    var targetTextBoxList = document.querySelectorAll(queryString);
    if (targetTextBoxList !== null) {
        document.forms[0].style.display = 'none'; //高速化対応 一旦非表示にしDOM追加ごとの再描画を抑止
        commonAppendInputBoxIcon(targetTextBoxList);
        document.forms[0].style.display = 'block'; //高速化対応 一旦非表示にしDOM追加ごとの再描画を抑止
    }
    /* ******************************** */
    /* 左ボックステーブル表示時の補正   */
    /* ******************************** */
    let userAgent = window.navigator.userAgent.toLowerCase();
    if (userAgent.indexOf('msie') !== -1 ||
        userAgent.indexOf('trident') !== -1) {
        //IE(display:stickyが効かない為IEはこれでカバー)
        commonLeftTableHeaderFixed();
    }
    /* ******************************** */
    /* 左ボックステーブルソート機能     */
    /* ******************************** */
    commonLeftTableSortEventBind();
    /* ************************************ */
    /* 数字入力のみの関数を仕込んでいる場合 */
    /* 全角→半角変換を行う                 */
    /* ************************************ */
    let numericTextObjList = document.querySelectorAll('input[type="text"][onkeypress*="CheckNum()"]');
    if (numericTextObjList !== null) {
        for (let i = 0; i < numericTextObjList.length; i++) {
            let numericObj = numericTextObjList[i];
            numericObj.setAttribute('inputmode', 'numeric');
            numericObj.addEventListener('change', (function (numericObj) {
                return function () {
                    ConvartWideCharToNormal(numericObj);
                };
            })(numericObj), true);
        }
    }
    /* ************************************ */
    /* 数字入力のみの関数を仕込んでいる場合 */
    /* 全角→半角変換を行う                 */
    /* ************************************ */
    let decimalTextObjList = document.querySelectorAll('input[type="text"][onkeypress*="CheckDeci()"]');
    if (decimalTextObjList !== null) {
        for (let i = 0; i < decimalTextObjList.length; i++) {
            let decimalObj = decimalTextObjList[i];
            decimalObj.setAttribute('inputmode', 'decimal');
            decimalObj.addEventListener('change', (function (decimalObj) {
                return function () {
                    ConvartWideCharToNormal(decimalObj);
                };
            })(decimalObj), true);
        }
    }
    /* ************************************ */
    /* 数字入力のみの関数を仕込んでいる場合 */
    /* 全角→半角変換を行う                 */
    /* ************************************ */
    let telTextObjList = document.querySelectorAll('input[type="text"][onkeypress*="CheckTel()"]');
    if (telTextObjList !== null) {
        for (let i = 0; i < telTextObjList.length; i++) {
            let telObj = telTextObjList[i];
            telObj.setAttribute('inputmode', 'numeric');
            telObj.addEventListener('change', (function (telObj) {
                return function () {
                    ConvartWideCharToNormal(telObj);
                };
            })(telObj), true);
        }
    }
    /* ************************************ */
    /* 一覧表変更情報を保持するための       */
    /* イベントバインド                     */
    /* ************************************ */
    bindcommonListChangedInput();
    /* ************************************  */
    /* 通常テキストフィールドのEntarタブ移動 */
    /* ************************************  */
    commonBindNormalEnterToNextTabStep();
    /* ************************************  */
    /* ダブルタップイベントの紐づけ(一覧のダブルクリック、および画面右上) */
    /* ************************************  */
    commonBindDblTapEvents();
    /* ************************************  */
    /* OT空回連携情報ステータス行ハイライト  */
    /* ************************************  */
    commonSetHasCmpResultsInfoToHighlight();
    /* ************************************  */
    /* 受注情報行ハイライト                  */
    /* ************************************  */
    commonSetHasOrderInfoToHighlight();
    /* ************************************  */
    /* 回送情報行ハイライト                  */
    /* ************************************  */
    commonSetHasKaisouInfoToHighlight();
    /* ************************************  */
    /* 受注進行ステータス行ハイライト        */
    /* ************************************  */
    commonSetHasOrderStatusToHighlight();
    /* ************************************  */
    /* カスタムポップアップ                  */
    /* ************************************  */
    let dispCustomPopUpObj = document.getElementById('MF_SHOWCUSTOMOPOUP');
    let divContentsPopUpWrapperObj = document.getElementById('divContentsPopUpWrapper');
    if (divContentsPopUpWrapperObj !== null) {
        okButtonObj = document.getElementById('WF_ButtonOkCommonPopUp');
        if (okButtonObj !== null) {
            if (typeof customPopUpOkButtonName !== "undefined") {
                okButtonObj.value = customPopUpOkButtonName;
            }
        }
        
        if (dispCustomPopUpObj.value === '1') {
            commonShowCustomPopup();
        } else {
            commonHideCustomPopup();
        }
    }
    /* ************************************  */
    /* 共通タイルチェックイベントバインド    */
    /* ************************************  */
    commonBindSingleCheckOnly();
    /* ************************************** */
    /* 共通複数選択ポップアップ表示非表示制御 */
    /* ************************************** */
    commonGrc0002InputSet();
    /* ************************************** */
    /* 共通ログアウトボタン表示非表示制御 */
    /* ************************************** */
    commonLogOutDisp();
});

// 処理後カーソルを戻す
//function AutoCursor() {
//    document.body.style.cursor = "auto";
//}
// 上下構成の２段コンテンツのフッターサイズ調整
function AdjustHeaderFooterContents(footerContentsId) {
    let footerContentObj = document.getElementById(footerContentsId);
    if (footerContentObj === null) {
        return;
    }
    /* 下部の高さを定義 */
    var footerClientRect = footerContentObj.getBoundingClientRect();
    /* 12はWrapperObjのPadding-Bottom*/
    let otherContntsHeight = footerContentObj.offsetTop;
    
    if (navigator.userAgent.match(/Edge\/(13|14|15|16|17|18)/)) {
        let otherObj = footerContentObj.parentNode;
        let otherHeight = otherObj.offsetHeight - otherObj.clientHeight;
        otherContntsHeight = otherHeight + otherContntsHeight;
    }
    footerContentObj.style.height = "calc(100% - " + otherContntsHeight + "px)";
}
// ポップアップ確認
function ConfirmWindow() {

    if (document.getElementById("MF_SUBMIT").value === "FALSE" &&
        document.getElementById("MF_ALERT").value === "TRUE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";        //親画面を操作不可にする
        document.getElementById("MF_ALERT").value = "FALSE";

        //確認ウィンドウ表示
        var btn = document.getElementById("MF_AGAIN").value;
        var msg = document.getElementById("MF_ALT_MSG").value;
        var left = (screen.width - 450) / 2;
        var top = (screen.height - 200) / 2;
        var param = "status=0, scrollbars=0, directories=0, menubar=0, resizable=0, toolbar=0, location=0, width=450, height=200, left=" + left + ", top=" + top;
        var win = window.open("GRCO0108CONFIRM.aspx?MSGbtn=" + btn + "&MSGtext=" + msg, "_blank", param);

        //0.5秒置きに子画面の状況を確認する
        var interval = setInterval(function () {
            if (win.closed) {
                clearInterval(interval);
                if (document.getElementById("MF_ALERT").value === "OK") {
                    //document.body.style.cursor = "wait";
                    commonDispWait();
                    document.forms[0].submit();
                } else {
                    document.getElementById("MF_SUBMIT").value = "FALSE";
                    document.getElementById("MF_ALERT").value = "FALSE";
                    return false;
                }
            } else {
                if (!win.document.hasFocus()) {
                    //子画面にフォーカスを充てる
                    win.focus();
                }
            }
        }, 500);

        //確認ポップアップ画面(confirm()はEdgeだと非表示に設定できるため中止)
        //if (window.confirm(document.getElementById("MF_ALT_MSG").value)) {
        //    document.getElementById("MF_SUBMIT").value = "TRUE";
        //    document.getElementById("MF_ALERT").value = "OK";
        //    document.getElementById("WF_ButtonClick").value = document.getElementById("MF_AGAIN").value;
        //    document.body.style.cursor = "wait";
        //    document.forms[0].submit();
        //} else {
        //    document.getElementById("MF_ALERT").value = "FALSE";
        //    return false;
        //}
    } else {
        document.getElementById("MF_ALERT").value = "FALSE";
        return false;
    }
}

// 左ボックスの拡張機能（ソート、フィルタ）を追加
// {TargetListBoxes} 以下のデータを配列としてもつ {リストボックスのID, ソート機能フラグ, フィルタ機能フラグ}
//                    ※ソート機能フラグ(0:無し, 1:名称のみ, 2:コードのみ, 3:両方)
//                    ※フィルタ機能フラグ(0:無し, 1:設定)
// 拡張機能によりリストが切れてしまう場合、各画面のリストボックスを<p>タグから<a>タグに変更してみてください。
function addLeftBoxExtention(TargetListBoxes) {
    // 引数未指定や配列がない場合は終了
    if (TargetListBoxes === null) {
        return;
    }
    if (TargetListBoxes.length === 0) {
        return;
    }
    // 左ボックスがない場合はそのまま終了
    var LeftBoxObj = document.getElementById('LF_LEFTBOX');
    if (LeftBoxObj === null) {
        return;
    }
    // 対象一覧のループ
    for (let i = 0; i < TargetListBoxes.length; i++) {
        // オブジェクトの存在チェック(存在しない場合はスキップ)
        if (document.getElementById(TargetListBoxes[i][0]) === null) {
            continue;
        }

        // リストボックスの取得、および拡張機能のフラグを取得
        var ListObj = document.getElementById(TargetListBoxes[i][0]);
        var SortFlag = TargetListBoxes[i][1];       //ソート機能フラグ
        var FilterFlag = TargetListBoxes[i][2];     //フィルタ機能フラグ
        var SubmitParam = TargetListBoxes[i][3];     //フィルタ機能　パラメータ

        // フラグが両方無しの場合意味がないので終了
        if (SortFlag === '0' && FilterFlag === '0') {
            return;
        }

        // ソート拡張機能を追加
        if (SortFlag === '1' || SortFlag === '2' || SortFlag === '3') {
            addLeftBoxSort(ListObj, SortFlag);
        }

        // フィルタ拡張機能を追加
        if (FilterFlag === '1') {
            addLeftBoxFilter(ListObj);
        } else if (FilterFlag === '2') {
            addLeftBoxsubmit(ListObj, SubmitParam);
        }

        //       // ソートデフォルトを名称検索状態にする
        //       if (SortFlag == '1' || SortFlag == '3') {
        //           var nameSortChkObj = document.getElementById('WF_LeftBoxSortNameASC');
        //           if (nameSortChkObj != null) {
        //               nameSortChkObj.click();
        //           }
        //       }

        // 1リストしか存在しえないので見つかったら処理終了
        return;
    }
}

// 左ボックスソート拡張機能を追加
//  ※ソート機能フラグ(0:無し, 1:名称のみ, 2:コードのみ, 3:両方)
function addLeftBoxSort(ListObj, SortFlag) {
    // オブジェクトの存在チェック(存在しない場合はスキップ)
    if (ListObj === null || ListObj === undefined) {
        return;
    }

    // ソートラジオボタンオブジェクトをクライアントサイドで生成するタグ
    var orderChooseTable = '<table id="WF_LeftBoxSortType">\n';

    // コード検索用ラジオボタン追加
    if (SortFlag === '2' || SortFlag === '3') {
        orderChooseTable = orderChooseTable + '  <tr>\n' +
            '    <td><input name="WF_LeftBoxSort" id="WF_LeftBoxSortCodeASC"  type="radio" value="CodeASC" />\n' +
            '        <label for="WF_LeftBoxSortCodeASC">コード昇順</label>\n' +
            '    </td>\n' +
            '    <td><input name="WF_LeftBoxSort" id="WF_LeftBoxSortCodeDESC" type="radio" value="CodeDesc" />\n' +
            '        <label for="WF_LeftBoxSortCodeDESC">コード降順</label>\n' +
            '    </td>\n' +
            '  </tr>\n';
    }

    // 名称検索用ラジオボタン追加
    if (SortFlag === '1' || SortFlag === '3') {
        //        let checkVal = ''; // 名称検索のみの場合はNameAscにデフォルトチェックをあてる
        //        if (SortFlag == '1') {
        //            checkVal = 'checked="checked"';
        //        }
        orderChooseTable = orderChooseTable + '  <tr>\n' +
            '    <td><input name="WF_LeftBoxSort" id="WF_LeftBoxSortNameASC"  type="radio" value="NameASC"  />\n' +
            '        <label for="WF_LeftBoxSortNameASC">名称昇順</label>\n' +
            '    </td>\n' +
            '    <td><input name="WF_LeftBoxSort" id="WF_LeftBoxSortNameDESC" type="radio" value="NameDesc" />\n' +
            '        <label for="WF_LeftBoxSortNameDESC">名称降順</label>\n' +
            '    </td>\n' +
            '  </tr>\n';
    }
    orderChooseTable = orderChooseTable + '</table>\n';

    // 上記で作成したタグをリストボックス前に挿入
    // ListObj.insertAdjacentHTML('beforebegin', orderChooseTable);
    // リストボックス前ではなく、ボタンの後に挿入(ダブルクリックイベント防止)
    document.getElementById('button').insertAdjacentHTML('beforeend', orderChooseTable);

    // ラジオボタンのイベントバインド(挿入したラジオボタンすべて)
    var objId = ListObj.id;
    var rbLists = document.getElementsByName('WF_LeftBoxSort');
    for (let i = 0; i < rbLists.length; i++) {
        var rbObj = rbLists[i];
        rbObj.onclick = (function (objId, rbObj) {
            return function () {
                leftListBoxSort(objId, rbObj);
            };
        })(objId, rbObj);
    }
}

// 左ボックスソートイベント
function leftListBoxSort(listBoxObjId, rbObj) {
    // オブジェクトの存在チェック(存在しない場合はスキップ)
    var sortBaseNode = document.getElementById(listBoxObjId);
    if (sortBaseNode === null) {
        return;
    }

    // 1件のみ0件はソートの意味がないのでそのまま終了
    if (sortBaseNode.length <= 1) {
        return;
    }

    var sortClone = sortBaseNode.cloneNode(true);
    sortClone.value = sortBaseNode.value;

    // リストボックスの選択肢ループ
    var optionArray = Array.prototype.slice.call(sortClone.options);

    // チェックボックスの値によって上記定義のソートメソッドを実行
    switch (rbObj.value) {
        case 'CodeASC':
            optionArray.sort(compareValueAsc);
            break;
        case 'CodeDesc':
            optionArray.sort(compareValueDesc);
            break;
        case 'NameASC':
            optionArray.sort(compareTextAsc);
            break;
        case 'NameDesc':
            optionArray.sort(compareTextDesc);
            break;
    }

    for (let i = 0; i < optionArray.length; i++) {
        sortClone.appendChild(sortClone.removeChild(optionArray[i]));
    }

    sortBaseNode.parentNode.replaceChild(sortClone, sortBaseNode);

    // フィルタ機能が有効な場合、画面で見えているクローンにも反映させる
    var cloneList = document.getElementById('WF_ListBoxCLONE');
    if (cloneList !== null) {
        leftListBoxFilter(cloneList, listBoxObjId);
    }

    // コード昇順
    function compareValueAsc(a, b) {
        if (a.value > b.value) {
            return 1;
        } else if (a.value < b.value) {
            return -1;
        } else {
            return 0;
        }
    }

    // 名称昇順
    function compareTextAsc(a, b) {
        var displayStringAPart = a.textContent.substring(a.textContent.indexOf(':'));
        var displayStringBPart = b.textContent.substring(b.textContent.indexOf(':'));
        if (displayStringAPart > displayStringBPart) {
            return 1;
        } else if (displayStringAPart < displayStringBPart) {
            return -1;
        } else {
            return 0;
        }
    }

    // コード降順
    function compareValueDesc(a, b) {
        if (a.value < b.value) {
            return 1;
        } else if (a.value > b.value) {
            return -1;
        } else {
            return 0;
        }
    }

    // 名称降順
    function compareTextDesc(a, b) {
        var displayStringAPart = a.textContent.substring(a.textContent.indexOf(':'));
        var displayStringBPart = b.textContent.substring(b.textContent.indexOf(':'));
        if (displayStringAPart < displayStringBPart) {
            return 1;
        } else if (displayStringAPart > displayStringBPart) {
            return -1;
        } else {
            return 0;
        }
    }
}

// LeftBoxのSelect選択時のtabキー移動の制御
function moveFocusLeftBox(event) {
    // event.which=9はtabキー押下のkeycode
    if (event.which == 9) {
        // テキストボックスの直前の要素を指定(onkeyupのタブ移動を考慮)
        document.getElementById('WF_LeftBoxSortCodeASC').focus();
    }
} 

// 左ボックスフィルタ拡張機能を追加
function addLeftBoxFilter(ListObj) {
    if (ListObj === null || ListObj === undefined) {
        return;
    }

    // フィルタテキスト及びフィルタ実行ボタンを生成するタグ
    var filterTable = '<table id="WF_LeftBoxFilter">\n' +
        '  <tr>\n' +
        '    <td><a class="ef"><input id="WF_LEFTBOXFILTER" type="text" value="" title="Filter Condition" />\n' +
        '    </a></td>\n' +
        '    <td><input id="WF_buttonFilter"  class="btn-sticky" type="button" value="検 索" />\n' +
        '    </td>\n' +
        '  </tr>\n' +
        '</table>\n';

    // サーバーより取得したリストボックスでの選択肢の表示非表示をCSSでOnOffできないので
    // 隠して、リストボックスのクローンを生成しクローンで選択肢の追加削除を行う準備

    // サーバーより取得したリストボックスをspanタグで括り隠す
    let wrapper = document.createElement('span');
    wrapper.style.display = 'none';
    ListObj.parentNode.appendChild(wrapper);
    // サーバーより取得したリストボックスのクローンをID=WF_ListBoxCLONEとして生成
    var listClone = '<select id="WF_ListBoxCLONE" size="4" onkeydown="moveFocusLeftBox(event);" >' + ListObj.innerHTML + '</select>';
    wrapper.appendChild(ListObj);
    //wrapper.insertAdjacentHTML('beforebegin', filterTable); //(ダブルクリックイベント防止)
    document.getElementById('button').insertAdjacentHTML('beforeend', filterTable);
    wrapper.insertAdjacentHTML('beforebegin', listClone);

    // フィルタボタンのイベントの紐づけ
    var leftFilterButton = document.getElementById('WF_buttonFilter');
    var leftListClone = document.getElementById('WF_ListBoxCLONE');
    leftFilterButton.onclick = (function (leftListClone, listBoxObj) {
        return function () {
            leftListBoxFilter(leftListClone, listBoxObj.id);
        };
    })(leftListClone, ListObj);

    // クローンのリストボックスのスタイルを指定
    leftListClone.className = "WF_ListBoxArea";

    // リストボックスのクローンにて選択されてイベントをバインド
    // クローンリストが選択されていたら、隠している本物のリストの選択肢も同じ状態にする。
    leftListClone.onchange = (function (leftListClone, ListObj) {
        return function () {
            var baseList = document.getElementById(ListObj.id);

            baseList.value = leftListClone.value;
            let hdnObjId = 'commonLeftListSelectedText';
            let hdnObj = document.getElementById(hdnObjId);
            if (hdnObj === null) {
                hdnObj = document.createElement('input');
                hdnObj.type = 'hidden';
                hdnObj.id = hdnObjId;
                hdnObj.name = hdnObjId;
                document.forms[0].appendChild(hdnObj);
                hdnObj = document.getElementById(hdnObjId);
            }
            let selectIdx = leftListClone.selectedIndex;
            hdnObj.value = leftListClone.options[selectIdx].text;
            
        };
    })(leftListClone, ListObj);

    // リストボックスのクローンのダブルクリックイベントバインド
    // 本物のリストのダブルクリックイベントを発火させる

    //######ワンクリックに変更　2019/12/26 #######
    //leftListClone.ondblclick = (function (ListObj) {
    leftListClone.onclick = (function (ListObj) {
        //######ワンクリックに変更　2019/12/26 #######
        return function () {
            ListboxDBclick();
        };
    })(ListObj);
}


// 左ボックスフィルタイベント
function leftListBoxFilter(leftListClone, listBoxObjId) {
    var filterCond = document.getElementById('WF_LEFTBOXFILTER').value.trim();
    if (filterCond === "") {
        filterCond = '.*';
    } else {
        filterCond = '.*' + filterCond.replace(/[\\^$.*+?()[\]{}|]/g, '\\$&') + '.*';
    }

    var listBoxObjBase = document.getElementById(listBoxObjId);
    var listBoxObjClone = listBoxObjBase.cloneNode(true);
    listBoxObjClone.value = listBoxObjBase.value;

    // 一旦画面表示上の選択ボックスクリア 
    for (let i = leftListClone.options.length - 1; i >= 0; i--) {
        leftListClone.remove(i);
    }

    // 検索条件にて絞り込み
    var reg = new RegExp(filterCond, "i");
    for (let i = 0; i < listBoxObjClone.length; i++) {
        var optionElm = listBoxObjClone.options[i];
        // 検索条件が未設定の場合はすべて対象、それ以外は検索条件に一致すること
        var targetText = optionElm.textContent;
        if (reg.test(targetText)) {
            optionClone = optionElm.cloneNode(true);
            leftListClone.appendChild(optionClone);
            if (optionElm.selected) {
                optionClone.selected = true;
            }
        }
    }

    listBoxObjBase.parentNode.replaceChild(listBoxObjClone, listBoxObjBase);
}

// 左ボックス検索処理拡張機能を追加
//  ※機能フラグ(0:無し, 1:項目指定)
function addLeftBoxsubmit(ListObj, SubmitParam) {
    // オブジェクトの存在チェック(存在しない場合はスキップ)
    if (ListObj === null || ListObj === undefined) {
        return;
    }

    // 名称検索用ラジオボタン追加
    var orderChooseTable = '<table id="WF_LeftBoxSubmit">\n' +
        '<tr>\n' +
        '    <td><input name="WF_LeftBoxParam" id="WF_LeftBoxParam"  type="text" value="' + SubmitParam + '"  title="Filter Param" />\n' +
        '    </td>\n' +
        '    <td><input id="WF_LeftBoxSubmit" type="button" value="検 索" onclick="ButtonClick(\'WF_LeftBoxSubmit\')" />\n' +
        '    </td>\n' +
        '  </tr>\n';

    orderChooseTable = orderChooseTable + '</table>\n';

    // 上記で作成したタグをリストボックス前に挿入
    // ListObj.insertAdjacentHTML('beforebegin', orderChooseTable);
    // リストボックス前ではなく、ボタンの後に挿入(ダブルクリックイベント防止)
    document.getElementById('button').insertAdjacentHTML('beforeend', orderChooseTable);
}


// ○左Box用処理（左Box表示/非表示切り替え）
function REF_Field_DBclick(repfield, fieldNM, tabNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD_REP').value = repfield;
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_LeftMViewChange').value = tabNo;
        document.getElementById('WF_LeftboxOpen').value = "Open";
        document.getElementById('WF_ButtonClick').value = "WF_Field_DBClick";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}
// ○左Box用処理（左Box表示/非表示切り替え）
function Field_DBclick(fieldNM, tabNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        // クリック位置取得
        var elem = document.getElementById(fieldNM);
        var rect = elem.getBoundingClientRect();
        document.getElementById("WF_saveTop").value = rect.top + rect.height;
        document.getElementById("WF_saveLeft").value = rect.left;

        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_LeftMViewChange').value = tabNo;
        document.getElementById('WF_LeftboxOpen').value = "Open";

        document.getElementById("WF_ButtonClick").value = "WF_Field_DBClick";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

// ○左BOX用処理（DBクリック選択+値反映）
function ListboxDBclick() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LeftboxOpen').value = "";
        document.getElementById("WF_ButtonClick").value = "WF_ListboxDBclick";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}
// ○左BOX用処理（DBクリック選択+値反映）
function WF_TableF_DbClick(callerObj) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        let keyValue = callerObj.dataset.key;
        let itemValues = callerObj.dataset.values;
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LeftboxOpen').value = "";
        document.getElementById('hdnLeftTableSelectedKey').value = keyValue;
        document.getElementById('WF_TBL_SELECT').value = itemValues;
        document.getElementById("WF_ButtonClick").value = "WF_ListboxDBclick";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}
// ○左BOX用処理（TextBox変更時、名称取得）
function TextBox_change(fieldNM) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_ButtonClick').value = "WF_LeftBoxSelectClick";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

// ○右Box用処理（右Box表示/非表示切り替え）
function r_boxDisplayNonSubmit() {
    if (document.getElementById("RF_RIGHTBOX").style.width === "0em") {
        document.getElementById("RF_RIGHTBOX").style.width = "25em";
        document.getElementById('WF_RightboxOpen').value = "Open";
        document.getElementById("rightb").style.display = "none";
    } else {
        document.getElementById("RF_RIGHTBOX").style.width = "0em";
        document.getElementById('WF_RightboxOpen').value = "";
        /* エラーレポートの内容が設定されている場合 */
        if (String(document.getElementById("RF_ERR_REPORT").value).length > 0) {
            document.getElementById("rightb").style.display = "block";
        }
    }
}

// ○右Box用処理（右Box表示/非表示切り替え）
function r_boxDisplay() {
    if (document.getElementById("RF_RIGHTBOX").style.width === "0em") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
        document.getElementById('WF_RightboxOpen').value = "Open";
        document.getElementById("rightb").style.display = "none";
        document.getElementById("WF_ButtonClick").value = "WF_RIGHT_VIEW_DBClick";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    } else {
        document.getElementById("RF_RIGHTBOX").style.width = "0em";
        document.getElementById('WF_RightboxOpen').value = "";
        /* エラーレポートの内容が設定されている場合 */
        if (String(document.getElementById("RF_ERR_REPORT").value).length > 0) {
            document.getElementById("rightb").style.display = "block";
        }
    }
}

// ○右BOX用処理（ラジオボタン）
function rightboxChange(tabNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_RightViewChange').value = tabNo;
        document.getElementById('WF_RightboxOpen').value = "Open";
        document.getElementById('WF_ButtonClick').value = "WF_RadioButonClick";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

// ○右BOX用処理（メモ変更）
function MEMOChange() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_MEMOChange";
        document.getElementById('WF_RightboxOpen').value = "Open";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}


// ○ドロップ処理（ドラッグドロップ入力）
function f_dragEvent(e) {
    document.getElementById("WF_MESSAGE").textContent = "ファイルアップロード開始";
    document.getElementById("WF_MESSAGE").style.color = "blue";
    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";

    // ドラッグされたファイル情報を取得
    var files = e.dataTransfer.files;

    // 送信用FormData オブジェクトを用意
    var fd = new FormData();

    // ファイル情報を追加する
    for (var i = 0; i < files.length; i++) {
        fd.append("files", files[i]);
    }

    // XMLHttpRequest オブジェクトを作成
    var xhr = new XMLHttpRequest();

    // 「POST メソッド」「接続先 URL」を指定
    xhr.open("POST", "../../inc/GRCO0100XLSUP.ashx", false);

    // イベント設定
    // ⇒XHR 送信正常で実行されるイベント
    xhr.onload = function (e) {
        if (e.currentTarget.status === 200) {
            document.getElementById('WF_ButtonClick').value = "WF_EXCEL_UPLOAD";
            //document.body.style.cursor = "wait";
            commonDispWait();
            document.forms[0].submit();                             //aspx起動
        } else {
            document.getElementById("WF_MESSAGE").textContent = "ファイルアップロードが失敗しました。";
            document.getElementById("WF_MESSAGE").style.color = "red";
            document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        }
    };

    // ⇒XHR 送信ERRで実行されるイベント
    xhr.onerror = function (e) {
        document.getElementById("WF_MESSAGE").textContent = "ファイルアップロードが失敗しました。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
    };

    // ⇒XHR 通信中止すると実行されるイベント
    xhr.onabort = function (e) {
        document.getElementById("WF_MESSAGE").textContent = "通信を中止しました。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
    };

    // ⇒送信中にタイムアウトエラーが発生すると実行されるイベント
    xhr.ontimeout = function (e) {
        document.getElementById("WF_MESSAGE").textContent = "タイムアウトエラーが発生しました。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
    };

    // 「送信データ」を指定、XHR 通信を開始する
    xhr.send(fd);
}

// ○メッセージクリア
function MsgClear() {
    document.getElementById("WF_MESSAGE").innerText = "";
}

// ○ヘルプBox用処理
function HelpDisplay() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "HELP";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

// ○ドロップ処理（処理抑止）
function f_dragEventCancel(event) {
    event.preventDefault();  //イベントをキャンセル
}


// ○ダウンロード処理
function f_ExcelPrint() {
    //// リンク参照
    //window.open(document.getElementById("WF_PrintURL").value, "view", "_blank");
    // ダウンロード（新たなウインドウを開かない）
    window.open(document.getElementById("WF_PrintURL").value, "view");
}
// ○ダウンロード処理フォルダ固定でバッチでフォルダ指定ダウンロード(テスト用)
// 対象[ダウンロードファイル名].batを生成し指定フォルダダウンロードバッチを作成する
function f_ExcelPrintTsst() {
    // リンク参照
    let url = document.getElementById("WF_PrintURL").value;
    // URLよりファイル名のみ抜き出し
    let arr = url.split("/");
    let fileName = arr[arr.length - 1];
    // バッチコマンド生成
    let textContents = "";
    textContents = textContents + 'cd c:\r\n';
    textContents = textContents + 'bitsadmin /transfer download \"' + url + '\" \"C:\\test\\' + fileName + '\"\r\n';
    // コマンドテキストよりファイルBlob生成
    var blob = new Blob([textContents], { "type": "text/plain" });
    // ダウンロード処理実行
    if (window.navigator.msSaveBlob) {
        // ブラウザIEの場合こちらの分岐
        window.navigator.msSaveBlob(blob, fileName + ".bat");
        // msSaveOrOpenBlobの場合はファイルを保存せずに開ける
        window.navigator.msSaveOrOpenBlob(blob, fileName + ".bat");
    } else {
        // ブラウザIE以外の場合はこちらの分岐
        let downloadLink = document.createElement("a");
        downloadLink.href = window.URL.createObjectURL(blob);
        downloadLink.download = fileName + ".bat";

        document.body.appendChild(downloadLink);
        downloadLink.click();
        document.body.removeChild(downloadLink);
    }
}

function f_PDFPrint() {
    // リンク参照
    window.open(document.getElementById("WF_PrintURL").value + "?" + (new Date()).getTime(), "view", "_blank");
}

// ○ダウンロード処理
function f_CSVPrint() {
    // ダウンロード（新たなウインドウを開かない）
    window.open(document.getElementById("WF_PrintURL").value, "view");
}

// ○各ボタン押下処理
function ButtonClick(btn) {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        //押下されたボタンを設定
        document.getElementById("WF_ButtonClick").value = btn;
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    } else {
        return false;
    }
}

// ○ディテール(タブ切替)処理
function DtabChange(tabNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_DTAB_CHANGE_NO').value = tabNo;
        document.getElementById('WF_ButtonClick').value = "WF_DTAB_Click";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

// ○ディテール(PDF表示)処理
function DtabPDFdisplay(filename) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_DTAB_PDF_DISP_FILE').value = filename;
        document.getElementById('WF_ButtonClick').value = "WF_DTAB_PDF_Click";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}

// ○ディテール(PDF表示)処理
function PDFselectChange() {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_ButtonClick').value = "WF_DTAB_PDF_Change";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}


/**
 * 列名(cellfiedlname)及び、対象のパネルIDを元に対象の列ID、テーブルオブジェクトを返却
 * @param {string}colName カラム名称(ヘッダーのcellfiedlnameの設定値)
 * @param {string}listId 対象パネルオブジェクトのID
 * @return {object} 戻りオブジェクト.ColumnNo=対象カラム番号,戻りオブジェクト.TargetTable=対象のデータテーブル
 * @example 使用方法 呼出し側で 
 * var [ご自由な変数] = getTargetColumnNoTable('USDBR', 'WF_LISTAREA');
 * var [ご自由なcellObj] = [ご自由な変数].TargetTable.rows[ご自由な行No].cells[[ご自由な変数].ColumnNo];
 * →[ご自由なcellObj].textContent とするとセルの文字が取り出せたりします
 */
function getTargetColumnNoTable(colName, listId) {
    var listArea = document.getElementById(listId);
    // 表エリアの描画なし
    if (listArea === null) {
        return null; // そのまま終了
    }
    var leftHeaderDiv = document.getElementById(listArea.id + "_HL");
    var rightHeaderDiv = document.getElementById(listArea.id + "_HR");
    var leftDataDiv = document.getElementById(listArea.id + "_DL");
    var rightDataDiv = document.getElementById(listArea.id + "_DR");
    if (leftHeaderDiv === null && rightHeaderDiv === null) {
        return null; // そのまま終了
    }
    // 左固定列のカラム名検索
    if (leftHeaderDiv !== null && leftHeaderDiv.getElementsByTagName("table") !== null) {
        let leftHeaderTable = leftHeaderDiv.getElementsByTagName("table")[0];
        let leftHeaderRow = leftHeaderTable.rows[0];
        for (let i = 0; i < leftHeaderRow.cells.length; i++) {
            let targetCell = leftHeaderRow.cells[i];
            if (targetCell.getAttribute("cellfiedlname") === colName) {
                let retDataTable = leftDataDiv.getElementsByTagName("table")[0];
                let retVal = { ColumnNo: i, TargetTable: retDataTable };
                return retVal;
            }
        }
    }
    // 右動的列のカラム名検索
    if (rightHeaderDiv !== null && rightHeaderDiv.getElementsByTagName("table") !== null) {
        let rightHeaderTable = rightHeaderDiv.getElementsByTagName("table")[0];
        let rightHeaderRow = rightHeaderTable.rows[0];
        for (let i = 0; i < rightHeaderRow.cells.length; i++) {
            let targetCell = rightHeaderRow.cells[i];
            if (targetCell.getAttribute("cellfiedlname") === colName) {
                let retDataTable = rightDataDiv.getElementsByTagName("table")[0];
                let retVal = { ColumnNo: i, TargetTable: retDataTable };
                return retVal;
            }
        }
    }
    // ここまで来た場合は検索結果なしnull返却
    return null;
}
/**
 * リストの共通イベント(ホイール、横スクロール)をバインド
 * @param {string}listObjId リストオブジェクトのID
 * @param {string}isPostBack 各ページで'<%= if(IsPostBack = True, "1", "0") %>'を指定（外部スクリプトではサーバータグが使用できない為)
 * @param {boolean}adjustHeight 高さを調整するか
 * @param {boolean}keepHScrollWhenPostBack 省略可 ポストバック時に横スクロールを保持するか(True:保持(デフォルト),False:保持しない)
 * @param {boolean}resetXposFirstLoad 省略可 初回ロード時にスクロールバー位置の記憶をリセットするか(True:リセット(デフォルト),False:保持))
 * @param {boolean}useWheelEvent 省略可 マウスホイールイベントを使用するか(True:使用する(デフォルト),False:使用しない))
 * @return {undefined} なし
 * @example 使用方法  
 * bindListCommonEvents('<%= Me.WF_LISTAREA.ClientId %>','<%= if(IsPostBack = True, "1", "0") %>');
 */
function bindListCommonEvents(listObjId, isPostBack, adjustHeight, keepHScrollWhenPostBack, resetXposFirstLoad, useWheelEvent) {
    // 第3引数が未指定の場合
    if (adjustHeight === undefined) {
        adjustHeight = false;
    }
    // 第4引数が未指定の場合
    if (keepHScrollWhenPostBack === undefined) {
        keepHScrollWhenPostBack = true;
    }
    // 第5引数が未指定の場合
    if (resetXposFirstLoad === undefined) {
        resetXposFirstLoad = true;
    }
    // 第6引数が未指定の場合
    if (useWheelEvent === undefined) {
        useWheelEvent = true;
    }

    var listObj = document.getElementById(listObjId);
    // そもそもリストがレンダリングされていなければ終了
    if (listObj === null) {
        return;
    }
    // Mouseホイールイベントのバインド
    if (useWheelEvent) {
        var mousewheelevent = 'onwheel' in listObj ? 'wheel' : 'onmousewheel' in listObj ? 'mousewheel' : 'DOMMouseScroll';
        listObj.addEventListener(mousewheelevent, commonListMouseWheel, true);
    }
    // 横スクロールイベントのバインド
    // 可変列ヘッダーテーブル、可変列データテーブルのオブジェクトを取得
    var headerTableObj = document.getElementById(listObjId + '_HR');
    var dataTableObj = document.getElementById(listObjId + '_DR');
    // 可変列の描画がない場合はそのまま終了
    if (headerTableObj === null || dataTableObj === null) {
        return;
    }
    // スクロールイベントのバインド
    dataTableObj.addEventListener('scroll', (function (listObj) {
        return function () {
            commonListScroll(listObj);
        };
    })(listObj), false);

    // 画面キーダウンイベントのバインド
    // GridView処理（矢印処理）
    if (useWheelEvent) {
        document.addEventListener('keydown', (function () {
            return function () {
                var objSubmit = document.getElementById('MF_SUBMIT');
                var objEventHandler = document.getElementById('WF_ButtonClick');
                //var objMouseWheel = document.getElementById('WF_MouseWheel');
                // ↑キー押下時
                if (window.event.keyCode === 38) {
                    if (objSubmit.value === 'FALSE') {
                        // リストの現在見えている位置が最上部の場合はポストバックせず終了
                        var listPosition = document.getElementById("WF_GridPosition");
                        if (listPosition !== null) {
                            if (listPosition.value === '' || listPosition.value === '1') {
                                return false;
                            }
                        }
                        // スクロール前に変更FocusOutしイベントを発火させる
                        let actvElm = document.activeElement;
                        if (actvElm !== null) {
                            actvElm.blur();
                        } 
                        objSubmit.value = 'TRUE';
                        //objMouseWheel.value = '-';
                        objEventHandler.value = "WF_MouseWheelDown";
                        //document.body.style.cursor = "wait";
                        commonDispWait();
                        document.forms[0].submit();  //aspx起動
                        return false;
                    }
                }
                // ↓キー押下時
                if (window.event.keyCode === 40) {
                    if (objSubmit.value === 'FALSE') {
                        // スクロール前に変更FocusOutしイベントを発火させる
                        let actvElm = document.activeElement;
                        if (actvElm !== null) {
                            actvElm.blur();
                        } 
                        objSubmit.value = 'TRUE';
                        //objMouseWheel.value = '+';
                        objEventHandler.value = "WF_MouseWheelUp";
                        //document.body.style.cursor = "wait";
                        commonDispWait();
                        document.forms[0].submit();  //aspx起動
                        return false;
                    }
                }
            };
        })(), false);
    }

    // スクロールを保持する場合
    if (isPostBack === '0' && keepHScrollWhenPostBack && resetXposFirstLoad) {
        // 初回ロード時は左スクロール位置を0とる
        setCommonListScrollXpos(listObj.id, '0');
    }
    // ポストバック時は保持したスクロール位置に戻す
    if (isPostBack === '1' && keepHScrollWhenPostBack) {
        var xpos = getCommonListScrollXpos(listObj.id);
        dataTableObj.scrollLeft = xpos;
        var e = document.createEvent("UIEvents");
        e.initUIEvent("scroll", true, true, window, 1);
        dataTableObj.dispatchEvent(e);
    }
    //高さ調整
    if (adjustHeight === true) {
        /* 現在の表示を調整 */
        commonListAdjustHeight(listObj.id);
        /* リサイズイベントにバインド */
        window.addEventListener('resize', function () {
            commonListAdjustHeight(listObj.id);
        }, false);
    }
    bindCommonListHighlight(listObj.id);
}
/* 共通リストのハイライトイベント */
function bindCommonListHighlight(listObjId) {
    // 可変列ヘッダーテーブル、可変列データテーブルのオブジェクトを取得
    var leftDataDivObj = document.getElementById(listObjId + '_DL');
    var rightDataDivObj = document.getElementById(listObjId + '_DR');
    if (leftDataDivObj === null || rightDataDivObj === null) {
        return;
    }
    var leftTrList = leftDataDivObj.getElementsByTagName('tr');
    var rightTrList = rightDataDivObj.getElementsByTagName('tr');
    for (let i = 0; i < leftTrList.length; i++) {
        var leftTr = leftTrList[i];
        var rightTr = null;
        if (rightTrList !== null) {
            rightTr = rightTrList[i];
        }
        // 左のEventListener設定
        leftTr.addEventListener('mouseover', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.add("hover");
                rightTr.classList.add("hover");
            };
        })(leftTr, rightTr), false);
        // 左のEventListener設定
        leftTr.addEventListener('mouseout', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.remove("hover");
                rightTr.classList.remove("hover");
            };
        })(leftTr, rightTr), false);
        // 右のEventListener設定
        rightTr.addEventListener('mouseover', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.add("hover");
                rightTr.classList.add("hover");
            };
        })(leftTr, rightTr), false);
        // 右のEventListener設定
        rightTr.addEventListener('mouseout', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.remove("hover");
                rightTr.classList.remove("hover");
            };
        })(leftTr, rightTr), false);
    }
}
/**
 * リストデータ部スクロール共通処理（ヘッダー部のスクロールを連動させる)
 * @param {object}listObj リスト全体のオブジェクト
 * @return {undefined} なし
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function commonListScroll(listObj) {
    var rightHeaderTableObj = document.getElementById(listObj.id + '_HR');
    var rightDataTableObj = document.getElementById(listObj.id + '_DR');
    var leftDataTableObj = document.getElementById(listObj.id + '_DL');

    setCommonListScrollXpos(listObj.id, rightDataTableObj.scrollLeft);
    rightHeaderTableObj.scrollLeft = rightDataTableObj.scrollLeft; // 左右連動させる
    leftDataTableObj.scrollTop = rightDataTableObj.scrollTop; // 上下連動させる
}
/**
 * リストボックスの変更をキャッチするイベントを追加
 * @return {undefined} なし
 * @example 個別ページからの使用想定はなし,data-generatedのinputタグの変更を保持する
 */
function bindcommonListChangedInput() {
    let pnlOBJECTS = document.querySelectorAll("div[data-generated='1']");
    if (pnlOBJECTS === null) {
        return;
    }
    /* パネルループ複数パネルを考慮 */
    for (let i = 0; i < pnlOBJECTS.length; i++) {
        let pnlObj = pnlOBJECTS[i];
        let inputTextOBJECTS = pnlObj.querySelectorAll("input[type='text']");
        /* 一覧にテキストボックスが存在しなければ次のパネルへスキップ */
        if (inputTextOBJECTS === null) {
            continue;
        }
        if (inputTextOBJECTS.length === 0) {
            continue;
        }
        /* パネルのIDを取得 */
        let pnlId = pnlObj.id;
        /* 変更フィールド保持用のhiddenタグをパネル内に生成 */
        let hiddenModInfoItem = document.createElement('input');
        hiddenModInfoItem.type = 'hidden';
        let hiddenModInfoItemId = pnlId + 'modval';
        hiddenModInfoItem.id = hiddenModInfoItemId;
        hiddenModInfoItem.name = hiddenModInfoItemId;
        pnlObj.appendChild(hiddenModInfoItem);
        
        /* パネル内のテキストボックスオブジェクトループ */
        for (let j = 0; j < inputTextOBJECTS.length; j++) {
            /* テキストボックスのIDよりフィールド名取得
             * txt[panelId]フィールド名lineCntでフィールド名以外を除去する */
            let inputTextObj = inputTextOBJECTS[j];
            let lineCnt = inputTextObj.attributes.getNamedItem("rownum").value;
            let fieldName = inputTextObj.id.substring(("txt" + pnlId).length);
            fieldName = fieldName.substring(0, fieldName.length - lineCnt.length);
            /* 変更イベントをバインド */
            inputTextObj.parentNode.addEventListener('change', (function (inputTextObj, lineCnt, fieldName, hiddenModInfoItemId) {
                return function () {
                    commonListChangedInput(inputTextObj, lineCnt, fieldName,  hiddenModInfoItemId);
                };
            })(inputTextObj, lineCnt, fieldName, hiddenModInfoItemId), false);
        }
    }

}
/**
 * リストボックスの変更をキャッチするイベントを追加
 * @param {Element} inputTextObj テキストボックス
 * @param {string} lineCnt 行番号
 * @param {string} fieldName フィールド名
 * @param {string} hiddenModInfoItemId 変更フィールド保持hiddenID
 * @return {undefined} なし
 * @example 個別ページからの使用想定はなし,data-generatedのinputタグの変更を保持する
 */
function commonListChangedInput(inputTextObj, lineCnt, fieldName, hiddenModInfoItemId) {  
    let hdnObj = document.getElementById(hiddenModInfoItemId);
    if (hdnObj === null) {
        return;
    }
    /* 変更フィールド情報の文字列をJson配列に変換 */
    let modValuesString = hdnObj.value;
    let jsonVal = [];
    if (modValuesString !== '') {
        jsonVal = JSON.parse(modValuesString);
    }
    /* 一旦対象LineCnt,フィールド名を削除 */
    // 配列より削除
    let removedjsonValObj = jsonVal.filter(function (itm) { return !(itm.FieldName === fieldName && itm.LineCnt === lineCnt);  });
    jsonVal = removedjsonValObj;

    let modVal = inputTextObj.value;
    if (inputTextObj.hasAttribute("data-withenterval")) {
        modVal = inputTextObj.dataset.withenterval;
    }
    
    let modItem;
    modItem = {
        FieldName: fieldName,
        LineCnt: lineCnt,
        ModValue: modVal
    };
    jsonVal.push(modItem);
    encodedValue = '';
    if (jsonVal.length > 0) {
        encodedValue = JSON.stringify(jsonVal);
    }
    hdnObj.value = encodedValue;
}
/**
 * リストの高さを調節する
 * @param {string}listId リスト全体のオブジェクトID
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function commonListAdjustHeight(listId) {
    var userAgent = window.navigator.userAgent.toLowerCase();
    var browserAjust = -1;
    if (userAgent.indexOf('msie') !== -1 ||
        userAgent.indexOf('trident') !== -1) {
        //IE
    } else if (userAgent.indexOf('edge') !== -1) {
        //Edge
    } else if (userAgent.indexOf('chrome') !== -1) {
        //Chrome
        //browserAjust = -10;

    } else if (userAgent.indexOf('safari') !== -1) {
        //Safari
    } else if (userAgent.indexOf('firefox') !== -1) {
        //FireFox
    } else if (userAgent.indexOf('opera') !== -1) {
        //Opera
    }

    var listObj = document.getElementById(listId);
    var listObjParent = listObj.parentNode;
    var parentRect = listObjParent.getBoundingClientRect();
    var listRect = listObj.getBoundingClientRect();

    var listHeight = parentRect.top + listObjParent.clientHeight - listRect.top;

    //alert(parentBottom);
    listObj.style.height = (listHeight + browserAjust - 15).toString() + 'px';
}
/**
 * リストの横スクロール位置をwebStrage(セッションストレージ)に保持した値より取得する
 * @param {string}listId リスト全体のオブジェクトID
 * @return {string} リスト設定文字
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function getCommonListScrollXpos(listId) {
    var saveKey = document.forms[0].id + listId + "xScrollPos";
    var retValue = sessionStorage.getItem(saveKey);
    if (retValue === null) {
        retValue = '';
    }
    return retValue;
}

/**
 * リストの横スクロール位置をwebStrage(セッションストレージ)に保持する
 * @param {string}listId リスト全体のオブジェクトID
 * @param {string}val リストに保持する値
 * @return {undefined} なし
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function setCommonListScrollXpos(listId, val) {
    var saveKey = document.forms[0].id + listId + "xScrollPos";
    sessionStorage.setItem(saveKey, val);
}

/**
 * 一覧表のマウスホイールイベント
 * @param {Event}event 未使用
 * @returns {boolean} Boolean
 * @example サーバーにポストしスクロール分の一覧データを表示
 */
function commonListMouseWheel(event) {
    var objSubmit = document.getElementById("MF_SUBMIT");
    //var objMouseWheel = document.getElementById("WF_MouseWheel");
    var objEventHandler = document.getElementById('WF_ButtonClick');
    if (objSubmit.value === "FALSE") {
        if (window.event.wheelDelta < 0) {
            //objMouseWheel.value = "+";
            objEventHandler.value = "WF_MouseWheelUp";
        } else {
            // リストの現在見えている位置が最上部の場合はポストバックせず終了
            var listPosition = document.getElementById("WF_GridPosition");
            if (listPosition !== null) {
                if (listPosition.value === '' || listPosition.value === '1') {
                    return false;
                }
            }
            //objMouseWheel.value = "-";
            objEventHandler.value = "WF_MouseWheelDown";

        }
        // スクロール前に変更FocusOutしイベントを発火させる
        let actvElm = document.activeElement;
        if (actvElm !== null) {
            actvElm.blur();
        } 
        objSubmit.value = "TRUE";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    } else {
        return false;
    }
}
/**
 * リストのソートイベント
 * @param {string}listId 対象リストのID
 * @param {string}fieldId ソート対象のフィールド
 * @returns {boolean} Boolean
 * @example ソート設定を記載しサーバーへサブミット
 */
function commonListSortClick(listId, fieldId) {
    var objSubmit = document.getElementById('MF_SUBMIT');
    var formId = document.forms[0].id;
    var sortOrderObj = document.getElementById('hdnListSortValue' + formId + listId);
    var objEventHandler = document.getElementById('WF_ButtonClick');
    var listPosition = document.getElementById('WF_GridPosition');
    if (objSubmit === null || sortOrderObj === null) {
        return false;
    }

    var sortOrderValue = sortOrderObj.value;
    if (sortOrderValue === '') {
        sortOrderValue = fieldId + ' ASC';
    } else {
        var sortOrderValueArray = [];
        if (sortOrderValue !== '') {
            sortOrderValueArray = sortOrderValue.split(',');
        }
        var keyValueSort = {};
        for (let i = 0; i < sortOrderValueArray.length; i++) {
            var sortOrder = sortOrderValueArray[i];
            keyValueSort[sortOrder.split(' ')[0]] = { sort: i, value: sortOrder.split(' ')[1] };
        }

        if (keyValueSort[fieldId]) {
            if (keyValueSort[fieldId].value === "ASC") {
                keyValueSort[fieldId].value = "DESC";
            } else if (keyValueSort[fieldId].value === "DESC") {
                delete keyValueSort[fieldId];
            }
        } else {
            keyValueSort[fieldId] = { sort: 9999, value: "ASC" };
        }
        var retArray = [];
        for (key in keyValueSort) {
            retArray.push({ field: key, sort: keyValueSort[key].sort, value: keyValueSort[key].value });
        }
        retArray.sort(function (a, b) {
            if (a.sort < b.sort) return -1;
            if (a.sort > b.sort) return 1;
            return 0;
        });
        sortOrderValue = '';
        for (let i = 0; i < retArray.length; i++) {
            if (sortOrderValue === '') {
                sortOrderValue = retArray[i].field + ' ' + retArray[i].value;
            } else {
                sortOrderValue = sortOrderValue + ',' + retArray[i].field + ' ' + retArray[i].value;
            }
        }
    }
    sortOrderObj.value = sortOrderValue;
    document.getElementById("WF_LIST_SORTING").value = sortOrderValue;
    objSubmit.value = "TRUE";
    objEventHandler.value = "WF_LIST_SORTING";
    //document.body.style.cursor = "wait";
    commonDispWait();
    document.forms[0].submit();                            //aspx起動
}

/**
 *  上部一覧表のリストダブルクリックイベント
 * @param {object} obj TR(行)オブジェクト
 * @param {string} lineCnt 行No
 * @return {undefined} なし
 * @description 詳細エリアのタブ変更時イベント
 */
function ListDbClick(obj, lineCnt) {
    var objSubmit = document.getElementById('MF_SUBMIT');
    var objListDbClick = document.getElementById('WF_GridDBclick');
    var objEventHandler = document.getElementById('WF_ButtonClick');
    // 対象のオブジェクトが存在していない場合は終了
    if (objSubmit === null || objListDbClick === null) {
        return;
    }
    // SUBMITフラグを見て処理実行
    if (objSubmit.value === 'FALSE') {
        objSubmit.value = 'TRUE';
        objListDbClick.value = lineCnt;
        var objIsHideDetailBox = document.getElementById('WF_IsHideDetailBox');
        if (objIsHideDetailBox !== null) {
            objIsHideDetailBox.value = '0';
        }
        objEventHandler.value = "WF_GridDBclick";
        //document.body.style.cursor = "wait";
        commonDispWait();
        document.forms[0].submit();
    }
}
var commonTapCnt = 0;
/**
 * リストの行およびヘルプのダブルクリックイベントをダブルタップでも反応させる
 * @return {undefined} なし
 * @description ダブルタップされたら、ダブルクリックイベントを発火させる
 */
function commonBindDblTapEvents() {
    // タッチイベントが存在しないデバイスか判定
    if (window.ontouchstart !== null) {
        // タッチデバイスではない場合終了
        return;
    }
    // ダブルクリックが紐づいているオブジェクトの検索
    let dblClickOBJECTS = document.querySelectorAll('[ondblclick*="r_boxDisplay("],[ondblclick*="ListDbClick("]');
    // オブジェクトが存在しない場合は終了
    if (dblClickOBJECTS === null) {
        return;
    }
    if (dblClickOBJECTS.length === 0) {
        return;
    }
    // ダブルタップ検知イベントのバインド
    for (let i = 0; i < dblClickOBJECTS.length; i++) {
        let dblClickObj = dblClickOBJECTS[i];
        dblClickObj.addEventListener('touchstart', (function (dblClickObj) {
            return function () {
                if (!commonTapCnt) {
                    // タップ回数を増加
                    commonTapCnt = commonTapCnt + 1;

                    // 350ミリ秒だけ、タップ回数を維持
                    setTimeout(function () {
                        commonTapCnt = 0;
                    }, 350);
                } else {
                    // ダブルタップされたら自身に紐づいているダブルクリックイベントを発火
                    commonTapCnt = 0;
                    var evt = document.createEvent('MouseEvent');
                    evt.initMouseEvent('dblclick', !0, !0, window, 0, 0, 0, 0, 0, !1, !1, !1, !1, 0, null);
                    dblClickObj.dispatchEvent(evt);
                    event.stopPropagation(); // ブラウザのタップアクション拡大をさせない対策
                }

            };
        })(dblClickObj), true);
    }
}
/**
 * 一覧表の情報列が存在する場合ハイライトする情報を仕込む(cssでハイライトは定義)
 * @return {undefined} なし
 * @description 左ボックステーブル表示のフィルタイベント
 */
function commonSetHasCmpResultsInfoToHighlight() {
    let generatedTables = document.querySelectorAll("div[data-generated='1']");
    if (generatedTables === null) {
        return;
    }
    if (generatedTables.length === 0) {
        return;
    }
    for (let i = 0, len = generatedTables.length; i < len; ++i) {
        let generatedTable = generatedTables[i];
        let panelId = generatedTable.id;
        // 情報フィールドが存在するかチェック
        let orderStatusFieldName = 'CMPRESULTSNAME';
        let infoHeader = generatedTable.querySelector("th[cellfieldname='" + orderStatusFieldName + "']");
        if (infoHeader === null) {
            //存在しない場合はスキップ
            continue;
        }
        // リストの列番号取得
        let colIdx = infoHeader.cellIndex;
        // 右可変行オブジェクトの取得
        let dataAreaDrObj = document.getElementById(panelId + "_DR");
        //右可変行が未存在なら終了
        if (dataAreaDrObj === null) {
            return;
        }
        let rightTableObj = dataAreaDrObj.querySelector('table');
        if (rightTableObj === null) {
            return;
        }
        let leftTableObj = document.getElementById(panelId + "_DL").querySelector('table');
        for (let rowIdx = 0, rowlen = rightTableObj.rows.length; rowIdx < rowlen; rowIdx++) {
            // ありえないがデータ列のインデックス（最大カラム数）が情報カラムの位置より小さい場合
            if (rightTableObj.rows[rowIdx].cells.length < colIdx) {
                // ループの終了
                break;
            }

            let cellObj = rightTableObj.rows[rowIdx].cells[colIdx];
            if (cellObj.textContent === '' || cellObj.textContent === '一致') {
                continue;
            }
            rightTableObj.rows[rowIdx].classList.add('hasCmpResultsInfoValue');
            leftTableObj.rows[rowIdx].classList.add('hasCmpResultsInfoValue');
            ////ワーニング（黄色）判定
            //if (cellObj.textContent === '検査間近有'
            //    || cellObj.textContent === '前回黒油'
            //    || cellObj.textContent === '前回揮発油'
            //    || cellObj.textContent === '前回灯軽油') {
            //    rightTableObj.rows[rowIdx].classList.add('warnInfo');
            //    leftTableObj.rows[rowIdx].classList.add('warnInfo');
            //}
        }
    }
}
function commonSetHasOrderInfoToHighlight() {
    let generatedTables = document.querySelectorAll("div[data-generated='1']");
    if (generatedTables === null) {
        return;
    }
    if (generatedTables.length === 0) {
        return;
    }
    for (let i = 0, len = generatedTables.length; i < len; ++i) {
        let generatedTable = generatedTables[i];
        let panelId = generatedTable.id;
        // 情報フィールドが存在するかチェック
        let orderStatusFieldName = 'ORDERINFONAME';
        let infoHeader = generatedTable.querySelector("th[cellfieldname='" + orderStatusFieldName + "']");
        if (infoHeader === null) {
            //存在しない場合はスキップ
            continue;
        }
        // リストの列番号取得
        let colIdx = infoHeader.cellIndex;
        // 右可変行オブジェクトの取得
        let dataAreaDrObj = document.getElementById(panelId + "_DR");
        //右可変行が未存在なら終了
        if (dataAreaDrObj === null) {
            return;
        }
        let rightTableObj = dataAreaDrObj.querySelector('table');
        if (rightTableObj === null) {
            return;
        }
        let leftTableObj = document.getElementById(panelId + "_DL").querySelector('table');
        for (let rowIdx = 0, rowlen = rightTableObj.rows.length; rowIdx < rowlen; rowIdx++) {
            // ありえないがデータ列のインデックス（最大カラム数）が情報カラムの位置より小さい場合
            if (rightTableObj.rows[rowIdx].cells.length < colIdx) {
                // ループの終了
                break;
            }
            
            let cellObj = rightTableObj.rows[rowIdx].cells[colIdx];
            if (cellObj.textContent === '' || cellObj.textContent === '積置') {
                continue;
            }
            rightTableObj.rows[rowIdx].classList.add('hasOrderInfoValue');
            leftTableObj.rows[rowIdx].classList.add('hasOrderInfoValue');
            //ワーニング（黄色）判定
            if (cellObj.textContent === '検査間近有'
                || cellObj.textContent === '前回黒油'
                || cellObj.textContent === '前回揮発油'
                || cellObj.textContent === '前回灯軽油') {
                rightTableObj.rows[rowIdx].classList.add('warnInfo');
                leftTableObj.rows[rowIdx].classList.add('warnInfo');
            }
        }
    }
}
function commonSetHasKaisouInfoToHighlight() {
    let generatedTables = document.querySelectorAll("div[data-generated='1']");
    if (generatedTables === null) {
        return;
    }
    if (generatedTables.length === 0) {
        return;
    }
    for (let i = 0, len = generatedTables.length; i < len; ++i) {
        let generatedTable = generatedTables[i];
        let panelId = generatedTable.id;
        // 情報フィールドが存在するかチェック
        let kaisouStatusFieldName = 'KAISOUINFONAME';
        let infoHeader = generatedTable.querySelector("th[cellfieldname='" + kaisouStatusFieldName + "']");
        if (infoHeader === null) {
            //存在しない場合はスキップ
            continue;
        }
        // リストの列番号取得
        let colIdx = infoHeader.cellIndex;
        // 右可変行オブジェクトの取得
        let dataAreaDrObj = document.getElementById(panelId + "_DR");
        //右可変行が未存在なら終了
        if (dataAreaDrObj === null) {
            return;
        }
        let rightTableObj = dataAreaDrObj.querySelector('table');
        if (rightTableObj === null) {
            return;
        }
        let leftTableObj = document.getElementById(panelId + "_DL").querySelector('table');
        for (let rowIdx = 0, rowlen = rightTableObj.rows.length; rowIdx < rowlen; rowIdx++) {
            // ありえないがデータ列のインデックス（最大カラム数）が情報カラムの位置より小さい場合
            if (rightTableObj.rows[rowIdx].cells.length < colIdx) {
                // ループの終了
                break;
            }

            let cellObj = rightTableObj.rows[rowIdx].cells[colIdx];
            if (cellObj.textContent === '' || cellObj.textContent === '積置') {
                continue;
            }
            rightTableObj.rows[rowIdx].classList.add('hasKaisouInfoValue');
            leftTableObj.rows[rowIdx].classList.add('hasKaisouInfoValue');
            //ワーニング（黄色）判定
            if (cellObj.textContent === '検査間近有'
                || cellObj.textContent === '前回黒油'
                || cellObj.textContent === '前回揮発油'
                || cellObj.textContent === '前回灯軽油') {
                rightTableObj.rows[rowIdx].classList.add('warnInfo');
                leftTableObj.rows[rowIdx].classList.add('warnInfo');
            }
        }
    }
}

/**
 * 一覧表の受注進行ステータス列が存在する場合ハイライトする情報を仕込む(cssでハイライトは定義)
 * @return {undefined} なし
 * @description 左ボックステーブル表示のフィルタイベント
 */
function commonSetHasOrderStatusToHighlight() {
    let generatedTables = document.querySelectorAll("div[data-generated='1']");
    if (generatedTables === null) {
        return;
    }
    if (generatedTables.length === 0) {
        return;
    }
    for (let i = 0, len = generatedTables.length; i < len; ++i) {
        let generatedTable = generatedTables[i];
        let panelId = generatedTable.id;
        // 情報フィールドが存在するかチェック
        let orderStatusFieldName = 'ORDERSTATUSNAME';
        let infoHeader = generatedTable.querySelector("th[cellfieldname='" + orderStatusFieldName + "']");
        if (infoHeader === null) {
            //存在しない場合はスキップ
            continue;
        }
        // リストの列番号取得
        let colIdx = infoHeader.cellIndex;
        // 右可変行オブジェクトの取得
        let dataAreaDrObj = document.getElementById(panelId + "_DR");
        //右可変行が未存在なら終了
        if (dataAreaDrObj === null) {
            return;
        }
        let rightTableObj = dataAreaDrObj.querySelector('table');
        if (rightTableObj === null) {
            return;
        }
        let leftTableObj = document.getElementById(panelId + "_DL").querySelector('table');
        for (let rowIdx = 0, rowlen = rightTableObj.rows.length; rowIdx < rowlen; rowIdx++) {
            // ありえないがデータ列のインデックス（最大カラム数）が情報カラムの位置より小さい場合
            if (rightTableObj.rows[rowIdx].cells.length < colIdx) {
                // ループの終了
                break;
            }

            let cellObj = rightTableObj.rows[rowIdx].cells[colIdx];
            if (cellObj.textContent !== '受注受付' && cellObj.textContent !== '受注キャンセル') {
                continue;
            }
            rightTableObj.rows[rowIdx].classList.add('hasOrderInfoValue');
            leftTableObj.rows[rowIdx].classList.add('hasOrderInfoValue');
            //ワーニング（黄色）判定
            if (cellObj.textContent === '受注受付') {
                rightTableObj.rows[rowIdx].classList.add('warnInfo');
                leftTableObj.rows[rowIdx].classList.add('warnInfo');
            }
        }
    }
}

/**
 * 左ボックステーブル表示の検索ボタン押下時イベント
 * のタグを追加する
 * @return {undefined} なし
 * @description 左ボックステーブル表示のフィルタイベント
 */
function commonLeftTableFilter() {
    // 念の為表示エリアのオブジェクト有無確認（なければ終了）
    let leftTableArea = document.getElementById('pnlLeftList');
    if (leftTableArea === null) {
        return;
    }
    let findTextObj = document.getElementById('txtSearchLeftTable');
    if (findTextObj === null) {
        return;
    }
    let findText = findTextObj.value;
    let hiddenList = leftTableArea.querySelectorAll('.leftTableDataRow[style*="display: none"],.leftTableDataRow[style*="display : none"]');
    for (let i = 0; i < hiddenList.length; i++) {
        hiddenList[i].style.display = '';
    }
    let userAgent = window.navigator.userAgent.toLowerCase();
    // 検索文字が無い場合は全部表示させ終了
    if (findText === '') {
        commonLeftTableMarkLastRow(leftTableArea);
        if (userAgent.indexOf('msie') !== -1 ||
            userAgent.indexOf('trident') !== -1) {
            //IE(display:stickyが効かない為IEはこれでカバー)
            commonLeftTableScroll(leftTableArea);
        }
        return;
    }
    // 検索文字に一致しない行は非表示
    let searchList = leftTableArea.querySelectorAll('.leftTableDataRow');
    for (let i = 0; i < searchList.length; i++) {
        let rowObj = searchList[i];
        let foundCell = rowObj.querySelectorAll('span');
        let isFound = false;
        if (foundCell === null) {
            continue;
        }
        for (let j = 0; j < foundCell.length; j++) {
            if (foundCell[j].textContent.indexOf(findText) >= 0) {
                isFound = true;
                continue;
            }
        }
        // ここまで来た場合は全セル一致なしの為、非表示
        if (isFound === false) {
            rowObj.style.display = 'none';
        }
    }
    commonLeftTableMarkLastRow(leftTableArea);
    
    if (userAgent.indexOf('msie') !== -1 ||
        userAgent.indexOf('trident') !== -1) {
        //IE(display:stickyが効かない為IEはこれでカバー)
        commonLeftTableScroll(leftTableArea);
    }
    
}
/* 左ボックステーブルの最終行に印をつける */
/**
 * 左ボックステーブルの最終行に印をつける
 * @param {Element} leftTableArea 左ボックステーブルエリア
 * @return {undefined} なし
 * @description 左ボックステーブル表示のフィルタイベント
 */
function commonLeftTableMarkLastRow(leftTableArea) {
    let curLastRow = leftTableArea.querySelector('.leftTableDataRow.lastRow');
    if (curLastRow !== null) {
        curLastRow.classList.remove('lastRow');
    }

    let displayRowList = leftTableArea.querySelectorAll('.leftTableDataRow:not([style*="display:none"]):not([style*="display: none"])');
    let currentOrder = 0;
    let curIndex = 0;
    for (let i = 0; i < displayRowList.length; i++) {
        let rowObj = displayRowList[i];
        let styleOrder = Number(rowObj.style.order);
        if (currentOrder < styleOrder) {
            currentOrder = styleOrder;
            curIndex = i;
        }
    }
    if (currentOrder !== 0) {
        let rowObj = displayRowList[curIndex];
        rowObj.classList.add('lastRow');
    }
}
/**
 * 左ボックスのテーブル補正
 * @return {undefined} なし
 * @description 詳細エリアのタブ変更時イベント
 */
function commonLeftTableHeaderFixed() {
    let leftTableObj = document.getElementById('pnlLeftList');
    if (leftTableObj === null) {
        return;
    }
    let headerArea = leftTableObj.querySelector('.leftTableHeaderWrapper');
    let headerRowArea = leftTableObj.querySelector('.leftTableHeader');
    let dataArea = leftTableObj.querySelector('.leftTableDataWrapper');
    if (headerArea === null) {
        return;
    }
    if (headerRowArea === null) {
        return;
    }
    if (dataArea === null) {
        return;
    }
    headerArea.style.position = "absolute";
    headerRowArea.style.position = "fixed";
    headerRowArea.style.overflow = "hidden";
    headerRowArea.style.zIndex = "2";
    headerRowArea.style.width = leftTableObj.clientWidth + 'px';
    dataArea.style.position = "relative";
    dataArea.style.top = headerRowArea.clientHeight + 'px';

    leftTableObj.addEventListener('scroll', (function (leftTableObj) {
        return function () {
            commonLeftTableScroll(leftTableObj);
        };
    })(leftTableObj), false);
    
    window.addEventListener('resize', (function (leftTableObj) {
        return function () {
            commonLeftTableScroll(leftTableObj);
        };
    })(leftTableObj), false);
    
}
function commonLeftTableScroll(leftTableObj) {
    let headerArea = leftTableObj.querySelector('.leftTableHeaderWrapper');
    let headerRowArea = leftTableObj.querySelector('.leftTableHeader');
    let dataArea = leftTableObj.querySelector('.leftTableDataWrapper');
    if (headerArea === null) {
        return;
    }
    if (headerRowArea === null) {
        return;
    }
    if (dataArea === null) {
        return;
    }
    headerRowArea.style.width = leftTableObj.clientWidth + 'px';
    headerRowArea.scrollLeft = leftTableObj.scrollLeft;
    leftTableObj.scrollLeft = headerRowArea.scrollLeft;
}
function commonLeftTableSortEventBind() {
    let leftTableObj = document.getElementById('pnlLeftList');
    if (leftTableObj === null) {
        return;
    }
    let sortItemObj = document.createElement('input');
    sortItemObj.id = 'commonLeftListSortItem';
    sortItemObj.type = 'hidden';
    sortItemObj.value = '';
    leftTableObj.appendChild(sortItemObj);
    let headerArea = leftTableObj.querySelector('.leftTableHeaderWrapper');
    if (headerArea === null) {
        return;
    }
    let headerTextAreaList = headerArea.querySelectorAll('span[data-fieldname]');
    if (headerTextAreaList === null) {
        return;
    }

    for (let i = 0; i < headerTextAreaList.length; i++) {
        headerTextObj = headerTextAreaList[i];
        headerTextObj.addEventListener('click', (function (headerTextObj) {
            return function () {
                commonLeftTableSort(headerTextObj);
            };
        })(headerTextObj), false);
    }
}
/**
 * 左表のソート処理
 * @param {Element} headerTextObj ヘッダー
 * @return {undefined} なし
 * @description 詳細エリアのタブ変更時イベント
 */
function commonLeftTableSort(headerTextObj) {
    /* ********************************
     * クリックされたフィールドを元にソート情報を生成
    ******************************** */
    let sortValue = document.getElementById('commonLeftListSortItem');
    if (sortValue === null) {
        return;
    }
    let sortObj = [];
    /* Textエンコードした配列を復元 */
    if (sortValue.value !== '') {
        sortObj = JSON.parse(sortValue.value);
    }

    if (headerTextObj !== null) {
        addCssClassName = '';
        if (headerTextObj.classList.contains('sortAsc')) {
            addCssClassName = 'sortDesc';
            headerTextObj.classList.remove('sortAsc');
        } else if (headerTextObj.classList.contains('sortDesc')) {
            headerTextObj.classList.remove('sortDesc');
            addCssClassName = '';
        } else {
            addCssClassName = 'sortAsc';
        }
        if (addCssClassName !== '') {
            headerTextObj.classList.add(addCssClassName);
        }
        let targetField = headerTextObj.dataset.fieldname;
        let isnumericField = '';
        if (headerTextObj.dataset.isnumfield) {
            isnumericField = headerTextObj.dataset.isnumfield;
        }

        if (addCssClassName === '') {
            // 配列より削除
            let removedSordObj = sortObj.filter(function (itm) { return itm.FieldName !== targetField; });
            sortObj = removedSordObj;
        } else {
            // 配列を追加 or 変更
            let sortItem;
            for (let i = sortObj.length - 1; i >= 0; i--) {
                if (sortObj[i].FieldName === targetField) {
                    sortItem = sortObj[i];
                    sortObj[i].SortClass = addCssClassName;
                    break;
                }

            } // ソート条件更新

            if (sortItem === undefined) {

                sortItem = {
                    FieldName: targetField,
                    SortClass: addCssClassName,
                    IsNumericField: isnumericField
                };
                sortObj.push(sortItem); 
            } // ソート条件末尾に追加

        } //ソート情報配列最新化
        // ソート条件を隠しフィールドに保存
        encodedValue = '';
        if (sortObj.length > 0) {
            encodedValue = JSON.stringify(sortObj);
        }
        sortValue.value = encodedValue;
    } // HeaderObject isnot null
    /* ********************************
     * ソート情報を元に一覧表をソート
    ******************************** */
    let dataLists = document.querySelectorAll('#pnlLeftList .leftTableDataRow');
    /* データ行が無い場合ソートできないので終了 */
    if (dataLists === null) {
        return;
    }
    if (dataLists.length === 0) {
        return;
    }
    document.forms[0].style.display = 'none';
    //document.body.style.cursor = "wait";
    if (sortObj.length === 0) {
        //アイテムが存在しない場合は初期表示に戻す
        for (let i = 0; i < dataLists.length; i++) {
            let dataItm = dataLists[i];
            dataItm.style.order = dataItm.dataset.initorder;
        }
    } else {
        dataArr = [].slice.call(dataLists);
        dataArr.sort(sortLeftList(sortObj));
        
        for (let i = 0; i < dataArr.length; i++) {
            let dataItm = dataArr[i];
            dataItm.style.order = (i + 1).toString();
        }

    }
    let leftTableArea = document.getElementById('pnlLeftList');
    commonLeftTableMarkLastRow(leftTableArea);
    document.forms[0].style.display = 'block';
    //document.body.style.cursor = "auto";
    // 並び替え処理
    function sortLeftList(sortObj) {
        return function (a, b) {
            for (let i = 0; i < sortObj.length; i++) {
                let fieldName = sortObj[i].FieldName;
                let sortClass = sortObj[i].SortClass;
                let isNumericField = sortObj[i].IsNumericField;
                let aObj = a.querySelector('[data-fieldname="' + fieldName + '"] > span');
                let bObj = b.querySelector('[data-fieldname="' + fieldName + '"] > span');
                if (aObj === null || bObj === null) {
                    return 0;
                }
                let varA = (typeof aObj.textContent === 'string') ?
                    aObj.textContent.toUpperCase() : aObj.textContent;
                let varB = (typeof bObj.textContent === 'string') ?
                    bObj.textContent.toUpperCase() : bObj.textContent;
                if (isNumericField === '1') {
                    varA = varA.replace(/,/g, ''); // 念の為カンマ除去
                    if (isNaN(Number(varA)) === false) {
                        varA = Number(varA);
                    }
                    varB = varB.replace(/,/g, '');　// 念の為カンマ除去
                    if (isNaN(Number(varB)) === false) {
                        varB = Number(varB);
                    }
                }

                if (varA > varB) {
                    comparison = 1;
                    if (sortClass === 'sortDesc') {
                        return comparison * -1;
                    } else {
                        return comparison;
                    }
                } else if (varA < varB) {
                    comparison = -1;
                    if (sortClass === 'sortDesc') {
                        return comparison * -1;
                    } else {
                        return comparison;
                    }
                }
            }
            return 0;
        };
    } // end  sortLeftList
} // commonLeftTableSort
/**
 * Inputタグで虫眼鏡を表示するオブジェクトに対して虫眼鏡、カレンダーアイコン
 * のタグを追加する
 * @param {object} targetTextBoxList Inputタグオブジェクト
 * @return {undefined} なし
 * @description 詳細エリアのタブ変更時イベント
 */
function commonAppendInputBoxIcon(targetTextBoxList) {
    // オブジェクトが無ければ終了
    if (targetTextBoxList.length === 0) {
        return;
    }

    //対象のオブジェクトをループ
    for (let i = 0; i < targetTextBoxList.length; i++) {
        let inputObj = targetTextBoxList[i];
        let parentObj = inputObj.parentElement;

        // 対象オブジェクトが使用不可(または読み取り)の場合は
        // ダブルクリックをワークさせない
        let iconOnly = false;
  
        if (inputObj.disabled || (inputObj.readOnly && !inputObj.classList.contains('iconOnly'))) {
            parentObj.ondblclick = ""; /* 親要素のダブルクリックを排除 */
            inputObj.addEventListener('dblclick', function (e) {
                e.stopPropagation(); /* テキストボックスのダブルクリック伝達を抑止 */
            });
            inputObj.style.width = "100%";
            continue;
        }

        parentObj.style.position = 'relative';
        let additionalClass = 'boxIconArea';
        if (inputObj.classList.contains('calendarIcon') || inputObj.classList.contains('disabledcalendarIcon')) {
            additionalClass = 'calendarIconArea';
        }

        if (parentObj.tagName === 'TD') {
            if (inputObj.id.indexOf('REMARK') > -1) {
                additionalClass = 'richTextIconArea';
                inputObj.classList.add('richTextIcon');
                inputObj.classList.add('iconOnly');
                inputObj.readOnly = 'readonly';
            } else if (inputObj.id.indexOf('DATE') > -1) {
                additionalClass = 'calendarIconArea';
                inputObj.classList.add('calendarIcon');
            } else if (inputObj.id.indexOf('_RO') > -1) {
                inputObj.readOnly = 'readonly';
                inputObj.classList.add('boxIcon');
            } else {
                inputObj.classList.add('boxIcon');
            }
        }

        /* 削除アイコンの追加 */
        if (inputObj.classList.contains('iconOnly') && inputObj.classList.contains('showDeleteIcon')) {
            let delIconElm = document.createElement('div');
            let delInputObjId = inputObj.id;
            let delObjId = delInputObjId + 'commonDelIcon';
            let delIconElmImage = document.createElement('div');
            delIconElm.appendChild(delIconElmImage);
            delIconElm.id = delObjId;
            delIconElm.classList.add('delIconArea');
            insertAfter(delIconElm, inputObj);
            delIconElm = document.getElementById(delObjId);
            delIconElm.addEventListener('click', (function (delInputObjId) {
                return function () {
                    commonDeleteIconClick(delInputObjId);
                };
            })(delInputObjId), false);
        }

        /* アイコンの追加 */
        let iconElm = document.createElement('div');
        let inputObjId = inputObj.id;
        let orgWidth = inputObj.scrollWidth;
        let objId = inputObjId + 'commonIcon';
        let iconElmImage = document.createElement('div');
        iconElm.appendChild(iconElmImage);
        iconElm.id = objId;
        iconElm.classList.add(additionalClass);

        //parentObj.appendChild(iconElm);
        parentObj.insertBefore(iconElm, inputObj);

        // Iconオブジェクトを再取得しイベントバインド(再取得しないとバインドできない)
        iconElm = document.getElementById(objId);
        iconElm.addEventListener('click', (function (inputObjId) {
            return function () {
                // 画像クリック時にテキストボックスのダブルクリックイベント発火
                var evt = document.createEvent('MouseEvent');
                evt.initMouseEvent('dblclick', !0, !0, window, 0, 0, 0, 0, 0, !1, !1, !1, !1, 0, null);
                //evt.dataTransfer = data;
                elm = document.getElementById(inputObjId);
                elm.dispatchEvent(evt);
            };
        })(inputObjId), false);

        //フォーカス保持用
        parentObj.addEventListener('dblclick', (function (inputObjId, additionalClass) {
            return function () {
                var saveKey = document.title + "currentItemId";
                sessionStorage.setItem(saveKey, inputObjId);
                var divContensboxObj = document.getElementById("divContensbox");
                if (divContensboxObj !== null) {
                    saveScrollKey = document.title + "contentsXpos";
                    sessionStorage.setItem(saveScrollKey, divContensboxObj.scrollLeft);
                }
                if (additionalClass === 'richTextIconArea') {
                    commonRichTextInputOpen(inputObjId);
                    event.stopPropagation(); // 行のダブルクリックに伝達させない
                }
            };
        })(inputObjId, additionalClass), true);

        // 日付以外の場合、アイコン配置後のテキストボックスのサイズを補正(アイコンが無い状態に合わせる)
        inputObj = document.getElementById(inputObj.id);
        inputObj.style.width = "calc(100% + 1px)";

    }
}
/**
 * 指定したノードの後ろにエレメントを追加する
 * @param {Element} newItem 追加するノード
 * @param {Element} target 追加するノード
 * @return {undefined} なし
 */
function insertAfter(newItem, target) {
    target.parentNode.insertBefore(newItem, target.nextSibling);
}
/**
 * 削除アイコン押下時処理
 * @param {string} objId 削除ボタンを押下したテキストボックスID
 * @return {undefined} なし
 */
function commonDeleteIconClick(objId) {
    let txtObj = document.getElementById(objId);
    let comTargetNameObj = document.getElementById('MF_CommonDeleteItemName');
    let comTargetRowObj = document.getElementById('MF_CommonDeleteRow');
    let comTargetListIdObj = document.getElementById('MF_CommonDeleteListId');
    let btnClickObj = document.getElementById('WF_ButtonClick');
    let submitObj = document.getElementById('MF_SUBMIT');
    // 対象のオブジェクト群が存在しない場合そのまま終了
    if (txtObj === null) {
        return;
    }
    if (btnClickObj === null) {
        return;
    }
    if (submitObj === null) {
        return;
    }
    // サーバー処理実行中ならそのままスキップ
    if (submitObj.value === "TRUE") {
        return;
    }
    if (comTargetNameObj === null) {
        return;
    }
    if (comTargetRowObj === null) {
        return;
    }
    if (comTargetListIdObj === null) {
        return;
    }
    
    if (txtObj.hasAttribute("rownum")) {
        // テキストボックスにrownumの属性を持つ場合は行内に描画されたリストと判定
        let parentPanelObj = txtObj.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode;
        comTargetListIdObj.value = parentPanelObj.id;
        comTargetRowObj.value = txtObj.getAttribute("rownum");
        comTargetNameObj.value = txtObj.dataset.fieldName;
    } else {
        // 通常時はテキストボックスのID保持
        comTargetNameObj.value = txtObj.id;
    }
    // サーバー処理呼出しのサブミット
    submitObj.value = "TRUE";
    btnClickObj.value = "WF_ComDeleteIconClick";
    commonDispWait();
    document.forms[0].submit();
}
/**
 * ポップアップの背面操作禁止を解除
 * @param {string} modalWapperId ポップアップのID
 * @return {undefined} なし
 */
function commonCloseModal(modalWapperId) {
    var disableElemType = 'select,input:not([type="hidden"]),textarea,button';
    var popUpInnerOBJECTS = null;
    var popUpInnerOBJECTSId = new Array();
    if (modalWapperId !== '') {
        var keepElemType = '{0} select,{0} input:not([type="hidden"]),{0} textarea,{0} button';
        keepElemType = keepElemType.split('{0}').join("#" + modalWapperId);
        popUpInnerOBJECTS = document.forms[0].querySelectorAll(keepElemType);
        if (popUpInnerOBJECTS !== null) {
            for (let i = 0, len = popUpInnerOBJECTS.length; i < len; ++i) {
                popUpInnerOBJECTSId.push(popUpInnerOBJECTS[i].id);
            }
        }
    }
    document.forms[0].removeAttribute('data-showmodal');
    var inputItems = document.forms[0].querySelectorAll(disableElemType);
    for (let i = 0, len = inputItems.length; i < len; ++i) {
        let inputItem = inputItems[i];
        if (popUpInnerOBJECTSId.indexOf(inputItem.id) >= 0) {
            continue;
        }
        inputItem.tabIndex = null;
        inputItem.removeAttribute('tabIndex');
        let indexVal = inputItem.getAttribute('data-orgtabindex');
        if (indexVal !== null) {
            inputItem.tabIndex = indexVal;
            inputItem.removeAttribute('data-orgtabindex');
        }
    }
}
/**
 * ポップアップの背面操作を禁止
  * @param {string} modalWapperId ポップアップのID
 * @return {undefined} なし
 */
function commonDisableModalBg(modalWapperId) {
    var disableElemType = 'select,input:not([type="hidden"]),textarea,button,div.firstPage,div.lastPage';
    var popUpInnerOBJECTS = null;
    var popUpInnerOBJECTSId = new Array();
    if (modalWapperId !== '') {
        var keepElemType = '{0} select,{0} input:not([type="hidden"]),{0} textarea,{0} button';
        keepElemType = keepElemType.split('{0}').join("#" + modalWapperId);
        popUpInnerOBJECTS = document.forms[0].querySelectorAll(keepElemType);
        if (popUpInnerOBJECTS !== null) {
            for (let i = 0, len = popUpInnerOBJECTS.length; i < len; ++i) {
                popUpInnerOBJECTSId.push(popUpInnerOBJECTS[i].id);
            }
        }
    }
    var inputItems = document.forms[0].querySelectorAll(disableElemType);
    for (let i = 0, len = inputItems.length; i < len; ++i) {
        let inputItem = inputItems[i];
        if (popUpInnerOBJECTSId.indexOf(inputItem.id) >= 0) {
            continue;
        }
        let indexVal = inputItem.tabIndex;
        if (inputItem.hasAttribute('tabIndex')) {
            inputItem.dataset.orgtabindex = indexVal; //('data-orgtabindex', indexVal);
        }
        inputItem.tabIndex = '-1';
    }
    // keydownイベントの無効化
    if (modalWapperId !== '') {
        var modalWapperObj = document.getElementById(modalWapperId);
        if (modalWapperObj !== null) {
            modalWapperObj.tabIndex = '-1';
            modalWapperObj.style.outline = 'none';
            // 画面キーダウンイベントのバインド
            modalWapperObj.addEventListener('keydown', (function (event) {
                return function (event) {
                    // ↑キー押下時
                    if (window.event.keyCode === 38) {
                        window.event.stopPropagation(); //フォームのキーダウンイベントに↑キー伝達抑止
                    }
                    // ↓キー押下時
                    if (window.event.keyCode === 40) {
                        window.event.stopPropagation(); //フォームのキーダウンイベントに↓キー伝達抑止
                    }
                };
            })(event), false);
        }
    }
}
/**
 *  ウェイト画面表示
 * @return {undefined} なし
 * @description 
 */
function commonDispWait() {
    var hasElm = document.getElementById('comloading');
    if (hasElm !== null) {
        document.body.removeChild(hasElm);
    }
    // ウエイトスクリーン用半透明の大枠オブジェクト
    var lodingObj;
    lodingObj = document.createElement('div');
    lodingObj.id = 'comloading';
    lodingObj.classList.add('comloading');
    // ウエイトスクリーン用のフォーカス移動抑止のオブジェクト
    var forsubObj;
    forsubObj = document.createElement('input');
    forsubObj.id = 'comlodingtextbox';
    forsubObj.type = 'text';
    forsubObj.classList.add('comlodingtextbox');
    forsubObj.tabindex = '1';
    lodingObj.appendChild(forsubObj);
    // ウェイトスクリーン用のアニメーション枠
    var lodingMsgObj = document.createElement('div');
    lodingMsgObj.classList.add('comloadingmsg');
    // 子要素追加
    var lodingMsgChild1Obj = document.createElement('div');
    var lodingMsgChild2Obj = document.createElement('div');
    var lodingMsgChild3Obj = document.createElement('div');
    lodingMsgObj.appendChild(lodingMsgChild1Obj);
    lodingMsgObj.appendChild(lodingMsgChild2Obj);
    lodingMsgObj.appendChild(lodingMsgChild3Obj);
    //lodingMsgObj.innerText = 'Loading.....';
    lodingObj.appendChild(lodingMsgObj);
    document.body.appendChild(lodingObj);
    // テキストボックスにフォーカスを合わせておく
    forsubObj = document.getElementById('comlodingtextbox');
    forsubObj.select();
    forsubObj.onblur = (function (forsubObj) {
        return function () {
            forsubObj.select();
        };
    }(forsubObj));
    commonDisableModalBg('comloading');
}
/**
 *  ウェイト画面非表示
 * @return {undefined} なし
 * @description 
 */
function commonHideWait() {
    var hasElm = document.getElementById('comloading');
    if (hasElm !== null) {
        commonCloseModal('');
        document.body.removeChild(hasElm);
    }
}
// 〇数値のみ入力可能 一旦callerObj以外の引数無視
function commonAutoDecPoint(callerObj, decPint, totalLength) {
    // 呼出し元オブジェクト
    if (callerObj === null) {
        return;
    }
    let targetObj = callerObj;
    if (callerObj.tagName.toLowerCase() !== "input") {
        targetObj = callerObj.querySelector("input");
    }
    // デフォルト値
    let defVal = '00.000';
    let inpValue = targetObj.value;
    // 一旦小数点は除去
    inpValue = inpValue.replace(/[.]/g, '');
    // 除去した結果の長さが5以外ならデフォルト
    if (inpValue.length !== 5) {
        inpValue = defVal;
    } else {
        // 2文字目まで + "." + 3文字目以降を設定
        inpValue = inpValue.substring(0, 2) + "." + inpValue.substring(2);
    }
    targetObj.value = inpValue;
}
// 〇数値のみ入力可能
function CheckNum() {
    if (event.keyCode < 48 || event.keyCode > 57) {
        window.event.returnValue = false; // IEだと効かないので↓追加
        event.preventDefault(); // IEはこれで効く
    }
}
// 〇数値のみ入力可能(小数点あり)
function CheckDeci() {
    if (event.keyCode !== 46) {
        if (event.keyCode < 48 || event.keyCode > 57) {
            window.event.returnValue = false; // IEだと効かないので↓追加
            event.preventDefault(); // IEはこれで効く
        }
    }
}
// 〇数値のみ入力可能(ハイフン(-)あり)
function CheckTel() {
    if (event.keyCode !== 45 && event.keyCode !== 109) {
        if (event.keyCode < 48 || event.keyCode > 57) {
            window.event.returnValue = false; // IEだと効かないので↓追加
            event.preventDefault(); // IEはこれで効く
        }
    }
}
// 〇数値と英字のみ入力可能
function CheckNumAZ() {
    if (event.keyCode < 48 || event.keyCode > 90) {
        window.event.returnValue = false; // IEだと効かないので↓追加
        event.preventDefault(); // IEはこれで効く
    }
}
// 〇カレンダー値のみ入力可能
function CheckCalendar() {
    if (event.keyCode < 47 || event.keyCode > 57) {
        window.event.returnValue = false; // IEだと効かないので↓追加
        event.preventDefault(); // IEはこれで効く
    }
}
// 〇全角⇔半角変換
function ConvartWideCharToNormal(obj) {
    if (obj === null) {
        return;
    }
    if (obj.value === '') {
        return;
    }
    let repVal = '';
    repVal = obj.value.replace(/[Ａ-Ｚａ-ｚ０-９]/g, function (s) {
        return String.fromCharCode(s.charCodeAt(0) - 0xFEE0);
    });
    repVal = repVal.replace(/[．]/g, '.');
    repVal = repVal.replace(/[ー]/g, '-');
    repVal = repVal.replace(/[－]/g, '-');
    repVal = repVal.replace(/，/g, '');
    repVal = repVal.replace(/,/g, '');
    //repVal = repVal.replace(/[^0-9]/g, '');
    repVal = repVal.replace(/[^-^0-9^\.]/g, "");
    //repVal = repVal.match(/-?\d+\.?\d*/);
    obj.value = repVal;
}
/**
 *  リストテーブルを除くテーブルにつきEnterキーで次のテキストボックスにタブを移すイベントバインド
 * @return {undefined} なし
 * @description 
 */
function commonBindNormalEnterToNextTabStep() {
    let inputObjList = document.querySelectorAll('input[type=password],input[type=text]:not([disabled]):not([tabindex="-1"]):not([rownum])');
    if (inputObjList === null) {
        return;
    }
    /* 画面表示していないオブジェクト判定 */
    let visibleInputObj = [];
    for (let i = 0, len = inputObjList.length; i < len; ++i) {
        //if (inputObjList[i].clientWidth === 0) {
        //    continue;
        //}
        //let dispStyle = window.getComputedStyle(inputObjList[i]);
        //if (dispStyle.display === 'none') {
        //    continue;
        //}
        visibleInputObj.push(inputObjList[i]);

    }

    for (let i = 0, len = visibleInputObj.length; i < len; ++i) {
        let textBox = visibleInputObj[i];
        let nextTextObj = visibleInputObj[0];
        if (visibleInputObj.length !== i + 1) {
            nextTextObj = visibleInputObj[i + 1];
        }
        textBox.dataset.nexttextid = nextTextObj.id;
        textBox.addEventListener('keypress', (function (textBox, nextTextObj) {
            return function () {
                if (event.key === 'Enter') {
                    if (commonKeyEnterProgress === false) {
                        commonKeyEnterProgress = true; //Enter連打抑止
                        let foundDisplayText = false;
                        let wkNextTextObj = nextTextObj;
                        let loopCnt = 0;
                        while (!foundDisplayText) {
                            loopCnt = loopCnt + 1;
                            // 全部テキストボックスが見えない状態での無限ループ抑止
                            if (loopCnt >= 2000) {
                                break;
                            }
                            // hidden等で見えない状態・親などのスタイルで実際見えない状態
                            // の場合はスキップ
                            if (wkNextTextObj.clientWidth === 0) {
                                wkNextTextObj = document.getElementById(wkNextTextObj.dataset.nexttextid);
                                continue;
                            }
                            let dispStyle = window.getComputedStyle(wkNextTextObj);
                            if (dispStyle.display === 'none') {
                                wkNextTextObj = document.getElementById(wkNextTextObj.dataset.nexttextid);
                                continue;
                            }
                            // ここまで来た場合は存在するのでループ終了
                            foundDisplayText = true;
                        }

                        wkNextTextObj.focus();
                        return setTimeout(function () {
                            commonKeyEnterProgress = false;　///Enter連打抑止
                        }, 10); // 5ミリ秒だと連打でフォーカスパニックになったので10ミリ秒に
                    }
                }
            };
        })(textBox, nextTextObj), true);
    }
}
/**
 *  リストテーブルのEnterキーで下のテキストにタブを移すイベントバインド
 * @return {undefined} なし
 * @description 
 */
function commonBindEnterToVerticalTabStep() {
    let generatedTables = document.querySelectorAll("div[data-generated='1']");
    if (generatedTables === null) {
        return;
    }
    if (generatedTables.length === 0) {
        return;
    }
    let focusObjKey = document.forms[0].id + "ListFocusObjId";
    if (sessionStorage.getItem(focusObjKey) !== null) {
        if (IsPostBack === undefined) {
            sessionStorage.removeItem(focusObjKey);
        }
        if (IsPostBack === '1') {
            focusObjId = sessionStorage.getItem(focusObjKey);
            setTimeout(function () {
                document.getElementById(focusObjId).focus();
                sessionStorage.removeItem(focusObjKey);
            }, 10);
        } else {
            sessionStorage.removeItem(focusObjKey);
        }

    }

    for (let i = 0, len = generatedTables.length; i < len; ++i) {
        let generatedTable = generatedTables[i];
        let panelId = generatedTable.id;
        //生成したテーブルオブジェクトのテキストボックス確認
        let textBoxes = generatedTable.querySelectorAll('input[type=text]');
        //テキストボックスが無ければ次の描画されたリストテーブルへ
        if (textBoxes === null) {
            continue;
        }
        // テキストボックスのループ
        for (let j = 0; j < textBoxes.length; j++) {
            let textBox = textBoxes[j];
            let lineCnt = textBox.attributes.getNamedItem("rownum").value;
            let fieldName = textBox.id.substring(("txt" + panelId).length);
            fieldName = fieldName.substring(0, fieldName.length - lineCnt.length);
            let nextTextFieldName = fieldName;
            if (textBoxes.length === j + 1) {
                // 最後のテキストボックスは先頭のフィールド
                nextTextFieldName = textBoxes[0].id.substring(("txt" + panelId).length);
                lineCnt = textBoxes[0].attributes.getNamedItem("rownum").value;
                nextTextFieldName = nextTextFieldName.substring(0, nextTextFieldName.length - lineCnt.length);
            } else if (textBoxes.length > j + 1) {
                nextTextFieldName = textBoxes[j + 1].id.substring(("txt" + panelId).length);
                lineCnt = textBoxes[j + 1].attributes.getNamedItem("rownum").value;
                nextTextFieldName = nextTextFieldName.substring(0, nextTextFieldName.length - lineCnt.length);
            }

            textBox.dataset.fieldName = fieldName;
            textBox.dataset.nextTextFieldName = nextTextFieldName;
            textBox.addEventListener('keypress', (function (textBox, panelId) {
                return function () {
                    if (event.key === 'Enter') {
                        if (commonKeyEnterProgress === false) {
                            commonKeyEnterProgress = true; //Enter連打抑止
                            commonListEnterToVerticalTabStep(textBox, panelId);
                            return setTimeout(function () {
                                commonKeyEnterProgress = false;　///Enter連打抑止
                            }, 10); // 5ミリ秒だと連打でフォーカスパニックになったので10ミリ秒に
                        }
                    }
                 };
            })(textBox, panelId), true);
        }
    }
}
/**
 *  リストテーブルのEnterキーで下のテキストにタブを移すイベント
 * @param {Node} textBox テキストボックス
 * @param {string} panelId テキストボックス
 * @return {undefined} なし
 * @description 
 */
function commonListEnterToVerticalTabStep(textBox, panelId) {
    let curLineCnt = Number(textBox.attributes.getNamedItem("rownum").value);
    let fieldName = textBox.dataset.fieldName;
    let nextTextFieldName = textBox.dataset.nextTextFieldName;
    let found = false;
    let focusNode;
    let maxLineCnt = 999;
    let targetObjPrefix = "txt" + panelId + fieldName;
    while (found === false) {
        curLineCnt = curLineCnt + 1;
        let targetObj = targetObjPrefix + curLineCnt;
        focusNode = document.getElementById(targetObj);
        if (focusNode !== null) {
            found = true;
        } else {
            curLineCnt = 0;

            targetObjPrefix = "txt" + panelId + nextTextFieldName;
        }

        // 無限ループ抑止
        if (maxLineCnt === curLineCnt) {
            found = true;
        }
    }

    var parentNodeObj = textBox.parentNode;
    if (parentNodeObj.hasAttribute('onchange')) {
        var focusObjKey = document.forms[0].id + "ListFocusObjId";
        sessionStorage.setItem(focusObjKey, focusNode.id);
    }
    //var retValue = sessionStorage.getItem(forcusObjKey);
    //if (retValue === null) {
    //    retValue = '';
    //}
    focusNode.focus();
    return;
}
/**
 *  リッチテキスト入力画面オープン
 * @param {string} inputObjId 入力テキストID
 * @return {undefined} なし
 * @description 
 */
function commonRichTextInputOpen(inputObjId) {
    //let targetOBJECTS = 
    let textObj = document.getElementById(inputObjId);
    let richTextWrapperId = 'pnlCommonRichTextWrapper';
    let richTextWrapper = document.getElementById(richTextWrapperId);
    // 既に同IDのオブジェクトが存在していた場合はクリア
    if (document.getElementById(richTextWrapperId) !== null) {
        richTextWrapper.parentNode.removeChild(richTextWrapper);
    }
    // 全体を覆うDivを生成
    richTextWrapper = document.createElement('div');
    richTextWrapper.id = richTextWrapperId;
    // 中央に配置するコンテンツを生成
    let richTextContents = document.createElement('div');
    richTextContents.id = 'pnlCommonRichTextContents';
    // タイトルバー設定
    let richTextTitle = document.createElement('div');
    richTextTitle.id = 'pnlCommonRichTextTitle';
    // アイコン
    let richTextTitleIcon = document.createElement('div');
    richTextTitleIcon.id = 'pnlCommonRichTextTitleIcon';
    // 決定、キャンセルボタン設定
    let cancelButton = document.createElement('input');
    cancelButton.id = 'btnCommonRichTextCancel';
    cancelButton.type = 'button';
    cancelButton.value = "キャンセル";
    cancelButton.addEventListener('click', function () {
        let richTextWrapper = document.getElementById(richTextWrapperId);
        // 既に同IDのオブジェクトが存在していた場合はクリア
        if (document.getElementById(richTextWrapperId) !== null) {
            richTextWrapper.parentNode.removeChild(richTextWrapper);
        }
        document.forms[0].disabled = false;
    });

    let okButton = document.createElement('input');
    okButton.id = 'btnCommonRichTextOk';
    okButton.type = 'button';
    okButton.value = "OK";
    // 画面キーダウンイベントのバインド
    okButton.addEventListener('click', (function (textObj) {
        return function () {
            /* ありえないが呼出し元のテキストボックス、書き込み先のテキストボックスが無ければ終了 */
            if (textObj === null) {
                return;
            }

            let txtRichText = document.getElementById('txtCommonRichText');
            /* txtRichText */
            if (txtRichText === null) {
                return;
            }
            /* リッチテキスト入力値をテキストボックスに反映 */
            let textVal = txtRichText.value.replace(/\\r\\n|\\r|\\n/g, "\r\n");
            textObj.value = textVal;
            textObj.dataset.withenterval = textVal;
            /* テキストボックスの変更イベントを発火 */
            var evt = document.createEvent("HTMLEvents");
            evt.initEvent("change", false, true);
            textObj.parentNode.dispatchEvent(evt);
            
            /* 後始末 */
            let richTextWrapper = document.getElementById(richTextWrapperId);
            // 既に同IDのオブジェクトが存在していた場合はクリア
            if (document.getElementById(richTextWrapperId) !== null) {
                richTextWrapper.parentNode.removeChild(richTextWrapper);
            }
            document.forms[0].disabled = false;
        };
    })(textObj), false);
    // リッチテキスト本体エリア
    let richRichTextArea = document.createElement('div');
    richRichTextArea.id = 'pnlCommonRichTextArea';
    // リッチテキストオブジェクト
    let txtRichText = document.createElement('textarea');
    txtRichText.id = 'txtCommonRichText';
    txtRichText.value = textObj.value;
    if (textObj.hasAttribute("data-withenterval")) {
        txtRichText.value  = textObj.dataset.withenterval;
    }
    // 生成したオブジェクトを組み立て
    richTextTitle.appendChild(richTextTitleIcon);
    richTextTitle.appendChild(okButton);
    richTextTitle.appendChild(cancelButton);

    richRichTextArea.appendChild(txtRichText);

    richTextContents.appendChild(richTextTitle);
    richTextContents.appendChild(richRichTextArea);

    richTextWrapper.appendChild(richTextContents);

    document.forms[0].disabled = true;
    document.body.appendChild(richTextWrapper);
    document.getElementById('txtCommonRichText').focus();
    return false;
}
/**
 *  新しいタブで開く
 * @param {string} urlText URL
 * @return {undefined} なし
 * @description 
 */
function commonOpenNewTab(urlText) {

    let currentForm = document.getElementsByTagName("form")[0];
    currentForm.target = "_blank";
    document.forms[0].submit();
    currentForm.target = '';
}
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
    let needsPostBack = targetTextBox.dataset.monthpickerneedspostback;
    if (typeof needsPostBack === "undefined") {
        return;
    }
    if (needsPostBack === '1') {
        ButtonClick(targetTextBox.id);
    }
}
/* カスタムポップアップ表示 */
function commonShowCustomPopup() {
    let customPopUpAreaObj = document.getElementById('divContentsPopUpWrapper');
    let dispCustomPopUpObj = document.getElementById('MF_SHOWCUSTOMOPOUP');
    let WF_ButtonOkCommonPopUpObj = document.getElementById('WF_ButtonOkCommonPopUp');
    customPopUpAreaObj.style.display = 'block';
    dispCustomPopUpObj.value = '1';
    WF_ButtonOkCommonPopUpObj.focus();
}
/* カスタムポップアップ非表示 */
function commonHideCustomPopup() {
    let customPopUpAreaObj = document.getElementById('divContentsPopUpWrapper');
    let dispCustomPopUpObj = document.getElementById('MF_SHOWCUSTOMOPOUP');
    customPopUpAreaObj.style.display = 'none';
    dispCustomPopUpObj.value = '0';
}
/* タイルボックスコントロール */
function commonBindSingleCheckOnly() {
    let tailOBJECTS = document.querySelectorAll('ul[id$="_chklGrc0001SelectionBox"]');
    if (tailOBJECTS === null) {
        return;
    }
    for (let i = 0; i < tailOBJECTS.length; i++) {
        let tailObject = tailOBJECTS[i];
        let tailObjectControlId = tailObject.id.replace("_chklGrc0001SelectionBox",""); //レンダリング時ではないVB.NET上のID 
        let multiSetting = tailObject.parentNode.querySelector('input[data-id=SelectionMode]');
        if (multiSetting === null) {
            continue;
        }

        let needsPostBack = tailObject.parentNode.querySelector('input[data-id=NeedsAfterPostBack]');
        if (needsPostBack === null) {
            continue;
        }

        if (multiSetting.value === '1' && needsPostBack.value === 'False') {
            continue;
        }

        let insideCheckBoxes = tailObject.querySelectorAll('input[type=checkbox]');
        if (insideCheckBoxes === null) {
            return;
        }
        for (let j = 0; j < insideCheckBoxes.length; j++) {
            let insideCheckBox = insideCheckBoxes[j];
            let parentId = tailObject.id;
            let targetId = insideCheckBox.id;
            if (multiSetting.value === '0') {
                insideCheckBox.addEventListener('click', (function (parentId, targetId) {
                    return function () {
                        commonTailSingleCheck(parentId, targetId);
                    };
                })(parentId, targetId), false);
            }

            if (needsPostBack.value !== 'False') {
                insideCheckBox.addEventListener('click', (function (tailObjectControlId) {
                    return function () {
                        ButtonClick(tailObjectControlId);
                    };
                })(tailObjectControlId), false);
                
            }
        }
    }
}
function commonTailSingleCheck(parentId, targetId) {
    let parentObj = document.getElementById(parentId);
    let targetObj = document.getElementById(targetId);
    let currentVal = targetObj.checked;
    let insideCheckBoxes = parentObj.querySelectorAll('input[type=checkbox]');
    for (let i = 0; i < insideCheckBoxes.length; i++) {
        let insideCheckBox = insideCheckBoxes[i];
        insideCheckBox.checked = false;
    }
    targetObj.checked = currentVal;
}

/* ********************************************** 
 * 複数選択ポップアップ関連処理
 * ********************************************** */
/**
 * 共通複数選択ポップアップを閉じる
 * @param {string} hiddenStatusObjId ポップアップの表示非表示を設定するHiddenオブジェクトID
 * @return {undefined} なし
 */
function commonHideGrc0002InputCLOSEClick(hiddenStatusObjId) {
    let hdnObj = document.getElementById(hiddenStatusObjId);
    hdnObj.value = '0';
    commonGrc0002InputSet();
}
/**
 * 共通複数選択ポップアップを閉じる
 * @return {undefined} なし
 * @description 使い方ではありえないが念のため１画面に同コントロールを
 * 複数置きを考慮
 */
function commonGrc0002InputSet() {
    let grcwrapperObjectgs = document.querySelectorAll('div[data-grc0002control="1"]');
    if (grcwrapperObjectgs === null) {
        return false;
    }
    for (let i = 0; i < grcwrapperObjectgs.length; i++) {
        let grcwrapperObject = grcwrapperObjectgs[i];
        let hdnObj = grcwrapperObject.querySelector('span[data-shohidehiddenobj="1"] > input[type="hidden"]');
        grcwrapperObject.classList.remove('hide');
        grcwrapperObject.classList.remove('show');
        grcwrapperObject.classList.remove('edit');
        grcwrapperObject.classList.remove('new');
        if (hdnObj.value === '0') {
            grcwrapperObject.classList.add('hide');
        } else if (hdnObj.value === '1') {
            grcwrapperObject.classList.add('show');
            grcwrapperObject.classList.add('new');
        } else if (hdnObj.value === '2') {
            grcwrapperObject.classList.add('show');
            grcwrapperObject.classList.add('edit');
        }
    }
}
/**
 * 共通複数選択ポップアップ行選択時
 * @return {undefined} なし
 * @param {string} keyVal 行データを一意に特定するキー
 * @param {string} wrapperObjId 表示非表示定義のラッパーOBJID
 * @param {string} cotrolId 共通コントロールGrc0002を画面に置いた際のID
 * @param {HTMLElement} callerrownode 呼出し元行
 * @description 使い方ではありえないが念のため１画面に同コントロールを
 * 複数置きを考慮
 */
function commonGrc0002PopUpChangeSelect(keyVal,wrapperObjId,cotrolId,callerrownode) {
    let baseNode = document.getElementById(wrapperObjId);

    if (baseNode.classList.contains('edit') !== true) {
        if (callerrownode !== null) {
            let chkObj = callerrownode.querySelector('.chkGrc0002InsideList input[type="checkbox"]');
            if (chkObj !== null) {
                callerrownode.classList.remove('checkedrow');
                if (chkObj.checked === true) {
                    callerrownode.classList.add('checkedrow');
                }
            }
        }

        
        return false;
    }

    let hdnObj = baseNode.querySelector('span[data-uniquekeyhiddenobj="1"] > input[type="hidden"]');
    if (hdnObj !== null) {
        hdnObj.value = keyVal;
    }
    ButtonClick(cotrolId + 'RowSelected');
}

// 全角→半角(英数字)
function replaceFullToHalf(str){
  return str.replace(/[！-～]/g, function(s){
    return String.fromCharCode(s.charCodeAt(0) - 0xFEE0);
  });
}

/**
 * コード、名称手入力検索
 * @return {undefined} なし
 * @param {string} val      入力項目ID
 * @param {string} list     データ保持リストID
 * @param {string} code     取得値設定コード
 * @param {string} name     取得値設定名称
 * @param {string} srcid    検索子画面呼び出し用
 * @param {string} tabNo    検索子画面呼び出し用
 * @description 入力された内容から、コード、名称を取得
 */
function CodeName_OnChange(val, list, code, name, srcid, tabNo) {
    var strToriVal = document.getElementById(val).value;
    var strToriList = document.getElementById(list);

    // 未入力の場合、処理を行わない
    if (strToriVal == "") {
        document.getElementById(name).value = "";
        return;
    }

    var count = 0;
    var flg = 0;
    var rtnCode = "";
    var rtnName = "";

    // コード、名称を検索
    for (var i = 0; i < strToriList.length; i++) {

        // コード検索
        if (strToriList.options[i].value.indexOf(strToriVal) > -1) {
            //件数カウント、フラグを立てる
            count += 1;
            flg = 1;
        }

        // 名称検索
        if (strToriList.options[i].textContent.indexOf(strToriVal) > -1) {
            //件数カウント、フラグを立てる
            count += 1;
            flg = 1;
        }

        // 部分一致しているデータを設定（最初に見つかった１件のみ取得）
        if (flg == 1 && count == 1) {
            rtnCode = strToriList.options[i].value;
            rtnName = strToriList.options[i].textContent;
        }

        // フラグクリア
        flg = 0;
    }

    // 1件のみ取得の場合、設定
    if (count == 1) {
        document.getElementById(code).value = rtnCode;
        document.getElementById(name).value = rtnName;
    } else {
        // 0件もしくは複数件取得の場合、検索子画面表示
        Field_DBclick(srcid, tabNo);
    }
}

/**
 * カンマ編集処理
 * @return {undefined} なし
 * @param {string} val      入力項目ID
 * @description 入力された内容から、カンマ編集処理を行う。
 */
function commaEdit_onblur(val) {
    var strToriVal = document.getElementById(val).value;

    // 未入力の場合、処理を行わない
    if (strToriVal == "") {
        return;
    } else {
        // カンマ編集
        strToriVal = strToriVal.replace(/,/g, '');
        document.getElementById(val).value = Number(strToriVal).toLocaleString();
    }
}

/**
 * ログアウトボタン表示非表示制御処理
 * @return {undefined} なし
 * @description ログアウトボタンの表示非表示を制御する
 */
function commonLogOutDisp() {
    if (document.getElementById('MF_MAPID').value === "M00001") {
        document.getElementById('WF_ButtonLogOut').style.display = 'inline';
    } else {
        document.getElementById('WF_ButtonLogOut').style.display = 'none';
    }
}
