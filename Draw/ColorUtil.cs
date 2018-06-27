/// ColorUtil: System.DrawingのColor用のユーティリティ.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using MACS;

namespace MACS.Draw {

/// <summary>
///   System.DrawingのColor用のユーティリティ
/// </summary>
public static class ColorUtil {

    /// <summary>
    ///   名前から色を得る
    /// </summary>
    public static Color Get(string cname) {
        return Get(cname, Color.Black);
    }

    public static Color Get(string cname, Color defColor) {
        if(String.IsNullOrEmpty(cname)) {
            return defColor;
        }
        cname = cname.Trim().ToLower();
        if(cname == "auto") {
            return Auto;
        }
        if(cname[0] == '#') {
            switch(cname.Length) {
            case 4: // #RGB
                return FromRGB(StringUtil.ToHexInt(cname.Substring(1,1)+cname.Substring(1,1)),
                               StringUtil.ToHexInt(cname.Substring(2,1)+cname.Substring(2,1)),
                               StringUtil.ToHexInt(cname.Substring(3,1)+cname.Substring(3,1)));
            case 5: // #ARGB
                return FromRGB(StringUtil.ToHexInt(cname.Substring(2,1)+cname.Substring(2,1)),
                               StringUtil.ToHexInt(cname.Substring(3,1)+cname.Substring(3,1)),
                               StringUtil.ToHexInt(cname.Substring(4,1)+cname.Substring(4,1)),
                               StringUtil.ToHexInt(cname.Substring(1,1)+cname.Substring(1,1)));
            case 7: // #RRGGBB
                return FromRGB(StringUtil.ToHexInt(cname.Substring(1,2)),
                               StringUtil.ToHexInt(cname.Substring(3,2)),
                               StringUtil.ToHexInt(cname.Substring(5,2)));
            case 9: // #AARRGGBB
                return FromRGB(StringUtil.ToHexInt(cname.Substring(3,2)),
                               StringUtil.ToHexInt(cname.Substring(5,2)),
                               StringUtil.ToHexInt(cname.Substring(7,2)),
                               StringUtil.ToHexInt(cname.Substring(1,2)));
            default:
                return Color.FromArgb(255,255,255,255);
            }
        }
        Match m = pat_hsv.Match(cname);
        if(m.Success) {
            return FromHSV(StringUtil.ToDouble(m.Groups[1].Value),
                           StringUtil.ToDouble(m.Groups[2].Value),
                           StringUtil.ToDouble(m.Groups[3].Value));
        }
        m = pat_hsva.Match(cname);
        if(m.Success) {
            return FromHSV(StringUtil.ToDouble(m.Groups[1].Value),
                           StringUtil.ToDouble(m.Groups[2].Value),
                           StringUtil.ToDouble(m.Groups[3].Value),
                           StringUtil.ToDouble(m.Groups[4].Value));
        }
        m = pat_hsl.Match(cname);
        if(m.Success) {
            return FromHSV(StringUtil.ToDouble(m.Groups[1].Value),
                           StringUtil.ToDouble(m.Groups[2].Value)/100.0,
                           StringUtil.ToDouble(m.Groups[3].Value)/100.0);
        }
        m = pat_hsla.Match(cname);
        if(m.Success) {
            return FromHSV(StringUtil.ToDouble(m.Groups[1].Value),
                           StringUtil.ToDouble(m.Groups[2].Value)/100.0,
                           StringUtil.ToDouble(m.Groups[3].Value)/100.0,
                           StringUtil.ToDouble(m.Groups[4].Value));
        }
        m = pat_rgb.Match(cname);
        if(m.Success) {
            return FromRGB(StringUtil.ToInt(m.Groups[1].Value),
                           StringUtil.ToInt(m.Groups[2].Value),
                           StringUtil.ToInt(m.Groups[3].Value));
        }
        m = pat_rgbp.Match(cname);
        if(m.Success) {
            return FromRGB(StringUtil.ToDouble(m.Groups[1].Value)/100.0,
                           StringUtil.ToDouble(m.Groups[2].Value)/100.0,
                           StringUtil.ToDouble(m.Groups[3].Value)/100.0);
        }
        m = pat_rgba.Match(cname);
        if(m.Success) {
            return FromRGB(StringUtil.ToInt(m.Groups[1].Value),
                           StringUtil.ToInt(m.Groups[2].Value),
                           StringUtil.ToInt(m.Groups[3].Value),
                           (int)(StringUtil.ToDouble(m.Groups[4].Value)*255.0));
        }
        m = pat_rgbap.Match(cname);
        if(m.Success) {
            return FromRGB(StringUtil.ToDouble(m.Groups[1].Value)/100.0,
                           StringUtil.ToDouble(m.Groups[2].Value)/100.0,
                           StringUtil.ToDouble(m.Groups[3].Value)/100.0,
                           StringUtil.ToDouble(m.Groups[4].Value));
        }
        Color c = Color.FromName(cname);
        if((c.A == 0)&&(c.R == 0)&&(c.G == 0)&&(c.B == 0))
            return defColor;
        return c;
    }

