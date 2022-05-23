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

namespace QuasarAdminSys
{
    /// <summary>
    /// Lógica de interacción para LogoutDialog.xaml
    /// </summary>
    public partial class LogoutDialog : UserControl
    {
       
        public LogoutDialog()
        {
            InitializeComponent();
        }

        public void acceptLogout(object sender, EventArgs args)
        {
            Client.instance.begin(ConstantsResponse.CLT_LOGOUT);
            Client.instance.end();
        }
    }
}
