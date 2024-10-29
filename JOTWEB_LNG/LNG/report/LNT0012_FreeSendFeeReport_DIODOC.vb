Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 回送運賃報告書作成クラス
''' </summary>
Public Class LNT0012_FreeSendFeeReport_DIODOC

#Region "データ種別"
    ''' <summary>
    ''' データ種別(実績)
    ''' </summary>
    Private Const CONST_DATATYPE_ACHIEVEMENTS As String = "ACHIEVEMENTS"
    ''' <summary>
    ''' データ種別(予算)
    ''' </summary>
    Private Const CONST_DATATYPE_BUDGET As String = "BUDGET"
#End Region

#Region "大分類コード"
    ''' <summary>
    ''' 大分類コード(冷蔵)
    ''' </summary>
    Private Const CONST_BIGCTNTYPE_RATED As String = "10"
    ''' <summary>
    ''' 大分類コード(ｽｰﾊﾟｰＵＲ)
    ''' </summary>
    Private Const CONST_BIGCTNTYPE_SUR As String = "11"
    ''' <summary>
    ''' 大分類コード(冷凍)
    ''' </summary>
    Private Const CONST_BIGCTNTYPE_RATION As String = "15"
    ''' <summary>
    ''' 大分類コード(L10t)
    ''' </summary>
    Private Const CONST_BIGCTNTYPE_L10T As String = "20"
    ''' <summary>
    ''' 大分類コード(無蓋)
    ''' </summary>
    Private Const CONST_BIGCTNTYPE_NOLID As String = "35"
#End Region

#Region "勘定科目用状態区分"
    ''' <summary>
    ''' 勘定科目用状態区分(修繕)
    ''' </summary>
    Private Const CONST_ACCOUNTSTATUSKBN_REPAIR As String = "4"
    ''' <summary>
    ''' 勘定科目用状態区分(売却)
    ''' </summary>
    Private Const CONST_ACCOUNTSTATUSKBN_SALES As String = "9"
#End Region

#Region "地区(管内・管外)"
    ''' <summary>
    ''' 地区(管内)
    ''' </summary>
    Private Const CONST_AREA_PIPE As String = "0"
    ''' <summary>
    ''' 地区(管外)
    ''' </summary>
    Private Const CONST_AREA_SURGERY As String = "1"
#End Region


#Region "帳票出力位置設定"
    ''' <summary>
    ''' 年月度
    ''' </summary>
    Private Const CONST_REPORT_YEARMONTH As String = "A3"
    ''' <summary>
    ''' 支店名
    ''' </summary>
    Private Const CONST_REPORT_BRANCHNAME As String = "P4"


#Region "当月分実績-冷蔵"
    ''' <summary>
    ''' 当月分実績-冷蔵-管内-個数
    ''' </summary>
    Private Const CONST_CURACH_RATED_PIPE_QUANTITY As String = "C8"
    ''' <summary>
    ''' 当月分実績-冷蔵-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_CURACH_RATED_PIPE_RAILFARE As String = "D8"
    ''' <summary>
    ''' 当月分実績-冷蔵-管内-発送料
    ''' </summary>
    Private Const CONST_CURACH_RATED_PIPE_SHIPFEE As String = "E8"

    ''' <summary>
    ''' 当月分実績-冷蔵-管外-個数
    ''' </summary>
    Private Const CONST_CURACH_RATED_SURGERY_QUANTITY As String = "C9"
    ''' <summary>
    ''' 当月分実績-冷蔵-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_CURACH_RATED_SURGERY_RAILFARE As String = "D9"
    ''' <summary>
    ''' 当月分実績-冷蔵-管外-発送料
    ''' </summary>
    Private Const CONST_CURACH_RATED_SURGERY_SHIPFEE As String = "E9"
#End Region
#Region "当月分実績-冷凍"
    ''' <summary>
    ''' 当月分実績-冷凍-管内-個数
    ''' </summary>
    Private Const CONST_CURACH_RATION_PIPE_QUANTITY As String = "C12"
    ''' <summary>
    ''' 当月分実績-冷凍-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_CURACH_RATION_PIPE_RAILFARE As String = "D12"
    ''' <summary>
    ''' 当月分実績-冷凍-管内-発送料
    ''' </summary>
    Private Const CONST_CURACH_RATION_PIPE_SHIPFEE As String = "E12"

    ''' <summary>
    ''' 当月分実績-冷凍-管外-個数
    ''' </summary>
    Private Const CONST_CURACH_RATION_SURGERY_QUANTITY As String = "C13"
    ''' <summary>
    ''' 当月分実績-冷凍-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_CURACH_RATION_SURGERY_RAILFARE As String = "D13"
    ''' <summary>
    ''' 当月分実績-冷凍-管外-発送料
    ''' </summary>
    Private Const CONST_CURACH_RATION_SURGERY_SHIPFEE As String = "E13"
#End Region
#Region "当月分実績-SUR"
    ''' <summary>
    ''' 当月分実績-SUR-管内-個数
    ''' </summary>
    Private Const CONST_CURACH_SUR_PIPE_QUANTITY As String = "C16"
    ''' <summary>
    ''' 当月分実績-SUR-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_CURACH_SUR_PIPE_RAILFARE As String = "D16"
    ''' <summary>
    ''' 当月分実績-SUR-管内-発送料
    ''' </summary>
    Private Const CONST_CURACH_SUR_PIPE_SHIPFEE As String = "E16"

    ''' <summary>
    ''' 当月分実績-SUR-管外-個数
    ''' </summary>
    Private Const CONST_CURACH_SUR_SURGERY_QUANTITY As String = "C17"
    ''' <summary>
    ''' 当月分実績-SUR-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_CURACH_SUR_SURGERY_RAILFARE As String = "D17"
    ''' <summary>
    ''' 当月分実績-SUR-管外-発送料
    ''' </summary>
    Private Const CONST_CURACH_SUR_SURGERY_SHIPFEE As String = "E17"
#End Region
#Region "当月分実績-L10t"
    ''' <summary>
    ''' 当月分実績-L10t-管内-個数
    ''' </summary>
    Private Const CONST_CURACH_L10T_PIPE_QUANTITY As String = "C20"
    ''' <summary>
    ''' 当月分実績-L10t-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_CURACH_L10T_PIPE_RAILFARE As String = "D20"
    ''' <summary>
    ''' 当月分実績-L10t-管内-発送料
    ''' </summary>
    Private Const CONST_CURACH_L10T_PIPE_SHIPFEE As String = "E20"

    ''' <summary>
    ''' 当月分実績-L10t-管外-個数
    ''' </summary>
    Private Const CONST_CURACH_L10T_SURGERY_QUANTITY As String = "C21"
    ''' <summary>
    ''' 当月分実績-L10t-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_CURACH_L10T_SURGERY_RAILFARE As String = "D21"
    ''' <summary>
    ''' 当月分実績-L10t-管外-発送料
    ''' </summary>
    Private Const CONST_CURACH_L10T_SURGERY_SHIPFEE As String = "E21"
#End Region
#Region "当月分実績-無蓋"
    ''' <summary>
    ''' 当月分実績-無蓋-管内-個数
    ''' </summary>
    Private Const CONST_CURACH_NOLID_PIPE_QUANTITY As String = "C24"
    ''' <summary>
    ''' 当月分実績-無蓋-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_CURACH_NOLID_PIPE_RAILFARE As String = "D24"
    ''' <summary>
    ''' 当月分実績-無蓋-管内-発送料
    ''' </summary>
    Private Const CONST_CURACH_NOLID_PIPE_SHIPFEE As String = "E24"

    ''' <summary>
    ''' 当月分実績-無蓋-管外-個数
    ''' </summary>
    Private Const CONST_CURACH_NOLID_SURGERY_QUANTITY As String = "C25"
    ''' <summary>
    ''' 当月分実績-無蓋-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_CURACH_NOLID_SURGERY_RAILFARE As String = "D25"
    ''' <summary>
    ''' 当月分実績-無蓋-管外-発送料
    ''' </summary>
    Private Const CONST_CURACH_NOLID_SURGERY_SHIPFEE As String = "E25"
#End Region

