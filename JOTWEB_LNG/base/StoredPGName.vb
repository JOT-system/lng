Option Strict On
''' <summary>
''' ストアドプログラム名の定数定義
''' </summary>
Public Module StoredPGName

    ''' <summary>
    ''' ストアドプログラム名
    ''' </summary>
    Public Class C_STORED_NAME
        ''' <summary>
        ''' １２５キロ賃率取得
        ''' </summary>
        Public Const CTN_GET_TINR2 As String = "LNG.GET_TINR2"

        ''' <summary>
        ''' 端数取得
        ''' </summary>
        Public Const CTN_GET_HASUU As String = "LNG.GET_HASUU"

        ''' <summary>
        ''' コンテナマスタ取得
        ''' </summary>
        Public Const CTN_GET_RECONM As String = "LNG.GET_RECONM"

        ''' <summary>
        ''' 運用状況表 現況表データ取得
        ''' </summary>
        Public Const CTN_GET_OPESITUATION_SHIPMENTSLIST As String = "LNG.GET_OPESITUATION_SHIPMENTSLIST"

        ''' <summary>
        ''' 運用状況表 収支表データ取得
        ''' </summary>
        Public Const CTN_GET_OPESITUATION_AMOUNTLIST As String = "LNG.GET_OPESITUATIONAMOUNT"

        ''' <summary>
        ''' 発送個数累計グラフデータ取得
        ''' </summary>
        Public Const CTN_GET_SHIPPINGDATA As String = "LNG.GET_SHIPPINGDATA"

        ''' <summary>
        ''' 現況表データ取得
        ''' </summary>
        Public Const CTN_GET_PRESENTSTATE As String = "LNG.GET_PRESENTSTATE"

        ''' <summary>
        ''' JOT店所コード取得処理(駅マスタ)
        ''' </summary>
        Public Const CTN_GET_JOTBRANCHCD As String = "LNG.GET_JOTBRANCHCD"

        ''' <summary>
        ''' レンタルシステム用リース物件マスタ取得処理
        ''' </summary>
        Public Const CTN_GET_LAMASM As String = "LNG.GET_LAMASM"

        ''' <summary>
        ''' GAL部門新旧変換マスタ取得処理
        ''' </summary>
        Public Const CTN_GET_EGMNOO As String = "LNG.GET_EGMNOO"

        ''' <summary>
        ''' コンテナ決済マスタ取得処理
        ''' </summary>
        Public Const CTN_GET_REKEJM As String = "LNG.GET_REKEJM"

        ''' <summary>
        ''' コード変換特例処理
        ''' </summary>
        Public Const CTN_UPD_CODECHANGE As String = "LNG.UPD_CODECHANGE"

        ''' <summary>
        ''' 計算屯数、割引番号、割増番号取得処理
        ''' </summary>
        Public Const CTN_GET_TONSU As String = "LNG.GET_TONSU"

        ''' <summary>
        ''' キロ程取得処理
        ''' </summary>
        Public Const CTN_GET_KIRO As String = "LNG.GET_KIRO"

        ''' <summary>
        ''' 賃率取得処理
        ''' </summary>
        Public Const CTN_GET_TINRT As String = "LNG.GET_TINRT"

        ''' <summary>
        ''' 適用率取得処理
        ''' </summary>
        Public Const CTN_GET_TEKRT As String = "LNG.GET_TEKRT"

        ''' <summary>
        ''' 基本料金計算処理
        ''' </summary>
        Public Const CTN_GET_KYOT As String = "LNG.GET_KYOT"

        ''' <summary>
        ''' 使用料金計算処理
        ''' </summary>
        Public Const CTN_GET_SHIY As String = "LNG.GET_SHIY"

        ''' <summary>
        ''' 回送費計算処理
        ''' </summary>
        Public Const CTN_GET_KAIS As String = "LNG.GET_KAIS"

        ''' <summary>
        ''' 税率取得処理
        ''' </summary>
        Public Const CTN_GET_ZERIT As String = "LNG.GET_ZERIT"

        ''' <summary>
        ''' リース登録用　リース明細登録・更新処理
        ''' </summary>
        Public Const CTN_UPD_LEASEDATA As String = "LNG.UPD_LEASEDATA"

        ''' <summary>
        ''' リース登録用　リース明細登録・更新処理
        ''' </summary>
        Public Const CTN_UPD_LEASEDATA_INVOICE As String = "LNG.UPD_LEASEDATA_INVOICE"

        ''' <summary>
        ''' リース登録用　リース明細チェック処理
        ''' </summary>
        Public Const CTN_CHK_LEASEDATA As String = "LNG.CHK_LEASEDATA"

        ''' <summary>
        ''' リース登録用　リースデータ削除処理
        ''' </summary>
        Public Const CTN_DEL_LEASEDATA As String = "LNG.DEL_LEASEDATA"

        ''' <summary>
        ''' 精算ファイル登録用　コンテナ精算ファイル(初回状態)作成処理
        ''' </summary>
        Public Const CTN_INS_RESSNFINITDATA As String = "LNG.INS_RESSNF_INIT"

        ''' <summary>
        ''' 精算ファイル登録用　計上日、入金日取得処理
        ''' </summary>
        Public Const CTN_GET_KEIJYONYUKINDATA As String = "LNG.GET_KEIJONYUKINDATE"

        ''' <summary>
        ''' 精算ファイル登録用　計上日、入金日取得処理(回送用)
        ''' </summary>
        Public Const CTN_GET_KEIJYONYUKINDATA_FREE As String = "LNG.GET_KEIJONYUKINDATE_FREE"

        ''' <summary>
        ''' リース判定処理
        ''' </summary>
        Public Const CTN_GET_LEASE_HANTEI As String = "LNG.GET_LEASE_HANTEI"

        ''' <summary>
        ''' リース登録用　精算ファイル更新処理
        ''' </summary>
        Public Const CTN_UPD_LEASERESSNF As String = "LNG.UPD_LEASED_RESSNF"

        ''' <summary>
        ''' 締用　リースデータ　計上済未計上処理
        ''' </summary>
        Public Const CTN_UPD_LEASEKEIZYO As String = "LNG.UPD_LEASE_KEIZYO"

        ''' <summary>
        ''' 賦金表 更新処理
        ''' </summary>
        Public Const CTN_CHG_LEVIESDATA As String = "LNG.CTN_CHG_LEVIESDATA"

    End Class

End Module 'End BaseDllConst