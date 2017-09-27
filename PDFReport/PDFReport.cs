using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MACS;

/// <summary>
///   XML定義に従ってPDF帳票を作成する
/// </summary>
public class PDFReport: Loggable, IDisposable {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public PDFReport(string xmlfilename_) {
        xmlFilename = xmlfilename_;
        if(!File.Exists(xmlFilename))
            throw new IOException(String.Format("File not found ({0})", xmlFilename));
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~PDFReport() {
        Dispose();
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    public void Dispose() {
        end();
    }

    /// <summary>
    ///   フィールド名表示モード
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     このフラグをtrueにすると、フィールド名展開は行なわれない。
    ///     また、強制的に1ページのみ印字する。
    ///   </para>
    /// </remarks>
    public bool ShowFieldName = false;

    /// <summary>
    ///   ページ枠表示モード
    /// </summary>
    public bool ShowPageFrame = false;

    /// <summary>
    ///   強制横書きモード
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Epson LP9600SのドライバがLandscapeにきちんと対応できていないようなので、
    ///     強制的に90度回転したPDFを作成するモードを作った。
    ///   </para>
    /// </remarks>
    /// <summary>
    public bool Rotate = false;

    /// <summary>
    ///   用紙サイズのデバッグフラグ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     認識した用紙とマージンのサイズを標準出力に表示する。
    ///     プリンタドライバの用紙定義などとの齟齬をデバッグする際に使用。
    ///   </para>
    /// </remarks>
    /// <summary>
    public static bool DebugPaperSize = false;

    /// <summary>
    ///   フォントディレクトリ
    /// </summary>
    public static string FontDir = "./fonts";

    /// <summary>
    ///   デフォルトフォントファイル名
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     FontDirからの相対パス指定。
    ///     TTCファイルを指定する場合には、ファイル名の後に",インデックス番号"を
    ///     付けること。
    ///   </para>
    /// </remarks>
    public static string DefaultFontFile = "ipag.ttc,0";

    /// <summary>
    ///   PDFファイル作成
    /// </summary>
    public void Output(string pdffilename, DataArray field_) {
        Output(pdffilename, field_, new List<List<DataArray>>());
    }

    /// <summary>
    ///   PDFファイル作成
    /// </summary>
    public void Output(string pdffilename, string[] field_) {
        Output(pdffilename, new DataArray(field_), new List<List<DataArray>>());
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素付き）
    /// </summary>
    public void Output(string pdffilename, DataArray field_, List<DataArray> list) {
        List<List<DataArray>> listList_ = new List<List<DataArray>>();
        listList_.Add(list);
        Output(pdffilename, field_, listList_);
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素付き）
    /// </summary>
    public void Output(string pdffilename, string[] field_, List<string[]> list) {
        Output(pdffilename, new DataArray(field_), DataArray.Convert(list));
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素2つ付き）
    /// </summary>
    public void Output(string pdffilename, DataArray field_, List<DataArray> list1, List<DataArray>list2) {
        List<List<DataArray>> listList_ = new List<List<DataArray>>();
        listList_.Add(list1);
        listList_.Add(list2);
        Output(pdffilename, field_, listList_);
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素2つ付き）
    /// </summary>
    public void Output(string pdffilename, string[] field_, List<string[]> list1, List<string[]>list2) {
        Output(pdffilename, new DataArray(field_), DataArray.Convert(list1), DataArray.Convert(list2));
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素3つ付き）
    /// </summary>
    public void Output(string pdffilename, DataArray field_, List<DataArray> list1, List<DataArray>list2, List<DataArray>list3) {
        List<List<DataArray>> listList_ = new List<List<DataArray>>();
        listList_.Add(list1);
        listList_.Add(list2);
        listList_.Add(list3);
        Output(pdffilename, field_, listList_);
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素3つ付き）
    /// </summary>
    public void Output(string pdffilename, string[] field_, List<string[]> list1, List<string[]>list2, List<string[]>list3) {
        Output(pdffilename, new DataArray(field_), DataArray.Convert(list1), DataArray.Convert(list2), DataArray.Convert(list3));
    }

    /// <summary>
    ///   PDFファイル作成（任意個数のリスト要素付き）
    /// </summary>
    public void Output(string pdffilename, DataArray field_, List<List<DataArray>> listList_) {
        using(FileStream fs = FileUtil.BinaryWriter(pdffilename)) {
            if(fs == null)
                throw new IOException(String.Format("Can't open for writing. ({0})", pdffilename));
            Output(fs, field_, listList_);
            fs.Close();
        }
    }

    /// <summary>
    ///   PDFファイル作成（任意個数のリスト要素付き）
    /// </summary>
    public void Output(string pdffilename, string[] field_, List<List<string[]>> listList_) {
        List<List<DataArray>> ll = new List<List<DataArray>>();
        foreach(List<string[]> list in listList_)
            ll.Add(DataArray.Convert(list));
        Output(pdffilename, new DataArray(field_), ll);
    }

    /// <summary>
    ///   PDFファイル作成
    /// </summary>
    public void Output(Stream fs, DataArray field_) {
        Output(fs, field_, new List<List<DataArray>>());
    }

    /// <summary>
    ///   PDFファイル作成
    /// </summary>
    public void Output(Stream fs, string[] field_) {
        Output(fs, new DataArray(field_), new List<List<DataArray>>());
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素付き）
    /// </summary>
    public void Output(Stream fs, DataArray field_, List<DataArray> list) {
        List<List<DataArray>> listList_ = new List<List<DataArray>>();
        listList_.Add(list);
        Output(fs, field_, listList_);
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素付き）
    /// </summary>
    public void Output(Stream fs, string[] field_, List<string[]> list) {
        Output(fs, new DataArray(field_), DataArray.Convert(list));
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素2つ付き）
    /// </summary>
    public void Output(Stream fs, DataArray field_, List<DataArray> list1, List<DataArray>list2) {
        List<List<DataArray>> listList_ = new List<List<DataArray>>();
        listList_.Add(list1);
        listList_.Add(list2);
        Output(fs, field_, listList_);
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素2つ付き）
    /// </summary>
    public void Output(Stream fs, string[] field_, List<string[]> list1, List<string[]>list2) {
        Output(fs, new DataArray(field_), DataArray.Convert(list1), DataArray.Convert(list2));
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素3つ付き）
    /// </summary>
    public void Output(Stream fs, DataArray field_, List<DataArray> list1, List<DataArray>list2, List<DataArray>list3) {
        List<List<DataArray>> listList_ = new List<List<DataArray>>();
        listList_.Add(list1);
        listList_.Add(list2);
        listList_.Add(list3);
        Output(fs, field_, listList_);
    }

    /// <summary>
    ///   PDFファイル作成（リスト要素3つ付き）
    /// </summary>
    public void Output(Stream fs, string[] field_, List<string[]> list1, List<string[]>list2, List<string[]>list3) {
        Output(fs, new DataArray(field_), DataArray.Convert(list1), DataArray.Convert(list2), DataArray.Convert(list3));
    }

    /// <summary>
    ///   PDFファイル作成（任意個数のリスト要素付き）
    /// </summary>
    public void Output(Stream fs, DataArray field_, List<List<DataArray>> listList_) {
        field = field_;
        listList = listList_;
        using(XmlFile xml = new XmlFile(xmlFilename, "document")) {
            begin(xml, fs);
            generate(xml);
            end();
        }
    }

    /// <summary>
    ///   PDFファイル作成（任意個数のリスト要素付き）
    /// </summary>
    public void Output(Stream fs, string[] field_, List<List<string[]>> listList_) {
        List<List<DataArray>> ll = new List<List<DataArray>>();
        foreach(List<string[]> list in listList_)
            ll.Add(DataArray.Convert(list));
        Output(fs, new DataArray(field_), ll);
    }

    /// <summary>
    ///   PDFファイル作成開始
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    /// <example>
    ///   using(PDFReport pdf = new PDFReport(xmlfilename)) {
    ///       using(FileStream fs = FileUtil.BinaryWriter(pdffilename)) {
    ///           pdf.Begin(fs);
    ///           pdf.Output(fieldset1);
    ///           pdf.Output(fieldset2);
    ///           pdf.Output(fieldset3);
    ///           pdf.End();
    ///       }
    ///   }
    /// </example>
    public void Begin(Stream fs) {
        using(XmlFile xml = new XmlFile(xmlFilename, "document")) {
            begin(xml, fs);
        }
    }

    /// <summary>
    ///   PDFファイル作成終了
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void End() {
        end();
    }
    /// <summary>
    ///   PDF出力
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void Output(DataArray field_) {
        Output(field_, new List<List<DataArray>>());
    }

    /// <summary>
    ///   PDF出力
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void Output(string[] field_) {
        Output(new DataArray(field_), new List<List<DataArray>>());
    }

    /// <summary>
    ///   PDF出力（リスト要素付き）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void Output(DataArray field_, List<DataArray> list) {
        List<List<DataArray>> listList_ = new List<List<DataArray>>();
        listList_.Add(list);
        Output(field_, listList_);
    }

    /// <summary>
    ///   PDF出力（リスト要素付き）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void Output(string[] field_, List<string[]> list) {
        Output(new DataArray(field_), DataArray.Convert(list));
    }

    /// <summary>
    ///   PDF出力（リスト要素2つ付き）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void Output(DataArray field_, List<DataArray> list1, List<DataArray>list2) {
        List<List<DataArray>> listList_ = new List<List<DataArray>>();
        listList_.Add(list1);
        listList_.Add(list2);
        Output(field_, listList_);
    }

    /// <summary>
    ///   PDF出力（リスト要素2つ付き）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void Output(string[] field_, List<string[]> list1, List<string[]>list2) {
        Output(new DataArray(field_), DataArray.Convert(list1), DataArray.Convert(list2));
    }

    /// <summary>
    ///   PDF出力（リスト要素3つ付き）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void Output(DataArray field_, List<DataArray> list1, List<DataArray>list2, List<DataArray>list3) {
        List<List<DataArray>> listList_ = new List<List<DataArray>>();
        listList_.Add(list1);
        listList_.Add(list2);
        listList_.Add(list3);
        Output(field_, listList_);
    }

    /// <summary>
    ///   PDF出力（リスト要素3つ付き）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void Output(string[] field_, List<string[]> list1, List<string[]>list2, List<string[]>list3) {
        Output(new DataArray(field_), DataArray.Convert(list1), DataArray.Convert(list2), DataArray.Convert(list3));
    }

    /// <summary>
    ///   PDF出力（任意個数のリスト要素付き）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void Output(DataArray field_, List<List<DataArray>> listList_) {
        field = field_;
        listList = listList_;
        using(XmlFile xml = new XmlFile(xmlFilename, "document")) {
            generate(xml);
        }
    }

    /// <summary>
    ///   PDF出力（任意個数のリスト要素付き）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一つの文書に何度もOutputを行なう時に使用する。
    ///   </para>
    /// </remarks>
    public void Output(string[] field_, List<List<string[]>> listList_) {
        List<List<DataArray>> ll = new List<List<DataArray>>();
        foreach(List<string[]> list in listList_)
            ll.Add(DataArray.Convert(list));
        Output(new DataArray(field_), ll);
    }


    /// <summary>
    ///   複数のPDFファイルを結合して1つのPDFファイルを作成する
    /// </summary>
    /// <param name="outfilename">作成するPDFファイル名</param>
    /// <param name="infilenames">元のPDFファイル名（任意個数指定可）</param>
    /// <remarks>
    ///   <para>
    ///     元のPDFファイルを1つも指定しない場合はエラー(ArgumentException)
    ///   </para>
    /// </remarks>
    public static void Combine(string outfilename, params string[] infilenames) {
        if(String.IsNullOrEmpty(outfilename))
            throw new ArgumentException("Output PDF file name is null or empty.");
        if((infilenames == null) || (infilenames.Length <= 0))
            throw new ArgumentException("Input PDF file is not specified.");
        try {
            using(FileStream fs = FileUtil.BinaryWriter(outfilename)) {
                PdfCopyFields copy = new PdfCopyFields(fs);
                foreach(string infilename in infilenames) {
                    if(String.IsNullOrEmpty(infilename))
                        throw new ArgumentException("Input PDF file name is null or empty.");
                    if(!File.Exists(infilename))
                        throw new IOException(String.Format("Input PDF file ({0}) does not exist.", infilename));
                    PdfReader reader = new PdfReader(infilename);
                    copy.AddDocument(reader);
                }
                copy.Close();
            }
        } catch(Exception e) {
            File.Delete(outfilename);
            throw e;
        }
    }


    private static readonly string[] basicFonts = new string[]{
        BaseFont.COURIER,
        BaseFont.COURIER_BOLD,
        BaseFont.COURIER_BOLDOBLIQUE,
        BaseFont.COURIER_OBLIQUE,
        BaseFont.HELVETICA,
        BaseFont.HELVETICA_BOLD,
        BaseFont.HELVETICA_BOLDOBLIQUE,
        BaseFont.HELVETICA_OBLIQUE,
        BaseFont.TIMES_ROMAN,
        BaseFont.TIMES_BOLD,
        BaseFont.TIMES_BOLDITALIC,
        BaseFont.TIMES_ITALIC,
    };

    /// <summary>
    ///   フォントファイルを指定しベースフォントを得る
    /// </summary>
    private static BaseFont GetBaseFont(string fontfile) {
        if(String.IsNullOrEmpty(fontfile))
            fontfile = DefaultFontFile;
        try {
            string ff = fontfile.ToLower();
            foreach(string f in basicFonts) {
                if(ff == f.ToLower())
                    return BaseFont.CreateFont(f, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            }
            return BaseFont.CreateFont(Path.Combine(FontDir, fontfile),
                                       BaseFont.IDENTITY_H,
                                       BaseFont.EMBEDDED);
        } catch(Exception e) {
            Console.WriteLine(e.Message);
            return BaseFont.CreateFont(Path.Combine(FontDir, DefaultFontFile),
                                       BaseFont.IDENTITY_H,
                                       BaseFont.EMBEDDED);
        }
    }


    //
    // 以下プライベートインプリメンテーション
    //

    private string xmlFilename;
    private Document doc;
    private PdfWriter writer;
    private BaseFont baseFont;
    private Dictionary<string,FontParam> fontDict;
    private int pageNumber;
    private int totalPages;
    private DataArray field;
    private List<List<DataArray>> listList;
    private List<int> listRows;


    private void calcPages(XmlFile xml) {
        // ドキュメント内のリストフィールドの行数を数える。
        // 注: tableタグにrows属性が付いていないものは数えられない。
        listRows = new List<int>();
        foreach(List<DataArray> list in listList)
            listRows.Add(0);
        foreach(XmlNode node in xml.Document.GetElementsByTagName("table")){
            XmlElement elem = (XmlElement)node;
            int listNumber = StringUtil.ToInt(elem.GetAttribute("list"), 1);
            int rows = StringUtil.ToInt(elem.GetAttribute("rows"))+StringUtil.ToInt(elem.GetAttribute("rowoffset"), 1)-1;
            if((listNumber <= 0) || (listNumber > listRows.Count))
                continue;
            if(listRows[listNumber-1] < rows)
                listRows[listNumber-1] = rows;
        }
        // 最大ページ数を探す
        totalPages = 1;
        for(int i = 0; i < listRows.Count; i++) {
            if(listList[i] == null)
                continue;
            if(listRows[i] <= 0)
                continue;
            int n = (listList[i].Count+listRows[i]-1)/listRows[i];
            if(n > totalPages)
                totalPages = n;
        }
    }

    private void begin(XmlFile xml, Stream fs) {
        if(doc != null)
            throw new InvalidOperationException("PDF document is already open.");

        // デフォルトフォントのセットアップ
        baseFont = GetBaseFont(DefaultFontFile);
        fontDict = new Dictionary<string,FontParam>();
        fontDict.Add("default", new FontParam(baseFont, 12.0f));
        //BaseFont.AddToResourceSearch("iTextAsian.dll");
        //Font font = new Font(BaseFont.CreateFont("HeiseiMin-W3", "UniJIS-UCS2-HW-H",false),20);

        // ドキュメント属性
        string paper = xml.Root.GetAttribute("paper");
        if(String.IsNullOrEmpty(paper))
            paper = "A4";
        bool isLandscape = ("landscape" == xml.Root.GetAttribute("orientation"));
        Rectangle paperrect = getPageSize(paper);
        float margin = getSize(xml.Root.GetAttribute("margin"), 36.0f);
        float margint = getSize(xml.Root.GetAttribute("margin-top"), margin);
        float marginr = getSize(xml.Root.GetAttribute("margin-right"), margin);
        float marginb = getSize(xml.Root.GetAttribute("margin-bottom"), margin);
        float marginl = getSize(xml.Root.GetAttribute("margin-left"), margin);
        if(Rotate) {
            isLandscape = !isLandscape;
            float tmp = margint;
            margint = marginr;
            marginr = marginb;
            marginb = marginl;
            marginl = tmp;
        }
        if(isLandscape)
            paperrect = paperrect.Rotate();

        // ドキュメントオブジェクト作成
        doc = new Document(paperrect, marginl, marginr, margint, marginb);
        if(DebugPaperSize) {
            Console.WriteLine("PAPER: [{0},{1}]-[{2},{3}]", paperrect.Left, paperrect.Top, paperrect.Right, paperrect.Bottom);
            Console.WriteLine("MARGIN: Left={0}, Top={1}, Right={2}, Bottom={3}",
                              doc.LeftMargin, doc.TopMargin, doc.RightMargin, doc.BottomMargin);
            Console.WriteLine("REGION: [{0},{1}]-[{2},{3}]",
                              doc.Left, doc.Top, doc.Right, doc.Bottom);
        }
        writer = PdfWriter.GetInstance(doc, fs);
        doc.Open();
    }


    private void generate(XmlFile xml) {
        XmlElement elem;

        if(doc == null)
            throw new InvalidOperationException("PDF document has not been opened.");

        // フォント定義
        if((elem = xml.GetElement(xml.Root, "basefont")) != null) {
            baseFont = GetBaseFont(elem.GetAttribute("file"));
        }
        foreach(XmlElement el in xml.GetElements(xml.Root, "font")) {
            addFont(el);
        }

        // ページ数計算
        calcPages(xml);
        if(ShowFieldName)
            totalPages = 1;
        if(ShowPageFrame)
            xml.Root.SetAttribute("border", "0.1");

        pageNumber = 1;
        while(pageNumber <= totalPages) {
            doc.NewPage();
            writer.NewPage();
            Context ctx = new Context(this);

            //ctx.test();
            ctx.show(xml.Root);
            pageNumber++;
        }
    }

    private void end() {
        if(doc != null) {
            try {
                doc.Close();
            } catch(Exception) {
                // just ignore
            }
        }
        doc = null;
    }

    private void addFont(XmlElement elem) {
        string key = elem.GetAttribute("name");
        if(String.IsNullOrEmpty(key))
            key = "default";
        if(fontDict.ContainsKey(key))
            fontDict[key] = new FontParam(elem, baseFont);
        else
            fontDict.Add(key, new FontParam(elem, baseFont));
    }

    private static Dictionary<string,string> pageSizeList;

    private static Rectangle getPageSize(string txt) {
        if(pageSizeList == null) {
            pageSizeList = new Dictionary<string,string>();
            pageSizeList.Add("B5", "516,729");
        }
        if(String.IsNullOrEmpty(txt))
            return PageSize.A4;
        foreach(string sz in pageSizeList.Keys) {
            if(sz == txt) {
                txt = pageSizeList[sz];
                break;
            }
        }
        string[] s = txt.Split(" ,".ToCharArray());
        if(s.Length == 2) {
            return new Rectangle(getSize(s[0], PageSize.A4.Right-PageSize.A4.Left),
                                 getSize(s[1], PageSize.A4.Top-PageSize.A4.Bottom));
        }
        return PageSize.GetRectangle(txt);
    }

    private static readonly Regex pat_size = new Regex(@"^([0-9.]+)(\D*)$");

    private static float getSize(string txt, float def) {
        if(String.IsNullOrEmpty(txt))
            return def;
        GroupCollection result = pat_size.Match(txt).Groups;
        if(!result[0].Success)
            return def;
        float val = StringUtil.ToFloat(result[1].Value);
        if(result[2].Value.Length > 0) {
            switch(result[2].Value[0]) {
            case 'm': // mm
                val *= 72.0f/25.4f;
                break;
            }
        }
        return val;
    }

    private string extractField(string txt, int currentList, int currentLine) {
        if(txt == null)
            return "";
        FieldReplacer replacer = new FieldReplacer(this, currentList, currentLine);
        return pat_field.Replace(txt, new MatchEvaluator(replacer.Replace));
    }

    private static readonly Regex pat_field = new Regex(@"\{([^{}]+)\}");

    private class FieldReplacer {
        public FieldReplacer(PDFReport report_) {
            report = report_;
            currentList = 0;
            currentLine = 0;
        }

        public FieldReplacer(PDFReport report_, int currentList_, int currentLine_) {
            report = report_;
            currentList = currentList_;
            currentLine = currentLine_;
        }

        public string Replace(Match m) {
            string f = m.Groups[1].Value;
            string fieldname = f;
            string fieldopt = "";
            int idx = f.IndexOf(':');
            if(idx >= 0) {
                fieldname = f.Substring(0, idx);
                fieldopt = f.Substring(idx+1);
            }
            if((report.field != null) && report.field.Contains(fieldname))
                return applyOption(report.field[fieldname], fieldopt);
            if(fieldname.StartsWith("field")) {
                int n = StringUtil.ToInt(fieldname.Substring(5));
                if((report.field == null) || (n <= 0) || (n > report.field.Length))
                    return "";
                return applyOption(report.field[n-1], fieldopt);
            }
            if(fieldname.StartsWith("list")) {
                string[] subnumbers = fieldname.Substring(4).Trim("-.".ToCharArray()).Split("-.".ToCharArray());
                int listnum, linenum;
                string columnname;
                switch(subnumbers.Length) {
                case 3:
                    // リスト要素直接指定
                    listnum = StringUtil.ToInt(subnumbers[0]);
                    linenum = StringUtil.ToInt(subnumbers[1]);
                    columnname = subnumbers[2];
                    break;
                case 2:
                    // リスト番号とカラム指定
                    listnum = StringUtil.ToInt(subnumbers[0]);
                    linenum = currentLine;
                    columnname = subnumbers[1];
                    break;
                case 1:
                    // カラムのみ指定
                    listnum = currentList;
                    linenum = currentLine;
                    columnname = subnumbers[0];
                    break;
                default:
                    return ""; // invalid format
                }
                if((report.listList == null) || (listnum <= 0) || (listnum > report.listList.Count))
                    return ""; // no such list
                List<DataArray> list = report.listList[listnum-1];
                int n = linenum+(report.pageNumber-1)*report.listRows[listnum-1];
                if((list == null) || (n <= 0) || (n > list.Count))
                    return ""; // no such line
                DataArray values = list[n-1];
                if(values == null)
                    return ""; // empty data
                if(values.Contains(columnname))
                    return applyOption(values[columnname], fieldopt);
                int columnnum = StringUtil.ToInt(columnname);
                if((columnnum <= 0) || (columnnum > values.Length))
                    return ""; // no such column
                return applyOption(values[columnnum-1], fieldopt);
            }
            switch(fieldname){
            case "page":
                return applyOption(report.pageNumber.ToString(), fieldopt);
            case "totalpages":
                return applyOption(report.totalPages.ToString(), fieldopt);
            case "currentdate":
                return applyOption(DateTime.Now.ToString("yyyy/MM/dd"), fieldopt);
            case "currenttime":
                return applyOption(DateTime.Now.ToString("HH:mm:ss"), fieldopt);
            case "firstpage":
                return (report.pageNumber == 1)?"yes":"no";
            case "lastpage":
                return (report.pageNumber == report.totalPages)?"yes":"no";
            case "lastline":
                if((report.listList == null) || (currentList <= 0) || (currentList > report.listList.Count)) {
                    return "no";
                } else {
                    List<DataArray> list = report.listList[currentList-1];
                    if(list == null)
                        return "no";
                    return (currentLine == list.Count)?"yes":"no";
                }
            }
            return m.Value;
        }


        private PDFReport report;
        private int currentList;
        private int currentLine;

        private static string applyOption(string txt, string opt) {
            if(txt == null)
                return null;
            if(txt == "")
                return "";
            foreach(string o in opt.Split(":".ToCharArray())) {
                if(o == "")
                    continue;
                string oname = o;
                string[] arg = null;
                int idx = o.IndexOf('(');
                if((idx >= 0) && (o[o.Length-1] == ')')) {
                    oname = o.Substring(0, idx);
                    arg = o.Substring(idx+1,o.Length-idx-2).Split(",".ToCharArray());
                    for(int i = 0; i < arg.Length; i++)
                        arg[i] = arg[i].Trim();
                }
                oname = oname.Trim().ToLower();
                switch(oname) {
                case "number": // 数値表記指定
                    if((arg != null) && (arg.Length >= 1)) {
                        // 小数点下桁数指定
                        txt = StringUtil.ToDouble(txt).ToString("F"+StringUtil.ToInt(arg[0]).ToString());
                    } else {
                        // 整数指定
                        txt = StringUtil.ToDouble(txt).ToString("F0");
                    }
                    break;
                case "decimal": // 位取り表記指定
                    if((arg != null) && (arg.Length >= 1)) {
                        // 小数点下桁数指定
                        txt = StringUtil.DecimalString(txt, StringUtil.ToInt(arg[0]));
                    } else {
                        // 整数指定
                        txt = StringUtil.DecimalString(txt, 0);
                    }
                    break;
                case "fixlength": // 固定長表記
                    {
                        int len = 5;
                        char fill = '0';
                        if(arg != null) {
                            if(arg.Length >= 1)
                                len = StringUtil.ToInt(arg[0],len);
                            if((arg.Length >= 2) && (arg[1].Length >= 1))
                                fill = arg[1][0];
                        }
                        txt = StringUtil.FixLength(txt, len, fill);
                    }
                    break;
                case "plusonly": // 正の数値のみ
                    if(StringUtil.ToDouble(txt, -1.0) < 0.0)
                        txt = "0";
                    break;
                case "prefix": // 前置文字列
                    if((arg != null) && (arg.Length >= 1))
                        txt = arg[0]+txt;
                    break;
                case "postfix": // 前置文字列
                    if((arg != null) && (arg.Length >= 1))
                        txt = txt+arg[0];
                    break;
                }
            }
            return txt;
        }
    }


    private class Context {

        public Context(PDFReport report_) {
            report = report_;
            if(report.Rotate) {
                offsetX = report.doc.BottomMargin;
                offsetY = report.doc.Left;
                width = report.doc.Top-report.doc.Bottom;
                height = report.doc.Right-report.doc.Left;
            } else {
                offsetX = report.doc.Left;
                offsetY = 0f;
                width = report.doc.Right-report.doc.Left;
                height = report.doc.Top-report.doc.Bottom;
            }
            init();
        }

        public Context(PDFReport report_, float x_, float y_, float w_, float h_) {
            report = report_;
            offsetX = x_;
            offsetY = y_;
            width = w_;
            height = h_;
            init();
        }

        public void show(XmlElement elem) {
            showPageBox(elem);
            foreach(XmlNode node in elem.ChildNodes) {
                if(node.NodeType != XmlNodeType.Element)
                    continue;
                XmlElement el = (XmlElement)node;
                selectFont(el);
                x = paramX(el.GetAttribute("x"), x);
                y = paramY(el.GetAttribute("y"), y);
                string cond = el.GetAttribute("cond");
                if(!String.IsNullOrEmpty(cond)) {
                    cond = report.extractField(cond, currentList, currentLine);
                    if((cond == "") || (cond == "0") || (cond == "false") || (cond == "no"))
                        continue;
                }
                switch(el.Name) {
                case "text":
                    showText(el, XmlFile.GetText((XmlElement)node));
                    break;
                case "box":
                    showBox(el);
                    break;
                case "table":
                    showTable(el);
                    break;
                case "barcode":
                    showBarcode(el, XmlFile.GetText((XmlElement)node));
                    break;
                }
            }
        }

        public void showPageBox(XmlElement elem) {
            showBox(elem, 0, 0, width, height);
        }

        public void showText(XmlElement elem, string txt) {
            float w = paramX(elem.GetAttribute("width"), 0f);
            float xx = x;
            int align = PdfContentByte.ALIGN_LEFT;
            string val = elem.GetAttribute("align");
            if(val != null){
                switch(val) {
                case "right":
                    align = PdfContentByte.ALIGN_RIGHT;
                    xx = x-w;
                    break;
                case "center":
                    align = PdfContentByte.ALIGN_CENTER;
                    xx = x-w/2f;
                    break;
                default: // includes "left"
                    align = PdfContentByte.ALIGN_LEFT;
                    break;
                }
            }
            float dy = 0f;
            val = elem.GetAttribute("vertical-align");
            if(val != null){
                switch(val) {
                case "bottom":
                    dy = descent;
                    break;
                case "center":
                    dy = cy/2f+descent;
                    break;
                default: // includes "top":
                    dy = cy+descent;
                    break;
                }
            }
            txt = report.extractField(txt, currentList, currentLine);
            cb.SaveState();
            string clipflag = elem.GetAttribute("clip");
            if(String.IsNullOrEmpty(clipflag))
                clipflag = "char";
            if((clipflag == "char") && (w > 0.0)) {
                // clip by character
                while((txt.Length > 0) && (getTextWidth(txt) > w)) {
                    txt = txt.Substring(0, txt.Length-1);
                }
            }
            if((clipflag == "shrink") && (w > 0.0)) {
                // down font size
                float fontsize = font.Size;
                while((getTextWidth(txt, fontsize) > w) && (fontsize > 5.0)) {
                    fontsize -= 1.0f;
                    cb.SetFontAndSize(font.BaseFont, fontsize);
                }
            }
            if((clipflag == "yes") && (w > 0.0) && (cy > 0.0))
                clipRect(_x(xx), _y(y), w, cy);
            cb.BeginText();
            //cb.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL);
            showTextAligned(align, txt, _x(x), _y(y+dy), 0);
            cb.EndText();
            cb.RestoreState();
            showBox(elem, xx, y, w, cy);
        }

        public void showBox(XmlElement elem) {
            float w = paramX(elem.GetAttribute("width"), 0f);
            float h = paramY(elem.GetAttribute("height"), 0f);
            float orgx, orgy;
            alignPosition(elem, w, h, out orgx, out orgy);
            Context ctx = new Context(report, offsetX+x, offsetY+y, w, h);
            ctx.show(elem);
            x = orgx;
            y = orgy;
        }

        public void showTable(XmlElement elem) {
            float w = paramX(elem.GetAttribute("width"), width);
            float h = paramY(elem.GetAttribute("height"), 0f);
            int rows = StringUtil.ToInt(elem.GetAttribute("rows"), 0);
            currentList = StringUtil.ToInt(elem.GetAttribute("list"), 1);
            int rowOffset = StringUtil.ToInt(elem.GetAttribute("rowoffset"), 1);
            XmlElement header = null;
            XmlElement body = null;
            XmlElement footer = null;
            foreach(XmlNode node in elem.ChildNodes) {
                if(node.NodeType != XmlNodeType.Element)
                    continue;
                switch(node.Name) {
                case "header":
                    header = (XmlElement)node;
                    break;
                case "body":
                    body = (XmlElement)node;
                    break;
                case "footer":
                    footer = (XmlElement)node;
                    break;
                }
            }
            float headerH;
            if(header != null)
                headerH = paramY(header.GetAttribute("height"), cy);
            else
                headerH = 0f;
            float bodyH;
            if(body != null)
                bodyH = paramY(body.GetAttribute("height"), cy);
            else
                bodyH = 0f;
            float footerH;
            if(footer != null)
                footerH = paramY(footer.GetAttribute("height"), cy);
            else
                footerH = 0f;
            
            if(h <= 0f) {
                if(rows <= 0)
                    h = height;
                else
                    h = headerH+bodyH*rows+footerH;
            }
            float orgx, orgy;
            alignPosition(elem, w, h, out orgx, out orgy);
            showBox(elem, x, y, w, h);
            if((rows <= 0) && (bodyH > 0f))
                rows = (int)((h-headerH-footerH)/bodyH);
            if(header != null) {
                Context ctx = new Context(report, offsetX+x, offsetY+y, w, headerH);
                ctx.currentList = currentList;
                ctx.currentLine = 1;
                ctx.autoWidth(header);
                ctx.show(header);
            }
            if(body != null) {
                string val = body.GetAttribute("omitnodata");
                bool omitnodata = (!String.IsNullOrEmpty(val) && (val == "yes"));
                for(int i = 0; i < rows; i++) {
                    // omitnodataオプション指定時は、データが存在する時だけ印字する
                    if(omitnodata) {
                        if((report.listList == null) || (currentList <= 0) || (currentList > report.listList.Count))
                            continue;
                        List<DataArray> list = report.listList[currentList-1];
                        int n = i+rowOffset+(report.pageNumber-1)*report.listRows[currentList-1];
                        if((list == null) || (n <= 0) || (n > list.Count))
                            continue;
                    }
                    Context ctx = new Context(report, offsetX+x, offsetY+y+headerH+bodyH*i, w, bodyH);
                    ctx.currentList = currentList;
                    ctx.currentLine = i+rowOffset;
                    ctx.autoWidth(body);
                    ctx.show(body);
                }
            }
            if(footer != null) {
                Context ctx = new Context(report, offsetX+x, offsetY+y+headerH+bodyH*rows, w, footerH);
                ctx.currentList = currentList;
                ctx.currentLine = 1;
                ctx.autoWidth(footer);
                ctx.show(footer);
            }
            currentList = 0;
            x = orgx;
            y = orgy;
        }

        public void showBarcode(XmlElement elem, string code) {
            code = report.extractField(code, currentList, currentLine);
            if(String.IsNullOrEmpty(code))
                return;
            Barcode bc;
            string codeType = elem.GetAttribute("type");
            if(String.IsNullOrEmpty(codeType))
                codeType = "EAN13";
            switch(codeType.ToUpper()) {
            case "EAN13":
            case "EAN":
            case "JAN13":
            case "JAN":
                bc = new BarcodeEAN();
                bc.CodeType = BarcodeEAN.EAN13;
                break;
            case "EAN8":
            case "JAN8":
                bc = new BarcodeEAN();
                bc.CodeType = BarcodeEAN.EAN8;
                break;
            case "UPCA":
                bc = new BarcodeEAN();
                bc.CodeType = BarcodeEAN.UPCA;
                break;
            case "UPCE":
                bc = new BarcodeEAN();
                bc.CodeType = BarcodeEAN.UPCE;
                break;
            case "ITF14":
            case "ITF":
                bc = new BarcodeInter25();
                break;
            case "CODE39":
                bc = new Barcode39();
                break;
            case "CODABAR":
                bc = new BarcodeCodabar();
                break;
            default:
                showText(elem, "Invalid Barcode type");
                return;
            }
            bc.Code = code;
            bc.Font = font.BaseFont;
            bc.Size = font.Size;
            bc.Baseline = paramY(elem.GetAttribute("baseline"), font.LineHeight);
            bc.StartStopText = ("yes" == elem.GetAttribute("startstoptext"));
            if("yes" == elem.GetAttribute("notext")){
                bc.AltText = "";
                bc.Baseline = 0.001f;
            }
            if("yes" == elem.GetAttribute("toptext")){
                bc.Baseline = 0f;
            }
            string alt = elem.GetAttribute("alttext");
            if(!String.IsNullOrEmpty(alt)) {
                bc.AltText = report.extractField(alt, currentList, currentLine);
            }

            try {
                Rectangle brect = bc.BarcodeSize;
                float w = paramX(elem.GetAttribute("width"), brect.Right);
                float h = paramY(elem.GetAttribute("height"), brect.Top);
                float xx = x;
                string val = elem.GetAttribute("align");
                if(val != null){
                    switch(val) {
                    case "right":
                        xx = x-w;
                        break;
                    case "center":
                        xx = x-w/2f;
                        break;
                    default: // includes "left"
                        break;
                    }
                }
                Image barimage = bc.CreateImageWithBarcode(cb, null, null);
                addImage(barimage, _x(xx), _y(y+h), w, h);
            } catch(Exception) {
                showText(elem, code);
            }
            //showBox(elem, x, y, w, h);
        }


        /*
        public void test() {
            x = 100.0f;
            y = 100.0f;

            Console.WriteLine("Screen is [{0},{1}]x[{2},{3}]", offsetX, offsetY, width, height);
            Console.WriteLine("50={0}", paramX("50",9999.9f));
            Console.WriteLine("50%={0}", paramX("50%",9999.9f));
            Console.WriteLine("+50={0}", paramX("+50",9999.9f));
            Console.WriteLine("-50={0}", paramX("-50",9999.9f));
            Console.WriteLine("50char={0}", paramX("50char",9999.9f));
            Console.WriteLine("+50char={0}", paramX("+50char",9999.9f));
            Console.WriteLine("-50char={0}", paramX("-50char",9999.9f));
            Console.WriteLine("50mm={0}", paramX("50mm",9999.9f));
            Console.WriteLine("+50mm={0}", paramX("+50mm",9999.9f));
            Console.WriteLine("-50mm={0}", paramX("-50mm",9999.9f));

            cb.BeginText();
            for(int i = 0; i < 10; i++) {
                showTextAligned(PdfContentByte.ALIGN_LEFT, "国", _x(x), _y(y), 0);
                x += cx;
            }
            x = 100.0f;
            y = 100.0f;
            for(int i = 0; i < 10; i++) {
                showTextAligned(PdfContentByte.ALIGN_LEFT, "国", _x(x), _y(y), 0);
                y += cy;
            }
            cb.EndText();

            cb.SetLineWidth(0f);
            moveTo(_x(100f),_y(100f));
            lineTo(_x(200f),_y(200f));
            moveTo(_x(200f),_y(100f));
            lineTo(_x(100f),_y(100f));
            lineTo(_x(100f),_y(200f));
            cb.Stroke();
        }
        */

        private PDFReport report;

        private float offsetX, offsetY;
        private float width, height;

        private float x, y;
        private float cx, cy, descent;

        private FontParam font;

        private PdfContentByte cb;

        private int currentList;
        private int currentLine;

        private void init() {
            x = 0f;
            y = 0f;
            cb = report.writer.DirectContent;
            selectFont("");
        }

        private void selectFont(XmlElement elem) {
            if(elem == null) {
                selectFont("");
                return;
            }
            string f = elem.GetAttribute("font");
            if(!String.IsNullOrEmpty(f)) {
                selectFont(f);
                return;
            }
            if((elem.ParentNode == null) || (elem.ParentNode.NodeType != XmlNodeType.Element)) {
                selectFont("");
                return;
            }
            selectFont((XmlElement)(elem.ParentNode));
        }

        private void selectFont(string key) {
            if(String.IsNullOrEmpty(key) || !report.fontDict.ContainsKey(key))
                font = report.fontDict["default"];
            else
                font = report.fontDict[key];
            cx = font.CharWidth;
            descent = font.Descent;
            cy = font.LineHeight;
            cb.SetFontAndSize(font.BaseFont, font.Size);
            cb.SetCharacterSpacing(font.CharSpace);
        }

        private static readonly Regex pat_param = new Regex(@"^([+\-!]?)([0-9.]+)(\D*)$");

        private float paramX(string txt, float def) {
            if(String.IsNullOrEmpty(txt))
                return def;
            GroupCollection result = pat_param.Match(txt).Groups;
            if(!result[0].Success)
                return def;
            // 数値
            float val = StringUtil.ToFloat(result[2].Value);
            // 単位
            if(result[3].Value.Length > 0) {
                switch(result[3].Value[0]) {
                case '%': // ページ幅に対する割合
                    val = width*val/100.0f;
                    break;
                case 'c': // 文字数
                    val *= cx;
                    break;
                case 'm': // mm
                    val *= 72.0f/25.4f;
                    break;
                }
            }
            switch(result[1].Value) {
            case "+":
                val += x;
                break;
            case "-":
                val = x-val;
                break;
            case "!":
                val = width-val;
                break;
            }
            return val;
        }

        private float paramY(string txt, float def) {
            if(String.IsNullOrEmpty(txt))
                return def;
            GroupCollection result = pat_param.Match(txt).Groups;
            if(!result[0].Success)
                return def;
            // 数値
            float val = StringUtil.ToFloat(result[2].Value);
            // 単位
            if(result[3].Value.Length > 0) {
                switch(result[3].Value[0]) {
                case '%': // ページ高さに対する割合
                    val = height*val/100.0f;
                    break;
                case 'l': // 行数
                    val *= cy;
                    break;
                case 'm': // mm
                    val *= 72.0f/25.4f;
                    break;
                }
            }
            switch(result[1].Value) {
            case "+":
                val += y;
                break;
            case "-":
                val = y-val;
                break;
            case "!":
                val = height-val;
                break;
            }
            return val;
        }

        private void alignPosition(XmlElement elem, float w, float h, out float orgx, out float orgy) {
            orgx = x;
            orgy = y;
            switch(elem.GetAttribute("align")) {
            case "right":
                x -= w;
                break;
            case "center":
                x -= w/2;
                break;
            }
            switch(elem.GetAttribute("vertical-align")) {
            case "bottom":
                y -= h;
                break;
            case "center":
            case "middle":
                y -= h/2;
                break;
            }
        }

        private float getTextWidth(string txt) {
            return getTextWidth(txt, font.Size);
        }

        private float getTextWidth(string txt, float fontsize) {
            return font.BaseFont.GetWidthPoint(txt, fontsize);
        }

        private void showTextAligned(int align, string txt, float x, float y, float rot) {
            if(report.Rotate)
                cb.ShowTextAligned(align, txt, report.doc.Top-y, x, rot+90f);
            else
                cb.ShowTextAligned(align, txt, x, y, rot);
        }

        private void clipRect(float x, float y, float w, float h) {
            cb.NewPath();
            if(report.Rotate)
                cb.Rectangle(report.doc.Top-y, x, h, w);
            else
                cb.Rectangle(x, y, w, -h);
            cb.Clip();
            cb.NewPath();
        }

        private void moveTo(float x, float y) {
            if(report.Rotate)
                cb.MoveTo(report.doc.Top-y, x);
            else
                cb.MoveTo(x, y);
        }

        private void lineTo(float x, float y) {
            if(report.Rotate)
                cb.LineTo(report.doc.Top-y, x);
            else
                cb.LineTo(x, y);
        }

        private void addImage(Image img, float x, float y, float w, float h) {
            if(report.Rotate)
                cb.AddImage(img, 0, w, -h, 0, report.doc.Top-y, x);
            else
                cb.AddImage(img, w, 0, 0, h, x, y);
        }

        private float _x(float v) {
            return offsetX+v;
        }

        private float _y(float v) {
            return report.doc.Top-(offsetY+v);
        }

        private void showBox(XmlElement elem, float xx, float yy, float w, float h) {
            /* LP9600用のPSドライバが setdash を正しく処理できないらしいので、
               点線機能は割愛。
            // 点線の設定
            string dashstr = elem.GetAttribute("linedash");
            if(String.IsNullOrEmpty(dashstr))
                dashstr = "0";
            string[] dashstrs = dashstr.Split(",".ToCharArray());
            float[] dash = new float[dashstrs.Length];
            for(int i = 0; i < dashstrs.Length; i++)
                dash[i] = paramX(dashstrs[i].Trim(), 0f);
            if(dash.Length == 1)
                cb.SetLineDash(dash[0], dash[0], 0f);
            else if(dash.Length == 2)
                cb.SetLineDash(dash[0], dash[1], 0f);
            else
                cb.SetLineDash(dash, 0f);
            */

            showBoxAux(elem, xx, yy, w, h);
            showCrossLine(elem, xx, yy, w, h);
        }

        private void showBoxAux(XmlElement elem, float xx, float yy, float w, float h) {
            float b = paramX(elem.GetAttribute("border"), 0f);
            float bt = paramY(elem.GetAttribute("border-top"), b);
            float br = paramX(elem.GetAttribute("border-right"), b);
            float bb = paramY(elem.GetAttribute("border-bottom"), b);
            float bl = paramX(elem.GetAttribute("border-left"), b);
            if((bt <= 0f)&&(br <= 0f)&&(bb <= 0f)&&(bl <= 0f))
                return;
            if((bt == br) && (br == bb) && (bb == bl)) {
                cb.SetLineWidth(bt);
                moveTo(_x(xx),_y(yy));
                lineTo(_x(xx+w),_y(yy));
                lineTo(_x(xx+w),_y(yy+h));
                lineTo(_x(xx),_y(yy+h));
                lineTo(_x(xx),_y(yy));
                lineTo(_x(xx+w),_y(yy));
                cb.Stroke();
                return;
            }
            if(bt > 0f) {
                cb.SetLineWidth(bt);
                moveTo(_x(xx),_y(yy));
                lineTo(_x(xx+w),_y(yy));
                cb.Stroke();
            }
            if(br > 0f) {
                cb.SetLineWidth(br);
                moveTo(_x(xx+w),_y(yy));
                lineTo(_x(xx+w),_y(yy+h));
                cb.Stroke();
            }
            if(bb > 0f) {
                cb.SetLineWidth(bb);
                moveTo(_x(xx+w),_y(yy+h));
                lineTo(_x(xx),_y(yy+h));
                cb.Stroke();
            }
            if(bl > 0f) {
                cb.SetLineWidth(bl);
                moveTo(_x(xx),_y(yy+h));
                lineTo(_x(xx),_y(yy));
                cb.Stroke();
            }
        }

        private void showCrossLine(XmlElement elem, float xx, float yy, float w, float h) {
            float b = paramX(elem.GetAttribute("crossline"), 0f);
            float bs = paramY(elem.GetAttribute("slashline"), b);
            float bb = paramY(elem.GetAttribute("backslashline"), b);

            if((bs <= 0f)&&(bb <= 0f))
                return;
            if(bs > 0f) {
                cb.SetLineWidth(bs);
                moveTo(_x(xx+w),_y(yy));
                lineTo(_x(xx),_y(yy+h));
                cb.Stroke();
            }
            if(bb > 0f) {
                cb.SetLineWidth(bb);
                moveTo(_x(xx),_y(yy));
                lineTo(_x(xx+w),_y(yy+h));
                cb.Stroke();
            }
        }

        private void autoWidth(XmlElement elem) {
            if("yes" != elem.GetAttribute("autowidth"))
                return;
            elem.SetAttribute("autowidth", "no");
            float columnspace = paramX(elem.GetAttribute("columnspace"), cx);
            float totalsize = 0f;
            float validwidth = width;
            bool first = true;
            foreach(XmlNode node in elem.ChildNodes) {
                if(node.NodeType != XmlNodeType.Element)
                    continue;
                XmlElement el = (XmlElement)node;
                if((el.Name != "text") && (el.Name != "barcode") && (el.Name != "box"))
                    continue;
                if(first)
                    first = false;
                else
                    validwidth -= columnspace;
                totalsize += paramX(el.GetAttribute("width"), width/10f);
            }
            if(totalsize <= 0f)
                return;
            float dw = validwidth/totalsize;
            float pos = 0f;
            first = true;
            foreach(XmlNode node in elem.ChildNodes) {
                if(node.NodeType != XmlNodeType.Element)
                    continue;
                XmlElement el = (XmlElement)node;
                if((el.Name != "text") && (el.Name != "barcode") && (el.Name != "box"))
                    continue;
                if(first)
                    first = false;
                else
                    pos += columnspace;
                float w = dw*paramX(el.GetAttribute("width"), width/10f);
                el.SetAttribute("width", w.ToString());
                string align = el.GetAttribute("align");
                if(align == null)
                    align = "left";
                switch(align){
                case "right":
                    el.SetAttribute("x", (pos+w).ToString());
                    break;
                case "center":
                    el.SetAttribute("x", (pos+w/2f).ToString());
                    break;
                default: // includes "left"
                    el.SetAttribute("x", pos.ToString());
                    break;
                }
                pos += w;
            }
        }

    }

    private class FontParam {
        public FontParam(BaseFont basefont_, float size_) {
            baseFont = basefont_;
            if(size_ > 0f)
                size = size_;
            else
                size = 12.0f;
            lineHeight = descent = charWidth = charSpace = 0f;
        }

        public FontParam(XmlElement elem, BaseFont basefont_) {
            string fontfile = elem.GetAttribute("file");
            if(String.IsNullOrEmpty(fontfile))
                baseFont = basefont_;
            else
                baseFont = GetBaseFont(fontfile);
            size = StringUtil.ToFloat(elem.GetAttribute("size"), 12.0f);
            lineHeight = StringUtil.ToFloat(elem.GetAttribute("lineheight"), 0f);
            descent = StringUtil.ToFloat(elem.GetAttribute("descent"), 0f);
            charWidth = StringUtil.ToFloat(elem.GetAttribute("charwidth"), 0f);
            charSpace = StringUtil.ToFloat(elem.GetAttribute("charspace"), 0f);
        }

        public BaseFont BaseFont {
            get { return baseFont; }
        }

        public float Size {
            get { return size; }
        }

        public float LineHeight {
            get {
                if(lineHeight <= 0f) {
                    getLineHeight();
                }
                return lineHeight;
            }
        }

        public float Descent {
            get {
                if(lineHeight <= 0f)
                    getLineHeight();
                return descent;
            }
        }

        public float CharWidth {
            get {
                if(charWidth <= 0f) {
                    charWidth = baseFont.GetWidthPoint("X", size);
                    if(charWidth <= 0f)
                        charWidth = size;
                }
                return charWidth;
            }
        }

        public float CharSpace {
            get {
                return charSpace;
            }
        }

        private BaseFont baseFont;
        private float size;
        private float lineHeight;
        private float descent;
        private float charWidth;
        private float charSpace;

        private void getLineHeight() {
            descent = baseFont.GetDescentPoint("g", size);
            lineHeight = baseFont.GetAscentPoint("X国", size)-descent;
            if(lineHeight <= 0f)
                lineHeight = size;
        }
    }

}
