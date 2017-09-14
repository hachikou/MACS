using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace MACS {

public class Program {

    private static readonly Dictionary<int,char> specialChars = new Dictionary<int,char>(){
        {0x814c, '´'},
        {0x814e, '¨'},
        {0x817d, '±'},
        {0x817e, '×'},
        {0x8180, '÷'},
        {0x818b, '°'},
        {0x8198, '§'},
        {0x81f7, '¶'},
    };

    private static readonly Dictionary<int,string> CP932Chars = new Dictionary<int,string>() {
        {0xED, "纊褜鍈銈蓜俉炻昱棈鋹曻彅丨仡仼伀伃伹佖侒侊侚侔俍偀倢俿倞偆偰偂傔僴僘兊兤冝冾凬刕劜劦勀勛匀匇匤卲厓厲叝﨎咜咊咩哿喆坙坥垬埈埇﨏塚增墲夋奓奛奝奣妤妺孖寀甯寘寬尞岦岺峵崧嵓﨑嵂嵭嶸嶹巐弡弴彧德忞恝悅悊惞惕愠惲愑愷愰憘戓抦揵摠撝擎敎昀昕昻昉昮昞昤晥晗晙晴晳暙暠暲暿曺朎朗杦枻桒柀栁桄棏﨓楨﨔榘槢樰橫橆橳橾櫢櫤毖氿汜沆汯泚洄涇浯涖涬淏淸淲淼渹湜渧渼溿澈澵濵瀅瀇瀨炅炫焏焄煜煆煇凞燁燾犱"},
        {0xEE, "犾猤猪獷玽珉珖珣珒琇珵琦琪琩琮瑢璉璟甁畯皂皜皞皛皦益睆劯砡硎硤硺礰礼神祥禔福禛竑竧靖竫箞精絈絜綷綠緖繒罇羡羽茁荢荿菇菶葈蒴蕓蕙蕫﨟薰蘒﨡蠇裵訒訷詹誧誾諟諸諶譓譿賰賴贒赶﨣軏﨤逸遧郞都鄕鄧釚釗釞釭釮釤釥鈆鈐鈊鈺鉀鈼鉎鉙鉑鈹鉧銧鉷鉸鋧鋗鋙鋐﨧鋕鋠鋓錥錡鋻﨨錞鋿錝錂鍰鍗鎤鏆鏞鏸鐱鑅鑈閒隆﨩隝隯霳霻靃靍靏靑靕顗顥飯飼餧館馞驎髙髜魵魲鮏鮱鮻鰀鵰鵫鶴鸙黑??ⅰⅱⅲⅳⅴⅵⅶⅷⅸⅹ￢￤＇＂"},
        {0xFA, "ⅰⅱⅲⅳⅴⅵⅶⅷⅸⅹⅠⅡⅢⅣⅤⅥⅦⅧⅨⅩ￢￤＇＂㈱№㏍℡纊褜鍈銈蓜俉炻昱棈鋹曻彅丨仡仼伀伃伹佖侒侊侚侔俍偀倢俿倞偆偰偂傔僴僘兊兤冝冾凬刕劜劦勀勛匀匇匤卲厓厲叝﨎咜咊咩哿喆坙坥垬埈埇﨏塚增墲夋奓奛奝奣妤妺孖寀甯寘寬尞岦岺峵崧嵓﨑嵂嵭嶸嶹巐弡弴彧德忞恝悅悊惞惕愠惲愑愷愰憘戓抦揵摠撝擎敎昀昕昻昉昮昞昤晥晗晙晴晳暙暠暲暿曺朎朗杦枻桒柀栁桄棏﨓楨﨔榘槢樰橫橆橳橾櫢櫤毖氿汜沆汯泚洄涇浯"},
        {0xFB, "涖涬淏淸淲淼渹湜渧渼溿澈澵濵瀅瀇瀨炅炫焏焄煜煆煇凞燁燾犱犾猤猪獷玽珉珖珣珒琇珵琦琪琩琮瑢璉璟甁畯皂皜皞皛皦益睆劯砡硎硤硺礰礼神祥禔福禛竑竧靖竫箞精絈絜綷綠緖繒罇羡羽茁荢荿菇菶葈蒴蕓蕙蕫﨟薰蘒﨡蠇裵訒訷詹誧誾諟諸諶譓譿賰賴贒赶﨣軏﨤逸遧郞都鄕鄧釚釗釞釭釮釤釥鈆鈐鈊鈺鉀鈼鉎鉙鉑鈹鉧銧鉷鉸鋧鋗鋙鋐﨧鋕鋠鋓錥錡鋻﨨錞????鍗鎤鏆鏞鏸鐱鑅鑈閒隆﨩隝隯霳霻靃靍靏靑靕顗顥飯飼餧館馞驎髙"},
        {0xFC, "髜魵魲鮏鮱鮻鰀鵰鵫鶴鸙黑????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????"}
    };

    private static readonly Encoding sjis = Encoding.GetEncoding("Shift_JIS");

    public static int Main() {
        StringBuilder sb = new StringBuilder();
        for(int j = 0x81; j <= 0x9f; j++) {
            putOne(sb, j);
        }
        for(int j = 0xe0; j <= 0xfc; j++) {
            putOne(sb, j);
        }
        string zen = sb.ToString();

        sb.Clear();
        byte[] buf = new byte[1];
        for(int i = 0xa1; i <= 0xdf; i++) {
            buf[0] = (byte)i;
            sb.Append(sjis.GetString(buf));
        }
        string han = sb.ToString();
        if(han.Length != (0xdf-0xa1+1)) {
            Console.WriteLine("Error on hankaku code");
            return 1;
        }

        sb.Clear();
        for(int j = 0x81; j <= 0x9f; j++) {
            putRevOne(sb, j);
        }
        for(int j = 0xe0; j <= 0xfc; j++) {
            putRevOne(sb, j);
        }
        string rev = sb.ToString();

        using(StreamReader sr = FileUtil.Reader("SJISDictionary.cs.in", Encoding.UTF8))
        using(StreamWriter sw = FileUtil.Writer("SJISDictionary.cs", Encoding.UTF8)) {
            string line;
            while((line = sr.ReadLine()) != null) {
                sw.WriteLine(line.Replace("// HANKAKUTABLE", han).Replace("// ZENKAKUTABLE", zen).Replace("// REVERSETABLE", rev));
            }
        }

        /*
        // CP932コードのテスト
        using(StreamWriter sw = FileUtil.Writer("CP932TEST.utf8", Encoding.UTF8)) {
            foreach(KeyValuePair<int,string> kv in CP932Chars)
                sw.WriteLine("{0:X2}:{1}", kv.Key, kv.Value);
        }
        using(StreamWriter sw = FileUtil.Writer("CP932TEST.sjis", Encoding.GetEncoding("Shift_JIS"))) {
            foreach(KeyValuePair<int,string> kv in CP932Chars)
                sw.WriteLine("{0:X2}:{1}", kv.Key, kv.Value);
        }
        using(StreamWriter sw = FileUtil.Writer("CP932TEST.cp932", Encoding.GetEncoding("CP932"))) {
            foreach(KeyValuePair<int,string> kv in CP932Chars)
                sw.WriteLine("{0:X2}:{1}", kv.Key, kv.Value);
        }
        */

        return 0;
    }

    private static void putOne(StringBuilder sb, int j) {
        byte[] buf = new byte[2];
        StringBuilder tb = new StringBuilder();
        for(int i = 0x40; i <= 0x7e; i++) {
            int code = j*256+i;
            if(specialChars.ContainsKey(code)) {
                tb.Append(specialChars[code]);
            } else if(CP932Chars.ContainsKey(j)) {
                tb.Append(CP932Chars[j][i-0x40]);
            } else if(j >= 0xf0) {
                tb.Append('?');
            } else {
                buf[0] = (byte)j;
                buf[1] = (byte)i;
                tb.Append(sjis.GetString(buf));
            }
        }
        for(int i = 0x80; i <= 0xfc; i++) {
            int code = j*256+i;
            if(specialChars.ContainsKey(code)) {
                tb.Append(specialChars[code]);
            } else if(CP932Chars.ContainsKey(j)) {
                tb.Append(CP932Chars[j][i-0x80+0x7e-0x40+1]);
            } else if(j >= 0xf0) {
                tb.Append('?');
            } else {
                buf[0] = (byte)j;
                buf[1] = (byte)i;
                tb.Append(sjis.GetString(buf));
            }
        }
        if(tb.Length != (0x7e-0x40+1)+(0xfc-0x80+1))
            Console.WriteLine("Zenkaku error 0x{0:x2}", j);
        sb.AppendFormat("        /* 0x{0:x2} */ \"{1}\",\n", j, tb.ToString());
    }

    private static Dictionary<char,ushort> revList = null;

    private static void putRevOne(StringBuilder sb, int j) {
        if(revList == null)
            revList = new Dictionary<char,ushort>();
        byte[] buf = new byte[2];
        for(int i = 0x40; i <= 0x7e; i++) {
            ushort code = (ushort)(j*256+i);
            buf[0] = (byte)j;
            buf[1] = (byte)i;
            char ch;
            if(specialChars.ContainsKey(code)) {
                ch = specialChars[code];
            } else if(CP932Chars.ContainsKey(j)) {
                ch = CP932Chars[j][i-0x40];
            } else if(j >= 0xf0) {
                ch = '?';
            } else {
                ch = sjis.GetChars(buf)[0];
            }
            if(ch != '?') {
                if(revList.ContainsKey(ch)) {
                    sb.AppendFormat("        //{{'{0}',0x{1:X4} }},  is duplicated (0x{2:X4})\n", ch, code, revList[ch]);
                } else {
                    sb.AppendFormat("        {{'{0}',0x{1:X4} }},\n", ch, code);
                    revList.Add(ch, code);
                }
            }
        }
        for(int i = 0x80; i <= 0xfc; i++) {
            ushort code = (ushort)(j*256+i);
            buf[0] = (byte)j;
            buf[1] = (byte)i;
            char ch;
            if(specialChars.ContainsKey(code)) {
                ch = specialChars[code];
            } else if(CP932Chars.ContainsKey(j)) {
                ch = CP932Chars[j][i-0x80+0x7e-0x40+1];
            } else if(j >= 0xf0) {
                ch = '?';
            } else {
                ch = sjis.GetChars(buf)[0];
            }
            if(ch != '?') {
                if(revList.ContainsKey(ch)) {
                    sb.AppendFormat("        //{{'{0}',0x{1:X4} }},  is duplicated (0x{2:X4})\n", ch, code, revList[ch]);
                } else {
                    sb.AppendFormat("        {{'{0}',0x{1:X4} }},\n", ch, code);
                    revList.Add(ch, code);
                }
            }
        }
    }
}

} // End of namespace
