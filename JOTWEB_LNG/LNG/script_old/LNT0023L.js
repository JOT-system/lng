//************************************************************
//支払先マスタメンテ一覧画面
//作成日 2024/05/15
//更新日 
//作成者 大浜
//更新者 
//
//修正履歴:2024/05/15 新規作成
//        :2024/08/02 星 送信時、送信チェック判定追加
//************************************************************

const Col = {
    LINKCHK: 0 //送信チェック
};


// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);


}

//送信ボタン押下時
function btnLinkClick(obj, lineCnt, fieldNM) {
    //チェックボックスの状態を確認
    var objDL = document.getElementById("pnlListArea_DL").children[0];
    var objDR = document.getElementById("pnlListArea_DR").children[0];
    var wkchk;

    for (var i = 0; i < objDL.rows.length; i++) {
        if (objDL.rows[i].cells[0].innerHTML == lineCnt) {
            //wkchk = objDR.rows[i].cells[Col.LINKCHK].querySelector("input[type='checkbox']");   '2024/08/02 星DEL
            //if (wkchk.checked) {                                                                '2024/08/02 星DEL
                document.getElementById("WF_SelectedIndex").value = lineCnt
                document.getElementById("WF_ButtonClick").value = "WF_LinkButtonClick";
                document.forms[0].submit();
            //}                                                                                   '2024/08/02 星DEL
        }
    }
}

//チェックボックス変更時
function chkLinkOnChange() {
    var objDL = document.getElementById("pnlListArea_DL").children[0];
    var objDR = document.getElementById("pnlListArea_DR").children[0];

    var lineCntArray = [];
    var CHKLINKArray = [];
    var wkchk;

    for (var i = 0; i < objDL.rows.length; i++) {
        lineCntArray.push(objDL.rows[i].cells[0].innerHTML); //項番
        wkchk = objDR.rows[i].cells[Col.LINKCHK].querySelector("input[type='checkbox']");
        if (wkchk.checked) {
        // チェック有
            CHKLINKArray.push("有");
        } else {
        // チェック無
            CHKLINKArray.push("無");
        }
    }
    document.getElementById("WF_lineCntLIST").value = lineCntArray.join(",");
    document.getElementById("WF_CHKLINKLIST").value = CHKLINKArray.join(",");
}