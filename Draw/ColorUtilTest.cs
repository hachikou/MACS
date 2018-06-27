/// ColorUtilTest: ColorUtilのテストハンドラ.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

static class ColorUtilTest {

    [STAThread]
    static void Main() {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ColorUtilTestForm());
    }

}
