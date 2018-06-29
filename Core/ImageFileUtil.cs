/// ImageFileUtil: 画像ファイルを取り扱うオブジェクト.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Drawing;
using System.IO;

namespace MACS {

/// <summary>
///   画像ファイルを取り扱うオブジェクト
/// </summary>
public class ImageFileUtil {

    /// <summary>
    ///   画像ファイルフォーマット
    /// </summary>
    public enum ImageFileFormat {
        UNKNOWN, // 不明
        BMP,     // ビットマップ
        GIF,     // GIF
        JPG,     // JPEG
        PNG,     // W3C PNG
        EXIF,    // Exif
        TIFF,    // Tiff
        ICO,     // Windows アイコン
        EMF,     // 拡張メタファイル
        WMF,     // Windows メタファイル
        MBMP     // メモリビットマップ
    }

    /// <summary>
    ///   画像ファイルの形式をチェックする
    /// </summary>
    public static bool CheckImageFormat(string src, Array formatList) {
        return (Array.IndexOf(formatList, GetImageFormatString(src)) > -1);
    }

    /// <summary>
    ///   画像ファイルの形式を文字列で返す
    /// </summary>
    public static string GetImageFormatString(string src) {
        try {
            switch(getImageFormat(src)) {
            case ImageFileFormat.BMP:
                return "bmp";
            case ImageFileFormat.GIF:
                return "gif";
            case ImageFileFormat.JPG:
                return "jpeg";
            case ImageFileFormat.PNG:
                return "png";
            case ImageFileFormat.EXIF:
                return "exif";
            case ImageFileFormat.TIFF:
                return "tiff";
            case ImageFileFormat.ICO:
                return "ico";
            case ImageFileFormat.EMF:
                return "emf";
            case ImageFileFormat.WMF:
                return "wmf";
            case ImageFileFormat.MBMP:
                return "mbmp";
            case ImageFileFormat.UNKNOWN:
            default:
                return "unknown";
            }
        } catch(Exception) {
            //just ignore
        }
        return "unknown";
    }

    /// <summary>
    ///   画像ファイルの形式を返す
    /// </summary>
    private static ImageFileFormat getImageFormat(string src) {
        try {
            using(Image img = Image.FromFile(src)) {
                //イメージのファイル形式を調べる
                if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp)) {
                    return ImageFileFormat.BMP;
                } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif)) {
                    return ImageFileFormat.GIF;
                } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg)) {
                    return ImageFileFormat.JPG;
                } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png)) {
                    return ImageFileFormat.PNG;
                } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Exif)) {
                    return ImageFileFormat.EXIF;
                } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Tiff)) {
                    return ImageFileFormat.TIFF;
                } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Icon)) {
                    return ImageFileFormat.ICO;
                } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Emf)) {
                    return ImageFileFormat.EMF;
                } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Wmf)) {
                    return ImageFileFormat.WMF;
                } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp)) {
                    return ImageFileFormat.MBMP;
                } else {
                    return ImageFileFormat.UNKNOWN;
                }
            }
        } catch(Exception) {
            //just ignore
        }
        return ImageFileFormat.UNKNOWN;
    }
}

} // End of namespace
