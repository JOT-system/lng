const Col = {
    MODIFYKBNNAME: 1 //変更区分
    , DELFLG: 4 //削除フラグ
};

// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);


}

// 画面読み込み時処理
window.onload = function () {
    try {
        document.getElementById('WF_ButtonLogOut').style.display = 'inline';
    } catch (e) {}
}

// ○ダウンロード処理
function f_ExcelPrint() {
    // リンク参照
    var objPrintUrl = document.getElementById("WF_PrintURL");
    if (objPrintUrl === null) {
        return;
    }
    commonDownload(objPrintUrl.value);
    document.getElementById("WF_ButtonClick").value = "WF_DisplayGrid";
    document.forms[0].submit();
    //return false;
}

// ○変更箇所を強調表示
function f_ModifyHatching() {
    var objTable = document.getElementById("pnlListArea_DR").children[0];

    for (var i = 0; i < objTable.rows.length; i++) {
        //変更後の行の場合
        if (objTable.rows[i].cells[Col.MODIFYKBNNAME].innerHTML == "変更後") {
            //変更前と変更後の各項目を比較
            for (var j = Col.DELFLG; j < objTable.rows[0].cells.length; j++) {
                if (typeof (objTable.rows[i - 1]) != "undefined") {
                    //値が一致しない場合
                    if (objTable.rows[i - 1].cells[j].innerHTML != objTable.rows[i].cells[j].innerHTML) {
                        objTable.rows[i].cells[j].style.backgroundColor = "yellow";
                        objTable.rows[i].cells[j].style.fontWeight = "bold";
                        objTable.rows[i].cells[j].style.color = "red";
                    }
                }
            }
        }
    }
}