    /// <summary>
    ///   RGB値から色を得る
    /// </summary>
    /// <param name="r">赤の値(0-255)</param>
    /// <param name="g">緑の値(0-255)</param>
    /// <param name="b">青の値(0-255)</param>
    /// <param name="a">不透明度の値(0-255)</param>
    public static Color FromRGB(int r, int g, int b, int a=255) {
        if(r < 0)
            r = 0;
        else if(r > 255)
            r = 255;
        if(g < 0)
            g = 0;
        else if(g > 255)
            g = 255;
        if(b < 0)
            b = 0;
        else if(b > 255)
            b = 255;
        if(a < 0)
            a = 0;
        else if(a > 255)
            a = 255;
        return Color.FromArgb(a,r,g,b);
    }

    /// <summary>
    ///   RGB値から色を得る
    /// </summary>
    /// <param name="r">赤の値(0.0-1.0)</param>
    /// <param name="g">緑の値(0.0-1.0)</param>
    /// <param name="b">青の値(0.0-1.0)</param>
    /// <param name="a">不透明度の値(0.0-1.0)</param>
    public static Color FromRGB(double r, double g, double b, double a=1.0) {
        return FromRGB((int)(r*255),(int)(g*255),(int)(b*255),(int)(a*255));
    }

    /// <summary>
    ///   2つの色の中間色を得る
    /// </summary>
    /// <param name="colA">色A</param>
    /// <param name="colB">色B</param>
    /// <param name="ratio">色Aの比率(0.0-1.0)</param>
    public static Color MeanColor(Color colA, Color colB, double ratio) {
        double xratio = 1.0-ratio;
        int r = (int)((double)colA.R*ratio+(double)colB.R*xratio);
        int g = (int)((double)colA.G*ratio+(double)colB.G*xratio);
        int b = (int)((double)colA.B*ratio+(double)colB.B*xratio);
        int a = (int)((double)colA.A*ratio+(double)colB.A*xratio);
        return FromRGB(r,g,b,a);
    }

    /// <summary>
    ///   指定色を明るくした色を得る
    /// </summary>
    /// <param name="col">元の色</param>
    /// <param name="ratio">明るくする度合(0.0-1.0)</param>
    public static Color BrightColor(this Color col, double ratio) {
        return MeanColor(Color.White, col, ratio);
    }

    /// <summary>
    ///   指定色を暗くした色を得る
    /// </summary>
    /// <param name="col">元の色</param>
    /// <param name="ratio">暗くする度合(0.0-1.0)</param>
    public static Color DarkColor(this Color col, double ratio) {
        return MeanColor(Color.Black, col, ratio);
    }

    /// <summary>
    ///   指定した背景色に応じた白または黒を得る
    /// </summary>
    public static Color GetBWColor(this Color backColor) {
        double l = GetLuminance(backColor);
        if(l < 0.5)
            return Color.White;
        else
            return Color.Black;
    }

