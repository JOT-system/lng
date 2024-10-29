Option Strict On
Imports MySQL.Data.MySqlClient

''' <summary>
''' ガイダンス取得クラス
''' </summary>
''' <remarks>各種請求ヘッダーデータに登録する際はこちらに定義</remarks>
Public Class CmnGuidanceData

    ''' <summary>
    ''' 表示用のガイダンスデータ取得
    ''' </summary>
    ''' <param name="sqlCon">MySqlConnection</param>
    ''' <returns>ガイダンスデータ</returns>
    Public Shared Function GetGuidanceData(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, userid As String) As DataTable
        Dim retDt As New DataTable
        With retDt.Columns
            .Add("GUIDANCENO", GetType(String))
            .Add("TYPE", GetType(String))
            .Add("TITLE", GetType(String))
            .Add("NAIYOU", GetType(String))
        End With

        '〇ガイダンス情報h追加
        'ガイダンス：ダイレクト修正_担当者
        'retDt = rowsSet(retDt, GetLNT0008Appl(sqlCon, sqlTran, userid))

        'ガイダンス：ダイレクト修正_承認者
        'retDt = rowsSet(retDt, GetLNT0008Approval(sqlCon, sqlTran, userid))

        'ガイダンス：収入管理_担当者
        'retDt = rowsSet(retDt, GetLNT0007Appl(sqlCon, sqlTran, userid))

        'ガイダンス：収入管理_承認者
        'retDt = rowsSet(retDt, GetLNT0007Approval(sqlCon, sqlTran, userid))

        Return retDt
    End Function

    ''' <summary>
    ''' ガイダンスdatatable追加
    ''' </summary>
    ''' <param name="dtlOriginal">Datatable集約元</param>
    ''' <param name="dtlTo">Datatable追加データ</param>
    ''' <returns>ガイダンスデータ</returns>
    Public Shared Function rowsSet(dtlOriginal As DataTable, dtlTo As DataTable) As DataTable
        'レコードをdatatableに追加
        Dim dr As DataRow
        For Each rowData As DataRow In dtlTo.Rows
            dr = dtlOriginal.NewRow
            dr("GUIDANCENO") = rowData("GUIDANCENO").ToString
            dr("TYPE") = rowData("TYPE").ToString
            dr("TITLE") = "・" & HttpUtility.HtmlEncode(rowData("TITLE").ToString)
            dr("NAIYOU") = HttpUtility.HtmlEncode(rowData("NAIYOU").ToString).Replace(ControlChars.CrLf, "<br />").Replace(ControlChars.Cr, "<br />").Replace(ControlChars.Lf, "<br />")

            dtlOriginal.Rows.Add(dr)
        Next

        Return dtlOriginal
    End Function

    ''' <summary>
    ''' ガイダンス：ダイレクト修正_担当者　取得
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="userid">ログインユーザーID</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function GetLNT0008Appl(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, userid As String) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    A01.GUIDANCENO AS GUIDANCENO")                                                                      'ガイダンス№
        SQLBldr.AppendLine("    , A01.TYPE AS TYPE")                                                                                '種類
        SQLBldr.AppendLine("    , A01.TITLE AS TITLE")                                                                              'タイトル
        SQLBldr.AppendLine("    , 'A=1 B=22' AS NAIYOU")                                                                              '内容
        'SQLBldr.AppendLine("    , REPLACE(REPLACE(A01.NAIYOU, '@1', coalesce(A02.KENSU, 0)), '@2', coalesce(A03.KENSU, 0)) AS NAIYOU")  '内容
        SQLBldr.AppendLine("FROM")
        'メイン ガイダンスマスタ
        SQLBldr.AppendLine("    com.LNS0008_GUIDANCE A01")
        'コンテナ清算ファイル(承認済み)   ※担当者用
        'SQLBldr.AppendLine("    LEFT JOIN (")
        'SQLBldr.AppendLine("        SELECT")
        'SQLBldr.AppendLine("            APPLUSER")                                  '担当者ID
        'SQLBldr.AppendLine("            , COUNT(*) AS KENSU")                       '件数(承認済み)
        'SQLBldr.AppendLine("        FROM")
        'SQLBldr.AppendLine("            lng.LNT0017_RESSNF")
        'SQLBldr.AppendLine("        WHERE")
        'SQLBldr.AppendLine("            APPLSTATUS = '4'")
        'SQLBldr.AppendLine("            AND DELFLG = 0")
        'SQLBldr.AppendLine("        GROUP BY")
        'SQLBldr.AppendLine("            APPLUSER) A02")
        'SQLBldr.AppendLine("        ON A02.APPLUSER = '" & userid & "'")
        ''コンテナ清算ファイル(差し戻し)   ※担当者用
        'SQLBldr.AppendLine("    LEFT JOIN (")
        'SQLBldr.AppendLine("        SELECT")
        'SQLBldr.AppendLine("            APPLUSER")                                  '担当者ID
        'SQLBldr.AppendLine("            , COUNT(*) AS KENSU")                       '件数(差し戻し)
        'SQLBldr.AppendLine("        FROM")
        'SQLBldr.AppendLine("            lng.LNT0017_RESSNF")
        'SQLBldr.AppendLine("        WHERE")
        'SQLBldr.AppendLine("            APPLSTATUS = '3'")
        'SQLBldr.AppendLine("            AND DELFLG = 0")
        'SQLBldr.AppendLine("        GROUP BY")
        'SQLBldr.AppendLine("            APPLUSER) A03")
        'SQLBldr.AppendLine("        ON A03.APPLUSER = '" & userid & "'")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        '検索条件追加
        SQLBldr.AppendLine("    A01.DELFLG = 0")
        SQLBldr.AppendLine("    AND A01.PRGRMID = 'LNT0008'")                       '機能ID
        SQLBldr.AppendLine("    AND A01.PRGRMKBN = '1'")                            '機能区分
        'SQLBldr.AppendLine("    AND (")                                             '件数が一つもない場合、対象外
        'SQLBldr.AppendLine("       (A02.KENSU IS NOT NULL AND A02.KENSU <> '0')")
        'SQLBldr.AppendLine("    OR (A03.KENSU IS NOT NULL AND A03.KENSU <> '0')")
        'SQLBldr.AppendLine("        )")
        '並び順
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    A01.GUIDANCENO")
        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

    ''' <summary>
    ''' ガイダンス：ダイレクト修正_承認者　取得
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="userid">ログインユーザーID</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function GetLNT0008Approval(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, userid As String) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    A01.GUIDANCENO AS GUIDANCENO")                                                                      'ガイダンス№
        SQLBldr.AppendLine("    , A01.TYPE AS TYPE")                                                                                '種類
        SQLBldr.AppendLine("    , A01.TITLE AS TITLE")                                                                              'タイトル
        SQLBldr.AppendLine("    , REPLACE(A01.NAIYOU, '@1', coalesce(A02.KENSU, 0)) AS NAIYOU")                                       '内容
        SQLBldr.AppendLine("FROM")
        'メイン ガイダンスマスタ
        SQLBldr.AppendLine("    com.LNS0008_GUIDANCE A01")
        'コンテナ清算ファイル(申請中)    ※承認者用
        SQLBldr.AppendLine("    INNER JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            APPROVALUSER")                              '承認者ID
        SQLBldr.AppendLine("            , COUNT(*) AS KENSU")                       '件数(申請中)
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0017_RESSNF")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            APPLSTATUS IN ('1','2')")
        SQLBldr.AppendLine("            AND DELFLG = 0")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            APPROVALUSER) A02")
        SQLBldr.AppendLine("        ON A02.APPROVALUSER = '" & userid & "'")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        '検索条件追加
        SQLBldr.AppendLine("    A01.DELFLG = 0")
        SQLBldr.AppendLine("    AND A01.PRGRMID = 'LNT0008'")                       '機能ID
        SQLBldr.AppendLine("    AND A01.PRGRMKBN = '2'")                            '機能区分
        SQLBldr.AppendLine("    AND (")                                             '件数が一つもない場合、対象外
        SQLBldr.AppendLine("       (A02.KENSU IS NOT NULL AND A02.KENSU <> '0')")
        SQLBldr.AppendLine("        )")

        '並び順
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    A01.GUIDANCENO")
        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

    ''' <summary>
    ''' ガイダンス：収入管理_担当者　取得
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="userid">ログインユーザーID</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function GetLNT0007Appl(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, userid As String) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    A01.GUIDANCENO AS GUIDANCENO")                                                                      'ガイダンス№
        SQLBldr.AppendLine("    , A01.TYPE AS TYPE")                                                                                '種類
        SQLBldr.AppendLine("    , A01.TITLE AS TITLE")                                                                              'タイトル
        SQLBldr.AppendLine("    , REPLACE(REPLACE(A01.NAIYOU, '@1', coalesce(A02.KENSU, 0)), '@2', coalesce(A03.KENSU, 0)) AS NAIYOU")  '請求番号
        SQLBldr.AppendLine("FROM")
        'メイン ガイダンスマスタ
        SQLBldr.AppendLine("    com.LNS0008_GUIDANCE A01")
        '請求ヘッダーデータ(承認済み)    ※担当者用
        SQLBldr.AppendLine("    LEFT JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            RQSTAFF")                                   '担当者ID
        SQLBldr.AppendLine("            , COUNT(*) AS KENSU")                       '件数(承認済み)
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0064_INVOICEHEAD")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            REQUESTSTATUS = '5'")
        SQLBldr.AppendLine("            AND DELFLG = 0")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            RQSTAFF) A02")
        SQLBldr.AppendLine("        ON A02.RQSTAFF = '" & userid & "'")
        '請求ヘッダーデータ(差し戻し)    ※担当者用
        SQLBldr.AppendLine("    LEFT JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            RQSTAFF")                                   '担当者ID
        SQLBldr.AppendLine("            , COUNT(*) AS KENSU")                       '件数(承認済み)
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0064_INVOICEHEAD")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            REQUESTSTATUS = '1'")
        SQLBldr.AppendLine("            AND DELFLG = 0")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            RQSTAFF) A03")
        SQLBldr.AppendLine("        ON A03.RQSTAFF = '" & userid & "'")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        '検索条件追加
        SQLBldr.AppendLine("    A01.DELFLG = 0")
        SQLBldr.AppendLine("    AND A01.PRGRMID = 'LNT0007'")                       '機能ID
        SQLBldr.AppendLine("    AND A01.PRGRMKBN = '1'")                            '機能区分
        SQLBldr.AppendLine("    AND (")                                             '件数が一つもない場合、対象外
        SQLBldr.AppendLine("       (A02.KENSU IS NOT NULL AND A02.KENSU <> '0')")
        SQLBldr.AppendLine("    OR (A03.KENSU IS NOT NULL AND A03.KENSU <> '0')")
        SQLBldr.AppendLine("        )")

        '並び順
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    A01.GUIDANCENO")
        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

    ''' <summary>
    ''' ガイダンス収入管理_承認者　取得
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="userid">ログインユーザーID</param>
    ''' <remarks>データ行オブジェクト</remarks>
    Public Shared Function GetLNT0007Approval(sqlCon As MySqlConnection, sqlTran As MySqlTransaction, userid As String) As DataTable
        Dim dt = New DataTable

        '◯データ検索SQL
        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    A01.GUIDANCENO AS GUIDANCENO")                                                                      'ガイダンス№
        SQLBldr.AppendLine("    , A01.TYPE AS TYPE")                                                                                '種類
        SQLBldr.AppendLine("    , A01.TITLE AS TITLE")                                                                              'タイトル
        SQLBldr.AppendLine("    , REPLACE(A01.NAIYOU, '@1', coalesce(A02.KENSU, 0)) AS NAIYOU")                                       '請求番号
        SQLBldr.AppendLine("FROM")
        'メイン ガイダンスマスタ
        SQLBldr.AppendLine("    com.LNS0008_GUIDANCE A01")
        '請求ヘッダーデータ(確認依頼中)   ※承認者用
        SQLBldr.AppendLine("    LEFT JOIN (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            RQACKNOWLEDGER")                            '承認者ID
        SQLBldr.AppendLine("            , COUNT(*) AS KENSU")                       '件数(確認依頼中)
        SQLBldr.AppendLine("        FROM")
        SQLBldr.AppendLine("            lng.LNT0064_INVOICEHEAD")
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            REQUESTSTATUS = '3'")
        SQLBldr.AppendLine("            AND DELFLG = 0")
        SQLBldr.AppendLine("        GROUP BY")
        SQLBldr.AppendLine("            RQACKNOWLEDGER) A02")
        SQLBldr.AppendLine("        ON A02.RQACKNOWLEDGER = '" & userid & "'")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        '検索条件追加
        SQLBldr.AppendLine("    A01.DELFLG = 0")
        SQLBldr.AppendLine("    AND A01.PRGRMID = 'LNT0007'")                       '機能ID
        SQLBldr.AppendLine("    AND A01.PRGRMKBN = '2'")                            '機能区分
        SQLBldr.AppendLine("    AND (")                                             '件数が一つもない場合、対象外
        SQLBldr.AppendLine("       (A02.KENSU IS NOT NULL AND A02.KENSU <> '0')")
        SQLBldr.AppendLine("        )")

        '並び順
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    A01.GUIDANCENO")
        Using SQLcmd As New MySqlCommand(SQLBldr.ToString, sqlCon, sqlTran)

            'SQL実行
            Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                dt.Load(SQLdr)
            End Using

        End Using

        '取得データ返却
        Return dt

    End Function

    Public Shared Function BlankToDBNull(strTarger As Object) As Object

        If strTarger Is Nothing Then
            Return CType(DBNull.Value, Object)
        ElseIf strTarger.ToString.Trim = "" Then
            Return CType(DBNull.Value, Object)
        Else
            Return strTarger
        End If

    End Function

End Class
