''************************************************************
' 賦金表一覧登録
' 作成日 2022/05/26
' 更新日 
' 作成者 名取
' 更新者 
'
' 修正履歴 : 2022/05/26 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient

''' <summary>
''' 賦金表一覧登録
''' </summary>
''' <remarks></remarks>
Public Class CreateLeviesList

    ''' <summary>
    ''' 賦金表計算
    ''' </summary>
    ''' <param name="LeaseNo"></param>              リース登録番号
    ''' <param name="CtnType"></param>              コンテナ記号
    ''' <param name="CtnNo"></param>                コンテナ番号
    ''' <param name="UserID"></param>               ユーザーID
    ''' <param name="ApsvID"></param>               端末ID
    ''' <param name="ProgramID"></param>            プラグラムID
    ''' <param name="SQLcon"></param>               DataBase接続情報
    ''' <param name="sqlTran"></param>              SQLトランザクション
    ''' <param name="DiscountPresentValue"></param> 割引現在価値
    ''' <param name="ErrCode"></param>              エラーコード
    Public Shared Sub CalculationLevies(ByVal LeaseNo As String, ByVal CtnType As String, ByVal CtnNo As String,
                                        ByVal UserID As String, ByVal ApsvID As String, ByVal ProgramID As String,
                                        ByVal SQLcon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                        ByVal DiscountPresentValue As Long, ByRef ErrCode As String)

        Dim CS0050SESSION As New CS0050SESSION                 'セッション情報操作処理
        Dim CS0011LOGWrite As New CS0011LOGWrite               'ログ出力
        Dim LeaseDetailtbl As New DataTable                    'リース明細テーブル

        Dim InvoiceOutOrgCd As String = ""                     '請求書出力先組織コード
        Dim KeijoOrgCd As String = ""                          '計上先組織コード
        Dim ToriCode As String = ""                            '請求取引先コード
        Dim LeaseStartYMD As Date                              'リース開始日
        Dim LeaseEndYMD As Date                                'リース終了日
        Dim PurchasePrice As Long                              '購入価格
        Dim RemodelingCost As Long                             '改造費
        Dim SurvivalRate As Decimal                            '残存率
        Dim LastMonthBalance As Long                           '前月末残
        Dim PayPrincipal As Long                               '支払元本
        Dim PayInterest As Long                                '支払利息
        Dim CurMonthBalance As Long                            '当月末残
        Dim TotalPaymentAmount As Long                         '合計支払元本
        Dim DataRowCount As Integer = 0                        'レコードカウント
        Dim RowCount As Integer = 0                            '実行カウント

        Try
            '○ 画面表示データ取得
            ' リースヘッダーデータ検索
            SelectLeaseApply(SQLcon, sqlTran, LeaseNo, LeaseStartYMD, LeaseEndYMD, PurchasePrice, RemodelingCost, SurvivalRate)
            ' リース詳細データ検索
            SelectLeaseData(SQLcon, sqlTran, LeaseNo, CtnType, CtnNo, LeaseDetailtbl)

            DataRowCount = LeaseDetailtbl.Rows.Count

            For Each LeaseDetailtblrow As DataRow In LeaseDetailtbl.Rows
                RowCount += 1

                If RowCount = 1 Then
                    LastMonthBalance = CLng(LeaseDetailtblrow("INITRESIDUAL"))
                End If

                ' 支払利息の算出((前月末残 × 利率) ÷ 12)
                PayInterest = CLng(Math.Floor(CDbl(LastMonthBalance * CDec(LeaseDetailtblrow("INTERESTRATE"))) / 12))

                ' 支払元本の算出(月額リース料 - 支払利息)
                PayPrincipal = CLng(LeaseDetailtblrow("MONTHLEASEFEE")) - PayInterest

                ' 当月末残の算出(前月末残 - 支払元本)
                CurMonthBalance = LastMonthBalance - PayPrincipal

                TotalPaymentAmount += PayPrincipal

                ' 支払額補正
                If DataRowCount = RowCount Then
                    ' 支払元本の再算出(当月支払元本 - 合計支払元本 + 初回前残 - 残存価格)
                    PayPrincipal = PayPrincipal - TotalPaymentAmount + CLng(LeaseDetailtblrow("INITRESIDUAL")) - CLng(LeaseDetailtblrow("RESIDUALPRICE"))

                    ' 支払利息の再算出(月額リース料 - 支払元本)
                    PayInterest = CLng(LeaseDetailtblrow("MONTHLEASEFEE")) - PayPrincipal

                    ' 当月末残の再算出(前月末残 - 支払元本)
                    CurMonthBalance = LastMonthBalance - PayPrincipal

                End If

                ' ファイナンスリース賦金テーブル登録
                CreateLevies(SQLcon, sqlTran, LeaseNo, CtnType, CtnNo,
                             LeaseDetailtblrow("INVOICEOUTORGCD"), LeaseDetailtblrow("KEIJOORGCD"),
                             LeaseDetailtblrow("TORICODE"), LeaseDetailtblrow("INACCOUNTCD"), LeaseDetailtblrow("TAXRATE"),
                             LeaseStartYMD, LeaseEndYMD, LeaseDetailtblrow("DEPOSITYMD"), LeaseDetailtblrow("KEIJOYM"),
                             PurchasePrice, RemodelingCost, SurvivalRate, CLng(LeaseDetailtblrow("RESIDUALPRICE")),
                             LastMonthBalance, CLng(LeaseDetailtblrow("MONTHLEASEFEE")), PayPrincipal,
                             PayInterest, CurMonthBalance, LeaseDetailtblrow("BEFOREINVOICEOUTORGCD"), LeaseDetailtblrow("BEFOREKEIJOORGCD"),
                             LeaseDetailtblrow("INTERESTRATE"), LeaseDetailtblrow("ECONOSERVICELIFE"),
                             LeaseDetailtblrow("PRESENTVALUE"), DiscountPresentValue,
                             UserID, ApsvID, ProgramID, ErrCode)

                ' 来月用値設定
                ' 前月末残
                LastMonthBalance = CurMonthBalance
            Next
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "賦金表計算 処理"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' リース適用データ検索
    ''' </summary>
    ''' <param name="SQLcon"></param>          DataBase接続情報
    ''' <param name="sqlTran"></param>         SQLトランザクション
    ''' <param name="LeaseNo"></param>         リース登録番号
    ''' <param name="LeaseStartYMD"></param>   リース開始日
    ''' <param name="LeaseEndYMD"></param>     リース終了日
    ''' <param name="PurchasePrice"></param>   購入価格（1個当たり）
    ''' <param name="RemodelingCost"></param>  改造費（総額）
    ''' <param name="SurvivalRate"></param>    残存率
    Protected Shared Sub SelectLeaseApply(ByVal SQLcon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                          ByVal LeaseNo As String, ByRef LeaseStartYMD As Date,
                                          ByRef LeaseEndYMD As Date, ByRef PurchasePrice As Long,
                                          ByRef RemodelingCost As Long, ByRef SurvivalRate As Decimal)
        '○ 共通関数宣言(BASEDLL)
        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力

        Dim LNT0011tbl As DataTable = New DataTable
        If LNT0011tbl.Columns.Count <> 0 Then
            LNT0011tbl.Columns.Clear()
        End If
        LNT0011tbl.Clear()

        '○ 検索SQL
        Dim SQLStr As String =
              " Select                                    " _
            & "     MIN(APPLYSTARTYMD)  AS LEASESTARTYMD  " _
            & "   , MAX(APPLYENDYMD)    AS LEASEENDYMD    " _
            & "   , MAX(PURCHASEPRICE)  AS PURCHASEPRICE  " _
            & "   , MAX(REMODELINGCOST) AS REMODELINGCOST " _
            & "   , MAX(SURVIVALRATE)   AS SURVIVALRATE   " _
            & " FROM                                      " _
            & "     LNG.LNT0041_LEASEAPPLY                " _
            & " WHERE                                     " _
            & "     LEASENO      = @P1                    " _
            & " AND CONTRALNTYPE = '2'                    "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon, sqlTran)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 11)  'リース登録番号
                PARA1.Value = LeaseNo

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0011tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0011tbl.Load(SQLdr)
                End Using
                For Each LNT0011tblrow As DataRow In LNT0011tbl.Rows
                    LeaseStartYMD = LNT0011tblrow("LEASESTARTYMD")
                    LeaseEndYMD = LNT0011tblrow("LEASEENDYMD")
                    PurchasePrice = CLng(LNT0011tblrow("PURCHASEPRICE"))
                    RemodelingCost = CLng(LNT0011tblrow("REMODELINGCOST"))
                    SurvivalRate = CDec(LNT0011tblrow("SURVIVALRATE"))
                    Exit For
                Next

            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0040 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' リース明細データ検索
    ''' </summary>
    ''' <param name="SQLcon"></param>               DataBase接続情報
    ''' <param name="sqlTran"></param>              SQLトランザクション
    ''' <param name="LeaseNo"></param>              リース登録番号
    ''' <param name="CtnType"></param>              コンテナ記号
    ''' <param name="CtnNo"></param>                コンテナ番号
    ''' <param name="LNT0011tbl"></param>           データテーブル
    Protected Shared Sub SelectLeaseData(ByVal SQLcon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                         ByVal LeaseNo As String, ByVal CtnType As String,
                                         ByVal CtnNo As String, ByRef LNT0011tbl As DataTable)
        '○ 共通関数宣言(BASEDLL)
        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力

        If IsNothing(LNT0011tbl) Then
            LNT0011tbl = New DataTable
        End If
        If LNT0011tbl.Columns.Count <> 0 Then
            LNT0011tbl.Columns.Clear()
        End If
        LNT0011tbl.Clear()

        '○ 検索SQL
        Dim SQLStr As String =
              " Select                                                         " _
            & "     KEIJOYM                                                    " _
            & "   , INVOICEOUTORGCD                                            " _
            & "   , KEIJOORGCD                                                 " _
            & "   , TORICODE                                                   " _
            & "   , INACCOUNTCD                                                " _
            & "   , TAXRATE                                                    " _
            & "   , MONTHLEASEFEE                                              " _
            & "   , RESIDUALPRICE                                              " _
            & "   , INITRESIDUAL                                               " _
            & "   , INTERESTRATE                                               " _
            & "   , ECONOSERVICELIFE                                           " _
            & "   , PRESENTVALUE                                               " _
            & "   , DEPOSITYMD                                                 " _
            & "   , coalesce(BEFOREINVOICEOUTORGCD, '') AS BEFOREINVOICEOUTORGCD " _
            & "   , coalesce(BEFOREKEIJOORGCD, '')      AS BEFOREKEIJOORGCD      " _
            & " FROM                                                           " _
            & "     LNG.LNT0042_LEASEDATA                                      " _
            & " WHERE                                                          " _
            & "     LEASENO = @P1                                              " _
            & " AND CTNTYPE = @P2                                              " _
            & " AND CTNNO   = @P3                                              " _
            & " ORDER BY                                                       " _
            & "     KEIJOYM                                                    "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon, sqlTran)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 11)  'リース登録番号
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 5)   'コンテナ記号
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 8)   'コンテナ番号
                PARA1.Value = LeaseNo
                PARA2.Value = CtnType
                PARA3.Value = CtnNo

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0011tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0011tbl.Load(SQLdr)
                End Using

            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0042 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' ファイナンスリース賦金テーブル登録・更新
    ''' </summary>
    ''' <param name="SQLcon"></param>                DataBase接続情報
    ''' <param name="sqlTran"></param>               SQLトランザクション
    ''' <param name="LeaseNo"></param>               リース登録番号
    ''' <param name="CtnType"></param>               コンテナ記号
    ''' <param name="CtnNo"></param>                 コンテナ番号
    ''' <param name="InvoiceOutOrgCd"></param>       請求書出力先組織コード
    ''' <param name="KeijoOrgCd"></param>            計上先組織コード
    ''' <param name="ToriCode"></param>              請求取引先コード
    ''' <param name="InAccountCd"></param>           社内口座コード
    ''' <param name="LeaseStartYMD"></param>         リース開始日
    ''' <param name="LeaseEndYMD"></param>           リース終了日
    ''' <param name="PayDate"></param>               支払日付(入金日)
    ''' <param name="KeijoYM"></param>               計上年月
    ''' <param name="PurchasePrice"></param>         購入価格
    ''' <param name="RemodelingCost"></param>        改造費
    ''' <param name="SurvivalRate"></param>          残存率
    ''' <param name="ResidualAmount"></param>        残存価額
    ''' <param name="LastMonthBalance"></param>      前月末残
    ''' <param name="MonthlyAmount"></param>         支払月額
    ''' <param name="PayPrincipal"></param>          支払元本
    ''' <param name="PayInterest"></param>           支払利息
    ''' <param name="CurMonthBalance"></param>       当月末残
    ''' <param name="BeforeInvoiceOutOrgCd"></param> 変換前請求書出力先組織コード
    ''' <param name="BeforeKeijoOrgCd"></param>      変換前計上先組織コード
    ''' <param name="InterestRate"></param>          利率
    ''' <param name="EconomicServiceLife"></param>   経済的耐用年数
    ''' <param name="PresentValue"></param>         現在価値
    ''' <param name="DiscountPresentValue"></param> 割引現在価値
    ''' <param name="UserID"></param>                ユーザーID
    ''' <param name="ApsvID"></param>                端末ID
    ''' <param name="ProgramID"></param>             プラグラムID
    ''' <param name="ErrCode"></param>               エラーコード
    Protected Shared Sub CreateLevies(ByVal SQLcon As MySqlConnection, ByVal sqlTran As MySqlTransaction,
                                      ByVal LeaseNo As String, ByVal CtnType As String, ByVal CtnNo As String,
                                      ByVal InvoiceOutOrgCd As String, ByVal KeijoOrgCd As String,
                                      ByVal ToriCode As String, ByVal InAccountCd As String, ByVal TaxRate As String,
                                      ByVal LeaseStartYMD As Date, ByVal LeaseEndYMD As Date,
                                      ByVal PayDate As String, ByVal KeijoYM As Integer,
                                      ByVal PurchasePrice As Long, ByVal RemodelingCost As Long,
                                      ByVal SurvivalRate As Decimal, ByVal ResidualAmount As Long,
                                      ByVal LastMonthBalance As Long, ByVal MonthlyAmount As Long,
                                      ByVal PayPrincipal As Long, ByVal PayInterest As Long,
                                      ByVal CurMonthBalance As Long, ByVal BeforeInvoiceOutOrgCd As String,
                                      ByVal BeforeKeijoOrgCd As String,
                                      ByVal InterestRate As String, ByVal EconomicServiceLife As Long,
                                      ByVal PresentValue As Long, ByVal DiscountPresentValue As Long,
                                      ByVal UserID As String, ByVal ApsvID As String,
                                      ByVal ProgramID As String, ByRef ErrCode As String)

        '○ 検索結果格納Table
        Dim CPT0011tbl As New DataTable                             '更新用テーブル

        '○ 共通関数宣言(BASEDLL)
        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        Dim CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
        Dim CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)

        '○ DB更新SQL(ファイナンスリース賦金テーブル)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;                 " _
            & "     SET @hensuu = 0 ;                       " _
            & " DECLARE hensuu CURSOR FOR                   " _
            & "     SELECT                                  " _
            & "         UPDTIMSTP AS hensuu                 " _
            & "     FROM                                    " _
            & "         LNG.LNT0065_FL_LEVIES               " _
            & "     WHERE                                   " _
            & "             LEASENO  = @P01                 " _
            & "         AND CTNTYPE  = @P02                 " _
            & "         AND CTNNO    = @P03                 " _
            & "         AND PAYDATE  = @P04                 " _
            & "         AND KEIJYOYM = @P05 ;               " _
            & " OPEN hensuu ;                               " _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;       " _
            & " IF (@@FETCH_STATUS = 0)                     " _
            & "     UPDATE LNG.LNT0065_FL_LEVIES            " _
            & "     SET                                     " _
            & "         DELFLG                = @P00        " _
            & "       , INVOICEOUTORGCD       = @P06        " _
            & "       , KEIJOORGCD            = @P07        " _
            & "       , TORICODE              = @P08        " _
            & "       , INACCOUNTCD           = @P09        " _
            & "       , TAXRATE               = @P10        " _
            & "       , LEASESTARTYMD         = @P11        " _
            & "       , LEASEENDYMD           = @P12        " _
            & "       , LEASEYEARS            = @P13        " _
            & "       , INTERESTRATE          = @P14        " _
            & "       , ECONOSERVICELIFE      = @P15        " _
            & "       , PRESENTVALUE          = @P16        " _
            & "       , DISCOUNTPRESENTVALUE  = @P17        " _
            & "       , PURCHASEPRICE         = @P18        " _
            & "       , REMODELINGCOST        = @P19        " _
            & "       , SURVIVALRATE          = @P20        " _
            & "       , RESIDUALPRICE         = @P21        " _
            & "       , MONTHLEASEFEE         = @P22        " _
            & "       , LASTMONTHBALANCE      = @P23        " _
            & "       , PAYMONTHLYAMOUNT      = @P24        " _
            & "       , PAYPRINCIPAL          = @P25        " _
            & "       , PAYINTEREST           = @P26        " _
            & "       , CURMONTHBALANCE       = @P27        " _
            & "       , BEFOREINVOICEOUTORGCD = @P28        " _
            & "       , BEFOREKEIJOORGCD      = @P29        " _
            & "       , UPDYMD                = @P34        " _
            & "       , UPDUSER               = @P35        " _
            & "       , UPDTERMID             = @P36        " _
            & "       , UPDPGID               = @P37        " _
            & "       , RECEIVEYMD            = @P38        " _
            & "     WHERE                                   " _
            & "             LEASENO  = @P01                 " _
            & "         AND CTNTYPE  = @P02                 " _
            & "         AND CTNNO    = @P03                 " _
            & "         AND PAYDATE  = @P04                 " _
            & "         AND KEIJYOYM = @P05 ;               " _
            & " IF (@@FETCH_STATUS <> 0)                    " _
            & "     INSERT INTO LNG.LNT0065_FL_LEVIES       " _
            & "        (DELFLG                              " _
            & "       , LEASENO                             " _
            & "       , CTNTYPE                             " _
            & "       , CTNNO                               " _
            & "       , PAYDATE                             " _
            & "       , KEIJYOYM                            " _
            & "       , INVOICEOUTORGCD                     " _
            & "       , KEIJOORGCD                          " _
            & "       , TORICODE                            " _
            & "       , INACCOUNTCD                         " _
            & "       , TAXRATE                             " _
            & "       , LEASESTARTYMD                       " _
            & "       , LEASEENDYMD                         " _
            & "       , LEASEYEARS                          " _
            & "       , INTERESTRATE                        " _
            & "       , ECONOSERVICELIFE                    " _
            & "       , PRESENTVALUE                        " _
            & "       , DISCOUNTPRESENTVALUE                " _
            & "       , PURCHASEPRICE                       " _
            & "       , REMODELINGCOST                      " _
            & "       , SURVIVALRATE                        " _
            & "       , RESIDUALPRICE                       " _
            & "       , MONTHLEASEFEE                       " _
            & "       , LASTMONTHBALANCE                    " _
            & "       , PAYMONTHLYAMOUNT                    " _
            & "       , PAYPRINCIPAL                        " _
            & "       , PAYINTEREST                         " _
            & "       , CURMONTHBALANCE                     " _
            & "       , BEFOREINVOICEOUTORGCD               " _
            & "       , BEFOREKEIJOORGCD                    " _
            & "       , INITYMD                             " _
            & "       , INITUSER                            " _
            & "       , INITTERMID                          " _
            & "       , INITPGID                            " _
            & "       , UPDYMD                              " _
            & "       , UPDUSER                             " _
            & "       , UPDTERMID                           " _
            & "       , UPDPGID                             " _
            & "       , RECEIVEYMD)                         " _
            & "     VALUES                                  " _
            & "        (@P00                                " _
            & "       , @P01                                " _
            & "       , @P02                                " _
            & "       , @P03                                " _
            & "       , @P04                                " _
            & "       , @P05                                " _
            & "       , @P06                                " _
            & "       , @P07                                " _
            & "       , @P08                                " _
            & "       , @P09                                " _
            & "       , @P10                                " _
            & "       , @P11                                " _
            & "       , @P12                                " _
            & "       , @P13                                " _
            & "       , @P14                                " _
            & "       , @P15                                " _
            & "       , @P16                                " _
            & "       , @P17                                " _
            & "       , @P18                                " _
            & "       , @P19                                " _
            & "       , @P20                                " _
            & "       , @P21                                " _
            & "       , @P22                                " _
            & "       , @P23                                " _
            & "       , @P24                                " _
            & "       , @P25                                " _
            & "       , @P26                                " _
            & "       , @P27                                " _
            & "       , @P28                                " _
            & "       , @P29                                " _
            & "       , @P30                                " _
            & "       , @P31                                " _
            & "       , @P32                                " _
            & "       , @P33                                " _
            & "       , @P34                                " _
            & "       , @P35                                " _
            & "       , @P36                                " _
            & "       , @P37                                " _
            & "       , @P38) ;                             " _
            & " CLOSE hensuu ;                              " _
            & " DEALLOCATE hensuu ;                         "

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                     " _
            & "     DELFLG                                 " _
            & "   , LEASENO                                " _
            & "   , CTNTYPE                                " _
            & "   , CTNNO                                  " _
            & "   , PAYDATE                                " _
            & "   , KEIJYOYM                               " _
            & "   , INVOICEOUTORGCD                        " _
            & "   , KEIJOORGCD                             " _
            & "   , TORICODE                               " _
            & "   , INACCOUNTCD                            " _
            & "   , TAXRATE                                " _
            & "   , LEASESTARTYMD                          " _
            & "   , LEASEENDYMD                            " _
            & "   , LEASEYEARS                             " _
            & "   , INTERESTRATE                           " _
            & "   , ECONOSERVICELIFE                       " _
            & "   , PRESENTVALUE                           " _
            & "   , DISCOUNTPRESENTVALUE                   " _
            & "   , PURCHASEPRICE                          " _
            & "   , REMODELINGCOST                         " _
            & "   , SURVIVALRATE                           " _
            & "   , RESIDUALPRICE                          " _
            & "   , MONTHLEASEFEE                          " _
            & "   , LASTMONTHBALANCE                       " _
            & "   , PAYMONTHLYAMOUNT                       " _
            & "   , PAYPRINCIPAL                           " _
            & "   , PAYINTEREST                            " _
            & "   , CURMONTHBALANCE                        " _
            & "   , BEFOREINVOICEOUTORGCD                  " _
            & "   , BEFOREKEIJOORGCD                       " _
            & "   , INITYMD                                " _
            & "   , INITUSER                               " _
            & "   , INITTERMID                             " _
            & "   , INITPGID                               " _
            & "   , UPDYMD                                 " _
            & "   , UPDUSER                                " _
            & "   , UPDTERMID                              " _
            & "   , UPDPGID                                " _
            & "   , RECEIVEYMD                             " _
            & "   , UPDTIMSTP                              " _
            & " FROM                                       " _
            & "     LNG.LNT0065_FL_LEVIES                  " _
            & " WHERE                                      " _
            & "         LEASENO  = @P01                    " _
            & "     AND CTNTYPE  = @P02                    " _
            & "     AND CTNNO    = @P03                    " _
            & "     AND PAYDATE  = @P04                    " _
            & "     AND KEIJYOYM = @P05 ;                  "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon, sqlTran), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon, sqlTran)
                ' DB更新用パラメータ
                Dim PARA00 As MySqlParameter = SQLcmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 11)        'リース登録番号
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 5)         'コンテナ記号
                Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 8)         'コンテナ番号
                Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.Date)                '支払日付
                Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 6)         '計上年月
                Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 6)         '請求書出力先組織コード
                Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 6)         '計上先組織コード
                Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.VarChar, 10)        '取引先コード
                Dim PARA09 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.VarChar, 4)         '社内口座コード
                Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 2)         '税率
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.Date)                'リース開始日
                Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.Date)                'リース終了日
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 2)         'リース契約年数
                Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.VarChar, 8)         '利率
                Dim PARA15 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.VarChar, 2)         '経済的耐用年数
                Dim PARA16 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.Decimal)               '現在価値
                Dim PARA17 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.Decimal)               '割引現在価値
                Dim PARA18 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.Decimal)               '購入価格
                Dim PARA19 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.Decimal)               '改造費
                Dim PARA20 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 4)         '残存率
                Dim PARA21 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.Decimal)               '残存価額
                Dim PARA22 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.Decimal)               '月額リース料
                Dim PARA23 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.Decimal)               '前月末残
                Dim PARA24 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.Decimal)               '支払月額
                Dim PARA25 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.Decimal)               '支払元本
                Dim PARA26 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.Decimal)               '支払利息
                Dim PARA27 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.Decimal)               '当月末残
                Dim PARA28 As MySqlParameter = SQLcmd.Parameters.Add("@P28", MySqlDbType.VarChar, 6)         '変換前請求書出力先組織コード
                Dim PARA29 As MySqlParameter = SQLcmd.Parameters.Add("@P29", MySqlDbType.VarChar, 6)         '変換前計上先組織コード
                Dim PARA30 As MySqlParameter = SQLcmd.Parameters.Add("@P30", MySqlDbType.DateTime)            '登録年月日
                Dim PARA31 As MySqlParameter = SQLcmd.Parameters.Add("@P31", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim PARA32 As MySqlParameter = SQLcmd.Parameters.Add("@P32", MySqlDbType.VarChar, 20)        '登録端末
                Dim PARA33 As MySqlParameter = SQLcmd.Parameters.Add("@P33", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim PARA34 As MySqlParameter = SQLcmd.Parameters.Add("@P34", MySqlDbType.DateTime)            '更新年月日
                Dim PARA35 As MySqlParameter = SQLcmd.Parameters.Add("@P35", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA36 As MySqlParameter = SQLcmd.Parameters.Add("@P36", MySqlDbType.VarChar, 20)        '更新端末
                Dim PARA37 As MySqlParameter = SQLcmd.Parameters.Add("@P37", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                Dim PARA38 As MySqlParameter = SQLcmd.Parameters.Add("@P38", MySqlDbType.DateTime)            '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 11)    'リース登録番号
                Dim JPARA02 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P02", MySqlDbType.VarChar, 5)     'コンテナ記号
                Dim JPARA03 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P03", MySqlDbType.VarChar, 8)     'コンテナ番号
                Dim JPARA04 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P04", MySqlDbType.Date)            '支払日付
                Dim JPARA05 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P05", MySqlDbType.VarChar, 6)     '計上年月

                Dim ContractYears As Integer
                Dim WW_DateNow As DateTime = Date.Now

                ' 年数算出
                ContractYears = DateDiff("yyyy", LeaseStartYMD, DateAdd("d", 1, LeaseEndYMD))

                ' DB更新
                PARA00.Value = "0"                               '削除フラグ
                PARA01.Value = LeaseNo                           'リース登録番号
                PARA02.Value = CtnType                           'コンテナ番号
                PARA03.Value = CtnNo                             'コンテナ番号
                PARA04.Value = PayDate                           '支払日付
                PARA05.Value = KeijoYM                           '計上年月
                PARA06.Value = InvoiceOutOrgCd                   '請求書出力先組織コード
                PARA07.Value = KeijoOrgCd                        '計上先組織コード
                PARA08.Value = ToriCode                          '取引先コード
                PARA09.Value = InAccountCd                       '社内口座コード
                PARA10.Value = TaxRate                           '税率
                PARA11.Value = LeaseStartYMD                     'リース開始日
                PARA12.Value = LeaseEndYMD                       'リース終了日
                PARA13.Value = ContractYears                     'リース契約年数
                PARA14.Value = InterestRate                      '利率
                PARA15.Value = EconomicServiceLife               '経済的耐用年数
                PARA16.Value = PresentValue                      '現在価値
                PARA17.Value = DiscountPresentValue              '割引現在価値
                PARA18.Value = PurchasePrice                     '購入価格
                PARA19.Value = RemodelingCost                    '改造費
                PARA20.Value = SurvivalRate                      '残存率
                PARA21.Value = ResidualAmount                    '残存価額
                PARA22.Value = MonthlyAmount                     '月額リース料
                PARA23.Value = LastMonthBalance                  '前月末残
                PARA24.Value = MonthlyAmount                     '支払月額
                PARA25.Value = PayPrincipal                      '支払元本
                PARA26.Value = PayInterest                       '支払利息
                PARA27.Value = CurMonthBalance                   '当月末残
                If BeforeInvoiceOutOrgCd <> "" Then
                    PARA28.Value = BeforeInvoiceOutOrgCd         '変換前請求書出力先組織コード
                Else
                    PARA28.Value = DBNull.Value                  '変換前請求書出力先組織コード
                End If
                If BeforeKeijoOrgCd <> "" Then
                    PARA29.Value = BeforeKeijoOrgCd              '変換前計上先組織コード
                Else
                    PARA29.Value = DBNull.Value                  '変換前計上先組織コード
                End If
                PARA30.Value = WW_DateNow                        '登録年月日
                PARA31.Value = UserID                            '登録ユーザーＩＤ
                PARA32.Value = ApsvID                            '登録端末
                PARA33.Value = ProgramID                         '登録プログラムＩＤ
                PARA34.Value = WW_DateNow                        '更新年月日
                PARA35.Value = UserID                            '更新ユーザーＩＤ
                PARA36.Value = ApsvID                            '更新端末
                PARA37.Value = ProgramID                         '更新プログラムＩＤ
                PARA38.Value = C_DEFAULT_YMD                     '集信日時
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA01.Value = LeaseNo
                JPARA02.Value = CtnType
                JPARA03.Value = CtnNo
                JPARA04.Value = PayDate
                JPARA05.Value = KeijoYM

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(CPT0011tbl) Then
                        CPT0011tbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            CPT0011tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    CPT0011tbl.Clear()
                    CPT0011tbl.Load(SQLdr)
                End Using

                For Each LNM0002UPDrow As DataRow In CPT0011tbl.Rows
                    CS0020JOURNAL.TABLENM = "LNT0065_FL_LEVIES"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0002UPDrow
                    CS0020JOURNAL.CS0020JOURNAL()
                    If Not isNormal(CS0020JOURNAL.ERR) Then

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"               'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                        CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                        CS0011LOGWrite.CS0011LOGWrite()                   'ログ出力

                        ErrCode = CS0020JOURNAL.ERR
                        Exit Sub
                    End If
                Next
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0065_FL_LEVIES UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

            ErrCode = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Class