#Region "修理時回送・売却時回送-冷蔵"
    ''' <summary>
    ''' 修理時回送・売却時回送-冷蔵-管内-個数
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATED_PIPE_QUANTITY As String = "H8"
    ''' <summary>
    ''' 修理時回送・売却時回送-冷蔵-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATED_PIPE_RAILFARE As String = "I8"
    ''' <summary>
    ''' 修理時回送・売却時回送-冷蔵-管内-発送料
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATED_PIPE_SHIPFEE As String = "J8"

    ''' <summary>
    ''' 修理時回送・売却時回送-冷蔵-管外-個数
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATED_SURGERY_QUANTITY As String = "H9"
    ''' <summary>
    ''' 修理時回送・売却時回送-冷蔵-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATED_SURGERY_RAILFARE As String = "I9"
    ''' <summary>
    ''' 修理時回送・売却時回送-冷蔵-管外-発送料
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATED_SURGERY_SHIPFEE As String = "J9"
#End Region
#Region "修理時回送・売却時回送-冷凍"
    ''' <summary>
    ''' 修理時回送・売却時回送-冷凍-管内-個数
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATION_PIPE_QUANTITY As String = "H12"
    ''' <summary>
    ''' 修理時回送・売却時回送-冷凍-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATION_PIPE_RAILFARE As String = "I12"
    ''' <summary>
    ''' 修理時回送・売却時回送-冷凍-管内-発送料
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATION_PIPE_SHIPFEE As String = "J12"

    ''' <summary>
    ''' 修理時回送・売却時回送-冷凍-管外-個数
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATION_SURGERY_QUANTITY As String = "H13"
    ''' <summary>
    ''' 修理時回送・売却時回送-冷凍-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATION_SURGERY_RAILFARE As String = "I13"
    ''' <summary>
    ''' 修理時回送・売却時回送-冷凍-管外-発送料
    ''' </summary>
    Private Const CONST_REPAIRSALES_RATION_SURGERY_SHIPFEE As String = "J13"
#End Region
#Region "修理時回送・売却時回送-SUR"
    ''' <summary>
    ''' 修理時回送・売却時回送-SUR-管内-個数
    ''' </summary>
    Private Const CONST_REPAIRSALES_SUR_PIPE_QUANTITY As String = "H16"
    ''' <summary>
    ''' 修理時回送・売却時回送-SUR-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_REPAIRSALES_SUR_PIPE_RAILFARE As String = "I16"
    ''' <summary>
    ''' 修理時回送・売却時回送-SUR-管内-発送料
    ''' </summary>
    Private Const CONST_REPAIRSALES_SUR_PIPE_SHIPFEE As String = "J16"

    ''' <summary>
    ''' 修理時回送・売却時回送-SUR-管外-個数
    ''' </summary>
    Private Const CONST_REPAIRSALES_SUR_SURGERY_QUANTITY As String = "H17"
    ''' <summary>
    ''' 修理時回送・売却時回送-SUR-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_REPAIRSALES_SUR_SURGERY_RAILFARE As String = "I17"
    ''' <summary>
    ''' 修理時回送・売却時回送-SUR-管外-発送料
    ''' </summary>
    Private Const CONST_REPAIRSALES_SUR_SURGERY_SHIPFEE As String = "J17"
#End Region
#Region "修理時回送・売却時回送-L10t"
    ''' <summary>
    ''' 修理時回送・売却時回送-L10t-管内-個数
    ''' </summary>
    Private Const CONST_REPAIRSALES_L10T_PIPE_QUANTITY As String = "H20"
    ''' <summary>
    ''' 修理時回送・売却時回送-L10t-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_REPAIRSALES_L10T_PIPE_RAILFARE As String = "I20"
    ''' <summary>
    ''' 修理時回送・売却時回送-L10t-管内-発送料
    ''' </summary>
    Private Const CONST_REPAIRSALES_L10T_PIPE_SHIPFEE As String = "J20"

    ''' <summary>
    ''' 修理時回送・売却時回送-L10t-管外-個数
    ''' </summary>
    Private Const CONST_REPAIRSALES_L10T_SURGERY_QUANTITY As String = "H21"
    ''' <summary>
    ''' 修理時回送・売却時回送-L10t-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_REPAIRSALES_L10T_SURGERY_RAILFARE As String = "I21"
    ''' <summary>
    ''' 修理時回送・売却時回送-L10t-管外-発送料
    ''' </summary>
    Private Const CONST_REPAIRSALES_L10T_SURGERY_SHIPFEE As String = "J21"
#End Region
#Region "修理時回送・売却時回送-無蓋"
    ''' <summary>
    ''' 修理時回送・売却時回送-無蓋-管内-個数
    ''' </summary>
    Private Const CONST_REPAIRSALES_NOLID_PIPE_QUANTITY As String = "H24"
    ''' <summary>
    ''' 修理時回送・売却時回送-無蓋-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_REPAIRSALES_NOLID_PIPE_RAILFARE As String = "I24"
    ''' <summary>
    ''' 修理時回送・売却時回送-無蓋-管内-発送料
    ''' </summary>
    Private Const CONST_REPAIRSALES_NOLID_PIPE_SHIPFEE As String = "J24"

    ''' <summary>
    ''' 修理時回送・売却時回送-無蓋-管外-個数
    ''' </summary>
    Private Const CONST_REPAIRSALES_NOLID_SURGERY_QUANTITY As String = "H25"
    ''' <summary>
    ''' 修理時回送・売却時回送-無蓋-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_REPAIRSALES_NOLID_SURGERY_RAILFARE As String = "I25"
    ''' <summary>
    ''' 修理時回送・売却時回送-無蓋-管外-発送料
    ''' </summary>
    Private Const CONST_REPAIRSALES_NOLID_SURGERY_SHIPFEE As String = "J25"
#End Region

#Region "当初予算-冷蔵"
    ''' <summary>
    ''' 当初予算-冷蔵-個数
    ''' </summary>
    Private Const CONST_INITIALLYBUDGET_RATED_QUANTITY As String = "D35"
    ''' <summary>
    ''' 当初予算-冷蔵-鉄道運賃
    ''' </summary>
    Private Const CONST_INITIALLYBUDGET_RATED_RAILFARE As String = "E35"
    '''' <summary>
    '''' 当初予算-冷蔵-発送料
    '''' </summary>
    Private Const CONST_INITIALLYBUDGET_RATED_SHIPFEE As String = "F35"
#End Region
#Region "当初予算-冷凍"
    ''' <summary>
    ''' 当初予算-冷凍-個数
    ''' </summary>
    Private Const CONST_INITIALLYBUDGET_RATION_QUANTITY As String = "D39"
    ''' <summary>
    ''' 当初予算-冷凍-鉄道運賃
    ''' </summary>
    Private Const CONST_INITIALLYBUDGET_RATION_RAILFARE As String = "E39"
    '''' <summary>
    '''' 当初予算-冷凍-発送料
    '''' </summary>
    Private Const CONST_INITIALLYBUDGET_RATION_SHIPFEE As String = "F39"
#End Region
#Region "当初予算-SUR"
    ''' <summary>
    ''' 当初予算-SUR-個数
    ''' </summary>
    Private Const CONST_INITIALLYBUDGET_SUR_QUANTITY As String = "D43"
    ''' <summary>
    ''' 当初予算-SUR-鉄道運賃
    ''' </summary>
    Private Const CONST_INITIALLYBUDGET_SUR_RAILFARE As String = "E43"
    '''' <summary>
    '''' 当初予算-SUR-発送料
    '''' </summary>
    Private Const CONST_INITIALLYBUDGET_SUR_SHIPFEE As String = "F43"
#End Region

