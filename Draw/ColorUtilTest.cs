using System;
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
