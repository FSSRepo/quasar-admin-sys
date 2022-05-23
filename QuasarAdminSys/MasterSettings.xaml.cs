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
    /// Lógica de interacción para MasterSettings.xaml
    /// </summary>
    public partial class MasterSettings : UserControl
    {
        SystemMainWindow sys;

        public MasterSettings(SystemMainWindow sys)
        {
            InitializeComponent();
            this.sys = sys;
            etName.Text = this.sys.name;
            etLastName.Text = this.sys.lastname;
            etToken.Text = this.sys.token;
        }

        public void appplyChanges(object sender, EventArgs args)
        {
            if(etName.Text.Length == 0)
            {
                MaterialDesignThemes.Wpf.TextFieldAssist.SetUnderlineBrush(etName, Brushes.Red);
                MaterialDesignThemes.Wpf.HintAssist.SetHelperText(etName, "Ingrese un nombre");
                return;
            }
            if (etLastName.Text.Length == 0)
            {
                MaterialDesignThemes.Wpf.TextFieldAssist.SetUnderlineBrush(etLastName, Brushes.Red);
                MaterialDesignThemes.Wpf.HintAssist.SetHelperText(etLastName, "Ingrese un apellido");
                return;
            }
            if (etToken.Text.Length == 0)
            {
                MaterialDesignThemes.Wpf.TextFieldAssist.SetUnderlineBrush(etToken, Brushes.Red);
                MaterialDesignThemes.Wpf.HintAssist.SetHelperText(etToken, "Ingrese un token valido");
                return;
            }
            if (etPassword.Password.Length > 0 && etPassword.Password.Length < 8)
            {
                MaterialDesignThemes.Wpf.TextFieldAssist.SetUnderlineBrush(etPassword, Brushes.Red);
                MaterialDesignThemes.Wpf.HintAssist.SetHelperText(etPassword, "Al menos 8 caracteres");
                return;
            }
            Client inst = Client.instance;
            inst.begin(ConstantsResponse.CLT_APPLY_MASTER);
            inst.output.writeString(etName.Text);
            inst.output.writeString(etLastName.Text);
            inst.output.writeString(etToken.Text);
            inst.output.writeString(etPassword.Password);
            inst.end();
            sys.name = etName.Text;
            sys.lastname = etLastName.Text;
            sys.token = etToken.Text;
            sys.updateName();
            System.Threading.Thread.Sleep(50);
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null,null);
            sys.updateUser();
        }
    }
}
