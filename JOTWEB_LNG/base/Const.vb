Option Strict On
''' <summary>
''' 共通で利用する定数定義
''' </summary>
Public Module BaseDllConst

    ''' <summary>
    ''' システムコード グループ会社向け(GR)
    ''' </summary>
    Public Const C_SYSCODE_GR As String = "GR"
    ''' <summary>
    '''  項目値の分割用デリミター値
    ''' </summary>
    Public Const C_VALUE_SPLIT_DELIMITER As String = "|"
    ''' <summary>
    ''' フラグ用 有効値
    ''' </summary>
    Public Const CONST_FLAG_YES As String = "Y" '"1"
    ''' <summary>
    ''' フラグ用 無効値
    ''' </summary>
    Public Const CONST_FLAG_NO As String = "N" '"0"
    ''' <summary>
    ''' URL関連
    ''' </summary>
    Public Class C_URL
        ''' <summary>
        ''' ログインURL
        ''' </summary>
        Public Const LOGIN As String = "~/M10000LOGON.aspx"
        ''' <summary>
        ''' アップロード処理用ハンドラーURL
        ''' </summary>
        Public Const UPLOAD_HANDLER As String = "~/xx.ashx"
        ''' <summary>
        ''' 採番取得用ハンドラーURL
        ''' </summary>
        Public Const NUMBER_ASSIGNMENT As String = "/office/GR/GRCO0103AUTONUM.ashx"
        ''' <summary>
        ''' HELP画面
        ''' </summary>
        Public Const HELP As String = "~/GR/GRCO0105HELP.aspx"
    End Class
    ''' <summary>
    ''' 他システム届先用接頭文字列
    ''' </summary>
    Public Class C_ANOTHER_SYSTEMS_DISTINATION_PREFIX
        ''' <summary>
        ''' JX(TG含む)
        ''' </summary>
        Public Const JX As String = "JX"
        ''' <summary>
        ''' COSMO
        ''' </summary>
        Public Const COSMO As String = "COSMO"

    End Class
    ''' <summary>
    ''' 言語設定
    ''' </summary>
    Public Class C_LANG
        ''' <summary>
        ''' 日本語
        ''' </summary>
        Public Const JA As String = "JA"
        ''' <summary>
        ''' 英語
        ''' </summary>
        Public Const EN As String = "EN"
    End Class
    ''' <summary>
    ''' 実行区分
    ''' </summary>
    Public Class C_RUNKBN
        ''' <summary>
        ''' オンライン
        ''' </summary>
        Public Const ONLINE As String = "ONLINE"
        ''' <summary>
        ''' バッチ
        ''' </summary>
        Public Const BATCH As String = "BATCH"
    End Class
    ''' <summary>
    ''' 削除フラグ
    ''' </summary>
    Public Class C_DELETE_FLG
        ''' <summary>
        ''' 削除
        ''' </summary>
        Public Const DELETE As String = "1"
        ''' <summary>
        ''' 生存
        ''' </summary>
        Public Const ALIVE As String = "0"
    End Class
    ''' <summary>
    ''' ロールの値
    ''' </summary>
    ''' <remarks></remarks>
    Public Class C_ROLE_VARIANT
        ''' <summary>
        ''' ユーザの表示会社権限
        ''' </summary>
        Public Const USER_COMP As String = "CAMP"
        ''' <summary>
        ''' ユーザの操作部署権限
        ''' </summary>
        Public Const USER_ORG As String = "ORG"
        ''' <summary>
        ''' ユーザの更新権限（各画面）
        ''' </summary>
        Public Const USER_PERTMIT As String = "MAP"
        ''' <summary>
        ''' ユーザのプロファイル変更権限（各ユーザ）
        ''' </summary>
        Public Const USER_PROFILE As String = "USER"
        ''' <summary>
        ''' サーバの表示会社権限
        ''' </summary>
        Public Const SERV_COMP As String = "SRVCAMP"
        ''' <summary>
        ''' APサーバにおける操作部署権限
        ''' </summary>
        Public Const SERV_ORG As String = "SRVORG"
        ''' <summary>
        ''' APサーバにおける更新権限（各画面）
        ''' </summary>
        Public Const SERV_PERTMIT As String = "SRVMAP"

    End Class
    ''' <summary>
    ''' 権限コード
    ''' </summary>
    Public Class C_PERMISSION
        ''' <summary>
        ''' 参照・更新
        ''' </summary>
        Public Const UPDATE As String = "2"
        ''' <summary>
        ''' 参照のみ
        ''' </summary>
        Public Const REFERLANCE As String = "1"
        ''' <summary>
        ''' 権限なし
        ''' </summary>
        Public Const INVALID As String = "0"
    End Class
    ''' <summary>
    ''' 一覧のOPERATION項目に設定するコード
    ''' </summary>
    Public Class C_LIST_OPERATION_CODE
        ''' <summary>
        ''' データなし
        ''' </summary>
        Public Const NODATA As String = ""
        ''' <summary>
        ''' 表示なし
        ''' </summary>
        Public Const NODISP As String = "＆nbsp;"
        ''' <summary>
        ''' 行選択
        ''' </summary>
        Public Const SELECTED As String = "★"
        ''' <summary>
        ''' 追加対象
        ''' </summary>
        Public Const INSERTING As String = "追加"
        ''' <summary>
        ''' 更新対象
        ''' </summary>
        Public Const UPDATING As String = "更新"
        ''' <summary>
        ''' エラー行対象
        ''' </summary>
        Public Const ERRORED As String = "エラー"
        ''' <summary>
        ''' 更新（警告あり）対象
        ''' </summary>
        Public Const WARNING As String = "警告"
    End Class
    ''' <summary>
    ''' 検査日に対応したアラート用コード
    ''' </summary>
    Public Class C_INSPECTIONALERT
        ''' <summary>
        ''' 赤丸（3日以内のタンク車）
        ''' </summary>
        Public Const ALERT_RED As String = "検査日まで後、3日以内のタンク車"
        ''' <summary>
        ''' 黄丸（4日～6日のタンク車）
        ''' </summary>
        Public Const ALERT_YELLOW As String = "検査日まで後、4日～6日のタンク車"
        ''' <summary>
        ''' 緑丸（7日以上のタンク車）
        ''' </summary>
        Public Const ALERT_GREEN As String = "検査日まで後、7日以上のタンク車"
    End Class
    ''' <summary>
    ''' 端末分類（LNS0001_TERM TERMCLASS）
    ''' </summary>
    Public Class C_TERMCLASS
        ''' <summary>
        ''' 端末（未使用）
        ''' </summary>
        Public Const CLIENT As String = "0"
        ''' <summary>
        ''' 拠点サーバ（未使用）
        ''' </summary>
        Public Const BASE As String = "1"
        ''' <summary>
        ''' 本社サーバ
        ''' </summary>
        Public Const HEAD As String = "2"
        ''' <summary>
        ''' クラウド（全社）サーバ
        ''' </summary>
        Public Const CLOUD As String = "9"
    End Class
    ''' <summary>
    ''' SQL共通条件文
    ''' </summary>
    Public Const C_SQL_COMMON_COND As String = "   and STYMD   <= @STYMD " _
                                             & "   and ENDYMD  >= @ENDYMD " _
                                             & "   and DELFLG  <> @DELFLG "
    ''' <summary>
    ''' デフォルトデータ検索値
    ''' </summary>
    Public Const C_DEFAULT_DATAKEY As String = "Default"

    ''' <summary>
    ''' 日付デフォルト値
    ''' </summary>
    Public Const C_DEFAULT_YMD As String = "1950/01/01"
    ''' <summary>
    ''' 日付最大値
    ''' </summary>
    Public Const C_MAX_YMD As String = "9999/12/31"
    ''' <summary>
    ''' 計上年月202307以降を総額で消費税計算を行う
    ''' </summary>
    Public Const C_SOUGAKUFROM_YM As Integer = 202307

#Region "組織"
    ''' <summary>
    ''' 情報システム部
    ''' </summary>
    Public Const CONST_OFFICECODE_SYSTEM As String = "011308"

    ''' <summary>
    ''' 高圧ガス１部
    ''' </summary>
    Public Const CONST_OFFICECODE_011310 As String = "011310"

    ''' <summary>
    ''' 北海道支店
    ''' </summary>
    Public Const CONST_OFFICECODE_HOKKAIDO As String = "010102"

    ''' <summary>
    ''' 東北支店
    ''' </summary>
    Public Const CONST_OFFICECODE_TOHOKU As String = "010401"

    ''' <summary>
    ''' 関東支店
    ''' </summary>
    Public Const CONST_OFFICECODE_KANTO As String = "011402"

    ''' <summary>
    ''' 中部支店
    ''' </summary>
    Public Const CONST_OFFICECODE_CHUBU As String = "012401"

    ''' <summary>
    ''' 関西支店
    ''' </summary>
    Public Const CONST_OFFICECODE_KANSAI As String = "012701"

    ''' <summary>
    ''' 九州支店
    ''' </summary>
    Public Const CONST_OFFICECODE_KYUSYU As String = "014001"

    ''' <summary>
    ''' 五井営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_011201 As String = "011201"
    ''' <summary>
    ''' 甲子営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_011202 As String = "011202"
    ''' <summary>
    ''' 袖ヶ浦営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_011203 As String = "011203"
    ''' <summary>
    ''' 根岸営業所
    ''' </summary>
    Public Const CONST_OFFICECODE_011402 As String = "011402"

