// 画面読み込み時処理
window.onload = function () {
    document.getElementById('WF_ButtonLogOut').style.display = 'inline';

    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //非表示判断キー
    const VisibleKeyControlTable = document.getElementById('VisibleKeyControlTable').value;

    //入力不可キー
    document.getElementById('TxtSelLineCNT').readOnly = true; 
    document.getElementById('TxtCampCode').readOnly = true;

    //非表示項目
    switch (VisibleKeyControlTable) {
        case "LNM0010LHA": //八戸特別料金マスタ
            document.getElementById('SPRATE_LINE_TODOKECODE').style.display = "none";　//届先コード
            document.getElementById('SPRATE_LINE_SYABAN').style.display = "none";　//車番
            document.getElementById('SPRATE_LINE_TAISHOYM').style.display = "none";　//対象年月
            document.getElementById('SPRATE_LINE_SYABARA').style.display = "none";　//車腹
            document.getElementById('SPRATE_LINE_KOTEIHI').style.display = "none";　//固定費
            document.getElementById('SPRATE_LINE_KYORI_KEIYU').style.display = "none";　//走行距離、実勢軽油価格
            document.getElementById('SPRATE_LINE_KIZYUN_TANKASA').style.display = "none";　//基準価格、単価差
            document.getElementById('SPRATE_LINE_KAISU').style.display = "none";　//輸送回数
            document.getElementById('SPRATE_LINE_COUNT').style.display = "none";　//回数
            document.getElementById('SPRATE_LINE_USAGECHARGE_SURCHARGE').style.display = "none";　//燃料使用量、サーチャージ
            document.getElementById('SPRATE_LINE_BIKOU1').style.display = "none";　//備考1
            document.getElementById('SPRATE_LINE_BIKOU2').style.display = "none";　//備考2
            document.getElementById('SPRATE_LINE_BIKOU3').style.display = "none";　//備考3
            break;
        case "LNM0010LEN": //ENEOS業務委託料マスタ
            document.getElementById('SPRATE_LINE_TODOKECODE').style.display = "none";　//届先コード
            document.getElementById('SPRATE_LINE_SYABAN').style.display = "none";　//車番
            document.getElementById('SPRATE_LINE_TAISHOYM').style.display = "none";　//対象年月
            document.getElementById('SPRATE_LINE_SYABARA').style.display = "none";　//車腹
            document.getElementById('SPRATE_LINE_KOTEIHI').style.display = "none";　//固定費
            document.getElementById('SPRATE_LINE_KYORI_KEIYU').style.display = "none";　//走行距離、実勢軽油価格
            document.getElementById('SPRATE_LINE_KIZYUN_TANKASA').style.display = "none";　//基準価格、単価差
            document.getElementById('SPRATE_LINE_KAISU').style.display = "none";　//輸送回数
            document.getElementById('SPRATE_LINE_COUNT').style.display = "none";　//回数
            document.getElementById('SPRATE_LINE_USAGECHARGE_SURCHARGE').style.display = "none";　//燃料使用量、サーチャージ
            document.getElementById('SPRATE_LINE_BIKOU1').style.display = "none";　//備考1
            document.getElementById('SPRATE_LINE_BIKOU2').style.display = "none";　//備考2
            document.getElementById('SPRATE_LINE_BIKOU3').style.display = "none";　//備考3
            break;
        case "LNM0010LTO": //東北電力車両別追加料金マスタ
            document.getElementById('SPRATE_LINE_RECOID').style.display = "none";　//レコードID
            document.getElementById('SPRATE_LINE_TODOKECODE').style.display = "none";　//届先コード
            document.getElementById('SPRATE_LINE_KINGAKU').style.display = "none";　//金額
            document.getElementById('SPRATE_LINE_TAISHOYM').style.display = "none";　//対象年月
            document.getElementById('SPRATE_LINE_SYABARA').style.display = "none";　//車腹
            document.getElementById('SPRATE_LINE_KYORI_KEIYU').style.display = "none";　//走行距離、実勢軽油価格
            document.getElementById('SPRATE_LINE_KIZYUN_TANKASA').style.display = "none";　//基準価格、単価差
            document.getElementById('SPRATE_LINE_KAISU').style.display = "none";　//輸送回数
            document.getElementById('SPRATE_LINE_USAGECHARGE_SURCHARGE').style.display = "none";　//燃料使用量、サーチャージ
            document.getElementById('SPRATE_LINE_BIKOU1').style.display = "none";　//備考1
            document.getElementById('SPRATE_LINE_BIKOU2').style.display = "none";　//備考2
            document.getElementById('SPRATE_LINE_BIKOU3').style.display = "none";　//備考3
            break;
        case "LNM0010LSKSP": //SK特別料金マスタ
            document.getElementById('SPRATE_LINE_KINGAKU').style.display = "none";　//金額
            document.getElementById('SPRATE_LINE_SYABAN').style.display = "none";　//車番
            document.getElementById('SPRATE_LINE_TAISHOYM').style.display = "none";　//対象年月
            document.getElementById('SPRATE_LINE_KYORI_KEIYU').style.display = "none";　//走行距離、実勢軽油価格
            document.getElementById('SPRATE_LINE_KIZYUN_TANKASA').style.display = "none";　//基準価格、単価差
            document.getElementById('SPRATE_LINE_KAISU').style.display = "none";　//輸送回数
            document.getElementById('SPRATE_LINE_COUNT').style.display = "none";　//回数
            document.getElementById('SPRATE_LINE_USAGECHARGE_SURCHARGE').style.display = "none";　//燃料使用量、サーチャージ
            break;
        case "LNM0010LSKSU": //SK燃料サーチャージマスタ
            document.getElementById('SPRATE_LINE_ENDYMD_ANNOTATION').style.display = "none";　//有効終了日文言
            document.getElementById('SPRATE_LINE_RECOID').style.display = "none";　//レコードID
            document.getElementById('SPRATE_LINE_KIZYUN_TANKASA').style.display = "none";　//基準価格、単価差
            document.getElementById('SPRATE_LINE_STYMD_ENDYMD').style.display = "none";　//有効開始日、有効終了日
            document.getElementById('SPRATE_LINE_KINGAKU').style.display = "none";　//金額
            document.getElementById('SPRATE_LINE_SYABAN').style.display = "none";　//車番
            document.getElementById('SPRATE_LINE_SYABARA').style.display = "none";　//車腹
            document.getElementById('SPRATE_LINE_KOTEIHI').style.display = "none";　//固定費
            document.getElementById('SPRATE_LINE_COUNT').style.display = "none";　//回数
            document.getElementById('SPRATE_LINE_BIKOU2').style.display = "none";　//備考2
            document.getElementById('SPRATE_LINE_BIKOU3').style.display = "none";　//備考3
            break;
    }

    //変更不可判断キーに値が入っていない場合
    if (DisabledKeyItem == "") {

        console.log(DisabledKeyItem);


        //名称を変更する
        document.getElementById('WF_ButtonUPDATE').value = "追加";
        switch (VisibleKeyControlTable) {
            case "LNM0010LHA": //八戸特別料金マスタ
                document.getElementById('PAGE_NAME1').innerText = "八戸特別料金マスタ（追加）";
                document.getElementById('PAGE_NAME2').innerText = "八戸特別料金マスタ追加";
                break;
            case "LNM0010LEN": //ENEOS業務委託料マスタ
                document.getElementById('PAGE_NAME1').innerText = "ENEOS業務委託料マスタ（追加）";
                document.getElementById('PAGE_NAME2').innerText = "ENEOS業務委託料マスタ追加";
                break;
            case "LNM0010LTO": //東北電力車両別追加料金マスタ
                document.getElementById('PAGE_NAME1').innerText = "東北電力車両別追加料金マスタ（追加）";
                document.getElementById('PAGE_NAME2').innerText = "東北電力車両別追加料金マスタ追加";
                break;
            case "LNM0010LSKSP": //SK特別料金マスタ
                document.getElementById('PAGE_NAME1').innerText = "SK特別料金マスタ（追加）";
                document.getElementById('PAGE_NAME2').innerText = "SK特別料金マスタ追加";
                break;
            case "LNM0010LSKSU": //SK燃料サーチャージマスタ
                document.getElementById('PAGE_NAME1').innerText = "SK燃料サーチャージマスタ（追加）";
                document.getElementById('PAGE_NAME2').innerText = "SK燃料サーチャージマスタ追加";
                break;
        }
    } else {
        switch (VisibleKeyControlTable) {
            case "LNM0010LHA": //八戸特別料金マスタ
                document.getElementById('PAGE_NAME1').innerText = "八戸特別料金マスタ（詳細）";
                document.getElementById('PAGE_NAME2').innerText = "八戸特別料金マスタ詳細";
                break;
            case "LNM0010LEN": //ENEOS業務委託料マスタ
                document.getElementById('PAGE_NAME1').innerText = "ENEOS業務委託料マスタ（詳細）";
                document.getElementById('PAGE_NAME2').innerText = "ENEOS業務委託料マスタ詳細";
                break;
            case "LNM0010LTO": //東北電力車両別追加料金マスタ
                document.getElementById('PAGE_NAME1').innerText = "東北電力車両別追加料金マスタ（詳細）";
                document.getElementById('PAGE_NAME2').innerText = "東北電力車両別追加料金マスタ詳細";
                break;
            case "LNM0010LSKSP": //SK特別料金マスタ
                document.getElementById('PAGE_NAME1').innerText = "SK特別料金マスタ（詳細）";
                document.getElementById('PAGE_NAME2').innerText = "SK特別料金マスタ詳細";
                break;
            case "LNM0010LSKSU": //SK燃料サーチャージマスタ
                document.getElementById('PAGE_NAME1').innerText = "SK燃料サーチャージマスタ（詳細）";
                document.getElementById('PAGE_NAME2').innerText = "SK燃料サーチャージマスタ詳細";
                break;
        }
    }

    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") { 
        //取引先コード
        document.getElementById('TxtTORICODE').readOnly = true;
        //document.getElementById('TxtTORICODEcommonIcon').style.display = "none";
        //取引先名称
        document.getElementById('TxtTORINAME').readOnly = true;
        //加算先部門コード
        document.getElementById('TxtKASANORGCODE').readOnly = true;
        // document.getElementById('TxtKASANORGCODEcommonIcon').style.display = "none";
    
        //加算先部門名称
        document.getElementById('TxtKASANORGNAME').readOnly = true;
        //有効開始日
        //document.getElementById('WF_StYMD').readOnly = true;
        //document.getElementById('WF_StYMD_CALENDAR').style.display = "none";
    };

};

