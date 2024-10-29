// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //コンテナ記号
    const TxtCTNType = document.getElementById('TxtCTNType');
    const TxtCTNTypecommonIcon = document.getElementById('TxtCTNTypecommonIcon');
    const TxtCTNTypeEvent = document.getElementById('TxtCTNTypeEvent');
    //コンテナ番号
    const TxtCTNNo = document.getElementById('TxtCTNNo');
    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //コンテナ記号
        TxtCTNType.readOnly = true;
        TxtCTNTypecommonIcon.style.display = "none";
        TxtCTNTypeEvent.disabled = "disabled";
        TxtCTNTypeEvent.ondblclick = "";
        //コンテナ番号
        TxtCTNNo.readOnly = true;
    };
    
    // スクロール位置を復元 
    if (document.getElementById("divContensbox") !== null) {
        document.getElementById("divContensbox").scrollTop = document.getElementById("WF_ClickedScrollTop").value;
    }

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen") !== null) {
        if (document.getElementById("WF_LeftboxOpen").value === "Open") {
            document.getElementById("LF_LEFTBOX").style.display = "block";
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
};

function saveScrollPosition() {
    let detailbox = document.getElementById("divContensbox");
    if (detailbox !== null) {
        document.getElementById("WF_ClickedScrollTop").value = detailbox.scrollTop;
    }
}

window.addEventListener("DOMContentLoaded", () => {
    // 全体スクロールイベントに、ポジション記録処理を付与する
    document.getElementById("divContensbox").addEventListener('scroll', saveScrollPosition);
});