#End Region

#Region "届先取引先"
    ''' <summary>
    ''' ＥＮＥＯＳ
    ''' </summary>
    Public Const CONST_TORICODE_0005700000 As String = "0005700000"
    ''' <summary>
    ''' エスジーリキッドサービス
    ''' </summary>
    Public Const CONST_TORICODE_0045300000 As String = "0045300000"
    ''' <summary>
    ''' ＤＧＥ
    ''' </summary>
    Public Const CONST_TORICODE_0051200000 As String = "0051200000"
    ''' <summary>
    ''' エスケイ産業
    ''' </summary>
    Public Const CONST_TORICODE_0045200000 As String = "0045200000"
    ''' <summary>
    ''' 石油資源開発
    ''' </summary>
    Public Const CONST_TORICODE_0132800000 As String = "0132800000"
    ''' <summary>
    ''' シーエナジー
    ''' </summary>
    Public Const CONST_TORICODE_0110600000 As String = "0110600000"
    ''' <summary>
    ''' 北陸エルネス
    ''' </summary>
    Public Const CONST_TORICODE_0238900000 As String = "0238900000"
#End Region

#Region "受注受付部署"
    ''' <summary>
    ''' EX_八戸営業所
    ''' </summary>
    Public Const CONST_ORDERORGCODE_020202 As String = "020202"
    ''' <summary>
    ''' EX_水島営業所
    ''' </summary>
    Public Const CONST_ORDERORGCODE_023301 As String = "023301"
    ''' <summary>
    ''' EX 九州営業所
    ''' </summary>
    Public Const CONST_ORDERORGCODE_024001 As String = "024001"
    ''' <summary>
    ''' EX_西日本支店車庫
    ''' </summary>
    Public Const CONST_ORDERORGCODE_022702 As String = "022702"
    ''' <summary>
    ''' EX_姫路営業所
    ''' </summary>
    Public Const CONST_ORDERORGCODE_022801 As String = "022801"
    ''' <summary>
    ''' EX_新潟支店車庫
    ''' </summary>
    Public Const CONST_ORDERORGCODE_021502 As String = "021502"
    ''' <summary>
    ''' EX_庄内営業所
    ''' </summary>
    Public Const CONST_ORDERORGCODE_020601 As String = "020601"
    ''' <summary>
    ''' EX_東北支店車庫
    ''' </summary>
    Public Const CONST_ORDERORGCODE_020402 As String = "020402"
    ''' <summary>
    ''' EX_茨城営業所
    ''' </summary>
    Public Const CONST_ORDERORGCODE_020804 As String = "020804"
    ''' <summary>
    ''' EX_石狩営業所
    ''' </summary>
    Public Const CONST_ORDERORGCODE_020104 As String = "020104"
    ''' <summary>
    ''' EX_中部支店車庫
    ''' </summary>
    Public Const CONST_ORDERORGCODE_022302 As String = "022302"
    ''' <summary>
    ''' EX_上越営業所
    ''' </summary>
    Public Const CONST_ORDERORGCODE_021504 As String = "021504"
    ''' <summary>
    ''' EX_富山営業所
    ''' </summary>
    Public Const CONST_ORDERORGCODE_021601 As String = "021601"
    ''' <summary>
    ''' EX_四日市営業所
    ''' </summary>
    Public Const CONST_ORDERORGCODE_022401 As String = "022401"
#End Region

#Region "届先"
    ''' <summary>
    ''' 三井Ｅ＆Ｓマシナリー（デ組）
    ''' </summary>
    Public Const CONST_TODOKECODE_004002 As String = "004002"
    ''' <summary>
    ''' コカ・コーラ　ボトラーズジャパン
    ''' </summary>
    Public Const CONST_TODOKECODE_005509 As String = "005509"
    ''' <summary>
    ''' 新宮ガス
    ''' </summary>
    Public Const CONST_TODOKECODE_001640 As String = "001640"
    ''' <summary>
    ''' 日本栄船
    ''' </summary>
    Public Const CONST_TODOKECODE_004916 As String = "004916"
    ''' <summary>
    ''' 昭和産業㈱
    ''' </summary>
    Public Const CONST_TODOKECODE_005866 As String = "005866"
    ''' <summary>
    ''' 東部瓦斯
    ''' </summary>
    Public Const CONST_TODOKECODE_005487 As String = "005487"
    ''' <summary>
    ''' ニプロ（株）伊藤忠
    ''' </summary>
    Public Const CONST_TODOKECODE_001269 As String = "001269"
    ''' <summary>
    ''' ニプロ（株）カメイ
    ''' </summary>
    Public Const CONST_TODOKECODE_003840 As String = "003840"
    ''' <summary>
    ''' ナガセケムテックス
    ''' </summary>
    Public Const CONST_TODOKECODE_006880 As String = "006880"
    ''' <summary>
    ''' リコー福井事業所
    ''' </summary>
    Public Const CONST_TODOKECODE_004559 As String = "004559"
    ''' <summary>
    ''' （ＳＫ）北陸ガス　栃尾
    ''' </summary>
    Public Const CONST_TODOKECODE_004012 As String = "004012"
    ''' <summary>
    ''' 寺岡製作所（SK）
    ''' </summary>
    Public Const CONST_TODOKECODE_005890 As String = "005890"
    ''' <summary>
    ''' （SK）ジャパン・パックライス男鹿
    ''' </summary>
    Public Const CONST_TODOKECODE_007273 As String = "007273"
    ''' <summary>
    ''' （ＳＫ）テーブルマーク　塩沢
    ''' </summary>
    Public Const CONST_TODOKECODE_002019 As String = "002019"
    ''' <summary>
    ''' （ＳＫ）若松ガス　玉川
    ''' </summary>
    Public Const CONST_TODOKECODE_002025 As String = "002025"
    ''' <summary>
    ''' （ＳＫ）本田金属　喜多方サテライト
    ''' </summary>
    Public Const CONST_TODOKECODE_002022 As String = "002022"
    ''' <summary>
    ''' 釧路ガス
    ''' </summary>
    Public Const CONST_TODOKECODE_003561 As String = "003561"
    ''' <summary>
    ''' (SK)室蘭ガス
    ''' </summary>
    Public Const CONST_TODOKECODE_003563 As String = "003563"
    ''' <summary>
    ''' ＳＫ勇払（工場）
    ''' </summary>
    Public Const CONST_TODOKECODE_005834 As String = "005834"
    ''' <summary>
    ''' 室蘭港バンカリング
    ''' </summary>
    Public Const CONST_TODOKECODE_006915 As String = "006915"
    ''' <summary>
    ''' 商船三井
    ''' </summary>
    Public Const CONST_TODOKECODE_007110 As String = "007110"
