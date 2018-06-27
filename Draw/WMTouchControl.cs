/// WMTouchControl: �^�b�`�p�l���Ή��̃R���g���[�����W���[��.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

// This code was designed and coded by YAGI H. (COSMOSTORK Inc.)

/*
 * ����: Windows10���Â�OS�ł͓��삵�܂���B
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
            // �}���`�^�b�`���E�B���h�E�ɓo�^�ł��Ȃ��ꍇ��O�𔭐�
            if (!RegisterTouchWindow(this.Handle, 0)) {
                throw new InvalidOperationException();
            }
        }
        catch (Exception) {
            // RegisterTouchWindow API���g�p�ł��Ȃ�
            throw;
        }
        touchInputSize = Marshal.SizeOf(new TOUCHINPUT());
    }

    // �^�b�`�C�x���g�n���h��
    protected event EventHandler<WMTouchEventArgs> TouchDown;
    protected event EventHandler<WMTouchEventArgs> TouchUp;
    protected event EventHandler<WMTouchEventArgs> TouchMove;

    // �^�b�`�C�x���g�E�B���h�E���b�Z�[�W�萔 [winuser.h]
    public const int WM_TOUCH = 0x0240;

    // �^�b�`�C�x���g�t���O ((TOUCHINPUT.dwFlags) [winuser.h]
    public const int TOUCHEVENTF_MOVE = 0x0001;
    public const int TOUCHEVENTF_DOWN = 0x0002;
    public const int TOUCHEVENTF_UP = 0x0004;
    public const int TOUCHEVENTF_INRANGE = 0x0008;
    public const int TOUCHEVENTF_PRIMARY = 0x0010;
    public const int TOUCHEVENTF_NOCOALESCE = 0x0020;
    public const int TOUCHEVENTF_PEN = 0x0040;

    // �^�b�`�C���v�b�g�}�X�N�ϐ� (TOUCHINPUT.dwMask) [winuser.h]
    public const int TOUCHINPUTMASKF_TIMEFROMSYSTEM = 0x0001;
    public const int TOUCHINPUTMASKF_EXTRAINFO = 0x0002;
    public const int TOUCHINPUTMASKF_CONTACTAREA = 0x0004;

    // �^�b�`API��`�̍\���� [winuser.h]
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

    // �^�b�`�C���v�b�g�\���̂̃T�C�Y
    private int touchInputSize;

    /// <summary>
    ///   �C�x���g�n���h����o�^����
    /// </summary>
    /// <param name="sender">�Z���_�[</param>
    /// <param name="e">�C�x���g����</param>
    private void OnLoadHandler(Object sender, EventArgs e) {
        try {
            // �}���`�^�b�`���E�B���h�E�ɓo�^�ł��Ȃ��ꍇ��O�𔭐�
            if (!RegisterTouchWindow(this.Handle, 0)) {
                throw new InvalidOperationException();
            }
        } catch (Exception) {
            // RegisterTouchWindow API���g�p�ł��Ȃ�
            throw;
        }
    }

    /// <summary>
    ///   WM_TOUCH���b�Z�[�W����������
    /// </summary>
    /// <param name="m">�E�B���h�E���b�Z�[�W</param>
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

        // WndProc�̃f�t�H���g����
        base.WndProc(ref m);

        if (handled) {
            // �C�x���g����������
            m.Result = new System.IntPtr(1);
        }
    }

    private static int LoWord(int number) {
        return (number & 0xffff);
    }

    /// <summary>
    ///   WM_TOUCH���b�Z�[�W���f�R�[�h����
    /// </summary>
    /// <param name="m">�E�B���h�E���b�Z�[�W</param>
    private bool DecodeTouch(ref Message m) {
        // �^�b�`��
        int inputCount = LoWord(m.WParam.ToInt32());

        TOUCHINPUT[] inputs;
        inputs = new TOUCHINPUT[inputCount];

        // �^�b�`��������o���č\���̂̔z��ɓ����
        if (!GetTouchInputInfo(m.LParam, inputCount, inputs, touchInputSize)) {
            // �^�b�`���擾���s
            return false;
        }

        // �^�b�`������n���h���Ƀf�B�X�p�b�`����
        bool handled = false;
        for (int i = 0; i < inputCount; i++) {
            TOUCHINPUT ti = inputs[i];

            // �^�b�`�C�x���g���n���h���ɃA�T�C������
            EventHandler<WMTouchEventArgs> handler = null;
            if ((ti.dwFlags & TOUCHEVENTF_DOWN) != 0) {
                handler = TouchDown;
            } else if ((ti.dwFlags & TOUCHEVENTF_UP) != 0) {
                handler = TouchUp;
            } else if ((ti.dwFlags & TOUCHEVENTF_MOVE) != 0) {
                handler = TouchMove;
            }

            // ���b�Z�[�W�p�����[�^���^�b�`�C�x���g�����ɕϊ����ăC�x���g����������
            if (handler != null) {
                // raw touchinput���b�Z�[�W���^�b�`�C�x���g�ɕϊ�����
                WMTouchEventArgs te = new WMTouchEventArgs();

                // �^�b�`�C�x���g������������ʂ�XY�ʒu���擾����
                te.ContactY = ti.cyContact/100;
                te.ContactX = ti.cxContact/100;
                te.Id = ti.dwID;

                Point pt = PointToClient(new Point(ti.x/100, ti.y/100));
                te.LocationX = pt.X;
                te.LocationY = pt.Y;

                te.Time = ti.dwTime;
                te.Mask = ti.dwMask;
                te.Flags = ti.dwFlags;

                // �C�x���g�n���h�����Ăяo��
                handler(this, te);

                // ���̃C�x���g�������ςƂ��ă}�[�N����
                handled = true;
            }
        }

        CloseTouchInputHandle(m.LParam);

        return handled;
    }

}


// �^�b�`�n���h���ɓn���C�x���g�ϐ�
public class WMTouchEventArgs : System.EventArgs {
    // �v���C�x�[�g�����o�ϐ�
    private int x;                  // �^�b�`X�ʒu
    private int y;                  // �^�b�`Y�ʒu
    private int id;                 // ID
    private int mask;               // �}�X�N
    private int flags;              // �t���O
    private int time;               // �^�b�`�C�x���g�^�C��
    private int contactX;           // �ڐG�̈��X�T�C�Y
    private int contactY;           // �ڐG�̈��Y�T�C�Y

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

    // �R���X�g���N�^
    public WMTouchEventArgs() {
    }
}

} // End of namespace
