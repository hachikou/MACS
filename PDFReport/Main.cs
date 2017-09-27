using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MACS;

public class Program {
    public static int Main(string[] args) {
        string xmlfile = null;
        string fieldfile = null;
        List<string> listfiles = new List<string>();
        string outfile = null;
        bool showFieldName = false;
        bool showPageFrame = false;
        bool rotate = false;

        for(int i = 0; i < args.Length; i++) {
            switch(args[i]) {
            case "-o":
                outfile = args[++i];
                break;
            case "-f":
                showFieldName = true;
                showPageFrame = true;
                break;
            case "-r":
                rotate = true;
                break;
            default:
                if(xmlfile == null)
                    xmlfile = args[i];
                else if(fieldfile == null)
                    fieldfile = args[i];
                else
                    listfiles.Add(args[i]);
                break;
            }
        }

        PDFReport.FontDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "fonts");

        if(xmlfile == null) {
            Console.WriteLine("usage: PDFReport [options] xmlfile [fieldfile listfile1 listfile2...]");
            Console.WriteLine("option:");
            Console.WriteLine("  -o filename: Specify outpur PDF file name.");
            Console.WriteLine("  -f:          Show field names and page frame.");
            return 1;
        }

        PDFReport rep = new PDFReport(xmlfile);
        rep.Rotate = rotate;
        rep.ShowFieldName = showFieldName;
        rep.ShowPageFrame = showPageFrame;
        DataArray field = null;
        if(fieldfile != null) {
            field = DataArray.FromFile(fieldfile);
        }
        List<List<DataArray>> listList = new List<List<DataArray>>();
        foreach(string listfile in listfiles) {
            using(CSVFile csv = new CSVFile(listfile)) {
                listList.Add(csv.ReadAllData());
                csv.Close();
            }
        }

        if(outfile == null)
            rep.Output(Console.OpenStandardOutput(), field, listList);
        else
            rep.Output(outfile, field, listList);

        return 0;
    }
}
