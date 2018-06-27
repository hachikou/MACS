/// IpaddrRange: IPアドレス範囲を管理するクラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;

namespace MACS {


/// <summary>
///   IPアドレス範囲クラス
/// </summary>
public class IpaddrRange {

    /// <summary>
    ///   開始アドレス
    /// </summary>
    public readonly Ipaddr StartAddr = new Ipaddr();

    /// <summary>
    ///   終了アドレス
    /// </summary>
    public readonly Ipaddr EndAddr = new Ipaddr();

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public IpaddrRange() {
        // nothing to do.
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public IpaddrRange(Ipaddr start, Ipaddr end) {
        Set(start, end);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public IpaddrRange(string start, string end) {
        Set(new Ipaddr(start), new Ipaddr(end));
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public IpaddrRange(Ipaddr addr) {
        StartAddr.Set(addr);
        EndAddr.Set(addr);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public IpaddrRange(string addr) {
        StartAddr.Set(addr);
        EndAddr.Set(StartAddr);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public IpaddrRange(Ipaddr[] addrList) {
        switch(addrList.Length) {
        case 0:
            // nothing to do.
            break;
        case 1:
            StartAddr.Set(addrList[0]);
            EndAddr.Set(StartAddr);
            break;
        case 2:
            StartAddr.Set(addrList[0]);
            EndAddr.Set(addrList[1]);
            break;
        default:
            throw new ArgumentException("Too many addresses");
        }
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public IpaddrRange(IpaddrRange src) {
        StartAddr.Set(src.StartAddr);
        EndAddr.Set(src.EndAddr);
    }

    /// <summary>
    ///   アドレス範囲をセットする
    /// </summary>
    public void Set(Ipaddr start, Ipaddr end) {
        if(start <= end) {
            StartAddr.Set(start);
            EndAddr.Set(end);
        } else {
            StartAddr.Set(end);
            EndAddr.Set(start);
        }
    }

    /// <summary>
    ///   1個のアドレスをアドレス範囲としてセットする
    /// </summary>
    public void Set(Ipaddr addr) {
        StartAddr.Set(addr);
        EndAddr.Set(addr);
    }

    /// <summary>
    ///   アドレス範囲をアドレスリストにして返す
    /// </summary>
    /// <param name="limit">最大件数（負の場合制限無し）</param>
    public Ipaddr[] GetList(int limit=-1) {
        List<Ipaddr> list = new List<Ipaddr>();
        GetList(list, limit);
        return list.ToArray();
    }

    /// <summary>
    ///   アドレス範囲内のアドレスをアドレスリストに追加する
    /// </summary>
    /// <param name="list">このリストに追加する</param>
    /// <param name="limit">最大件数（負の場合制限無し）</param>
    public void GetList(List<Ipaddr> list, int limit) {
        Ipaddr addr = new Ipaddr(StartAddr);
        while(addr <= EndAddr) {
            if((limit >= 0) && (list.Count >= limit))
                return;
            if(!list.Contains(addr))
                list.Add(new Ipaddr(addr));
            addr.Incr();
        }
    }

    /// <summary>
    ///   有効なアドレス範囲かどうか
    /// </summary>
    public bool IsValid {
        get {
            return StartAddr.IsComplete() && EndAddr.IsComplete();
        }
    }

    /// <summary>
    ///   インスタンスがsrcで示されるアドレス範囲と同じかどうかを返す
    /// </summary>
    public bool Equals(IpaddrRange src) {
        if(src == null)
            return false;
        return (this.StartAddr == src.StartAddr) && (this.EndAddr == src.EndAddr);
    }

    /// <summary>
    ///   インスタンスがsrcで示されるアドレス範囲と同じかどうかを返す
    ///   一般オブジェクトとの比較バージョン。
    /// </summary>
    public override bool Equals(Object obj) {
        if(obj == null)
            return false;
        IpaddrRange src = obj as IpaddrRange;
        return Equals(src);
    }

    /// <summary>
    ///   インスタンスのハッシュコードを返す。
    /// </summary>
    public override int GetHashCode() {
        return StartAddr.GetHashCode()+EndAddr.GetHashCode();
    }

    public static bool operator ==(IpaddrRange a, IpaddrRange b) {
        if(ReferenceEquals(a, b))
            return true;
        if(ReferenceEquals(a, null))
            return false;
        return a.Equals(b);
    }

    public static bool operator !=(IpaddrRange a, IpaddrRange b) {
        return !(a == b);
    }

    /// <summary>
    ///   文字列表現を返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     開始アドレスと終了アドレスが同一の場合は単一IPアドレスの形式、
    ///     さもなければ '-'で開始アドレスを終了アドレスをつなげた形式。
    ///     無効なIPアドレスの場合は空文字列を返します。
    ///   </para>
    /// </remarks>
    override public string ToString() {
        if(!StartAddr.IsValid())
            return "";
        if(!EndAddr.IsValid() || EndAddr.IsZero() || (StartAddr == EndAddr))
            return StartAddr.ToString();
        return StartAddr.ToString()+"-"+EndAddr.ToString();
    }

}

} // End of namespace