#End Region

#Region "業務ID"
    ''' <summary>
    ''' 業務ID
    ''' </summary>
    Public Class C_SERVICEID_CD
        ''' <summary>
        ''' 精算ファイル
        ''' </summary>
        Public Const ID_RESSNF As String = "ressnf"
        ''' <summary>
        ''' リース
        ''' </summary>
        Public Const ID_LEASE As String = "lease"
        ''' <summary>
        ''' 共通KEY用
        ''' </summary>
        Public Const KEY_ALL As String = "ALL"
    End Class
#End Region

#Region "受注進行ステータス"
    ''' <summary>
    ''' 100:受注受付
    ''' </summary>
    Public Const CONST_ORDERSTATUS_100 As String = "100"
    ''' <summary>
    ''' 200:受注受付済
    ''' </summary>
    Public Const CONST_ORDERSTATUS_200 As String = "200"
    ''' <summary>
    ''' 300:請求済
    ''' </summary>
    Public Const CONST_ORDERSTATUS_300 As String = "300"
    ''' <summary>
    ''' 900:受注キャンセル
    ''' </summary>
    Public Const CONST_ORDERSTATUS_900 As String = "900"
#End Region

#Region "積空区分"
    ''' <summary>
    ''' 積空区分
    ''' </summary>
    Public Class C_STACKFREE_KBN
        ''' <summary>
        ''' 積
        ''' </summary>
        Public Const KBN_STACK As String = "1"
        ''' <summary>
        ''' 空
        ''' </summary>
        Public Const KBN_FREE As String = "2"
    End Class
#End Region

#Region "状態区分"
    ''' <summary>
    ''' 状態区分
    ''' </summary>
    Public Class C_OPERATIONKBN
        ''' <summary>
        ''' 積発
        ''' </summary>
        Public Const KBN_STACK_0 As String = "10"

        ''' <summary>
        ''' オ積
        ''' </summary>
        Public Const KBN_STACK_1 As String = "11"

        ''' <summary>
        ''' 積持
        ''' </summary>
        Public Const KBN_STACK_2 As String = "12"

        ''' <summary>
        ''' 空
        ''' </summary>
        Public Const KBN_FREE_0 As String = "20"

        ''' <summary>
        ''' オ空
        ''' </summary>
        Public Const KBN_FREE_1 As String = "21"

        ''' <summary>
        ''' 空持
        ''' </summary>
        Public Const KBN_FREE_2 As String = "22"

        ''' <summary>
        ''' 空下
        ''' </summary>
        Public Const KBN_FREE_5 As String = "25"

        ''' <summary>
        ''' 空上
        ''' </summary>
        Public Const KBN_FREE_6 As String = "26"

        ''' <summary>
        ''' 停滞
        ''' </summary>
        Public Const KBN_FREE_9 As String = "29"

        ''' <summary>
        ''' 検修
        ''' </summary>
        Public Const KBN_FREE_30 As String = "30"

        ''' <summary>
        ''' 空☆
        ''' </summary>
        Public Const KBN_FREE_80 As String = "80"

        ''' <summary>
        ''' 空売却
        ''' </summary>
        Public Const KBN_FREE_81 As String = "81"

        ''' <summary>
        ''' キャンセル
        ''' </summary>
        Public Const KBN_CANCEL As String = "99"

    End Class
#End Region

#Region "コンテナ状態区分"
    ''' <summary>
    ''' コンテナ状態区分
    ''' </summary>
    Public Class C_CONTSTATUSKBN
        ''' <summary>
        ''' 空白
        ''' </summary>
        Public Const KBN_BLANK As String = "0"

        ''' <summary>
        ''' 回送予定
        ''' </summary>
        Public Const KBN_KAISOU_PLAN As String = "1"

        ''' <summary>
        ''' 使用予定
        ''' </summary>
        Public Const KBN_SIYOU_PLAN As String = "2"

        ''' <summary>
        ''' 修理
        ''' </summary>
        Public Const KBN_SYURI As String = "3"

        ''' <summary>
        ''' 破損
        ''' </summary>
        Public Const KBN_HASON As String = "4"

        ''' <summary>
        ''' 特留
        ''' </summary>
        Public Const KBN_TOKURYU As String = "5"

        ''' <summary>
        ''' 濡損
        ''' </summary>
        Public Const KBN_WET_LOSS As String = "6"

        ''' <summary>
        ''' 営業外引合待
        ''' </summary>
        Public Const KBN_EIGYOGAI_HIKIAI_WAIT As String = "20"

        ''' <summary>
        ''' 営業外折衝中
        ''' </summary>
        Public Const KBN_EIGYOGAI_SESSHOCHU As String = "21"

        ''' <summary>
        ''' 営業外契約待
        ''' </summary>
        Public Const KBN_EIGYOGAI_KEIYAKU_WAIT As String = "22"

        ''' <summary>
        ''' 営業外受領待
        ''' </summary>
        Public Const KBN_EIGYOGAI_ZTYRYOU_WAIT As String = "23"

        ''' <summary>
        ''' 営業外処分
        ''' </summary>
        Public Const KBN_EIGYOGAI_DISPOSAL As String = "24"

        ''' <summary>
        ''' 営業外抹消待
        ''' </summary>
        Public Const KBN_EIGYOGAI_MASYO_WAIT As String = "25"

        ''' <summary>
        ''' 営業外抹消済
        ''' </summary>
        Public Const KBN_EIGYOGAI_MASYO_ZUMI As String = "26"

        ''' <summary>
        ''' 引合待
        ''' </summary>
        Public Const KBN_HIKIAI_WAIT As String = "10"

        ''' <summary>
        ''' 折衝中
        ''' </summary>
        Public Const KBN_SESSHOCHU As String = "11"

        '''' <summary>
        '''' 入金待
        '''' </summary>
        'Public Const KBN_NYUKIN_WAIT As String = "12"

        ''' <summary>
        ''' 契約待
        ''' </summary>
        Public Const KBN_KEIYAKU_WAIT As String = "13"

        ''' <summary>
        ''' 受領待
        ''' </summary>
        Public Const KBN_ZTYRYOU_WAIT As String = "14"

        ''' <summary>
        ''' 在庫処分
        ''' </summary>
        Public Const KBN_STOCK_DISPOSAL As String = "15"

        ''' <summary>
        ''' 抹消待
        ''' </summary>
        Public Const KBN_MASYO_WAIT As String = "30"

        ''' <summary>
        ''' 抹消済
        ''' </summary>
        Public Const KBN_MASYO_ZUMI As String = "31"

    End Class
