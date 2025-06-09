Option Strict On
''' <summary>
''' メッセージ関連の定数定義
''' </summary>
Public Module Messages

    ''' <summary>
    ''' メッセージタイプ
    ''' </summary>
    Public Class C_MESSAGE_TYPE
        ''' <summary>
        ''' Normal
        ''' </summary>
        Public Const NOR As String = "N"
        ''' <summary>
        ''' Info
        ''' </summary>
        Public Const INF As String = "I"
        ''' <summary>
        ''' Warning
        ''' </summary>
        Public Const WAR As String = "W"
        ''' <summary>
        ''' Error
        ''' </summary>
        Public Const ERR As String = "E"
        ''' <summary>
        ''' 異常
        ''' </summary>
        Public Const ABORT As String = "A"
        ''' <summary>
        ''' 確認
        ''' </summary>
        Public Const QUES As String = "Q"
    End Class

    ''' <summary>
    ''' メッセージNO
    ''' </summary>
    Public Class C_MESSAGE_NO
        ''' <summary>
        ''' 正常終了時
        ''' </summary>
        Public Const NORMAL As String = "00000"
        ''' <summary>
        ''' システム管理者へ連絡
        ''' </summary>
        Public Const SYSTEM_ADM_ERROR As String = "00001"
        ''' <summary>
        ''' DLL I/F エラー
        ''' </summary>
        Public Const DLL_IF_ERROR As String = "00002"
        ''' <summary>
        ''' DBエラー
        ''' </summary>
        Public Const DB_ERROR As String = "00003"
        ''' <summary>
        ''' File I/Oエラー
        ''' </summary>
        Public Const FILE_IO_ERROR As String = "00004"
        ''' <summary>
        ''' システム起動不能
        ''' </summary>
        Public Const SYSTEM_CANNOT_WAKEUP As String = "00005"
        ''' <summary>
        ''' EXCEL　OPENエラー
        ''' </summary>
        Public Const EXCEL_OPEN_ERROR As String = "00006"
        ''' <summary>
        ''' 型変換エラー
        ''' </summary>
        Public Const CAST_FORMAT_ERROR As String = "00007"
        ''' <summary>
        ''' ディレクトリ未存在
        ''' </summary>
        Public Const DIRECTORY_NOT_EXISTS_ERROR As String = "00008"
        ''' <summary>
        ''' ファイル未存在
        ''' </summary>
        Public Const FILE_NOT_EXISTS_ERROR As String = "00009"
        ''' <summary>
        ''' FIELD名アンマッチ
        ''' </summary>
        Public Const FIELD_NOT_FOUND_ERROR As String = "00010"
        ''' <summary>
        ''' 型変換エラー
        ''' </summary>
        Public Const CAST_FORMAT_ERROR_EX As String = "00011"
        ''' <summary>
        ''' FTP送信エラー
        ''' </summary>
        Public Const FILE_SEND_ERROR As String = "00012"
        ''' <summary>
        ''' ID　パスワード　入力依頼
        ''' </summary>
        Public Const INPUT_ID_PASSWD As String = "10000"
        ''' <summary>
        ''' ID　パスワード　誤入力
        ''' </summary>
        Public Const UNMATCH_ID_PASSWD_ERROR As String = "10001"
        ''' <summary>
        ''' パスワード　期限切れ期間が近い
        ''' </summary>
        Public Const PASSWORD_INVALID_AT_SOON As String = "10002"
        ''' <summary>
        ''' 権限エラー
        ''' </summary>
        Public Const AUTHORIZATION_ERROR As String = "10003"
        ''' <summary>
        ''' サービス停止
        ''' </summary>
        Public Const CLOSED_SERVICE As String = "10004"
        ''' <summary>
        ''' 書式エラー
        ''' </summary>
        Public Const FORMAT_ERROR As String = "10005"
        ''' <summary>
        ''' データ未選択エラー
        ''' </summary>
        Public Const NO_DATA_SELECT_ERROR As String = "10006"
        ''' <summary>
        ''' データ更新エラー（キー変更）
        ''' </summary>
        Public Const PRIMARY_KEY_NO_CHANGE_ERROR As String = "10007"
        ''' <summary>
        ''' マスタ未存在エラー
        ''' </summary>
        Public Const MASTER_NOT_FOUND_ERROR As String = "10008"
        ''' <summary>
        ''' データ重複登録エラー
        ''' </summary>
        Public Const ALREADY_UPDATE_ERROR As String = "10009"
        ''' <summary>
        ''' 印刷用EXCELファイル未存在エラー
        ''' </summary>
        Public Const REPORT_EXCEL_NOT_FOUND_ERROR As String = "10010"
        ''' <summary>
        ''' 帳票ID未存在エラー
        ''' </summary>
        Public Const REPORT_ID_NOT_EXISTS As String = "10011"
        ''' <summary>
        ''' INDEXサポートエラー
        ''' </summary>
        Public Const INDEX_SUPPORT_ERROR As String = "10012"
        ''' <summary>
        ''' 日付書式エラー
        ''' </summary>
        Public Const DATE_FORMAT_ERROR As String = "10013"
        ''' <summary>
        ''' 開始　終了　日付関連エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const START_END_DATE_RELATION_ERROR As String = "10014"
        ''' <summary>
        ''' データ未存在エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const NO_DATA_EXISTS_ERROR As String = "10015"
        ''' <summary>
        ''' 開始終了の関連エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const START_END_RELATION_ERROR As String = "10016"
        ''' <summary>
        ''' BOXエラー存在
        ''' </summary>
        ''' <remarks></remarks>
        Public Const BOX_ERROR_EXIST As String = "10018"
        ''' <summary>
        ''' 登録データ期間重複エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const UPDATE_DATA_RELATION_ERROR As String = "10019"
        ''' <summary>
        ''' 必須項目エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const PREREQUISITE_ERROR As String = "10020"
        ''' <summary>
        ''' 選択無効データ
        ''' </summary>
        Public Const INVALID_SELECTION_DATA As String = "10021"
        ''' <summary>
        ''' 追加可能データ未存在
        ''' </summary>
        Public Const REGISTRATION_RECORD_NOT_EXIST_ERROR As String = "10022"
        ''' <summary>
        ''' 追加不可データ
        ''' </summary>
        Public Const INVALID_REGIST_RECORD_ERROR As String = "10023"
        ''' <summary>
        ''' エラーレコード存在
        ''' </summary>
        Public Const ERROR_RECORD_EXIST As String = "10024"
        ''' <summary>
        ''' 表追加　正常終了
        ''' </summary>
        Public Const TABLE_ADDION_SUCCESSFUL As String = "10025"
        ''' <summary>
        ''' クリア　正常終了
        ''' </summary>
        Public Const DATA_CLEAR_SUCCESSFUL As String = "10026"
        ''' <summary>
        ''' 絞り込み　正常終了
        ''' </summary>
        Public Const DATA_FILTER_SUCCESSFUL As String = "10027"
        ''' <summary>
        ''' DB更新　正常終了
        ''' </summary>
        Public Const DATA_UPDATE_SUCCESSFUL As String = "10028"
        ''' <summary>
        ''' 更新不可データ
        ''' </summary>
        Public Const INVALID_UPDATE_RECORD_ERROR As String = "10029"
        ''' <summary>
        ''' インポートエラー
        ''' </summary>
        Public Const IMPORT_ERROR As String = "10030"
        ''' <summary>
        ''' パスワードの有効期限
        ''' </summary>
        Public Const PASSWORD_VALID_LIMIT As String = "10031"
        ''' <summary>
        ''' 再入力値不一致
        ''' </summary>
        Public Const REINPUT_DATA_UNMATCH_ERROR As String = "10032"
        ''' <summary>
        ''' 数値項目エラー
        ''' </summary>
        Public Const NUMERIC_VALUE_ERROR As String = "10033"
        ''' <summary>
        ''' 整数部桁数超過エラー
        ''' </summary>
        Public Const INTEGER_LENGTH_OVER_ERROR As String = "10034"
        ''' <summary>
        ''' 小数部桁数超過エラー
        ''' </summary>
        Public Const DECIMAL_LENGTH_OVER_ERROR As String = "10035"
        ''' <summary>
        ''' 文字数桁数超過エラー
        ''' </summary>
        Public Const STRING_LENGTH_OVER_ERROR As String = "10036"
        ''' <summary>
        ''' 数値範囲エラー
        ''' </summary>
        Public Const NUMBER_RANGE_ERROR As String = "10037"
        ''' <summary>
        ''' 明細未選択エラー
        ''' </summary>
        Public Const SELECT_DETAIL_ERROR As String = "10038"
        ''' <summary>
        ''' インポート成功
        ''' </summary>
        Public Const IMPORT_SUCCESSFUL As String = "10039"
        ''' <summary>
        ''' 明細表示　正常
        ''' </summary>
        Public Const DETAIL_VIEW_SUCCESSFUL As String = "10040"
        ''' <summary>
        ''' PDF情報は再読込
        ''' </summary>
        Public Const PDF_DATA_REVIEW_SUCCESSFUL As String = "10041"
        ''' <summary>
        ''' 他Excel処理完了待ち
        ''' </summary>
        Public Const WAIT_OTHER_EXCEL_JOB As String = "10042"
        ''' <summary>
        ''' 端末IDエラー
        ''' </summary>
        Public Const INVALID_TERMINAL_ID_ERROR As String = "10043"
        ''' <summary>
        ''' 無効な処理
        ''' </summary>
        Public Const INVALID_PROCCESS_ERROR As String = "10044"
        ''' <summary>
        ''' Excel書式定義エラー
        ''' </summary>
        Public Const EXCEL_COLUMNS_FORMAT_ERROR As String = "10045"
        ''' <summary>
        ''' 集計指定選択
        ''' </summary>
        Public Const SELECT_AGGREGATE_CONDITION As String = "10046"
        ''' <summary>
        ''' 警告レコード存在
        ''' </summary>
        Public Const WORNING_RECORD_EXIST As String = "10047"
        ''' <summary>
        ''' 保持時間超過エラー
        ''' </summary>
        Public Const OVER_RETENTION_PERIOD_ERROR As String = "10048"
        ''' <summary>
        ''' 他車庫の登録操作エラー
        ''' </summary>
        Public Const ANOTHER_SERVER_REGISTLATION_ERROR As String = "10049"
        ''' <summary>
        ''' 更新権限エラー
        ''' </summary>
        Public Const UPDATE_AUTHORIZATION_ERROR As String = "10050"
        ''' <summary>
        ''' 勤怠締後の変更エラー
        ''' </summary>
        Public Const OVER_CLOSING_DATE_ERROR As String = "10051"
        ''' <summary>
        ''' 重複データエラー
        ''' </summary>
        Public Const OVERLAP_DATA_ERROR As String = "10052"
        ''' <summary>
        '''EXCEL UPLOADエラー
        ''' </summary>
        Public Const EXCEL_UPLOAD_ERROR As String = "10053"

        ''' <summary>
        '''データ表示件数オーバー
        ''' </summary>
        Public Const DISPLAY_RECORD_OVER As String = "10054"

        ''' <summary>
        ''' 代行違反エラー
        ''' </summary>
        Public Const ACTING_LOGON_ERROR As String = "10055"

        ''' <summary>
        ''' パスワード誤り回数を超えた時のメッセージ
        ''' </summary>
        Public Const LOGIN_PSWNUM_ERROR As String = "10056"

        ''' <summary>
        ''' パスワード入力間違いの時のメッセージ 
        ''' </summary>
        Public Const LOGIN_PSWINPUT_ERROR As String = "10057"

        ''' <summary>
        ''' ＩＤ、パスワード入力間違いの時のメッセージ
        ''' </summary>
        Public Const LOGIN_IDPSW_ERROR As String = "10058"

        ''' <summary>
        ''' 閏年未存在
        ''' </summary>
        Public Const CTN_LEAPYEAR_NOTFOUND As String = "10059"

        ''' <summary>
        ''' 月日範囲エラー
        ''' </summary>
        Public Const CTN_MONTH_DAY_OVER_ERROR As String = "10060"

        ''' <summary>
        ''' 汎用メッセージ(?01。?02)
        ''' </summary>
        Public Const CTN_UNIVERSAL_MESSAGE As String = "10061"

        ''' <summary>
        ''' FTP接続エラー
        ''' </summary>
        Public Const FTP_CONNECT_ERROR As String = "11001"

        ''' <summary>
        ''' FTPファイル取得エラー
        ''' </summary>
        Public Const FTP_FILE_GET_ERROR As String = "11002"

        ''' <summary>
        ''' FTPファイル未存在
        ''' </summary>
        Public Const FTP_FILE_NOTFOUND As String = "11003"

        ''' <summary>
        ''' FTPファイルインポート成功
        ''' </summary>
        Public Const FTP_IMPORT_SUCCESSFUL As String = "11004"

        ''' <summary>
        ''' FTPファイル送信エラー
        ''' </summary>
        Public Const FTP_FILE_PUT_ERROR As String = "11005"

        ''' <summary>
        ''' FTPファイル送信データ件数不一致
        ''' </summary>
        Public Const FTP_RECORD_UNMATCH As String = "11006"

        ''' <summary>
        ''' FTPファイル送信成功
        ''' </summary>
        Public Const FTP_EXPORT_SUCCESSFUL As String = "11007"

        ''' <summary>
        ''' 登録・更新画面での「戻る」ボタン押下時確認メッセージ
        ''' </summary>
        Public Const UPDATE_CANCEL_CONFIRM As String = "12001"

        ''' <summary>
        ''' 登録・更新画面での「更新」ボタン押下時の変更なしエラー
        ''' </summary>
        Public Const NO_CHANGE_UPDATE As String = "12002"

        ''' <summary>
        ''' 帳票出力でのデータ未存在エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const NO_REPORT_DATA_EXISTS_ERROR As String = "12003"

        ''' <summary>
        ''' 項目入力エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const FIELD_INPUT_ERROR As String = "12004"

        ''' <summary>
        ''' 一意制約エラー
        ''' </summary>
        Public Const PRIMARYKEY_REPEAT_ERROR As String = "13001"

        ''' <summary>
        ''' 金額必須項目エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const PREREQUISITE_MONEY_ERROR As String = "20053"

        ''' <summary>
        ''' 承認者不在エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const RQACKNOWLEDGER_EXIST_ERROR As String = "20054"

        ''' <summary>
        ''' FREEメッセージ
        ''' </summary>
        Public Const OIL_FREE_MESSAGE As String = "20138"

        ''' <summary>
        ''' 削除行有効化メッセージ
        ''' </summary>
        Public Const DELETE_ROW_ACTIVATION As String = "20139"

#Region "コンテナ向け"

        ''' <summary>
        ''' コンテナ記号未選択
        ''' </summary>
        Public Const CTN_CTNTYPE_UNSELECT As String = "20001"

        ''' <summary>
        ''' １２５キロ賃率取得エラー
        ''' </summary>
        Public Const CTN_GET_TINR2_ERR As String = "20002"

        ''' <summary>
        ''' 端数取得エラー
        ''' </summary>
        Public Const CTN_GET_HASUU_ERR As String = "20003"

        ''' <summary>
        ''' 受注キャンセルしますよろしいですか？
        ''' </summary>
        Public Const CTN_CONFIRM_CANCEL_ORDER As String = "20004"

        ''' <summary>
        ''' キャンセルデータ未存在
        ''' </summary>
        Public Const CTN_CANCELDATA_NOTFOUND As String = "20005"

        ''' <summary>
        ''' キャンセル行未存在
        ''' </summary>
        Public Const CTN_CANCELLINE_NOTFOUND As String = "20006"

        ''' <summary>
        ''' 削除行未存在
        ''' </summary>
        Public Const CTN_DELLINE_NOTFOUND As String = "20007"

        ''' <summary>
        ''' 一意制約エラー
        ''' </summary>
        Public Const CTN_PRIMARYKEY_REPEAT_ERROR As String = "20008"

        ''' <summary>
        ''' 削除データ未存在
        ''' </summary>
        Public Const CTN_DELDATA_NOTFOUND As String = "20009"

        ''' <summary>
        ''' 受注がキャンセルされているため選択できません。
        ''' </summary>
        Public Const CTN_CANCEL_ENTRY_ORDER As String = "20010"

        ''' <summary>
        ''' コンテナマスタ取得エラー
        ''' </summary>
        Public Const CTN_GET_RECONM_ERR As String = "20011"

        ''' <summary>
        ''' JOT店所コード取得処理(駅マスタ)エラー
        ''' </summary>
        Public Const CTN_GET_JOTBRANCHCD_ERR As String = "20012"

        ''' <summary>
        ''' レンタルシステム用リース物件マスタ取得処理エラー
        ''' </summary>
        Public Const CTN_GET_LAMASM_ERR As String = "20013"

        ''' <summary>
        ''' GAL部門新旧変換マスタ取得処理エラー
        ''' </summary>
        Public Const CTN_GET_EGMNOO_ERR As String = "20014"

        ''' <summary>
        ''' コンテナ決済マスタ取得処理エラー
        ''' </summary>
        Public Const CTN_GET_REKEJM_ERR As String = "20015"

        ''' <summary>
        ''' コード変換特例処理エラー
        ''' </summary>
        Public Const CTN_UPD_CODECHANGE_ERR As String = "20016"

        ''' <summary>
        ''' 計算屯数、割引番号、割増番号取得処理エラー
        ''' </summary>
        Public Const CTN_TONSU_ERR As String = "20017"

        ''' <summary>
        ''' キロ程取得処理エラー
        ''' </summary>
        Public Const CTN_KIRO_ERR As String = "20018"

        ''' <summary>
        ''' 賃率取得処理エラー
        ''' </summary>
        Public Const CTN_TINRT_ERR As String = "20019"

        ''' <summary>
        ''' 適用率取得処理エラー
        ''' </summary>
        Public Const CTN_TEKRT_ERR As String = "20020"

        ''' <summary>
        ''' 基本料金計算処理エラー
        ''' </summary>
        Public Const CTN_KYOT_ERR As String = "20021"

        ''' <summary>
        ''' 使用料金計算処理エラー
        ''' </summary>
        Public Const CTN_SHIY_ERR As String = "20022"

        ''' <summary>
        ''' 回送費計算処理エラー
        ''' </summary>
        Public Const CTN_KAIS_ERR As String = "20023"

        ''' <summary>
        ''' 受注データ重複エラー
        ''' </summary>
        Public Const CTN_ORDER_REPEAT As String = "20024"

        ''' <summary>
        ''' 受注データ未存在エラー
        ''' </summary>
        Public Const CTN_ORDER_NONE As String = "20025"

        ''' <summary>
        ''' 受注明細データ未存在エラー
        ''' </summary>
        Public Const CTN_ORDERDETAIL_NONE As String = "20026"

        ''' <summary>
        ''' 税率取得処理エラー
        ''' </summary>
        Public Const CTN_ZERIT_ERR As String = "20027"

        ''' <summary>
        ''' 排他データエラー
        ''' </summary>
        Public Const CTN_HAITA_DATA_ERROR As String = "20028"

        ''' <summary>
        ''' 規定値未入力エラー
        ''' </summary>
        Public Const CTN_KOBANCYCLE_ERR As String = "20029"

        ''' <summary>
        ''' 警告メッセージ存在エラー
        ''' </summary>
        Public Const CTN_KEIKOKUMS_ERR As String = "20030"

        ''' <summary>
        ''' リース明細データ未存在エラー
        ''' </summary>
        Public Const CTN_LEASEDETAIL_NONE As String = "20031"

        ''' <summary>
        ''' 添付ファイル最大数超えメッセージ
        ''' </summary>
        Public Const ATTACHMENT_COUNTOVER As String = "20032"

        ''' <summary>
        ''' 過去日は入力できません。メッセージ
        ''' </summary>
        Public Const CTN_DATE_PAST As String = "20033"

        ''' <summary>
        ''' リース登録 チェック処理エラー
        ''' </summary>
        Public Const CTN_LEASE_CHECK_ERR As String = "20034"

        ''' <summary>
        ''' リース登録 登録処理エラー
        ''' </summary>
        Public Const CTN_LEASE_UPD_ERR As String = "20035"

        ''' <summary>
        ''' リース登録 削除処理エラー
        ''' </summary>
        Public Const CTN_LEASE_DEL_ERR As String = "20036"

        ''' <summary>
        ''' リース登録 既に計上済みの為、削除することは出来ません。
        ''' </summary>
        Public Const CTN_LEASE_DELEND_ERR As String = "20037"

        ''' <summary>
        ''' 範囲エラー
        ''' </summary>
        Public Const CTN_DATE_UPDSTART As String = "20038"

        ''' <summary>
        ''' 入力された内容ではファイナンスリースとして登録できません。
        ''' </summary>
        Public Const CTN_LEASE_FINAL_NOTREGIST As String = "20039"

        ''' <summary>
        ''' 契約形態が「ファイナンス」の場合、月初日以外は設定できません。
        ''' </summary>
        Public Const CTN_LEASE_FINAL_MONTHFIRST As String = "20040"

        ''' <summary>
        ''' 契約形態が「ファイナンス」の場合、月末日以外は設定できません。
        ''' </summary>
        Public Const CTN_LEASE_FINAL_MONTHEND As String = "20041"

        ''' <summary>
        ''' 契約形態が「ファイナンス」の場合、1年未満は設定できません。
        ''' </summary>
        Public Const CTN_LEASE_FINAL_YEARS As String = "20042"

        ''' <summary>
        ''' 初回の締日に過去日が設定されてます。リース開始日、又は、計上月を確認してください。
        ''' </summary>
        Public Const CTN_LEASE_CLOSINGERR As String = "20043"

        ''' <summary>
        ''' [確認依頼]依頼中請求書存在チェック
        ''' </summary>
        Public Const CTN_REQUEST_EXIST As String = "20044"

        ''' <summary>
        ''' [確認依頼]承認済み請求書存在チェック
        ''' </summary>
        Public Const CTN_APPROVAL_EXIST As String = "20045"

        ''' <summary>
        ''' [確認依頼]依頼中請求書存在チェック(警告メッセージ)
        ''' </summary>
        Public Const CTN_REQUEST_WARN As String = "20046"

        ''' <summary>
        ''' 請求書存在チェック
        ''' </summary>
        Public Const CTN_INVOICE_NONE As String = "20047"

        ''' <summary>
        ''' [差し戻し]担当者同一チェック
        ''' </summary>
        Public Const CTN_REQUEST_SAME As String = "20048"

        ''' <summary>
        ''' 選択データ存在チェック
        ''' </summary>
        Public Const CTN_SELECT_EXIST As String = "20049"

        ''' <summary>
        ''' [差し戻し]確認依頼チェック
        ''' </summary>
        Public Const CTN_REQUEST_NONE As String = "20050"

        ''' <summary>
        ''' 選択可能件数は最大?01件までです。
        ''' </summary>
        Public Const CTN_SEL_CNTMAX As String = "20051"

        ''' <summary>
        ''' 営業日報のデータが存在しません。
        ''' </summary>
        Public Const CTN_SEL_NOTDATA As String = "20052"

        ''' <summary>
        ''' 承認者へ確認依頼を出しました。
        ''' </summary>
        Public Const CTN_APPROVAL_REQUEST As String = "20055"

        ''' <summary>
        ''' 承認をしました。
        ''' </summary>
        Public Const CTN_APPROVAL_SUCCESSFUL As String = "20056"

        ''' <summary>
        ''' 申請を取り戻しました。
        ''' </summary>
        Public Const CTN_REQUEST_CANCEL As String = "20057"

        ''' <summary>
        ''' 差戻をしました。
        ''' </summary>
        Public Const CTN_SENDBACK_SUCCESSFUL As String = "20058"

        ''' <summary>
        ''' 初期データの読み込みに失敗しました。システム管理者へ連絡して下さい。
        ''' </summary>
        Public Const CTN_INITIAL_ERROR As String = "20059"

        ''' <summary>
        ''' データ連携に失敗しました。システム管理者へ連絡して下さい。
        ''' </summary>
        Public Const CTN_ALIGNMENT_ERROR As String = "20060"

        ''' <summary>
        ''' データ取得に失敗しました。システム管理者へ連絡して下さい。
        ''' </summary>
        Public Const CTN_ACQUISITION_ERROR As String = "20061"

        ''' <summary>
        ''' 新規の行を選択している為、複写することは出来ません。
        ''' </summary>
        Public Const CTN_LEASENEWROW_ERROR As String = "20062"

        ''' <summary>
        ''' 自動更新を設定している行を選択している為、複写することは出来ません。
        ''' </summary>
        Public Const CTN_LEASEAUTOCALC_ERROR As String = "20063"

        ''' <summary>
        ''' 入力した内容の反映先の明細を、チェックして選択して下さい。
        ''' </summary>
        Public Const CTN_LEASEDETAILCH_ERROR As String = "20064"

        ''' <summary>
        ''' 更新対象の明細を、チェックして選択して下さい。
        ''' </summary>
        Public Const CTN_UPDCHK_ERROR As String = "20065"

        ''' <summary>
        ''' 入力したコンテナ番号・契約開始日は既に登録されています。
        ''' </summary>
        Public Const CTN_LEASESTARTYMD_ERROR As String = "20066"

        ''' <summary>
        ''' リースの登録が完了しました。
        ''' </summary>
        Public Const CTN_LEASEINSERT_ERROR As String = "20067"

        ''' <summary>
        ''' 削除処理に失敗しました。システム管理者へ連絡して下さい。( ?01 )
        ''' </summary>
        Public Const CTN_DELETE_ERROR As String = "20068"

        ''' <summary>
        ''' リース登録はコンテナ部のみ登録・更新が可能です。
        ''' </summary>
        Public Const CTN_LEASECTN_ERROR As String = "20069"

        ''' <summary>
        ''' 追加対象の明細を、チェックして選択して下さい。
        ''' </summary>
        Public Const CTN_ADDCHK_ERROR As String = "20070"

        ''' <summary>
        ''' 削除対象の明細を、チェックして選択して下さい。
        ''' </summary>
        Public Const CTN_DELCHK_ERROR As String = "20071"

        ''' <summary>
        ''' 削除確認
        ''' </summary>
        Public Const CTN_DELETE_CHK As String = "20072"

        ''' <summary>
        ''' [金額追加]依頼中請求書存在チェック
        ''' </summary>
        Public Const CTN_REQUEST_EXIST_ADD As String = "20073"
        
        ''' <summary>
        ''' 未選択エラー
        ''' </summary>
        Public Const NOT_SELECT_ERROR As String = "20074"

        ''' <summary>
        ''' 連携失敗レコードあり
        ''' </summary>
        Public Const INVOICE_RENKEI_NG_ERROR As String = "20075"

        ''' <summary>
        ''' 処理完了
        ''' </summary>
        Public Const INVOICE_DOWNLOAD_SUCCESSFUL As String = "20076"

        ''' <summary>
        ''' 出力先請求書区分変更チェック
        ''' </summary>
        Public Const CTN_ALL_CHGRENTAL As String = "20077"

        ''' <summary>
        '''出力先請求書区分変更チェック
        ''' </summary>
        Public Const CTN_ALL_CHGLEASE As String = "20078"

        ''' <summary>
        '''出力先請求書区分変更チェック
        ''' </summary>
        Public Const CTN_OUTINV_RENTCHK As String = "20079"

        ''' <summary>
        '''出力先請求書区分変更チェック
        ''' </summary>
        Public Const CTN_OUTINV_LEASECHK As String = "20080"

        ''' <summary>
        '''行削除メッセージ
        ''' </summary>
        Public Const CTN_INFO_DELETE As String = "20081"

        ''' <summary>
        '''保存メッセージ
        ''' </summary>
        Public Const CTN_INFO_SAVE As String = "20082"

        ''' <summary>
        ''' 確認依頼チェック
        ''' </summary>
        Public Const CTN_REQUEST_CHK As String = "20083"

        ''' <summary>
        ''' 却下経理締め後エラー
        ''' </summary>
        Public Const CTN_REJECT_ERROR As String = "20084"

        ''' <summary>
        ''' 次の契約のリース開始日までに空白の期間が存在します。リース開始日を確認してください。
        ''' </summary>
        Public Const CTN_LEASE_FINAL_KIKAN As String = "20085"

        ''' <summary>
        ''' 開始日付、終了日付が逆転しています。
        ''' </summary>
        Public Const CTN_LEASE_FINAL_DATEFROMTO As String = "20086"

        ''' <summary>
        '''出力先請求書区分変更チェック
        ''' </summary>
        Public Const CTN_OUTINV_WRITECHK As String = "20088"

        ''' <summary>
        '''選択された行は追加明細では無い為、訂正は行えません。
        ''' </summary>
        Public Const CTN_ERR_ADDAMT As String = "20089"

        ''' <summary>
        '''データの読み込みに失敗しました。システム管理者へ連絡して下さい。(?01)
        ''' </summary>
        Public Const CTN_READDATA_ERR As String = "20090"

        ''' <summary>
        '''データの登録に失敗しました。システム管理者へ連絡して下さい。(?01)
        ''' </summary>
        Public Const CTN_INSDATA_ERR As String = "20091"

        ''' <summary>
        '''CSVファイルの作成に失敗しました。システム管理者へ連絡して下さい。(?01)
        ''' </summary>
        Public Const CTN_CRTCSV_ERR As String = "20092"

        ''' <summary>
        '''請求連携に失敗しました。システム管理者へ連絡して下さい。(?01)
        ''' </summary>
        Public Const CTN_COOP_ERR As String = "20093"

        ''' <summary>
        '''このメニューは利用出来ません。（参照権限設定無し）
        ''' </summary>
        Public Const CTN_ACSAUTHORIZATION_MENU_ERR As String = "20094"

        ''' <summary>
        '''自支店以外のデータは参照出来ません。（参照権限設定無し）
        ''' </summary>
        Public Const CTN_ACSAUTHORIZATION_ONLY_ERR As String = "20095"

        ''' <summary>
        '''このデータの変更は行えません。（更新権限設定無し）
        ''' </summary>
        Public Const CTN_UPDAUTHORIZATION_ERR As String = "20096"

        ''' <summary>
        '''?01を選択して下さい。
        ''' </summary>
        Public Const CTN_INPUT_ERR As String = "20097"

        ''' <summary>
        '''操作する明細を、チェックして選択して下さい。
        ''' </summary>
        Public Const CTN_SELECT_DETAIL_ERR As String = "20098"

        ''' <summary>
        '''請求連携が完了しました。
        ''' </summary>
        Public Const CTN_COOP_SUCCESSFUL As String = "20099"

        ''' <summary>
        '''「承認済」のみ請求連携が可能です。
        ''' </summary>
        Public Const CTN_COOP_STATUS_ERR As String = "20100"

        ''' <summary>
        '''選択した請求書種類は既に?01です。
        ''' </summary>
        Public Const CTN_STATUS_ERR As String = "20101"

        ''' <summary>
        '''他の担当者の請求書の為、取り下げは行えません。
        ''' </summary>
        Public Const CTN_CANCEL_RQSTAFF_ERR As String = "20102"

        ''' <summary>
        '''「未依頼」「取り下げ」のみ申請を行えます。
        ''' </summary>
        Public Const CTN_REQUEST_STATUS_ERR As String = "20103"

        ''' <summary>
        '''「申請中」のみ?01に更新出来ます。
        ''' </summary>
        Public Const CTN_UPD_REQUEST_STATUS_ERR As String = "20104"

        ''' <summary>
        '''自支店のデータのみ?01が可能です。
        ''' </summary>
        Public Const CTN_UPDAUTHORIZATION_ONLY_ERR As String = "20105"

        ''' <summary>
        '''既に?01の為、変更は行えません。?02を行って下さい。
        ''' </summary>
        Public Const CTN_ADDAMOUNT_ERR As String = "20106"

        ''' <summary>
        '''?01の場合、勘定科目に「その他販売収入」または「元請輸送」を選択して下さい。
        ''' </summary>
        Public Const CTN_ACCOUNTCODE_MIS_ERR As String = "20107"

        ''' <summary>
        '''?01の場合、勘定科目に「その他販売収入」または「元請輸送」以外を選択して下さい。
        ''' </summary>
        Public Const CTN_ACCOUNTCODE_ERR As String = "20108"

        ''' <summary>
        '''契約開始日・終了日を確認して下さい。期間指定に誤りがあります。
        ''' </summary>
        Public Const CTN_LEASEDATE_DURATION_ERR As String = "20109"

        ''' <summary>
        '''「申請中」のみ?01を行えます。
        ''' </summary>
        Public Const CTN_CANCEL_STATUS_ERR As String = "20110"

        ''' <summary>
        '''?01を入力して下さい。
        ''' </summary>
        Public Const CTN_INPUT_FILED_ERR As String = "20111"

        ''' <summary>
        '''?01には数値を入力して下さい。
        ''' </summary>
        Public Const CTN_INPUT_NUM_ERR As String = "20112"

        ''' <summary>
        '''入力した日付に誤りがあります。
        ''' </summary>
        Public Const CTN_INPUT_DATE_ERR As String = "20113"

        ''' <summary>
        '''他の確認者の請求書の為、?01は行えません。
        ''' </summary>
        Public Const CTN_RQACKNOWLEDGER_ERR As String = "20114"

        ''' <summary>
        '''確定状態を変更します。よろしいですか。
        ''' </summary>
        Public Const CTN_CONFIRM_CHK As String = "20115"

        ''' <summary>
        '''選択された請求先、決済条件は既に一覧画面に存在します。一覧画面から処理を行ってください。
        ''' </summary>
        Public Const CTN_LIST_EXIST As String = "20116"

        ''' <summary>
        '''連携可能な請求情報がありませんでした。
        ''' </summary>
        Public Const CTN_COOP_DATA_ERR As String = "20117"

        ''' <summary>
        '''原価計算が完了しました。
        ''' </summary>
        Public Const CTN_CALC_SUCCESSFUL As String = "20118"

        ''' <summary>
        '''原価を確定し、次月在庫を作成しました。
        ''' </summary>
        Public Const CTN_CONFIRM_SUCCESSFUL As String = "20119"

        ''' <summary>
        '''原価を未確定に戻しました。
        ''' </summary>
        Public Const CTN_UNCONFIRM_SUCCESSFUL As String = "20120"

        ''' <summary>
        '''入力した?01が?02に存在しません。
        ''' </summary>
        Public Const CTN_NON_EXISTENCE As String = "20121"

        ''' <summary>
        '''?01を削除し、前月に戻します。よろしいですか？
        ''' </summary>
        Public Const CTN_INFO_UNCONFIRM As String = "20122"

        ''' <summary>
        '''現在の明細の内容で原価計算を行い、在庫を次月に繰り越します。よろしいですか？
        ''' </summary>
        Public Const CTN_INFO_CONFIRM As String = "20123"

        ''' <summary>
        '''備考欄に , は入力できません。
        ''' </summary>
        Public Const CTN_INPUT_CONMA As String = "20124"

        ''' <summary>
        '''申請が完了しました。収入管理で確認を行ってください。
        ''' </summary>
        Public Const CTN_INFO_REQUEST As String = "20125"

        ''' <summary>
        '''既にリース登録済の請求先の為、リース新規申請できません。
        ''' </summary>
        Public Const CTN_LEASEAPPL_EXIST_ERR As String = "20126"

        ''' <summary>
        ''' コンテナ情報のコンテナ記号、コンテナ番号を1件以上入力して下さい。
        ''' </summary>
        Public Const CTN_LEASEAPPL_UPDCHK_ERROR As String = "20127"

        ''' <summary>
        ''' リース新規申請のデータが存在しません。
        ''' </summary>
        Public Const CTN_LEASEAPPL_NOTDATA_ERR As String = "20128"

        ''' <summary>
        ''' リース未登録の請求先の為、リース変更申請できません。
        ''' </summary>
        Public Const CTN_LEASEAPPL_NOTLEASEDATA_ERR As String = "20129"

        ''' <summary>
        ''' リース変更申請のデータが存在しません。
        ''' </summary>
        Public Const CTN_LEASEAPPL_NOTUPDDATA_ERR As String = "20130"

        ''' <summary>
        ''' 有効期間重複エラーです。重複する期間の開始日・終了日を調整をしてください。
        ''' </summary>
        Public Const CTN_OVERLAPPERIODS_ERR As String = "20131"

        ''' <summary>
        ''' 入力値エラーです。日付では無い値が入力されている為、修正をしてください。
        ''' </summary>
        Public Const CTN_OVERLAPPERIODS_NOTDATE_ERR As String = "20132"

        ''' <summary>
        ''' 過去日エラーです。今回入力の終了日が過去日の為、未来日に修正してください。
        ''' </summary>
        Public Const CTN_OVERLAPPERIODS_PASTDATE_ERR As String = "20133"

        ''' <summary>
        ''' 楽々販売への送信は完了しましたがSyRIUSの送信後処理でエラーが起きました。情報システム部へご連絡ください。
        ''' </summary>
        Public Const CTN_ERR_INVOICERENKEI_DBUPDATE As String = "20134"

        ''' <summary>
        ''' 楽々販売への送信に失敗しました。情報システム部へご連絡ください。
        ''' </summary>
        Public Const CTN_ERR_INVOICERENKEI_DATASEND As String = "20135"

        ''' <summary>
        ''' コンテナ決済マスタに取引先コードが設定されていない発受託人が選択されています。請求・支払の対象外となりますが、よろしいですか？
        ''' </summary>
        Public Const CTN_INSERT_CHK As String = "20136"

        ''' <summary>
        ''' 送信するには送信チェックにチェックを入れてください。
        ''' </summary>
        Public Const CTN_ERR_PAYEELINK_CHK As String = "20137"

#End Region


        Shared Function REPORTID() As String
            Throw New NotImplementedException
        End Function

    End Class
    ''' <summary>
    ''' メッセージの固定文字列
    ''' </summary>
    ''' <remarks></remarks>
    Public Class C_MESSAGE_TEXT
        ''' <summary>
        ''' パラメータエラーによるシステム管理者に問い合わせのメッセージ
        ''' </summary>
        ''' <remarks></remarks>
        Public Const IN_PARAM_ERROR_TEXT As String = "システム管理者へ連絡して下さい(In PARAM Err)"
        ''' <summary>
        ''' 選択無効エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const SELECT_INVALID_VALUE_ERROR As String = "選択不可能な値です。"
        ''' <summary>
        ''' 日付書式エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const DATE_FORMAT_ERROR_TEXT As String = "日付を入力してください。"
        ''' <summary>
        ''' 日付超過エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const DATE_MAX_OVER_ERROR_TEXT As String = "最大日付超（最大：2099/12/31）エラー"
        ''' <summary>
        ''' 時刻書式エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const TIME_FORMAT_ERROR_TEXT As String = "時刻を入力してください。"
        ''' <summary>
        ''' 時刻書式エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const TIME_FORMAT_SPLIT_ERROR_TEXT As String = "分単位で入力してください。"
        ''' <summary>
        ''' 必須項目時のエラーメッセージ
        ''' </summary>
        ''' <remarks></remarks>
        Public Const PREREQUISITE_ERROR_TEXT As String = "必須入力です。"
        ''' <summary>
        ''' 数値項目エラーメッセージ
        ''' </summary>
        ''' <remarks></remarks>
        Public Const NUMERIC_ERROR_TEXT As String = "数値を入力してください。"
        ''' <summary>
        ''' 整数部桁数超過エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const INTEGER_LENGTH_OVER_ERROR_TEXT As String = "整数桁数エラー"
        ''' <summary>
        ''' 小数部桁数超過エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const DECIMAL_LENGTH_OVER_ERROR_TEXT As String = "少数桁数エラー"
        ''' <summary>
        ''' 小数部桁数超過エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const DECIMAL_ERROR_TEXT As String = "少数は入力できません。"
        ''' <summary>
        ''' 文字数超過エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const STRING_LENGTH_OVER_ERROR_TEXT1 As String = "桁数が大き過ぎます。"
        Public Const STRING_LENGTH_OVER_ERROR_TEXT2 As String = "桁以内で入力して下さい。"
        ''' <summary>
        ''' 固定桁数数エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const INTEGER_LENGTH_FIXED_ERROR_TEXT As String = "桁で入力して下さい。"
    End Class
    ''' <summary>
    ''' メッセージNOが正常終了か判定する
    ''' </summary>
    ''' <param name="message">判定するメッセージNO</param>
    ''' <param name="O_RTN" >成否判定　TRUE：正常終了　FALSE：それ以外</param>
    ''' <returns>成否判定　TRUE：正常終了　FALSE：それ以外</returns>
    ''' <remarks></remarks>
    Public Function isNormal(ByVal message As String, Optional ByRef O_RTN As String = "TRUE") As Boolean

        If message = C_MESSAGE_NO.NORMAL Then
            isNormal = True
            If Not O_RTN Is Nothing Then
                O_RTN = "TRUE"
            End If
        Else
            isNormal = False
            If Not O_RTN Is Nothing Then
                O_RTN = "FALSE"
            End If
        End If
    End Function
End Module 'End BaseDllConst