using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace QuasarAdminSys
{
    /// <summary>
    /// Lógica de interacción para CloseAppDialog.xaml
    /// </summary>
    public partial class CloseAppDialog : UserControl
    {
        
        public CloseAppDialog()
        {
            InitializeComponent();
        }
        public void acceptClose(object sender, EventArgs args)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
