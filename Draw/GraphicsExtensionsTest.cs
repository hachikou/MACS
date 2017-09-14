using System;
using System.Windows.Forms;

static class GraphicsExtensionsTest {

    [STAThread]
    static void Main() {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new GraphicsExtensionsTestForm());
    }
    
}