#End Region

#Region "勘定科目用状態区分"
    ''' <summary>
    ''' 勘定科目用状態区分
    ''' </summary>
    Public Class C_ACCOUNTSTATUSKBN
        ''' <summary>
        ''' 積
        ''' </summary>
        Public Const KBN_STACK_DEF As String = "1"

        ''' <summary>
        ''' 積（着変）
        ''' </summary>
        Public Const KBN_STACK_CHG As String = "2"

        ''' <summary>
        ''' 空
        ''' </summary>
        Public Const KBN_FREE_DEF As String = "3"

        ''' <summary>
        ''' 空（修繕）
        ''' </summary>
        Public Const KBN_FREE_UPD As String = "4"

        ''' <summary>
        ''' 空（除却）
        ''' </summary>
        Public Const KBN_FREE_DEL As String = "5"

        ''' <summary>
        ''' 空(パレット返送)
        ''' </summary>
        Public Const KBN_FREE_RET As String = "6"

        ''' <summary>
        ''' 空（着変）
        ''' </summary>
        Public Const KBN_FREE_CHG As String = "7"

        ''' <summary>
        ''' キャンセル
        ''' </summary>
        Public Const KBN_CANCEL As String = "8"

        ''' <summary>
        ''' 空（売却）
        ''' </summary>
        Public Const KBN_FREE_SAL As String = "9"

    End Class
