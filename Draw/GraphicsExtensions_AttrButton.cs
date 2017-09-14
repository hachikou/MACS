using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using MACS;

namespace MACS.Draw {

/// <summary>
///   System.Drawing.Graphicsクラスの拡張メソッド
/// </summary>
public static partial class GraphicsExtensions {

#region テキスト属性指定ボタン描画
        
    /// <summary>
    ///   テキスト属性指定ボタンを描画する
    /// </summary>
    public static void DrawButton(this Graphics g,
                                  Rectangle rect, float radius,
                                  string text, MPAttribute attr,
                                  ButtonFace face) {
        if(String.IsNullOrEmpty(text)) {
            g.DrawButton(rect, radius, new string[0], attr, face);
        } else {
            g.DrawButton(rect, radius, g.SplitXMLText(text), attr, face);
        }
    }
    
    /// <summary>
    ///   ボタンを描画する
    /// </summary>
    public static void DrawButton(this Graphics g,
                                  Rectangle rect, float radius,
                                  string[] text, MPAttribute attr,
                                  ButtonFace face) {
        g.DrawButton(rect, radius, new string[0], face);
        
        // 文字描画
        if((text != null) && (text.Length > 0)) {
            // テキスト描画エリア
            int mgn = (int)(radius/(1.41421356F*2F));
            if(mgn < (int)face.BorderWidth*2)
                mgn = (int)face.BorderWidth*2;
            Rectangle tRect = new Rectangle(rect.X+mgn+(int)face.TextOffset.X, rect.Y+mgn+(int)face.TextOffset.Y, rect.Width-mgn*2, rect.Height-mgn*2);
            g.DrawText(text, attr, tRect);
        }
    }
    
#endregion
    
}

} // End of namespace
