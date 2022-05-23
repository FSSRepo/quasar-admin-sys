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
    /// Lógica de interacción para MessageDialog.xaml
    /// </summary>
    
    public partial class MessageAppDialog : UserControl
    {
        public MessageAppDialog(string title,string message)
        {
            InitializeComponent();
            Title.Text = title;
            Message.Text = message;
        }
        
    }
}
