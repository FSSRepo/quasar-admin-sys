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
    /// Lógica de interacción para SaveChanges.xaml
    /// </summary>
    public partial class SaveChanges : UserControl
    {
        SystemMainWindow sys;

        public SaveChanges(SystemMainWindow system)
        {
            InitializeComponent();
            sys = system;
        }

        public void accept(object sender, EventArgs args)
        {
            if (sys.modify_product)
            {
                sys.resetProductView();
                sys.modify_product = false;
                sys.changeProductSelect();
                MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
            }
            else
            {
                sys.resetUserView();
                sys.modify_user = false;
                sys.addingNewUser = false;
                sys.changeUserSelect();
                MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
            }
        }

        public void cancel(object sender, EventArgs args)
        {
            if (!sys.modify_product)
            {
                sys.setCurrentUser();
            }
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
