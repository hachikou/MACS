/**
 * MACアドレス範囲を管理するクラス
 * $Id: $
 *
 * Copyright (C) 2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Collections.Generic;

namespace MACS {


/// <summary>
///   MACアドレス範囲
/// </summary>
public class HwaddrRange {

    /// <summary>
    ///   開始アドレス
    /// </summary>
    public readonly Hwaddr StartAddr = new Hwaddr();

    /// <summary>
    ///   終了アドレス
    /// </summary>
    public readonly Hwaddr EndAddr = new Hwaddr();

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public HwaddrRange() {
        // nothing to do.
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public HwaddrRange(Hwaddr start, Hwaddr end) {
        Set(start, end);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public HwaddrRange(string start, string end) {
        Set(new Hwaddr(start), new Hwaddr(end));
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public HwaddrRange(Hwaddr addr) {
        StartAddr.Set(addr);
        EndAddr.Set(addr);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public HwaddrRange(string addr) {
        StartAddr.Set(addr);
        EndAddr.Set(StartAddr);
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public HwaddrRange(HwaddrRange src) {
        StartAddr.Set(src.StartAddr);
        EndAddr.Set(src.EndAddr);
    }

    /// <summary>
    ///   値をセットする
    /// </summary>
    public void Set(Hwaddr start, Hwaddr end) {
        if(start <= end) {
            StartAddr.Set(start);
            EndAddr.Set(end);
        } else {
            StartAddr.Set(end);
            EndAddr.Set(start);
        }
    }

    /// <summary>
    ///   値をセットする
    /// </summary>
    public void Set(Hwaddr addr) {
        StartAddr.Set(addr);
        EndAddr.Set(addr);
    }

    /// <summary>
    ///   ベンダコードをMACアドレス範囲として得る
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     srcの上位3オクテットのみをベンダコードとして用います。
    ///   </para>
    /// </remarks>
    public static HwaddrRange GetVendorCode(Hwaddr src) {
        byte[] vals = new byte[6];
        src.GetBytes(vals, 0);
        HwaddrRange addr = new HwaddrRange();
        vals[3] = vals[4] = vals[5] = 0x00;
        addr.StartAddr.Set(vals, 0, 6);
        vals[3] = vals[4] = vals[5] = 0xff;
        addr.EndAddr.Set(vals, 0, 6);
        return addr;
    }


    /// <summary>
    ///   アドレス範囲をアドレスリストにして返す
    /// </summary>
    /// <param name="limit">最大件数（負の場合制限無し）</param>
    public Hwaddr[] GetList(int limit=-1) {
        List<Hwaddr> list = new List<Hwaddr>();
        GetList(list, limit);
        return list.ToArray();
    }

    /// <summary>
    ///   アドレス範囲をアドレスリストに追加する
    /// </summary>
    /// <param name="list">このリストに追加する</param>
    /// <param name="limit">最大件数（負の場合制限無し）</param>
    public void GetList(List<Hwaddr> list, int limit) {
        Hwaddr addr = new Hwaddr(StartAddr);
        while(addr <= EndAddr) {
            if((limit >= 0) && (list.Count >= limit))
                return;
            if(!list.Contains(addr))
                list.Add(new Hwaddr(addr));
            addr.Incr();
        }
    }

    /// <summary>
    ///   インスタンスがsrcで示されるアドレス範囲と同じかどうかを返す
    /// </summary>
    public bool Equals(HwaddrRange src) {
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
        HwaddrRange src = obj as HwaddrRange;
        return Equals(src);
    }

    /// <summary>
    ///   インスタンスのハッシュコードを返す。
    /// </summary>
    public override int GetHashCode() {
        return StartAddr.GetHashCode()+EndAddr.GetHashCode();
    }

    public static bool operator ==(HwaddrRange a, HwaddrRange b) {
        if(ReferenceEquals(a, b))
            return true;
        if(ReferenceEquals(a, null))
            return false;
        return a.Equals(b);
    }

    public static bool operator !=(HwaddrRange a, HwaddrRange b) {
        return !(a == b);
    }

}

} // End of namespace
