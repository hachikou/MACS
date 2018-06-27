/// DBTableJson: DBTableとNDJsonを結びつける拡張メソッド.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

namespace MACS.DB {

/// <summary>
///   DBTableとNDJsonを結びつける拡張メソッド
/// </summary>
public static class DBTableJson {

    /// <summary>
    ///   DBTableのQuery結果をNDJsonで取り出す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     得られるJSONは、{"columns":["カラム1","カラム2",...],
    ///     "list":[["値1-1","値1-2",...],["値2-1","値2-2",...],...]} という形式になります。
    ///   </para>
    /// </remarks>
    /// <param name="tbl">対象DBTable</param>
    /// <param name="limit">最大取り出し件数。0の場合無制限</param>
    /// <param name="offset">何件目以降を返すか。先頭レコードは0</param>
    public static NDJson GetJson(this DBTable tbl, int limit=0, int offset=0) {
        NDJson json = new NDJson("columns", tbl.Columns);
        NDJson list = new NDJson();
        using(DBReader reader = tbl.Query(limit,offset)) {
            string[] item;
            while((item = reader.Get()) != null) {
                list.Add(new NDJson(item));
            }
        }
        json.Set("list",list);
        return json;
    }
}

} // End of namespace
