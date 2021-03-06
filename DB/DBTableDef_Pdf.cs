/// DBTableDef_Pdf: データベーステーブル定義.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MACS;

namespace MACS.DB {

/// <summary>
///   データベーステーブル定義
/// </summary>
public partial class DBTableDef {

    public static string FontFile = "fonts/ipag.ttc,0";

    /// <summary>
    ///   テーブル定義をPDF化する。
    /// </summary>
    /// <param name="list">テーブル定義リスト</param>
    /// <param name="filepath">PDFファイルパス</param>
    public static void GeneratePdf(List<DBTableDef> list, string filepath) {
        BaseFont baseFont = BaseFont.CreateFont(FontFile, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        Font lfont = new Font(baseFont, 20, Font.NORMAL);
        Font mfont = new Font(baseFont, 12, Font.NORMAL);
        Font sfont = new Font(baseFont, 9, Font.NORMAL);

        using(Stream fs = FileUtil.BinaryWriter(filepath)) {
            Document doc = new Document(PageSize.A4.Rotate());
            PdfWriter pw = PdfWriter.GetInstance(doc, fs);
            doc.Open();

            foreach(DBTableDef tabledef in list) {

                PdfPTable tbl = new PdfPTable(3);
                string[] header = {"ヘッダ1", "ヘッダ2", "ヘッダ3"};
                for(int i = 0; i < header.Length; i++) {
                    PdfPCell cell = new PdfPCell(new Phrase(header[i], mfont));
                    tbl.AddCell(cell);
                }
                doc.Add(tbl);
            }
            doc.Close();
        }
    }

}

} // End of namespace
