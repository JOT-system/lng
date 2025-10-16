Option Explicit On
Imports MySql.Data.MySqlClient
Imports System.Data.SqlClient
Public Class YusouhiUpdate
    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    Private Master As LNGMasterPage
    Private TaishoYm As String

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    Public Sub New(ByVal iMaster As LNGMasterPage, ByVal iYM As String)
        Master = iMaster
        TaishoYm = iYM
    End Sub

    ''' <summary>
    ''' 輸送費テーブル更新
    ''' </summary>
    Public Sub YusouhiTblUpd(ByVal iTori As String)

        Dim ToriCodeArray() As String = iTori.Split(",")

        Try
            For Each ToriCode As String In ToriCodeArray

                '荷主選択
                Select Case ToriCode
                    Case CONST_TORICODE_0005700000    'ＥＮＥＯＳ株式会社ガス事業部
                        ENEOS_Update(ToriCode, TaishoYm)
                    Case CONST_TORICODE_0045200000    'エスケイ産業株式会社
                        ESUKEI_Update(ToriCode, TaishoYm)
                    Case CONST_TORICODE_0045300000    'エスジーリキッドサービス株式会社
                        SAIBUGUS_Update(ToriCode, TaishoYm)
                    Case CONST_TORICODE_0051200000    'Ｄａｉｇａｓエナジー株式会社液化ガスエネ
                        OG_Update(ToriCode, TaishoYm)
                    Case CONST_TORICODE_0110600000    '株式会社シーエナジー
                        CENALNESU_Update(ToriCode, TaishoYm)
                    Case CONST_TORICODE_0132800000    '石油資源開発株式会社営業本部
                        SEKIYUHOKKAIDO_Update(ToriCode, TaishoYm)
                        SEKIYUHONSYU_Update(ToriCode, TaishoYm)
                    Case CONST_TORICODE_0167600000    '東京ガスケミカル株式会社
                        TOKYOGUS_Update(ToriCode, TaishoYm)
                    Case CONST_TORICODE_0175300000    '東北天然ガス株式会社営業部
                        TNG_Update(ToriCode, TaishoYm)
                    Case CONST_TORICODE_0175400000    '東北電力株式会社グループ事業推進部
                        TOHOKU_Update(ToriCode, TaishoYm)
                    Case CONST_TORICODE_0238900000    '北陸エルネス
                        CENALNESU_Update(ToriCode, TaishoYm)
                    Case CONST_TORICODE_0239900000    '北海道ＬＮＧ株式会社
                        HOKKAIDOLNG_Update(ToriCode, TaishoYm)

                End Select
            Next

        Catch ex As Exception
            Throw
        End Try

    End Sub
    ''' <summary>
    ''' ENEOS輸送費テーブル更新
    ''' </summary>
    Private Sub ENEOS_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(ENEOS輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0016_ENEOSYUSOUHI                                 " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(ENEOS輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0016_ENEOSYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(ENEOS輸送費テーブル)
            SQLStr =
              " INSERT INTO LNG.LNT0016_ENEOSYUSOUHI(                                                                                   " _
            & "     RECONO,                                                                                                             " _
            & "     LOADUNLOTYPE,                                                                                                       " _
            & "     STACKINGTYPE,                                                                                                       " _
            & "     ORDERORGCODE,                                                                                                       " _
            & "     ORDERORGNAME,                                                                                                       " _
            & "     KASANAMEORDERORG,                                                                                                   " _
            & "     KASANCODEORDERORG,                                                                                                  " _
            & "     ORDERORG,                                                                                                           " _
            & "     PRODUCT2NAME,                                                                                                       " _
            & "     PRODUCT2,                                                                                                           " _
            & "     PRODUCT1NAME,                                                                                                       " _
            & "     PRODUCT1,                                                                                                           " _
            & "     OILNAME,                                                                                                            " _
            & "     OILTYPE,                                                                                                            " _
            & "     TODOKECODE,                                                                                                         " _
            & "     TODOKENAME,                                                                                                         " _
            & "     TODOKENAMES,                                                                                                        " _
            & "     TORICODE,                                                                                                           " _
            & "     TORINAME,                                                                                                           " _
            & "     SHUKABASHO,                                                                                                         " _
            & "     SHUKANAME,                                                                                                          " _
            & "     SHUKANAMES,                                                                                                         " _
            & "     SHUKATORICODE,                                                                                                      " _
            & "     SHUKATORINAME,                                                                                                      " _
            & "     SHUKADATE,                                                                                                          " _
            & "     LOADTIME,                                                                                                           " _
            & "     LOADTIMEIN,                                                                                                         " _
            & "     TODOKEDATE,                                                                                                         " _
            & "     SHITEITIME,                                                                                                         " _
            & "     SHITEITIMEIN,                                                                                                       " _
            & "     ZYUTYU,                                                                                                             " _
            & "     ZISSEKI,                                                                                                            " _
            & "     TANNI,                                                                                                              " _
            & "     TANKNUM,                                                                                                            " _
            & "     TANKNUMBER,                                                                                                         " _
            & "     GYOMUTANKNUM,                                                                                                       " _
            & "     SYAGATA,                                                                                                            " _
            & "     SYABARA,                                                                                                            " _
            & "     NINUSHINAME,                                                                                                        " _
            & "     CONTYPE,                                                                                                            " _
            & "     TRIP,                                                                                                               " _
            & "     DRP,                                                                                                                " _
            & "     STAFFSLCT,                                                                                                          " _
            & "     STAFFNAME,                                                                                                          " _
            & "     STAFFCODE,                                                                                                          " _
            & "     SUBSTAFFSLCT,                                                                                                       " _
            & "     SUBSTAFFNAME,                                                                                                       " _
            & "     SUBSTAFFNUM,                                                                                                        " _
            & "     SHUKODATE,                                                                                                          " _
            & "     KIKODATE,                                                                                                           " _
            & "     TANKA,                                                                                                              " _
            & "     JURYORYOKIN,                                                                                                        " _
            & "     TSUKORYO,                                                                                                           " _
            & "     KYUZITUTANKA,                                                                                                       " _
            & "     YUSOUHI,                                                                                                            " _
            & "     CALCKBN,                                                                                                            " _
            & "     WORKINGDAY,                                                                                                         " _
            & "     PUBLICHOLIDAYNAME,                                                                                                  " _
            & "     DELFLG,                                                                                                             " _
            & "     INITYMD,                                                                                                            " _
            & "     INITUSER,                                                                                                           " _
            & "     INITTERMID,                                                                                                         " _
            & "     INITPGID,                                                                                                           " _
            & "     UPDYMD,                                                                                                             " _
            & "     UPDUSER,                                                                                                            " _
            & "     UPDTERMID,                                                                                                          " _
            & "     UPDPGID,                                                                                                            " _
            & "     RECEIVEYMD)                                                                                                         " _
            & " SELECT                                                                                                                  " _
            & "     ZISSEKIMAIN.RECONO            AS RECONO,                                                                            " _
            & "     ZISSEKIMAIN.LOADUNLOTYPE      AS LOADUNLOTYPE,                                                                      " _
            & "     ZISSEKIMAIN.STACKINGTYPE      AS STACKINGTYPE,                                                                      " _
            & "     ZISSEKIMAIN.ORDERORGCODE      AS ORDERORGCODE,                                                                      " _
            & "     ZISSEKIMAIN.ORDERORGNAME      AS ORDERORGNAME,                                                                      " _
            & "     ZISSEKIMAIN.KASANAMEORDERORG  AS KASANAMEORDERORG,                                                                  " _
            & "     ZISSEKIMAIN.KASANCODEORDERORG AS KASANCODEORDERORG,                                                                 " _
            & "     ZISSEKIMAIN.ORDERORG          AS ORDERORG,                                                                          " _
            & "     ZISSEKIMAIN.PRODUCT2NAME      AS PRODUCT2NAME,                                                                      " _
            & "     ZISSEKIMAIN.PRODUCT2          AS PRODUCT2,                                                                          " _
            & "     ZISSEKIMAIN.PRODUCT1NAME      AS PRODUCT1NAME,                                                                      " _
            & "     ZISSEKIMAIN.PRODUCT1          AS PRODUCT1,                                                                          " _
            & "     ZISSEKIMAIN.OILNAME           AS OILNAME,                                                                           " _
            & "     ZISSEKIMAIN.OILTYPE           AS OILTYPE,                                                                           " _
            & "     ZISSEKIMAIN.TODOKECODE        AS TODOKECODE,                                                                        " _
            & "     ZISSEKIMAIN.TODOKENAME        AS TODOKENAME,                                                                        " _
            & "     ZISSEKIMAIN.TODOKENAMES       AS TODOKENAMES,                                                                       " _
            & "     ZISSEKIMAIN.TORICODE          AS TORICODE,                                                                          " _
            & "     ZISSEKIMAIN.TORINAME          AS TORINAME,                                                                          " _
            & "     ZISSEKIMAIN.SHUKABASHO        AS SHUKABASHO,                                                                        " _
            & "     ZISSEKIMAIN.SHUKANAME         AS SHUKANAME,                                                                         " _
            & "     ZISSEKIMAIN.SHUKANAMES        AS SHUKANAMES,                                                                        " _
            & "     ZISSEKIMAIN.SHUKATORICODE     AS SHUKATORICODE,                                                                     " _
            & "     ZISSEKIMAIN.SHUKATORINAME     AS SHUKATORINAME,                                                                     " _
            & "     ZISSEKIMAIN.SHUKADATE         AS SHUKADATE,                                                                         " _
            & "     ZISSEKIMAIN.LOADTIME          AS LOADTIME,                                                                          " _
            & "     ZISSEKIMAIN.LOADTIMEIN        AS LOADTIMEIN,                                                                        " _
            & "     ZISSEKIMAIN.TODOKEDATE        AS TODOKEDATE,                                                                        " _
            & "     ZISSEKIMAIN.SHITEITIME        AS SHITEITIME,                                                                        " _
            & "     ZISSEKIMAIN.SHITEITIMEIN      AS SHITEITIMEIN,                                                                      " _
            & "     ZISSEKIMAIN.ZYUTYU            AS ZYUTYU,                                                                            " _
            & "     ZISSEKIMAIN.ZISSEKI           AS ZISSEKI,                                                                           " _
            & "     ZISSEKIMAIN.TANNI             AS TANNI,                                                                             " _
            & "     ZISSEKIMAIN.TANKNUM           AS TANKNUM,                                                                           " _
            & "     ZISSEKIMAIN.TANKNUMBER        AS TANKNUMBER,                                                                        " _
            & "     ZISSEKIMAIN.GYOMUTANKNUM      AS GYOMUTANKNUM,                                                                      " _
            & "     ZISSEKIMAIN.SYAGATA           AS SYAGATA,                                                                           " _
            & "     ZISSEKIMAIN.SYABARA           AS SYABARA,                                                                           " _
            & "     ZISSEKIMAIN.NINUSHINAME       AS NINUSHINAME,                                                                       " _
            & "     ZISSEKIMAIN.CONTYPE           AS CONTYPE,                                                                           " _
            & "     ZISSEKIMAIN.TRIP              AS TRIP,                                                                              " _
            & "     ZISSEKIMAIN.DRP               AS DRP,                                                                               " _
            & "     ZISSEKIMAIN.STAFFSLCT         AS STAFFSLCT,                                                                         " _
            & "     ZISSEKIMAIN.STAFFNAME         AS STAFFNAME,                                                                         " _
            & "     ZISSEKIMAIN.STAFFCODE         AS STAFFCODE,                                                                         " _
            & "     ZISSEKIMAIN.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                                                                      " _
            & "     ZISSEKIMAIN.SUBSTAFFNAME      AS SUBSTAFFNAME,                                                                      " _
            & "     ZISSEKIMAIN.SUBSTAFFNUM       AS SUBSTAFFNUM,                                                                       " _
            & "     ZISSEKIMAIN.SHUKODATE         AS SHUKODATE,                                                                         " _
            & "     ZISSEKIMAIN.KIKODATE          AS KIKODATE,                                                                          " _
            & "     ZISSEKIMAIN.TANKA             AS TANKA,                                                                             " _
            & "     NULL                          AS JURYORYOKIN,                                                                       " _
            & "     NULL                          AS TSUKORYO,                                                                          " _
            & "     ZISSEKIMAIN.KYUZITUTANKA      AS KYUZITUTANKA,                                                                      " _
            & "     ZISSEKIMAIN.YUSOUHI           AS YUSOUHI,                                                                           " _
            & "     ZISSEKIMAIN.CALCKBN           AS CALCKBN,                                                                           " _
            & "     ZISSEKIMAIN.WORKINGDAY        AS WORKINGDAY,                                                                        " _
            & "     ZISSEKIMAIN.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                                 " _
            & "     ZISSEKIMAIN.DELFLG            AS DELFLG,                                                                            " _
            & "     @INITYMD                      AS INITYMD,                                                                           " _
            & "     @INITUSER                     AS INITUSER,                                                                          " _
            & "     @INITTERMID                   AS INITTERMID,                                                                        " _
            & "     @INITPGID                     AS INITPGID,                                                                          " _
            & "     NULL                          AS UPDYMD,                                                                            " _
            & "     NULL                          AS UPDUSER,                                                                           " _
            & "     NULL                          AS UPDTERMID,                                                                         " _
            & "     NULL                          AS UPDPGID,                                                                           " _
            & "     @RECEIVEYMD                   AS RECEIVEYMD                                                                         " _
            & " FROM(                                                                                                                   " _
            & "      SELECT                                                                                                             " _
            & "          ZISSEKI.RECONO            AS RECONO,                                                                           " _
            & "          ZISSEKI.LOADUNLOTYPE      AS LOADUNLOTYPE,                                                                     " _
            & "          ZISSEKI.STACKINGTYPE      AS STACKINGTYPE,                                                                     " _
            & "          ZISSEKI.ORDERORGCODE      AS ORDERORGCODE,                                                                     " _
            & "          ZISSEKI.ORDERORGNAME      AS ORDERORGNAME,                                                                     " _
            & "          ZISSEKI.KASANAMEORDERORG  AS KASANAMEORDERORG,                                                                 " _
            & "          ZISSEKI.KASANCODEORDERORG AS KASANCODEORDERORG,                                                                " _
            & "          ZISSEKI.ORDERORG          AS ORDERORG,                                                                         " _
            & "          ZISSEKI.PRODUCT2NAME      AS PRODUCT2NAME,                                                                     " _
            & "          ZISSEKI.PRODUCT2          AS PRODUCT2,                                                                         " _
            & "          ZISSEKI.PRODUCT1NAME      AS PRODUCT1NAME,                                                                     " _
            & "          ZISSEKI.PRODUCT1          AS PRODUCT1,                                                                         " _
            & "          ZISSEKI.OILNAME           AS OILNAME,                                                                          " _
            & "          ZISSEKI.OILTYPE           AS OILTYPE,                                                                          " _
            & "          ZISSEKI.TODOKECODE        AS TODOKECODE,                                                                       " _
            & "          ZISSEKI.TODOKENAME        AS TODOKENAME,                                                                       " _
            & "          ZISSEKI.TODOKENAMES       AS TODOKENAMES,                                                                      " _
            & "          ZISSEKI.TORICODE          AS TORICODE,                                                                         " _
            & "          ZISSEKI.TORINAME          AS TORINAME,                                                                         " _
            & "          CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                          " _
            & "          THEN (SELECT SHUKABASHO                                                                                        " _
            & "                  FROM LNG.LNT0001_ZISSEKI                                                                               " _
            & "                 WHERE                                                                                                   " _
            & "                       TORICODE     = ZISSEKI.TORICODE                                                                   " _
            & "                   AND ORDERORG     = ZISSEKI.ORDERORG                                                                   " _
            & "                   AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                               " _
            & "                   AND TRIP         = ZISSEKI.TRIP -1                                                                    " _
            & "                   AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                 " _
            & "                   AND DELFLG       = '0'                                                                                " _
            & "               )                                                                                                         " _
            & "          ELSE ZISSEKI.SHUKABASHO                                                                                        " _
            & "          END AS SHUKABASHO,                                                                                             " _
            & "          CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                          " _
            & "          THEN (SELECT SHUKANAME                                                                                         " _
            & "                  FROM LNG.LNT0001_ZISSEKI                                                                               " _
            & "                 WHERE                                                                                                   " _
            & "                       TORICODE     = ZISSEKI.TORICODE                                                                   " _
            & "                   AND ORDERORG     = ZISSEKI.ORDERORG                                                                   " _
            & "                   AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                               " _
            & "                   AND TRIP         = ZISSEKI.TRIP -1                                                                    " _
            & "                   AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                 " _
            & "                   AND DELFLG       = '0'                                                                                " _
            & "               )                                                                                                         " _
            & "          ELSE ZISSEKI.SHUKANAME                                                                                         " _
            & "          END AS SHUKANAME,                                                                                              " _
            & "          ZISSEKI.SHUKANAMES        AS SHUKANAMES,                                                                       " _
            & "          ZISSEKI.SHUKATORICODE     AS SHUKATORICODE,                                                                    " _
            & "          ZISSEKI.SHUKATORINAME     AS SHUKATORINAME,                                                                    " _
            & "          ZISSEKI.SHUKADATE         AS SHUKADATE,                                                                        " _
            & "          ZISSEKI.LOADTIME          AS LOADTIME,                                                                         " _
            & "          ZISSEKI.LOADTIMEIN        AS LOADTIMEIN,                                                                       " _
            & "          ZISSEKI.TODOKEDATE        AS TODOKEDATE,                                                                       " _
            & "          ZISSEKI.SHITEITIME        AS SHITEITIME,                                                                       " _
            & "          ZISSEKI.SHITEITIMEIN      AS SHITEITIMEIN,                                                                     " _
            & "          ZISSEKI.ZYUTYU            AS ZYUTYU,                                                                           " _
            & "          ZISSEKI.ZISSEKI           AS ZISSEKI,                                                                          " _
            & "          ZISSEKI.TANNI             AS TANNI,                                                                            " _
            & "          ZISSEKI.TANKNUM           AS TANKNUM,                                                                          " _
            & "          ZISSEKI.TANKNUMBER        AS TANKNUMBER,                                                                       " _
            & "          ZISSEKI.GYOMUTANKNUM      AS GYOMUTANKNUM,                                                                     " _
            & "          ZISSEKI.SYAGATA           AS SYAGATA,                                                                          " _
            & "          ZISSEKI.SYABARA           AS SYABARA,                                                                          " _
            & "          ZISSEKI.NINUSHINAME       AS NINUSHINAME,                                                                      " _
            & "          ZISSEKI.CONTYPE           AS CONTYPE,                                                                          " _
            & "          ZISSEKI.TRIP              AS TRIP,                                                                             " _
            & "          ZISSEKI.DRP               AS DRP,                                                                              " _
            & "          ZISSEKI.STAFFSLCT         AS STAFFSLCT,                                                                        " _
            & "          ZISSEKI.STAFFNAME         AS STAFFNAME,                                                                        " _
            & "          ZISSEKI.STAFFCODE         AS STAFFCODE,                                                                        " _
            & "          ZISSEKI.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                                                                     " _
            & "          ZISSEKI.SUBSTAFFNAME      AS SUBSTAFFNAME,                                                                     " _
            & "          ZISSEKI.SUBSTAFFNUM       AS SUBSTAFFNUM,                                                                      " _
            & "          ZISSEKI.SHUKODATE         AS SHUKODATE,                                                                        " _
            & "          ZISSEKI.KIKODATE          AS KIKODATE,                                                                         " _
            & "          HOLIDAYRATE.TANKA         AS KYUZITUTANKA,                                                                     " _
            & "          TANKA.TANKA               AS TANKA,                                                                            " _
            & "          CASE TANKA.CALCKBN                                                                                             " _
            & "            WHEN 'トン' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                                     " _
            & "            WHEN '回'   THEN COALESCE(TANKA.TANKA, 0)                                                                    " _
            & "            WHEN '距離' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0)                                     " _
            & "            WHEN '定数' THEN COALESCE(TANKA.TANKA, 0)                                                                    " _
            & "                        ELSE COALESCE(TANKA.TANKA, 0)                                                                    " _
            & "          END                       AS YUSOUHI,                                                                          " _
            & "          TANKA.CALCKBN             AS CALCKBN,                                                                          " _
            & "          CALENDAR.WORKINGDAY       AS WORKINGDAY,                                                                       " _
            & "          CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                               " _
            & "          ZISSEKI.DELFLG            AS DELFLG                                                                            " _
            & "      FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                                   " _
            & "      LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                               " _
            & "          ON @TORICODE = TANKA.TORICODE                                                                                  " _
            & "          And ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                                       " _
            & "          And ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                             " _
            & "          And ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                               " _
            & "          And REPLACE(ZISSEKI.SYAGATA, '単車タンク', '単車') = TANKA.SYAGATANAME                                         " _
            & "          AND CASE WHEN ZISSEKI.TODOKECODE = '005509' THEN ZISSEKI.SYABARA = TANKA.SYABARA ELSE 1 = 1 END                " _
            & "          AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                                                                      " _
            & "          AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                                         " _
            & "          AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                                         " _
            & "          AND TANKA.DELFLG = @DELFLG                                                                                     " _
            & "      LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                            " _
            & "          ON @TORICODE = CALENDAR.TORICODE                                                                               " _
            & "          AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                          " _
            & "          AND CALENDAR.DELFLG = @DELFLG                                                                                  " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                        " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                        " _
            & "       AND CASE HOLIDAYRATE.ORDERORGCATEGORY                                                                             " _
            & "             WHEN '1' THEN HOLIDAYRATE.ORDERORGCODE = ZISSEKI.ORDERORGCODE                                               " _
            & "             WHEN '2' THEN HOLIDAYRATE.ORDERORGCODE <> ZISSEKI.ORDERORGCODE                                              " _
            & "             ELSE 1 = 1                                                                                                  " _
            & "           END                                                                                                           " _
            & "       AND CASE HOLIDAYRATE.SHUKABASHOCATEGORY                                                                           " _
            & "             WHEN '1' THEN HOLIDAYRATE.SHUKABASHO = ZISSEKI.SHUKABASHO                                                   " _
            & "             WHEN '2' THEN HOLIDAYRATE.SHUKABASHO <> ZISSEKI.SHUKABASHO                                                  " _
            & "             ELSE 1 = 1                                                                                                  " _
            & "           END                                                                                                           " _
            & "       AND CASE HOLIDAYRATE.TODOKECATEGORY                                                                               " _
            & "             WHEN '1' THEN HOLIDAYRATE.TODOKECODE = ZISSEKI.TODOKECODE                                                   " _
            & "             WHEN '2' THEN HOLIDAYRATE.TODOKECODE <> ZISSEKI.TODOKECODE                                                  " _
            & "             ELSE 1 = 1                                                                                                  " _
            & "           END                                                                                                           " _
            & "       AND CASE WHEN HOLIDAYRATE.GYOMUTANKNUMFROM = '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                              " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM <= HOLIDAYRATE.GYOMUTANKNUMTO                                             " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO = ''                              " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM >= HOLIDAYRATE.GYOMUTANKNUMFROM                                           " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                             " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM BETWEEN HOLIDAYRATE.GYOMUTANKNUMFROM AND HOLIDAYRATE.GYOMUTANKNUMTO       " _
            & "                ELSE 1 = 1                                                                                               " _
            & "           END                                                                                                           " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                               " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                  " _
            & "      WHERE                                                                                                              " _
            & "          ZISSEKI.TORICODE = @TORICODE                                                                                   " _
            & "          AND ZISSEKI.ZISSEKI <> 0                                                                                       " _
            & "          AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                            " _
            & "          AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                              " _
            & "          AND ZISSEKI.STACKINGTYPE <> '積置'                                                                             " _
            & "          AND ZISSEKI.DELFLG = @DELFLG                                                                                   " _
            & " ) ZISSEKIMAIN                                                                                                           " _
            & " ON DUPLICATE KEY UPDATE                                                                                                 " _
            & "         RECONO                    = VALUES(RECONO),                                                                     " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                               " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                               " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                               " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                               " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                                           " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                                          " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                                   " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                               " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                                   " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                               " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                                   " _
            & "         OILNAME                   = VALUES(OILNAME),                                                                    " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                                    " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                                 " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                                 " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                                " _
            & "         TORICODE                  = VALUES(TORICODE),                                                                   " _
            & "         TORINAME                  = VALUES(TORINAME),                                                                   " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                                 " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                                  " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                                 " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                              " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                              " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                                  " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                                   " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                                 " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                                 " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                                 " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                               " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                                     " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                                    " _
            & "         TANNI                     = VALUES(TANNI),                                                                      " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                                    " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                                 " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                               " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                                    " _
            & "         SYABARA                   = VALUES(SYABARA),                                                                    " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                                " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                                    " _
            & "         TRIP                      = VALUES(TRIP),                                                                       " _
            & "         DRP                       = VALUES(DRP),                                                                        " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                                  " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                                  " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                                  " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                               " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                               " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                                " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                                  " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                                   " _
            & "         TANKA                     = VALUES(TANKA),                                                                      " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                                " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                                   " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                               " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                                    " _
            & "         CALCKBN                   = VALUES(CALCKBN),                                                                    " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                                 " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                                          " _
            & "         DELFLG                    = @DELFLG,                                                                            " _
            & "         UPDYMD                    = @UPDYMD,                                                                            " _
            & "         UPDUSER                   = @UPDUSER,                                                                           " _
            & "         UPDTERMID                 = @UPDTERMID,                                                                         " _
            & "         UPDPGID                   = @UPDPGID,                                                                           " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                                        "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(ENEOS輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(TaishoYm) AndAlso IsDate(TaishoYm & "/01") Then
                        YMDFROM.Value = TaishoYm & "/01"
                        YMDTO.Value = TaishoYm & DateTime.DaysInMonth(CDate(TaishoYm).Year, CDate(TaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0016_ENEOSYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' エスケイ輸送費テーブル更新
    ''' </summary>
    Private Sub ESUKEI_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(エスケイ輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0020_ESUKEIYUSOUHI                                 " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(エスケイ輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0020_ESUKEIYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(エスケイ輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0020_ESUKEIYUSOUHI(                                                                   " _
            & "        RECONO,                                                                                              " _
            & "        LOADUNLOTYPE,                                                                                        " _
            & "        STACKINGTYPE,                                                                                        " _
            & "        ORDERORGCODE,                                                                                        " _
            & "        ORDERORGNAME,                                                                                        " _
            & "        KASANAMEORDERORG,                                                                                    " _
            & "        KASANCODEORDERORG,                                                                                   " _
            & "        ORDERORG,                                                                                            " _
            & "        PRODUCT2NAME,                                                                                        " _
            & "        PRODUCT2,                                                                                            " _
            & "        PRODUCT1NAME,                                                                                        " _
            & "        PRODUCT1,                                                                                            " _
            & "        OILNAME,                                                                                             " _
            & "        OILTYPE,                                                                                             " _
            & "        TODOKECODE,                                                                                          " _
            & "        TODOKENAME,                                                                                          " _
            & "        TODOKENAMES,                                                                                         " _
            & "        TORICODE,                                                                                            " _
            & "        TORINAME,                                                                                            " _
            & "        SHUKABASHO,                                                                                          " _
            & "        SHUKANAME,                                                                                           " _
            & "        SHUKANAMES,                                                                                          " _
            & "        SHUKATORICODE,                                                                                       " _
            & "        SHUKATORINAME,                                                                                       " _
            & "        SHUKADATE,                                                                                           " _
            & "        LOADTIME,                                                                                            " _
            & "        LOADTIMEIN,                                                                                          " _
            & "        TODOKEDATE,                                                                                          " _
            & "        SHITEITIME,                                                                                          " _
            & "        SHITEITIMEIN,                                                                                        " _
            & "        ZYUTYU,                                                                                              " _
            & "        ZISSEKI,                                                                                             " _
            & "        TANNI,                                                                                               " _
            & "        TANKNUM,                                                                                             " _
            & "        TANKNUMBER,                                                                                          " _
            & "        GYOMUTANKNUM,                                                                                        " _
            & "        SYAGATA,                                                                                             " _
            & "        SYABARA,                                                                                             " _
            & "        NINUSHINAME,                                                                                         " _
            & "        CONTYPE,                                                                                             " _
            & "        TRIP,                                                                                                " _
            & "        DRP,                                                                                                 " _
            & "        STAFFSLCT,                                                                                           " _
            & "        STAFFNAME,                                                                                           " _
            & "        STAFFCODE,                                                                                           " _
            & "        SUBSTAFFSLCT,                                                                                        " _
            & "        SUBSTAFFNAME,                                                                                        " _
            & "        SUBSTAFFNUM,                                                                                         " _
            & "        SHUKODATE,                                                                                           " _
            & "        KIKODATE,                                                                                            " _
            & "        TANKA,                                                                                               " _
            & "        JURYORYOKIN,                                                                                         " _
            & "        TSUKORYO,                                                                                            " _
            & "        KYUZITUTANKA,                                                                                        " _
            & "        YUSOUHI,                                                                                             " _
            & "        CALCKBN,                                                                                             " _
            & "        WORKINGDAY,                                                                                          " _
            & "        PUBLICHOLIDAYNAME,                                                                                   " _
            & "        DELFLG,                                                                                              " _
            & "        INITYMD,                                                                                             " _
            & "        INITUSER,                                                                                            " _
            & "        INITTERMID,                                                                                          " _
            & "        INITPGID,                                                                                            " _
            & "        UPDYMD,                                                                                              " _
            & "        UPDUSER,                                                                                             " _
            & "        UPDTERMID,                                                                                           " _
            & "        UPDPGID,                                                                                             " _
            & "        RECEIVEYMD)                                                                                          " _
            & "    SELECT                                                                                                   " _
            & "        ZISSEKI.RECONO             AS RECONO,                                                                " _
            & "        ZISSEKI.LOADUNLOTYPE       AS LOADUNLOTYPE,                                                          " _
            & "        ZISSEKI.STACKINGTYPE       AS STACKINGTYPE,                                                          " _
            & "        ZISSEKI.ORDERORGCODE       AS ORDERORGCODE,                                                          " _
            & "        ZISSEKI.ORDERORGNAME       AS ORDERORGNAME,                                                          " _
            & "        ZISSEKI.KASANAMEORDERORG   AS KASANAMEORDERORG,                                                      " _
            & "        ZISSEKI.KASANCODEORDERORG  AS KASANCODEORDERORG,                                                     " _
            & "        ZISSEKI.ORDERORG           AS ORDERORG,                                                              " _
            & "        ZISSEKI.PRODUCT2NAME       AS PRODUCT2NAME,                                                          " _
            & "        ZISSEKI.PRODUCT2           AS PRODUCT2,                                                              " _
            & "        ZISSEKI.PRODUCT1NAME       AS PRODUCT1NAME,                                                          " _
            & "        ZISSEKI.PRODUCT1           AS PRODUCT1,                                                              " _
            & "        ZISSEKI.OILNAME            AS OILNAME,                                                               " _
            & "        ZISSEKI.OILTYPE            AS OILTYPE,                                                               " _
            & "        ZISSEKI.TODOKECODE         AS TODOKECODE,                                                            " _
            & "        ZISSEKI.TODOKENAME         AS TODOKENAME,                                                            " _
            & "        ZISSEKI.TODOKENAMES        AS TODOKENAMES,                                                           " _
            & "        ZISSEKI.TORICODE           AS TORICODE,                                                              " _
            & "        ZISSEKI.TORINAME           AS TORINAME,                                                              " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKABASHO                                                                              " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "               WHERE                                                                                         " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                                         " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                                         " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                     " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                                          " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                       " _
            & "                 AND DELFLG       = '0'                                                                      " _
            & "             )                                                                                               " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                                              " _
            & "        END AS SHUKABASHO,                                                                                   " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKANAME                                                                               " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "               WHERE                                                                                         " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                                         " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                                         " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                     " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                                          " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                       " _
            & "                 AND DELFLG       = '0'                                                                      " _
            & "             )                                                                                               " _
            & "        ELSE ZISSEKI.SHUKANAME                                                                               " _
            & "        END AS SHUKANAME,                                                                                    " _
            & "        ZISSEKI.SHUKANAMES         AS SHUKANAMES,                                                            " _
            & "        ZISSEKI.SHUKATORICODE      AS SHUKATORICODE,                                                         " _
            & "        ZISSEKI.SHUKATORINAME      AS SHUKATORINAME,                                                         " _
            & "        ZISSEKI.SHUKADATE          AS SHUKADATE,                                                             " _
            & "        ZISSEKI.LOADTIME           AS LOADTIME,                                                              " _
            & "        ZISSEKI.LOADTIMEIN         AS LOADTIMEIN,                                                            " _
            & "        ZISSEKI.TODOKEDATE         AS TODOKEDATE,                                                            " _
            & "        ZISSEKI.SHITEITIME         AS SHITEITIME,                                                            " _
            & "        ZISSEKI.SHITEITIMEIN       AS SHITEITIMEIN,                                                          " _
            & "        ZISSEKI.ZYUTYU             AS ZYUTYU,                                                                " _
            & "        ZISSEKI.ZISSEKI            AS ZISSEKI,                                                               " _
            & "        ZISSEKI.TANNI              AS TANNI,                                                                 " _
            & "        ZISSEKI.TANKNUM            AS TANKNUM,                                                               " _
            & "        ZISSEKI.TANKNUMBER         AS TANKNUMBER,                                                            " _
            & "        ZISSEKI.GYOMUTANKNUM       AS GYOMUTANKNUM,                                                          " _
            & "        ZISSEKI.SYAGATA            AS SYAGATA,                                                               " _
            & "        ZISSEKI.SYABARA            AS SYABARA,                                                               " _
            & "        ZISSEKI.NINUSHINAME        AS NINUSHINAME,                                                           " _
            & "        ZISSEKI.CONTYPE            AS CONTYPE,                                                               " _
            & "        ZISSEKI.TRIP               AS TRIP,                                                                  " _
            & "        ZISSEKI.DRP                AS DRP,                                                                   " _
            & "        ZISSEKI.STAFFSLCT          AS STAFFSLCT,                                                             " _
            & "        ZISSEKI.STAFFNAME          AS STAFFNAME,                                                             " _
            & "        ZISSEKI.STAFFCODE          AS STAFFCODE,                                                             " _
            & "        ZISSEKI.SUBSTAFFSLCT       AS SUBSTAFFSLCT,                                                          " _
            & "        ZISSEKI.SUBSTAFFNAME       AS SUBSTAFFNAME,                                                          " _
            & "        ZISSEKI.SUBSTAFFNUM        AS SUBSTAFFNUM,                                                           " _
            & "        ZISSEKI.SHUKODATE          AS SHUKODATE,                                                             " _
            & "        ZISSEKI.KIKODATE           AS KIKODATE,                                                              " _
            & "        TANKA.TANKA                AS TANKA,                                                                 " _
            & "        NULL                       AS JURYORYOKIN,                                                           " _
            & "        NULL                       AS TSUKORYO,                                                              " _
            & "        HOLIDAYRATE.TANKA          AS KYUZITUTANKA,                                                          " _
            & "        CASE TANKA.CALCKBN                                                                                   " _
            & "          WHEN 'トン' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                           " _
            & "          WHEN '回'   THEN COALESCE(TANKA.TANKA, 0)                                                          " _
            & "          WHEN '距離' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0)                           " _
            & "          WHEN '定数' THEN COALESCE(TANKA.TANKA, 0)                                                          " _
            & "                      ELSE COALESCE(TANKA.TANKA, 0)                                                          " _
            & "        END                         AS YUSOUHI,                                                              " _
            & "        TANKA.CALCKBN               AS CALCKBN,                                                              " _
            & "        CALENDAR.WORKINGDAY        AS WORKINGDAY,                                                            " _
            & "        CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                     " _
            & "        ZISSEKI.DELFLG             AS DELFLG,                                                                " _
            & "        @INITYMD                   AS INITYMD,                                                               " _
            & "        @INITUSER                  AS INITUSER,                                                              " _
            & "        @INITTERMID                AS INITTERMID,                                                            " _
            & "        @INITPGID                  AS INITPGID,                                                              " _
            & "        NULL                       AS UPDYMD,                                                                " _
            & "        NULL                       AS UPDUSER,                                                               " _
            & "        NULL                       AS UPDTERMID,                                                             " _
            & "        NULL                       AS UPDPGID,                                                               " _
            & "        @RECEIVEYMD                AS RECEIVEYMD                                                             " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                         " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                     " _
            & "        ON @TORICODE = TANKA.TORICODE                                                                        " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                             " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                   " _
            & "        AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                     " _
            & "        AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.BRANCHCODE = ZISSEKI.BRANCHCODE                                                            " _
            & "        AND TANKA.DELFLG = @DELFLG                                                                           " _
            & "    LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                  " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                                     " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                                        " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                           " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                           " _
            & "       AND CASE HOLIDAYRATE.ORDERORGCATEGORY                                                                                " _
            & "             WHEN '1' THEN HOLIDAYRATE.ORDERORGCODE = ZISSEKI.ORDERORGCODE                                                  " _
            & "             WHEN '2' THEN HOLIDAYRATE.ORDERORGCODE <> ZISSEKI.ORDERORGCODE                                                 " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE HOLIDAYRATE.SHUKABASHOCATEGORY                                                                              " _
            & "             WHEN '1' THEN HOLIDAYRATE.SHUKABASHO = ZISSEKI.SHUKABASHO                                                      " _
            & "             WHEN '2' THEN HOLIDAYRATE.SHUKABASHO <> ZISSEKI.SHUKABASHO                                                     " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE HOLIDAYRATE.TODOKECATEGORY                                                                                  " _
            & "             WHEN '1' THEN HOLIDAYRATE.TODOKECODE = ZISSEKI.TODOKECODE                                                      " _
            & "             WHEN '2' THEN HOLIDAYRATE.TODOKECODE <> ZISSEKI.TODOKECODE                                                     " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE WHEN HOLIDAYRATE.GYOMUTANKNUMFROM = '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                 " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM <= HOLIDAYRATE.GYOMUTANKNUMTO                                                " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO = ''                                 " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM >= HOLIDAYRATE.GYOMUTANKNUMFROM                                              " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM BETWEEN HOLIDAYRATE.GYOMUTANKNUMFROM AND HOLIDAYRATE.GYOMUTANKNUMTO          " _
            & "                ELSE 1 = 1                                                                                                  " _
            & "           END                                                                                                              " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                  " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                     " _
            & "    WHERE                                                                                                    " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                                         " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                                             " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                                                   " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                  " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                    " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                                         " _
            & "    ORDER BY                                                                                                 " _
            & "       SHUKADATE,                                                                                            " _
            & "       TODOKEDATE                                                                                            " _
            & " ON DUPLICATE KEY UPDATE                                                                                     " _
            & "         RECONO                    = VALUES(RECONO),                                                         " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                   " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                   " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                   " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                   " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                               " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                              " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                       " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                   " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                       " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                   " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                       " _
            & "         OILNAME                   = VALUES(OILNAME),                                                        " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                        " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                     " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                     " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                    " _
            & "         TORICODE                  = VALUES(TORICODE),                                                       " _
            & "         TORINAME                  = VALUES(TORINAME),                                                       " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                     " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                      " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                     " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                  " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                  " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                      " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                       " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                     " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                     " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                     " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                   " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                         " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                        " _
            & "         TANNI                     = VALUES(TANNI),                                                          " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                        " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                     " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                   " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                        " _
            & "         SYABARA                   = VALUES(SYABARA),                                                        " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                    " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                        " _
            & "         TRIP                      = VALUES(TRIP),                                                           " _
            & "         DRP                       = VALUES(DRP),                                                            " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                      " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                      " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                      " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                   " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                   " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                    " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                      " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                       " _
            & "         TANKA                     = VALUES(TANKA),                                                          " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                    " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                       " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                   " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                        " _
            & "         CALCKBN                   = VALUES(CALCKBN),                                                        " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                     " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                              " _
            & "         DELFLG                    = @DELFLG,                                                                " _
            & "         UPDYMD                    = @UPDYMD,                                                                " _
            & "         UPDUSER                   = @UPDUSER,                                                               " _
            & "         UPDTERMID                 = @UPDTERMID,                                                             " _
            & "         UPDPGID                   = @UPDPGID,                                                               " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                            "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(エスケイ輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0020_ESUKEIYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 西部ガス輸送費テーブル更新
    ''' </summary>
    Private Sub SAIBUGUS_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(西部ガス輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0019_SAIBUGUSYUSOUHI                              " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(西部ガス輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019_SAIBUGUSYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(西部ガス輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0019_SAIBUGUSYUSOUHI(                                             " _
            & "        RECONO,                                                                          " _
            & "        LOADUNLOTYPE,                                                                    " _
            & "        STACKINGTYPE,                                                                    " _
            & "        ORDERORGCODE,                                                                    " _
            & "        ORDERORGNAME,                                                                    " _
            & "        KASANAMEORDERORG,                                                                " _
            & "        KASANCODEORDERORG,                                                               " _
            & "        ORDERORG,                                                                        " _
            & "        PRODUCT2NAME,                                                                    " _
            & "        PRODUCT2,                                                                        " _
            & "        PRODUCT1NAME,                                                                    " _
            & "        PRODUCT1,                                                                        " _
            & "        OILNAME,                                                                         " _
            & "        OILTYPE,                                                                         " _
            & "        TODOKECODE,                                                                      " _
            & "        TODOKENAME,                                                                      " _
            & "        TODOKENAMES,                                                                     " _
            & "        TORICODE,                                                                        " _
            & "        TORINAME,                                                                        " _
            & "        SHUKABASHO,                                                                      " _
            & "        SHUKANAME,                                                                       " _
            & "        SHUKANAMES,                                                                      " _
            & "        SHUKATORICODE,                                                                   " _
            & "        SHUKATORINAME,                                                                   " _
            & "        SHUKADATE,                                                                       " _
            & "        LOADTIME,                                                                        " _
            & "        LOADTIMEIN,                                                                      " _
            & "        TODOKEDATE,                                                                      " _
            & "        SHITEITIME,                                                                      " _
            & "        SHITEITIMEIN,                                                                    " _
            & "        ZYUTYU,                                                                          " _
            & "        ZISSEKI,                                                                         " _
            & "        TANNI,                                                                           " _
            & "        TANKNUM,                                                                         " _
            & "        TANKNUMBER,                                                                      " _
            & "        GYOMUTANKNUM,                                                                    " _
            & "        SYAGATA,                                                                         " _
            & "        SYABARA,                                                                         " _
            & "        NINUSHINAME,                                                                     " _
            & "        CONTYPE,                                                                         " _
            & "        TRIP,                                                                            " _
            & "        DRP,                                                                             " _
            & "        STAFFSLCT,                                                                       " _
            & "        STAFFNAME,                                                                       " _
            & "        STAFFCODE,                                                                       " _
            & "        SUBSTAFFSLCT,                                                                    " _
            & "        SUBSTAFFNAME,                                                                    " _
            & "        SUBSTAFFNUM,                                                                     " _
            & "        SHUKODATE,                                                                       " _
            & "        KIKODATE,                                                                        " _
            & "        TANKA,                                                                           " _
            & "        JURYORYOKIN,                                                                     " _
            & "        TSUKORYO,                                                                        " _
            & "        KYUZITUTANKA,                                                                    " _
            & "        YUSOUHI,                                                                         " _
            & "        CALCKBN,                                                                         " _
            & "        WORKINGDAY,                                                                      " _
            & "        PUBLICHOLIDAYNAME,                                                               " _
            & "        DELFLG,                                                                          " _
            & "        INITYMD,                                                                         " _
            & "        INITUSER,                                                                        " _
            & "        INITTERMID,                                                                      " _
            & "        INITPGID,                                                                        " _
            & "        UPDYMD,                                                                          " _
            & "        UPDUSER,                                                                         " _
            & "        UPDTERMID,                                                                       " _
            & "        UPDPGID,                                                                         " _
            & "        RECEIVEYMD)                                                                      " _
            & "    SELECT                                                                               " _
            & "        ZISSEKI.RECONO             AS RECONO,                                            " _
            & "        ZISSEKI.LOADUNLOTYPE       AS LOADUNLOTYPE,                                      " _
            & "        ZISSEKI.STACKINGTYPE       AS STACKINGTYPE,                                      " _
            & "        ZISSEKI.ORDERORGCODE       AS ORDERORGCODE,                                      " _
            & "        ZISSEKI.ORDERORGNAME       AS ORDERORGNAME,                                      " _
            & "        ZISSEKI.KASANAMEORDERORG   AS KASANAMEORDERORG,                                  " _
            & "        ZISSEKI.KASANCODEORDERORG  AS KASANCODEORDERORG,                                 " _
            & "        ZISSEKI.ORDERORG           AS ORDERORG,                                          " _
            & "        ZISSEKI.PRODUCT2NAME       AS PRODUCT2NAME,                                      " _
            & "        ZISSEKI.PRODUCT2           AS PRODUCT2,                                          " _
            & "        ZISSEKI.PRODUCT1NAME       AS PRODUCT1NAME,                                      " _
            & "        ZISSEKI.PRODUCT1           AS PRODUCT1,                                          " _
            & "        ZISSEKI.OILNAME            AS OILNAME,                                           " _
            & "        ZISSEKI.OILTYPE            AS OILTYPE,                                           " _
            & "        ZISSEKI.TODOKECODE         AS TODOKECODE,                                        " _
            & "        ZISSEKI.TODOKENAME         AS TODOKENAME,                                        " _
            & "        ZISSEKI.TODOKENAMES        AS TODOKENAMES,                                       " _
            & "        ZISSEKI.TORICODE           AS TORICODE,                                          " _
            & "        ZISSEKI.TORINAME           AS TORINAME,                                          " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                            " _
            & "        THEN (SELECT SHUKABASHO                                                          " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                 " _
            & "               WHERE                                                                     " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                     " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                     " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                 " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                      " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                   " _
            & "                 AND DELFLG       = '0'                                                  " _
            & "             )                                                                           " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                          " _
            & "        END AS SHUKABASHO,                                                               " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                            " _
            & "        THEN (SELECT SHUKANAME                                                           " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                 " _
            & "               WHERE                                                                     " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                     " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                     " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                 " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                      " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                   " _
            & "                 AND DELFLG       = '0'                                                  " _
            & "             )                                                                           " _
            & "        ELSE ZISSEKI.SHUKANAME                                                           " _
            & "        END AS SHUKANAME,                                                                " _
            & "        ZISSEKI.SHUKANAMES         AS SHUKANAMES,                                        " _
            & "        ZISSEKI.SHUKATORICODE      AS SHUKATORICODE,                                     " _
            & "        ZISSEKI.SHUKATORINAME      AS SHUKATORINAME,                                     " _
            & "        ZISSEKI.SHUKADATE          AS SHUKADATE,                                         " _
            & "        ZISSEKI.LOADTIME           AS LOADTIME,                                          " _
            & "        ZISSEKI.LOADTIMEIN         AS LOADTIMEIN,                                        " _
            & "        ZISSEKI.TODOKEDATE         AS TODOKEDATE,                                        " _
            & "        ZISSEKI.SHITEITIME         AS SHITEITIME,                                        " _
            & "        ZISSEKI.SHITEITIMEIN       AS SHITEITIMEIN,                                      " _
            & "        ZISSEKI.ZYUTYU             AS ZYUTYU,                                            " _
            & "        ZISSEKI.ZISSEKI            AS ZISSEKI,                                           " _
            & "        ZISSEKI.TANNI              AS TANNI,                                             " _
            & "        ZISSEKI.TANKNUM            AS TANKNUM,                                           " _
            & "        ZISSEKI.TANKNUMBER         AS TANKNUMBER,                                        " _
            & "        ZISSEKI.GYOMUTANKNUM       AS GYOMUTANKNUM,                                      " _
            & "        ZISSEKI.SYAGATA            AS SYAGATA,                                           " _
            & "        ZISSEKI.SYABARA            AS SYABARA,                                           " _
            & "        ZISSEKI.NINUSHINAME        AS NINUSHINAME,                                       " _
            & "        ZISSEKI.CONTYPE            AS CONTYPE,                                           " _
            & "        ZISSEKI.TRIP               AS TRIP,                                              " _
            & "        ZISSEKI.DRP                AS DRP,                                               " _
            & "        ZISSEKI.STAFFSLCT          AS STAFFSLCT,                                         " _
            & "        ZISSEKI.STAFFNAME          AS STAFFNAME,                                         " _
            & "        ZISSEKI.STAFFCODE          AS STAFFCODE,                                         " _
            & "        ZISSEKI.SUBSTAFFSLCT       AS SUBSTAFFSLCT,                                      " _
            & "        ZISSEKI.SUBSTAFFNAME       AS SUBSTAFFNAME,                                      " _
            & "        ZISSEKI.SUBSTAFFNUM        AS SUBSTAFFNUM,                                       " _
            & "        ZISSEKI.SHUKODATE          AS SHUKODATE,                                         " _
            & "        ZISSEKI.KIKODATE           AS KIKODATE,                                          " _
            & "        TANKA.TANKA                AS TANKA,                                             " _
            & "        NULL                       AS JURYORYOKIN,                                       " _
            & "        NULL                       AS TSUKORYO,                                          " _
            & "        NULL                       AS KYUZITUTANKA,                                      " _
            & "        CASE TANKA.CALCKBN                                                               " _
            & "          WHEN 'トン' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)       " _
            & "          WHEN '回'   THEN COALESCE(TANKA.TANKA, 0)                                      " _
            & "          WHEN '距離' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0)       " _
            & "          WHEN '定数' THEN COALESCE(TANKA.TANKA, 0)                                      " _
            & "                      ELSE COALESCE(TANKA.TANKA, 0)                                      " _
            & "        END                        AS YUSOUHI,                                           " _
            & "        TANKA.CALCKBN              AS CALCKBN,                                           " _
            & "        CALENDAR.WORKINGDAY        AS WORKINGDAY,                                        " _
            & "        CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                 " _
            & "        ZISSEKI.DELFLG             AS DELFLG,                                            " _
            & "        @INITYMD                   AS INITYMD,                                           " _
            & "        @INITUSER                  AS INITUSER,                                          " _
            & "        @INITTERMID                AS INITTERMID,                                        " _
            & "        @INITPGID                  AS INITPGID,                                          " _
            & "        NULL                       AS UPDYMD,                                            " _
            & "        NULL                       AS UPDUSER,                                           " _
            & "        NULL                       AS UPDTERMID,                                         " _
            & "        NULL                       AS UPDPGID,                                           " _
            & "        @RECEIVEYMD                AS RECEIVEYMD                                         " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                     " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                 " _
            & "        ON @TORICODE = TANKA.TORICODE                                                    " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                         " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                               " _
            & "        AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                 " _
            & "        AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                           " _
            & "        AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                           " _
            & "        AND TANKA.DELFLG = @DELFLG                                                       " _
            & "        AND TANKA.BRANCHCODE = ZISSEKI.BRANCHCODE                                        " _
            & "    LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                              " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                 " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                            " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                    " _
            & "    WHERE                                                                                " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                     " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                         " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                               " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                              " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                     " _
            & " ON DUPLICATE KEY UPDATE                                                                 " _
            & "         RECONO                    = VALUES(RECONO),                                     " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                               " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                               " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                               " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                               " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                           " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                          " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                   " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                               " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                   " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                               " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                   " _
            & "         OILNAME                   = VALUES(OILNAME),                                    " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                    " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                 " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                 " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                " _
            & "         TORICODE                  = VALUES(TORICODE),                                   " _
            & "         TORINAME                  = VALUES(TORINAME),                                   " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                 " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                  " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                 " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                              " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                              " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                  " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                   " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                 " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                 " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                 " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                               " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                     " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                    " _
            & "         TANNI                     = VALUES(TANNI),                                      " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                    " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                 " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                               " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                    " _
            & "         SYABARA                   = VALUES(SYABARA),                                    " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                    " _
            & "         TRIP                      = VALUES(TRIP),                                       " _
            & "         DRP                       = VALUES(DRP),                                        " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                  " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                  " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                  " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                               " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                               " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                  " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                   " _
            & "         TANKA                     = VALUES(TANKA),                                      " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                   " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                               " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                    " _
            & "         CALCKBN                   = VALUES(CALCKBN),                                    " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                 " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                          " _
            & "         DELFLG                    = @DELFLG,                                            " _
            & "         UPDYMD                    = @UPDYMD,                                            " _
            & "         UPDUSER                   = @UPDUSER,                                           " _
            & "         UPDTERMID                 = @UPDTERMID,                                         " _
            & "         UPDPGID                   = @UPDPGID,                                           " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                        "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(西部ガス輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019_SAIBUGUSYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' OG輸送費テーブル更新
    ''' </summary>
    Private Sub OG_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(OG輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0022_OGYUSOUHI                                    " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(OG輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0022_OGYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(OG輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0022_OGYUSOUHI(                                                                                                       " _
            & "        RECONO,                                                                                                                              " _
            & "        LOADUNLOTYPE,                                                                                                                        " _
            & "        STACKINGTYPE,                                                                                                                        " _
            & "        ORDERORGCODE,                                                                                                                        " _
            & "        ORDERORGNAME,                                                                                                                        " _
            & "        KASANAMEORDERORG,                                                                                                                    " _
            & "        KASANCODEORDERORG,                                                                                                                   " _
            & "        ORDERORG,                                                                                                                            " _
            & "        PRODUCT2NAME,                                                                                                                        " _
            & "        PRODUCT2,                                                                                                                            " _
            & "        PRODUCT1NAME,                                                                                                                        " _
            & "        PRODUCT1,                                                                                                                            " _
            & "        OILNAME,                                                                                                                             " _
            & "        OILTYPE,                                                                                                                             " _
            & "        TODOKECODE,                                                                                                                          " _
            & "        TODOKENAME,                                                                                                                          " _
            & "        TODOKENAMES,                                                                                                                         " _
            & "        TORICODE,                                                                                                                            " _
            & "        TORINAME,                                                                                                                            " _
            & "        SHUKABASHO,                                                                                                                          " _
            & "        SHUKANAME,                                                                                                                           " _
            & "        SHUKANAMES,                                                                                                                          " _
            & "        SHUKATORICODE,                                                                                                                       " _
            & "        SHUKATORINAME,                                                                                                                       " _
            & "        SHUKADATE,                                                                                                                           " _
            & "        LOADTIME,                                                                                                                            " _
            & "        LOADTIMEIN,                                                                                                                          " _
            & "        TODOKEDATE,                                                                                                                          " _
            & "        SHITEITIME,                                                                                                                          " _
            & "        SHITEITIMEIN,                                                                                                                        " _
            & "        ZYUTYU,                                                                                                                              " _
            & "        ZISSEKI,                                                                                                                             " _
            & "        TANNI,                                                                                                                               " _
            & "        TANKNUM,                                                                                                                             " _
            & "        TANKNUMBER,                                                                                                                          " _
            & "        GYOMUTANKNUM,                                                                                                                        " _
            & "        SYAGATA,                                                                                                                             " _
            & "        SYABARA,                                                                                                                             " _
            & "        NINUSHINAME,                                                                                                                         " _
            & "        CONTYPE,                                                                                                                             " _
            & "        TRIP,                                                                                                                                " _
            & "        DRP,                                                                                                                                 " _
            & "        STAFFSLCT,                                                                                                                           " _
            & "        STAFFNAME,                                                                                                                           " _
            & "        STAFFCODE,                                                                                                                           " _
            & "        SUBSTAFFSLCT,                                                                                                                        " _
            & "        SUBSTAFFNAME,                                                                                                                        " _
            & "        SUBSTAFFNUM,                                                                                                                         " _
            & "        SHUKODATE,                                                                                                                           " _
            & "        KIKODATE,                                                                                                                            " _
            & "        TANKA,                                                                                                                               " _
            & "        JURYORYOKIN,                                                                                                                         " _
            & "        TSUKORYO,                                                                                                                            " _
            & "        KYUZITUTANKA,                                                                                                                        " _
            & "        YUSOUHI,                                                                                                                             " _
            & "        CALCKBN,                                                                                                                             " _
            & "        WORKINGDAY,                                                                                                                          " _
            & "        PUBLICHOLIDAYNAME,                                                                                                                   " _
            & "        DELFLG,                                                                                                                              " _
            & "        INITYMD,                                                                                                                             " _
            & "        INITUSER,                                                                                                                            " _
            & "        INITTERMID,                                                                                                                          " _
            & "        INITPGID,                                                                                                                            " _
            & "        UPDYMD,                                                                                                                              " _
            & "        UPDUSER,                                                                                                                             " _
            & "        UPDTERMID,                                                                                                                           " _
            & "        UPDPGID,                                                                                                                             " _
            & "        RECEIVEYMD)                                                                                                                          " _
            & "    SELECT                                                                                                                                   " _
            & "        ZISSEKI.RECONO             AS RECONO,                                                                                                " _
            & "        ZISSEKI.LOADUNLOTYPE       AS LOADUNLOTYPE,                                                                                          " _
            & "        ZISSEKI.STACKINGTYPE       AS STACKINGTYPE,                                                                                          " _
            & "        ZISSEKI.ORDERORGCODE       AS ORDERORGCODE,                                                                                          " _
            & "        ZISSEKI.ORDERORGNAME       AS ORDERORGNAME,                                                                                          " _
            & "        ZISSEKI.KASANAMEORDERORG   AS KASANAMEORDERORG,                                                                                      " _
            & "        ZISSEKI.KASANCODEORDERORG  AS KASANCODEORDERORG,                                                                                     " _
            & "        ZISSEKI.ORDERORG           AS ORDERORG,                                                                                              " _
            & "        ZISSEKI.PRODUCT2NAME       AS PRODUCT2NAME,                                                                                          " _
            & "        ZISSEKI.PRODUCT2           AS PRODUCT2,                                                                                              " _
            & "        ZISSEKI.PRODUCT1NAME       AS PRODUCT1NAME,                                                                                          " _
            & "        ZISSEKI.PRODUCT1           AS PRODUCT1,                                                                                              " _
            & "        ZISSEKI.OILNAME            AS OILNAME,                                                                                               " _
            & "        ZISSEKI.OILTYPE            AS OILTYPE,                                                                                               " _
            & "        ZISSEKI.TODOKECODE         AS TODOKECODE,                                                                                            " _
            & "        ZISSEKI.TODOKENAME         AS TODOKENAME,                                                                                            " _
            & "        ZISSEKI.TODOKENAMES        AS TODOKENAMES,                                                                                           " _
            & "        ZISSEKI.TORICODE           AS TORICODE,                                                                                              " _
            & "        ZISSEKI.TORINAME           AS TORINAME,                                                                                              " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                                " _
            & "        THEN (SELECT SHUKABASHO                                                                                                              " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                                                     " _
            & "               WHERE                                                                                                                         " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                                                                         " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                                                                         " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                                     " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                                                                          " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                                       " _
            & "                 AND DELFLG       = '0'                                                                                                      " _
            & "             )                                                                                                                               " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                                                                              " _
            & "        END AS SHUKABASHO,                                                                                                                   " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                                " _
            & "        THEN (SELECT SHUKANAME                                                                                                               " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                                                     " _
            & "               WHERE                                                                                                                         " _
            & "                     TORICODE     = ZISSEKI.TORICODE                                                                                         " _
            & "                 AND ORDERORG     = ZISSEKI.ORDERORG                                                                                         " _
            & "                 AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                                     " _
            & "                 AND TRIP         = ZISSEKI.TRIP -1                                                                                          " _
            & "                 AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                                       " _
            & "                 AND DELFLG       = '0'                                                                                                      " _
            & "             )                                                                                                                               " _
            & "        ELSE ZISSEKI.SHUKANAME                                                                                                               " _
            & "        END AS SHUKANAME,                                                                                                                    " _
            & "        ZISSEKI.SHUKANAMES         AS SHUKANAMES,                                                                                            " _
            & "        ZISSEKI.SHUKATORICODE      AS SHUKATORICODE,                                                                                         " _
            & "        ZISSEKI.SHUKATORINAME      AS SHUKATORINAME,                                                                                         " _
            & "        ZISSEKI.SHUKADATE          AS SHUKADATE,                                                                                             " _
            & "        ZISSEKI.LOADTIME           AS LOADTIME,                                                                                              " _
            & "        ZISSEKI.LOADTIMEIN         AS LOADTIMEIN,                                                                                            " _
            & "        ZISSEKI.TODOKEDATE         AS TODOKEDATE,                                                                                            " _
            & "        ZISSEKI.SHITEITIME         AS SHITEITIME,                                                                                            " _
            & "        ZISSEKI.SHITEITIMEIN       AS SHITEITIMEIN,                                                                                          " _
            & "        ZISSEKI.ZYUTYU             AS ZYUTYU,                                                                                                " _
            & "        ZISSEKI.ZISSEKI            AS ZISSEKI,                                                                                               " _
            & "        ZISSEKI.TANNI              AS TANNI,                                                                                                 " _
            & "        ZISSEKI.TANKNUM            AS TANKNUM,                                                                                               " _
            & "        ZISSEKI.TANKNUMBER         AS TANKNUMBER,                                                                                            " _
            & "        ZISSEKI.GYOMUTANKNUM       AS GYOMUTANKNUM,                                                                                          " _
            & "        ZISSEKI.SYAGATA            AS SYAGATA,                                                                                               " _
            & "        ZISSEKI.SYABARA            AS SYABARA,                                                                                               " _
            & "        ZISSEKI.NINUSHINAME        AS NINUSHINAME,                                                                                           " _
            & "        ZISSEKI.CONTYPE            AS CONTYPE,                                                                                               " _
            & "        ZISSEKI.TRIP               AS TRIP,                                                                                                  " _
            & "        ZISSEKI.DRP                AS DRP,                                                                                                   " _
            & "        ZISSEKI.STAFFSLCT          AS STAFFSLCT,                                                                                             " _
            & "        ZISSEKI.STAFFNAME          AS STAFFNAME,                                                                                             " _
            & "        ZISSEKI.STAFFCODE          AS STAFFCODE,                                                                                             " _
            & "        ZISSEKI.SUBSTAFFSLCT       AS SUBSTAFFSLCT,                                                                                          " _
            & "        ZISSEKI.SUBSTAFFNAME       AS SUBSTAFFNAME,                                                                                          " _
            & "        ZISSEKI.SUBSTAFFNUM        AS SUBSTAFFNUM,                                                                                           " _
            & "        ZISSEKI.SHUKODATE          AS SHUKODATE,                                                                                             " _
            & "        ZISSEKI.KIKODATE           AS KIKODATE,                                                                                              " _
            & "        CASE                                                                                                                                 " _
            & "            WHEN ZISSEKI.ORDERORGCODE = '022702'                                                                                             " _
            & "                THEN TANKA_SENBOKU.TANKA                                                                                                     " _
            & "            WHEN ZISSEKI.ORDERORGCODE = '022801'                                                                                             " _
            & "                THEN TANKA_HIMEZI.TANKA                                                                                                      " _
            & "        END                        AS TANKA,                                                                                                 " _
            & "        NULL                       AS JURYORYOKIN,                                                                                           " _
            & "        NULL                       AS TSUKORYO,                                                                                              " _
            & "        HOLIDAYRATE.TANKA          AS KYUZITUTANKA,                                                                                          " _
            & "        CASE                                                                                                                                 " _
            & "            WHEN ZISSEKI.ORDERORGCODE = '022702'                                                                                             " _
            & "                THEN                                                                                                                         " _
            & "                     CASE TANKA_SENBOKU.CALCKBN                                                                                              " _
            & "                         WHEN 'トン' THEN COALESCE(TANKA_SENBOKU.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                                    " _
            & "                         WHEN '回'   THEN COALESCE(TANKA_SENBOKU.TANKA, 0)                                                                   " _
            & "                         WHEN '距離' THEN COALESCE(TANKA_SENBOKU.TANKA, 0) * COALESCE(TANKA_SENBOKU.ROUNDTRIP, 0)                            " _
            & "                         WHEN '定数' THEN COALESCE(TANKA_SENBOKU.TANKA, 0)                                                                   " _
            & "                                     ELSE COALESCE(TANKA_SENBOKU.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                                    " _
            & "                     END                                                                                                                     " _
            & "            WHEN ZISSEKI.ORDERORGCODE = '022801'                                                                                             " _
            & "                THEN                                                                                                                         " _
            & "                     CASE TANKA_HIMEZI.CALCKBN                                                                                               " _
            & "                         WHEN 'トン' THEN COALESCE(TANKA_HIMEZI.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                                     " _
            & "                         WHEN '回'   THEN COALESCE(TANKA_HIMEZI.TANKA, 0)                                                                    " _
            & "                         WHEN '距離' THEN COALESCE(TANKA_HIMEZI.TANKA, 0) * COALESCE(TANKA_HIMEZI.ROUNDTRIP, 0)                              " _
            & "                         WHEN '定数' THEN COALESCE(TANKA_HIMEZI.TANKA, 0)                                                                    " _
            & "                                     ELSE COALESCE(TANKA_HIMEZI.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                                     " _
            & "                     END                                                                                                                     " _
            & "        END                        AS YUSOUHI,                                                                                               " _
            & "        CASE                                                                                                                                 " _
            & "            WHEN ZISSEKI.ORDERORGCODE = '022702'                                                                                             " _
            & "                THEN TANKA_SENBOKU.CALCKBN                                                                                                   " _
            & "            WHEN ZISSEKI.ORDERORGCODE = '022801'                                                                                             " _
            & "                THEN TANKA_HIMEZI.CALCKBN                                                                                                    " _
            & "        END                        AS CALCKBN,                                                                                               " _
            & "        CALENDAR.WORKINGDAY        AS WORKINGDAY,                                                                                            " _
            & "        CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                                                     " _
            & "        ZISSEKI.DELFLG             AS DELFLG,                                                                                                " _
            & "        @INITYMD                   AS INITYMD,                                                                                               " _
            & "        @INITUSER                  AS INITUSER,                                                                                              " _
            & "        @INITTERMID                AS INITTERMID,                                                                                            " _
            & "        @INITPGID                  AS INITPGID,                                                                                              " _
            & "        NULL                       AS UPDYMD,                                                                                                " _
            & "        NULL                       AS UPDUSER,                                                                                               " _
            & "        NULL                       AS UPDTERMID,                                                                                             " _
            & "        NULL                       AS UPDPGID,                                                                                               " _
            & "        @RECEIVEYMD                AS RECEIVEYMD                                                                                             " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                                                         " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA_SENBOKU                                                                                             " _
            & "        ON @TORICODE = TANKA_SENBOKU.TORICODE                                                                                                " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA_SENBOKU.ORGCODE                                                                                     " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA_SENBOKU.KASANORGCODE                                                                           " _
            & "        AND ZISSEKI.TODOKECODE = TANKA_SENBOKU.AVOCADOTODOKECODE                                                                             " _
            & "        AND ZISSEKI.SYABARA = TANKA_SENBOKU.SYABARA                                                                                          " _
            & "        AND TANKA_SENBOKU.STYMD  <= ZISSEKI.TODOKEDATE                                                                                       " _
            & "        AND TANKA_SENBOKU.ENDYMD >= ZISSEKI.TODOKEDATE                                                                                       " _
            & "        AND TANKA_SENBOKU.DELFLG = @DELFLG                                                                                                   " _
            & "        AND TANKA_SENBOKU.ORGCODE = '022702'                                                                                                 " _
            & "        AND TANKA_SENBOKU.BRANCHCODE = ZISSEKI.BRANCHCODE                                                                                    " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA_HIMEZI                                                                                              " _
            & "        ON @TORICODE = TANKA_HIMEZI.TORICODE                                                                                                 " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA_HIMEZI.ORGCODE                                                                                      " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA_HIMEZI.KASANORGCODE                                                                            " _
            & "        AND ZISSEKI.TODOKECODE = TANKA_HIMEZI.AVOCADOTODOKECODE                                                                              " _
            & "        AND REPLACE(ZISSEKI.SYAGATA, '単車タンク', '単車') = TANKA_HIMEZI.SYAGATANAME                                                        " _
            & "        AND TANKA_HIMEZI.STYMD  <= ZISSEKI.TODOKEDATE                                                                                        " _
            & "        AND TANKA_HIMEZI.ENDYMD >= ZISSEKI.TODOKEDATE                                                                                        " _
            & "        AND TANKA_HIMEZI.DELFLG = @DELFLG                                                                                                    " _
            & "        AND TANKA_HIMEZI.ORGCODE = '022801'                                                                                                  " _
            & "        AND TANKA_HIMEZI.BRANCHCODE = ZISSEKI.BRANCHCODE                                                                                     " _
            & "    LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                                                  " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                                                                     " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                                                " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                                                                        " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                           " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                           " _
            & "       AND CASE HOLIDAYRATE.ORDERORGCATEGORY                                                                                " _
            & "             WHEN '1' THEN HOLIDAYRATE.ORDERORGCODE = ZISSEKI.ORDERORGCODE                                                  " _
            & "             WHEN '2' THEN HOLIDAYRATE.ORDERORGCODE <> ZISSEKI.ORDERORGCODE                                                 " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE HOLIDAYRATE.SHUKABASHOCATEGORY                                                                              " _
            & "             WHEN '1' THEN HOLIDAYRATE.SHUKABASHO = ZISSEKI.SHUKABASHO                                                      " _
            & "             WHEN '2' THEN HOLIDAYRATE.SHUKABASHO <> ZISSEKI.SHUKABASHO                                                     " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE HOLIDAYRATE.TODOKECATEGORY                                                                                  " _
            & "             WHEN '1' THEN HOLIDAYRATE.TODOKECODE = ZISSEKI.TODOKECODE                                                      " _
            & "             WHEN '2' THEN HOLIDAYRATE.TODOKECODE <> ZISSEKI.TODOKECODE                                                     " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE WHEN HOLIDAYRATE.GYOMUTANKNUMFROM = '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                 " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM <= HOLIDAYRATE.GYOMUTANKNUMTO                                                " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO = ''                                 " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM >= HOLIDAYRATE.GYOMUTANKNUMFROM                                              " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM BETWEEN HOLIDAYRATE.GYOMUTANKNUMFROM AND HOLIDAYRATE.GYOMUTANKNUMTO          " _
            & "                ELSE 1 = 1                                                                                                  " _
            & "           END                                                                                                              " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                  " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                     " _
            & "    WHERE                                                                                                                                    " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                                                                         " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                                                                             " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                                                  " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                                                    " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                                                                                   " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                                                                         " _
            & " ON DUPLICATE KEY UPDATE                                                                                                                     " _
            & "         RECONO                    = VALUES(RECONO),                                                                                         " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                                                   " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                                                   " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                                                   " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                                                   " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                                                               " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                                                              " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                                                       " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                                                   " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                                                       " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                                                   " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                                                       " _
            & "         OILNAME                   = VALUES(OILNAME),                                                                                        " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                                                        " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                                                     " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                                                     " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                                                    " _
            & "         TORICODE                  = VALUES(TORICODE),                                                                                       " _
            & "         TORINAME                  = VALUES(TORINAME),                                                                                       " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                                                     " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                                                      " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                                                     " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                                                  " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                                                  " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                                                      " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                                                       " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                                                     " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                                                     " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                                                     " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                                                   " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                                                         " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                                                        " _
            & "         TANNI                     = VALUES(TANNI),                                                                                          " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                                                        " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                                                     " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                                                   " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                                                        " _
            & "         SYABARA                   = VALUES(SYABARA),                                                                                        " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                                                    " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                                                        " _
            & "         TRIP                      = VALUES(TRIP),                                                                                           " _
            & "         DRP                       = VALUES(DRP),                                                                                            " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                                                      " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                                                      " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                                                      " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                                                   " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                                                   " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                                                    " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                                                      " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                                                       " _
            & "         TANKA                     = VALUES(TANKA),                                                                                          " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                                                    " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                                                       " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                                                   " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                                                        " _
            & "         CALCKBN                   = VALUES(CALCKBN),                                                                                        " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                                                     " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                                                              " _
            & "         DELFLG                    = @DELFLG,                                                                                                " _
            & "         UPDYMD                    = @UPDYMD,                                                                                                " _
            & "         UPDUSER                   = @UPDUSER,                                                                                               " _
            & "         UPDTERMID                 = @UPDTERMID,                                                                                             " _
            & "         UPDPGID                   = @UPDPGID,                                                                                               " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                                                            "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(OG輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0022_OGYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' シーエナジーエルネス輸送費テーブル更新
    ''' </summary>
    Private Sub CENALNESU_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(シーエナジーエルネス輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0025_CENALNESUYUSOUHI                             " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(シーエナジーエルネス輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0025_CENALNESUYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(シーエナジーエルネス輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0025_CENALNESUYUSOUHI(                                                                                                                              " _
            & "        RECONO,                                                                                                                                                            " _
            & "        LOADUNLOTYPE,                                                                                                                                                      " _
            & "        STACKINGTYPE,                                                                                                                                                      " _
            & "        ORDERORGCODE,                                                                                                                                                      " _
            & "        ORDERORGNAME,                                                                                                                                                      " _
            & "        KASANAMEORDERORG,                                                                                                                                                  " _
            & "        KASANCODEORDERORG,                                                                                                                                                 " _
            & "        ORDERORG,                                                                                                                                                          " _
            & "        PRODUCT2NAME,                                                                                                                                                      " _
            & "        PRODUCT2,                                                                                                                                                          " _
            & "        PRODUCT1NAME,                                                                                                                                                      " _
            & "        PRODUCT1,                                                                                                                                                          " _
            & "        OILNAME,                                                                                                                                                           " _
            & "        OILTYPE,                                                                                                                                                           " _
            & "        TODOKECODE,                                                                                                                                                        " _
            & "        TODOKENAME,                                                                                                                                                        " _
            & "        TODOKENAMES,                                                                                                                                                       " _
            & "        TORICODE,                                                                                                                                                          " _
            & "        TORINAME,                                                                                                                                                          " _
            & "        SHUKABASHO,                                                                                                                                                        " _
            & "        SHUKANAME,                                                                                                                                                         " _
            & "        SHUKANAMES,                                                                                                                                                        " _
            & "        SHUKATORICODE,                                                                                                                                                     " _
            & "        SHUKATORINAME,                                                                                                                                                     " _
            & "        SHUKADATE,                                                                                                                                                         " _
            & "        LOADTIME,                                                                                                                                                          " _
            & "        LOADTIMEIN,                                                                                                                                                        " _
            & "        TODOKEDATE,                                                                                                                                                        " _
            & "        SHITEITIME,                                                                                                                                                        " _
            & "        SHITEITIMEIN,                                                                                                                                                      " _
            & "        ZYUTYU,                                                                                                                                                            " _
            & "        ZISSEKI,                                                                                                                                                           " _
            & "        TANNI,                                                                                                                                                             " _
            & "        TANKNUM,                                                                                                                                                           " _
            & "        TANKNUMBER,                                                                                                                                                        " _
            & "        GYOMUTANKNUM,                                                                                                                                                      " _
            & "        SYAGATA,                                                                                                                                                           " _
            & "        SYABARA,                                                                                                                                                           " _
            & "        NINUSHINAME,                                                                                                                                                       " _
            & "        CONTYPE,                                                                                                                                                           " _
            & "        TRIP,                                                                                                                                                              " _
            & "        DRP,                                                                                                                                                               " _
            & "        STAFFSLCT,                                                                                                                                                         " _
            & "        STAFFNAME,                                                                                                                                                         " _
            & "        STAFFCODE,                                                                                                                                                         " _
            & "        SUBSTAFFSLCT,                                                                                                                                                      " _
            & "        SUBSTAFFNAME,                                                                                                                                                      " _
            & "        SUBSTAFFNUM,                                                                                                                                                       " _
            & "        SHUKODATE,                                                                                                                                                         " _
            & "        KIKODATE,                                                                                                                                                          " _
            & "        TANKA,                                                                                                                                                             " _
            & "        JURYORYOKIN,                                                                                                                                                       " _
            & "        TSUKORYO,                                                                                                                                                          " _
            & "        KYUZITUTANKA,                                                                                                                                                      " _
            & "        YUSOUHI,                                                                                                                                                           " _
            & "        CALCKBN,                                                                                                                                                           " _
            & "        WORKINGDAY,                                                                                                                                                        " _
            & "        PUBLICHOLIDAYNAME,                                                                                                                                                 " _
            & "        DELFLG,                                                                                                                                                            " _
            & "        INITYMD,                                                                                                                                                           " _
            & "        INITUSER,                                                                                                                                                          " _
            & "        INITTERMID,                                                                                                                                                        " _
            & "        INITPGID,                                                                                                                                                          " _
            & "        UPDYMD,                                                                                                                                                            " _
            & "        UPDUSER,                                                                                                                                                           " _
            & "        UPDTERMID,                                                                                                                                                         " _
            & "        UPDPGID,                                                                                                                                                           " _
            & "        RECEIVEYMD)                                                                                                                                                        " _
            & "    SELECT                                                                                                                                                                 " _
            & "        ZISSEKIMAIN.RECONO            AS RECONO,                                                                                                                           " _
            & "        ZISSEKIMAIN.LOADUNLOTYPE      AS LOADUNLOTYPE,                                                                                                                     " _
            & "        ZISSEKIMAIN.STACKINGTYPE      AS STACKINGTYPE,                                                                                                                     " _
            & "        ZISSEKIMAIN.ORDERORGCODE      AS ORDERORGCODE,                                                                                                                     " _
            & "        ZISSEKIMAIN.ORDERORGNAME      AS ORDERORGNAME,                                                                                                                     " _
            & "        ZISSEKIMAIN.KASANAMEORDERORG  AS KASANAMEORDERORG,                                                                                                                 " _
            & "        ZISSEKIMAIN.KASANCODEORDERORG AS KASANCODEORDERORG,                                                                                                                " _
            & "        ZISSEKIMAIN.ORDERORG          AS ORDERORG,                                                                                                                         " _
            & "        ZISSEKIMAIN.PRODUCT2NAME      AS PRODUCT2NAME,                                                                                                                     " _
            & "        ZISSEKIMAIN.PRODUCT2          AS PRODUCT2,                                                                                                                         " _
            & "        ZISSEKIMAIN.PRODUCT1NAME      AS PRODUCT1NAME,                                                                                                                     " _
            & "        ZISSEKIMAIN.PRODUCT1          AS PRODUCT1,                                                                                                                         " _
            & "        ZISSEKIMAIN.OILNAME           AS OILNAME,                                                                                                                          " _
            & "        ZISSEKIMAIN.OILTYPE           AS OILTYPE,                                                                                                                          " _
            & "        ZISSEKIMAIN.TODOKECODE        AS TODOKECODE,                                                                                                                       " _
            & "        ZISSEKIMAIN.TODOKENAME        AS TODOKENAME,                                                                                                                       " _
            & "        ZISSEKIMAIN.TODOKENAMES       AS TODOKENAMES,                                                                                                                      " _
            & "        ZISSEKIMAIN.TORICODE          AS TORICODE,                                                                                                                         " _
            & "        ZISSEKIMAIN.TORINAME          AS TORINAME,                                                                                                                         " _
            & "        ZISSEKIMAIN.SHUKABASHO        AS SHUKABASHO,                                                                                                                       " _
            & "        ZISSEKIMAIN.SHUKANAME         AS SHUKANAME,                                                                                                                        " _
            & "        ZISSEKIMAIN.SHUKANAMES        AS SHUKANAMES,                                                                                                                       " _
            & "        ZISSEKIMAIN.SHUKATORICODE     AS SHUKATORICODE,                                                                                                                    " _
            & "        ZISSEKIMAIN.SHUKATORINAME     AS SHUKATORINAME,                                                                                                                    " _
            & "        ZISSEKIMAIN.SHUKADATE         AS SHUKADATE,                                                                                                                        " _
            & "        ZISSEKIMAIN.LOADTIME          AS LOADTIME,                                                                                                                         " _
            & "        ZISSEKIMAIN.LOADTIMEIN        AS LOADTIMEIN,                                                                                                                       " _
            & "        ZISSEKIMAIN.TODOKEDATE        AS TODOKEDATE,                                                                                                                       " _
            & "        ZISSEKIMAIN.SHITEITIME        AS SHITEITIME,                                                                                                                       " _
            & "        ZISSEKIMAIN.SHITEITIMEIN      AS SHITEITIMEIN,                                                                                                                     " _
            & "        ZISSEKIMAIN.ZYUTYU            AS ZYUTYU,                                                                                                                           " _
            & "        ZISSEKIMAIN.ZISSEKI           AS ZISSEKI,                                                                                                                          " _
            & "        ZISSEKIMAIN.TANNI             AS TANNI,                                                                                                                            " _
            & "        ZISSEKIMAIN.TANKNUM           AS TANKNUM,                                                                                                                          " _
            & "        ZISSEKIMAIN.TANKNUMBER        AS TANKNUMBER,                                                                                                                       " _
            & "        ZISSEKIMAIN.GYOMUTANKNUM      AS GYOMUTANKNUM,                                                                                                                     " _
            & "        ZISSEKIMAIN.SYAGATA           AS SYAGATA,                                                                                                                          " _
            & "        ZISSEKIMAIN.SYABARA           AS SYABARA,                                                                                                                          " _
            & "        ZISSEKIMAIN.NINUSHINAME       AS NINUSHINAME,                                                                                                                      " _
            & "        ZISSEKIMAIN.CONTYPE           AS CONTYPE,                                                                                                                          " _
            & "        ZISSEKIMAIN.TRIP              AS TRIP,                                                                                                                             " _
            & "        ZISSEKIMAIN.DRP               AS DRP,                                                                                                                              " _
            & "        ZISSEKIMAIN.STAFFSLCT         AS STAFFSLCT,                                                                                                                        " _
            & "        ZISSEKIMAIN.STAFFNAME         AS STAFFNAME,                                                                                                                        " _
            & "        ZISSEKIMAIN.STAFFCODE         AS STAFFCODE,                                                                                                                        " _
            & "        ZISSEKIMAIN.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                                                                                                                     " _
            & "        ZISSEKIMAIN.SUBSTAFFNAME      AS SUBSTAFFNAME,                                                                                                                     " _
            & "        ZISSEKIMAIN.SUBSTAFFNUM       AS SUBSTAFFNUM,                                                                                                                      " _
            & "        ZISSEKIMAIN.SHUKODATE         AS SHUKODATE,                                                                                                                        " _
            & "        ZISSEKIMAIN.KIKODATE          AS KIKODATE,                                                                                                                         " _
            & "        NULL                          AS TANKA,                                                                                                                            " _
            & "        ZISSEKIMAIN.JURYORYOKIN       AS JURYORYOKIN,                                                                                                                      " _
            & "        ZISSEKIMAIN.TSUKORYO          AS TSUKORYO,                                                                                                                         " _
            & "        ZISSEKIMAIN.KYUZITUTANKA      AS KYUZITUTANKA,                                                                                                                     " _
            & "        COALESCE(ZISSEKIMAIN.JURYORYOKIN, 0) AS YUSOUHI,                                                                                                                   " _
            & "        ZISSEKIMAIN.CALCKBN           AS CALCKBN,                                                                                                                          " _
            & "        ZISSEKIMAIN.WORKINGDAY        AS WORKINGDAY,                                                                                                                       " _
            & "        ZISSEKIMAIN.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                                                                                " _
            & "        ZISSEKIMAIN.DELFLG            AS DELFLG,                                                                                                                           " _
            & "        @INITYMD                      AS INITYMD,                                                                                                                          " _
            & "        @INITUSER                     AS INITUSER,                                                                                                                         " _
            & "        @INITTERMID                   AS INITTERMID,                                                                                                                       " _
            & "        @INITPGID                     AS INITPGID,                                                                                                                         " _
            & "        NULL                          AS UPDYMD,                                                                                                                           " _
            & "        NULL                          AS UPDUSER,                                                                                                                          " _
            & "        NULL                          AS UPDTERMID,                                                                                                                        " _
            & "        NULL                          AS UPDPGID,                                                                                                                          " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                                                                                                        " _
            & "    FROM(                                                                                                                                                                  " _
            & "         SELECT                                                                                                                                                            " _
            & "             ZISSEKI.RECONO             AS RECONO,                                                                                                                         " _
            & "             ZISSEKI.LOADUNLOTYPE       AS LOADUNLOTYPE,                                                                                                                   " _
            & "             ZISSEKI.STACKINGTYPE       AS STACKINGTYPE,                                                                                                                   " _
            & "             ZISSEKI.ORDERORGCODE       AS ORDERORGCODE,                                                                                                                   " _
            & "             ZISSEKI.ORDERORGNAME       AS ORDERORGNAME,                                                                                                                   " _
            & "             ZISSEKI.KASANAMEORDERORG   AS KASANAMEORDERORG,                                                                                                               " _
            & "             ZISSEKI.KASANCODEORDERORG  AS KASANCODEORDERORG,                                                                                                              " _
            & "             ZISSEKI.ORDERORG           AS ORDERORG,                                                                                                                       " _
            & "             ZISSEKI.PRODUCT2NAME       AS PRODUCT2NAME,                                                                                                                   " _
            & "             ZISSEKI.PRODUCT2           AS PRODUCT2,                                                                                                                       " _
            & "             ZISSEKI.PRODUCT1NAME       AS PRODUCT1NAME,                                                                                                                   " _
            & "             ZISSEKI.PRODUCT1           AS PRODUCT1,                                                                                                                       " _
            & "             ZISSEKI.OILNAME            AS OILNAME,                                                                                                                        " _
            & "             ZISSEKI.OILTYPE            AS OILTYPE,                                                                                                                        " _
            & "             ZISSEKI.TODOKECODE         AS TODOKECODE,                                                                                                                     " _
            & "             ZISSEKI.TODOKENAME         AS TODOKENAME,                                                                                                                     " _
            & "             ZISSEKI.TODOKENAMES        AS TODOKENAMES,                                                                                                                    " _
            & "             ZISSEKI.TORICODE           AS TORICODE,                                                                                                                       " _
            & "             ZISSEKI.TORINAME           AS TORINAME,                                                                                                                       " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                                                         " _
            & "             THEN (SELECT SHUKABASHO                                                                                                                                       " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                                                                                                              " _
            & "                     WHERE                                                                                                                                                 " _
            & "                         TORICODE     = ZISSEKI.TORICODE                                                                                                                   " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                                                                                                                   " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                                                               " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                                                                                                                    " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                                                                 " _
            & "                     AND DELFLG       = '0'                                                                                                                                " _
            & "                 )                                                                                                                                                         " _
            & "             ELSE ZISSEKI.SHUKABASHO                                                                                                                                       " _
            & "             END AS SHUKABASHO,                                                                                                                                            " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                                                         " _
            & "             THEN (SELECT SHUKANAME                                                                                                                                        " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                                                                                                              " _
            & "                     WHERE                                                                                                                                                 " _
            & "                         TORICODE     = ZISSEKI.TORICODE                                                                                                                   " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                                                                                                                   " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                                                               " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                                                                                                                    " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                                                                 " _
            & "                     AND DELFLG       = '0'                                                                                                                                " _
            & "                 )                                                                                                                                                         " _
            & "             ELSE ZISSEKI.SHUKANAME                                                                                                                                        " _
            & "             END AS SHUKANAME,                                                                                                                                             " _
            & "             ZISSEKI.SHUKANAMES         AS SHUKANAMES,                                                                                                                     " _
            & "             ZISSEKI.SHUKATORICODE      AS SHUKATORICODE,                                                                                                                  " _
            & "             ZISSEKI.SHUKATORINAME      AS SHUKATORINAME,                                                                                                                  " _
            & "             ZISSEKI.SHUKADATE          AS SHUKADATE,                                                                                                                      " _
            & "             ZISSEKI.LOADTIME           AS LOADTIME,                                                                                                                       " _
            & "             ZISSEKI.LOADTIMEIN         AS LOADTIMEIN,                                                                                                                     " _
            & "             ZISSEKI.TODOKEDATE         AS TODOKEDATE,                                                                                                                     " _
            & "             ZISSEKI.SHITEITIME         AS SHITEITIME,                                                                                                                     " _
            & "             ZISSEKI.SHITEITIMEIN       AS SHITEITIMEIN,                                                                                                                   " _
            & "             ZISSEKI.ZYUTYU             AS ZYUTYU,                                                                                                                         " _
            & "             ZISSEKI.ZISSEKI            AS ZISSEKI,                                                                                                                        " _
            & "             ZISSEKI.TANNI              AS TANNI,                                                                                                                          " _
            & "             ZISSEKI.TANKNUM            AS TANKNUM,                                                                                                                        " _
            & "             ZISSEKI.TANKNUMBER         AS TANKNUMBER,                                                                                                                     " _
            & "             ZISSEKI.GYOMUTANKNUM       AS GYOMUTANKNUM,                                                                                                                   " _
            & "             ZISSEKI.SYAGATA            AS SYAGATA,                                                                                                                        " _
            & "             ZISSEKI.SYABARA            AS SYABARA,                                                                                                                        " _
            & "             ZISSEKI.NINUSHINAME        AS NINUSHINAME,                                                                                                                    " _
            & "             ZISSEKI.CONTYPE            AS CONTYPE,                                                                                                                        " _
            & "             ZISSEKI.TRIP               AS TRIP,                                                                                                                           " _
            & "             ZISSEKI.DRP                AS DRP,                                                                                                                            " _
            & "             ZISSEKI.STAFFSLCT          AS STAFFSLCT,                                                                                                                      " _
            & "             ZISSEKI.STAFFNAME          AS STAFFNAME,                                                                                                                      " _
            & "             ZISSEKI.STAFFCODE          AS STAFFCODE,                                                                                                                      " _
            & "             ZISSEKI.SUBSTAFFSLCT       AS SUBSTAFFSLCT,                                                                                                                   " _
            & "             ZISSEKI.SUBSTAFFNAME       AS SUBSTAFFNAME,                                                                                                                   " _
            & "             ZISSEKI.SUBSTAFFNUM        AS SUBSTAFFNUM,                                                                                                                    " _
            & "             ZISSEKI.SHUKODATE          AS SHUKODATE,                                                                                                                      " _
            & "             ZISSEKI.KIKODATE           AS KIKODATE,                                                                                                                       " _
            & "             CASE TANKA.CALCKBN                                                                                                                                            " _
            & "                  WHEN 'トン' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                                                                                 " _
            & "                  WHEN '回'   THEN COALESCE(TANKA.TANKA, 0)                                                                                                                " _
            & "                  WHEN '距離' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0)                                                                                 " _
            & "                  WHEN '定数' THEN COALESCE(TANKA.TANKA, 0)                                                                                                                " _
            & "                              ELSE COALESCE(TANKA.TANKA, 0)                                                                                                                " _
            & "             END                        AS JURYORYOKIN,                                                                                                                    " _
            & "             CASE                                                                                                                                                          " _
            & "                 WHEN ZISSEKI.TORICODE = '0110600000' THEN COALESCE(TANKA.TOLLFEE, 0)                                                                                      " _
            & "                 WHEN ZISSEKI.TORICODE = '0238900000' THEN 0                                                                                                               " _
            & "             END                        AS TSUKORYO,                                                                                                                       " _
            & "             TANKA.CALCKBN              AS CALCKBN,                                                                                                                        " _
            & "             CASE WHEN ZISSEKI.TRIP = '1'                                                                                                                                  " _
            & "                  THEN HOLIDAYRATE.TANKA                                                                                                                                   " _
            & "                  ELSE NULL                                                                                                                                                " _
            & "             END                        AS KYUZITUTANKA,                                                                                                                   " _
            & "             CALENDAR.WORKINGDAY        AS WORKINGDAY,                                                                                                                     " _
            & "             CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                                                                              " _
            & "             ZISSEKI.DELFLG             AS DELFLG                                                                                                                          " _
            & "         FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                                                                                  " _
            & "          LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                                                                             " _
            & "              ON @TORICODE = TANKA.TORICODE                                                                                                                                " _
            & "              And ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                                                                                     " _
            & "              And ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                                                                           " _
            & "              And TANKA.AVOCADOSHUKABASHO = CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                          " _
            & "                                                 THEN (SELECT SHUKABASHO                                                                                                   " _
            & "                                                         FROM LNG.LNT0001_ZISSEKI                                                                                          " _
            & "                                                        WHERE                                                                                                              " _
            & "                                                              TORICODE     = ZISSEKI.TORICODE                                                                              " _
            & "                                                          AND ORDERORG     = ZISSEKI.ORDERORG                                                                              " _
            & "                                                          AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                          " _
            & "                                                          AND TRIP         = ZISSEKI.TRIP -1                                                                               " _
            & "                                                          AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                            " _
            & "                                                          AND DELFLG       = '0'                                                                                           " _
            & "                                                      )                                                                                                                    " _
            & "                                                 ELSE ZISSEKI.SHUKABASHO                                                                                                   " _
            & "                                            END                                                                                                                            " _
            & "              AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                                                                             " _
            & "              AND ZISSEKI.GYOMUTANKNUM = TANKA.SHABAN                                                                                                                      " _
            & "              AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                                                                                       " _
            & "              AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                                                                                       " _
            & "              AND TANKA.BRANCHCODE = ZISSEKI.BRANCHCODE                                                                                                                    " _
            & "              AND TANKA.DELFLG = @DELFLG                                                                                                                                   " _
            & "          LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                                                                          " _
            & "             ON ZISSEKI.TORICODE = CALENDAR.TORICODE                                                                                                                       " _
            & "             AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                                                                         " _
            & "             AND CALENDAR.DELFLG = @DELFLG                                                                                                                                 " _
            & "         LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                                                                     " _
            & "             ON  HOLIDAYRATE.TORICODE = @TORICNV                                                                                                                           " _
            & "             AND CASE HOLIDAYRATE.ORDERORGCATEGORY                                                                                                                         " _
            & "                     WHEN '1' THEN HOLIDAYRATE.ORDERORGCODE = ZISSEKI.ORDERORGCODE                                                                                         " _
            & "                  WHEN '2' THEN HOLIDAYRATE.ORDERORGCODE <> ZISSEKI.ORDERORGCODE                                                                                           " _
            & "                     ELSE 1 = 1                                                                                                                                            " _
            & "                 END                                                                                                                                                       " _
            & "             AND CASE HOLIDAYRATE.SHUKABASHOCATEGORY                                                                                                                       " _
            & "                     WHEN '1' THEN HOLIDAYRATE.SHUKABASHO = ZISSEKI.SHUKABASHO                                                                                             " _
            & "                     WHEN '2' THEN HOLIDAYRATE.SHUKABASHO <> ZISSEKI.SHUKABASHO                                                                                            " _
            & "                     ELSE 1 = 1                                                                                                                                            " _
            & "                  END                                                                                                                                                      " _
            & "             AND CASE HOLIDAYRATE.TODOKECATEGORY                                                                                                                           " _
            & "                     WHEN '1' THEN HOLIDAYRATE.TODOKECODE = ZISSEKI.TODOKECODE                                                                                             " _
            & "                     WHEN '2' THEN HOLIDAYRATE.TODOKECODE <> ZISSEKI.TODOKECODE                                                                                            " _
            & "                     ELSE 1 = 1                                                                                                                                            " _
            & "                 END                                                                                                                                                       " _
            & "             AND CASE WHEN HOLIDAYRATE.GYOMUTANKNUMFROM = '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                                                          " _
            & "                             THEN ZISSEKI.GYOMUTANKNUM <= HOLIDAYRATE.GYOMUTANKNUMTO                                                                                       " _
            & "                     WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO = ''                                                                           " _
            & "                             THEN ZISSEKI.GYOMUTANKNUM >= HOLIDAYRATE.GYOMUTANKNUMFROM                                                                                     " _
            & "                     WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                                                          " _
            & "                             THEN ZISSEKI.GYOMUTANKNUM BETWEEN HOLIDAYRATE.GYOMUTANKNUMFROM AND HOLIDAYRATE.GYOMUTANKNUMTO                                                 " _
            & "                     ELSE 1 = 1                                                                                                                                            " _
            & "                 END                                                                                                                                                       " _
            & "             AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                                                           " _
            & "             AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                                                              " _
            & "         WHERE                                                                                                                                                             " _
            & "             ZISSEKI.TORICODE = @TORICODE                                                                                                                                  " _
            & "             AND ZISSEKI.ZISSEKI <> 0                                                                                                                                      " _
            & "             AND ZISSEKI.STACKINGTYPE <> '積置'                                                                                                                            " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                                                                           " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                                                                             " _
            & "             AND ZISSEKI.DELFLG = @DELFLG                                                                                                                                  " _
            & "    ) ZISSEKIMAIN                                                                                                                                                          " _
            & " ON DUPLICATE KEY UPDATE                                                                                                                                                   " _
            & "         RECONO                    = VALUES(RECONO),                                                                                                                       " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                                                                                 " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                                                                                 " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                                                                                 " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                                                                                 " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                                                                                             " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                                                                                            " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                                                                                     " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                                                                                 " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                                                                                     " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                                                                                 " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                                                                                     " _
            & "         OILNAME                   = VALUES(OILNAME),                                                                                                                      " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                                                                                      " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                                                                                   " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                                                                                   " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                                                                                  " _
            & "         TORICODE                  = VALUES(TORICODE),                                                                                                                     " _
            & "         TORINAME                  = VALUES(TORINAME),                                                                                                                     " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                                                                                   " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                                                                                    " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                                                                                   " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                                                                                " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                                                                                " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                                                                                    " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                                                                                     " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                                                                                   " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                                                                                   " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                                                                                   " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                                                                                 " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                                                                                       " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                                                                                      " _
            & "         TANNI                     = VALUES(TANNI),                                                                                                                        " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                                                                                      " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                                                                                   " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                                                                                 " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                                                                                      " _
            & "         SYABARA                   = VALUES(SYABARA),                                                                                                                      " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                                                                                  " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                                                                                      " _
            & "         TRIP                      = VALUES(TRIP),                                                                                                                         " _
            & "         DRP                       = VALUES(DRP),                                                                                                                          " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                                                                                    " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                                                                                    " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                                                                                    " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                                                                                 " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                                                                                 " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                                                                                  " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                                                                                    " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                                                                                     " _
            & "         TANKA                     = VALUES(TANKA),                                                                                                                        " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                                                                                  " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                                                                                     " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                                                                                 " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                                                                                      " _
            & "         CALCKBN                   = VALUES(CALCKBN),                                                                                                                      " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                                                                                   " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                                                                                            " _
            & "         DELFLG                    = @DELFLG,                                                                                                                              " _
            & "         UPDYMD                    = @UPDYMD,                                                                                                                              " _
            & "         UPDUSER                   = @UPDUSER,                                                                                                                             " _
            & "         UPDTERMID                 = @UPDTERMID,                                                                                                                           " _
            & "         UPDPGID                   = @UPDPGID,                                                                                                                             " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                                                                                          "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(シーエナジーエルネス輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim TORICNV As MySqlParameter = SQLcmd.Parameters.Add("@TORICNV", MySqlDbType.VarChar)                  '変換取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    TORICNV.Value = iTori                                                   '変換取引先コード
                    If iTori = CONST_TORICODE_0238900000 Then                               'エルネスの場合、シーエナジーコードで休日単価マスタを取得する
                        TORICNV.Value = CONST_TORICODE_0110600000
                    End If
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0025_CENALNESUYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 石油資源開発(北海道)輸送費テーブル更新
    ''' </summary>
    Private Sub SEKIYUHOKKAIDO_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(石油資源開発(北海道)輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0023_SEKIYUHOKKAIDOYUSOUHI                        " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(石油資源開発(北海道)輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0023_SEKIYUHOKKAIDOYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(石油資源開発(北海道)輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0023_SEKIYUHOKKAIDOYUSOUHI(                                 " _
            & "        RECONO,                                                                    " _
            & "        LOADUNLOTYPE,                                                              " _
            & "        STACKINGTYPE,                                                              " _
            & "        ORDERORGCODE,                                                              " _
            & "        ORDERORGNAME,                                                              " _
            & "        KASANAMEORDERORG,                                                          " _
            & "        KASANCODEORDERORG,                                                         " _
            & "        ORDERORG,                                                                  " _
            & "        PRODUCT2NAME,                                                              " _
            & "        PRODUCT2,                                                                  " _
            & "        PRODUCT1NAME,                                                              " _
            & "        PRODUCT1,                                                                  " _
            & "        OILNAME,                                                                   " _
            & "        OILTYPE,                                                                   " _
            & "        TODOKECODE,                                                                " _
            & "        TODOKENAME,                                                                " _
            & "        TODOKENAMES,                                                               " _
            & "        TORICODE,                                                                  " _
            & "        TORINAME,                                                                  " _
            & "        SHUKABASHO,                                                                " _
            & "        SHUKANAME,                                                                 " _
            & "        SHUKANAMES,                                                                " _
            & "        SHUKATORICODE,                                                             " _
            & "        SHUKATORINAME,                                                             " _
            & "        SHUKADATE,                                                                 " _
            & "        LOADTIME,                                                                  " _
            & "        LOADTIMEIN,                                                                " _
            & "        TODOKEDATE,                                                                " _
            & "        SHITEITIME,                                                                " _
            & "        SHITEITIMEIN,                                                              " _
            & "        ZYUTYU,                                                                    " _
            & "        ZISSEKI,                                                                   " _
            & "        TANNI,                                                                     " _
            & "        TANKNUM,                                                                   " _
            & "        TANKNUMBER,                                                                " _
            & "        GYOMUTANKNUM,                                                              " _
            & "        SYAGATA,                                                                   " _
            & "        SYABARA,                                                                   " _
            & "        NINUSHINAME,                                                               " _
            & "        CONTYPE,                                                                   " _
            & "        TRIP,                                                                      " _
            & "        DRP,                                                                       " _
            & "        STAFFSLCT,                                                                 " _
            & "        STAFFNAME,                                                                 " _
            & "        STAFFCODE,                                                                 " _
            & "        SUBSTAFFSLCT,                                                              " _
            & "        SUBSTAFFNAME,                                                              " _
            & "        SUBSTAFFNUM,                                                               " _
            & "        SHUKODATE,                                                                 " _
            & "        KIKODATE,                                                                  " _
            & "        TANKA,                                                                     " _
            & "        JURYORYOKIN,                                                               " _
            & "        TSUKORYO,                                                                  " _
            & "        KYUZITUTANKA,                                                              " _
            & "        YUSOUHI,                                                                   " _
            & "        CALCKBN,                                                                   " _
            & "        WORKINGDAY,                                                                " _
            & "        PUBLICHOLIDAYNAME,                                                         " _
            & "        DELFLG,                                                                    " _
            & "        INITYMD,                                                                   " _
            & "        INITUSER,                                                                  " _
            & "        INITTERMID,                                                                " _
            & "        INITPGID,                                                                  " _
            & "        UPDYMD,                                                                    " _
            & "        UPDUSER,                                                                   " _
            & "        UPDTERMID,                                                                 " _
            & "        UPDPGID,                                                                   " _
            & "        RECEIVEYMD)                                                                " _
            & "    SELECT                                                                         " _
            & "        ZISSEKIMAIN.RECONO            AS RECONO,                                   " _
            & "        ZISSEKIMAIN.LOADUNLOTYPE      AS LOADUNLOTYPE,                             " _
            & "        ZISSEKIMAIN.STACKINGTYPE      AS STACKINGTYPE,                             " _
            & "        ZISSEKIMAIN.ORDERORGCODE      AS ORDERORGCODE,                             " _
            & "        ZISSEKIMAIN.ORDERORGNAME      AS ORDERORGNAME,                             " _
            & "        ZISSEKIMAIN.KASANAMEORDERORG  AS KASANAMEORDERORG,                         " _
            & "        ZISSEKIMAIN.KASANCODEORDERORG AS KASANCODEORDERORG,                        " _
            & "        ZISSEKIMAIN.ORDERORG          AS ORDERORG,                                 " _
            & "        ZISSEKIMAIN.PRODUCT2NAME      AS PRODUCT2NAME,                             " _
            & "        ZISSEKIMAIN.PRODUCT2          AS PRODUCT2,                                 " _
            & "        ZISSEKIMAIN.PRODUCT1NAME      AS PRODUCT1NAME,                             " _
            & "        ZISSEKIMAIN.PRODUCT1          AS PRODUCT1,                                 " _
            & "        ZISSEKIMAIN.OILNAME           AS OILNAME,                                  " _
            & "        ZISSEKIMAIN.OILTYPE           AS OILTYPE,                                  " _
            & "        ZISSEKIMAIN.TODOKECODE        AS TODOKECODE,                               " _
            & "        ZISSEKIMAIN.TODOKENAME        AS TODOKENAME,                               " _
            & "        ZISSEKIMAIN.TODOKENAMES       AS TODOKENAMES,                              " _
            & "        ZISSEKIMAIN.TORICODE          AS TORICODE,                                 " _
            & "        ZISSEKIMAIN.TORINAME          AS TORINAME,                                 " _
            & "        ZISSEKIMAIN.SHUKABASHO        AS SHUKABASHO,                               " _
            & "        ZISSEKIMAIN.SHUKANAME         AS SHUKANAME,                                " _
            & "        ZISSEKIMAIN.SHUKANAMES        AS SHUKANAMES,                               " _
            & "        ZISSEKIMAIN.SHUKATORICODE     AS SHUKATORICODE,                            " _
            & "        ZISSEKIMAIN.SHUKATORINAME     AS SHUKATORINAME,                            " _
            & "        ZISSEKIMAIN.SHUKADATE         AS SHUKADATE,                                " _
            & "        ZISSEKIMAIN.LOADTIME          AS LOADTIME,                                 " _
            & "        ZISSEKIMAIN.LOADTIMEIN        AS LOADTIMEIN,                               " _
            & "        ZISSEKIMAIN.TODOKEDATE        AS TODOKEDATE,                               " _
            & "        ZISSEKIMAIN.SHITEITIME        AS SHITEITIME,                               " _
            & "        ZISSEKIMAIN.SHITEITIMEIN      AS SHITEITIMEIN,                             " _
            & "        ZISSEKIMAIN.ZYUTYU            AS ZYUTYU,                                   " _
            & "        ZISSEKIMAIN.ZISSEKI           AS ZISSEKI,                                  " _
            & "        ZISSEKIMAIN.TANNI             AS TANNI,                                    " _
            & "        ZISSEKIMAIN.TANKNUM           AS TANKNUM,                                  " _
            & "        ZISSEKIMAIN.TANKNUMBER        AS TANKNUMBER,                               " _
            & "        ZISSEKIMAIN.GYOMUTANKNUM      AS GYOMUTANKNUM,                             " _
            & "        ZISSEKIMAIN.SYAGATA           AS SYAGATA,                                  " _
            & "        ZISSEKIMAIN.SYABARA           AS SYABARA,                                  " _
            & "        ZISSEKIMAIN.NINUSHINAME       AS NINUSHINAME,                              " _
            & "        ZISSEKIMAIN.CONTYPE           AS CONTYPE,                                  " _
            & "        ZISSEKIMAIN.TRIP              AS TRIP,                                     " _
            & "        ZISSEKIMAIN.DRP               AS DRP,                                      " _
            & "        ZISSEKIMAIN.STAFFSLCT         AS STAFFSLCT,                                " _
            & "        ZISSEKIMAIN.STAFFNAME         AS STAFFNAME,                                " _
            & "        ZISSEKIMAIN.STAFFCODE         AS STAFFCODE,                                " _
            & "        ZISSEKIMAIN.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                             " _
            & "        ZISSEKIMAIN.SUBSTAFFNAME      AS SUBSTAFFNAME,                             " _
            & "        ZISSEKIMAIN.SUBSTAFFNUM       AS SUBSTAFFNUM,                              " _
            & "        ZISSEKIMAIN.SHUKODATE         AS SHUKODATE,                                " _
            & "        ZISSEKIMAIN.KIKODATE          AS KIKODATE,                                 " _
            & "        ZISSEKIMAIN.TANKA             AS TANKA,                                    " _
            & "        NULL                          AS JURYORYOKIN,                              " _
            & "        NULL                          AS TSUKORYO,                                 " _
            & "        NULL                          AS KYUZITUTANKA,                             " _
            & "        ZISSEKIMAIN.YUSOUHI           AS YUSOUHI,                                  " _
            & "        ZISSEKIMAIN.CALCKBN           AS CALCKBN,                                  " _
            & "        ZISSEKIMAIN.WORKINGDAY        AS WORKINGDAY,                               " _
            & "        ZISSEKIMAIN.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                        " _
            & "        ZISSEKIMAIN.DELFLG            AS DELFLG,                                   " _
            & "        @INITYMD                      AS INITYMD,                                  " _
            & "        @INITUSER                     AS INITUSER,                                 " _
            & "        @INITTERMID                   AS INITTERMID,                               " _
            & "        @INITPGID                     AS INITPGID,                                 " _
            & "        NULL                          AS UPDYMD,                                   " _
            & "        NULL                          AS UPDUSER,                                  " _
            & "        NULL                          AS UPDTERMID,                                " _
            & "        NULL                          AS UPDPGID,                                  " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                " _
            & "    FROM(                                                                          " _
            & "         SELECT                                                                    " _
            & "             ZISSEKI.RECONO            AS RECONO,                                  " _
            & "             ZISSEKI.LOADUNLOTYPE      AS LOADUNLOTYPE,                            " _
            & "             ZISSEKI.STACKINGTYPE      AS STACKINGTYPE,                            " _
            & "             ZISSEKI.ORDERORGCODE      AS ORDERORGCODE,                            " _
            & "             ZISSEKI.ORDERORGNAME      AS ORDERORGNAME,                            " _
            & "             ZISSEKI.KASANAMEORDERORG  AS KASANAMEORDERORG,                        " _
            & "             ZISSEKI.KASANCODEORDERORG AS KASANCODEORDERORG,                       " _
            & "             ZISSEKI.ORDERORG          AS ORDERORG,                                " _
            & "             ZISSEKI.PRODUCT2NAME      AS PRODUCT2NAME,                            " _
            & "             ZISSEKI.PRODUCT2          AS PRODUCT2,                                " _
            & "             ZISSEKI.PRODUCT1NAME      AS PRODUCT1NAME,                            " _
            & "             ZISSEKI.PRODUCT1          AS PRODUCT1,                                " _
            & "             ZISSEKI.OILNAME           AS OILNAME,                                 " _
            & "             ZISSEKI.OILTYPE           AS OILTYPE,                                 " _
            & "             ZISSEKI.TODOKECODE        AS TODOKECODE,                              " _
            & "             ZISSEKI.TODOKENAME        AS TODOKENAME,                              " _
            & "             ZISSEKI.TODOKENAMES       AS TODOKENAMES,                             " _
            & "             ZISSEKI.TORICODE          AS TORICODE,                                " _
            & "             ZISSEKI.TORINAME          AS TORINAME,                                " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                 " _
            & "             THEN (SELECT SHUKABASHO                                               " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                      " _
            & "                     WHERE                                                         " _
            & "                         TORICODE     = ZISSEKI.TORICODE                           " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                           " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                       " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                            " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                         " _
            & "                     AND DELFLG       = '0'                                        " _
            & "                 )                                                                 " _
            & "             ELSE ZISSEKI.SHUKABASHO                                               " _
            & "             END AS SHUKABASHO,                                                    " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                 " _
            & "             THEN (SELECT SHUKANAME                                                " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                      " _
            & "                     WHERE                                                         " _
            & "                         TORICODE     = ZISSEKI.TORICODE                           " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                           " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                       " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                            " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                         " _
            & "                     AND DELFLG       = '0'                                        " _
            & "                 )                                                                 " _
            & "             ELSE ZISSEKI.SHUKANAME                                                " _
            & "             END AS SHUKANAME,                                                     " _
            & "             ZISSEKI.SHUKANAMES        AS SHUKANAMES,                              " _
            & "             ZISSEKI.SHUKATORICODE     AS SHUKATORICODE,                           " _
            & "             ZISSEKI.SHUKATORINAME     AS SHUKATORINAME,                           " _
            & "             ZISSEKI.SHUKADATE         AS SHUKADATE,                               " _
            & "             ZISSEKI.LOADTIME          AS LOADTIME,                                " _
            & "             ZISSEKI.LOADTIMEIN        AS LOADTIMEIN,                              " _
            & "             ZISSEKI.TODOKEDATE        AS TODOKEDATE,                              " _
            & "             ZISSEKI.SHITEITIME        AS SHITEITIME,                              " _
            & "             ZISSEKI.SHITEITIMEIN      AS SHITEITIMEIN,                            " _
            & "             ZISSEKI.ZYUTYU            AS ZYUTYU,                                  " _
            & "             ZISSEKI.ZISSEKI           AS ZISSEKI,                                 " _
            & "             ZISSEKI.TANNI             AS TANNI,                                   " _
            & "             ZISSEKI.TANKNUM           AS TANKNUM,                                 " _
            & "             ZISSEKI.TANKNUMBER        AS TANKNUMBER,                              " _
            & "             ZISSEKI.GYOMUTANKNUM      AS GYOMUTANKNUM,                            " _
            & "             ZISSEKI.SYAGATA           AS SYAGATA,                                 " _
            & "             ZISSEKI.SYABARA           AS SYABARA,                                 " _
            & "             ZISSEKI.NINUSHINAME       AS NINUSHINAME,                             " _
            & "             ZISSEKI.CONTYPE           AS CONTYPE,                                 " _
            & "             ZISSEKI.TRIP              AS TRIP,                                    " _
            & "             ZISSEKI.DRP               AS DRP,                                     " _
            & "             ZISSEKI.STAFFSLCT         AS STAFFSLCT,                               " _
            & "             ZISSEKI.STAFFNAME         AS STAFFNAME,                               " _
            & "             ZISSEKI.STAFFCODE         AS STAFFCODE,                               " _
            & "             ZISSEKI.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                            " _
            & "             ZISSEKI.SUBSTAFFNAME      AS SUBSTAFFNAME,                            " _
            & "             ZISSEKI.SUBSTAFFNUM       AS SUBSTAFFNUM,                             " _
            & "             ZISSEKI.SHUKODATE         AS SHUKODATE,                               " _
            & "             ZISSEKI.KIKODATE          AS KIKODATE,                                " _
            & "             HOLIDAYRATE.TANKA         AS KYUZITUTANKA,                            " _
            & "             TANKA.TANKA               AS TANKA,                                   " _
            & "             CASE TANKA.CALCKBN                                                         " _
            & "               WHEN 'トン' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0) " _
            & "               WHEN '回'   THEN COALESCE(TANKA.TANKA, 0)                                " _
            & "               WHEN '距離' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0) " _
            & "               WHEN '定数' THEN COALESCE(TANKA.TANKA, 0)                                " _
            & "                           ELSE COALESCE(TANKA.TANKA, 0)                                " _
            & "             END                       AS YUSOUHI,                                      " _
            & "             TANKA.CALCKBN             AS CALCKBN,                                 " _
            & "             CALENDAR.WORKINGDAY       AS WORKINGDAY,                              " _
            & "             CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                      " _
            & "             ZISSEKI.DELFLG            AS DELFLG                                   " _
            & "         FROM LNG.LNT0001_ZISSEKI ZISSEKI                                          " _
            & "         LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                      " _
            & "             ON @TORICODE = TANKA.TORICODE                                         " _
            & "             AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                              " _
            & "             AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                    " _
            & "             AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                      " _
            & "             AND CASE WHEN TANKA.SHABAN = ''                                       " _
            & "                      THEN 1 = 1                                                   " _
            & "                      ELSE ZISSEKI.GYOMUTANKNUM = TANKA.SHABAN                     " _
            & "                 END                                                               " _
            & "             AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                " _
            & "             AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                " _
            & "             AND TANKA.DELFLG = @DELFLG                                            " _
            & "             AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                             " _
            & "         LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                   " _
            & "             ON @TORICODE = CALENDAR.TORICODE                                      " _
            & "             AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                 " _
            & "             AND CALENDAR.DELFLG = @DELFLG                                         " _
            & "         LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                               " _
            & "             ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                              " _
            & "             AND CASE HOLIDAYRATE.ORDERORGCATEGORY                                                                                   " _
            & "                     WHEN '1' THEN HOLIDAYRATE.ORDERORGCODE = ZISSEKI.ORDERORGCODE                                                   " _
            & "                     WHEN '2' THEN HOLIDAYRATE.ORDERORGCODE <> ZISSEKI.ORDERORGCODE                                                  " _
            & "                     ELSE 1 = 1                                                                                                      " _
            & "                 END                                                                                                                 " _
            & "             AND CASE HOLIDAYRATE.SHUKABASHOCATEGORY                                                                                 " _
            & "                     WHEN '1' THEN HOLIDAYRATE.SHUKABASHO = ZISSEKI.SHUKABASHO                                                       " _
            & "                     WHEN '2' THEN HOLIDAYRATE.SHUKABASHO <> ZISSEKI.SHUKABASHO                                                      " _
            & "                     ELSE 1 = 1                                                                                                      " _
            & "                 END                                                                                                                 " _
            & "             AND CASE HOLIDAYRATE.TODOKECATEGORY                                                                                     " _
            & "                     WHEN '1' THEN HOLIDAYRATE.TODOKECODE = ZISSEKI.TODOKECODE                                                       " _
            & "                     WHEN '2' THEN HOLIDAYRATE.TODOKECODE <> ZISSEKI.TODOKECODE                                                      " _
            & "                     ELSE 1 = 1                                                                                                      " _
            & "                 END                                                                                                                 " _
            & "             AND CASE WHEN HOLIDAYRATE.GYOMUTANKNUMFROM = '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                    " _
            & "                             THEN ZISSEKI.GYOMUTANKNUM <= HOLIDAYRATE.GYOMUTANKNUMTO                                                 " _
            & "                     WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO = ''                                     " _
            & "                             THEN ZISSEKI.GYOMUTANKNUM >= HOLIDAYRATE.GYOMUTANKNUMFROM                                               " _
            & "                     WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                    " _
            & "                             THEN ZISSEKI.GYOMUTANKNUM BETWEEN HOLIDAYRATE.GYOMUTANKNUMFROM AND HOLIDAYRATE.GYOMUTANKNUMTO           " _
            & "                     ELSE 1 = 1                                                                                                      " _
            & "                 END                                                                                                                 " _
            & "             AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                     " _
            & "             AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                        " _
            & "         WHERE                                                                     " _
            & "             ZISSEKI.TORICODE = @TORICODE                                          " _
            & "             AND ZISSEKI.ZISSEKI <> 0                                              " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                   " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                     " _
            & "             AND ZISSEKI.STACKINGTYPE <> '積置'                                    " _
            & "             AND ZISSEKI.DELFLG = @DELFLG                                          " _
            & "             AND ZISSEKI.ORDERORGCODE = '020104'                                   " _
            & "         ) ZISSEKIMAIN                                                             " _
            & " ON DUPLICATE KEY UPDATE                                                           " _
            & "         RECONO                    = VALUES(RECONO),                               " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                         " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                         " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                         " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                         " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                     " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                    " _
            & "         ORDERORG                  = VALUES(ORDERORG),                             " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                         " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                             " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                         " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                             " _
            & "         OILNAME                   = VALUES(OILNAME),                              " _
            & "         OILTYPE                   = VALUES(OILTYPE),                              " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                           " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                           " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                          " _
            & "         TORICODE                  = VALUES(TORICODE),                             " _
            & "         TORINAME                  = VALUES(TORINAME),                             " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                           " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                            " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                           " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                        " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                        " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                            " _
            & "         LOADTIME                  = VALUES(LOADTIME),                             " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                           " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                           " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                           " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                         " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                               " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                              " _
            & "         TANNI                     = VALUES(TANNI),                                " _
            & "         TANKNUM                   = VALUES(TANKNUM),                              " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                           " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                         " _
            & "         SYAGATA                   = VALUES(SYAGATA),                              " _
            & "         SYABARA                   = VALUES(SYABARA),                              " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                          " _
            & "         CONTYPE                   = VALUES(CONTYPE),                              " _
            & "         TRIP                      = VALUES(TRIP),                                 " _
            & "         DRP                       = VALUES(DRP),                                  " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                            " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                            " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                            " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                         " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                         " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                          " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                            " _
            & "         KIKODATE                  = VALUES(KIKODATE),                             " _
            & "         TANKA                     = VALUES(TANKA),                                " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                          " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                             " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                         " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                              " _
            & "         CALCKBN                   = VALUES(CALCKBN),                              " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                           " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                    " _
            & "         DELFLG                    = @DELFLG,                                      " _
            & "         UPDYMD                    = @UPDYMD,                                      " _
            & "         UPDUSER                   = @UPDUSER,                                     " _
            & "         UPDTERMID                 = @UPDTERMID,                                   " _
            & "         UPDPGID                   = @UPDPGID,                                     " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                  "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(石油資源開発(北海道)輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0023_SEKIYUHOKKAIDOYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 石油資源開発(本州分)輸送費テーブル更新
    ''' </summary>
    Private Sub SEKIYUHONSYU_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(石油資源開発(本州分)輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0021_SEKIYUHONSYUYUSOUHI                          " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(石油資源開発(本州分)輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0021_SEKIYUHONSYUYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(石油資源開発(本州分)輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0021_SEKIYUHONSYUYUSOUHI(                                   " _
            & "        RECONO,                                                                    " _
            & "        LOADUNLOTYPE,                                                              " _
            & "        STACKINGTYPE,                                                              " _
            & "        ORDERORGCODE,                                                              " _
            & "        ORDERORGNAME,                                                              " _
            & "        KASANAMEORDERORG,                                                          " _
            & "        KASANCODEORDERORG,                                                         " _
            & "        ORDERORG,                                                                  " _
            & "        PRODUCT2NAME,                                                              " _
            & "        PRODUCT2,                                                                  " _
            & "        PRODUCT1NAME,                                                              " _
            & "        PRODUCT1,                                                                  " _
            & "        OILNAME,                                                                   " _
            & "        OILTYPE,                                                                   " _
            & "        TODOKECODE,                                                                " _
            & "        TODOKENAME,                                                                " _
            & "        TODOKENAMES,                                                               " _
            & "        TORICODE,                                                                  " _
            & "        TORINAME,                                                                  " _
            & "        SHUKABASHO,                                                                " _
            & "        SHUKANAME,                                                                 " _
            & "        SHUKANAMES,                                                                " _
            & "        SHUKATORICODE,                                                             " _
            & "        SHUKATORINAME,                                                             " _
            & "        SHUKADATE,                                                                 " _
            & "        LOADTIME,                                                                  " _
            & "        LOADTIMEIN,                                                                " _
            & "        TODOKEDATE,                                                                " _
            & "        SHITEITIME,                                                                " _
            & "        SHITEITIMEIN,                                                              " _
            & "        ZYUTYU,                                                                    " _
            & "        ZISSEKI,                                                                   " _
            & "        TANNI,                                                                     " _
            & "        TANKNUM,                                                                   " _
            & "        TANKNUMBER,                                                                " _
            & "        GYOMUTANKNUM,                                                              " _
            & "        SYAGATA,                                                                   " _
            & "        SYABARA,                                                                   " _
            & "        NINUSHINAME,                                                               " _
            & "        CONTYPE,                                                                   " _
            & "        TRIP,                                                                      " _
            & "        DRP,                                                                       " _
            & "        STAFFSLCT,                                                                 " _
            & "        STAFFNAME,                                                                 " _
            & "        STAFFCODE,                                                                 " _
            & "        SUBSTAFFSLCT,                                                              " _
            & "        SUBSTAFFNAME,                                                              " _
            & "        SUBSTAFFNUM,                                                               " _
            & "        SHUKODATE,                                                                 " _
            & "        KIKODATE,                                                                  " _
            & "        TANKA,                                                                     " _
            & "        JURYORYOKIN,                                                               " _
            & "        TSUKORYO,                                                                  " _
            & "        KYUZITUTANKA,                                                              " _
            & "        YUSOUHI,                                                                   " _
            & "        CALCKBN,                                                                   " _
            & "        WORKINGDAY,                                                                " _
            & "        PUBLICHOLIDAYNAME,                                                         " _
            & "        DELFLG,                                                                    " _
            & "        INITYMD,                                                                   " _
            & "        INITUSER,                                                                  " _
            & "        INITTERMID,                                                                " _
            & "        INITPGID,                                                                  " _
            & "        UPDYMD,                                                                    " _
            & "        UPDUSER,                                                                   " _
            & "        UPDTERMID,                                                                 " _
            & "        UPDPGID,                                                                   " _
            & "        RECEIVEYMD)                                                                " _
            & "    SELECT                                                                         " _
            & "        ZISSEKIMAIN.RECONO            AS RECONO,                                   " _
            & "        ZISSEKIMAIN.LOADUNLOTYPE      AS LOADUNLOTYPE,                             " _
            & "        ZISSEKIMAIN.STACKINGTYPE      AS STACKINGTYPE,                             " _
            & "        ZISSEKIMAIN.ORDERORGCODE      AS ORDERORGCODE,                             " _
            & "        ZISSEKIMAIN.ORDERORGNAME      AS ORDERORGNAME,                             " _
            & "        ZISSEKIMAIN.KASANAMEORDERORG  AS KASANAMEORDERORG,                         " _
            & "        ZISSEKIMAIN.KASANCODEORDERORG AS KASANCODEORDERORG,                        " _
            & "        ZISSEKIMAIN.ORDERORG          AS ORDERORG,                                 " _
            & "        ZISSEKIMAIN.PRODUCT2NAME      AS PRODUCT2NAME,                             " _
            & "        ZISSEKIMAIN.PRODUCT2          AS PRODUCT2,                                 " _
            & "        ZISSEKIMAIN.PRODUCT1NAME      AS PRODUCT1NAME,                             " _
            & "        ZISSEKIMAIN.PRODUCT1          AS PRODUCT1,                                 " _
            & "        ZISSEKIMAIN.OILNAME           AS OILNAME,                                  " _
            & "        ZISSEKIMAIN.OILTYPE           AS OILTYPE,                                  " _
            & "        ZISSEKIMAIN.TODOKECODE        AS TODOKECODE,                               " _
            & "        ZISSEKIMAIN.TODOKENAME        AS TODOKENAME,                               " _
            & "        ZISSEKIMAIN.TODOKENAMES       AS TODOKENAMES,                              " _
            & "        ZISSEKIMAIN.TORICODE          AS TORICODE,                                 " _
            & "        ZISSEKIMAIN.TORINAME          AS TORINAME,                                 " _
            & "        ZISSEKIMAIN.SHUKABASHO        AS SHUKABASHO,                               " _
            & "        ZISSEKIMAIN.SHUKANAME         AS SHUKANAME,                                " _
            & "        ZISSEKIMAIN.SHUKANAMES        AS SHUKANAMES,                               " _
            & "        ZISSEKIMAIN.SHUKATORICODE     AS SHUKATORICODE,                            " _
            & "        ZISSEKIMAIN.SHUKATORINAME     AS SHUKATORINAME,                            " _
            & "        ZISSEKIMAIN.SHUKADATE         AS SHUKADATE,                                " _
            & "        ZISSEKIMAIN.LOADTIME          AS LOADTIME,                                 " _
            & "        ZISSEKIMAIN.LOADTIMEIN        AS LOADTIMEIN,                               " _
            & "        ZISSEKIMAIN.TODOKEDATE        AS TODOKEDATE,                               " _
            & "        ZISSEKIMAIN.SHITEITIME        AS SHITEITIME,                               " _
            & "        ZISSEKIMAIN.SHITEITIMEIN      AS SHITEITIMEIN,                             " _
            & "        ZISSEKIMAIN.ZYUTYU            AS ZYUTYU,                                   " _
            & "        ZISSEKIMAIN.ZISSEKI           AS ZISSEKI,                                  " _
            & "        ZISSEKIMAIN.TANNI             AS TANNI,                                    " _
            & "        ZISSEKIMAIN.TANKNUM           AS TANKNUM,                                  " _
            & "        ZISSEKIMAIN.TANKNUMBER        AS TANKNUMBER,                               " _
            & "        ZISSEKIMAIN.GYOMUTANKNUM      AS GYOMUTANKNUM,                             " _
            & "        ZISSEKIMAIN.SYAGATA           AS SYAGATA,                                  " _
            & "        ZISSEKIMAIN.SYABARA           AS SYABARA,                                  " _
            & "        ZISSEKIMAIN.NINUSHINAME       AS NINUSHINAME,                              " _
            & "        ZISSEKIMAIN.CONTYPE           AS CONTYPE,                                  " _
            & "        ZISSEKIMAIN.TRIP              AS TRIP,                                     " _
            & "        ZISSEKIMAIN.DRP               AS DRP,                                      " _
            & "        ZISSEKIMAIN.STAFFSLCT         AS STAFFSLCT,                                " _
            & "        ZISSEKIMAIN.STAFFNAME         AS STAFFNAME,                                " _
            & "        ZISSEKIMAIN.STAFFCODE         AS STAFFCODE,                                " _
            & "        ZISSEKIMAIN.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                             " _
            & "        ZISSEKIMAIN.SUBSTAFFNAME      AS SUBSTAFFNAME,                             " _
            & "        ZISSEKIMAIN.SUBSTAFFNUM       AS SUBSTAFFNUM,                              " _
            & "        ZISSEKIMAIN.SHUKODATE         AS SHUKODATE,                                " _
            & "        ZISSEKIMAIN.KIKODATE          AS KIKODATE,                                 " _
            & "        ZISSEKIMAIN.TANKA             AS TANKA,                                    " _
            & "        NULL                          AS JURYORYOKIN,                              " _
            & "        NULL                          AS TSUKORYO,                                 " _
            & "        ZISSEKIMAIN.KYUZITUTANKA      AS KYUZITUTANKA,                             " _
            & "        ZISSEKIMAIN.YUSOUHI           AS YUSOUHI,                                  " _
            & "        ZISSEKIMAIN.CALCKBN           AS CALCKBN,                                  " _
            & "        ZISSEKIMAIN.WORKINGDAY        AS WORKINGDAY,                               " _
            & "        ZISSEKIMAIN.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                        " _
            & "        ZISSEKIMAIN.DELFLG            AS DELFLG,                                   " _
            & "        @INITYMD                      AS INITYMD,                                  " _
            & "        @INITUSER                     AS INITUSER,                                 " _
            & "        @INITTERMID                   AS INITTERMID,                               " _
            & "        @INITPGID                     AS INITPGID,                                 " _
            & "        NULL                          AS UPDYMD,                                   " _
            & "        NULL                          AS UPDUSER,                                  " _
            & "        NULL                          AS UPDTERMID,                                " _
            & "        NULL                          AS UPDPGID,                                  " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                " _
            & "    FROM(                                                                          " _
            & "         SELECT                                                                    " _
            & "             ZISSEKI.RECONO            AS RECONO,                                  " _
            & "             ZISSEKI.LOADUNLOTYPE      AS LOADUNLOTYPE,                            " _
            & "             ZISSEKI.STACKINGTYPE      AS STACKINGTYPE,                            " _
            & "             ZISSEKI.ORDERORGCODE      AS ORDERORGCODE,                            " _
            & "             ZISSEKI.ORDERORGNAME      AS ORDERORGNAME,                            " _
            & "             ZISSEKI.KASANAMEORDERORG  AS KASANAMEORDERORG,                        " _
            & "             ZISSEKI.KASANCODEORDERORG AS KASANCODEORDERORG,                       " _
            & "             ZISSEKI.ORDERORG          AS ORDERORG,                                " _
            & "             ZISSEKI.PRODUCT2NAME      AS PRODUCT2NAME,                            " _
            & "             ZISSEKI.PRODUCT2          AS PRODUCT2,                                " _
            & "             ZISSEKI.PRODUCT1NAME      AS PRODUCT1NAME,                            " _
            & "             ZISSEKI.PRODUCT1          AS PRODUCT1,                                " _
            & "             ZISSEKI.OILNAME           AS OILNAME,                                 " _
            & "             ZISSEKI.OILTYPE           AS OILTYPE,                                 " _
            & "             ZISSEKI.TODOKECODE        AS TODOKECODE,                              " _
            & "             ZISSEKI.TODOKENAME        AS TODOKENAME,                              " _
            & "             ZISSEKI.TODOKENAMES       AS TODOKENAMES,                             " _
            & "             ZISSEKI.TORICODE          AS TORICODE,                                " _
            & "             ZISSEKI.TORINAME          AS TORINAME,                                " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                 " _
            & "             THEN (SELECT SHUKABASHO                                               " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                      " _
            & "                     WHERE                                                         " _
            & "                         TORICODE     = ZISSEKI.TORICODE                           " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                           " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                       " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                            " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                         " _
            & "                     AND DELFLG       = '0'                                        " _
            & "                 )                                                                 " _
            & "             ELSE ZISSEKI.SHUKABASHO                                               " _
            & "             END AS SHUKABASHO,                                                    " _
            & "             CASE ZISSEKI.SHUKABASHO WHEN '006928'                                 " _
            & "             THEN (SELECT SHUKANAME                                                " _
            & "                     FROM LNG.LNT0001_ZISSEKI                                      " _
            & "                     WHERE                                                         " _
            & "                         TORICODE     = ZISSEKI.TORICODE                           " _
            & "                     AND ORDERORG     = ZISSEKI.ORDERORG                           " _
            & "                     AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                       " _
            & "                     AND TRIP         = ZISSEKI.TRIP -1                            " _
            & "                     AND TODOKEDATE   = ZISSEKI.TODOKEDATE                         " _
            & "                     AND DELFLG       = '0'                                        " _
            & "                 )                                                                 " _
            & "             ELSE ZISSEKI.SHUKANAME                                                " _
            & "             END AS SHUKANAME,                                                     " _
            & "             ZISSEKI.SHUKANAMES        AS SHUKANAMES,                              " _
            & "             ZISSEKI.SHUKATORICODE     AS SHUKATORICODE,                           " _
            & "             ZISSEKI.SHUKATORINAME     AS SHUKATORINAME,                           " _
            & "             ZISSEKI.SHUKADATE         AS SHUKADATE,                               " _
            & "             ZISSEKI.LOADTIME          AS LOADTIME,                                " _
            & "             ZISSEKI.LOADTIMEIN        AS LOADTIMEIN,                              " _
            & "             ZISSEKI.TODOKEDATE        AS TODOKEDATE,                              " _
            & "             ZISSEKI.SHITEITIME        AS SHITEITIME,                              " _
            & "             ZISSEKI.SHITEITIMEIN      AS SHITEITIMEIN,                            " _
            & "             ZISSEKI.ZYUTYU            AS ZYUTYU,                                  " _
            & "             ZISSEKI.ZISSEKI           AS ZISSEKI,                                 " _
            & "             ZISSEKI.TANNI             AS TANNI,                                   " _
            & "             ZISSEKI.TANKNUM           AS TANKNUM,                                 " _
            & "             ZISSEKI.TANKNUMBER        AS TANKNUMBER,                              " _
            & "             ZISSEKI.GYOMUTANKNUM      AS GYOMUTANKNUM,                            " _
            & "             ZISSEKI.SYAGATA           AS SYAGATA,                                 " _
            & "             ZISSEKI.SYABARA           AS SYABARA,                                 " _
            & "             ZISSEKI.NINUSHINAME       AS NINUSHINAME,                             " _
            & "             ZISSEKI.CONTYPE           AS CONTYPE,                                 " _
            & "             ZISSEKI.TRIP              AS TRIP,                                    " _
            & "             ZISSEKI.DRP               AS DRP,                                     " _
            & "             ZISSEKI.STAFFSLCT         AS STAFFSLCT,                               " _
            & "             ZISSEKI.STAFFNAME         AS STAFFNAME,                               " _
            & "             ZISSEKI.STAFFCODE         AS STAFFCODE,                               " _
            & "             ZISSEKI.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                            " _
            & "             ZISSEKI.SUBSTAFFNAME      AS SUBSTAFFNAME,                            " _
            & "             ZISSEKI.SUBSTAFFNUM       AS SUBSTAFFNUM,                             " _
            & "             ZISSEKI.SHUKODATE         AS SHUKODATE,                               " _
            & "             ZISSEKI.KIKODATE          AS KIKODATE,                                " _
            & "             CASE WHEN ZISSEKI.TODOKECODE = HOLIDAYRATE.TODOKECODE                 " _
            & "                       AND HOLIDAYRATE.TODOKECATEGORY = '2' THEN NULL              " _
            & "                  ELSE HOLIDAYRATE.TANKA                                           " _
            & "             END                       AS KYUZITUTANKA,                            " _
            & "             TANKA.TANKA               AS TANKA,                                   " _
            & "             CASE TANKA.CALCKBN                                                         " _
            & "               WHEN 'トン' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0) " _
            & "               WHEN '回'   THEN COALESCE(TANKA.TANKA, 0)                                " _
            & "               WHEN '距離' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0) " _
            & "               WHEN '定数' THEN COALESCE(TANKA.TANKA, 0)                                " _
            & "                           ELSE COALESCE(TANKA.TANKA, 0)                                " _
            & "             END                       AS YUSOUHI,                                      " _
            & "             TANKA.CALCKBN             AS CALCKBN,                                 " _
            & "             CALENDAR.WORKINGDAY AS WORKINGDAY,                                    " _
            & "             CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                      " _
            & "             ZISSEKI.DELFLG AS DELFLG                                              " _
            & "         FROM LNG.LNT0001_ZISSEKI ZISSEKI                                          " _
            & "         LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                      " _
            & "             ON @TORICODE = TANKA.TORICODE                                         " _
            & "             AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                              " _
            & "             AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                    " _
            & "             AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                      " _
            & "             AND TANKA.AVOCADOSHUKABASHO = CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                          " _
            & "                                                THEN (SELECT SHUKABASHO                                                                                                   " _
            & "                                                        FROM LNG.LNT0001_ZISSEKI                                                                                          " _
            & "                                                       WHERE                                                                                                              " _
            & "                                                             TORICODE     = ZISSEKI.TORICODE                                                                              " _
            & "                                                         AND ORDERORG     = ZISSEKI.ORDERORG                                                                              " _
            & "                                                         AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                          " _
            & "                                                         AND TRIP         = ZISSEKI.TRIP -1                                                                               " _
            & "                                                         AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                            " _
            & "                                                         AND DELFLG       = '0'                                                                                           " _
            & "                                                     )                                                                                                                    " _
            & "                                                ELSE ZISSEKI.SHUKABASHO                                                                                                   " _
            & "                                           END                                                                                                                            " _
            & "             AND ZISSEKI.GYOMUTANKNUM = TANKA.SHABAN                               " _
            & "             AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                " _
            & "             AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                " _
            & "             AND TANKA.DELFLG = @DELFLG                                            " _
            & "             AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                             " _
            & "         LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                   " _
            & "             ON @TORICODE = CALENDAR.TORICODE                                      " _
            & "             AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                 " _
            & "             AND CALENDAR.DELFLG = @DELFLG                                         " _
            & "         LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                               " _
            & "             ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                              " _
            & "             AND CASE HOLIDAYRATE.ORDERORGCATEGORY                                                                                   " _
            & "                     WHEN '1' THEN HOLIDAYRATE.ORDERORGCODE = ZISSEKI.ORDERORGCODE                                                   " _
            & "                     WHEN '2' THEN HOLIDAYRATE.ORDERORGCODE <> ZISSEKI.ORDERORGCODE                                                  " _
            & "                     ELSE 1 = 1                                                                                                      " _
            & "                 END                                                                                                                 " _
            & "             AND CASE HOLIDAYRATE.SHUKABASHOCATEGORY                                                                                 " _
            & "                     WHEN '1' THEN HOLIDAYRATE.SHUKABASHO = ZISSEKI.SHUKABASHO                                                       " _
            & "                     WHEN '2' THEN HOLIDAYRATE.SHUKABASHO <> ZISSEKI.SHUKABASHO                                                      " _
            & "                     ELSE 1 = 1                                                                                                      " _
            & "                 END                                                                                                                 " _
            & "             AND CASE HOLIDAYRATE.TODOKECATEGORY                                                                                     " _
            & "                     WHEN '1' THEN HOLIDAYRATE.TODOKECODE = ZISSEKI.TODOKECODE                                                       " _
            & "                     WHEN '2' THEN HOLIDAYRATE.TODOKECODE <> ZISSEKI.TODOKECODE                                                      " _
            & "                     ELSE 1 = 1                                                                                                      " _
            & "                 END                                                                                                                 " _
            & "             AND CASE WHEN HOLIDAYRATE.GYOMUTANKNUMFROM = '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                    " _
            & "                             THEN ZISSEKI.GYOMUTANKNUM <= HOLIDAYRATE.GYOMUTANKNUMTO                                                 " _
            & "                     WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO = ''                                     " _
            & "                             THEN ZISSEKI.GYOMUTANKNUM >= HOLIDAYRATE.GYOMUTANKNUMFROM                                               " _
            & "                     WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                    " _
            & "                             THEN ZISSEKI.GYOMUTANKNUM BETWEEN HOLIDAYRATE.GYOMUTANKNUMFROM AND HOLIDAYRATE.GYOMUTANKNUMTO           " _
            & "                     ELSE 1 = 1                                                                                                      " _
            & "                 END                                                                                                                 " _
            & "             AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                     " _
            & "             AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                        " _
            & "         WHERE                                                                     " _
            & "             ZISSEKI.TORICODE = @TORICODE                                          " _
            & "             AND ZISSEKI.ZISSEKI <> 0                                              " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                   " _
            & "             AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                     " _
            & "             AND ZISSEKI.STACKINGTYPE <> '積置'                                    " _
            & "             AND ZISSEKI.DELFLG = @DELFLG                                          " _
            & "             AND ZISSEKI.ORDERORGCODE <> '020104'                                  " _
            & "         ) ZISSEKIMAIN                                                             " _
            & " ON DUPLICATE KEY UPDATE                                                           " _
            & "         RECONO                    = VALUES(RECONO),                               " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                         " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                         " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                         " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                         " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                     " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                    " _
            & "         ORDERORG                  = VALUES(ORDERORG),                             " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                         " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                             " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                         " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                             " _
            & "         OILNAME                   = VALUES(OILNAME),                              " _
            & "         OILTYPE                   = VALUES(OILTYPE),                              " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                           " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                           " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                          " _
            & "         TORICODE                  = VALUES(TORICODE),                             " _
            & "         TORINAME                  = VALUES(TORINAME),                             " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                           " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                            " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                           " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                        " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                        " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                            " _
            & "         LOADTIME                  = VALUES(LOADTIME),                             " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                           " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                           " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                           " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                         " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                               " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                              " _
            & "         TANNI                     = VALUES(TANNI),                                " _
            & "         TANKNUM                   = VALUES(TANKNUM),                              " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                           " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                         " _
            & "         SYAGATA                   = VALUES(SYAGATA),                              " _
            & "         SYABARA                   = VALUES(SYABARA),                              " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                          " _
            & "         CONTYPE                   = VALUES(CONTYPE),                              " _
            & "         TRIP                      = VALUES(TRIP),                                 " _
            & "         DRP                       = VALUES(DRP),                                  " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                            " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                            " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                            " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                         " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                         " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                          " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                            " _
            & "         KIKODATE                  = VALUES(KIKODATE),                             " _
            & "         TANKA                     = VALUES(TANKA),                                " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                          " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                             " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                         " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                              " _
            & "         CALCKBN                   = VALUES(CALCKBN),                              " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                           " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                    " _
            & "         DELFLG                    = @DELFLG,                                      " _
            & "         UPDYMD                    = @UPDYMD,                                      " _
            & "         UPDUSER                   = @UPDUSER,                                     " _
            & "         UPDTERMID                 = @UPDTERMID,                                   " _
            & "         UPDPGID                   = @UPDPGID,                                     " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                  "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(石油資源開発(本州分)輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0021_SEKIYUHONSYUYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 東京ガス輸送費テーブル更新
    ''' </summary>
    Private Sub TOKYOGUS_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(東京ガス輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0027_TOKYOGUSYUSOUHI                              " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東京ガス輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0027_TOKYOGUSYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(東京ガス輸送費テーブル)
            SQLStr =
              " INSERT INTO LNG.LNT0027_TOKYOGUSYUSOUHI(                                      " _
            & "     RECONO,                                                                   " _
            & "     LOADUNLOTYPE,                                                             " _
            & "     STACKINGTYPE,                                                             " _
            & "     ORDERORGCODE,                                                             " _
            & "     ORDERORGNAME,                                                             " _
            & "     KASANAMEORDERORG,                                                         " _
            & "     KASANCODEORDERORG,                                                        " _
            & "     ORDERORG,                                                                 " _
            & "     PRODUCT2NAME,                                                             " _
            & "     PRODUCT2,                                                                 " _
            & "     PRODUCT1NAME,                                                             " _
            & "     PRODUCT1,                                                                 " _
            & "     OILNAME,                                                                  " _
            & "     OILTYPE,                                                                  " _
            & "     TODOKECODE,                                                               " _
            & "     TODOKENAME,                                                               " _
            & "     TODOKENAMES,                                                              " _
            & "     TORICODE,                                                                 " _
            & "     TORINAME,                                                                 " _
            & "     SHUKABASHO,                                                               " _
            & "     SHUKANAME,                                                                " _
            & "     SHUKANAMES,                                                               " _
            & "     SHUKATORICODE,                                                            " _
            & "     SHUKATORINAME,                                                            " _
            & "     SHUKADATE,                                                                " _
            & "     LOADTIME,                                                                 " _
            & "     LOADTIMEIN,                                                               " _
            & "     TODOKEDATE,                                                               " _
            & "     SHITEITIME,                                                               " _
            & "     SHITEITIMEIN,                                                             " _
            & "     ZYUTYU,                                                                   " _
            & "     ZISSEKI,                                                                  " _
            & "     TANNI,                                                                    " _
            & "     TANKNUM,                                                                  " _
            & "     TANKNUMBER,                                                               " _
            & "     GYOMUTANKNUM,                                                             " _
            & "     SYAGATA,                                                                  " _
            & "     SYABARA,                                                                  " _
            & "     NINUSHINAME,                                                              " _
            & "     CONTYPE,                                                                  " _
            & "     TRIP,                                                                     " _
            & "     DRP,                                                                      " _
            & "     STAFFSLCT,                                                                " _
            & "     STAFFNAME,                                                                " _
            & "     STAFFCODE,                                                                " _
            & "     SUBSTAFFSLCT,                                                             " _
            & "     SUBSTAFFNAME,                                                             " _
            & "     SUBSTAFFNUM,                                                              " _
            & "     SHUKODATE,                                                                " _
            & "     KIKODATE,                                                                 " _
            & "     TANKA,                                                                    " _
            & "     JURYORYOKIN,                                                              " _
            & "     TSUKORYO,                                                                 " _
            & "     KYUZITUTANKA,                                                             " _
            & "     YUSOUHI,                                                                  " _
            & "     CALCKBN,                                                                  " _
            & "     WORKINGDAY,                                                               " _
            & "     PUBLICHOLIDAYNAME,                                                        " _
            & "     DELFLG,                                                                   " _
            & "     INITYMD,                                                                  " _
            & "     INITUSER,                                                                 " _
            & "     INITTERMID,                                                               " _
            & "     INITPGID,                                                                 " _
            & "     UPDYMD,                                                                   " _
            & "     UPDUSER,                                                                  " _
            & "     UPDTERMID,                                                                " _
            & "     UPDPGID,                                                                  " _
            & "     RECEIVEYMD)                                                               " _
            & " SELECT                                                                        " _
            & "     ZISSEKIMAIN.RECONO            AS RECONO,                                  " _
            & "     ZISSEKIMAIN.LOADUNLOTYPE      AS LOADUNLOTYPE,                            " _
            & "     ZISSEKIMAIN.STACKINGTYPE      AS STACKINGTYPE,                            " _
            & "     ZISSEKIMAIN.ORDERORGCODE      AS ORDERORGCODE,                            " _
            & "     ZISSEKIMAIN.ORDERORGNAME      AS ORDERORGNAME,                            " _
            & "     ZISSEKIMAIN.KASANAMEORDERORG  AS KASANAMEORDERORG,                        " _
            & "     ZISSEKIMAIN.KASANCODEORDERORG AS KASANCODEORDERORG,                       " _
            & "     ZISSEKIMAIN.ORDERORG          AS ORDERORG,                                " _
            & "     ZISSEKIMAIN.PRODUCT2NAME      AS PRODUCT2NAME,                            " _
            & "     ZISSEKIMAIN.PRODUCT2          AS PRODUCT2,                                " _
            & "     ZISSEKIMAIN.PRODUCT1NAME      AS PRODUCT1NAME,                            " _
            & "     ZISSEKIMAIN.PRODUCT1          AS PRODUCT1,                                " _
            & "     ZISSEKIMAIN.OILNAME           AS OILNAME,                                 " _
            & "     ZISSEKIMAIN.OILTYPE           AS OILTYPE,                                 " _
            & "     ZISSEKIMAIN.TODOKECODE        AS TODOKECODE,                              " _
            & "     ZISSEKIMAIN.TODOKENAME        AS TODOKENAME,                              " _
            & "     ZISSEKIMAIN.TODOKENAMES       AS TODOKENAMES,                             " _
            & "     ZISSEKIMAIN.TORICODE          AS TORICODE,                                " _
            & "     ZISSEKIMAIN.TORINAME          AS TORINAME,                                " _
            & "     ZISSEKIMAIN.SHUKABASHO        AS SHUKABASHO,                              " _
            & "     ZISSEKIMAIN.SHUKANAME         AS SHUKANAME,                               " _
            & "     ZISSEKIMAIN.SHUKANAMES        AS SHUKANAMES,                              " _
            & "     ZISSEKIMAIN.SHUKATORICODE     AS SHUKATORICODE,                           " _
            & "     ZISSEKIMAIN.SHUKATORINAME     AS SHUKATORINAME,                           " _
            & "     ZISSEKIMAIN.SHUKADATE         AS SHUKADATE,                               " _
            & "     ZISSEKIMAIN.LOADTIME          AS LOADTIME,                                " _
            & "     ZISSEKIMAIN.LOADTIMEIN        AS LOADTIMEIN,                              " _
            & "     ZISSEKIMAIN.TODOKEDATE        AS TODOKEDATE,                              " _
            & "     ZISSEKIMAIN.SHITEITIME        AS SHITEITIME,                              " _
            & "     ZISSEKIMAIN.SHITEITIMEIN      AS SHITEITIMEIN,                            " _
            & "     ZISSEKIMAIN.ZYUTYU            AS ZYUTYU,                                  " _
            & "     ZISSEKIMAIN.ZISSEKI           AS ZISSEKI,                                 " _
            & "     ZISSEKIMAIN.TANNI             AS TANNI,                                   " _
            & "     ZISSEKIMAIN.TANKNUM           AS TANKNUM,                                 " _
            & "     ZISSEKIMAIN.TANKNUMBER        AS TANKNUMBER,                              " _
            & "     ZISSEKIMAIN.GYOMUTANKNUM      AS GYOMUTANKNUM,                            " _
            & "     ZISSEKIMAIN.SYAGATA           AS SYAGATA,                                 " _
            & "     ZISSEKIMAIN.SYABARA           AS SYABARA,                                 " _
            & "     ZISSEKIMAIN.NINUSHINAME       AS NINUSHINAME,                             " _
            & "     ZISSEKIMAIN.CONTYPE           AS CONTYPE,                                 " _
            & "     ZISSEKIMAIN.TRIP              AS TRIP,                                    " _
            & "     ZISSEKIMAIN.DRP               AS DRP,                                     " _
            & "     ZISSEKIMAIN.STAFFSLCT         AS STAFFSLCT,                               " _
            & "     ZISSEKIMAIN.STAFFNAME         AS STAFFNAME,                               " _
            & "     ZISSEKIMAIN.STAFFCODE         AS STAFFCODE,                               " _
            & "     ZISSEKIMAIN.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                            " _
            & "     ZISSEKIMAIN.SUBSTAFFNAME      AS SUBSTAFFNAME,                            " _
            & "     ZISSEKIMAIN.SUBSTAFFNUM       AS SUBSTAFFNUM,                             " _
            & "     ZISSEKIMAIN.SHUKODATE         AS SHUKODATE,                               " _
            & "     ZISSEKIMAIN.KIKODATE          AS KIKODATE,                                " _
            & "     ZISSEKIMAIN.TANKA             AS TANKA,                                   " _
            & "     NULL                          AS JURYORYOKIN,                             " _
            & "     NULL                          AS TSUKORYO,                                " _
            & "     ZISSEKIMAIN.KYUZITUTANKA      AS KYUZITUTANKA,                            " _
            & "     ZISSEKIMAIN.YUSOUHI           AS YUSOUHI,                                 " _
            & "     ZISSEKIMAIN.CALCKBN           AS CALCKBN,                                 " _
            & "     ZISSEKIMAIN.WORKINGDAY        AS WORKINGDAY,                              " _
            & "     ZISSEKIMAIN.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                       " _
            & "     ZISSEKIMAIN.DELFLG            AS DELFLG,                                  " _
            & "     @INITYMD                      AS INITYMD,                                 " _
            & "     @INITUSER                     AS INITUSER,                                " _
            & "     @INITTERMID                   AS INITTERMID,                              " _
            & "     @INITPGID                     AS INITPGID,                                " _
            & "     NULL                          AS UPDYMD,                                  " _
            & "     NULL                          AS UPDUSER,                                 " _
            & "     NULL                          AS UPDTERMID,                               " _
            & "     NULL                          AS UPDPGID,                                 " _
            & "     @RECEIVEYMD                   AS RECEIVEYMD                               " _
            & " FROM(                                                                         " _
            & "      SELECT                                                                   " _
            & "          ZISSEKI.RECONO            AS RECONO,                                 " _
            & "          ZISSEKI.LOADUNLOTYPE      AS LOADUNLOTYPE,                           " _
            & "          ZISSEKI.STACKINGTYPE      AS STACKINGTYPE,                           " _
            & "          ZISSEKI.ORDERORGCODE      AS ORDERORGCODE,                           " _
            & "          ZISSEKI.ORDERORGNAME      AS ORDERORGNAME,                           " _
            & "          ZISSEKI.KASANAMEORDERORG  AS KASANAMEORDERORG,                       " _
            & "          ZISSEKI.KASANCODEORDERORG AS KASANCODEORDERORG,                      " _
            & "          ZISSEKI.ORDERORG          AS ORDERORG,                               " _
            & "          ZISSEKI.PRODUCT2NAME      AS PRODUCT2NAME,                           " _
            & "          ZISSEKI.PRODUCT2          AS PRODUCT2,                               " _
            & "          ZISSEKI.PRODUCT1NAME      AS PRODUCT1NAME,                           " _
            & "          ZISSEKI.PRODUCT1          AS PRODUCT1,                               " _
            & "          ZISSEKI.OILNAME           AS OILNAME,                                " _
            & "          ZISSEKI.OILTYPE           AS OILTYPE,                                " _
            & "          ZISSEKI.TODOKECODE        AS TODOKECODE,                             " _
            & "          ZISSEKI.TODOKENAME        AS TODOKENAME,                             " _
            & "          ZISSEKI.TODOKENAMES       AS TODOKENAMES,                            " _
            & "          ZISSEKI.TORICODE          AS TORICODE,                               " _
            & "          ZISSEKI.TORINAME          AS TORINAME,                               " _
            & "          CASE ZISSEKI.SHUKABASHO WHEN '006928'                                " _
            & "          THEN (SELECT SHUKABASHO                                              " _
            & "                  FROM LNG.LNT0001_ZISSEKI                                     " _
            & "                  WHERE                                                        " _
            & "                      TORICODE     = ZISSEKI.TORICODE                          " _
            & "                  AND ORDERORG     = ZISSEKI.ORDERORG                          " _
            & "                  AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                      " _
            & "                  AND TRIP         = ZISSEKI.TRIP -1                           " _
            & "                  AND TODOKEDATE   = ZISSEKI.TODOKEDATE                        " _
            & "                  AND DELFLG       = '0'                                       " _
            & "              )                                                                " _
            & "          ELSE ZISSEKI.SHUKABASHO                                              " _
            & "          END AS SHUKABASHO,                                                   " _
            & "          CASE ZISSEKI.SHUKABASHO WHEN '006928'                                " _
            & "          THEN (SELECT SHUKANAME                                               " _
            & "                  FROM LNG.LNT0001_ZISSEKI                                     " _
            & "                  WHERE                                                        " _
            & "                      TORICODE     = ZISSEKI.TORICODE                          " _
            & "                  AND ORDERORG     = ZISSEKI.ORDERORG                          " _
            & "                  AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                      " _
            & "                  AND TRIP         = ZISSEKI.TRIP -1                           " _
            & "                  AND TODOKEDATE   = ZISSEKI.TODOKEDATE                        " _
            & "                  AND DELFLG       = '0'                                       " _
            & "              )                                                                " _
            & "          ELSE ZISSEKI.SHUKANAME                                               " _
            & "          END AS SHUKANAME,                                                    " _
            & "          ZISSEKI.SHUKANAMES        AS SHUKANAMES,                             " _
            & "          ZISSEKI.SHUKATORICODE     AS SHUKATORICODE,                          " _
            & "          ZISSEKI.SHUKATORINAME     AS SHUKATORINAME,                          " _
            & "          ZISSEKI.SHUKADATE         AS SHUKADATE,                              " _
            & "          ZISSEKI.LOADTIME          AS LOADTIME,                               " _
            & "          ZISSEKI.LOADTIMEIN        AS LOADTIMEIN,                             " _
            & "          ZISSEKI.TODOKEDATE        AS TODOKEDATE,                             " _
            & "          ZISSEKI.SHITEITIME        AS SHITEITIME,                             " _
            & "          ZISSEKI.SHITEITIMEIN      AS SHITEITIMEIN,                           " _
            & "          ZISSEKI.ZYUTYU            AS ZYUTYU,                                 " _
            & "          ZISSEKI.ZISSEKI           AS ZISSEKI,                                " _
            & "          ZISSEKI.TANNI             AS TANNI,                                  " _
            & "          ZISSEKI.TANKNUM           AS TANKNUM,                                " _
            & "          ZISSEKI.TANKNUMBER        AS TANKNUMBER,                             " _
            & "          ZISSEKI.GYOMUTANKNUM      AS GYOMUTANKNUM,                           " _
            & "          ZISSEKI.SYAGATA           AS SYAGATA,                                " _
            & "          ZISSEKI.SYABARA           AS SYABARA,                                " _
            & "          ZISSEKI.NINUSHINAME       AS NINUSHINAME,                            " _
            & "          ZISSEKI.CONTYPE           AS CONTYPE,                                " _
            & "          ZISSEKI.TRIP              AS TRIP,                                   " _
            & "          ZISSEKI.DRP               AS DRP,                                    " _
            & "          ZISSEKI.STAFFSLCT         AS STAFFSLCT,                              " _
            & "          ZISSEKI.STAFFNAME         AS STAFFNAME,                              " _
            & "          ZISSEKI.STAFFCODE         AS STAFFCODE,                              " _
            & "          ZISSEKI.SUBSTAFFSLCT      AS SUBSTAFFSLCT,                           " _
            & "          ZISSEKI.SUBSTAFFNAME      AS SUBSTAFFNAME,                           " _
            & "          ZISSEKI.SUBSTAFFNUM       AS SUBSTAFFNUM,                            " _
            & "          ZISSEKI.SHUKODATE         AS SHUKODATE,                              " _
            & "          ZISSEKI.KIKODATE          AS KIKODATE,                               " _
            & "          NULL                      AS KYUZITUTANKA,                           " _
            & "          TANKA.TANKA               AS TANKA,                                  " _
            & "          CASE TANKA.CALCKBN                                                         " _
            & "            WHEN 'トン' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0) " _
            & "            WHEN '回'   THEN COALESCE(TANKA.TANKA, 0)                                " _
            & "            WHEN '距離' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0) " _
            & "            WHEN '定数' THEN COALESCE(TANKA.TANKA, 0)                                " _
            & "                        ELSE COALESCE(TANKA.TANKA, 0)                                " _
            & "          END                       AS YUSOUHI,                                      " _
            & "          TANKA.CALCKBN             AS CALCKBN,                                " _
            & "          CALENDAR.WORKINGDAY       AS WORKINGDAY,                             " _
            & "          CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                     " _
            & "          ZISSEKI.DELFLG            AS DELFLG                                  " _
            & "      FROM LNG.LNT0001_ZISSEKI ZISSEKI                                         " _
            & "      LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                     " _
            & "          ON @TORICODE = TANKA.TORICODE                                        " _
            & "          AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                             " _
            & "          AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                   " _
            & "          AND TANKA.AVOCADOSHUKABASHO = CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                          " _
            & "                                             THEN (SELECT SHUKABASHO                                                                                                   " _
            & "                                                     FROM LNG.LNT0001_ZISSEKI                                                                                          " _
            & "                                                    WHERE                                                                                                              " _
            & "                                                          TORICODE     = ZISSEKI.TORICODE                                                                              " _
            & "                                                      AND ORDERORG     = ZISSEKI.ORDERORG                                                                              " _
            & "                                                      AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                          " _
            & "                                                      AND TRIP         = ZISSEKI.TRIP -1                                                                               " _
            & "                                                      AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                            " _
            & "                                                      AND DELFLG       = '0'                                                                                           " _
            & "                                                  )                                                                                                                    " _
            & "                                             ELSE ZISSEKI.SHUKABASHO                                                                                                   " _
            & "                                        END                                                                                                                            " _
            & "          AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                     " _
            & "          AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                            " _
            & "          AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                               " _
            & "          AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                               " _
            & "          AND TANKA.DELFLG = @DELFLG                                           " _
            & "      LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                  " _
            & "          ON @TORICODE = CALENDAR.TORICODE                                     " _
            & "          AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                " _
            & "          AND CALENDAR.DELFLG = @DELFLG                                        " _
            & "      WHERE                                                                    " _
            & "          ZISSEKI.TORICODE = @TORICODE                                         " _
            & "          AND ZISSEKI.ZISSEKI <> 0                                             " _
            & "          AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                  " _
            & "          AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                    " _
            & "          AND ZISSEKI.STACKINGTYPE <> '積置'                                   " _
            & "          AND ZISSEKI.DELFLG = @DELFLG                                         " _
            & " ) ZISSEKIMAIN                                                                 " _
            & " ON DUPLICATE KEY UPDATE                                                       " _
            & "         RECONO                    = VALUES(RECONO),                           " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                     " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                     " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                     " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                     " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                 " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                " _
            & "         ORDERORG                  = VALUES(ORDERORG),                         " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                     " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                         " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                     " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                         " _
            & "         OILNAME                   = VALUES(OILNAME),                          " _
            & "         OILTYPE                   = VALUES(OILTYPE),                          " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                       " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                       " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                      " _
            & "         TORICODE                  = VALUES(TORICODE),                         " _
            & "         TORINAME                  = VALUES(TORINAME),                         " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                       " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                        " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                       " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                    " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                    " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                        " _
            & "         LOADTIME                  = VALUES(LOADTIME),                         " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                       " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                       " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                       " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                     " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                           " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                          " _
            & "         TANNI                     = VALUES(TANNI),                            " _
            & "         TANKNUM                   = VALUES(TANKNUM),                          " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                       " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                     " _
            & "         SYAGATA                   = VALUES(SYAGATA),                          " _
            & "         SYABARA                   = VALUES(SYABARA),                          " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                      " _
            & "         CONTYPE                   = VALUES(CONTYPE),                          " _
            & "         TRIP                      = VALUES(TRIP),                             " _
            & "         DRP                       = VALUES(DRP),                              " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                        " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                        " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                        " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                     " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                     " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                      " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                        " _
            & "         KIKODATE                  = VALUES(KIKODATE),                         " _
            & "         TANKA                     = VALUES(TANKA),                            " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                      " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                         " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                     " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                          " _
            & "         CALCKBN                   = VALUES(CALCKBN),                          " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                       " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                " _
            & "         DELFLG                    = @DELFLG,                                  " _
            & "         UPDYMD                    = @UPDYMD,                                  " _
            & "         UPDUSER                   = @UPDUSER,                                 " _
            & "         UPDTERMID                 = @UPDTERMID,                               " _
            & "         UPDPGID                   = @UPDPGID,                                 " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                              "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東京ガス輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0027_TOKYOGUSYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 東北天然ガス輸送費テーブル更新
    ''' </summary>
    Private Sub TNG_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(東北天然ガス輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0017_TNGYUSOUHI                                   " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東北天然ガス輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0017_TNGYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(東北天然ガス輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0017_TNGYUSOUHI(                                                                      " _
            & "        RECONO,                                                                                              " _
            & "        LOADUNLOTYPE,                                                                                        " _
            & "        STACKINGTYPE,                                                                                        " _
            & "        ORDERORGCODE,                                                                                        " _
            & "        ORDERORGNAME,                                                                                        " _
            & "        KASANAMEORDERORG,                                                                                    " _
            & "        KASANCODEORDERORG,                                                                                   " _
            & "        ORDERORG,                                                                                            " _
            & "        PRODUCT2NAME,                                                                                        " _
            & "        PRODUCT2,                                                                                            " _
            & "        PRODUCT1NAME,                                                                                        " _
            & "        PRODUCT1,                                                                                            " _
            & "        OILNAME,                                                                                             " _
            & "        OILTYPE,                                                                                             " _
            & "        TODOKECODE,                                                                                          " _
            & "        TODOKENAME,                                                                                          " _
            & "        TODOKENAMES,                                                                                         " _
            & "        TORICODE,                                                                                            " _
            & "        TORINAME,                                                                                            " _
            & "        SHUKABASHO,                                                                                          " _
            & "        SHUKANAME,                                                                                           " _
            & "        SHUKANAMES,                                                                                          " _
            & "        SHUKATORICODE,                                                                                       " _
            & "        SHUKATORINAME,                                                                                       " _
            & "        SHUKADATE,                                                                                           " _
            & "        LOADTIME,                                                                                            " _
            & "        LOADTIMEIN,                                                                                          " _
            & "        TODOKEDATE,                                                                                          " _
            & "        SHITEITIME,                                                                                          " _
            & "        SHITEITIMEIN,                                                                                        " _
            & "        ZYUTYU,                                                                                              " _
            & "        ZISSEKI,                                                                                             " _
            & "        TANNI,                                                                                               " _
            & "        TANKNUM,                                                                                             " _
            & "        TANKNUMBER,                                                                                          " _
            & "        GYOMUTANKNUM,                                                                                        " _
            & "        SYAGATA,                                                                                             " _
            & "        SYABARA,                                                                                             " _
            & "        NINUSHINAME,                                                                                         " _
            & "        CONTYPE,                                                                                             " _
            & "        TRIP,                                                                                                " _
            & "        DRP,                                                                                                 " _
            & "        STAFFSLCT,                                                                                           " _
            & "        STAFFNAME,                                                                                           " _
            & "        STAFFCODE,                                                                                           " _
            & "        SUBSTAFFSLCT,                                                                                        " _
            & "        SUBSTAFFNAME,                                                                                        " _
            & "        SUBSTAFFNUM,                                                                                         " _
            & "        SHUKODATE,                                                                                           " _
            & "        KIKODATE,                                                                                            " _
            & "        TANKA,                                                                                               " _
            & "        JURYORYOKIN,                                                                                         " _
            & "        TSUKORYO,                                                                                            " _
            & "        KYUZITUTANKA,                                                                                        " _
            & "        YUSOUHI,                                                                                             " _
            & "        CALCKBN,                                                                                             " _
            & "        WORKINGDAY,                                                                                          " _
            & "        PUBLICHOLIDAYNAME,                                                                                   " _
            & "        DELFLG,                                                                                              " _
            & "        INITYMD,                                                                                             " _
            & "        INITUSER,                                                                                            " _
            & "        INITTERMID,                                                                                          " _
            & "        INITPGID,                                                                                            " _
            & "        UPDYMD,                                                                                              " _
            & "        UPDUSER,                                                                                             " _
            & "        UPDTERMID,                                                                                           " _
            & "        UPDPGID,                                                                                             " _
            & "        RECEIVEYMD)                                                                                          " _
            & "    SELECT                                                                                                   " _
            & "        ZISSEKI.RECONO                AS RECONO,                                                             " _
            & "        ZISSEKI.LOADUNLOTYPE          AS LOADUNLOTYPE,                                                       " _
            & "        ZISSEKI.STACKINGTYPE          AS STACKINGTYPE,                                                       " _
            & "        ZISSEKI.ORDERORGCODE          AS ORDERORGCODE,                                                       " _
            & "        ZISSEKI.ORDERORGNAME          AS ORDERORGNAME,                                                       " _
            & "        ZISSEKI.KASANAMEORDERORG      AS KASANAMEORDERORG,                                                   " _
            & "        ZISSEKI.KASANCODEORDERORG     AS KASANCODEORDERORG,                                                  " _
            & "        ZISSEKI.ORDERORG              AS ORDERORG,                                                           " _
            & "        ZISSEKI.PRODUCT2NAME          AS PRODUCT2NAME,                                                       " _
            & "        ZISSEKI.PRODUCT2              AS PRODUCT2,                                                           " _
            & "        ZISSEKI.PRODUCT1NAME          AS PRODUCT1NAME,                                                       " _
            & "        ZISSEKI.PRODUCT1              AS PRODUCT1,                                                           " _
            & "        ZISSEKI.OILNAME               AS OILNAME,                                                            " _
            & "        ZISSEKI.OILTYPE               AS OILTYPE,                                                            " _
            & "        ZISSEKI.TODOKECODE            AS TODOKECODE,                                                         " _
            & "        ZISSEKI.TODOKENAME            AS TODOKENAME,                                                         " _
            & "        ZISSEKI.TODOKENAMES           AS TODOKENAMES,                                                        " _
            & "        ZISSEKI.TORICODE              AS TORICODE,                                                           " _
            & "        ZISSEKI.TORINAME              AS TORINAME,                                                           " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKABASHO                                                                              " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "                WHERE                                                                                        " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                          " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                          " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                      " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                           " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                        " _
            & "                AND DELFLG       = '0'                                                                       " _
            & "            )                                                                                                " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                                              " _
            & "        END AS SHUKABASHO,                                                                                   " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKANAME                                                                               " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "                WHERE                                                                                        " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                          " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                          " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                      " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                           " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                        " _
            & "                AND DELFLG       = '0'                                                                       " _
            & "            )                                                                                                " _
            & "        ELSE ZISSEKI.SHUKANAME                                                                               " _
            & "        END AS SHUKANAME,                                                                                    " _
            & "        ZISSEKI.SHUKANAMES            AS SHUKANAMES,                                                         " _
            & "        ZISSEKI.SHUKATORICODE         AS SHUKATORICODE,                                                      " _
            & "        ZISSEKI.SHUKATORINAME         AS SHUKATORINAME,                                                      " _
            & "        ZISSEKI.SHUKADATE             AS SHUKADATE,                                                          " _
            & "        ZISSEKI.LOADTIME              AS LOADTIME,                                                           " _
            & "        ZISSEKI.LOADTIMEIN            AS LOADTIMEIN,                                                         " _
            & "        ZISSEKI.TODOKEDATE            AS TODOKEDATE,                                                         " _
            & "        ZISSEKI.SHITEITIME            AS SHITEITIME,                                                         " _
            & "        ZISSEKI.SHITEITIMEIN          AS SHITEITIMEIN,                                                       " _
            & "        ZISSEKI.ZYUTYU                AS ZYUTYU,                                                             " _
            & "        ZISSEKI.ZISSEKI               AS ZISSEKI,                                                            " _
            & "        ZISSEKI.TANNI                 AS TANNI,                                                              " _
            & "        ZISSEKI.TANKNUM               AS TANKNUM,                                                            " _
            & "        ZISSEKI.TANKNUMBER            AS TANKNUMBER,                                                         " _
            & "        ZISSEKI.GYOMUTANKNUM          AS GYOMUTANKNUM,                                                       " _
            & "        ZISSEKI.SYAGATA               AS SYAGATA,                                                            " _
            & "        ZISSEKI.SYABARA               AS SYABARA,                                                            " _
            & "        ZISSEKI.NINUSHINAME           AS NINUSHINAME,                                                        " _
            & "        ZISSEKI.CONTYPE               AS CONTYPE,                                                            " _
            & "        ZISSEKI.TRIP                  AS TRIP,                                                               " _
            & "        ZISSEKI.DRP                   AS DRP,                                                                " _
            & "        ZISSEKI.STAFFSLCT             AS STAFFSLCT,                                                          " _
            & "        ZISSEKI.STAFFNAME             AS STAFFNAME,                                                          " _
            & "        ZISSEKI.STAFFCODE             AS STAFFCODE,                                                          " _
            & "        ZISSEKI.SUBSTAFFSLCT          AS SUBSTAFFSLCT,                                                       " _
            & "        ZISSEKI.SUBSTAFFNAME          AS SUBSTAFFNAME,                                                       " _
            & "        ZISSEKI.SUBSTAFFNUM           AS SUBSTAFFNUM,                                                        " _
            & "        ZISSEKI.SHUKODATE             AS SHUKODATE,                                                          " _
            & "        ZISSEKI.KIKODATE              AS KIKODATE,                                                           " _
            & "        TANKA.TANKA                   AS TANKA,                                                              " _
            & "        NULL                          AS JURYORYOKIN,                                                        " _
            & "        NULL                          AS TSUKORYO,                                                           " _
            & "        HOLIDAYRATE.TANKA             AS KYUZITUTANKA,                                                       " _
            & "        CASE TANKA.CALCKBN                                                                                   " _
            & "          WHEN 'トン' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                           " _
            & "          WHEN '回'   THEN COALESCE(TANKA.TANKA, 0)                                                          " _
            & "          WHEN '距離' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0)                           " _
            & "          WHEN '定数' THEN COALESCE(TANKA.TANKA, 0)                                                          " _
            & "                      ELSE COALESCE(TANKA.TANKA, 0)                                                          " _
            & "        END                           AS YUSOUHI,                                                            " _
            & "        TANKA.CALCKBN                 AS CALCKBN,                                                            " _
            & "        CALENDAR.WORKINGDAY           AS WORKINGDAY,                                                         " _
            & "        CALENDAR.PUBLICHOLIDAYNAME    AS PUBLICHOLIDAYNAME,                                                  " _
            & "        ZISSEKI.DELFLG                AS DELFLG,                                                             " _
            & "        @INITYMD                      AS INITYMD,                                                            " _
            & "        @INITUSER                     AS INITUSER,                                                           " _
            & "        @INITTERMID                   AS INITTERMID,                                                         " _
            & "        @INITPGID                     AS INITPGID,                                                           " _
            & "        NULL                          AS UPDYMD,                                                             " _
            & "        NULL                          AS UPDUSER,                                                            " _
            & "        NULL                          AS UPDTERMID,                                                          " _
            & "        NULL                          AS UPDPGID,                                                            " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                                          " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                         " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                     " _
            & "        ON @TORICODE = TANKA.TORICODE                                                                        " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                             " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                   " _
            & "        AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                     " _
            & "        AND ZISSEKI.GYOMUTANKNUM = TANKA.SHABAN                                                              " _
            & "        AND TANKA.AVOCADOSHUKABASHO = CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                                          " _
            & "                                           THEN (SELECT SHUKABASHO                                                                                                   " _
            & "                                                   FROM LNG.LNT0001_ZISSEKI                                                                                          " _
            & "                                                  WHERE                                                                                                              " _
            & "                                                        TORICODE     = ZISSEKI.TORICODE                                                                              " _
            & "                                                    AND ORDERORG     = ZISSEKI.ORDERORG                                                                              " _
            & "                                                    AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                          " _
            & "                                                    AND TRIP         = ZISSEKI.TRIP -1                                                                               " _
            & "                                                    AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                            " _
            & "                                                    AND DELFLG       = '0'                                                                                           " _
            & "                                                )                                                                                                                    " _
            & "                                           ELSE ZISSEKI.SHUKABASHO                                                                                                   " _
            & "                                      END                                                                                                                            " _
            & "        AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.DELFLG = @DELFLG                                                                           " _
            & "        AND ZISSEKI.BRANCHCODE = TANKA.BRANCHCODE                                                            " _
            & "    LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                  " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                                     " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                                        " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                           " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                           " _
            & "       AND CASE HOLIDAYRATE.ORDERORGCATEGORY                                                                                " _
            & "             WHEN '1' THEN HOLIDAYRATE.ORDERORGCODE = ZISSEKI.ORDERORGCODE                                                  " _
            & "             WHEN '2' THEN HOLIDAYRATE.ORDERORGCODE <> ZISSEKI.ORDERORGCODE                                                 " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE HOLIDAYRATE.SHUKABASHOCATEGORY                                                                              " _
            & "             WHEN '1' THEN HOLIDAYRATE.SHUKABASHO = ZISSEKI.SHUKABASHO                                                      " _
            & "             WHEN '2' THEN HOLIDAYRATE.SHUKABASHO <> ZISSEKI.SHUKABASHO                                                     " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE HOLIDAYRATE.TODOKECATEGORY                                                                                  " _
            & "             WHEN '1' THEN HOLIDAYRATE.TODOKECODE = ZISSEKI.TODOKECODE                                                      " _
            & "             WHEN '2' THEN HOLIDAYRATE.TODOKECODE <> ZISSEKI.TODOKECODE                                                     " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE WHEN HOLIDAYRATE.GYOMUTANKNUMFROM = '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                 " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM <= HOLIDAYRATE.GYOMUTANKNUMTO                                                " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO = ''                                 " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM >= HOLIDAYRATE.GYOMUTANKNUMFROM                                              " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM BETWEEN HOLIDAYRATE.GYOMUTANKNUMFROM AND HOLIDAYRATE.GYOMUTANKNUMTO          " _
            & "                ELSE 1 = 1                                                                                                  " _
            & "           END                                                                                                              " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                  " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                     " _
            & "    WHERE                                                                                                    " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                                         " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                                             " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                                                   " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                  " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                    " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                                         " _
            & " ON DUPLICATE KEY UPDATE                                                                                     " _
            & "         RECONO                    = VALUES(RECONO),                                                         " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                   " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                   " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                   " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                   " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                               " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                              " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                       " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                   " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                       " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                   " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                       " _
            & "         OILNAME                   = VALUES(OILNAME),                                                        " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                        " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                     " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                     " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                    " _
            & "         TORICODE                  = VALUES(TORICODE),                                                       " _
            & "         TORINAME                  = VALUES(TORINAME),                                                       " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                     " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                      " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                     " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                  " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                  " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                      " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                       " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                     " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                     " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                     " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                   " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                         " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                        " _
            & "         TANNI                     = VALUES(TANNI),                                                          " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                        " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                     " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                   " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                        " _
            & "         SYABARA                   = VALUES(SYABARA),                                                        " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                    " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                        " _
            & "         TRIP                      = VALUES(TRIP),                                                           " _
            & "         DRP                       = VALUES(DRP),                                                            " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                      " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                      " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                      " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                   " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                   " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                    " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                      " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                       " _
            & "         TANKA                     = VALUES(TANKA),                                                          " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                    " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                       " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                   " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                        " _
            & "         CALCKBN                   = VALUES(CALCKBN),                                                        " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                     " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                              " _
            & "         DELFLG                    = @DELFLG,                                                                " _
            & "         UPDYMD                    = @UPDYMD,                                                                " _
            & "         UPDUSER                   = @UPDUSER,                                                               " _
            & "         UPDTERMID                 = @UPDTERMID,                                                             " _
            & "         UPDPGID                   = @UPDPGID,                                                               " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                            "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東北天然ガス輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0017_TNGYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 東北電力輸送費テーブル更新
    ''' </summary>
    Private Sub TOHOKU_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(東北電力輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0018_TOHOKUYUSOUHI                                " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東北電力輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0018_TOHOKUYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(東北電力輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0018_TOHOKUYUSOUHI(                                                                   " _
            & "        RECONO,                                                                                              " _
            & "        LOADUNLOTYPE,                                                                                        " _
            & "        STACKINGTYPE,                                                                                        " _
            & "        ORDERORGCODE,                                                                                        " _
            & "        ORDERORGNAME,                                                                                        " _
            & "        KASANAMEORDERORG,                                                                                    " _
            & "        KASANCODEORDERORG,                                                                                   " _
            & "        ORDERORG,                                                                                            " _
            & "        PRODUCT2NAME,                                                                                        " _
            & "        PRODUCT2,                                                                                            " _
            & "        PRODUCT1NAME,                                                                                        " _
            & "        PRODUCT1,                                                                                            " _
            & "        OILNAME,                                                                                             " _
            & "        OILTYPE,                                                                                             " _
            & "        TODOKECODE,                                                                                          " _
            & "        TODOKENAME,                                                                                          " _
            & "        TODOKENAMES,                                                                                         " _
            & "        TORICODE,                                                                                            " _
            & "        TORINAME,                                                                                            " _
            & "        SHUKABASHO,                                                                                          " _
            & "        SHUKANAME,                                                                                           " _
            & "        SHUKANAMES,                                                                                          " _
            & "        SHUKATORICODE,                                                                                       " _
            & "        SHUKATORINAME,                                                                                       " _
            & "        SHUKADATE,                                                                                           " _
            & "        LOADTIME,                                                                                            " _
            & "        LOADTIMEIN,                                                                                          " _
            & "        TODOKEDATE,                                                                                          " _
            & "        SHITEITIME,                                                                                          " _
            & "        SHITEITIMEIN,                                                                                        " _
            & "        ZYUTYU,                                                                                              " _
            & "        ZISSEKI,                                                                                             " _
            & "        TANNI,                                                                                               " _
            & "        TANKNUM,                                                                                             " _
            & "        TANKNUMBER,                                                                                          " _
            & "        GYOMUTANKNUM,                                                                                        " _
            & "        SYAGATA,                                                                                             " _
            & "        SYABARA,                                                                                             " _
            & "        NINUSHINAME,                                                                                         " _
            & "        CONTYPE,                                                                                             " _
            & "        TRIP,                                                                                                " _
            & "        DRP,                                                                                                 " _
            & "        STAFFSLCT,                                                                                           " _
            & "        STAFFNAME,                                                                                           " _
            & "        STAFFCODE,                                                                                           " _
            & "        SUBSTAFFSLCT,                                                                                        " _
            & "        SUBSTAFFNAME,                                                                                        " _
            & "        SUBSTAFFNUM,                                                                                         " _
            & "        SHUKODATE,                                                                                           " _
            & "        KIKODATE,                                                                                            " _
            & "        TANKA,                                                                                               " _
            & "        JURYORYOKIN,                                                                                         " _
            & "        TSUKORYO,                                                                                            " _
            & "        KYUZITUTANKA,                                                                                        " _
            & "        YUSOUHI,                                                                                             " _
            & "        CALCKBN,                                                                                             " _
            & "        WORKINGDAY,                                                                                          " _
            & "        PUBLICHOLIDAYNAME,                                                                                   " _
            & "        DELFLG,                                                                                              " _
            & "        INITYMD,                                                                                             " _
            & "        INITUSER,                                                                                            " _
            & "        INITTERMID,                                                                                          " _
            & "        INITPGID,                                                                                            " _
            & "        UPDYMD,                                                                                              " _
            & "        UPDUSER,                                                                                             " _
            & "        UPDTERMID,                                                                                           " _
            & "        UPDPGID,                                                                                             " _
            & "        RECEIVEYMD)                                                                                          " _
            & "    SELECT                                                                                                   " _
            & "        ZISSEKI.RECONO             AS RECONO,                                                                " _
            & "        ZISSEKI.LOADUNLOTYPE       AS LOADUNLOTYPE,                                                          " _
            & "        ZISSEKI.STACKINGTYPE       AS STACKINGTYPE,                                                          " _
            & "        ZISSEKI.ORDERORGCODE       AS ORDERORGCODE,                                                          " _
            & "        ZISSEKI.ORDERORGNAME       AS ORDERORGNAME,                                                          " _
            & "        ZISSEKI.KASANAMEORDERORG   AS KASANAMEORDERORG,                                                      " _
            & "        ZISSEKI.KASANCODEORDERORG  AS KASANCODEORDERORG,                                                     " _
            & "        ZISSEKI.ORDERORG           AS ORDERORG,                                                              " _
            & "        ZISSEKI.PRODUCT2NAME       AS PRODUCT2NAME,                                                          " _
            & "        ZISSEKI.PRODUCT2           AS PRODUCT2,                                                              " _
            & "        ZISSEKI.PRODUCT1NAME       AS PRODUCT1NAME,                                                          " _
            & "        ZISSEKI.PRODUCT1           AS PRODUCT1,                                                              " _
            & "        ZISSEKI.OILNAME            AS OILNAME,                                                               " _
            & "        ZISSEKI.OILTYPE            AS OILTYPE,                                                               " _
            & "        ZISSEKI.TODOKECODE         AS TODOKECODE,                                                            " _
            & "        ZISSEKI.TODOKENAME         AS TODOKENAME,                                                            " _
            & "        ZISSEKI.TODOKENAMES        AS TODOKENAMES,                                                           " _
            & "        ZISSEKI.TORICODE           AS TORICODE,                                                              " _
            & "        ZISSEKI.TORINAME           AS TORINAME,                                                              " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKABASHO                                                                              " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "                WHERE                                                                                        " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                          " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                          " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                      " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                           " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                        " _
            & "                AND DELFLG       = '0'                                                                       " _
            & "            )                                                                                                " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                                              " _
            & "        END AS SHUKABASHO,                                                                                   " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                " _
            & "        THEN (SELECT SHUKANAME                                                                               " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                     " _
            & "                WHERE                                                                                        " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                          " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                          " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                      " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                           " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                        " _
            & "                AND DELFLG       = '0'                                                                       " _
            & "            )                                                                                                " _
            & "        ELSE ZISSEKI.SHUKANAME                                                                               " _
            & "        END AS SHUKANAME,                                                                                    " _
            & "        ZISSEKI.SHUKANAMES         AS SHUKANAMES,                                                            " _
            & "        ZISSEKI.SHUKATORICODE      AS SHUKATORICODE,                                                         " _
            & "        ZISSEKI.SHUKATORINAME      AS SHUKATORINAME,                                                         " _
            & "        ZISSEKI.SHUKADATE          AS SHUKADATE,                                                             " _
            & "        ZISSEKI.LOADTIME           AS LOADTIME,                                                              " _
            & "        ZISSEKI.LOADTIMEIN         AS LOADTIMEIN,                                                            " _
            & "        ZISSEKI.TODOKEDATE         AS TODOKEDATE,                                                            " _
            & "        ZISSEKI.SHITEITIME         AS SHITEITIME,                                                            " _
            & "        ZISSEKI.SHITEITIMEIN       AS SHITEITIMEIN,                                                          " _
            & "        ZISSEKI.ZYUTYU             AS ZYUTYU,                                                                " _
            & "        ZISSEKI.ZISSEKI            AS ZISSEKI,                                                               " _
            & "        ZISSEKI.TANNI              AS TANNI,                                                                 " _
            & "        ZISSEKI.TANKNUM            AS TANKNUM,                                                               " _
            & "        ZISSEKI.TANKNUMBER         AS TANKNUMBER,                                                            " _
            & "        ZISSEKI.GYOMUTANKNUM       AS GYOMUTANKNUM,                                                          " _
            & "        ZISSEKI.SYAGATA            AS SYAGATA,                                                               " _
            & "        ZISSEKI.SYABARA            AS SYABARA,                                                               " _
            & "        ZISSEKI.NINUSHINAME        AS NINUSHINAME,                                                           " _
            & "        ZISSEKI.CONTYPE            AS CONTYPE,                                                               " _
            & "        ZISSEKI.TRIP               AS TRIP,                                                                  " _
            & "        ZISSEKI.DRP                AS DRP,                                                                   " _
            & "        ZISSEKI.STAFFSLCT          AS STAFFSLCT,                                                             " _
            & "        ZISSEKI.STAFFNAME          AS STAFFNAME,                                                             " _
            & "        ZISSEKI.STAFFCODE          AS STAFFCODE,                                                             " _
            & "        ZISSEKI.SUBSTAFFSLCT       AS SUBSTAFFSLCT,                                                          " _
            & "        ZISSEKI.SUBSTAFFNAME       AS SUBSTAFFNAME,                                                          " _
            & "        ZISSEKI.SUBSTAFFNUM        AS SUBSTAFFNUM,                                                           " _
            & "        ZISSEKI.SHUKODATE          AS SHUKODATE,                                                             " _
            & "        ZISSEKI.KIKODATE           AS KIKODATE,                                                              " _
            & "        TANKA.TANKA                AS TANKA,                                                                 " _
            & "        NULL                       AS JURYORYOKIN,                                                           " _
            & "        NULL                       AS TSUKORYO,                                                              " _
            & "        HOLIDAYRATE.TANKA          AS KYUZITUTANKA,                                                          " _
            & "        CASE TANKA.CALCKBN                                                                                   " _
            & "          WHEN 'トン' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                           " _
            & "          WHEN '回'   THEN COALESCE(TANKA.TANKA, 0)                                                          " _
            & "          WHEN '距離' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0)                           " _
            & "          WHEN '定数' THEN COALESCE(TANKA.TANKA, 0)                                                          " _
            & "                      ELSE COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                           " _
            & "        END                        AS YUSOUHI,                                                               " _
            & "        TANKA.CALCKBN              AS CALCKBN,                                                               " _
            & "        CALENDAR.WORKINGDAY AS WORKINGDAY,                                                                   " _
            & "        CALENDAR.PUBLICHOLIDAYNAME AS PUBLICHOLIDAYNAME,                                                     " _
            & "        ZISSEKI.DELFLG             AS DELFLG,                                                                " _
            & "        @INITYMD                      AS INITYMD,                                                            " _
            & "        @INITUSER                     AS INITUSER,                                                           " _
            & "        @INITTERMID                   AS INITTERMID,                                                         " _
            & "        @INITPGID                     AS INITPGID,                                                           " _
            & "        NULL                          AS UPDYMD,                                                             " _
            & "        NULL                          AS UPDUSER,                                                            " _
            & "        NULL                          AS UPDTERMID,                                                          " _
            & "        NULL                          AS UPDPGID,                                                            " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                                          " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                         " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                     " _
            & "        ON @TORICODE = TANKA.TORICODE                                                                        " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                             " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                   " _
            & "        AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                     " _
            & "        AND ZISSEKI.GYOMUTANKNUM = TANKA.SHABAN                                                              " _
            & "        AND TANKA.AVOCADOSHUKABASHO = CASE ZISSEKI.SHUKABASHO WHEN '006928'                                  " _
            & "                                           THEN (SELECT SHUKABASHO                                           " _
            & "                                                   FROM LNG.LNT0001_ZISSEKI                                  " _
            & "                                                  WHERE                                                      " _
            & "                                                        TORICODE     = ZISSEKI.TORICODE                      " _
            & "                                                    AND ORDERORG     = ZISSEKI.ORDERORG                      " _
            & "                                                    AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                  " _
            & "                                                    AND TRIP         = ZISSEKI.TRIP -1                       " _
            & "                                                    AND TODOKEDATE   = ZISSEKI.TODOKEDATE                    " _
            & "                                                    AND DELFLG       = '0'                                   " _
            & "                                                )                                                            " _
            & "                                           ELSE ZISSEKI.SHUKABASHO                                           " _
            & "                                      END                                                                    " _
            & "        AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                               " _
            & "        AND TANKA.DELFLG = @DELFLG                                                                           " _
            & "        AND TANKA.BRANCHCODE = ZISSEKI.BRANCHCODE                                                            " _
            & "    LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                  " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                                     " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                                        " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                           " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                           " _
            & "       AND CASE HOLIDAYRATE.ORDERORGCATEGORY                                                                                " _
            & "             WHEN '1' THEN HOLIDAYRATE.ORDERORGCODE = ZISSEKI.ORDERORGCODE                                                  " _
            & "             WHEN '2' THEN HOLIDAYRATE.ORDERORGCODE <> ZISSEKI.ORDERORGCODE                                                 " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE HOLIDAYRATE.SHUKABASHOCATEGORY                                                                              " _
            & "             WHEN '1' THEN HOLIDAYRATE.SHUKABASHO = ZISSEKI.SHUKABASHO                                                      " _
            & "             WHEN '2' THEN HOLIDAYRATE.SHUKABASHO <> ZISSEKI.SHUKABASHO                                                     " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE HOLIDAYRATE.TODOKECATEGORY                                                                                  " _
            & "             WHEN '1' THEN HOLIDAYRATE.TODOKECODE = ZISSEKI.TODOKECODE                                                      " _
            & "             WHEN '2' THEN HOLIDAYRATE.TODOKECODE <> ZISSEKI.TODOKECODE                                                     " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE WHEN HOLIDAYRATE.GYOMUTANKNUMFROM = '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                 " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM <= HOLIDAYRATE.GYOMUTANKNUMTO                                                " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO = ''                                 " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM >= HOLIDAYRATE.GYOMUTANKNUMFROM                                              " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM BETWEEN HOLIDAYRATE.GYOMUTANKNUMFROM AND HOLIDAYRATE.GYOMUTANKNUMTO          " _
            & "                ELSE 1 = 1                                                                                                  " _
            & "           END                                                                                                              " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                  " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                     " _
            & "    WHERE                                                                                                    " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                                         " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                                             " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                                                   " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                  " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                    " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                                         " _
            & " ON DUPLICATE KEY UPDATE                                                                                     " _
            & "         RECONO                    = VALUES(RECONO),                                                         " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                   " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                   " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                   " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                   " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                               " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                              " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                       " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                   " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                       " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                   " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                       " _
            & "         OILNAME                   = VALUES(OILNAME),                                                        " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                        " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                     " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                     " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                    " _
            & "         TORICODE                  = VALUES(TORICODE),                                                       " _
            & "         TORINAME                  = VALUES(TORINAME),                                                       " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                     " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                      " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                     " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                  " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                  " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                      " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                       " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                     " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                     " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                     " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                   " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                         " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                        " _
            & "         TANNI                     = VALUES(TANNI),                                                          " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                        " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                     " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                   " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                        " _
            & "         SYABARA                   = VALUES(SYABARA),                                                        " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                    " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                        " _
            & "         TRIP                      = VALUES(TRIP),                                                           " _
            & "         DRP                       = VALUES(DRP),                                                            " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                      " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                      " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                      " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                   " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                   " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                    " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                      " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                       " _
            & "         TANKA                     = VALUES(TANKA),                                                          " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                    " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                       " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                   " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                        " _
            & "         CALCKBN                   = VALUES(CALCKBN),                                                        " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                     " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                              " _
            & "         DELFLG                    = @DELFLG,                                                                " _
            & "         UPDYMD                    = @UPDYMD,                                                                " _
            & "         UPDUSER                   = @UPDUSER,                                                               " _
            & "         UPDTERMID                 = @UPDTERMID,                                                             " _
            & "         UPDPGID                   = @UPDPGID,                                                               " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                            "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(東北電力輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0018_TOHOKUYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 北海道LNG輸送費テーブル更新
    ''' </summary>
    Private Sub HOKKAIDOLNG_Update(ByVal iTori As String, ByVal iTaishoYm As String)

        Dim WW_DateNow As DateTime = Date.Now

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            ' DataBase接続
            SQLcon.Open()
            'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

            '○ DB更新SQL(北海道LNG輸送費テーブル)
            '期間内、一旦すべて削除
            Dim SQLStr As String =
              " UPDATE LNG.LNT0024_HOKKAIDOLNGYUSOUHI                           " _
            & " SET                                                             " _
            & "     DELFLG      = @DELFLG                                       " _
            & "   , UPDYMD      = @UPDYMD                                       " _
            & "   , UPDUSER     = @UPDUSER                                      " _
            & "   , UPDTERMID   = @UPDTERMID                                    " _
            & "   , UPDPGID     = @UPDPGID                                      " _
            & "   , RECEIVEYMD  = @RECEIVEYMD                                   " _
            & " WHERE                                                           " _
            & "     TORICODE = @TORICODE                                        " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM             " _
            & " AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO               "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(北海道LNG輸送費テーブル)
                    Dim ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar)        '営業所コード
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.DELETE                                      '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0024_HOKKAIDOLNGYUSOUHI UPDATE(DELETE)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

            '○ DB更新SQL(北海道LNG輸送費テーブル)
            SQLStr =
              "    INSERT INTO LNG.LNT0024_HOKKAIDOLNGYUSOUHI(                                                                             " _
            & "        RECONO,                                                                                                             " _
            & "        LOADUNLOTYPE,                                                                                                       " _
            & "        STACKINGTYPE,                                                                                                       " _
            & "        ORDERORGCODE,                                                                                                       " _
            & "        ORDERORGNAME,                                                                                                       " _
            & "        KASANAMEORDERORG,                                                                                                   " _
            & "        KASANCODEORDERORG,                                                                                                  " _
            & "        ORDERORG,                                                                                                           " _
            & "        PRODUCT2NAME,                                                                                                       " _
            & "        PRODUCT2,                                                                                                           " _
            & "        PRODUCT1NAME,                                                                                                       " _
            & "        PRODUCT1,                                                                                                           " _
            & "        OILNAME,                                                                                                            " _
            & "        OILTYPE,                                                                                                            " _
            & "        TODOKECODE,                                                                                                         " _
            & "        TODOKENAME,                                                                                                         " _
            & "        TODOKENAMES,                                                                                                        " _
            & "        TORICODE,                                                                                                           " _
            & "        TORINAME,                                                                                                           " _
            & "        SHUKABASHO,                                                                                                         " _
            & "        SHUKANAME,                                                                                                          " _
            & "        SHUKANAMES,                                                                                                         " _
            & "        SHUKATORICODE,                                                                                                      " _
            & "        SHUKATORINAME,                                                                                                      " _
            & "        SHUKADATE,                                                                                                          " _
            & "        LOADTIME,                                                                                                           " _
            & "        LOADTIMEIN,                                                                                                         " _
            & "        TODOKEDATE,                                                                                                         " _
            & "        SHITEITIME,                                                                                                         " _
            & "        SHITEITIMEIN,                                                                                                       " _
            & "        ZYUTYU,                                                                                                             " _
            & "        ZISSEKI,                                                                                                            " _
            & "        TANNI,                                                                                                              " _
            & "        TANKNUM,                                                                                                            " _
            & "        TANKNUMBER,                                                                                                         " _
            & "        GYOMUTANKNUM,                                                                                                       " _
            & "        SYAGATA,                                                                                                            " _
            & "        SYABARA,                                                                                                            " _
            & "        NINUSHINAME,                                                                                                        " _
            & "        CONTYPE,                                                                                                            " _
            & "        TRIP,                                                                                                               " _
            & "        DRP,                                                                                                                " _
            & "        STAFFSLCT,                                                                                                          " _
            & "        STAFFNAME,                                                                                                          " _
            & "        STAFFCODE,                                                                                                          " _
            & "        SUBSTAFFSLCT,                                                                                                       " _
            & "        SUBSTAFFNAME,                                                                                                       " _
            & "        SUBSTAFFNUM,                                                                                                        " _
            & "        SHUKODATE,                                                                                                          " _
            & "        KIKODATE,                                                                                                           " _
            & "        TANKA,                                                                                                              " _
            & "        JURYORYOKIN,                                                                                                        " _
            & "        TSUKORYO,                                                                                                           " _
            & "        KYUZITUTANKA,                                                                                                       " _
            & "        YUSOUHI,                                                                                                            " _
            & "        CALCKBN,                                                                                                            " _
            & "        WORKINGDAY,                                                                                                         " _
            & "        PUBLICHOLIDAYNAME,                                                                                                  " _
            & "        DELFLG,                                                                                                             " _
            & "        INITYMD,                                                                                                            " _
            & "        INITUSER,                                                                                                           " _
            & "        INITTERMID,                                                                                                         " _
            & "        INITPGID,                                                                                                           " _
            & "        UPDYMD,                                                                                                             " _
            & "        UPDUSER,                                                                                                            " _
            & "        UPDTERMID,                                                                                                          " _
            & "        UPDPGID,                                                                                                            " _
            & "        RECEIVEYMD)                                                                                                         " _
            & "    SELECT                                                                                                                  " _
            & "        ZISSEKI.RECONO                AS RECONO,                                                                            " _
            & "        ZISSEKI.LOADUNLOTYPE          AS LOADUNLOTYPE,                                                                      " _
            & "        ZISSEKI.STACKINGTYPE          AS STACKINGTYPE,                                                                      " _
            & "        ZISSEKI.ORDERORGCODE          AS ORDERORGCODE,                                                                      " _
            & "        ZISSEKI.ORDERORGNAME          AS ORDERORGNAME,                                                                      " _
            & "        ZISSEKI.KASANAMEORDERORG      AS KASANAMEORDERORG,                                                                  " _
            & "        ZISSEKI.KASANCODEORDERORG     AS KASANCODEORDERORG,                                                                 " _
            & "        ZISSEKI.ORDERORG              AS ORDERORG,                                                                          " _
            & "        ZISSEKI.PRODUCT2NAME          AS PRODUCT2NAME,                                                                      " _
            & "        ZISSEKI.PRODUCT2              AS PRODUCT2,                                                                          " _
            & "        ZISSEKI.PRODUCT1NAME          AS PRODUCT1NAME,                                                                      " _
            & "        ZISSEKI.PRODUCT1              AS PRODUCT1,                                                                          " _
            & "        ZISSEKI.OILNAME               AS OILNAME,                                                                           " _
            & "        ZISSEKI.OILTYPE               AS OILTYPE,                                                                           " _
            & "        ZISSEKI.TODOKECODE            AS TODOKECODE,                                                                        " _
            & "        ZISSEKI.TODOKENAME            AS TODOKENAME,                                                                        " _
            & "        ZISSEKI.TODOKENAMES           AS TODOKENAMES,                                                                       " _
            & "        ZISSEKI.TORICODE              AS TORICODE,                                                                          " _
            & "        ZISSEKI.TORINAME              AS TORINAME,                                                                          " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                               " _
            & "        THEN (SELECT SHUKABASHO                                                                                             " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                                    " _
            & "                WHERE                                                                                                       " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                                         " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                                         " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                     " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                                          " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                       " _
            & "                AND DELFLG       = '0'                                                                                      " _
            & "            )                                                                                                               " _
            & "        ELSE ZISSEKI.SHUKABASHO                                                                                             " _
            & "        END AS SHUKABASHO,                                                                                                  " _
            & "        CASE ZISSEKI.SHUKABASHO WHEN '006928'                                                                               " _
            & "        THEN (SELECT SHUKANAME                                                                                              " _
            & "                FROM LNG.LNT0001_ZISSEKI                                                                                    " _
            & "                WHERE                                                                                                       " _
            & "                    TORICODE     = ZISSEKI.TORICODE                                                                         " _
            & "                AND ORDERORG     = ZISSEKI.ORDERORG                                                                         " _
            & "                AND GYOMUTANKNUM = ZISSEKI.GYOMUTANKNUM                                                                     " _
            & "                AND TRIP         = ZISSEKI.TRIP -1                                                                          " _
            & "                AND TODOKEDATE   = ZISSEKI.TODOKEDATE                                                                       " _
            & "                AND DELFLG       = '0'                                                                                      " _
            & "            )                                                                                                               " _
            & "        ELSE ZISSEKI.SHUKANAME                                                                                              " _
            & "        END AS SHUKANAME,                                                                                                   " _
            & "        ZISSEKI.SHUKANAMES            AS SHUKANAMES,                                                                        " _
            & "        ZISSEKI.SHUKATORICODE         AS SHUKATORICODE,                                                                     " _
            & "        ZISSEKI.SHUKATORINAME         AS SHUKATORINAME,                                                                     " _
            & "        ZISSEKI.SHUKADATE             AS SHUKADATE,                                                                         " _
            & "        ZISSEKI.LOADTIME              AS LOADTIME,                                                                          " _
            & "        ZISSEKI.LOADTIMEIN            AS LOADTIMEIN,                                                                        " _
            & "        ZISSEKI.TODOKEDATE            AS TODOKEDATE,                                                                        " _
            & "        ZISSEKI.SHITEITIME            AS SHITEITIME,                                                                        " _
            & "        ZISSEKI.SHITEITIMEIN          AS SHITEITIMEIN,                                                                      " _
            & "        ZISSEKI.ZYUTYU                AS ZYUTYU,                                                                            " _
            & "        ZISSEKI.ZISSEKI               AS ZISSEKI,                                                                           " _
            & "        ZISSEKI.TANNI                 AS TANNI,                                                                             " _
            & "        ZISSEKI.TANKNUM               AS TANKNUM,                                                                           " _
            & "        ZISSEKI.TANKNUMBER            AS TANKNUMBER,                                                                        " _
            & "        ZISSEKI.GYOMUTANKNUM          AS GYOMUTANKNUM,                                                                      " _
            & "        ZISSEKI.SYAGATA               AS SYAGATA,                                                                           " _
            & "        ZISSEKI.SYABARA               AS SYABARA,                                                                           " _
            & "        ZISSEKI.NINUSHINAME           AS NINUSHINAME,                                                                       " _
            & "        ZISSEKI.CONTYPE               AS CONTYPE,                                                                           " _
            & "        ZISSEKI.TRIP                  AS TRIP,                                                                              " _
            & "        ZISSEKI.DRP                   AS DRP,                                                                               " _
            & "        ZISSEKI.STAFFSLCT             AS STAFFSLCT,                                                                         " _
            & "        ZISSEKI.STAFFNAME             AS STAFFNAME,                                                                         " _
            & "        ZISSEKI.STAFFCODE             AS STAFFCODE,                                                                         " _
            & "        ZISSEKI.SUBSTAFFSLCT          AS SUBSTAFFSLCT,                                                                      " _
            & "        ZISSEKI.SUBSTAFFNAME          AS SUBSTAFFNAME,                                                                      " _
            & "        ZISSEKI.SUBSTAFFNUM           AS SUBSTAFFNUM,                                                                       " _
            & "        ZISSEKI.SHUKODATE             AS SHUKODATE,                                                                         " _
            & "        ZISSEKI.KIKODATE              AS KIKODATE,                                                                          " _
            & "        TANKA.TANKA                   AS TANKA,                                                                             " _
            & "        NULL                          AS JURYORYOKIN,                                                                       " _
            & "        NULL                          AS TSUKORYO,                                                                          " _
            & "        HOLIDAYRATE.TANKA             AS KYUZITUTANKA,                                                                      " _
            & "        CASE TANKA.CALCKBN                                                                                                  " _
            & "             WHEN 'トン' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(ZISSEKI.ZISSEKI, 0)                                       " _
            & "             WHEN '回'   THEN COALESCE(TANKA.TANKA, 0)                                                                      " _
            & "             WHEN '距離' THEN COALESCE(TANKA.TANKA, 0) * COALESCE(TANKA.ROUNDTRIP, 0)                                       " _
            & "             WHEN '定数' THEN COALESCE(TANKA.TANKA, 0)                                                                      " _
            & "                         ELSE COALESCE(TANKA.TANKA, 0)                                                                      " _
            & "        END                           AS YUSOUHI,                                                                           " _
            & "        TANKA.CALCKBN                 AS CALCKBN,                                                                           " _
            & "        CALENDAR.WORKINGDAY           AS WORKINGDAY,                                                                        " _
            & "        CALENDAR.PUBLICHOLIDAYNAME    AS PUBLICHOLIDAYNAME,                                                                 " _
            & "        ZISSEKI.DELFLG                AS DELFLG,                                                                            " _
            & "        @INITYMD                      AS INITYMD,                                                                           " _
            & "        @INITUSER                     AS INITUSER,                                                                          " _
            & "        @INITTERMID                   AS INITTERMID,                                                                        " _
            & "        @INITPGID                     AS INITPGID,                                                                          " _
            & "        NULL                          AS UPDYMD,                                                                            " _
            & "        NULL                          AS UPDUSER,                                                                           " _
            & "        NULL                          AS UPDTERMID,                                                                         " _
            & "        NULL                          AS UPDPGID,                                                                           " _
            & "        @RECEIVEYMD                   AS RECEIVEYMD                                                                         " _
            & "    FROM LNG.LNT0001_ZISSEKI ZISSEKI                                                                                        " _
            & "    LEFT JOIN LNG.LNM0006_NEWTANKA TANKA                                                                                    " _
            & "        ON @TORICODE = TANKA.TORICODE                                                                                       " _
            & "        AND ZISSEKI.ORDERORGCODE = TANKA.ORGCODE                                                                            " _
            & "        AND ZISSEKI.KASANCODEORDERORG = TANKA.KASANORGCODE                                                                  " _
            & "        AND ZISSEKI.TODOKECODE = TANKA.AVOCADOTODOKECODE                                                                    " _
            & "        AND CASE WHEN COALESCE(TANKA.SYAGATANAME,'') = ''                                                                   " _
            & "                 THEN TANKA.SYAGATANAME = ''                                                                                " _
            & "                 ELSE TANKA.SYAGATANAME = REPLACE (ZISSEKI.SYAGATA, '単車タンク', '単車')                                   " _
            & "            END                                                                                                             " _
            & "        AND TANKA.STYMD  <= ZISSEKI.TODOKEDATE                                                                              " _
            & "        AND TANKA.ENDYMD >= ZISSEKI.TODOKEDATE                                                                              " _
            & "        AND TANKA.DELFLG = @DELFLG                                                                                          " _
            & "        AND TANKA.BRANCHCODE = ZISSEKI.BRANCHCODE                                                                           " _
            & "     LEFT JOIN LNG.LNM0016_CALENDAR CALENDAR                                                                                " _
            & "        ON @TORICODE = CALENDAR.TORICODE                                                                                    " _
            & "        AND ZISSEKI.TODOKEDATE = CALENDAR.YMD                                                                               " _
            & "        AND CALENDAR.DELFLG = @DELFLG                                                                                       " _
            & "    LEFT JOIN LNG.LNM0017_HOLIDAYRATE HOLIDAYRATE                                                                           " _
            & "       ON ZISSEKI.TORICODE = HOLIDAYRATE.TORICODE                                                                           " _
            & "       AND CASE HOLIDAYRATE.ORDERORGCATEGORY                                                                                " _
            & "             WHEN '1' THEN HOLIDAYRATE.ORDERORGCODE = ZISSEKI.ORDERORGCODE                                                  " _
            & "             WHEN '2' THEN HOLIDAYRATE.ORDERORGCODE <> ZISSEKI.ORDERORGCODE                                                 " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE HOLIDAYRATE.SHUKABASHOCATEGORY                                                                              " _
            & "             WHEN '1' THEN HOLIDAYRATE.SHUKABASHO = ZISSEKI.SHUKABASHO                                                      " _
            & "             WHEN '2' THEN HOLIDAYRATE.SHUKABASHO <> ZISSEKI.SHUKABASHO                                                     " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE HOLIDAYRATE.TODOKECATEGORY                                                                                  " _
            & "             WHEN '1' THEN HOLIDAYRATE.TODOKECODE = ZISSEKI.TODOKECODE                                                      " _
            & "             WHEN '2' THEN HOLIDAYRATE.TODOKECODE <> ZISSEKI.TODOKECODE                                                     " _
            & "             ELSE 1 = 1                                                                                                     " _
            & "           END                                                                                                              " _
            & "       AND CASE WHEN HOLIDAYRATE.GYOMUTANKNUMFROM = '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                 " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM <= HOLIDAYRATE.GYOMUTANKNUMTO                                                " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO = ''                                 " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM >= HOLIDAYRATE.GYOMUTANKNUMFROM                                              " _
            & "                WHEN HOLIDAYRATE.GYOMUTANKNUMFROM <> '' and HOLIDAYRATE.GYOMUTANKNUMTO <> ''                                " _
            & "                     THEN ZISSEKI.GYOMUTANKNUM BETWEEN HOLIDAYRATE.GYOMUTANKNUMFROM AND HOLIDAYRATE.GYOMUTANKNUMTO          " _
            & "                ELSE 1 = 1                                                                                                  " _
            & "           END                                                                                                              " _
            & "       AND HOLIDAYRATE.RANGECODE LIKE CONCAT('%',CALENDAR.WORKINGDAY, '%')                                                  " _
            & "       AND HOLIDAYRATE.DELFLG = @DELFLG                                                                                     " _
            & "    WHERE                                                                                                                   " _
            & "        ZISSEKI.TORICODE = @TORICODE                                                                                        " _
            & "        AND ZISSEKI.ZISSEKI <> 0                                                                                            " _
            & "        AND ZISSEKI.STACKINGTYPE <> '積置'                                                                                  " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') >= @YMDFROM                                                                 " _
            & "        AND date_format(TODOKEDATE, '%Y/%m/%d') <= @YMDTO                                                                   " _
            & "        AND ZISSEKI.DELFLG = @DELFLG                                                                                        " _
            & " ON DUPLICATE KEY UPDATE                                                                                                    " _
            & "         RECONO                    = VALUES(RECONO),                                                                        " _
            & "         LOADUNLOTYPE              = VALUES(LOADUNLOTYPE),                                                                  " _
            & "         STACKINGTYPE              = VALUES(STACKINGTYPE),                                                                  " _
            & "         ORDERORGCODE              = VALUES(ORDERORGCODE),                                                                  " _
            & "         ORDERORGNAME              = VALUES(ORDERORGNAME),                                                                  " _
            & "         KASANAMEORDERORG          = VALUES(KASANAMEORDERORG),                                                              " _
            & "         KASANCODEORDERORG         = VALUES(KASANCODEORDERORG),                                                             " _
            & "         ORDERORG                  = VALUES(ORDERORG),                                                                      " _
            & "         PRODUCT2NAME              = VALUES(PRODUCT2NAME),                                                                  " _
            & "         PRODUCT2                  = VALUES(PRODUCT2),                                                                      " _
            & "         PRODUCT1NAME              = VALUES(PRODUCT1NAME),                                                                  " _
            & "         PRODUCT1                  = VALUES(PRODUCT1),                                                                      " _
            & "         OILNAME                   = VALUES(OILNAME),                                                                       " _
            & "         OILTYPE                   = VALUES(OILTYPE),                                                                       " _
            & "         TODOKECODE                = VALUES(TODOKECODE),                                                                    " _
            & "         TODOKENAME                = VALUES(TODOKENAME),                                                                    " _
            & "         TODOKENAMES               = VALUES(TODOKENAMES),                                                                   " _
            & "         TORICODE                  = VALUES(TORICODE),                                                                      " _
            & "         TORINAME                  = VALUES(TORINAME),                                                                      " _
            & "         SHUKABASHO                = VALUES(SHUKABASHO),                                                                    " _
            & "         SHUKANAME                 = VALUES(SHUKANAME),                                                                     " _
            & "         SHUKANAMES                = VALUES(SHUKANAMES),                                                                    " _
            & "         SHUKATORICODE             = VALUES(SHUKATORICODE),                                                                 " _
            & "         SHUKATORINAME             = VALUES(SHUKATORINAME),                                                                 " _
            & "         SHUKADATE                 = VALUES(SHUKADATE),                                                                     " _
            & "         LOADTIME                  = VALUES(LOADTIME),                                                                      " _
            & "         LOADTIMEIN                = VALUES(LOADTIMEIN),                                                                    " _
            & "         TODOKEDATE                = VALUES(TODOKEDATE),                                                                    " _
            & "         SHITEITIME                = VALUES(SHITEITIME),                                                                    " _
            & "         SHITEITIMEIN              = VALUES(SHITEITIMEIN),                                                                  " _
            & "         ZYUTYU                    = VALUES(ZYUTYU),                                                                        " _
            & "         ZISSEKI                   = VALUES(ZISSEKI),                                                                       " _
            & "         TANNI                     = VALUES(TANNI),                                                                         " _
            & "         TANKNUM                   = VALUES(TANKNUM),                                                                       " _
            & "         TANKNUMBER                = VALUES(TANKNUMBER),                                                                    " _
            & "         GYOMUTANKNUM              = VALUES(GYOMUTANKNUM),                                                                  " _
            & "         SYAGATA                   = VALUES(SYAGATA),                                                                       " _
            & "         SYABARA                   = VALUES(SYABARA),                                                                       " _
            & "         NINUSHINAME               = VALUES(NINUSHINAME),                                                                   " _
            & "         CONTYPE                   = VALUES(CONTYPE),                                                                       " _
            & "         TRIP                      = VALUES(TRIP),                                                                          " _
            & "         DRP                       = VALUES(DRP),                                                                           " _
            & "         STAFFSLCT                 = VALUES(STAFFSLCT),                                                                     " _
            & "         STAFFNAME                 = VALUES(STAFFNAME),                                                                     " _
            & "         STAFFCODE                 = VALUES(STAFFCODE),                                                                     " _
            & "         SUBSTAFFSLCT              = VALUES(SUBSTAFFSLCT),                                                                  " _
            & "         SUBSTAFFNAME              = VALUES(SUBSTAFFNAME),                                                                  " _
            & "         SUBSTAFFNUM               = VALUES(SUBSTAFFNUM),                                                                   " _
            & "         SHUKODATE                 = VALUES(SHUKODATE),                                                                     " _
            & "         KIKODATE                  = VALUES(KIKODATE),                                                                      " _
            & "         TANKA                     = VALUES(TANKA),                                                                         " _
            & "         JURYORYOKIN               = VALUES(JURYORYOKIN),                                                                   " _
            & "         TSUKORYO                  = VALUES(TSUKORYO),                                                                      " _
            & "         KYUZITUTANKA              = VALUES(KYUZITUTANKA),                                                                  " _
            & "         YUSOUHI                   = VALUES(YUSOUHI),                                                                       " _
            & "         CALCKBN                   = VALUES(CALCKBN),                                                                       " _
            & "         WORKINGDAY                = VALUES(WORKINGDAY),                                                                    " _
            & "         PUBLICHOLIDAYNAME         = VALUES(PUBLICHOLIDAYNAME),                                                             " _
            & "         DELFLG                    = @DELFLG,                                                                               " _
            & "         UPDYMD                    = @UPDYMD,                                                                               " _
            & "         UPDUSER                   = @UPDUSER,                                                                              " _
            & "         UPDTERMID                 = @UPDTERMID,                                                                            " _
            & "         UPDPGID                   = @UPDPGID,                                                                              " _
            & "         RECEIVEYMD                = @RECEIVEYMD;                                                                           "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    ' DB更新用パラメータ(北海道LNG輸送費テーブル)
                    Dim TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                '取引先コード
                    Dim YMDFROM As MySqlParameter = SQLcmd.Parameters.Add("@YMDFROM", MySqlDbType.DateTime)                 '年月日FROM
                    Dim YMDTO As MySqlParameter = SQLcmd.Parameters.Add("@YMDTO", MySqlDbType.DateTime)                     '年月日TO
                    Dim DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                 '削除フラグ
                    Dim INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                    Dim INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)            '登録ユーザーＩＤ
                    Dim INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)        '登録端末
                    Dim INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)            '登録プログラムＩＤ
                    Dim UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                    Dim UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)              '更新ユーザーＩＤ
                    Dim UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)          '更新端末
                    Dim UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)              '更新プログラムＩＤ
                    Dim RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                    ' DB更新
                    TORICODE.Value = iTori                                                  '取引先コード
                    DELFLG.Value = C_DELETE_FLG.ALIVE                                       '削除フラグ（削除）
                    If Not String.IsNullOrEmpty(iTaishoYm) AndAlso IsDate(iTaishoYm & "/01") Then
                        YMDFROM.Value = iTaishoYm & "/01"
                        YMDTO.Value = iTaishoYm & DateTime.DaysInMonth(CDate(iTaishoYm).Year, CDate(iTaishoYm).Month).ToString("/00")
                    End If
                    INITYMD.Value = WW_DateNow                                              '登録年月日
                    INITUSER.Value = Master.USERID                                          '登録ユーザーＩＤ
                    INITTERMID.Value = Master.USERTERMID                                    '登録端末
                    INITPGID.Value = Me.GetType().BaseType.Name                             '登録プログラムＩＤ
                    UPDYMD.Value = WW_DateNow                                               '更新年月日
                    UPDUSER.Value = Master.USERID                                           '更新ユーザーＩＤ
                    UPDTERMID.Value = Master.USERTERMID                                     '更新端末
                    UPDPGID.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                    RECEIVEYMD.Value = C_DEFAULT_YMD                                        '集信日時

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0024_HOKKAIDOLNGYUSOUHI UPDATE(INSERT)"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Throw
            End Try

        End Using

    End Sub


End Class
