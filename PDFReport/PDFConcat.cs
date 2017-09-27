using System;
using System.IO;

public class Program {
    public static int Main(string[] args) {
        if(args.Length < 2) {
            Console.WriteLine("USAGE: PDFConcat srcfile [srcfile...] dstfile");
            return 1;
        }
        string[] infiles = new string[args.Length-1];
        for(int i = 0; i < args.Length-1; i++)
            infiles[i] = args[i];
        try {
            PDFReport.Combine(args[args.Length-1], infiles);
        } catch(Exception e) {
            Console.WriteLine("Error: {0}", e.Message);
            return 1;
        }

        return 0;
    }
}
