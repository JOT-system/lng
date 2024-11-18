// 画面読み込み時処理
window.onload = function () {

    // モード
    const Mode = document.getElementById('WF_MODE').value;

    // スプレッドのフォーカス移動設定処理
    setKeyMap()

    //発受託人検索OnLoad用処理
    TrusteeSrcOnload()

    // ボタン制御
    switch (document.getElementById('WF_MODE_KBN').value) {
        // 初期表示
        case "0":
            // 表示
            document.getElementById('btnArea').style.display = 'inline'

            // 20230131_コメントアウト
            //if (document.getElementById('WF_APPROVAL_FLG').value === "1"){
            //    document.getElementById('WF_ButtonCORRECT').style.display = 'none';       // 訂正(申請)
            //    document.getElementById('WF_ButtonWITHDRAW').style.display = 'none';      // 申請取下げ
            //    document.getElementById('WF_ButtonCONFIRMED').style.display = 'inline';   // 確認済みにする
            //    document.getElementById('WF_ButtonNEW_INSERT').style.display = 'none';    // 新規登録
            //    document.getElementById('WF_Button_DELETE').style.display = 'none';       // 削除
            //} else {
            //    document.getElementById('WF_ButtonCORRECT').style.display = 'inline';     // 訂正(申請)
            //    document.getElementById('WF_ButtonWITHDRAW').style.display = 'inline';    // 申請取下げ
            //    document.getElementById('WF_ButtonCONFIRMED').style.display = 'none';     // 確認済みにする
            //    document.getElementById('WF_ButtonNEW_INSERT').style.display = 'inline';  // 新規登録
            //    document.getElementById('WF_Button_DELETE').style.display = 'inline';     // 削除
            //}

            //// 非表示
            //document.getElementById('WF_Button_INSERT').style.display = 'none';           // 登録する
            //document.getElementById('WF_ButtonCORRECT_ALL').style.display = 'none';       // 申請
            //document.getElementById('WF_ButtonCONFIRMED_ALL').style.display = 'none';     // 承認
            //document.getElementById('WF_ButtonREMAND_ALL').style.display = 'none';        // 差戻し
            //document.getElementById('WF_ButtonSPREAD_LINE_ADD').style.display = 'none';   // 行追加
            //document.getElementById('WF_ButtonSPREAD_SEL_ALL').style.display = 'inline';  // 全選択
            //document.getElementById('WF_ButtonSPREAD_SEL_DEL').style.display = 'inline';  // 全選択解除
            //document.getElementById('WF_ButtonEND').style.display = 'none';               // 前画面へ戻る
            //break;

            // 20230131_追加（確認画面SKIP）
            if (document.getElementById('WF_APPROVAL_FLG').value === "1") {

                // 非表示
                document.getElementById('btnArea').style.display = 'none'                   // 訂正(申請)～新規登録

                document.getElementById('WF_Button_INSERT').style.display = 'none';         // 登録する
                document.getElementById('WF_ButtonCORRECT_ALL').style.display = 'none';     // 申請
                document.getElementById('WF_ButtonSPREAD_LINE_ADD').style.display = 'none'; // 行追加

                // 確認画面SKIPの為、前画面へ戻るボタンは非表示とする。
                document.getElementById('WF_ButtonEND').style.display = 'none';             // 前画面へ戻る

            } else {

                //document.getElementById('WF_ButtonCORRECT').style.display = 'inline';     // 訂正(申請)
                //document.getElementById('WF_ButtonWITHDRAW').style.display = 'inline';    // 申請取下げ
                //document.getElementById('WF_ButtonCONFIRMED').style.display = 'none';     // 確認済みにする
                //document.getElementById('WF_ButtonNEW_INSERT').style.display = 'inline';  // 新規登録
                //document.getElementById('WF_Button_DELETE').style.display = 'inline';     // 削除

                if (document.getElementById('WF_UPD_ROLE').value === "disapproval") {
                    document.getElementById('WF_ButtonCORRECT').style.display = 'none';     // 訂正(申請)
                    document.getElementById('WF_ButtonWITHDRAW').style.display = 'none';    // 申請取下げ
                    document.getElementById('WF_ButtonCONFIRMED').style.display = 'none';     // 確認済みにする
                    document.getElementById('WF_ButtonNEW_INSERT').style.display = 'none';  // 新規登録
                    document.getElementById('WF_Button_DELETE').style.display = 'none';     // 削除
                } else {
                    document.getElementById('WF_ButtonCORRECT').style.display = 'inline';     // 訂正(申請)
                    document.getElementById('WF_ButtonWITHDRAW').style.display = 'inline';    // 申請取下げ
                    document.getElementById('WF_ButtonCONFIRMED').style.display = 'none';     // 確認済みにする
                    document.getElementById('WF_ButtonNEW_INSERT').style.display = 'inline';  // 新規登録
                    document.getElementById('WF_Button_DELETE').style.display = 'inline';     // 削除
                }

                // 非表示
                document.getElementById('WF_Button_INSERT').style.display = 'none';           // 登録する
                document.getElementById('WF_ButtonCORRECT_ALL').style.display = 'none';       // 申請
                document.getElementById('WF_ButtonCONFIRMED_ALL').style.display = 'none';     // 承認
                document.getElementById('WF_ButtonREMAND_ALL').style.display = 'none';        // 差戻し
                document.getElementById('WF_ButtonSPREAD_LINE_ADD').style.display = 'none';   // 行追加
                document.getElementById('WF_ButtonSPREAD_SEL_ALL').style.display = 'inline';  // 全選択
                document.getElementById('WF_ButtonSPREAD_SEL_DEL').style.display = 'inline';  // 全選択解除
                document.getElementById('WF_ButtonEND').style.display = 'none';               // 前画面へ戻る
            }

            break;


        // 修正
        case "1":
            // 非表示
            document.getElementById('btnArea').style.display = 'none'                   // 訂正(申請)～新規登録
            document.getElementById('WF_Button_INSERT').style.display = 'none';         // 登録する
            document.getElementById('WF_ButtonCONFIRMED_ALL').style.display = 'none';   // 承認
            document.getElementById('WF_ButtonREMAND_ALL').style.display = 'none';      // 差戻し
            document.getElementById('WF_ButtonSPREAD_LINE_ADD').style.display = 'none'; // 行追加
            break;

        // 承認
        case "2":
            // 非表示
            document.getElementById('btnArea').style.display = 'none'                   // 訂正(申請)～新規登録
            document.getElementById('WF_Button_INSERT').style.display = 'none';         // 登録する
            document.getElementById('WF_ButtonCORRECT_ALL').style.display = 'none';     // 申請
            document.getElementById('WF_ButtonSPREAD_LINE_ADD').style.display = 'none'; // 行追加
            break;

        // 新規
        case "3":
            // 表示
            document.getElementById('WF_Button_INSERT').style.display = 'inline';            // 登録する
            document.getElementById('WF_ButtonSPREAD_LINE_ADD').style.display = 'inline';    // 行追加
            document.getElementById('WF_ButtonSPREAD_SEL_ALL').style.display = 'inline';     // 全選択
            document.getElementById('WF_ButtonSPREAD_SEL_DEL').style.display = 'inline';     // 全選択解除

            // 非表示
            document.getElementById('btnArea').style.display = 'none'                     // 訂正(申請)～新規登録
            document.getElementById('WF_ButtonCORRECT_ALL').style.display = 'none';       // 申請
            document.getElementById('WF_ButtonCONFIRMED_ALL').style.display = 'none';     // 承認
            document.getElementById('WF_ButtonREMAND_ALL').style.display = 'none';        // 差戻し
            break;
    }
};

