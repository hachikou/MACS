/// Ipaddr: IPアドレスを管理するクラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;
using System.Web;
using System.Net;

namespace MACS {


/// <summary>
///   IPアドレスを管理するクラス
/// </summary>
/// <remarks>
///   <para>
///     2015/4/16に、IPv6も統括して扱えるように大拡張しました。
///   </para>
/// </remarks>
public class Ipaddr: IComparable {

    private byte[] vals;

    /// <summary>
    ///   デフォルトコンストラクタ。値は無効値。
    /// </summary>
    public Ipaddr() {
        vals = null;
    }

    /// <summary>
    ///   文字列表現からのコンストラクタ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     文字列の内容に応じてIPv4/IPv6の扱いを自動的に切り替えます
    ///   </para>
    /// </remarks>
    public Ipaddr(string addr) {
        Set(addr);
    }

    /// <summary>
    ///   uint値からのコンストラクタ(IPv4)
    /// </summary>
    public Ipaddr(uint addr) {
        Set(addr);
    }

    /// <summary>
    ///   バイト列からのコンストラクタ
    /// </summary>
    /// <param name="addr">バイト列先頭アドレス</param>
    /// <param name="offset">読み取りオフセットバイト数</param>
    /// <param name="len">読み取りバイト数</param>
    /// <remarks>
    ///   <para>
    ///     バイト数に応じてIPv4/IPv6の扱いを自動的に切り替えます。
    ///   </para>
    /// </remarks>
    public Ipaddr(byte[] addr, int offset=0, int len=-1) {
        if(len < 0)
            len = addr.Length;
        vals = new byte[len];
        for(int i = 0; i < len; i++)
            vals[i] = addr[offset + i];
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public Ipaddr(Ipaddr addr) {
        Set(addr);
    }


    /// <summary>
    ///   定義を消してnew直後の状態にする
    /// </summary>
    /// <returns>自分自身</returns>
    public Ipaddr Clear() {
        vals = null;
        return this;
    }

    /// <summary>
    ///   文字列からIPアドレスをセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     文字列の内容に応じてIPv4/IPv6の扱いを自動的に切り替えます。
    ///     文字列の形式がIPアドレスの形式になっていなくても例外は発生しません。
    ///     できる限り読み取ろうとします。（"192.168.1.1:80"のような末尾の無効
    ///     文字は無視します。）
    ///     全く読み取ることができない場合は、値が未設定の状態になります。
    ///   </para>
    /// </remarks>
    public Ipaddr Set(string ipaddr) {
        if(ipaddr == null) {
            vals = new byte[0];
            return this;
        }
        ipaddr = ipaddr.Trim();
        if(ipaddr == "") {
            vals = new byte[0];
            return this;
        }
        string[] x = ipaddr.Split(".".ToCharArray());
        if(x.Length == 4) {
            // IPv4形式のようだ
            if((vals == null) || (vals.Length != x.Length))
                vals = new byte[x.Length];
            for(int i = 0; i < x.Length; i++) {
                if (isByte(x[i]))
                    vals[i] = toByte(x[i]);
                else {
                    Clear();
                    break;
                }
            }
            return this;
        }
        x = ipaddr.Split(":".ToCharArray());
        if(x.Length > 2) {
            // IPv6形式のようだ
            int idx = 0;
            if(x[0] == "") // 先頭が省略されている
                idx++;
            int len = x.Length-idx;
            if(x[x.Length-1] == "") // 末尾が省略されている
                len--;
            if(len > 8)
                len = 8; // fail safe
            int omitLen = 8-len+1; // 省略した数
            if((vals == null) || (vals.Length != 16))
                vals = new byte[16];
            int i = 0;
            while((i < 16)&&(idx < x.Length)){
                if(x[idx] == "") {
                    // 省略部分
                    for(int j = 0; j < omitLen; j++) {
                        vals[i++] = 0;
                        vals[i++] = 0;
                    }
                    omitLen = 0; // 万が一省略部が2箇所以上あったときのfail safe
                } else {
                    ushort d = toUShort(x[idx]);
                    vals[i++] = (byte)(d>>8);
                    vals[i++] = (byte)(d&0xff);
                }
                idx++;
                len--;
            }
            return this;
        }
        // 形式異常
        vals = null;
        return this;
    }

    /// <summary>
    ///   uint値からIPアドレスをセットする(IPv4)
    /// </summary>
    /// <returns>自分自身</returns>
    public Ipaddr Set(uint ipaddr) {
        if((vals == null) || (vals.Length != 4))
            vals = new byte[4];
        for(int i = 3; i >= 0; i--) {
            vals[i] = (byte)(ipaddr % 256);
            ipaddr /= 256;
        }
        return this;
    }

    /// <summary>
    ///   他のIpaddrをコピーする
    /// </summary>
    /// <returns>自分自身</returns>
    public Ipaddr Set(Ipaddr ipaddr) {
        if((ipaddr != null) && ipaddr.IsValid()) {
            if((vals == null)||(vals.Length != ipaddr.vals.Length))
                vals = new byte[ipaddr.vals.Length];
            for(int i = 0; i < vals.Length; i++)
                vals[i] = ipaddr.vals[i];
        } else {
            Clear();
        }
        return this;
    }

    public Ipaddr Set(byte[] addr, int offset, int len) {
        if((vals == null)||(vals.Length != len))
            vals = new byte[len];
        for(int i = 0; i < len; i++)
            vals[i] = addr[offset+i];
        return this;
    }

    /// <summary>
    ///   インスタンスが有効かどうかを返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     4バイト以下の状態でもtrueを返します。
    ///     4バイトまたは16バイトそろっているかどうかは、IsComplete()を使って
    ///     確認してください。
    ///   </para>
    /// </remarks>
    public bool IsValid() {
        if((vals == null) || (vals.Length == 0))
            return false;
        return true;
    }

    /// <summary>
    ///   インスタンスが4バイトまたは16バイトのアドレスを持っているかどうかを返す
    /// </summary>
    public bool IsComplete() {
        if(vals == null)
            return false;
        return ((vals.Length==4)||(vals.Length==16));
    }

    /// <summary>
    ///   インスタンスが4バイトまたは16バイトのアドレスを持っているかまたはまったく設定されていないときにtrueを返す
    /// </summary>
    public bool IsCompleteOrNull() {
        if(vals == null)
            return false;
        return ((vals.Length==0)||(vals.Length==4)||(vals.Length==16));
    }

    /// <summary>
    ///   インスタンスがIPv4アドレスのときにtrueを返す
    /// </summary>
    public bool IsV4() {
        return (vals != null)&&(vals.Length==4);
    }

    /// <summary>
    ///   インスタンスがIPv6アドレスのときにtrueを返す
    /// </summary>
    public bool IsV6() {
        return (vals != null)&&(vals.Length==16);
    }

    /// <summary>
    ///   0.0.0.0または::のときにtrueを返す。
    /// </summary>
    public bool IsZero() {
        if(vals == null)
            return false;
        for(int i = 0; i < vals.Length; i++){
            if(vals[i] != 0)
                return false;
        }
        return true;
    }

    /// <summary>
    ///   インスタンスがネットマスクになっているかどうかを返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     最上位ビットから任意のビットが連続して1になっていて、その後のビット
    ///     がすべて0になっていることをチェックします。
    ///   </para>
    /// </remarks>
    public bool IsNetmask() {
        if(!IsComplete())
            return false;
        int i = 0;
        byte b = 0x80;
        // 上のビット1を確認する。
        while(i < vals.Length) {
            byte x = vals[i];
            while(b != 0) {
                if((x&b) == 0)
                    goto fullbreak;
                b >>= 1;
            }
            i++;
            b = 0x80;
        }
 fullbreak:
        // iがビット0が出現したバイトインデックス、bが同ビット位置を示している。
        // それ以降のビット0を確認する。
        while(i < vals.Length) {
            byte x = vals[i];
            while(b != 0) {
                if((x&b) != 0)
                    return false;
                b >>= 1;
            }
            i++;
            b = 0x80;
        }
        return true;
    }

    /// <summary>
    ///   文字列表現を返す
    /// </summary>
    override public string ToString() {
        if(!IsValid())
            return "";
        StringBuilder sb = new StringBuilder();
        if(vals.Length <= 4) {
            // IPv4形式で返す
            int i = 0;
            while(i < vals.Length) {
                if(sb.Length != 0)
                    sb.Append('.');
                sb.Append(vals[i].ToString());
                i++;
            }
            while(i < 4) {
                if(sb.Length != 0)
                    sb.Append('.');
                sb.Append('*');
                i++;
            }
        } else if(vals.Length <= 16) {
            // IPv6形式で返す
            // まずushortの配列を作る
            ushort[] x = new ushort[8];
            for(int i = 0; i < x.Length; i++)
                x[i] = 0;
            for(int i = 0; i < vals.Length; i++) {
                if(i%2 == 0)
                    x[i/2] = (ushort)(vals[i]*256);
                else
                    x[i/2] += (ushort)vals[i];
            }
            // 省略位置を探す
            // 0000が1つだけの箇所は省略しない。
            // 省略箇所が複数ある場合は最も多く省略できる場所を省略する。
            // 最も多く省略できる場所が複数ある場合、最初の場所を省略する。
            int omitPos = -1;
            int omitLen = 1;
            for(int i = 0; i < x.Length; i++) {
                if(x[i] == 0) {
                    int len = 1;
                    while((i+len < x.Length)&&(x[i+len] == 0))
                        len++;
                    if(len > omitLen) {
                        omitPos = i;
                        omitLen = len;
                    }
                }
            }
            if(omitPos == 0)
                sb.Append(':');
            int j = 0;
            while(j < x.Length) {
                if(j > 0)
                    sb.Append(':');
                if(j == omitPos) {
                    j += omitLen;
                } else {
                    sb.Append(x[j].ToString("x"));
                    j++;
                }
            }
            if(omitPos+omitLen == x.Length)
                sb.Append(':');
        }
        return sb.ToString();
    }

    /// <summary>
    ///   URLエンコーディングされた文字列表現を返す
    /// </summary>
    public string ToUrlString() {
        return HttpUtility.UrlEncode(ToString(), Encoding.GetEncoding("us-ascii"));
    }

    /// <summary>
    ///   アドレスをuint型の値として返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     IPv4でない場合は正しい値を返しません。
    ///   </para>
    /// </remarks>
    public uint ToUint() {
        if(vals == null)
            return 0;
        uint x = 0;
        for(int i = 0; i < vals.Length; i++) {
            x *= 256;
            x += vals[i];
        }
        return x;
    }

    /// <summary>
    ///   IPアドレスのバイト列を返す
    /// </summary>
    public byte[] GetBytes() {
        return vals;
    }

    /// <summary>
    ///   IPアドレスのバイト列を獲得する。
    /// </summary>
    /// <param name="dst">格納先のバイト列</param>
    /// <param name="index">dstの何バイト目から格納するか</param>
    public void GetBytes(byte[] dst, int index) {
        if(vals != null)
            Array.Copy(vals, 0, dst, index, vals.Length);
    }

    /// <summary>
    ///   何バイトのデータかを返す
    /// </summary>
    public int Length() {
        if(vals == null)
            return 0;
        return vals.Length;
    }

    /// <summary>
    ///   上位bitsビットが1であるアドレスパターンを返す
    /// </summary>
    /// <param name="bits">ビット数</param>
    /// <param name="v6flag">true=IPv6アドレスとして返す,false=IPv4アドレスとして返す</param>
    static public Ipaddr GetNetmask(int bits, bool v6flag=false) {
        byte[] vals;
        if(v6flag)
            vals = new byte[16];
        else
            vals = new byte[4];
        int i = 0;
        while(i < vals.Length)
            vals[i++] = 0;
        i = 0;
        byte b = 0x80;
        while((bits > 0) && (i < vals.Length)) {
            vals[i] |= b;
            b >>= 1;
            if(b == 0) {
                b = 0x80;
                i++;
            }
            bits--;
        }
        return new Ipaddr(vals,0,vals.Length);
    }

    /// <summary>
    ///   インスタンスがsrcで示されるアドレスパターンを内包しているかどうかをチェックする
    /// </summary>
    public bool Contains(Ipaddr src) {
        if(!src.IsValid())
            return false;
        return ToString().Contains(src.ToString());
    }

    /// <summary>
    ///   インスタンスがsrcで示されるアドレスパターンを内包しているかどうかをチェックする
    ///   文字列版。
    /// </summary>
    public bool Contains(string src) {
        if(src == null)
            return false;
        return ToString().Contains(src);
    }

    /// <summary>
    ///   インスタンスがsrcで示されるアドレスと同じかどうかを返す
    /// </summary>
    public bool Equals(Ipaddr src) {
        if(src == null)
            return false;
        if(IsValid()) {
            if(!src.IsValid())
                return false;
            if(vals.Length != src.vals.Length)
                return false;
            for(int i = 0; i < vals.Length; i++) {
                if(vals[i] != src.vals[i])
                    return false;
            }
            return true;
        } else {
            if(src.IsValid())
                return false;
            return true;
        }
    }

    /// <summary>
    ///   インスタンスがsrcで示されるアドレスと同じかどうかを返す
    ///   一般オブジェクトとの比較バージョン。
    /// </summary>
    public override bool Equals(Object obj) {
        if(obj == null)
            return false;
        Ipaddr src = obj as Ipaddr;
        return Equals(src);
    }

    /// <summary>
    ///   インスタンスのハッシュコードを返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     全バイトを足し合わせたものをハッシュ値としている。
    ///   </para>
    /// </remarks>
    public override int GetHashCode() {
        if(vals == null)
            return 0;
        int res = 0;
        for(int i = 0; i < vals.Length; i++)
            res += (int)vals[i];
        return res;
    }

    public static bool operator ==(Ipaddr a, Ipaddr b) {
        if(ReferenceEquals(a, b))
            return true;
        if(ReferenceEquals(a, null))
            return false;
        return a.Equals(b);
    }

    public static bool operator !=(Ipaddr a, Ipaddr b) {
        return !(a == b);
    }

    /// <summary>
    ///   指定ホスト名のIPアドレスを返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     DNS検索ができなかった場合は、127.0.0.1を返す。
    ///   </para>
    /// </remarks>
    public static Ipaddr ByHostName(string hostname) {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(hostname);
        foreach(IPAddress i in ipHostInfo.AddressList) {
            Ipaddr a = new Ipaddr(i.ToString());
            if(a.IsComplete())
                return a;
        }
        return new Ipaddr("127.0.0.1");
    }

    /// <summary>
    ///   アドレス値の大小を比較する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     IPv4アドレスとIPv6アドレスを比較しようとすると、つねにIPv4アドレスの
    ///     方が小さいと判定されます。
    ///   </para>
    /// </remarks>
    public int CompareTo(Ipaddr src) {
        if(vals == null)
            return -1;
        if((src == null) || (src.vals == null))
            return 1;
        if(vals.Length < src.vals.Length)
            return -1;
        if(vals.Length > src.vals.Length)
            return 1;
        for(int i = 0; i < vals.Length; i++) {
            if(vals[i] < src.vals[i])
                return -1;
            if(vals[i] > src.vals[i])
                return 1;
        }
        return 0;
    }

    /// <summary>
    ///   アドレス値の大小を比較する。
    ///   一般オブジェクトとの比較バージョン。
    /// </summary>
    public int CompareTo(Object obj) {
        if(obj == null)
            return 1;
        return CompareTo(obj as Ipaddr);
    }

    public static bool operator <(Ipaddr a, Ipaddr b) {
        return (a.CompareTo(b) < 0);
    }

    public static bool operator <=(Ipaddr a, Ipaddr b) {
        return (a.CompareTo(b) <= 0);
    }

    public static bool operator >(Ipaddr a, Ipaddr b) {
        return (a.CompareTo(b) > 0);
    }

    public static bool operator >=(Ipaddr a, Ipaddr b) {
        return (a.CompareTo(b) >= 0);
    }

    /// <summary>
    ///   アドレス値の大小を比較する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     IPv4アドレスとIPv6アドレスを比較しようとすると、つねにIPv4アドレスの
    ///     方が小さいと判定されます。
    ///   </para>
    /// </remarks>
    public static int Compare(Ipaddr a, Ipaddr b) {
        if(a == null) {
            if(b == null)
                return 0;
            return -1;
        }
        return a.CompareTo(b);
    }

    /// <summary>
    ///   ネットワークアドレス内にあるかどうかを返す。
    /// </summary>
    public bool InNetwork(Ipaddr network, Ipaddr netmask) {
        if((vals == null) || (network.vals == null) || (netmask.vals == null)
           || (vals.Length != network.vals.Length) || (vals.Length != network.vals.Length))
            return false;
        for(int i = 0; i < vals.Length; i++) {
            if((byte)(vals[i]&netmask.vals[i]) != (byte)(network.vals[i]&netmask.vals[i]))
                return false;
        }
        return true;
    }

    /// <summary>
    ///   指定したIPアドレスを指定したネットマスクでマスクした値を返す。
    /// </summary>
    static public Ipaddr GetMasked(Ipaddr ipaddr, Ipaddr mask) {
        if((ipaddr.vals == null) || (mask.vals == null))
            return new Ipaddr();
        byte[] vals = new byte[ipaddr.vals.Length];
        for(int i = 0; i < vals.Length; i++){
            if(i < mask.vals.Length)
                vals[i] = (byte)((ipaddr.vals[i]) & (mask.vals[i]));
            else
                vals[i] = 0;
        }
        return new Ipaddr(vals, 0, vals.Length);
    }

    /// <summary>
    ///   アドレスを一つ進める
    /// </summary>
    public void Incr() {
        if(vals == null)
            return;
        int idx = vals.Length-1;
        while(idx >= 0) {
            if(vals[idx] >= 255) {
                vals[idx] = 0;
                idx--;
            } else {
                vals[idx]++;
                return;
            }
        }
    }

    /// <summary>
    ///   文字列表現にした時に指定文字列から始まっているかどうか
    /// </summary>
    public bool StartsWith(string str) {
        return this.ToString().StartsWith(str.ToLower());
    }

    /// <summary>
    ///   文字列表現にした時に指定文字列で終わっているかどうか
    /// </summary>
    public bool EndsWith(string str) {
        return this.ToString().EndsWith(str.ToLower());
    }


    private static bool isHexDigit(char x) {
        return (('0'<=x)&&(x<='9'))||(('a'<=x)&&(x<='f'))||(('A'<=x)&&(x<='F'));
    }

    private static bool isByte(string str) {
        if (String.IsNullOrEmpty(str))
            return false;

        foreach(char ch in str)
            if((ch < '0') || (ch > '9'))
                return false;

        return true;
    }

    private static byte toByte(string str) {
        byte x = 0;
        foreach(char ch in str)
            x = (byte)(x*10+(ch-'0'));

        return x;
    }

    private static ushort toUShort(string str) {
        ushort x = 0;
        foreach(char ch in str) {
            if((ch >= '0') && (ch <= '9')) {
                x = (ushort)(x*16+(ch-'0'));
            } else if((ch >= 'a') && (ch <= 'f')) {
                x = (ushort)(x*16+(ch-'a')+10);
            } else if((ch >= 'A') && (ch <= 'F')) {
                x = (ushort)(x*16+(ch-'A')+10);
            } else {
                break;
            }
        }
        return x;
    }

#if SELFTEST

    public static int Main(string[] args) {
        foreach(string arg in args) {
            Ipaddr addr = new Ipaddr(arg);
            Console.Write("{0} -> {1}", arg, addr.ToString());
            foreach(byte b in addr.GetBytes()) {
                Console.Write(" {0:X2}", b);
            }
            Console.WriteLine();
        }
        return 0;
    }
#endif

}

} // End of namespace
