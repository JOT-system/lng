
window.onload = function () {
    const ReportId = document.getElementById('TxtReportId').value;
    const OrgArea = document.getElementById('WF_ORG_AREA');
    const OrgSingleOrAllLabel = document.getElementById('WF_ORG_MASSEAGE');
    const OrgSingleOrMultipleLabel = document.getElementById('WF_ORG_MASSEAGE2');
    const DllOrgArea = document.getElementById('WF_ORGCODE_SELECT_DDL');
    const DllLeaseOrgArea = document.getElementById('WF_ORGCODE_LEASE_SELECT_DDL');
    const AllOrgArea = document.getElementById('WF_ORGCODE_ALL_SELECT');
    const SelectOrgArea = document.getElementById('WF_ORGCODE_SELECT');
    const SelectMultipleOrgArea = document.getElementById('WF_ORGCODE_MULTIPLE_SELECT');
    const StYMDLabel = document.getElementById('WF_STYMD_LABEL');
    const StYMDTitle = document.getElementById('WF_STYMD_TITLE');
    const StYMDArea = document.getElementById('WF_STYMD');
    const StYMDTextArea = document.getElementById('WF_STYMD_TEXT');
    const EndYMDLabel = document.getElementById('WF_ENDYMD_LABEL');
    const EndYMDArea = document.getElementById('WF_ENDYMD_AREA');
    const STYMTitleArea = document.getElementById('WF_STYM_TITLE_AREA');
    const STYMTextArea = document.getElementById('WF_STYM_TEXT');
    const SelectDateLabel = document.getElementById('WF_SELECDATE_LABEL');
    const SelectYMLabel = document.getElementById('WF_TARGETYM_LABEL');
    const BillingYMLabel = document.getElementById('WF_BILLINGYM_LABEL');
    const BillingFromYMLabel = document.getElementById('WF_BILLINGFROMYM_LABEL');
    const BillingMassage = document.getElementById('WF_BILLING_MASSEAGE');
    const SelectYMArea = document.getElementById('WF_TARGETYM');
    const BillingToYMLabel = document.getElementById('WF_BILLINGTOYM_LABEL');
    const BillingToYMArea = document.getElementById('WF_BILLINGTOYM_AREA');
    const ShipYmdFROMArea = document.getElementById('WF_SHIPYMDFROM_AREA');
    const ShipYmdTOArea = document.getElementById('WF_SHIPYMDTO_AREA');
    const DepStaLabel = document.getElementById('WF_DEPSTATION_LABEL');
    const NowStaLabel = document.getElementById('WF_NOWSTATION_LABEL');
    const StationArea = document.getElementById('WF_STA_AREA');
    const Prt0003Area = document.getElementById('WF_PRT0003_AREA');
    const Prt0005Area = document.getElementById('WF_PRT0005_AREA');
    const Prt0006Area = document.getElementById('WF_PRT0006_AREA');
    const Prt0007Area = document.getElementById('WF_PRT0007_AREA');
    const Prt0008Area = document.getElementById('WF_PRT0008_AREA');
    const Prt0012Area = document.getElementById('WF_PRT0012_AREA');
    const Prt0014Area = document.getElementById('WF_PRT0014_AREA');
    const Prt0016Area = document.getElementById('WF_PRT0016_AREA');
    const StackfreeArea = document.getElementById('WF_STACKFREE_AREA');
    const ReplaceArea = document.getElementById('WF_REPLACE_AREA');

    BindMonthPicker();

    // 画面項目初期設定
    OrgSingleOrAllLabel.style.display = "";
    OrgSingleOrMultipleLabel.style.display = "none";
    StYMDTitle.style.display = "";
    StYMDLabel.style.display = ""
    StYMDArea.style.display = "";
    StYMDTextArea.style.display = "";
    EndYMDLabel.style.display = "";
    EndYMDArea.style.display = "";
    OrgArea.style.display = "";
    DllOrgArea.style.display = "none";
    DllLeaseOrgArea.style.display = "none";
    AllOrgArea.style.display = "";
    STYMTitleArea.style.display = "";
    STYMTextArea.style.display = "";
    SelectOrgArea.style.display = "";
    SelectMultipleOrgArea.style.display = "none";
    SelectDateLabel.style.display = "none";
    SelectYMLabel.style.display = "none";
    BillingYMLabel.style.display = "none";
    BillingFromYMLabel.style.display = "none";
    BillingMassage.style.display = "none";
    SelectYMArea.style.display = "none";
    BillingToYMLabel.style.display = "none";
    BillingToYMArea.style.display = "none";
    ShipYmdFROMArea.style.display = "none";
    ShipYmdTOArea.style.display = "none";
    DepStaLabel.style.display = "none";
    NowStaLabel.style.display = "none";
    StationArea.style.display = "none";
    Prt0003Area.style.display = "none";
    Prt0005Area.style.display = "none";
    Prt0006Area.style.display = "none";
    Prt0007Area.style.display = "none";
    Prt0008Area.style.display = "none";
    Prt0012Area.style.display = "none";
    Prt0014Area.style.display = "none";
    Prt0016Area.style.display = "none";
    StackfreeArea.style.display = "none";
    ReplaceArea.style.display = "none";

    StYMDLabel.className = "WF_TEXT_LEFT requiredMark"
    document.getElementById('WF_ORG_LABEL').className = "WF_TEXT_LEFT requiredMark"
    document.getElementById('WF_ORG_LABEL2').className = "WF_TEXT_LEFT requiredMark"

    if (ReportId.substring(0, 7) == "PRT0001") {
        OrgArea.style.display = "none";
        DllOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
    } else if (ReportId.substring(0, 7) == "PRT0002") {
        OrgArea.style.display = "none";
        DllOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        //他駅発送明細が選ばれている場合、不要項目を非表示にする
    } else if (ReportId.substring(0, 7) == "LNT0010") {
        StYMDLabel.style.display = "none";
        EndYMDArea.style.display = "none";
        SelectDateLabel.style.display = "";
        SelectDateLabel.className = "WF_TEXT_LEFT requiredMark"
        OrgArea.style.display = "none";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        // コンテナ留置先一覧
    } else if (ReportId.substring(0, 7) == "PRT0003") {
        OrgArea.style.display = "none";
        DllOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        Prt0003Area.style.display = "";
        StYMDLabel.className = ""
        EndYMDLabel.className = "WF_TEXT_LEFT requiredMark"
        // 品目別販売実績表
    } else if (ReportId.substring(0, 7) == "PRT0004") {
        OrgArea.style.display = "none";
        DllOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        DepStaLabel.style.display = "";
        StationArea.style.display = "";
        EndYMDLabel.className = "WF_TEXT_LEFT requiredMark"
        // コンテナ動静表
    } else if (ReportId.substring(0, 7) == "PRT0005") {
        OrgArea.style.display = "none";
        DllOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        StYMDLabel.style.display = "none";
        EndYMDArea.style.display = "none";
        SelectDateLabel.style.display = "";
        NowStaLabel.style.display = "";
        StationArea.style.display = "";
        Prt0005Area.style.display = "";
        SelectDateLabel.className = "WF_TEXT_LEFT requiredMark"
        if (document.getElementById('TxtSearch').value == "1") {
            document.getElementById('WF_CTNTYPE_LABEL').className = "WF_TEXT_LEFT requiredMark"
            document.getElementById('WF_STCTNNO_LABEL').className = "WF_TEXT_LEFT requiredMark"
            document.getElementById('WF_ENDCTNNO_LABEL').className = "WF_TEXT_LEFT requiredMark"
        } else if (document.getElementById('TxtSearch').value == "2") {
            document.getElementById('WF_ORG_LABEL').className = "WF_TEXT_LEFT requiredMark"
        } else if (document.getElementById('TxtSearch').value == "3") {
            NowStaLabel.className = "WF_TEXT_LEFT requiredMark"
        } else if (document.getElementById('TxtSearch').value == "4") {
            document.getElementById('WF_ORG_LABEL').className = "WF_TEXT_LEFT requiredMark"
            document.getElementById('WF_STAGNATION_LABEL').className = "WF_TEXT_LEFT requiredMark"
        }
        // 発駅・通運別合計表
    } else if (ReportId.substring(0, 7) == "PRT0006") {
        OrgArea.style.display = "none";
        DllOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        DepStaLabel.style.display = "";
        StationArea.style.display = "";
        BillingYMLabel.style.display = "";
        SelectYMArea.style.display = "";
        Prt0006Area.style.display = "";
        BillingMassage.style.display = "";
        ShipYmdFROMArea.style.display = "";
        ShipYmdTOArea.style.display = "";
        StYMDLabel.style.display = "none"
        StYMDArea.style.display = "none";
        EndYMDArea.style.display = "none";
        // リース料明細チェックリスト
    } else if (ReportId.substring(0, 7) == "PRT0007") {
        OrgArea.style.display = "none";
        DllLeaseOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        StYMDLabel.style.display = "none"
        StYMDArea.style.display = "none";
        EndYMDArea.style.display = "none";
        SelectYMLabel.style.display = "";
        SelectYMArea.style.display = "";
        Prt0007Area.style.display = "";
        SelectYMLabel.className = "WF_TEXT_LEFT requiredMark"
        // 支店間流動表(金額)
    } else if (ReportId.substring(0, 7) == "PRT0008") {
        OrgArea.style.display = "none";
        Prt0008Area.style.display = "";
        StackfreeArea.style.display = "";
        ReplaceArea.style.display = "none";
        EndYMDLabel.className = "WF_TEXT_LEFT requiredMark"
        OrgArea.style.display = "none";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        // 支店間流動表・前年対比
    } else if (ReportId.substring(0, 7) == "PRT0009") {
        OrgArea.style.display = "none";
        Prt0008Area.style.display = "";
        StackfreeArea.style.display = "none";
        ReplaceArea.style.display = "";
        EndYMDLabel.className = "WF_TEXT_LEFT requiredMark"
        OrgArea.style.display = "none";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        // 発駅・通運別合計表(期間)
    } else if (ReportId.substring(0, 7) == "PRT0010") {
        OrgArea.style.display = "none";
        DllOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        DepStaLabel.style.display = "";
        StationArea.style.display = "";
        BillingFromYMLabel.style.display = "";
        SelectYMArea.style.display = "";
        BillingToYMLabel.style.display = "";
        BillingToYMArea.style.display = "";
        Prt0006Area.style.display = "";
        BillingMassage.style.display = "";
        ShipYmdFROMArea.style.display = "";
        ShipYmdTOArea.style.display = "";
        StYMDLabel.style.display = "none"
        StYMDArea.style.display = "none";
        EndYMDArea.style.display = "none";
        // コンテナ回送費明細（発駅・受託人別）
    } else if (ReportId.substring(0, 7) == "PRT0011") {
        OrgArea.style.display = "none";
        DllOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        StYMDLabel.style.display = "none"
        StYMDArea.style.display = "none";
        EndYMDLabel.style.display = "none";
        EndYMDArea.style.display = "none";
        SelectYMLabel.style.display = "";
        SelectYMArea.style.display = "";
        document.getElementById('WF_ORG_LABEL').className = "WF_TEXT_LEFT requiredMark"
        SelectYMLabel.className = "WF_TEXT_LEFT requiredMark"
        // レンタルコンテナ回送費明細(コンテナ別)
    } else if (ReportId.substring(0, 7) == "PRT0012") {
        OrgArea.style.display = "none";
        DllOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        Prt0012Area.style.display = "";
        EndYMDLabel.className = "requiredMark";
        document.getElementById('WF_ORG_LABEL').className = "WF_TEXT_LEFT requiredMark"
    // リース満了一覧表
    } else if (ReportId.substring(0, 7) == "PRT0013") {
        OrgArea.style.display = "none";
        DllLeaseOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        StYMDLabel.style.display = "none"
        StYMDArea.style.display = "none";
        EndYMDLabel.style.display = "none";
        EndYMDArea.style.display = "none";
        SelectYMLabel.style.display = "";
        SelectYMArea.style.display = "";
        document.getElementById('WF_ORG_LABEL3').className = "WF_TEXT_LEFT requiredMark"
        SelectYMLabel.className = "WF_TEXT_LEFT requiredMark"
    // 請求先・勘定科目別・計上店別営業収入計上一覧(全勘定科目)
    } else if (ReportId.substring(0, 7) == "PRT0014") {
        StYMDTitle.style.display = "none";
        StYMDLabel.style.display = "none";
        StYMDArea.style.display = "none";
        StYMDTextArea.style.display = "none";
        EndYMDLabel.style.display = "none";
        EndYMDArea.style.display = "none";
        STYMTitleArea.style.display = "none";
        STYMTextArea.style.display = "none";
        OrgArea.style.display = "none";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        Prt0014Area.style.display = "";
    // 科目別集計表
    } else if (ReportId.substring(0, 7) == "PRT0015") {
        OrgArea.style.display = "none";
        DllLeaseOrgArea.style.display = "none";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        StYMDLabel.style.display = "none"
        StYMDArea.style.display = "none";
        EndYMDLabel.style.display = "none";
        EndYMDArea.style.display = "none";
        SelectYMLabel.style.display = "";
        SelectYMArea.style.display = "";
        document.getElementById('WF_ORG_LABEL3').className = "WF_TEXT_LEFT requiredMark"
        SelectYMLabel.className = "WF_TEXT_LEFT requiredMark"
    // 使用料明細表
    } else if (ReportId.substring(0, 7) == "PRT0016") {
        OrgArea.style.display = "none";
        DllLeaseOrgArea.style.display = "";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        StYMDLabel.style.display = "none"
        StYMDArea.style.display = "none";
        EndYMDLabel.style.display = "none";
        EndYMDArea.style.display = "none";
        SelectYMLabel.style.display = "";
        SelectYMArea.style.display = "";
        document.getElementById('WF_ORG_LABEL3').className = "WF_TEXT_LEFT requiredMark"
        SelectYMLabel.className = "WF_TEXT_LEFT requiredMark"
        Prt0016Area.style.display = "";
    // 回送運賃報告書
    } else if (ReportId.substring(0, 7) == "PRT0017") {
        OrgSingleOrAllLabel.style.display = "none";
        OrgSingleOrMultipleLabel.style.display = "";
        DllOrgArea.style.display = "none";
        AllOrgArea.style.display = "none";
        SelectOrgArea.style.display = "none";
        SelectMultipleOrgArea.style.display = "";
        StYMDLabel.style.display = "none"
        StYMDArea.style.display = "none";
        EndYMDArea.style.display = "none";
        SelectYMLabel.style.display = "";
        SelectYMArea.style.display = "";
        SelectYMLabel.className = "WF_TEXT_LEFT requiredMark"
        document.getElementById('WF_ORG_LABEL').className = "WF_TEXT_LEFT requiredMark"
        var defaultDate = new Date();
        var defaultYear = defaultDate.getFullYear();
        var defaultMonth = defaultDate.getMonth() + 1;
        document.getElementById('TxtDownloadMonth').value = defaultYear + "/" + ("0" + defaultMonth).slice(-2);

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

/**
 *  年月選択Pickerの表示イベントバインド
 * @return {undefined} なし
 * @description 
 */
function BindMonthPicker() {
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
        targetTextBox.addEventListener('click', (function (targetTextBox) {
            return function () {
                commonDispMonthPicker(targetTextBox);
            };
        })(targetTextBox), true);
    }
}

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