#End Region

#Region "精算ファイル申請状況"
    ''' <summary>
    ''' 精算ファイル申請状況
    ''' </summary>
    Public Class C_SEISANF_APPLSTATUS
        ''' <summary>
        ''' 未修正
        ''' </summary>
        Public Const UNCENSORED As String = "0"
        ''' <summary>
        ''' 申請中
        ''' </summary>
        Public Const APPLYING As String = "1"
        ''' <summary>
        ''' 取下げ
        ''' </summary>
        Public Const WITHDRAWAL As String = "2"
        ''' <summary>
        ''' 差戻し
        ''' </summary>
        Public Const REMAND As String = "3"
        ''' <summary>
        ''' 承認済（確認済み）
        ''' </summary>
        Public Const APPROVED As String = "4"
        ''' <summary>
        ''' リース中
        ''' </summary>
        Public Const LEASE As String = "5"
    End Class
#End Region

#Region "計上区分"
    ''' <summary>
    ''' 計上区分
    ''' </summary>
    Public Class C_KEIJO_KBN
        ''' <summary>
        ''' 未計上
        ''' </summary>
        Public Const NOT_RECORDED As String = "0"
        ''' <summary>
        ''' 計上済
        ''' </summary>
        Public Const RECORDED As String = "1"
    End Class
