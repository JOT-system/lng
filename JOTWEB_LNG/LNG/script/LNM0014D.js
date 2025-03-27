﻿// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //const VisibleKeyOrgCode = document.getElementById('VisibleKeyOrgCode').value;

    //入力不可キー
    document.getElementById('TxtSelLineCNT').readOnly = true; 
    document.getElementById('TxtCampCode').readOnly = true;

    ////情シス、高圧ガス以外の場合、パンくず(検索)をを非表示にする
    //if (VisibleKeyOrgCode == "") {
    //    document.getElementById('PAGE_SEARCH').style.display = "none";
    //}

    //変更不可判断キーに値が入っていない場合
    if (DisabledKeyItem == "") {
        //名称を変更する
        document.getElementById('PAGE_NAME1').innerText = "特別料金マスタ（追加）";
        document.getElementById('PAGE_NAME2').innerText = "特別料金マスタ追加";
        document.getElementById('WF_ButtonUPDATE').value = "追加";

        ////選択可能な部門コードが2件(空白行1件と選択可能行1件)の場合
        //if (document.getElementById('DisabledKeyOrgCount').value == "2") {
        //    //選択可能な取引先コードが1件の場合
        //    if (document.getElementById('DisabledKeyToriCount').value == "1") {
        //       //取引先コード、取引先名称入力不可
        //        document.getElementById('TxtTORICODE').readOnly = true;
        //        document.getElementById('TxtTORICODEcommonIcon').style.display = "none";
        //        document.getElementById('TxtTORINAME').readOnly = true;
        //       //加算先部門コード、加算先部門名称入力不可
        //        document.getElementById('TxtKASANORGCODE').readOnly = true;
        //        document.getElementById('TxtKASANORGCODEcommonIcon').style.display = "none";
        //        document.getElementById('TxtKASANORGNAME').readOnly = true;
        //    }
        //}
    }

    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") { 
        //取引先コード
        document.getElementById('TxtTORICODE').readOnly = true;
        //取引先名称
        document.getElementById('TxtTORINAME').readOnly = true;
        //グループ名
        document.getElementById('TxtGROUPNAME').readOnly = true;
    };

};

document.addEventListener("DOMContentLoaded", function () {
    // カレンダー表示
    document.querySelectorAll('.datetimepicker').forEach(picker => {
        flatpickr(picker, {
            wrap: true,
            dateFormat: 'Y/m/d',
            locale: 'ja',
            clickOpens: false,
            allowInput: true,
            plugins: [
                new monthSelectPlugin({
                    shorthand: true, //defaults to false
                    dateFormat: "Y/m",
                    altFormat: "F Y", //defaults to "F Y"
                    theme: "light" // defaults to "light"
                })
            ]
        });
    });
});