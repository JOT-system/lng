// 画面読み込み時処理
window.onload = function () {

    // 期間重複調整画面
    OverlapPeriodsPopupOnload();

    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    const DisabledKeyItemUserId = document.getElementById('DisabledKeyItemUserId').value;
    const DisabledKeyItemPass = document.getElementById('DisabledKeyItemPass').value;
    //ユーザーID
    const TxtUserId = document.getElementById('TxtUserId');
    //開始年月日
    const TxtStYMD = document.getElementById('TxtStYMD');
    
    const TxtDelFlg = document.getElementById('TxtDelFlg');
    const TxtStaffNameS = document.getElementById('TxtStaffNameS');
    const TxtStaffNameL = document.getElementById('TxtStaffNameL');
    const TxtMissCNT = document.getElementById('TxtMissCNT');
    const TxtPassEndYMD = document.getElementById('TxtPassEndYMD');
    const TxtEndYMD = document.getElementById('TxtEndYMD');
    const TxtCampCode = document.getElementById('TxtCampCode');
    const TxtOrg = document.getElementById('TxtOrg');
    const TxtEMail = document.getElementById('TxtEMail');
    const TxtMenuRole = document.getElementById('TxtMenuRole');
    const TxtMapRole = document.getElementById('TxtMapRole');
    const TxtViewProfId = document.getElementById('TxtViewProfId');
    const TxtRprtProfId = document.getElementById('TxtRprtProfId');
    const TxtVariant = document.getElementById('TxtVariant');
    const TxtApproValid = document.getElementById('TxtApproValid');
    const TxtPassword = document.getElementById('TxtPassword');
    
    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //ユーザーID
        TxtUserId.readOnly = true;
        //開始年月日
        TxtStYMD.readOnly = true;
    };
    //情報システム部以外でログインした場合入力不可にする
    if (DisabledKeyItemUserId != "") {
        TxtDelFlg.readOnly = true;
        TxtStaffNameS.readOnly = true;
        TxtStaffNameL.readOnly = true;
        TxtMissCNT.readOnly = true;
        TxtPassEndYMD.readOnly = true;
        TxtEndYMD.readOnly = true;
        TxtCampCode.readOnly = true;
        TxtOrg.readOnly = true;
        TxtEMail.readOnly = true;
        TxtMenuRole.readOnly = true;
        TxtMapRole.readOnly = true;
        TxtViewProfId.readOnly = true;
        TxtRprtProfId.readOnly = true;
        TxtVariant.readOnly = true;
        TxtApproValid.readOnly = true;
        if (DisabledKeyItemPass != "") {
            TxtPassword.readOnly = true;
        };
    };
};

/*
 * コンテナ種別選択処理（再描画）
 */
function selectCheckBox() {

    //document.getElementById("MF_SUBMIT").value = "TRUE";
    //document.forms[0].submit();

}

// ○期間重複調整子画面OnLoad用処理
function OverlapPeriodsPopupOnload() {

    //表示・非表示項目
    const OverlapPeriodsWrapper = document.getElementById('pnlOverlapPeriodsWrapper');
    const OverlapPeriodsArea_AdjustLast = document.getElementById('pnlOverlapPeriodsLabelArea_AdjustLast');
    const OverlapPeriodsArea_Last = document.getElementById('pnlOverlapPeriodsArea_Last');
    const OverlapPeriodsArea_AdjustNext = document.getElementById('pnlOverlapPeriodsLabelArea_AdjustNext');
    const OverlapPeriodsArea_Next = document.getElementById('pnlOverlapPeriodsArea_Next');

    //表示・非表示判断キー
    const OverlapPeriodsSrc = document.getElementById('WF_OverlapPeriodsSrc');
    const OverlapPeriodsLast = document.getElementById('VisibleKey_OverlapPeriodsLast');
    const OverlapPeriodsNext = document.getElementById('VisibleKey_OverlapPeriodsNext');

    //変更不可判断項目
    const pnlTxtAdjustLastStYMD = document.getElementById('pnlTxtAdjustLastStYMD');
    const pnlTxtAdjustLastEndYMD = document.getElementById('pnlTxtAdjustLastEndYMD');
    const pnlTxtLastStYMD = document.getElementById('pnlTxtLastStYMD');
    const pnlTxtAdjustNextStYMD = document.getElementById('pnlTxtAdjustNextStYMD');
    const pnlTxtAdjustNextEndYMD = document.getElementById('pnlTxtAdjustNextEndYMD');
    const pnlTxtNextEndYMD = document.getElementById('pnlTxtNextEndYMD');
    const pnlTxtInputStYMD = document.getElementById('pnlTxtInputStYMD');
    const pnlTxtInputEndYMD = document.getElementById('pnlTxtInputEndYMD');

    //変更不可判断キー
    const DisabledKeyInput_Start = document.getElementById('DisabledKey_OverlapPeriodsInput_Start').value;
    const DisabledKeyInput_End = document.getElementById('DisabledKey_OverlapPeriodsInput_End').value;

    // 子画面の表示・非表示制御
    OverlapPeriodsWrapper.style.visibility = OverlapPeriodsSrc.value;
    pnlTxtAdjustLastStYMD.readOnly = true;
    pnlTxtAdjustLastEndYMD.readOnly = true;
    pnlTxtLastStYMD.readOnly = true;
    pnlTxtAdjustNextStYMD.readOnly = true;
    pnlTxtAdjustNextEndYMD.readOnly = true;
    pnlTxtNextEndYMD.readOnly = true;
    // 前回情報項目制御
    OverlapPeriodsArea_AdjustLast.style.display = OverlapPeriodsLast.value;
    OverlapPeriodsArea_Last.style.display = OverlapPeriodsLast.value;
    // 次回情報項目制御
    OverlapPeriodsArea_AdjustNext.style.display = OverlapPeriodsNext.value;
    OverlapPeriodsArea_Next.style.display = OverlapPeriodsNext.value;
    // 今回入力項目制御
    //  変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyInput_Start != "") {
        pnlTxtInputStYMD.readOnly = true;
    }
    else {
        pnlTxtInputStYMD.readOnly = false;
    };
    //  変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyInput_End != "") {
        pnlTxtInputEndYMD.readOnly = true;
    }
    else {
        pnlTxtInputEndYMD.readOnly = false;
    };
}

/**
 *  期間重複調整子画面『更新』ボタンクリックイベント
 */
function OverlapPeriodsSrcUpdateClick() {

    ButtonClick('WF_ButtonOverlapPeriodsSrcUpdate');
}

/**
 *  期間重複調整子画面『キャンセル』ボタンクリックイベント
 */
function OverlapPeriodsSrcCloseClick() {

    ButtonClick('WF_ButtonOverlapPeriodsSrcClose');
}