#Region "内訳明細-冷蔵"
    ''' <summary>
    ''' 内訳明細-冷蔵-表示範囲
    ''' </summary>
    Private Const CONST_DETAIL_RATED_RANGE As String = "M33:Q40"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷蔵-管内-表示範囲
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATED_PIPE_RANGE As String = "M35:Q35"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷蔵-管外-表示範囲
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATED_SURGERY_RANGE As String = "M36:Q36"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷蔵-管内-表示範囲
    ''' </summary>
    Private Const CONST_DETAILSALES_RATED_PIPE_RANGE As String = "M37:Q37"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷蔵-管外-表示範囲
    ''' </summary>
    Private Const CONST_DETAILSALES_RATED_SURGERY_RANGE As String = "M38:Q38"

    ''' <summary>
    ''' 内訳明細(修理時回送)-冷蔵-管内-個数
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATED_PIPE_QUANTITY As String = "O35"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷蔵-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATED_PIPE_RAILFARE As String = "P35"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷蔵-管内-発送料
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATED_PIPE_SHIPFEE As String = "Q35"

    ''' <summary>
    ''' 内訳明細(修理時回送)-冷蔵-管外-個数
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATED_SURGERY_QUANTITY As String = "O36"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷蔵-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATED_SURGERY_RAILFARE As String = "P36"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷蔵-管外-発送料
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATED_SURGERY_SHIPFEE As String = "Q36"

    ''' <summary>
    ''' 内訳明細(売却時回送)-冷蔵-管内-個数
    ''' </summary>
    Private Const CONST_DETAILSALES_RATED_PIPE_QUANTITY As String = "O37"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷蔵-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILSALES_RATED_PIPE_RAILFARE As String = "P37"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷蔵-管内-発送料
    ''' </summary>
    Private Const CONST_DETAILSALES_RATED_PIPE_SHIPFEE As String = "Q37"

    ''' <summary>
    ''' 内訳明細(売却時回送)-冷蔵-管外-個数
    ''' </summary>
    Private Const CONST_DETAILSALES_RATED_SURGERY_QUANTITY As String = "O38"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷蔵-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILSALES_RATED_SURGERY_RAILFARE As String = "P38"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷蔵-管外-発送料
    ''' </summary>
    Private Const CONST_DETAILSALES_RATED_SURGERY_SHIPFEE As String = "Q38"
#End Region
#Region "内訳明細-冷凍"
    ''' <summary>
    ''' 内訳明細-冷凍-表示範囲
    ''' </summary>
    Private Const CONST_DETAIL_RATION_RANGE As String = "M41:Q49"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷凍-管内-表示範囲
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATION_PIPE_RANGE As String = "M44:Q44"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷凍-管外-表示範囲
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATION_SURGERY_RANGE As String = "M45:Q45"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷凍-管内-表示範囲
    ''' </summary>
    Private Const CONST_DETAILSALES_RATION_PIPE_RANGE As String = "M46:Q46"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷凍-管外-表示範囲
    ''' </summary>
    Private Const CONST_DETAILSALES_RATION_SURGERY_RANGE As String = "M47:Q47"

    ''' <summary>
    ''' 内訳明細(修理時回送)-冷凍-管内-個数
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATION_PIPE_QUANTITY As String = "O44"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷凍-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATION_PIPE_RAILFARE As String = "P44"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷凍-管内-発送料
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATION_PIPE_SHIPFEE As String = "Q44"

    ''' <summary>
    ''' 内訳明細(修理時回送)-冷凍-管外-個数
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATION_SURGERY_QUANTITY As String = "O45"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷凍-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATION_SURGERY_RAILFARE As String = "P45"
    ''' <summary>
    ''' 内訳明細(修理時回送)-冷凍-管外-発送料
    ''' </summary>
    Private Const CONST_DETAILREPAIR_RATION_SURGERY_SHIPFEE As String = "Q45"

    ''' <summary>
    ''' 内訳明細(売却時回送)-冷凍-管内-個数
    ''' </summary>
    Private Const CONST_DETAILSALES_RATION_PIPE_QUANTITY As String = "O46"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷凍-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILSALES_RATION_PIPE_RAILFARE As String = "P46"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷凍-管内-発送料
    ''' </summary>
    Private Const CONST_DETAILSALES_RATION_PIPE_SHIPFEE As String = "Q46"

    ''' <summary>
    ''' 内訳明細(売却時回送)-冷凍-管外-個数
    ''' </summary>
    Private Const CONST_DETAILSALES_RATION_SURGERY_QUANTITY As String = "O47"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷凍-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILSALES_RATION_SURGERY_RAILFARE As String = "P47"
    ''' <summary>
    ''' 内訳明細(売却時回送)-冷凍-管外-発送料
    ''' </summary>
    Private Const CONST_DETAILSALES_RATION_SURGERY_SHIPFEE As String = "Q47"
#End Region
#Region "内訳明細-SUR"
    ''' <summary>
    ''' 内訳明細-SUR-表示範囲
    ''' </summary>
    Private Const CONST_DETAIL_SUR_RANGE As String = "M50:Q58"
    ''' <summary>
    ''' 内訳明細(修理時回送)-SUR-管内-表示範囲
    ''' </summary>
    Private Const CONST_DETAILREPAIR_SUR_PIPE_RANGE As String = "M53:Q53"
    ''' <summary>
    ''' 内訳明細(修理時回送)-SUR-管外-表示範囲
    ''' </summary>
    Private Const CONST_DETAILREPAIR_SUR_SURGERY_RANGE As String = "M54:Q54"
    ''' <summary>
    ''' 内訳明細(売却時回送)-SUR-管内-表示範囲
    ''' </summary>
    Private Const CONST_DETAILSALES_SUR_PIPE_RANGE As String = "M55:Q55"
    ''' <summary>
    ''' 内訳明細(売却時回送)-SUR-管外-表示範囲
    ''' </summary>
    Private Const CONST_DETAILSALES_SUR_SURGERY_RANGE As String = "M56:Q56"

    ''' <summary>
    ''' 内訳明細(修理時回送)-SUR-管内-個数
    ''' </summary>
    Private Const CONST_DETAILREPAIR_SUR_PIPE_QUANTITY As String = "O53"
    ''' <summary>
    ''' 内訳明細(修理時回送)-SUR-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILREPAIR_SUR_PIPE_RAILFARE As String = "P53"
    ''' <summary>
    ''' 内訳明細(修理時回送)-SUR-管内-発送料
    ''' </summary>
    Private Const CONST_DETAILREPAIR_SUR_PIPE_SHIPFEE As String = "Q53"

    ''' <summary>
    ''' 内訳明細(修理時回送)-SUR-管外-個数
    ''' </summary>
    Private Const CONST_DETAILREPAIR_SUR_SURGERY_QUANTITY As String = "O54"
    ''' <summary>
    ''' 内訳明細(修理時回送)-SUR-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILREPAIR_SUR_SURGERY_RAILFARE As String = "P54"
    ''' <summary>
    ''' 内訳明細(修理時回送)-SUR-管外-発送料
    ''' </summary>
    Private Const CONST_DETAILREPAIR_SUR_SURGERY_SHIPFEE As String = "Q54"

    ''' <summary>
    ''' 内訳明細(売却時回送)-SUR-管内-個数
    ''' </summary>
    Private Const CONST_DETAILSALES_SUR_PIPE_QUANTITY As String = "O55"
    ''' <summary>
    ''' 内訳明細(売却時回送)-SUR-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILSALES_SUR_PIPE_RAILFARE As String = "P55"
    ''' <summary>
    ''' 内訳明細(売却時回送)-SUR-管内-発送料
    ''' </summary>
    Private Const CONST_DETAILSALES_SUR_PIPE_SHIPFEE As String = "Q55"

    ''' <summary>
    ''' 内訳明細(売却時回送)-SUR-管外-個数
    ''' </summary>
    Private Const CONST_DETAILSALES_SUR_SURGERY_QUANTITY As String = "O56"
    ''' <summary>
    ''' 内訳明細(売却時回送)-SUR-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILSALES_SUR_SURGERY_RAILFARE As String = "P56"
    ''' <summary>
    ''' 内訳明細(売却時回送)-SUR-管外-発送料
    ''' </summary>
    Private Const CONST_DETAILSALES_SUR_SURGERY_SHIPFEE As String = "Q56"
