Option Strict On
Imports MySQL.Data.MySqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 固定値リスト取得
''' </summary>
''' <remarks></remarks>
Public Class GS0007FIXVALUElst
    Inherits GS0000
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' クラスコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLAS() As String
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE1() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE2() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE3() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE4() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE5() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE6() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE7() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE8() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE9() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE10() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE11() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE12() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE13() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE14() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE15() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE16() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE17() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE18() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE19() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE20() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX1() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX2() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX3() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX4() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX5() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX6() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX7() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX8() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX9() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX10() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX11() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX12() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX13() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX14() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX15() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX16() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX17() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX18() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX19() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX20() As ListBox

    ''' <summary>
    ''' SQL検索条件に含めるテキスト(このまま加える為、SQLインジェクションに注意)
    ''' </summary>
    ''' <returns></returns>
    Public Property ADDITIONAL_CONDITION As String = ""

    ''' <summary>
    ''' SQLのORDER BYの後にしてい未指定時はKEYCODEとなる
    ''' </summary>
    ''' <returns></returns>
    Public Property ADDITIONAL_SORT_ORDER As String = ""

    ''' <summary>
    ''' SQL検索条件(開始～終了)の条件
    ''' </summary>
    ''' <returns></returns>
    Public Property ADDITIONAL_FROM_TO As String = ""

    Protected METHOD_NAME As String = "GS0007FIXVALUElst"

    Public Sub GS0007FIXVALUElst()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        VALUE1 = New ListBox
        VALUE2 = New ListBox
        VALUE3 = New ListBox
        VALUE4 = New ListBox
        VALUE5 = New ListBox
        VALUE6 = New ListBox
        VALUE7 = New ListBox
        VALUE8 = New ListBox
        VALUE9 = New ListBox
        VALUE10 = New ListBox
        VALUE11 = New ListBox
        VALUE12 = New ListBox
        VALUE13 = New ListBox
        VALUE14 = New ListBox
        VALUE15 = New ListBox
        VALUE16 = New ListBox
        VALUE17 = New ListBox
        VALUE18 = New ListBox
        VALUE19 = New ListBox
        VALUE20 = New ListBox

        Try
            If IsNothing(LISTBOX1) Then
                LISTBOX1 = New ListBox
            Else
                LISTBOX1.Items.Clear()
            End If

            If IsNothing(LISTBOX2) Then
                LISTBOX2 = New ListBox
            Else
                LISTBOX2.Items.Clear()
            End If

            If IsNothing(LISTBOX3) Then
                LISTBOX3 = New ListBox
            Else
                LISTBOX3.Items.Clear()
            End If

            If IsNothing(LISTBOX4) Then
                LISTBOX4 = New ListBox
            Else
                LISTBOX4.Items.Clear()
            End If

            If IsNothing(LISTBOX5) Then
                LISTBOX5 = New ListBox
            Else
                LISTBOX5.Items.Clear()
            End If

            If IsNothing(LISTBOX6) Then
                LISTBOX6 = New ListBox
            Else
                LISTBOX6.Items.Clear()
            End If

            If IsNothing(LISTBOX7) Then
                LISTBOX7 = New ListBox
            Else
                LISTBOX7.Items.Clear()
            End If

            If IsNothing(LISTBOX8) Then
                LISTBOX8 = New ListBox
            Else
                LISTBOX8.Items.Clear()
            End If

            If IsNothing(LISTBOX9) Then
                LISTBOX9 = New ListBox
            Else
                LISTBOX9.Items.Clear()
            End If

            If IsNothing(LISTBOX10) Then
                LISTBOX10 = New ListBox
            Else
                LISTBOX10.Items.Clear()
            End If

            If IsNothing(LISTBOX11) Then
                LISTBOX11 = New ListBox
            Else
                LISTBOX11.Items.Clear()
            End If

            If IsNothing(LISTBOX12) Then
                LISTBOX12 = New ListBox
            Else
                LISTBOX12.Items.Clear()
            End If

            If IsNothing(LISTBOX13) Then
                LISTBOX13 = New ListBox
            Else
                LISTBOX13.Items.Clear()
            End If

            If IsNothing(LISTBOX14) Then
                LISTBOX14 = New ListBox
            Else
                LISTBOX14.Items.Clear()
            End If

            If IsNothing(LISTBOX15) Then
                LISTBOX15 = New ListBox
            Else
                LISTBOX15.Items.Clear()
            End If

            If IsNothing(LISTBOX16) Then
                LISTBOX16 = New ListBox
            Else
                LISTBOX16.Items.Clear()
            End If

            If IsNothing(LISTBOX17) Then
                LISTBOX17 = New ListBox
            Else
                LISTBOX17.Items.Clear()
            End If

            If IsNothing(LISTBOX18) Then
                LISTBOX18 = New ListBox
            Else
                LISTBOX18.Items.Clear()
            End If

            If IsNothing(LISTBOX19) Then
                LISTBOX19 = New ListBox
            Else
                LISTBOX19.Items.Clear()
            End If

            If IsNothing(LISTBOX20) Then
                LISTBOX20 = New ListBox
            Else
                LISTBOX20.Items.Clear()
            End If

        Catch ex As Exception
        End Try

        '●In PARAMチェック
        'PARAM01: CLAS
        If checkParam(METHOD_NAME, CLAS) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '初期値設定
        ERR = C_MESSAGE_NO.DLL_IF_ERROR

        '●固定値リスト取得(指定値)
        '○ DB(LNS0006_FIXVALUE)検索

        Dim SQLStr As String = String.Empty
        Try

            'LNS0006_FIXVALUE検索SQL文
            If String.IsNullOrEmpty(CLAS) Then
                SQLStr =
                      " SELECT DISTINCT                  " _
                    & "      rtrim(CLASS)  as KEYCODE , " _
                    & "      rtrim(VALUE1)  as VALUE1  , " _
                    & "      rtrim(VALUE2)  as VALUE2  , " _
                    & "      rtrim(VALUE3)  as VALUE3  , " _
                    & "      rtrim(VALUE4)  as VALUE4  , " _
                    & "      rtrim(VALUE5)  as VALUE5  , " _
                    & "      rtrim(VALUE6)  as VALUE6  , " _
                    & "      rtrim(VALUE7)  as VALUE7  , " _
                    & "      rtrim(VALUE8)  as VALUE8  , " _
                    & "      rtrim(VALUE9)  as VALUE9  , " _
                    & "      rtrim(VALUE10)  as VALUE10  , " _
                    & "      rtrim(VALUE11)  as VALUE11  , " _
                    & "      rtrim(VALUE12)  as VALUE12  , " _
                    & "      rtrim(VALUE13)  as VALUE13  , " _
                    & "      rtrim(VALUE14)  as VALUE14  , " _
                    & "      rtrim(VALUE15)  as VALUE15  , " _
                    & "      rtrim(VALUE16)  as VALUE16  , " _
                    & "      rtrim(VALUE17)  as VALUE17  , " _
                    & "      rtrim(VALUE18)  as VALUE18  , " _
                    & "      rtrim(VALUE19)  as VALUE19  , " _
                    & "      rtrim(VALUE20)  as VALUE20    " _
                    & " FROM  LNG.VIW0001_FIXVALUE             " _
                    & " Where CAMPCODE  = @P1 " _
                    & "   and DELFLG   <> @P5 "
                '### 20201013 START 指摘票対応(No153) ###################################
                '& "   and STYMD    <= @P3 " _
                '& "   and ENDYMD   >= @P4 " _
                If ADDITIONAL_FROM_TO <> "" Then
                    SQLStr = SQLStr & "   and STYMD    <= '" & ADDITIONAL_FROM_TO & "'" _
                                    & "   and ENDYMD   >= '" & ADDITIONAL_FROM_TO & "'"
                Else
                    SQLStr = SQLStr & "   and STYMD    <= @P3 " _
                                    & "   and ENDYMD   >= @P4 "
                End If
                '### 20201013 END   指摘票対応(No153) ###################################
                If ADDITIONAL_CONDITION <> "" Then
                    SQLStr = SQLStr & " " & ADDITIONAL_CONDITION & " "
                End If
                If Me.ADDITIONAL_SORT_ORDER <> "" Then
                    SQLStr = SQLStr & " ORDER BY " & Me.ADDITIONAL_SORT_ORDER & " "
                Else
                    SQLStr = SQLStr & " ORDER BY KEYCODE "
                End If

            Else
                SQLStr =
                      " SELECT                           " _
                    & "      rtrim(KEYCODE) as KEYCODE , " _
                    & "      rtrim(VALUE1)  as VALUE1  , " _
                    & "      rtrim(VALUE2)  as VALUE2  , " _
                    & "      rtrim(VALUE3)  as VALUE3  , " _
                    & "      rtrim(VALUE4)  as VALUE4  , " _
                    & "      rtrim(VALUE5)  as VALUE5  , " _
                    & "      rtrim(VALUE6)  as VALUE6  , " _
                    & "      rtrim(VALUE7)  as VALUE7  , " _
                    & "      rtrim(VALUE8)  as VALUE8  , " _
                    & "      rtrim(VALUE9)  as VALUE9  , " _
                    & "      rtrim(VALUE10)  as VALUE10  , " _
                    & "      rtrim(VALUE11)  as VALUE11  , " _
                    & "      rtrim(VALUE12)  as VALUE12  , " _
                    & "      rtrim(VALUE13)  as VALUE13  , " _
                    & "      rtrim(VALUE14)  as VALUE14  , " _
                    & "      rtrim(VALUE15)  as VALUE15  , " _
                    & "      rtrim(VALUE16)  as VALUE16  , " _
                    & "      rtrim(VALUE17)  as VALUE17  , " _
                    & "      rtrim(VALUE18)  as VALUE18  , " _
                    & "      rtrim(VALUE19)  as VALUE19  , " _
                    & "      rtrim(VALUE20)  as VALUE20    " _
                    & " FROM  LNG.VIW0001_FIXVALUE             " _
                    & " Where CAMPCODE  = @P1 " _
                    & "   and CLASS     = @P2 " _
                    & "   and DELFLG   <> @P5 "
                '### 20201013 START 指摘票対応(No153) ###################################
                '& "   and STYMD    <= @P3 " _
                '& "   and ENDYMD   >= @P4 " _
                If ADDITIONAL_FROM_TO <> "" Then
                    SQLStr = SQLStr & "   and STYMD    <= '" & ADDITIONAL_FROM_TO & "'" _
                                    & "   and ENDYMD   >= '" & ADDITIONAL_FROM_TO & "'"
                Else
                    SQLStr = SQLStr & "   and STYMD    <= @P3 " _
                                    & "   and ENDYMD   >= @P4 "
                End If
                '### 20201013 END   指摘票対応(No153) ###################################

                If ADDITIONAL_CONDITION <> "" Then
                    SQLStr = SQLStr & " " & ADDITIONAL_CONDITION & " "
                End If
                If Me.ADDITIONAL_SORT_ORDER <> "" Then
                    SQLStr = SQLStr & " ORDER BY " & Me.ADDITIONAL_SORT_ORDER & " "
                Else
                    SQLStr = SQLStr & " ORDER BY KEYCODE "
                End If
            End If

            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                MySqlConnection.ClearPool(SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", MySqlDbType.VarChar, 20).Value = CAMPCODE
                    .Add("@P2", MySqlDbType.VarChar, 25).Value = CLAS
                    .Add("@P3", MySqlDbType.Date).Value = Date.Now
                    .Add("@P4", MySqlDbType.Date).Value = Date.Now
                    .Add("@P5", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    Dim val(20) As String
                    Dim keyCode As String = ""
                    While SQLdr.Read
                        keyCode = Convert.ToString(SQLdr("KEYCODE"))
                        If keyCode <> "" Then
                            For i As Integer = 1 To 20
                                val(i) = Convert.ToString(SQLdr(String.Format("VALUE{0}", i)))
                            Next
                            VALUE1.Items.Add(New ListItem(val(1), keyCode))
                            VALUE2.Items.Add(New ListItem(val(2), keyCode))
                            VALUE3.Items.Add(New ListItem(val(3), keyCode))
                            VALUE4.Items.Add(New ListItem(val(4), keyCode))
                            VALUE5.Items.Add(New ListItem(val(5), keyCode))
                            VALUE6.Items.Add(New ListItem(val(6), keyCode))
                            VALUE7.Items.Add(New ListItem(val(7), keyCode))
                            VALUE8.Items.Add(New ListItem(val(8), keyCode))
                            VALUE9.Items.Add(New ListItem(val(9), keyCode))
                            VALUE10.Items.Add(New ListItem(val(10), keyCode))
                            VALUE11.Items.Add(New ListItem(val(11), keyCode))
                            VALUE12.Items.Add(New ListItem(val(12), keyCode))
                            VALUE13.Items.Add(New ListItem(val(13), keyCode))
                            VALUE14.Items.Add(New ListItem(val(14), keyCode))
                            VALUE15.Items.Add(New ListItem(val(15), keyCode))
                            VALUE16.Items.Add(New ListItem(val(16), keyCode))
                            VALUE17.Items.Add(New ListItem(val(17), keyCode))
                            VALUE18.Items.Add(New ListItem(val(18), keyCode))
                            VALUE19.Items.Add(New ListItem(val(19), keyCode))
                            VALUE20.Items.Add(New ListItem(val(20), keyCode))

                            LISTBOX1.Items.Add(New ListItem(val(1), keyCode))
                            LISTBOX2.Items.Add(New ListItem(val(2), keyCode))
                            LISTBOX3.Items.Add(New ListItem(val(3), keyCode))
                            LISTBOX4.Items.Add(New ListItem(val(4), keyCode))
                            LISTBOX5.Items.Add(New ListItem(val(5), keyCode))
                            LISTBOX6.Items.Add(New ListItem(val(6), keyCode))
                            LISTBOX7.Items.Add(New ListItem(val(7), keyCode))
                            LISTBOX8.Items.Add(New ListItem(val(8), keyCode))
                            LISTBOX9.Items.Add(New ListItem(val(9), keyCode))
                            LISTBOX10.Items.Add(New ListItem(val(10), keyCode))
                            LISTBOX11.Items.Add(New ListItem(val(11), keyCode))
                            LISTBOX12.Items.Add(New ListItem(val(12), keyCode))
                            LISTBOX13.Items.Add(New ListItem(val(13), keyCode))
                            LISTBOX14.Items.Add(New ListItem(val(14), keyCode))
                            LISTBOX15.Items.Add(New ListItem(val(15), keyCode))
                            LISTBOX16.Items.Add(New ListItem(val(16), keyCode))
                            LISTBOX17.Items.Add(New ListItem(val(17), keyCode))
                            LISTBOX18.Items.Add(New ListItem(val(18), keyCode))
                            LISTBOX19.Items.Add(New ListItem(val(19), keyCode))
                            LISTBOX20.Items.Add(New ListItem(val(20), keyCode))
                        End If
                    End While
                End Using 'SQLdr
                ERR = C_MESSAGE_NO.NORMAL
            End Using 'SQLcon,SQLcmd
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0011_UPROFXLS Select"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '●固定値リスト取得(デフォルト値)
        '○ DB(LNS0006_FIXVALUE)検索
        If VALUE1.Items.Count = 0 Then
            Try
                'DataBase接続文字
                Using SQLcon = sm.getConnection,
                      SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    SQLcon.Open() 'DataBase接続(Open)
                    MySqlConnection.ClearPool(SQLcon)
                    With SQLcmd.Parameters
                        .Add("@P1", MySqlDbType.VarChar, 20).Value = C_DEFAULT_DATAKEY
                        .Add("@P2", MySqlDbType.VarChar, 20).Value = CLAS
                        .Add("@P3", MySqlDbType.Date).Value = Date.Now
                        .Add("@P4", MySqlDbType.Date).Value = Date.Now
                        .Add("@P5", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE
                    End With
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        Dim val(20) As String
                        Dim keyCode As String = ""

                        While SQLdr.Read
                            keyCode = Convert.ToString(SQLdr("KEYCODE"))
                            If keyCode <> "" Then

                                For i As Integer = 1 To 20
                                    val(i) = Convert.ToString(SQLdr(String.Format("VALUE{0}", i)))
                                Next
                                VALUE1.Items.Add(New ListItem(val(1), keyCode))
                                VALUE2.Items.Add(New ListItem(val(2), keyCode))
                                VALUE3.Items.Add(New ListItem(val(3), keyCode))
                                VALUE4.Items.Add(New ListItem(val(4), keyCode))
                                VALUE5.Items.Add(New ListItem(val(5), keyCode))
                                VALUE6.Items.Add(New ListItem(val(6), keyCode))
                                VALUE7.Items.Add(New ListItem(val(7), keyCode))
                                VALUE8.Items.Add(New ListItem(val(8), keyCode))
                                VALUE9.Items.Add(New ListItem(val(9), keyCode))
                                VALUE10.Items.Add(New ListItem(val(10), keyCode))
                                VALUE11.Items.Add(New ListItem(val(11), keyCode))
                                VALUE12.Items.Add(New ListItem(val(12), keyCode))
                                VALUE13.Items.Add(New ListItem(val(13), keyCode))
                                VALUE14.Items.Add(New ListItem(val(14), keyCode))
                                VALUE15.Items.Add(New ListItem(val(15), keyCode))
                                VALUE16.Items.Add(New ListItem(val(16), keyCode))
                                VALUE17.Items.Add(New ListItem(val(17), keyCode))
                                VALUE18.Items.Add(New ListItem(val(18), keyCode))
                                VALUE19.Items.Add(New ListItem(val(19), keyCode))
                                VALUE20.Items.Add(New ListItem(val(20), keyCode))

                                LISTBOX1.Items.Add(New ListItem(val(1), keyCode))
                                LISTBOX2.Items.Add(New ListItem(val(2), keyCode))
                                LISTBOX3.Items.Add(New ListItem(val(3), keyCode))
                                LISTBOX4.Items.Add(New ListItem(val(4), keyCode))
                                LISTBOX5.Items.Add(New ListItem(val(5), keyCode))
                                LISTBOX6.Items.Add(New ListItem(val(6), keyCode))
                                LISTBOX7.Items.Add(New ListItem(val(7), keyCode))
                                LISTBOX8.Items.Add(New ListItem(val(8), keyCode))
                                LISTBOX9.Items.Add(New ListItem(val(9), keyCode))
                                LISTBOX10.Items.Add(New ListItem(val(10), keyCode))
                                LISTBOX11.Items.Add(New ListItem(val(11), keyCode))
                                LISTBOX12.Items.Add(New ListItem(val(12), keyCode))
                                LISTBOX13.Items.Add(New ListItem(val(13), keyCode))
                                LISTBOX14.Items.Add(New ListItem(val(14), keyCode))
                                LISTBOX15.Items.Add(New ListItem(val(15), keyCode))
                                LISTBOX16.Items.Add(New ListItem(val(16), keyCode))
                                LISTBOX17.Items.Add(New ListItem(val(17), keyCode))
                                LISTBOX18.Items.Add(New ListItem(val(18), keyCode))
                                LISTBOX19.Items.Add(New ListItem(val(19), keyCode))
                                LISTBOX20.Items.Add(New ListItem(val(20), keyCode))
                            End If
                        End While
                    End Using

                    ERR = C_MESSAGE_NO.NORMAL
                End Using 'SQLcon, SQLcmd
            Catch ex As Exception
                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0011_UPROFXLS Select DEFAULT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                ERR = C_MESSAGE_NO.DB_ERROR
            End Try
        End If

    End Sub
    ''' <summary>
    ''' FixValueより取得したテーブルを返却
    ''' </summary>
    ''' <returns>DataTable </returns>
    Public Function GS0007FIXVALUETbl() As DataTable
        Dim retDt As DataTable = Nothing
        '●In PARAMチェック
        'PARAM01: CLAS
        If checkParam(METHOD_NAME, CLAS) <> C_MESSAGE_NO.NORMAL Then
            Throw New Exception(String.Format("CLAS Name Undefine CLAS={0}", CLAS))
        End If
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '初期値設定
        ERR = C_MESSAGE_NO.NORMAL
        Dim sqlStat As New StringBuilder
        'SQL文字の生成
        If String.IsNullOrEmpty(CLAS) Then
            '使うか不明で、Datatableを返却するためリストボックスと違い同じ規則性不要
            sqlStat.AppendLine("SELECT DISTINCT")
            sqlStat.AppendLine("      ,rtrim(CLASS)  AS CLASS")
            sqlStat.AppendLine("      ,rtrim(NAMES)  AS NAMES")
            sqlStat.AppendLine("  FROM  LNG.VIW0001_FIXVALUE")
            sqlStat.AppendLine(" WHERE  CAMPCODE   = @CAMPCODE")
            sqlStat.AppendLine("   AND  STYMD     <= @STYMD")
            sqlStat.AppendLine("   AND  ENDYMD    >= @ENDYMD")
            sqlStat.AppendLine("   AND  DELFLG    <> @DELFLG")
            If ADDITIONAL_CONDITION <> "" Then
                sqlStat.AppendLine(ADDITIONAL_CONDITION)
            End If
            If Me.ADDITIONAL_SORT_ORDER <> "" Then
                sqlStat.AppendLine(" ORDER BY " & Me.ADDITIONAL_SORT_ORDER)
            Else
                sqlStat.AppendLine(" ORDER BY KEYCODE")
            End If
        Else
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       rtrim(coalesce(KEYCODE,''))  AS KEYCODE")
            For Each fieldName In {"VALUE1", "VALUE2", "VALUE3", "VALUE4", "VALUE5",
                                       "VALUE6", "VALUE7", "VALUE8", "VALUE9", "VALUE10",
                                       "VALUE11", "VALUE12", "VALUE13", "VALUE14", "VALUE15",
                                       "VALUE16", "VALUE17", "VALUE18", "VALUE19", "VALUE20"}
                sqlStat.AppendFormat("      ,rtrim(coalesce({0},''))   AS {0}", fieldName).AppendLine()
            Next fieldName
            sqlStat.AppendLine("      ,rtrim(coalesce(NAMES,''))  AS NAMES")
            sqlStat.AppendLine("      ,rtrim(coalesce(NAMEL,''))  AS NAMEL")
            sqlStat.AppendLine("      ,SYSTEMKEYFLG  AS SYSTEMKEYFLG")
            sqlStat.AppendLine("  FROM  LNG.VIW0001_FIXVALUE")
            sqlStat.AppendLine(" WHERE  CAMPCODE   = @CAMPCODE")
            sqlStat.AppendLine("   AND  CLASS      = @CLASS")
            sqlStat.AppendLine("   AND  STYMD     <= @STYMD")
            sqlStat.AppendLine("   AND  ENDYMD    >= @ENDYMD")
            sqlStat.AppendLine("   AND  DELFLG    <> @DELFLG")
            If ADDITIONAL_CONDITION <> "" Then
                sqlStat.AppendLine(ADDITIONAL_CONDITION)
            End If
            If Me.ADDITIONAL_SORT_ORDER <> "" Then
                sqlStat.AppendLine(" ORDER BY " & Me.ADDITIONAL_SORT_ORDER)
            Else
                sqlStat.AppendLine(" ORDER BY KEYCODE")
            End If
        End If

        Try
            'DataBase接続文字
            Using sqlCon = sm.getConnection,
                  sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@CLASS", MySqlDbType.VarChar, 20).Value = CLAS
                    .Add("@STYMD", MySqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", MySqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", MySqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE

                End With
                Dim paramCampCode = sqlCmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 20)
                'パラメータのCOMPCODE、なければCOMPCODE"Default"で検索
                For Each campVal As String In {CAMPCODE, C_DEFAULT_DATAKEY}
                    paramCampCode.Value = campVal
                    Using sqlDa As New MySqlDataAdapter(sqlCmd)
                        retDt = New DataTable
                        sqlDa.Fill(retDt)
                    End Using
                    'レコードがある場合はCOMPCODE="Default"で検索しない
                    If retDt IsNot Nothing AndAlso retDt.Rows.Count > 0 Then
                        Exit For
                    End If
                Next campVal
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0011_UPROFXLS Select DEFAULT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
        End Try
        Return retDt
    End Function
End Class
