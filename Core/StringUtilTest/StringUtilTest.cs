using System;
using MACS;

class StringUtilTest {
    static void Main(string[] args) {
        string txt = "半角カタカナを全角化するテスト。ガギグゲゴ、バビブベボ、パピプペポ";
        string txt2 = StringUtil.CompactString(txt);
        Console.WriteLine("{0} -> {1}", txt, txt2);
        txt = txt2;
        txt2 = StringUtil.ToZenkana(txt);
        Console.WriteLine("{0} -> {1}", txt, txt2);
        txt2 = StringUtil.ToZenkana(txt,true);
        Console.WriteLine("{0} -> {1}", txt, txt2);
    }
}