#End Region

#Region "リース契約形態"
    ''' <summary>
    ''' リース契約形態
    ''' </summary>
    Public Class C_LEASE_CONTRACT_TYPE
        ''' <summary>
        ''' 開示対象外
        ''' </summary>
        Public Const TYPE_NOTAPPL As String = "1"
        ''' <summary>
        ''' ファイナンス
        ''' </summary>
        Public Const TYPE_FINANCE As String = "2"
        ''' <summary>
        ''' オペレーティング
        ''' </summary>
        Public Const TYPE_OPERATING As String = "3"
    End Class
#End Region

#Region "リース適用区分"
    ''' <summary>
    ''' リース適用区分
    ''' </summary>
    Public Class C_LEASE_APPLY_KBN
        ''' <summary>
        ''' 有効前
        ''' </summary>
        Public Const KBN_INIT As String = "0"
        ''' <summary>
        ''' 有効
        ''' </summary>
        Public Const KBN_VALID As String = "1"
        ''' <summary>
        ''' 無効
        ''' </summary>
        Public Const KBN_INVALID As String = "2"
        ''' <summary>
        ''' 終了
        ''' </summary>
        Public Const KBN_END As String = "3"
    End Class
#End Region

#Region "リース計上状態"
    ''' <summary>
    ''' リース計上状態(計上区分とは別)
    ''' </summary>
    Public Class C_LEASE_KEIJOSTATUS
        ''' <summary>
        ''' 未計上
        ''' </summary>
        Public Const NOT_RECORDED As String = "0"
        ''' <summary>
        ''' 一部未計上
        ''' </summary>
        Public Const RECORDED_UNIT As String = "1"
        ''' <summary>
        ''' 計上済
        ''' </summary>
        Public Const RECORDED_ALL As String = "2"
        ''' <summary>
        ''' 新規
        ''' </summary>
        Public Const RECORDED_NEW As String = "3"
        ''' <summary>
        ''' 複写
        ''' </summary>
        Public Const RECORDED_COPY As String = "4"
    End Class
#End Region

#Region "リース情報区分"
    ''' <summary>
    ''' リース情報区分
    ''' </summary>
    Public Class C_LEASE_INFO_KBN
        ''' <summary>
        ''' 初回登録
        ''' </summary>
        Public Const UPD_INIT As String = "0"
        ''' <summary>
        ''' 更新【契約期間】
        ''' </summary>
        Public Const UPD_PERIOD As String = "1"
        ''' <summary>
        ''' 更新【リース額】
        ''' </summary>
        Public Const UPD_MONTHFEE As String = "2"
        ''' <summary>
        ''' 更新【契約期間・リース額】
        ''' </summary>
        Public Const UPD_ALL As String = "3"
    End Class