    /// <summary>
    ///   HSV値を得る
    /// </summary>
    public static void GetHSV(Color col, out double h, out double s, out double v) {
        int cmax, cmin;
        if(col.R < col.G) {
            if(col.G < col.B) {
                // R < G < B
                cmin = col.R;
                cmax = col.B;
            } else if(col.R < col.B) {
                // R < B <= G
                cmin = col.R;
                cmax = col.G;
            } else {
                // B <= R <= G
                cmin = col.B;
                cmax = col.G;
            }
        } else {
            if(col.R < col.B) {
                // G <= R < B
                cmin = col.G;
                cmax = col.B;
            } else if(col.G < col.B) {
                // G < B <= R
                cmin = col.G;
                cmax = col.R;
            } else {
                // B <= G <= R
                cmin = col.B;
                cmax = col.R;
            }
        }
        if(cmin == cmax) {
            h = 0.0;
        } else if(cmax == col.R) {
            h = 60.0*((double)(col.G-col.B)/(double)(cmax-cmin));
        } else if(cmax == col.G) {
            h = 60.0*((double)(col.B-col.R)/(double)(cmax-cmin))+120.0;
        } else { // cmax == col.B
            h = 60.0*((double)(col.R-col.G)/(double)(cmax-cmin))+240.0;
        }
        if(h < 0)
            h += 360;
        if(cmax == 0) {
            s = 0.0;
        } else {
            s = (double)(cmax-cmin)/(double)cmax;
        }
        v = (double)cmax/255.0;
    }

    /// <summary>
    ///   HSV値から色を得る
    /// </summary>
    public static Color FromHSV(double h, double s, double v, double a=1.0) {
        int r,g,b;
        double cmax = v*255;
        double cmin = cmax-(s*v*255);
        while(h >= 360)
            h -= 360;
        while(h < 0)
            h += 360;
        if(h < 60) {
            r = (int)cmax;
            g = (int)((h/60)*(cmax-cmin)+cmin);
            b = (int)cmin;
        } else if(h < 120) {
            r = (int)(((120-h)/60)*(cmax-cmin)+cmin);
            g = (int)cmax;
            b = (int)cmin;
        } else if(h < 180) {
            r = (int)cmin;
            g = (int)cmax;
            b = (int)(((h-120)/60)*(cmax-cmin)+cmin);
        } else if(h < 240) {
            r = (int)cmin;
            g = (int)(((240-h)/60)*(cmax-cmin)+cmin);
            b = (int)cmax;
        } else if(h < 300) {
            r = (int)(((h-240)/60)*(cmax-cmin)+cmin);
            g = (int)cmin;
            b = (int)cmax;
        } else {
            r = (int)cmax;
            g = (int)cmin;
            b = (int)(((360-h)/60)*(cmax-cmin)+cmin);
        }
        return FromRGB(r,g,b,(int)(a*255));
    }

    /// <summary>
    ///   指定色の輝度を得る。(0.0-1.0)
    /// </summary>
    public static double GetLuminance(this Color col) {
        return 0.299*(double)col.R/255.0+0.587*(double)col.G/255.0+0.114*(double)col.B/255.0;
    }

    /// <summary>
    ///   #RRGGBB形式の文字列を得る
    /// </summary>
    public static string ToRGBString(this Color col) {
        if(col.A == 255)
            return String.Format("#{0:X2}{1:X2}{2:X2}", col.R, col.G, col.B);
        else
            return String.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", col.A, col.R, col.G, col.B);
    }
    
    /// <summary>
    ///   無効色を意味する色
    /// </summary>
    public static readonly Color Invalid = FromRGB(255,255,255,0);

    /// <summary>
    ///   自動色指定を意味する色
    /// </summary>
    public static readonly Color Auto = FromRGB(128,128,128,0);

    
    private static readonly Regex pat_hsv = new Regex(@"^hsv\(\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*\)$");
    private static readonly Regex pat_hsva = new Regex(@"^hsva\(\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*\)$");
    private static readonly Regex pat_hsl = new Regex(@"^hsl\(\s*([0-9.]+)\s*,\s*([0-9.]+)%?\s*,\s*([0-9.]+)%?\s*\)$");
    private static readonly Regex pat_hsla = new Regex(@"^hsla\(\s*([0-9.]+)\s*,\s*([0-9.]+)%?\s*,\s*([0-9.]+)%?\s*,\s*([0-9.]+)\s*\)$");
    private static readonly Regex pat_rgb = new Regex(@"^rgb\(\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*\)$");
    private static readonly Regex pat_rgbp = new Regex(@"^rgb\(\s*([0-9.]+)%\s*,\s*([0-9.]+)%\s*,\s*([0-9.]+)%\s*\)$");
    private static readonly Regex pat_rgba = new Regex(@"^rgba\(\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*,\s*([0-9.]+)\s*\)$");
    private static readonly Regex pat_rgbap = new Regex(@"^rgba\(\s*([0-9.]+)%\s*,\s*([0-9.]+)%\s*,\s*([0-9.]+)%\s*,\s*([0-9.]+)\s*\)$");

}

} // End of namespace
