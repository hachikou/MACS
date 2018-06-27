/// WMTouchControl: タッチパネル対応のコントロールモジュール.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

// This code was designed and coded by YAGI H. (COSMOSTORK Inc.)

/*
 * 注意: Windows10より古いOSでは動作しません。
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Permissions;

namespace MACS.Draw {

public class WMTouchControl : Control {
    [SecurityPermission(SecurityAction.Demand)]
    public WMTouchControl() {
        try {
            // マルチタッチをウィンドウに登録できない場合例外を発生
            if (!RegisterTouchWindow(this.Handle, 0)) {
                throw new InvalidOperationException();
            }
        }
        catch (Exception) {
            // RegisterTouchWindow APIが使用できない
            throw;
        }
        touchInputSize = Marshal.SizeOf(new TOUCHINPUT());
    }

    // タッチイベントハンドラ
    protected event EventHandler<WMTouchEventArgs> TouchDown;
    protected event EventHandler<WMTouchEventArgs> TouchUp;
    protected event EventHandler<WMTouchEventArgs> TouchMove;

    // タッチイベントウィンドウメッセージ定数 [winuser.h]
    public const int WM_TOUCH = 0x0240;

    // タッチイベントフラグ ((TOUCHINPUT.dwFlags) [winuser.h]
    public const int TOUCHEVENTF_MOVE = 0x0001;
    public const int TOUCHEVENTF_DOWN = 0x0002;
    public const int TOUCHEVENTF_UP = 0x0004;
    public const int TOUCHEVENTF_INRANGE = 0x0008;
    public const int TOUCHEVENTF_PRIMARY = 0x0010;
    public const int TOUCHEVENTF_NOCOALESCE = 0x0020;
    public const int TOUCHEVENTF_PEN = 0x0040;

    // タッチインプットマスク変数 (TOUCHINPUT.dwMask) [winuser.h]
    public const int TOUCHINPUTMASKF_TIMEFROMSYSTEM = 0x0001;
    public const int TOUCHINPUTMASKF_EXTRAINFO = 0x0002;
    public const int TOUCHINPUTMASKF_CONTACTAREA = 0x0004;

    // タッチAPI定義の構造体 [winuser.h]
    [StructLayout(LayoutKind.Sequential)]
    private struct TOUCHINPUT {
        public int x;
        public int y;
        public System.IntPtr hSource;
        public int dwID;
        public int dwFlags;
        public int dwMask;
        public int dwTime;
        public System.IntPtr dwExtraInfo;
        public int cxContact;
        public int cyContact;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINTS {
        public short x;
        public short y;
    }

    [DllImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterTouchWindow(System.IntPtr hWnd, ulong ulFlags);

    [DllImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTouchInputInfo(System.IntPtr hTouchInput, int cInputs, [In, Out] TOUCHINPUT[] pInputs, int cbSize);

    [DllImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern void CloseTouchInputHandle(System.IntPtr lParam);

    // タッチインプット構造体のサイズ
    private int touchInputSize;

    /// <summary>
    ///   イベントハンドラを登録する
    /// </summary>
    /// <param name="sender">センダー</param>
    /// <param name="e">イベント引数</param>
    private void OnLoadHandler(Object sender, EventArgs e) {
        try {
            // マルチタッチをウィンドウに登録できない場合例外を発生
            if (!RegisterTouchWindow(this.Handle, 0)) {
                throw new InvalidOperationException();
            }
        } catch (Exception) {
            // RegisterTouchWindow APIが使用できない
            throw;
        }
    }

    /// <summary>
    ///   WM_TOUCHメッセージを処理する
    /// </summary>
    /// <param name="m">ウィンドウメッセージ</param>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    protected override void WndProc(ref Message m) {
        bool handled;
        switch (m.Msg) {
            case WM_TOUCH:
                handled = DecodeTouch(ref m);
                break;
            default:
                handled = false;
                break;
        }

        // WndProcのデフォルト処理
        base.WndProc(ref m);

        if (handled) {
            // イベントが発生した
            m.Result = new System.IntPtr(1);
        }
    }

    private static int LoWord(int number) {
        return (number & 0xffff);
    }

    /// <summary>
    ///   WM_TOUCHメッセージをデコードする
    /// </summary>
    /// <param name="m">ウィンドウメッセージ</param>
    private bool DecodeTouch(ref Message m) {
        // タッチ回数
        int inputCount = LoWord(m.WParam.ToInt32());

        TOUCHINPUT[] inputs;
        inputs = new TOUCHINPUT[inputCount];

        // タッチ操作を取り出して構造体の配列に入れる
        if (!GetTouchInputInfo(m.LParam, inputCount, inputs, touchInputSize)) {
            // タッチ情報取得失敗
            return false;
        }

        // タッチ操作をハンドラにディスパッチする
        bool handled = false;
        for (int i = 0; i < inputCount; i++) {
            TOUCHINPUT ti = inputs[i];

            // タッチイベントをハンドラにアサインする
            EventHandler<WMTouchEventArgs> handler = null;
            if ((ti.dwFlags & TOUCHEVENTF_DOWN) != 0) {
                handler = TouchDown;
            } else if ((ti.dwFlags & TOUCHEVENTF_UP) != 0) {
                handler = TouchUp;
            } else if ((ti.dwFlags & TOUCHEVENTF_MOVE) != 0) {
                handler = TouchMove;
            }

            // メッセージパラメータをタッチイベント引数に変換してイベントを処理する
            if (handler != null) {
                // raw touchinputメッセージをタッチイベントに変換する
                WMTouchEventArgs te = new WMTouchEventArgs();

                // タッチイベントが発生した画面のXY位置を取得する
                te.ContactY = ti.cyContact/100;
                te.ContactX = ti.cxContact/100;
                te.Id = ti.dwID;

                Point pt = PointToClient(new Point(ti.x/100, ti.y/100));
                te.LocationX = pt.X;
                te.LocationY = pt.Y;

                te.Time = ti.dwTime;
                te.Mask = ti.dwMask;
                te.Flags = ti.dwFlags;

                // イベントハンドラを呼び出す
                handler(this, te);

                // このイベントを処理済としてマークする
                handled = true;
            }
        }

        CloseTouchInputHandle(m.LParam);

        return handled;
    }

}


// タッチハンドラに渡すイベント変数
public class WMTouchEventArgs : System.EventArgs {
    // プライベートメンバ変数
    private int x;                  // タッチX位置
    private int y;                  // タッチY位置
    private int id;                 // ID
    private int mask;               // マスク
    private int flags;              // フラグ
    private int time;               // タッチイベントタイム
    private int contactX;           // 接触領域のXサイズ
    private int contactY;           // 接触領域のYサイズ

    public int LocationX {
        get { return x; }
        set { x = value; }
    }
    public int LocationY {
        get { return y; }
        set { y = value; }
    }
    public int Id {
        get { return id; }
        set { id = value; }
    }
    public int Flags {
        get { return flags; }
        set { flags = value; }
    }
    public int Mask {
        get { return mask; }
        set { mask = value; }
    }
    public int Time {
        get { return time; }
        set { time = value; }
    }
    public int ContactX {
        get { return contactX; }
        set { contactX = value; }
    }
    public int ContactY {
        get { return contactY; }
        set { contactY = value; }
    }
    public bool IsPrimaryContact {
        get { return (flags & WMTouchControl.TOUCHEVENTF_PRIMARY) != 0; }
    }

    // コンストラクタ
    public WMTouchEventArgs() {
    }
}

} // End of namespace