#End Region

#Region "締日入力フラグ"
    ''' <summary>
    ''' 締日入力フラグ
    ''' </summary>
    Public Class C_CLOSING_INPUT_FLG
        ''' <summary>
        ''' 末日
        ''' </summary>
        Public Const INPUT_FLG_OFF As String = "0"
        ''' <summary>
        ''' 手入力
        ''' </summary>
        Public Const INPUT_FLG_ON As String = ""
    End Class
#End Region

#Region "入金入力フラグ"
    ''' <summary>
    ''' 入金入力フラグ
    ''' </summary>
    Public Class C_DEPOSIT_INPUT_FLG
        ''' <summary>
        ''' 末日
        ''' </summary>
        Public Const INPUT_FLG_OFF As String = "0"
        ''' <summary>
        ''' 手入力
        ''' </summary>
        Public Const INPUT_FLG_ON As String = ""
    End Class
#End Region

#Region "自動更新フラグ"
    ''' <summary>
    ''' 自動更新フラグ
    ''' </summary>
    Public Class C_LEASE_AOUTSET_FLG
        ''' <summary>
        ''' しない
        ''' </summary>
        Public Const LEASE_AOUTSET_OFF As String = "0"
        ''' <summary>
        ''' する
        ''' </summary>
        Public Const LEASE_AOUTSET_ON As String = "1"
    End Class
#End Region

#Region "日割計算"
    ''' <summary>
    ''' 日割計算フラグ
    ''' </summary>
    Public Class C_LEASE_DAYCALC_FLG
        ''' <summary>
        ''' しない
        ''' </summary>
        Public Const LEASE_DAYCALC_OFF As String = "0"
        ''' <summary>
        ''' する
        ''' </summary>
        Public Const LEASE_DAYCALC_ON As String = "1"
    End Class