// ○左Box用処理（左Box表示/非表示切り替え）
function Spred_Field_DBclick(fieldNM, tabNo) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_LeftMViewChange').value = tabNo;
        document.getElementById('WF_LeftboxOpen').value = "Open";

        document.getElementById("WF_ButtonClick").value = "WF_SPREAD_BtnClick";
        document.body.style.cursor = "wait";
        document.getElementById("WF_saveLeft").value = 0;
        document.forms[0].submit();
    }
}

// 申請ボタン押下
function Spred_ButtonSel_click(btnIDClick) {
    var sheet = document.getElementById("spdDetailList");
    var row = sheet.GetActiveRow();
    var col = sheet.GetActiveCol();
    var elem = document.activeElement;
    var result1 = elem.id.indexOf('_');
    var result2 = elem.id.indexOf(',');

    if (result1 != -1 && result2 != -1) {
        if (document.getElementById("MF_SUBMIT").value === "FALSE") {
            document.getElementById("MF_SUBMIT").value = "TRUE";
            document.getElementById("WF_ButtonClick").value = btnIDClick;
            document.body.style.cursor = "wait";
            document.forms[0].submit();
        }
    }
}

// スプレッドのフォーカス移動設定処理
function setKeyMap() {
    var s = document.getElementById(spid);
    var kcode;

    // kcode = 9; tab
    // kcode = 13; enter

    // 縦移動
    kcode = 9;
    s.AddKeyMap(kcode, false, false, false, "element.MoveToNextRow()");
    s.AddKeyMap(kcode, false, true, false, "element.MoveToPrevRow()");
    kcode = 13;
    s.AddKeyMap(kcode, false, false, false, "element.MoveToNextRow()");
    s.AddKeyMap(kcode, false, true, false, "element.MoveToPrevRow()");

    // 横移動
    //kcode = 9;
    //s.AddKeyMap(kcode, false, false, false, "element.MoveToNextCell()");
    //s.AddKeyMap(kcode, false, true, false, "element.MoveToPrevCell()");
    //kcode = 13;
    //s.AddKeyMap(kcode, false, false, false, "element.MoveToNextCell()");
    //s.AddKeyMap(kcode, false, true, false, "element.MoveToPrevCell()");

}

