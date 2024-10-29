<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LNT0001WRKINC.ascx.vb" Inherits="JOTWEB_LNG.LNT0001WRKINC" %>

<!-- Work レイアウト -->
<div hidden="hidden">
    <!-- ■受注登録用 検索画面 -->
    <!-- コンテナ記号(コード) -->
    <asp:TextBox ID="WF_SEL_CTNTYPECODE" runat="server"></asp:TextBox>
    <!-- コンテナ記号(名) -->
    <asp:TextBox ID="WF_SEL_CTNTYPENAME" runat="server"></asp:TextBox>
    <!-- コンテナ番号(コード) -->
    <asp:TextBox ID="WF_SEL_CTNNOCODE" runat="server"></asp:TextBox>
    <!-- コンテナ番号(名) -->
    <asp:TextBox ID="WF_SEL_CTNNONAME" runat="server"></asp:TextBox>

    <!-- ■受注登録用 登録・更新用 -->
    <!-- 所管部コード-->
    <asp:TextBox ID="WF_UPD_JURISDICTIONCD" runat="server"></asp:TextBox>
    <!-- 経理資産コード -->
    <asp:TextBox ID="WF_UPD_ACCOUNTINGASSETSCD" runat="server"></asp:TextBox>
    <!-- 経理資産区分 -->
    <asp:TextBox ID="WF_UPD_ACCOUNTINGASSETSKBN" runat="server"></asp:TextBox>
    <!-- ダミー区分 -->
    <asp:TextBox ID="WF_UPD_DUMMYKBN" runat="server"></asp:TextBox>
    <!-- スポット区分 -->
    <asp:TextBox ID="WF_UPD_SPOTKBN" runat="server"></asp:TextBox>
    <!-- スポット区分　開始年月日 -->
    <asp:TextBox ID="WF_UPD_SPOTSTYMD" runat="server"></asp:TextBox>
    <!-- スポット区分　終了年月日 -->
    <asp:TextBox ID="WF_UPD_SPOTENDYMD" runat="server"></asp:TextBox>
    <!-- 複合一貫区分-->
    <asp:TextBox ID="WF_UPD_COMPKANKBN" runat="server"></asp:TextBox>
    <!-- 運用除外年月日 -->
    <asp:TextBox ID="WF_UPD_OPERATIONENDYMD" runat="server"></asp:TextBox>
    <!-- 除却年月日 -->
    <asp:TextBox ID="WF_UPD_RETIRMENTYMD" runat="server"></asp:TextBox>

    <!-- ■受注検索用 -->
    <!-- 会社コード -->
    <asp:TextBox ID="WF_SEL_CAMPCODE" runat="server"></asp:TextBox>
    <!-- 運用部署 -->
    <asp:TextBox ID="WF_SEL_UORG" runat="server"></asp:TextBox>
    <!-- 所管部コード(検索退避用) -->
    <asp:TextBox ID="WF_SEL_JURISDICTIONCDMAP" runat="server"></asp:TextBox>
    <!-- 所管部コード -->
    <asp:TextBox ID="WF_SEL_JURISDICTIONCD" runat="server"></asp:TextBox>
    <!-- 所管部名 -->
    <asp:TextBox ID="WF_SEL_JURISDICTIONNM" runat="server"></asp:TextBox>
    <!-- 年月日 -->
    <asp:TextBox ID="WF_SEL_DATE" runat="server"></asp:TextBox>
    <!-- 年月日 -->
    <asp:TextBox ID="WF_SEL_DATE_TO" runat="server"></asp:TextBox>
    <!-- JOT発店所コード(検索退避用) -->
    <asp:TextBox ID="WF_SEL_JOTDEPBRANCHCDMAP" runat="server"></asp:TextBox>
    <!-- JOT発店所コード -->
    <asp:TextBox ID="WF_SEL_JOTDEPBRANCHCD" runat="server"></asp:TextBox>
    <!-- JOT発店所名 -->
    <asp:TextBox ID="WF_SEL_JOTDEPBRANCHNM" runat="server"></asp:TextBox>
    <!-- 積空区分コード -->
    <asp:TextBox ID="WF_SEL_STACKFREEKBNCD" runat="server"></asp:TextBox>
    <!-- 積空区分名 -->
    <asp:TextBox ID="WF_SEL_STACKFREEKBNNM" runat="server"></asp:TextBox>
    <!-- 発駅コード(検索退避用) -->
    <asp:TextBox ID="WF_SEL_DEPSTATIONMAP" runat="server"></asp:TextBox>
    <!-- 発駅コード -->
    <asp:TextBox ID="WF_SEL_DEPSTATION" runat="server"></asp:TextBox>
    <!-- 発駅名 -->
    <asp:TextBox ID="WF_SEL_DEPSTATIONNM" runat="server"></asp:TextBox>
    <!-- 原発駅コード(検索退避用) -->
    <asp:TextBox ID="WF_SEL_RAWDEPSTATIONMAP" runat="server"></asp:TextBox>
    <!-- 原発駅コード -->
    <asp:TextBox ID="WF_SEL_RAWDEPSTATION" runat="server"></asp:TextBox>
    <!-- 原発駅名 -->
    <asp:TextBox ID="WF_SEL_RAWDEPSTATIONNM" runat="server"></asp:TextBox>
    <!-- 発受託人コード(検索退避用) -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEECDMAP" runat="server"></asp:TextBox>
    <!-- 発受託人コード -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEECD" runat="server"></asp:TextBox>
    <!-- 発受託人名 -->
    <asp:TextBox ID="WF_SEL_DEPTRUSTEENM" runat="server"></asp:TextBox>
    <!-- コンテナ記号 -->
    <asp:TextBox ID="WF_SEL_CTNTYPE" runat="server"></asp:TextBox>
    <!-- コンテナ番号 -->
    <asp:TextBox ID="WF_SEL_CTNNO" runat="server"></asp:TextBox>
    <!-- 状態コード -->
    <asp:TextBox ID="WF_SEL_STATUSCODE" runat="server"></asp:TextBox>
    <!-- 状態名 -->
    <asp:TextBox ID="WF_SEL_STATUS" runat="server"></asp:TextBox>
    <!-- 受注キャンセルフラグ -->
    <asp:TextBox ID="WF_SEL_ORDERCANCELFLG" runat="server"></asp:TextBox>
    <!-- 対象外フラグ -->
    <asp:TextBox ID="WF_SEL_NOTSELFLG" runat="server"></asp:TextBox>

    <!-- ■受注一覧用 -->
    <!-- 選択行 -->
    <asp:TextBox ID="WF_SELROW_LINECNT" runat="server"></asp:TextBox>
    <!-- 受注№ -->
    <asp:TextBox ID="WF_SELROW_ORDERNO" runat="server"></asp:TextBox>
    <!-- 受注明細№ -->
    <asp:TextBox ID="WF_SELROW_DETAILNO" runat="server"></asp:TextBox>
    <!-- 受注状態 -->
    <asp:TextBox ID="WF_SELROW_ORDERSTATUS" runat="server"></asp:TextBox>
    <!-- 受注状態名 -->
    <asp:TextBox ID="WF_SELROW_ORDERSTATUSNM" runat="server"></asp:TextBox>
    <!-- 品目コード -->
    <asp:TextBox ID="WF_SELROW_ITEMCD" runat="server"></asp:TextBox>
    <!-- 品目名 -->
    <asp:TextBox ID="WF_SELROW_ITEMNM" runat="server"></asp:TextBox>
    <!-- 鉄道発駅コード -->
    <asp:TextBox ID="WF_SELROW_RAILDEPSTATION" runat="server"></asp:TextBox>
    <!-- 鉄道発駅名 -->
    <asp:TextBox ID="WF_SELROW_RAILDEPSTATIONNM" runat="server"></asp:TextBox>
    <!-- 鉄道着駅コード -->
    <asp:TextBox ID="WF_SELROW_RAILARRSTATION" runat="server"></asp:TextBox>
    <!-- 鉄道着駅名 -->
    <asp:TextBox ID="WF_SELROW_RAILARRSTATIONNM" runat="server"></asp:TextBox>
    <!-- 原発駅コード -->
    <asp:TextBox ID="WF_SELROW_RAWDEPSTATION" runat="server"></asp:TextBox>
    <!-- 原発駅名 -->
    <asp:TextBox ID="WF_SELROW_RAWDEPSTATIONNM" runat="server"></asp:TextBox>
    <!-- 原着駅コード -->
    <asp:TextBox ID="WF_SELROW_RAWARRSTATION" runat="server"></asp:TextBox>
    <!-- 原着駅名 -->
    <asp:TextBox ID="WF_SELROW_RAWARRSTATIONNM" runat="server"></asp:TextBox>
    <!-- 発受託人コード -->
    <asp:TextBox ID="WF_SELROW_DEPTRUSTEECD" runat="server"></asp:TextBox>
    <!-- 発受託人 -->
    <asp:TextBox ID="WF_SELROW_DEPTRUSTEENM" runat="server"></asp:TextBox>
    <!-- 発集配業者コード -->
    <asp:TextBox ID="WF_SELROW_DEPPICKDELTRADERCD" runat="server"></asp:TextBox>
    <!-- 発集配業者 -->
    <asp:TextBox ID="WF_SELROW_DEPPICKDELTRADERNM" runat="server"></asp:TextBox>
    <!-- 着受託人コード -->
    <asp:TextBox ID="WF_SELROW_ARRTRUSTEECD" runat="server"></asp:TextBox>
    <!-- 着受託人 -->
    <asp:TextBox ID="WF_SELROW_ARRTRUSTEENM" runat="server"></asp:TextBox>
    <!-- 着集配業者コード -->
    <asp:TextBox ID="WF_SELROW_ARRPICKDELTRADERCD" runat="server"></asp:TextBox>
    <!-- 着集配業者 -->
    <asp:TextBox ID="WF_SELROW_ARRPICKDELTRADERNM" runat="server"></asp:TextBox>
    <!-- 発列車番号 -->
    <asp:TextBox ID="WF_SELROW_DEPTRAINNO" runat="server"></asp:TextBox>
    <!-- 着列車番号 -->
    <asp:TextBox ID="WF_SELROW_ARRTRAINNO" runat="server"></asp:TextBox>
    <!-- 到着予定日 -->
    <asp:TextBox ID="WF_SELROW_PLANARRYMD" runat="server"></asp:TextBox>
    <!-- 到着実績日 -->
    <asp:TextBox ID="WF_SELROW_RESULTARRYMD" runat="server"></asp:TextBox>
    <!-- 積空区分コード -->
    <asp:TextBox ID="WF_SELROW_STACKFREEKBNCD" runat="server"></asp:TextBox>
    <!-- 積空区分名 -->
    <asp:TextBox ID="WF_SELROW_STACKFREEKBNNM" runat="server"></asp:TextBox>
    <!-- 荷送人コード -->
    <asp:TextBox ID="WF_SELROW_SHIPPERCD" runat="server"></asp:TextBox>
    <!-- 荷送人 -->
    <asp:TextBox ID="WF_SELROW_SHIPPERNM" runat="server"></asp:TextBox>
    <!-- 集荷先電話番号 -->
    <asp:TextBox ID="WF_SELROW_SLCPICKUPTEL" runat="server"></asp:TextBox>
    <!-- その他料金 -->
    <asp:TextBox ID="WF_SELROW_OTHERFEE" runat="server"></asp:TextBox>
    <!-- 削除フラグ -->
    <asp:TextBox ID="WF_SELROW_DELFLG" runat="server"></asp:TextBox>

    <!-- ■共通 -->
    <!-- 作成フラグ -->
    <asp:TextBox ID="WF_SEL_CREATEFLG" runat="server"></asp:TextBox>
    <!-- 更新データ(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTBL" runat="server"></asp:TextBox>

    <!-- 明細画面(タブ１)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB1TBL" runat="server"></asp:TextBox>
    <!-- 明細画面(タブ２)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB2TBL" runat="server"></asp:TextBox>
    <!-- 明細画面(タブ３)(退避用) -->
    <asp:TextBox ID="WF_SEL_INPTAB3TBL" runat="server"></asp:TextBox>

    <!-- MAPID退避(受注明細画面への遷移制御のため) -->
    <asp:TextBox ID="WF_SEL_MAPIDBACKUP" runat="server"></asp:TextBox>

</div>