#End Region

#Region "改造費リース区分"
    ''' <summary>
    ''' 改造費リース区分
    ''' </summary>
    Public Class C_LEASE_REMODELLEASE_KBN
        ''' <summary>
        ''' なし
        ''' </summary>
        Public Const KBN_NONE As String = "0"
        ''' <summary>
        ''' 改造費リース
        ''' </summary>
        Public Const KBN_REMODELLEASE As String = "1"
    End Class
#End Region

#Region "メニュー縦のグループID"
    ''' <summary>
    ''' "メニュー縦のグループID(画面の表示順とは違うので気を付ける)
    ''' </summary>
    Public Class C_MENU_ROW_GROUP_ID
        ''' <summary>
        ''' 三大帳票
        ''' </summary>
        Public Const ROW_THREE_REPORT As String = "1"
        ''' <summary>
        ''' 営業日報
        ''' </summary>
        Public Const ROW_SALES_DAILY_REPORT As String = "2"
        ''' <summary>
        ''' レンタル　受注管理
        ''' </summary>
        Public Const ROW_RENTAL_ORDER As String = "3"
        ''' <summary>
        ''' レンタル　帳票
        ''' </summary>
        Public Const ROW_RENTAL_REPORT As String = "4"
        ''' <summary>
        ''' レンタル　マスタ管理①
        ''' </summary>
        Public Const ROW_RENTAL_MASTER_1 As String = "5"
        ''' <summary>
        ''' リース　受注管理(コンテナ部利用)
        ''' </summary>
        Public Const ROW_LEASE_ORDER_CTN As String = "6"
        ''' <summary>
        ''' リース　帳票
        ''' </summary>
        Public Const ROW_LEASE_REPORT As String = "7"
        ''' <summary>
        ''' 収入費用管理　請求経理締管理
        ''' </summary>
        Public Const ROW_PAY_BILLING As String = "9"
        ''' <summary>
        ''' 収入費用管理　帳票
        ''' </summary>
        Public Const ROW_PAY_REPORT As String = "10"
        ''' <summary>
        ''' システム管理　マスタ管理
        ''' </summary>
        Public Const ROW_SYSTEM_MASTER As String = "12"
        ''' <summary>
        ''' レンタル　マスタ管理②
        ''' </summary>
        Public Const ROW_RENTAL_MASTER_2 As String = "13"
        ''' <summary>
        ''' コンテナ販売　在庫管理
        ''' </summary>
        Public Const ROW_SALE_STOCK As String = "14"
        ''' <summary>
        ''' リース　受注管理(支店利用)
        ''' </summary>
        Public Const ROW_LEASE_ORDER_BRANCH As String = "15"

    End Class
#End Region

#Region "環境フラグ"
    ''' <summary>
    ''' 環境フラグ
    ''' </summary>
    Public Class C_ENVIRONMENTFLG
        ''' <summary>
        ''' ローカル環境
        ''' </summary>
        Public Const FLG_LOCAL As String = "0"
        ''' <summary>
        ''' 検証環境
        ''' </summary>
        Public Const FLG_KENSYO As String = "1"
        ''' <summary>
        ''' 本番環境
        ''' </summary>
        Public Const FLG_HONBAN As String = "2"
    End Class
#End Region

#Region "メール送信フラグ"
    ''' <summary>
    ''' リース申請　メール送信フラグ
    ''' </summary>
    Public Class C_LEASEAPPL_SENDFLG
        ''' <summary>
        ''' 送信しない
        ''' </summary>
        Public Const FLG_OFF As String = "0"
        ''' <summary>
        ''' 送信する
        ''' </summary>
        Public Const FLG_ON As String = "1"
    End Class
#End Region

End Module 'End BaseDllConst