// ドロップダウンリスト変更
function SelectDropDownList_OnChange(ddlIDClick) {
    const ReportFlags1 = document.getElementById('contents1_ReportFlags_1');
    const ReportFlags2 = document.getElementById('contents1_ReportFlags_0');
    const ReportTypeArea = document.getElementById('WF_REPORTTYPE_AREA');
    const OutputPatternArea = document.getElementById('WF_OUTPUTPATTERN_AREA');
    
    //他駅発送明細が選ばれている場合、不要項目を非表示にする
    if (ReportFlags1.checked && ReportFlags2.checked == false) {
        ReportTypeArea.style.display = "none";
        OutputPatternArea.style.display = "none";
    } else {
        ReportTypeArea.style.display = "";
        OutputPatternArea.style.display = "";
    };
};