// ドロップダウンリスト変更
function SelectDropDownList_OnChange(ddlIDClick) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = ddlIDClick;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// 確認ボタン押下
function Spred_ConfBtn_click(btnIDClick) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = btnIDClick;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// セル値を変更したときに発生
function Spred_dataChanged(event) {
    var row = event.spread.GetActiveRow();
    var col = event.spread.GetActiveCol();
    // 新規モードの場合
    if (document.getElementById('WF_MODE').value === "1") {
        // 発荷主が変更された場合
        if (row === 8) {
            // ポストバックデータを更新
            event.spread.UpdatePostbackData();
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = "Spred_shipperChanged";
                document.body.style.cursor = "wait";
                document.forms[0].submit();
            }
        }
        // 発駅、着駅が変更された場合
        if (row === 11 || row === 20) {
            // ポストバックデータを更新
            event.spread.UpdatePostbackData();
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = "Spred_stationChanged";
                document.body.style.cursor = "wait";
                document.forms[0].submit();
            }
        }
        // 発受託人、着受託人が変更された場合
        if (row === 12 || row === 13 || row === 21 || row === 22) {
            // ポストバックデータを更新
            event.spread.UpdatePostbackData();
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = "Spred_trusteeChanged";
                document.body.style.cursor = "wait";
                document.forms[0].submit();
            }
        }
        // 品目が変更された場合
        if (row === 14) {
            // ポストバックデータを更新
            event.spread.UpdatePostbackData();
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = "Spred_itemChanged";
                document.body.style.cursor = "wait";
                document.forms[0].submit();
            }
        }
    }
    // 更新モードの場合
    if (document.getElementById('WF_MODE').value === "2") {
        // 使用料金額が変更された場合、自動計算処理、割戻し運賃計算処理を行う
        // 30:固定使用料, 31:使用料金, 32:通運負担運賃, 44:私有割引額 
        if (row === 30 || row === 31 || row === 32 || row === 44) {
            // ポストバックデータを更新
            event.spread.UpdatePostbackData();
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = "Spred_dataChanged";
                document.body.style.cursor = "wait";
                document.forms[0].submit();
            }
        }
        // 発荷主が変更された場合
        if (row === 10) {
            // ポストバックデータを更新
            event.spread.UpdatePostbackData();
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = "Spred_shipperChanged";
                document.body.style.cursor = "wait";
                document.forms[0].submit();
            }
        }
        // 発駅、着駅が変更された場合
        if (row === 13 || row === 22) {
            // ポストバックデータを更新
            event.spread.UpdatePostbackData();
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = "Spred_stationChanged";
                document.body.style.cursor = "wait";
                document.forms[0].submit();
            }
        }
        // 発受託人、着受託人が変更された場合
        if (row === 14 || row === 15 || row === 23 || row === 24) {
            // ポストバックデータを更新
            event.spread.UpdatePostbackData();
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = "Spred_trusteeChanged";
                document.body.style.cursor = "wait";
                document.forms[0].submit();
            }
        }
        // 品目が変更された場合
        if (row === 16) {
            // ポストバックデータを更新
            event.spread.UpdatePostbackData();
            if (document.getElementById("MF_SUBMIT").value === "FALSE") {
                document.getElementById("MF_SUBMIT").value = "TRUE";
                document.getElementById("WF_ButtonClick").value = "Spred_itemChanged";
                document.body.style.cursor = "wait";
                document.forms[0].submit();
            }
        }
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

///**
// *  発受託人検索ダイアログ ボタンクリックイベント
// */
//function DeptrusteeSrc_Click() {

//    ButtonClick('WF_ButtonDeptrustee');
//}

///**
// *  着受託人検索ダイアログ ボタンクリックイベント
// */
//function ArrtrusteeSrc_Click() {

//    ButtonClick('WF_ButtonArrtrustee');
//}

///**
// *  受託人検索ダイアログ『閉じる』ボタンクリックイベント
// */
//function TrusteeSrcCloseClick() {

//    ButtonClick('WF_ButtonTrusteeSrcCLOSE');
//}

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
