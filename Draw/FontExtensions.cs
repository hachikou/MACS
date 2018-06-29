/// FontExtensions: System.Drawing.Fontクラスの拡張メソッド.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Drawing;
using MACS;

namespace MACS.Draw {

/// <summary>
///   System.Drawing.Fontクラスの拡張メソッド
/// </summary>
public static class FontExtensions {

    /// <summary>
    ///   ピクセル単位のEMサイズを得る
    /// </summary>
    public static float GetEmSize(this Font font) {
        return font.GetHeight()*font.FontFamily.GetEmHeight(font.Style)/font.FontFamily.GetLineSpacing(font.Style);
    }

    /// <summary>
    ///   フォントのコピーを作成して返す
    /// </summary>
    public static Font Copy(this Font font) {
        return new Font(font.FontFamily, font.Size, font.Style, font.Unit);
    }
    
    /// <summary>
    ///   本フォントを基準とした他のフォントを作成して返す
    /// </summary>
    public static Font GetNewFont(this Font font, float size, string familyName=null, string styleName=null, string unitName=null) {
        if(size < 1F)
            size = 1F; // Fail safe
        FontFamily family = font.FontFamily;
        if(!String.IsNullOrEmpty(familyName)) {
            try {
                family = new FontFamily(familyName);
            } catch(ArgumentException) {
                // just ignore.
            }
        }
        FontStyle style = font.Style;
        if(!String.IsNullOrEmpty(styleName)) {
            switch(styleName[0]) {
            case 'B':
            case 'b':
                style = FontStyle.Bold;
                break;
            case 'I':
            case 'i':
                style = FontStyle.Italic;
                break;
            case 'S':
            case 's':
                style = FontStyle.Strikeout;
                break;
            case 'U':
            case 'u':
                style = FontStyle.Underline;
                break;
            default:
                style = FontStyle.Regular;
                break;
            }
        }
        GraphicsUnit unit = font.Unit;
        if(!String.IsNullOrEmpty(unitName)) {
            switch(unitName.ToLower()) {
            case "display":
                unit = GraphicsUnit.Display;
                break;
            case "document":
                unit = GraphicsUnit.Document;
                break;
            case "inch":
                unit = GraphicsUnit.Inch;
                break;
            case "millimeter":
                unit = GraphicsUnit.Millimeter;
                break;
            case "pixel":
                unit = GraphicsUnit.Pixel;
                break;
            case "point":
                unit = GraphicsUnit.Point;
                break;
            case "world":
                unit = GraphicsUnit.World;
                break;
            }
        }
        Font newFont = new Font(family, size, style, unit);
        if(family != font.FontFamily)
            family.Dispose();
        return newFont;
    }

}

} // End of namespace
