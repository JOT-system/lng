// 画面読み込み時処理
window.onload = function () {

    // スプレッドのフォーカス移動設定処理
    setKeyMap()

    //月選択バインド
    commonBindMonthPicker();

    if(document.getElementById("WF_CloseFLG").value === "0"){
        for(var i=0; i < 6; i++){
            var idname = "Hokkaido"+i
            var syoflg = "WF_Hokkaido"+i
            var obj = document.getElementById(idname);
            var flg = document.getElementById(syoflg);
            if(flg.value === "0"){
                obj.style.backgroundColor = '#ffe3e3'; //未承認込み
                obj.style.color = '#822424';
            }
        }
        for(var i=0; i < 6; i++){
            var idname = "Touhoku"+i
            var syoflg = "WF_Touhoku"+i
            var obj = document.getElementById(idname);
            var flg = document.getElementById(syoflg);
            if(flg.value === "0"){
                obj.style.backgroundColor = '#ffe3e3'; //未承認込み
                obj.style.color = '#822424';
            }
        }
        for(var i=0; i < 6; i++){
            var idname = "Kantou"+i
            var syoflg = "WF_Kantou"+i
            var obj = document.getElementById(idname);
            var flg = document.getElementById(syoflg);
            if(flg.value === "0"){
                obj.style.backgroundColor = '#ffe3e3';  //未承認込み
                obj.style.color = '#822424';
            }
        }
        for(var i=0; i < 6; i++){
            var idname = "Tyubu"+i
            var syoflg = "WF_Tyubu"+i
            var obj = document.getElementById(idname);
            var flg = document.getElementById(syoflg);
            if(flg.value === "0"){
                obj.style.backgroundColor = '#ffe3e3';  //未承認込み
                obj.style.color = '#822424';
            }
        }
        for(var i=0; i < 6; i++){
            var idname = "Kansai"+i
            var syoflg = "WF_Kansai"+i
            var obj = document.getElementById(idname);
            var flg = document.getElementById(syoflg);
            if(flg.value === "0"){
                obj.style.backgroundColor = '#ffe3e3';  //未承認込み
                obj.style.color = '#822424';
            }
        }
        for(var i=0; i < 6; i++){
            var idname = "Kyusyu"+i
            var syoflg = "WF_Kyusyu"+i
            var obj = document.getElementById(idname);
            var flg = document.getElementById(syoflg);
            if(flg.value === "0"){
                obj.style.backgroundColor = '#ffe3e3';  //未承認込み
                obj.style.color = '#822424';
            }
        }
        for(var i=0; i < 6; i++){
            var idname = "CTN"+i
            var syoflg = "WF_CTN"+i
            var obj = document.getElementById(idname);
            var flg = document.getElementById(syoflg);
            if(flg.value === "0"){
                obj.style.backgroundColor = '#ffe3e3';  //未承認込み
                obj.style.color = '#822424';
            }
        }
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

// ○スプレッドシート内ボタン押下処理
function Spred_ButtonSel_click(btn) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = btn;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// ○一括ダウンロード処理
function f_AccountingDownload() {
    var objPrintUrl = document.getElementById("WF_PrintURL1").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL2").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL3").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    objPrintUrl = document.getElementById("WF_PrintURL4").value;
    if (objPrintUrl !== "") {
        commonDownload(objPrintUrl);
    }
    return false;
}
/**
 * ダウンロード処理
 * @param {string} url ダウンロードURL
 * @return {undefined} なし
 * @description 
 */
function commonDownload(url) {
    if ('1' === url) {
        //IEの場合
        window.open(url + '?rnd=' + new Date().getTime(), "view", "_blank");
    } else {
        // IE以外の場合
        var fileName = url.substring(url.lastIndexOf('/') + 1);
        let nondecodeFileName = fileName;
        fileName = decodeURIComponent(fileName);
        // リンク（<a>要素）を生成し、JavaScriptからクリックする
        var link = document.createElement("a");
        // キャッシュされたファイルをダウンロード扱いしないためURLパラメータをダミーで付与
        if (url.indexOf('?') === -1) {
            link.href = url + '?rnd=' + new Date().getTime();
        } else {
            link.href = url;
        }

        link.id = 'commondownloaddummylink';
        //link.download = fileName;
        link.setAttribute('download', nondecodeFileName);
        link.target = '_blank';
        link.innerText = 'dl';
        link.style.display = 'none';
        link.type = 'application/octet-stream';
        link.rel = 'noopener noreferrer';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
}

/**
 * コンテナ種別・経理資産区分選択処理（再描画）
 */
function selectCheckBox() {

    //document.getElementById("MF_SUBMIT").value = "TRUE";
    //document.forms[0].submit();

}
// スプレッドのフォーカス移動設定処理
function setKeyMap() {
    
// ダウンロードボタン入力不可制御
if (document.getElementById('WF_CSVDLDisabledFlg').value === "1"){
    document.getElementById('WF_CSV_DL').disabled = true;
} else {
    document.getElementById('WF_CSV_DL').disabled = false;
}
}