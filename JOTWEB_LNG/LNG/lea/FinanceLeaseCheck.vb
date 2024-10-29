''************************************************************
' ファイナンス判定
' 作成日 2022/05/26
' 更新日 
' 作成者 名取
' 更新者 
'
' 修正履歴 : 2022/05/26 新規作成
'          : 
''************************************************************

''' <summary>
''' ファイナンス判定
''' </summary>
''' <remarks></remarks>
Public Class FinanceLeaseCheck

    ''' <summary>
    ''' ファイナンス判定
    ''' </summary>
    ''' <param name="LegalServiceLife"></param>       法定耐用年数
    ''' <param name="ElapsedYears"></param>           経過年数
    ''' <param name="PurchasePrice"></param>          購入価格
    ''' <param name="LeaseStBokaPrice"></param>       リース開始時簿価
    ''' <param name="RemodelingPrice"></param>        改造価格
    ''' <param name="ContractStYMD"></param>          契約開始日
    ''' <param name="ContractEndYMD"></param>         契約終了日
    ''' <param name="SurvivalRate"></param>           残存率
    ''' <param name="MonthlyLeasePrice"></param>      月額リース料
    ''' <param name="GuaranteeAmount"></param>        残価保証額
    ''' <param name="Months"></param>                 月数
    ''' <param name="FirstRemainingAmount"></param>   初回前残
    ''' <param name="ResidualAmount"></param>         残存価格
    ''' <param name="InterestRate"></param>           利率
    ''' <param name="PresentValue"></param>           現在価値
    ''' <param name="DiscountPresentValue"></param>   割引現在価値
    ''' <param name="PurchasePriceRate"></param>      購入価格割合
    ''' <param name="ServiceLifeRate"></param>        耐用年数割合
    ''' <param name="PresentValueRate"></param>       現在価値割合
    ''' <param name="JudgResult"></param>             判定結果
    Public Shared Sub FinanceCheck(ByVal LegalServiceLife As String, ByVal ElapsedYears As String,
                                   ByVal PurchasePrice As String, ByVal LeaseStBokaPrice As String,
                                   ByVal RemodelingPrice As String, ByVal ContractStYMD As String,
                                   ByVal ContractEndYMD As String, ByVal SurvivalRate As String,
                                   ByVal MonthlyLeasePrice As String, ByVal GuaranteeAmount As String,
                                   ByRef Months As Long, ByRef FirstRemainingAmount As Long,
                                   ByRef ResidualAmount As Long, ByRef InterestRate As Decimal,
                                   ByRef PresentValue As Double, ByRef DiscountPresentValue As Double,
                                   ByRef PurchasePriceRate As Decimal, ByRef ServiceLifeRate As Decimal,
                                   ByRef PresentValueRate As Decimal, ByRef EconomicServiceLife As Long,
                                   ByRef JudgResult As String)

        Dim WorkYears As Long = 0               'Work年数
        Dim PresentValueGuaranteeAmount As Double
        Try
            ' 月数算出
            Months = DateDiff("m", CDate(ContractStYMD), CDate(ContractEndYMD).AddDays(1))

            ' 初回前残算出(リース開始時簿価 + 改造価格)
            FirstRemainingAmount = CLng(LeaseStBokaPrice) + CLng(RemodelingPrice)

            ' 残存価格算出(初回前残算出 × 残存率)
            ResidualAmount = CLng(FirstRemainingAmount * CDec(SurvivalRate))

            ' 利率計算(RATE(月数, 月額リース料 × -1, 初回前残, 残存価格 × -1) × 12)
            InterestRate = Rate(CDbl(Months), CDbl(MonthlyLeasePrice * -1), CDbl(FirstRemainingAmount), CDbl((ResidualAmount) * -1)) * 12
            '  小数点6位を切り捨て
            InterestRate *= 100000
            InterestRate = Math.Floor(InterestRate)
            InterestRate /= 100000

            ' 現在価値計算(PV(利率 ÷ 12, 月数, 月額リース料 × -1)
            PresentValue = PV(CDbl(InterestRate) / 12, CDbl(Months), CDbl(MonthlyLeasePrice * -1))
            ' 残価保証額計算(残価保証額 / (1 + 利率 / 12)
            PresentValueGuaranteeAmount = GuaranteeAmount / (1 + InterestRate / 12)
            '  現在価値に残価保証額を加算(小数点を切り捨て)
            DiscountPresentValue = Math.Floor(PresentValue + PresentValueGuaranteeAmount)
            '  小数点を切り捨て
            PresentValue = Math.Floor(PresentValue)

            ' 購入価格割合算出
            '  購入価格割合算出(購入価格 ÷ 初回前残)
            PurchasePriceRate = CDec(CLng(PurchasePrice) / FirstRemainingAmount)
            '  小数点3位を切り捨て
            PurchasePriceRate *= 100
            PurchasePriceRate = Math.Floor(PurchasePriceRate)
            PurchasePriceRate /= 100

            ' 現在価値割合算出
            '  現在価値割合算出(現在価値 ÷ 初回前残)
            PresentValueRate = CDec(PresentValue / FirstRemainingAmount)
            '  小数点3位を切り捨て
            PresentValueRate *= 100
            PresentValueRate = Math.Floor(PresentValueRate)
            PresentValueRate /= 100

            ' 耐用年数割合算出
            '  Work年数算出
            '  法定耐用年数・経過年数大小チェック(経過年数 < 法定耐用年数)
            If CInt(ElapsedYears) < CLng(LegalServiceLife) Then
                ' Work年数算出((法定耐用年数 - 経過年数) + 経過年数 × 0.2)
                WorkYears = (CInt(LegalServiceLife) - CInt(ElapsedYears)) + CInt(CInt(ElapsedYears) * 0.2)
            Else
                ' Work年数算出(法定耐用年数 × 0.2)
                WorkYears = CInt(CInt(LegalServiceLife) * 0.2)
            End If
            '  経済的耐用年数設定
            '  Work年数・固定値"2"小チェック(Work年数 < 2)
            If WorkYears < 2 Then
                EconomicServiceLife = 2
            Else
                EconomicServiceLife = WorkYears
            End If
            '  耐用年数割合算出(月数 ÷ 12 ÷ 経済的耐用年数)
            ServiceLifeRate = CDec(CInt(Months / 12) / EconomicServiceLife)
            '  小数点3位を四捨五入
            ServiceLifeRate = Math.Round(ServiceLifeRate, 2, MidpointRounding.AwayFromZero)

            ' ファイナンス判定
            ' 購入価格割合 & 初回前残　不正値チェック
            If PurchasePriceRate <= 0 OrElse
                FirstRemainingAmount <= 0 Then
                ' ファイナンス判定不能
                JudgResult = "1"
                Exit Sub
            ElseIf InterestRate < 0.00001 OrElse
                   InterestRate > 99.99999 Then
                ' ファイナンス判定不能
                JudgResult = "1"
                Exit Sub
            Else
                ' 購入価格割合の規定値チェック
                If PurchasePriceRate > 0.5 Then
                    ' 現在価値割合の規定値チェック
                    If PresentValueRate >= 0.9 Then
                        Exit Sub
                    Else
                        ' 耐用年数割合の規定値チェック
                        If ServiceLifeRate >= 0.75 Then
                            Exit Sub
                        Else
                            ' オペレーティング判定
                            JudgResult = "1"
                            Exit Sub
                        End If
                    End If
                Else
                    ' オペレーティング判定
                    JudgResult = "1"
                    Exit Sub
                End If
            End If

        Catch ex As Exception
            JudgResult = "1"
        End Try

    End Sub

End Class