#End Region
#Region "内訳明細-L10t"
    ''' <summary>
    ''' 内訳明細-L10t-表示範囲
    ''' </summary>
    Private Const CONST_DETAIL_L10T_RANGE As String = "M59:Q67"
    ''' <summary>
    ''' 内訳明細(修理時回送)-L10t-管内-表示範囲
    ''' </summary>
    Private Const CONST_DETAILREPAIR_L10T_PIPE_RANGE As String = "M62:Q62"
    ''' <summary>
    ''' 内訳明細(修理時回送)-L10t-管外-表示範囲
    ''' </summary>
    Private Const CONST_DETAILREPAIR_L10T_SURGERY_RANGE As String = "M63:Q63"
    ''' <summary>
    ''' 内訳明細(売却時回送)-L10t-管内-表示範囲
    ''' </summary>
    Private Const CONST_DETAILSALES_L10T_PIPE_RANGE As String = "M64:Q64"
    ''' <summary>
    ''' 内訳明細(売却時回送)-L10t-管外-表示範囲
    ''' </summary>
    Private Const CONST_DETAILSALES_L10T_SURGERY_RANGE As String = "M65:Q65"

    ''' <summary>
    ''' 内訳明細(修理時回送)-L10t-管内-個数
    ''' </summary>
    Private Const CONST_DETAILREPAIR_L10T_PIPE_QUANTITY As String = "O62"
    ''' <summary>
    ''' 内訳明細(修理時回送)-L10t-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILREPAIR_L10T_PIPE_RAILFARE As String = "P62"
    ''' <summary>
    ''' 内訳明細(修理時回送)-L10t-管内-発送料
    ''' </summary>
    Private Const CONST_DETAILREPAIR_L10T_PIPE_SHIPFEE As String = "Q62"

    ''' <summary>
    ''' 内訳明細(修理時回送)-L10t-管外-個数
    ''' </summary>
    Private Const CONST_DETAILREPAIR_L10T_SURGERY_QUANTITY As String = "O63"
    ''' <summary>
    ''' 内訳明細(修理時回送)-L10t-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILREPAIR_L10T_SURGERY_RAILFARE As String = "P63"
    ''' <summary>
    ''' 内訳明細(修理時回送)-L10t-管外-発送料
    ''' </summary>
    Private Const CONST_DETAILREPAIR_L10T_SURGERY_SHIPFEE As String = "Q63"

    ''' <summary>
    ''' 内訳明細(売却時回送)-L10t-管内-個数
    ''' </summary>
    Private Const CONST_DETAILSALES_L10T_PIPE_QUANTITY As String = "O64"
    ''' <summary>
    ''' 内訳明細(売却時回送)-L10t-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILSALES_L10T_PIPE_RAILFARE As String = "P64"
    ''' <summary>
    ''' 内訳明細(売却時回送)-L10t-管内-発送料
    ''' </summary>
    Private Const CONST_DETAILSALES_L10T_PIPE_SHIPFEE As String = "Q64"

    ''' <summary>
    ''' 内訳明細(売却時回送)-L10t-管外-個数
    ''' </summary>
    Private Const CONST_DETAILSALES_L10T_SURGERY_QUANTITY As String = "O65"
    ''' <summary>
    ''' 内訳明細(売却時回送)-L10t-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILSALES_L10T_SURGERY_RAILFARE As String = "P65"
    ''' <summary>
    ''' 内訳明細(売却時回送)-L10t-管外-発送料
    ''' </summary>
    Private Const CONST_DETAILSALES_L10T_SURGERY_SHIPFEE As String = "Q65"
#End Region
#Region "内訳明細-無蓋"
    ''' <summary>
    ''' 内訳明細-無蓋-表示範囲
    ''' </summary>
    Private Const CONST_DETAIL_NOLID_RANGE As String = "M68:Q76"
    ''' <summary>
    ''' 内訳明細(修理時回送)-無蓋-管内-表示範囲
    ''' </summary>
    Private Const CONST_DETAILREPAIR_NOLID_PIPE_RANGE As String = "M71:Q71"
    ''' <summary>
    ''' 内訳明細(修理時回送)-無蓋-管外-表示範囲
    ''' </summary>
    Private Const CONST_DETAILREPAIR_NOLID_SURGERY_RANGE As String = "M72:Q72"
    ''' <summary>
    ''' 内訳明細(売却時回送)-無蓋-管内-表示範囲
    ''' </summary>
    Private Const CONST_DETAILSALES_NOLID_PIPE_RANGE As String = "M73:Q73"
    ''' <summary>
    ''' 内訳明細(売却時回送)-無蓋-管外-表示範囲
    ''' </summary>
    Private Const CONST_DETAILSALES_NOLID_SURGERY_RANGE As String = "M74:Q74"
    ''' <summary>
    ''' 内訳明細(修理時回送)-無蓋-管内-個数
    ''' </summary>
    Private Const CONST_DETAILREPAIR_NOLID_PIPE_QUANTITY As String = "O35"
    ''' <summary>
    ''' 内訳明細(修理時回送)-無蓋-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILREPAIR_NOLID_PIPE_RAILFARE As String = "P35"
    ''' <summary>
    ''' 内訳明細(修理時回送)-無蓋-管内-発送料
    ''' </summary>
    Private Const CONST_DETAILREPAIR_NOLID_PIPE_SHIPFEE As String = "Q35"

    ''' <summary>
    ''' 内訳明細(修理時回送)-無蓋-管外-個数
    ''' </summary>
    Private Const CONST_DETAILREPAIR_NOLID_SURGERY_QUANTITY As String = "O36"
    ''' <summary>
    ''' 内訳明細(修理時回送)-無蓋-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILREPAIR_NOLID_SURGERY_RAILFARE As String = "P36"
    ''' <summary>
    ''' 内訳明細(修理時回送)-無蓋-管外-発送料
    ''' </summary>
    Private Const CONST_DETAILREPAIR_NOLID_SURGERY_SHIPFEE As String = "Q36"

    ''' <summary>
    ''' 内訳明細(売却時回送)-無蓋-管内-個数
    ''' </summary>
    Private Const CONST_DETAILSALES_NOLID_PIPE_QUANTITY As String = "O37"
    ''' <summary>
    ''' 内訳明細(売却時回送)-無蓋-管内-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILSALES_NOLID_PIPE_RAILFARE As String = "P37"
    ''' <summary>
    ''' 内訳明細(売却時回送)-無蓋-管内-発送料
    ''' </summary>
    Private Const CONST_DETAILSALES_NOLID_PIPE_SHIPFEE As String = "Q37"

    ''' <summary>
    ''' 内訳明細(売却時回送)-無蓋-管外-個数
    ''' </summary>
    Private Const CONST_DETAILSALES_NOLID_SURGERY_QUANTITY As String = "O38"
    ''' <summary>
    ''' 内訳明細(売却時回送)-無蓋-管外-鉄道運賃
    ''' </summary>
    Private Const CONST_DETAILSALES_NOLID_SURGERY_RAILFARE As String = "P38"
    ''' <summary>
    ''' 内訳明細(売却時回送)-無蓋-管外-発送料
    ''' </summary>
    Private Const CONST_DETAILSALES_NOLID_SURGERY_SHIPFEE As String = "Q38"
#End Region



#End Region

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private PrintTargetYm As String


    Private WW_Workbook As New Workbook
    Private WW_SheetNo As Integer = 0
    Private WW_tmpSheetNo As Integer = 0
    Private WW_InsDate As Date
    Private WW_CampCode As String = ""
    Private WW_KeyYMD As String = ""


    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable, TargetYm As String)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.PrintTargetYm = TargetYm
            Me.ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                      "PRINTFORMAT",
                                                      C_DEFAULT_DATAKEY,
                                                      mapId, excelFileName)
            Me.UploadRootPath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                   "PRINTWORK",
                                                   CS0050SESSION.USERID)
            'ディレクトリが存在しない場合は生成
            If IO.Directory.Exists(Me.UploadRootPath) = False Then
                IO.Directory.CreateDirectory(Me.UploadRootPath)
            End If
            '前日プリフィックスのアップロードファイルが残っていた場合は削除
            Dim targetFiles = IO.Directory.GetFiles(Me.UploadRootPath, "*.*")
            Dim keepFilePrefix As String = Now.ToString("yyyyMMdd")
            For Each targetFile In targetFiles
                Dim fileName As String = IO.Path.GetFileName(targetFile)
                '今日の日付がファイル名の日付の場合は残す
                If fileName.Contains(keepFilePrefix) Then
                    Continue For
                End If
                Try
                    IO.File.Delete(targetFile)
                Catch ex As Exception
                    '削除時のエラーは無視
                End Try
            Next targetFile
            'URLのルートを表示
            Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

            'ファイルOPEN
            WW_Workbook.Open(Me.ExcelTemplatePath)

        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreateExcelPrintData() As String

        Dim tmpFileName As String = "回送運賃報告書(" &
            Left(PrintTargetYm, 4) & "年" & Right(PrintTargetYm, 2) & "月).xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            '自動計算停止
            WW_Workbook.EnableCalculation = False

            'テンプレートシートに共通部分の年月度を書き込む
            Dim WW_TemplateSheet As IWorksheet = WW_Workbook.Worksheets(0)
            WW_TemplateSheet.Range(CONST_REPORT_YEARMONTH).Value = Left(PrintTargetYm, 4) & "年" & Right(PrintTargetYm, 2) & "月度"

            Dim WW_ORGHT As New Hashtable
            WW_ORGHT.Add("010102", "北海道支店")
            WW_ORGHT.Add("010401", "東北支店")
            WW_ORGHT.Add("011402", "関東支店")
            WW_ORGHT.Add("012401", "中部支店")
            WW_ORGHT.Add("012701", "関西支店")
            WW_ORGHT.Add("014001", "九州支店")

            Dim WW_ORGCODELIST As New ArrayList(WW_ORGHT.Keys)
            WW_ORGCODELIST.Sort()

            'For Each CODE As String In WW_ORGHT.Keys
            For Each CODE As String In WW_ORGCODELIST
                '支店毎のデータ取得
                Dim OrgDataRow As DataRow()
                OrgDataRow = PrintData.Select("JOTDEPBRANCHCD = '" + CODE + "'")
                'データを抽出できた場合
                If Not OrgDataRow.Count = 0 Then
                    Dim WW_TargetORGNAME As String = WW_ORGHT(CODE)
                    'テンプレートシートを複製してデータ出力用のシートを作成
                    Dim WW_CopySheet = WW_TemplateSheet.Copy()
                    WW_CopySheet.Activate()
                    WW_CopySheet.Name = WW_TargetORGNAME
                    '複製したシートに支店名入力
                    WW_CopySheet.Range(CONST_REPORT_BRANCHNAME).Value = WW_TargetORGNAME
                    '取得データ入力
                    SetOrgData(WW_CopySheet, OrgDataRow)
                End If
            Next

            'テンプレートシート削除
            Try
                WW_TemplateSheet.Delete()
            Catch ex As Exception
            End Try

            '全シート一行目削除(コンテナ御中削除)、A1選択
            For Each sheet As IWorksheet In WW_Workbook.Worksheets
                sheet.Rows(0).Delete()
                sheet.Range(0, 0).Select()
            Next

            '先頭シート選択
            WW_Workbook.Worksheets(0).Activate()

            '自動計算開始
            WW_Workbook.EnableCalculation = True

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                WW_Workbook.Save(tmpFilePath, SaveFileFormat.Xlsx)
            End SyncLock

            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try

    End Function

    ''' <summary>
    ''' 取得データ入力
    ''' </summary>
    Private Sub SetOrgData(ByVal sheet As IWorksheet, ByVal WW_Row As DataRow())