document.addEventListener("DOMContentLoaded", function () {
   var ControlTable = document.getElementById('VisibleKeyControlTable').value;
    switch (ControlTable) {
        case "LNM0010LHA": //八戸特別料金マスタ
            // カレンダー表示
            document.querySelectorAll('.datetimepicker').forEach(picker => {
                flatpickr(picker, {
                    wrap: true,
                    dateFormat: 'Y/m/d',
                    locale: 'ja',
                    clickOpens: false,
                    allowInput: true,
                    monthSelectorType: 'static',
                    //defaultDate: new Date() // 必要に応じてカスタマイズ
                });
            });
            break;
        case "LNM0010LEN": //ENEOS業務委託料マスタ
            // カレンダー表示
            document.querySelectorAll('.datetimepicker').forEach(picker => {
                flatpickr(picker, {
                    wrap: true,
                    dateFormat: 'Y/m/d',
                    locale: 'ja',
                    clickOpens: false,
                    allowInput: true,
                    monthSelectorType: 'static',
                    //defaultDate: new Date() // 必要に応じてカスタマイズ
                });
            });
            break;
        case "LNM0010LTO": //東北電力車両別追加料金マスタ
            // カレンダー表示
            document.querySelectorAll('.datetimepicker').forEach(picker => {
                flatpickr(picker, {
                    wrap: true,
                    dateFormat: 'Y/m/d',
                    locale: 'ja',
                    clickOpens: false,
                    allowInput: true,
                    monthSelectorType: 'static',
                    //defaultDate: new Date() // 必要に応じてカスタマイズ
                });
            });
            break;
        case "LNM0010LSKSP": //SK特別料金マスタ
            // カレンダー表示
            document.querySelectorAll('.datetimepicker').forEach(picker => {
                flatpickr(picker, {
                    wrap: true,
                    dateFormat: 'Y/m/d',
                    locale: 'ja',
                    clickOpens: false,
                    allowInput: true,
                    monthSelectorType: 'static',
                    //defaultDate: new Date() // 必要に応じてカスタマイズ
                });
            });
            break;
        case "LNM0010LSKSU": //SK燃料サーチャージマスタ
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
            break;
        }
});

