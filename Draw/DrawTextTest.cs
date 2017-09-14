using System;
using System.Windows.Forms;

static class DrawTextTest {

    [STAThread]
    static void Main() {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new DrawTextTestForm());
    }
    
}
