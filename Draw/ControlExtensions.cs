using System;
using System.Windows.Forms;
using System.Drawing;

namespace MACS.Draw {

/// <summary>
///   System.Windows.Forms.Controlクラスの拡張メソッド
/// </summary>
public static class ControlExtensions {

    /// <summary>
    ///   真の背景色を得る
    /// </summary>
    public static Color GetRealBackColor(this Control ctrl) {
        Control c = ctrl;
        while((c.BackColor.A == 0) && (c.Parent != null)) {
            c = c.Parent;
        }
        return c.BackColor;
    }

    /// <summary>
    ///   親コントロールの背景色を得る
    /// </summary>
    public static Color GetParentBackColor(this Control ctrl) {
        if(ctrl.Parent == null)
            return Color.Transparent;
        return ctrl.Parent.GetRealBackColor();
    }

    /// <summary>
    ///   背景を描画する（子コントロール用）
    /// </summary>
    public static void PaintBackground(this Control ctrl, PaintEventArgs e) {
        ctrl.PaintBackground(e, ctrl.ClientRectangle);
    }
    
    /// <summary>
    ///   背景を描画する（子コントロール用）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     PaintEventArgs内のGraphicsは子コントロールの座標系になっていることに
    ///     注意すること。ClipRectangleも同様。
    ///     一方、rectは本コントロールの座標系における子コントロールの位置を示し
    ///     ている。
    ///   </para>
    /// </remarks>
    public static void PaintBackground(this Control ctrl, PaintEventArgs e, Rectangle rect) {
        Color bgcolor = ctrl.GetRealBackColor();
        if(bgcolor.A > 0) {
            Graphics g = e.Graphics;
            using(Brush brush = new SolidBrush(bgcolor)) {
                g.FillRectangle(brush, e.ClipRectangle);
            }
        }
    }
    
}

} // End of namespace