#Region "当月分実績-冷蔵"
        ' 当月分実績-冷蔵-管内-個数
        sheet.Range(CONST_CURACH_RATED_PIPE_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当月分実績-冷蔵-管内-鉄道運賃
        sheet.Range(CONST_CURACH_RATED_PIPE_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当月分実績-冷蔵-管内-発送料
        sheet.Range(CONST_CURACH_RATED_PIPE_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 当月分実績-冷蔵-管外-個数
        sheet.Range(CONST_CURACH_RATED_SURGERY_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当月分実績-冷蔵-管外-鉄道運賃
        sheet.Range(CONST_CURACH_RATED_SURGERY_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当月分実績-冷蔵-管外-発送料
        sheet.Range(CONST_CURACH_RATED_SURGERY_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("SHIPFEE")))
#End Region
#Region "当月分実績-冷凍"
        ' 当月分実績-冷凍-管内-個数
        sheet.Range(CONST_CURACH_RATION_PIPE_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当月分実績-冷凍-管内-鉄道運賃
        sheet.Range(CONST_CURACH_RATION_PIPE_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当月分実績-冷凍-管内-発送料
        sheet.Range(CONST_CURACH_RATION_PIPE_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 当月分実績-冷凍-管外-個数
        sheet.Range(CONST_CURACH_RATION_SURGERY_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当月分実績-冷凍-管外-鉄道運賃
        sheet.Range(CONST_CURACH_RATION_SURGERY_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当月分実績-冷凍-管外-発送料
        sheet.Range(CONST_CURACH_RATION_SURGERY_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("SHIPFEE")))
#End Region
#Region "当月分実績-SUR"
        ' 当月分実績-SUR-管内-個数
        sheet.Range(CONST_CURACH_SUR_PIPE_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当月分実績-SUR-管内-鉄道運賃
        sheet.Range(CONST_CURACH_SUR_PIPE_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当月分実績-SUR-管内-発送料
        sheet.Range(CONST_CURACH_SUR_PIPE_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 当月分実績-SUR-管外-個数
        sheet.Range(CONST_CURACH_SUR_SURGERY_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当月分実績-SUR-管外-鉄道運賃
        sheet.Range(CONST_CURACH_SUR_SURGERY_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当月分実績-SUR-管外-発送料
        sheet.Range(CONST_CURACH_SUR_SURGERY_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("SHIPFEE")))
#End Region
#Region "当月分実績-L10t"
        ' 当月分実績-L10t-管内-個数
        sheet.Range(CONST_CURACH_L10T_PIPE_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当月分実績-L10t-管内-鉄道運賃
        sheet.Range(CONST_CURACH_L10T_PIPE_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当月分実績-L10t-管内-発送料
        sheet.Range(CONST_CURACH_L10T_PIPE_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 当月分実績-L10t-管外-個数
        sheet.Range(CONST_CURACH_L10T_SURGERY_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当月分実績-L10t-管外-鉄道運賃
        sheet.Range(CONST_CURACH_L10T_SURGERY_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当月分実績-L10t-管外-発送料
        sheet.Range(CONST_CURACH_L10T_SURGERY_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("SHIPFEE")))
#End Region
#Region "当月分実績-無蓋"
        ' 当月分実績-無蓋-管内-個数
        sheet.Range(CONST_CURACH_NOLID_PIPE_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当月分実績-無蓋-管内-鉄道運賃
        sheet.Range(CONST_CURACH_NOLID_PIPE_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当月分実績-無蓋-管内-発送料
        sheet.Range(CONST_CURACH_NOLID_PIPE_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 当月分実績-無蓋-管外-個数
        sheet.Range(CONST_CURACH_NOLID_SURGERY_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当月分実績-無蓋-管外-鉄道運賃
        sheet.Range(CONST_CURACH_NOLID_SURGERY_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当月分実績-無蓋-管外-発送料
        sheet.Range(CONST_CURACH_NOLID_SURGERY_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Sum(Function(row) CLng(row("SHIPFEE")))
#End Region

        Dim AccountStatusKbn As New List(Of String)
        AccountStatusKbn.Add(CONST_ACCOUNTSTATUSKBN_REPAIR)
        AccountStatusKbn.Add(CONST_ACCOUNTSTATUSKBN_SALES)

#Region "修理時回送・売却時回送-冷蔵"
        ' 修理時回送・売却時回送-冷蔵-管内-個数
        sheet.Range(CONST_REPAIRSALES_RATED_PIPE_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '修理時回送・売却時回送-冷蔵-管内-鉄道運賃
        sheet.Range(CONST_REPAIRSALES_RATED_PIPE_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '修理時回送・売却時回送-冷蔵-管内-発送料
        sheet.Range(CONST_REPAIRSALES_RATED_PIPE_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 修理時回送・売却時回送-冷蔵-管外-個数
        sheet.Range(CONST_REPAIRSALES_RATED_SURGERY_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '修理時回送・売却時回送-冷蔵-管外-鉄道運賃
        sheet.Range(CONST_REPAIRSALES_RATED_SURGERY_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '修理時回送・売却時回送-冷蔵-管外-発送料
        sheet.Range(CONST_REPAIRSALES_RATED_SURGERY_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("SHIPFEE")))
#End Region
#Region "修理時回送・売却時回送-冷凍"
        ' 修理時回送・売却時回送-冷凍-管内-個数
        sheet.Range(CONST_REPAIRSALES_RATION_PIPE_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '修理時回送・売却時回送-冷凍-管内-鉄道運賃
        sheet.Range(CONST_REPAIRSALES_RATION_PIPE_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '修理時回送・売却時回送-冷凍-管内-発送料
        sheet.Range(CONST_REPAIRSALES_RATION_PIPE_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 修理時回送・売却時回送-冷凍-管外-個数
        sheet.Range(CONST_REPAIRSALES_RATION_SURGERY_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '修理時回送・売却時回送-冷凍-管外-鉄道運賃
        sheet.Range(CONST_REPAIRSALES_RATION_SURGERY_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '修理時回送・売却時回送-冷凍-管外-発送料
        sheet.Range(CONST_REPAIRSALES_RATION_SURGERY_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("SHIPFEE")))
#End Region
#Region "修理時回送・売却時回送-SUR"
        ' 修理時回送・売却時回送-SUR-管内-個数
        sheet.Range(CONST_REPAIRSALES_SUR_PIPE_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '修理時回送・売却時回送-SUR-管内-鉄道運賃
        sheet.Range(CONST_REPAIRSALES_SUR_PIPE_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '修理時回送・売却時回送-SUR-管内-発送料
        sheet.Range(CONST_REPAIRSALES_SUR_PIPE_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 修理時回送・売却時回送-SUR-管外-個数
        sheet.Range(CONST_REPAIRSALES_SUR_SURGERY_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '修理時回送・売却時回送-SUR-管外-鉄道運賃
        sheet.Range(CONST_REPAIRSALES_SUR_SURGERY_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '修理時回送・売却時回送-SUR-管外-発送料
        sheet.Range(CONST_REPAIRSALES_SUR_SURGERY_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("SHIPFEE")))
#End Region
#Region "修理時回送・売却時回送-L10t"
        ' 修理時回送・売却時回送-L10t-管内-個数
        sheet.Range(CONST_REPAIRSALES_L10T_PIPE_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '修理時回送・売却時回送-L10t-管内-鉄道運賃
        sheet.Range(CONST_REPAIRSALES_L10T_PIPE_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '修理時回送・売却時回送-L10t-管内-発送料
        sheet.Range(CONST_REPAIRSALES_L10T_PIPE_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 修理時回送・売却時回送-L10t-管外-個数
        sheet.Range(CONST_REPAIRSALES_L10T_SURGERY_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '修理時回送・売却時回送-L10t-管外-鉄道運賃
        sheet.Range(CONST_REPAIRSALES_L10T_SURGERY_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '修理時回送・売却時回送-L10t-管外-発送料
        sheet.Range(CONST_REPAIRSALES_L10T_SURGERY_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("SHIPFEE")))
#End Region
#Region "修理時回送・売却時回送-無蓋"
        ' 修理時回送・売却時回送-無蓋-管内-個数
        sheet.Range(CONST_REPAIRSALES_NOLID_PIPE_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '修理時回送・売却時回送-無蓋-管内-鉄道運賃
        sheet.Range(CONST_REPAIRSALES_NOLID_PIPE_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '修理時回送・売却時回送-無蓋-管内-発送料
        sheet.Range(CONST_REPAIRSALES_NOLID_PIPE_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 修理時回送・売却時回送-無蓋-管外-個数
        sheet.Range(CONST_REPAIRSALES_NOLID_SURGERY_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '修理時回送・売却時回送-無蓋-管外-鉄道運賃
        sheet.Range(CONST_REPAIRSALES_NOLID_SURGERY_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '修理時回送・売却時回送-無蓋-管外-発送料
        sheet.Range(CONST_REPAIRSALES_NOLID_SURGERY_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) AccountStatusKbn.Contains(row("ACCOUNTSTATUSKBN").ToString)) _
        .Sum(Function(row) CLng(row("SHIPFEE")))
#End Region

#Region "当初予算-冷蔵"
        ' 当初予算-冷蔵-管内-個数
        sheet.Range(CONST_INITIALLYBUDGET_RATED_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_BUDGET) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当初予算-冷蔵-管内-鉄道運賃
        sheet.Range(CONST_INITIALLYBUDGET_RATED_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_BUDGET) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当初予算-冷蔵-管内-発送料
        sheet.Range(CONST_INITIALLYBUDGET_RATED_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_BUDGET) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Sum(Function(row) CLng(row("FREEPIPESHIPFEE")))
#End Region
#Region "当初予算-冷凍"
        ' 当初予算-冷凍-管内-個数
        sheet.Range(CONST_INITIALLYBUDGET_RATION_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_BUDGET) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当初予算-冷凍-管内-鉄道運賃
        sheet.Range(CONST_INITIALLYBUDGET_RATION_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_BUDGET) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当初予算-冷凍-管内-発送料
        sheet.Range(CONST_INITIALLYBUDGET_RATION_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_BUDGET) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Sum(Function(row) CLng(row("FREEPIPESHIPFEE")))
#End Region
#Region "当初予算-SUR"
        ' 当初予算-SUR-管内-個数
        sheet.Range(CONST_INITIALLYBUDGET_SUR_QUANTITY).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_BUDGET) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        '当初予算-SUR-管内-鉄道運賃
        sheet.Range(CONST_INITIALLYBUDGET_SUR_RAILFARE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_BUDGET) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        '当初予算-SUR-管内-発送料
        sheet.Range(CONST_INITIALLYBUDGET_SUR_SHIPFEE).Value = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_BUDGET) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Sum(Function(row) CLng(row("FREEPIPESHIPFEE")))
#End Region

        '　※内訳明細は不要項目を一番下の行から順に削除
#Region "内訳明細(修理時回送・売却時回送)-無蓋"
        ' 内訳明細(修理時回送)-無蓋-管内-個数
        Dim WW_DETAILREPAIR_NOLID_PIPE_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(修理時回送)-無蓋-管内-鉄道運賃
        Dim WW_DETAILREPAIR_NOLID_PIPE_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(修理時回送)-無蓋-管内-発送料
        Dim WW_DETAILREPAIR_NOLID_PIPE_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(修理時回送)-無蓋-管外-個数
        Dim WW_DETAILREPAIR_NOLID_SURGERY_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(修理時回送)-無蓋-管外-鉄道運賃
        Dim WW_DETAILREPAIR_NOLID_SURGERY_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(修理時回送)-無蓋-管外-発送料
        Dim WW_DETAILREPAIR_NOLID_SURGERY_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-無蓋-管内-個数
        Dim WW_DETAILSALES_NOLID_PIPE_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(売却時回送)-無蓋-管内-鉄道運賃
        Dim WW_DETAILSALES_NOLID_PIPE_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(売却時回送)-無蓋-管内-発送料
        Dim WW_DETAILSALES_NOLID_PIPE_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-無蓋-管外-個数
        Dim WW_DETAILSALES_NOLID_SURGERY_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(売却時回送)-無蓋-管外-鉄道運賃
        Dim WW_DETAILSALES_NOLID_SURGERY_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(売却時回送)-無蓋-管外-発送料
        Dim WW_DETAILSALES_NOLID_SURGERY_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_NOLID) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        'データがすべて存在しない場合は該当する明細範囲を削除
        If WW_DETAILREPAIR_NOLID_PIPE_QUANTITY = "0" And
            WW_DETAILREPAIR_NOLID_PIPE_RAILFARE = "0" And
            WW_DETAILREPAIR_NOLID_PIPE_SHIPFEE = "0" And
            WW_DETAILREPAIR_NOLID_SURGERY_QUANTITY = "0" And
            WW_DETAILREPAIR_NOLID_SURGERY_RAILFARE = "0" And
            WW_DETAILREPAIR_NOLID_SURGERY_SHIPFEE = "0" And
            WW_DETAILSALES_NOLID_PIPE_QUANTITY = "0" And
            WW_DETAILSALES_NOLID_PIPE_RAILFARE = "0" And
            WW_DETAILSALES_NOLID_PIPE_SHIPFEE = "0" And
            WW_DETAILSALES_NOLID_SURGERY_QUANTITY = "0" And
            WW_DETAILSALES_NOLID_SURGERY_RAILFARE = "0" And
            WW_DETAILSALES_NOLID_SURGERY_SHIPFEE = "0" Then
            sheet.Range(CONST_DETAIL_NOLID_RANGE).Delete(DeleteShiftDirection.Up)
        Else
            'データがない行は削除
            ' 内訳明細(売却時回送)-無蓋-管外
            If WW_DETAILSALES_NOLID_SURGERY_QUANTITY = "0" And
                WW_DETAILSALES_NOLID_SURGERY_RAILFARE = "0" And
                WW_DETAILSALES_NOLID_SURGERY_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILSALES_NOLID_SURGERY_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILSALES_NOLID_SURGERY_QUANTITY).Value = CLng(WW_DETAILSALES_NOLID_SURGERY_QUANTITY)
                sheet.Range(CONST_DETAILSALES_NOLID_SURGERY_RAILFARE).Value = CLng(WW_DETAILSALES_NOLID_SURGERY_RAILFARE)
                sheet.Range(CONST_DETAILSALES_NOLID_SURGERY_SHIPFEE).Value = CLng(WW_DETAILSALES_NOLID_SURGERY_SHIPFEE)
            End If
            ' 内訳明細(売却時回送)-無蓋-管内
            If WW_DETAILSALES_NOLID_PIPE_QUANTITY = "0" And
                WW_DETAILSALES_NOLID_PIPE_RAILFARE = "0" And
                WW_DETAILSALES_NOLID_PIPE_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILSALES_NOLID_PIPE_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILSALES_NOLID_PIPE_QUANTITY).Value = CLng(WW_DETAILSALES_NOLID_PIPE_QUANTITY)
                sheet.Range(CONST_DETAILSALES_NOLID_PIPE_RAILFARE).Value = CLng(WW_DETAILSALES_NOLID_PIPE_RAILFARE)
                sheet.Range(CONST_DETAILSALES_NOLID_PIPE_SHIPFEE).Value = CLng(WW_DETAILSALES_NOLID_PIPE_SHIPFEE)
            End If
            ' 内訳明細(修理時回送)-無蓋-管外
            If WW_DETAILREPAIR_NOLID_SURGERY_QUANTITY = "0" And
                WW_DETAILREPAIR_NOLID_SURGERY_RAILFARE = "0" And
                WW_DETAILREPAIR_NOLID_SURGERY_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILREPAIR_NOLID_SURGERY_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILREPAIR_NOLID_SURGERY_QUANTITY).Value = CLng(WW_DETAILREPAIR_NOLID_SURGERY_QUANTITY)
                sheet.Range(CONST_DETAILREPAIR_NOLID_SURGERY_RAILFARE).Value = CLng(WW_DETAILREPAIR_NOLID_SURGERY_RAILFARE)
                sheet.Range(CONST_DETAILREPAIR_NOLID_SURGERY_SHIPFEE).Value = CLng(WW_DETAILREPAIR_NOLID_SURGERY_SHIPFEE)
            End If

            ' 内訳明細(修理時回送)-無蓋-管内
            If WW_DETAILREPAIR_NOLID_PIPE_QUANTITY = "0" And
                WW_DETAILREPAIR_NOLID_PIPE_RAILFARE = "0" And
                WW_DETAILREPAIR_NOLID_PIPE_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILREPAIR_NOLID_PIPE_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILREPAIR_NOLID_PIPE_QUANTITY).Value = CLng(WW_DETAILREPAIR_NOLID_PIPE_QUANTITY)
                sheet.Range(CONST_DETAILREPAIR_NOLID_PIPE_RAILFARE).Value = CLng(WW_DETAILREPAIR_NOLID_PIPE_RAILFARE)
                sheet.Range(CONST_DETAILREPAIR_NOLID_PIPE_SHIPFEE).Value = CLng(WW_DETAILREPAIR_NOLID_PIPE_SHIPFEE)
            End If
        End If
#End Region
#Region "内訳明細(修理時回送・売却時回送)-L10t"
        ' 内訳明細(修理時回送)-L10t-管内-個数
        Dim WW_DETAILREPAIR_L10T_PIPE_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(修理時回送)-L10t-管内-鉄道運賃
        Dim WW_DETAILREPAIR_L10T_PIPE_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(修理時回送)-L10t-管内-発送料
        Dim WW_DETAILREPAIR_L10T_PIPE_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(修理時回送)-L10t-管外-個数
        Dim WW_DETAILREPAIR_L10T_SURGERY_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(修理時回送)-L10t-管外-鉄道運賃
        Dim WW_DETAILREPAIR_L10T_SURGERY_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(修理時回送)-L10t-管外-発送料
        Dim WW_DETAILREPAIR_L10T_SURGERY_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-L10t-管内-個数
        Dim WW_DETAILSALES_L10T_PIPE_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(売却時回送)-L10t-管内-鉄道運賃
        Dim WW_DETAILSALES_L10T_PIPE_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(売却時回送)-L10t-管内-発送料
        Dim WW_DETAILSALES_L10T_PIPE_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-L10t-管外-個数
        Dim WW_DETAILSALES_L10T_SURGERY_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(売却時回送)-L10t-管外-鉄道運賃
        Dim WW_DETAILSALES_L10T_SURGERY_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(売却時回送)-L10t-管外-発送料
        Dim WW_DETAILSALES_L10T_SURGERY_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_L10T) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        'データがすべて存在しない場合は該当する明細範囲を削除
        If WW_DETAILREPAIR_L10T_PIPE_QUANTITY = "0" And
            WW_DETAILREPAIR_L10T_PIPE_RAILFARE = "0" And
            WW_DETAILREPAIR_L10T_PIPE_SHIPFEE = "0" And
            WW_DETAILREPAIR_L10T_SURGERY_QUANTITY = "0" And
            WW_DETAILREPAIR_L10T_SURGERY_RAILFARE = "0" And
            WW_DETAILREPAIR_L10T_SURGERY_SHIPFEE = "0" And
            WW_DETAILSALES_L10T_PIPE_QUANTITY = "0" And
            WW_DETAILSALES_L10T_PIPE_RAILFARE = "0" And
            WW_DETAILSALES_L10T_PIPE_SHIPFEE = "0" And
            WW_DETAILSALES_L10T_SURGERY_QUANTITY = "0" And
            WW_DETAILSALES_L10T_SURGERY_RAILFARE = "0" And
            WW_DETAILSALES_L10T_SURGERY_SHIPFEE = "0" Then
            sheet.Range(CONST_DETAIL_L10T_RANGE).Delete(DeleteShiftDirection.Up)
        Else
            'データがない行は削除
            ' 内訳明細(売却時回送)-L10t-管外
            If WW_DETAILSALES_L10T_SURGERY_QUANTITY = "0" And
                WW_DETAILSALES_L10T_SURGERY_RAILFARE = "0" And
                WW_DETAILSALES_L10T_SURGERY_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILSALES_L10T_SURGERY_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILSALES_L10T_SURGERY_QUANTITY).Value = CLng(WW_DETAILSALES_L10T_SURGERY_QUANTITY)
                sheet.Range(CONST_DETAILSALES_L10T_SURGERY_RAILFARE).Value = CLng(WW_DETAILSALES_L10T_SURGERY_RAILFARE)
                sheet.Range(CONST_DETAILSALES_L10T_SURGERY_SHIPFEE).Value = CLng(WW_DETAILSALES_L10T_SURGERY_SHIPFEE)
            End If
            ' 内訳明細(売却時回送)-L10t-管内
            If WW_DETAILSALES_L10T_PIPE_QUANTITY = "0" And
                WW_DETAILSALES_L10T_PIPE_RAILFARE = "0" And
                WW_DETAILSALES_L10T_PIPE_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILSALES_L10T_PIPE_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILSALES_L10T_PIPE_QUANTITY).Value = CLng(WW_DETAILSALES_L10T_PIPE_QUANTITY)
                sheet.Range(CONST_DETAILSALES_L10T_PIPE_RAILFARE).Value = CLng(WW_DETAILSALES_L10T_PIPE_RAILFARE)
                sheet.Range(CONST_DETAILSALES_L10T_PIPE_SHIPFEE).Value = CLng(WW_DETAILSALES_L10T_PIPE_SHIPFEE)
            End If
            ' 内訳明細(修理時回送)-L10t-管外
            If WW_DETAILREPAIR_L10T_SURGERY_QUANTITY = "0" And
                WW_DETAILREPAIR_L10T_SURGERY_RAILFARE = "0" And
                WW_DETAILREPAIR_L10T_SURGERY_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILREPAIR_L10T_SURGERY_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILREPAIR_L10T_SURGERY_QUANTITY).Value = CLng(WW_DETAILREPAIR_L10T_SURGERY_QUANTITY)
                sheet.Range(CONST_DETAILREPAIR_L10T_SURGERY_RAILFARE).Value = CLng(WW_DETAILREPAIR_L10T_SURGERY_RAILFARE)
                sheet.Range(CONST_DETAILREPAIR_L10T_SURGERY_SHIPFEE).Value = CLng(WW_DETAILREPAIR_L10T_SURGERY_SHIPFEE)
            End If

            ' 内訳明細(修理時回送)-L10t-管内
            If WW_DETAILREPAIR_L10T_PIPE_QUANTITY = "0" And
                WW_DETAILREPAIR_L10T_PIPE_RAILFARE = "0" And
                WW_DETAILREPAIR_L10T_PIPE_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILREPAIR_L10T_PIPE_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILREPAIR_L10T_PIPE_QUANTITY).Value = CLng(WW_DETAILREPAIR_L10T_PIPE_QUANTITY)
                sheet.Range(CONST_DETAILREPAIR_L10T_PIPE_RAILFARE).Value = CLng(WW_DETAILREPAIR_L10T_PIPE_RAILFARE)
                sheet.Range(CONST_DETAILREPAIR_L10T_PIPE_SHIPFEE).Value = CLng(WW_DETAILREPAIR_L10T_PIPE_SHIPFEE)
            End If
        End If
#End Region
#Region "内訳明細(修理時回送・売却時回送)-SUR"
        ' 内訳明細(修理時回送)-SUR-管内-個数
        Dim WW_DETAILREPAIR_SUR_PIPE_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(修理時回送)-SUR-管内-鉄道運賃
        Dim WW_DETAILREPAIR_SUR_PIPE_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(修理時回送)-SUR-管内-発送料
        Dim WW_DETAILREPAIR_SUR_PIPE_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(修理時回送)-SUR-管外-個数
        Dim WW_DETAILREPAIR_SUR_SURGERY_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(修理時回送)-SUR-管外-鉄道運賃
        Dim WW_DETAILREPAIR_SUR_SURGERY_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(修理時回送)-SUR-管外-発送料
        Dim WW_DETAILREPAIR_SUR_SURGERY_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-SUR-管内-個数
        Dim WW_DETAILSALES_SUR_PIPE_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(売却時回送)-SUR-管内-鉄道運賃
        Dim WW_DETAILSALES_SUR_PIPE_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(売却時回送)-SUR-管内-発送料
        Dim WW_DETAILSALES_SUR_PIPE_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-SUR-管外-個数
        Dim WW_DETAILSALES_SUR_SURGERY_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(売却時回送)-SUR-管外-鉄道運賃
        Dim WW_DETAILSALES_SUR_SURGERY_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(売却時回送)-SUR-管外-発送料
        Dim WW_DETAILSALES_SUR_SURGERY_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_SUR) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        'データがすべて存在しない場合は該当する明細範囲を削除
        If WW_DETAILREPAIR_SUR_PIPE_QUANTITY = "0" And
            WW_DETAILREPAIR_SUR_PIPE_RAILFARE = "0" And
            WW_DETAILREPAIR_SUR_PIPE_SHIPFEE = "0" And
            WW_DETAILREPAIR_SUR_SURGERY_QUANTITY = "0" And
            WW_DETAILREPAIR_SUR_SURGERY_RAILFARE = "0" And
            WW_DETAILREPAIR_SUR_SURGERY_SHIPFEE = "0" And
            WW_DETAILSALES_SUR_PIPE_QUANTITY = "0" And
            WW_DETAILSALES_SUR_PIPE_RAILFARE = "0" And
            WW_DETAILSALES_SUR_PIPE_SHIPFEE = "0" And
            WW_DETAILSALES_SUR_SURGERY_QUANTITY = "0" And
            WW_DETAILSALES_SUR_SURGERY_RAILFARE = "0" And
            WW_DETAILSALES_SUR_SURGERY_SHIPFEE = "0" Then
            sheet.Range(CONST_DETAIL_SUR_RANGE).Delete(DeleteShiftDirection.Up)
        Else
            'データがない行は削除
            ' 内訳明細(売却時回送)-SUR-管外
            If WW_DETAILSALES_SUR_SURGERY_QUANTITY = "0" And
                WW_DETAILSALES_SUR_SURGERY_RAILFARE = "0" And
                WW_DETAILSALES_SUR_SURGERY_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILSALES_SUR_SURGERY_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILSALES_SUR_SURGERY_QUANTITY).Value = CLng(WW_DETAILSALES_SUR_SURGERY_QUANTITY)
                sheet.Range(CONST_DETAILSALES_SUR_SURGERY_RAILFARE).Value = CLng(WW_DETAILSALES_SUR_SURGERY_RAILFARE)
                sheet.Range(CONST_DETAILSALES_SUR_SURGERY_SHIPFEE).Value = CLng(WW_DETAILSALES_SUR_SURGERY_SHIPFEE)
            End If
            ' 内訳明細(売却時回送)-SUR-管内
            If WW_DETAILSALES_SUR_PIPE_QUANTITY = "0" And
                WW_DETAILSALES_SUR_PIPE_RAILFARE = "0" And
                WW_DETAILSALES_SUR_PIPE_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILSALES_SUR_PIPE_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILSALES_SUR_PIPE_QUANTITY).Value = CLng(WW_DETAILSALES_SUR_PIPE_QUANTITY)
                sheet.Range(CONST_DETAILSALES_SUR_PIPE_RAILFARE).Value = CLng(WW_DETAILSALES_SUR_PIPE_RAILFARE)
                sheet.Range(CONST_DETAILSALES_SUR_PIPE_SHIPFEE).Value = CLng(WW_DETAILSALES_SUR_PIPE_SHIPFEE)
            End If
            ' 内訳明細(修理時回送)-SUR-管外
            If WW_DETAILREPAIR_SUR_SURGERY_QUANTITY = "0" And
                WW_DETAILREPAIR_SUR_SURGERY_RAILFARE = "0" And
                WW_DETAILREPAIR_SUR_SURGERY_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILREPAIR_SUR_SURGERY_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILREPAIR_SUR_SURGERY_QUANTITY).Value = CLng(WW_DETAILREPAIR_SUR_SURGERY_QUANTITY)
                sheet.Range(CONST_DETAILREPAIR_SUR_SURGERY_RAILFARE).Value = CLng(WW_DETAILREPAIR_SUR_SURGERY_RAILFARE)
                sheet.Range(CONST_DETAILREPAIR_SUR_SURGERY_SHIPFEE).Value = CLng(WW_DETAILREPAIR_SUR_SURGERY_SHIPFEE)
            End If

            ' 内訳明細(修理時回送)-SUR-管内
            If WW_DETAILREPAIR_SUR_PIPE_QUANTITY = "0" And
                WW_DETAILREPAIR_SUR_PIPE_RAILFARE = "0" And
                WW_DETAILREPAIR_SUR_PIPE_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILREPAIR_SUR_PIPE_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILREPAIR_SUR_PIPE_QUANTITY).Value = CLng(WW_DETAILREPAIR_SUR_PIPE_QUANTITY)
                sheet.Range(CONST_DETAILREPAIR_SUR_PIPE_RAILFARE).Value = CLng(WW_DETAILREPAIR_SUR_PIPE_RAILFARE)
                sheet.Range(CONST_DETAILREPAIR_SUR_PIPE_SHIPFEE).Value = CLng(WW_DETAILREPAIR_SUR_PIPE_SHIPFEE)
            End If
        End If
#End Region
#Region "内訳明細(修理時回送・売却時回送)-冷凍"
        ' 内訳明細(修理時回送)-冷凍-管内-個数
        Dim WW_DETAILREPAIR_RATION_PIPE_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(修理時回送)-冷凍-管内-鉄道運賃
        Dim WW_DETAILREPAIR_RATION_PIPE_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(修理時回送)-冷凍-管内-発送料
        Dim WW_DETAILREPAIR_RATION_PIPE_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(修理時回送)-冷凍-管外-個数
        Dim WW_DETAILREPAIR_RATION_SURGERY_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(修理時回送)-冷凍-管外-鉄道運賃
        Dim WW_DETAILREPAIR_RATION_SURGERY_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(修理時回送)-冷凍-管外-発送料
        Dim WW_DETAILREPAIR_RATION_SURGERY_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-冷凍-管内-個数
        Dim WW_DETAILSALES_RATION_PIPE_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(売却時回送)-冷凍-管内-鉄道運賃
        Dim WW_DETAILSALES_RATION_PIPE_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(売却時回送)-冷凍-管内-発送料
        Dim WW_DETAILSALES_RATION_PIPE_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-冷凍-管外-個数
        Dim WW_DETAILSALES_RATION_SURGERY_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(売却時回送)-冷凍-管外-鉄道運賃
        Dim WW_DETAILSALES_RATION_SURGERY_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(売却時回送)-冷凍-管外-発送料
        Dim WW_DETAILSALES_RATION_SURGERY_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATION) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        'データがすべて存在しない場合は該当する明細範囲を削除
        If WW_DETAILREPAIR_RATION_PIPE_QUANTITY = "0" And
            WW_DETAILREPAIR_RATION_PIPE_RAILFARE = "0" And
            WW_DETAILREPAIR_RATION_PIPE_SHIPFEE = "0" And
            WW_DETAILREPAIR_RATION_SURGERY_QUANTITY = "0" And
            WW_DETAILREPAIR_RATION_SURGERY_RAILFARE = "0" And
            WW_DETAILREPAIR_RATION_SURGERY_SHIPFEE = "0" And
            WW_DETAILSALES_RATION_PIPE_QUANTITY = "0" And
            WW_DETAILSALES_RATION_PIPE_RAILFARE = "0" And
            WW_DETAILSALES_RATION_PIPE_SHIPFEE = "0" And
            WW_DETAILSALES_RATION_SURGERY_QUANTITY = "0" And
            WW_DETAILSALES_RATION_SURGERY_RAILFARE = "0" And
            WW_DETAILSALES_RATION_SURGERY_SHIPFEE = "0" Then
            sheet.Range(CONST_DETAIL_RATION_RANGE).Delete(DeleteShiftDirection.Up)
        Else
            'データがない行は削除
            ' 内訳明細(売却時回送)-冷凍-管外
            If WW_DETAILSALES_RATION_SURGERY_QUANTITY = "0" And
                WW_DETAILSALES_RATION_SURGERY_RAILFARE = "0" And
                WW_DETAILSALES_RATION_SURGERY_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILSALES_RATION_SURGERY_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILSALES_RATION_SURGERY_QUANTITY).Value = CLng(WW_DETAILSALES_RATION_SURGERY_QUANTITY)
                sheet.Range(CONST_DETAILSALES_RATION_SURGERY_RAILFARE).Value = CLng(WW_DETAILSALES_RATION_SURGERY_RAILFARE)
                sheet.Range(CONST_DETAILSALES_RATION_SURGERY_SHIPFEE).Value = CLng(WW_DETAILSALES_RATION_SURGERY_SHIPFEE)
            End If
            ' 内訳明細(売却時回送)-冷凍-管内
            If WW_DETAILSALES_RATION_PIPE_QUANTITY = "0" And
                WW_DETAILSALES_RATION_PIPE_RAILFARE = "0" And
                WW_DETAILSALES_RATION_PIPE_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILSALES_RATION_PIPE_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILSALES_RATION_PIPE_QUANTITY).Value = CLng(WW_DETAILSALES_RATION_PIPE_QUANTITY)
                sheet.Range(CONST_DETAILSALES_RATION_PIPE_RAILFARE).Value = CLng(WW_DETAILSALES_RATION_PIPE_RAILFARE)
                sheet.Range(CONST_DETAILSALES_RATION_PIPE_SHIPFEE).Value = CLng(WW_DETAILSALES_RATION_PIPE_SHIPFEE)
            End If
            ' 内訳明細(修理時回送)-冷凍-管外
            If WW_DETAILREPAIR_RATION_SURGERY_QUANTITY = "0" And
                WW_DETAILREPAIR_RATION_SURGERY_RAILFARE = "0" And
                WW_DETAILREPAIR_RATION_SURGERY_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILREPAIR_RATION_SURGERY_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILREPAIR_RATION_SURGERY_QUANTITY).Value = CLng(WW_DETAILREPAIR_RATION_SURGERY_QUANTITY)
                sheet.Range(CONST_DETAILREPAIR_RATION_SURGERY_RAILFARE).Value = CLng(WW_DETAILREPAIR_RATION_SURGERY_RAILFARE)
                sheet.Range(CONST_DETAILREPAIR_RATION_SURGERY_SHIPFEE).Value = CLng(WW_DETAILREPAIR_RATION_SURGERY_SHIPFEE)
            End If

            ' 内訳明細(修理時回送)-冷凍-管内
            If WW_DETAILREPAIR_RATION_PIPE_QUANTITY = "0" And
                WW_DETAILREPAIR_RATION_PIPE_RAILFARE = "0" And
                WW_DETAILREPAIR_RATION_PIPE_SHIPFEE = "0" Then
                sheet.Range(CONST_DETAILREPAIR_RATION_PIPE_RANGE).Delete(DeleteShiftDirection.Up)
            Else
                sheet.Range(CONST_DETAILREPAIR_RATION_PIPE_QUANTITY).Value = CLng(WW_DETAILREPAIR_RATION_PIPE_QUANTITY)
                sheet.Range(CONST_DETAILREPAIR_RATION_PIPE_RAILFARE).Value = CLng(WW_DETAILREPAIR_RATION_PIPE_RAILFARE)
                sheet.Range(CONST_DETAILREPAIR_RATION_PIPE_SHIPFEE).Value = CLng(WW_DETAILREPAIR_RATION_PIPE_SHIPFEE)
            End If
        End If
#End Region
#Region "内訳明細(修理時回送・売却時回送)-冷蔵"
        ' 内訳明細(修理時回送)-冷蔵-管内-個数
        Dim WW_DETAILREPAIR_RATED_PIPE_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(修理時回送)-冷蔵-管内-鉄道運賃
        Dim WW_DETAILREPAIR_RATED_PIPE_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(修理時回送)-冷蔵-管内-発送料
        Dim WW_DETAILREPAIR_RATED_PIPE_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(修理時回送)-冷蔵-管外-個数
        Dim WW_DETAILREPAIR_RATED_SURGERY_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(修理時回送)-冷蔵-管外-鉄道運賃
        Dim WW_DETAILREPAIR_RATED_SURGERY_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(修理時回送)-冷蔵-管外-発送料
        Dim WW_DETAILREPAIR_RATED_SURGERY_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_REPAIR) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-冷蔵-管内-個数
        Dim WW_DETAILSALES_RATED_PIPE_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(売却時回送)-冷蔵-管内-鉄道運賃
        Dim WW_DETAILSALES_RATED_PIPE_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(売却時回送)-冷蔵-管内-発送料
        Dim WW_DETAILSALES_RATED_PIPE_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_PIPE) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-冷蔵-管外-個数
        Dim WW_DETAILSALES_RATED_SURGERY_QUANTITY As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("QUANTITY")))

        ' 内訳明細(売却時回送)-冷蔵-管外-鉄道運賃
        Dim WW_DETAILSALES_RATED_SURGERY_RAILFARE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("RAILFARE")))

        ' 内訳明細(売却時回送)-冷蔵-管外-発送料
        Dim WW_DETAILSALES_RATED_SURGERY_SHIPFEE As String = WW_Row.Cast(Of DataRow) _
        .Where(Function(row) row("DATATYPE").ToString = CONST_DATATYPE_ACHIEVEMENTS) _
        .Where(Function(row) row("BIGCTNCD").ToString = CONST_BIGCTNTYPE_RATED) _
        .Where(Function(row) row("AREAKBN").ToString = CONST_AREA_SURGERY) _
        .Where(Function(row) row("ACCOUNTSTATUSKBN").ToString = CONST_ACCOUNTSTATUSKBN_SALES) _
        .Sum(Function(row) CLng(row("SHIPFEE")))

        ' 内訳明細(売却時回送)-冷蔵-管外
        If WW_DETAILSALES_RATED_SURGERY_QUANTITY = "0" And
                    WW_DETAILSALES_RATED_SURGERY_RAILFARE = "0" And
                    WW_DETAILSALES_RATED_SURGERY_SHIPFEE = "0" Then
            'sheet.Range(CONST_DETAILSALES_RATED_SURGERY_RANGE).Delete(DeleteShiftDirection.Up)
        Else
            sheet.Range(CONST_DETAILSALES_RATED_SURGERY_QUANTITY).Value = CLng(WW_DETAILSALES_RATED_SURGERY_QUANTITY)
            sheet.Range(CONST_DETAILSALES_RATED_SURGERY_RAILFARE).Value = CLng(WW_DETAILSALES_RATED_SURGERY_RAILFARE)
            sheet.Range(CONST_DETAILSALES_RATED_SURGERY_SHIPFEE).Value = CLng(WW_DETAILSALES_RATED_SURGERY_SHIPFEE)
        End If
        ' 内訳明細(売却時回送)-冷蔵-管内
        If WW_DETAILSALES_RATED_PIPE_QUANTITY = "0" And
                    WW_DETAILSALES_RATED_PIPE_RAILFARE = "0" And
                    WW_DETAILSALES_RATED_PIPE_SHIPFEE = "0" Then
            'sheet.Range(CONST_DETAILSALES_RATED_PIPE_RANGE).Delete(DeleteShiftDirection.Up)
        Else
            sheet.Range(CONST_DETAILSALES_RATED_PIPE_QUANTITY).Value = CLng(WW_DETAILSALES_RATED_PIPE_QUANTITY)
            sheet.Range(CONST_DETAILSALES_RATED_PIPE_RAILFARE).Value = CLng(WW_DETAILSALES_RATED_PIPE_RAILFARE)
            sheet.Range(CONST_DETAILSALES_RATED_PIPE_SHIPFEE).Value = CLng(WW_DETAILSALES_RATED_PIPE_SHIPFEE)
        End If
        ' 内訳明細(修理時回送)-冷蔵-管外
        If WW_DETAILREPAIR_RATED_SURGERY_QUANTITY = "0" And
                    WW_DETAILREPAIR_RATED_SURGERY_RAILFARE = "0" And
                    WW_DETAILREPAIR_RATED_SURGERY_SHIPFEE = "0" Then
            'sheet.Range(CONST_DETAILREPAIR_RATED_SURGERY_RANGE).Delete(DeleteShiftDirection.Up)
        Else
            sheet.Range(CONST_DETAILREPAIR_RATED_SURGERY_QUANTITY).Value = CLng(WW_DETAILREPAIR_RATED_SURGERY_QUANTITY)
            sheet.Range(CONST_DETAILREPAIR_RATED_SURGERY_RAILFARE).Value = CLng(WW_DETAILREPAIR_RATED_SURGERY_RAILFARE)
            sheet.Range(CONST_DETAILREPAIR_RATED_SURGERY_SHIPFEE).Value = CLng(WW_DETAILREPAIR_RATED_SURGERY_SHIPFEE)
        End If

        ' 内訳明細(修理時回送)-冷蔵-管内
        If WW_DETAILREPAIR_RATED_PIPE_QUANTITY = "0" And
                    WW_DETAILREPAIR_RATED_PIPE_RAILFARE = "0" And
                    WW_DETAILREPAIR_RATED_PIPE_SHIPFEE = "0" Then
            'sheet.Range(CONST_DETAILREPAIR_RATED_PIPE_RANGE).Delete(DeleteShiftDirection.Up)
        Else
            sheet.Range(CONST_DETAILREPAIR_RATED_PIPE_QUANTITY).Value = CLng(WW_DETAILREPAIR_RATED_PIPE_QUANTITY)
            sheet.Range(CONST_DETAILREPAIR_RATED_PIPE_RAILFARE).Value = CLng(WW_DETAILREPAIR_RATED_PIPE_RAILFARE)
            sheet.Range(CONST_DETAILREPAIR_RATED_PIPE_SHIPFEE).Value = CLng(WW_DETAILREPAIR_RATED_PIPE_SHIPFEE)
        End If

#End Region

    End Sub



End Class
