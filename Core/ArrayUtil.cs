/// ArrayUtil: 配列操作の便利ツール.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Text;

namespace MACS {


/// <summary>
///   配列操作の便利ツールを提供するクラス
/// </summary>
public static class ArrayUtil<T> where T:IEquatable<T>{

    /// <summary>
    ///   配列を逆順にしたものを作る
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     元の配列は変更されない。
    ///   </para>
    /// </remarks>
    /// <param name="array">元の配列</param>
    public static T[] GetReverse(T[] array) {
        if(array == null)
            return null;
        T[] res = new T[array.Length];
        for(int i = 0; i < array.Length; i++)
            res[array.Length-1-i] = array[i];
        return res;
    }

    /// <summary>
    ///   配列を逆順にする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     元の配列自身を変更する。
    ///   </para>
    /// </remarks>
    /// <param name="array">元の配列</param>
    public static void Reverse(T[] array) {
        if(array == null)
            return;
        for(int i = 0; i < array.Length/2; i++) {
            T x = array[i];
            array[i] = array[array.Length-1-i];
            array[array.Length-1-i] = x;
        }
    }

    /// <summary>
    ///   配列の違いを求める
    /// </summary>
    /// <param name="src">比較元配列</param>
    /// <param name="dst">比較先配列</param>
    /// <param name="newbie">srcにあってdstに無いもの</param>
    /// <param name="deleted">dstにあってsrcに無いもの</param>
    public static void GetDifference(T[] src, T[] dst, out T[] newbie, out T[] deleted) {
        List<T> newbieList = new List<T>();
        List<T> deletedList = new List<T>();
        // srcにあってdstに無いものを探す
        foreach(T s in src) {
            bool exist = false;
            foreach(T d in dst) {
                if(s.Equals(d)) {
                    exist = true;
                    break;
                }
            }
            if(!exist)
                newbieList.Add(s);
        }
        // dstにあってsrcに無いものを探す
        foreach(T d in dst) {
            bool exist = false;
            foreach(T s in src) {
                if(s.Equals(d)) {
                    exist = true;
                    break;
                }
            }
            if(!exist)
                deletedList.Add(d);
        }
        newbie = newbieList.ToArray();
        deleted = deletedList.ToArray();
    }

}

} // End of namespace
