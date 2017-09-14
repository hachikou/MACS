using System;
using System.Windows.Forms;

static class MPWidgetTest {

    [STAThread]
    static void Main() {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MPWidgetTestForm());
    }
    
}
