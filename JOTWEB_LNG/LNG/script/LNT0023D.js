//************************************************************
//支払先マスタメンテ登録画面
//作成日 2024/05/15
//更新日
//作成者 大浜
//更新者 
//
//修正履歴:2024/05/15 新規作成
//        :2024/08/02 星 顧客コード、顧客名自動入力処理追加
//************************************************************

// 画面読み込み時処理
window.onload = function () {
    //変更不可判断キー
    const DisabledKeyItem = document.getElementById('DisabledKeyItem').value;
    //外部コード
    const TxtToriCode = document.getElementById('TxtToriCode');
    //顧客コード
    const TxtClientCode = document.getElementById('TxtClientCode');

    //■入力不可
    //振込先銀行名
    document.getElementById('TxtPayBankName').readOnly = true;

    //振込先銀行名カナ
    document.getElementById('TxtPayBankNameKana').readOnly = true;

    //振込先支店名
    document.getElementById('TxtPayBankBranchName').readOnly = true;

    //振込先支店名カナ
    document.getElementById('TxtPayBankBranchNameKana').readOnly = true;

    //預金種別コード
    document.getElementById('TxtPayAccountType').readOnly = true;

    //連携状態区分
    document.getElementById('TxtLinkStatus').readOnly = true;
    //変更不可判断キーに値が入っている場合、一意項目を入力不可にする
    if (DisabledKeyItem != "") {
        //外部コード
        TxtToriCode.readOnly = true;
        //顧客コード
        TxtClientCode.readOnly = true;  
    };
};

function PayAccountTypeNameOnchange() {
    let ddlPayAccountTypeName = document.getElementById('ddlPayAccountTypeName').value;
    let PayAccountType = document.getElementById('TxtPayAccountType');

    switch (ddlPayAccountTypeName) {
        case '普通':
            PayAccountType.value = "1";
            break;
        case '当座':
            PayAccountType.value = "2";
            break;
        default:
            PayAccountType.value = "";
    }

}

//2024/08/02 星ADD START
function TxtToriCodeOnchange() {
    let TxtToriCode = document.getElementById('TxtToriCode').value;
    let TxtClientCode = document.getElementById('TxtClientCode');

    if (TxtToriCode === "") {
        TxtClientCode.value = "";
    }
    else {
        TxtClientCode.value = "01-" + TxtToriCode + "-1";
    };
}

function TxtToriNameOnchange() {
    let TxtToriName = document.getElementById('TxtToriName').value;
    let TxtToriDivName = document.getElementById('TxtToriDivName').value;
    let TxtClientName = document.getElementById('TxtClientName');

    TxtClientName.value = TxtToriName + TxtToriDivName;
}
//2024/08/02 星ADD END
