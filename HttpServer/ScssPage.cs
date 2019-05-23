using System;
using System.IO;
using System.Text;
using MACS.HttpServer;
using MACS;
using SharpScss;

public class ScssPage : HttpStaticPage {

    public bool ForceCompile = false;
    public string CssDir = "/css/";
    public string ScssDir = "/sass/";
    public Encoding CssEncoding = new UTF8Encoding(false);

    public ScssPage(string path) : base(path){}

    public override void PageLoad(string param) {
        
        // .cssファイルが要求されていないときは普通のStaticPageとして処理する
        if(param.EndsWith(".css")) {
            string fname = m_dir+param.Replace('\\','/');
            // .scssファイルを確認する
            string scssfile = fname.Replace(CssDir, ScssDir).Replace(".css", ".scss");
            if(File.Exists(scssfile) && (ForceCompile || !File.Exists(fname) || (File.GetLastWriteTime(fname) < File.GetLastWriteTime(scssfile)))) {
                // .scssファイルを.cssファイルにコンパイルする
                var result = Scss.ConvertFileToCss(scssfile, new ScssOptions(){
                        InputFile = scssfile,
                        OutputFile = fname,
                        GenerateSourceMap = true
                    });
                using(StreamWriter sw = FileUtil.Writer(fname, CssEncoding)) {
                    if(sw == null)
                        throw new IOException(String.Format("Can't write to {0}", fname));
                    sw.Write(result.Css);
                }
                using(StreamWriter sw = FileUtil.Writer(fname+".map", CssEncoding)) {
                    if(sw == null)
                        throw new IOException(String.Format("Can't write to {0}", fname+".map"));
                    sw.Write(result.SourceMap);
                }
                LOG_INFO("Compiled {0} to {1}", scssfile, fname);
            }
        }
        base.PageLoad(param);
    }

}
