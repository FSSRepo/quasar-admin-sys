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
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;

namespace QuasarAdminSys
{
    /// <summary>
    /// Lógica de interacción para LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        
        public LoginScreen()
        {
            InitializeComponent();
        }

        private void onLoadedWindow(object sender,RoutedEventArgs args)
        {
            Client.init();
            Client.instance.OnReceived += OnReceived;
            Client.instance.OnError += OnError;
            etPassword.PasswordChanged += login;
        }

        private  void login(object sender, RoutedEventArgs args)
        {
            if (Client.instance.begin(ConstantsResponse.CLT_LOGIN))
            {
                Client.instance.output.writeString(etToken.Text);
                Client.instance.output.writeString(etPassword.Password);
                Client.instance.end();
            } else
            {
                serverFailed("No es posible consultar al servidor");
            }
        }

        private async void CloseBtn(object sender, EventArgs args)
        {
            await DialogHost.Show(new CloseAppDialog(), "RootDialog");
        }

        private  void minimizeBtn(object sender, EventArgs args)
        {
            WindowState = WindowState.Minimized;
        }

        private  void performDrag(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private void OnReceived(object sender, ClientData data)
        {
            this.Dispatcher.Invoke(() => {
                loginResult(data);

            });        
        }

        private void OnError(object sender, ClientData data)
        {
            this.Dispatcher.Invoke(() => {
                serverFailed(data.error);
            });
        }

        private async void serverFailed(string text)
        {
            await DialogHost.Show(new MessageAppDialog("Error de servidor", text), "RootDialog");
        }

        public async void loginResult(ClientData data)
        {
            TextFieldAssist.SetUnderlineBrush(etToken, Brushes.Gray);
            HintAssist.SetHelperText(etToken, "");
            TextFieldAssist.SetUnderlineBrush(etPassword, Brushes.Gray);
            HintAssist.SetHelperText(etPassword, "");
            switch (data.responseID)
            {
                case ConstantsResponse.SRV_INVALID_TOKEN:
                    MaterialDesignThemes.Wpf.TextFieldAssist.SetUnderlineBrush(etToken, Brushes.Red);
                    MaterialDesignThemes.Wpf.HintAssist.SetHelperText(etToken, "Token invalido"); break;
                case ConstantsResponse.SRV_INVALID_PASSWORD:
                    MaterialDesignThemes.Wpf.TextFieldAssist.SetUnderlineBrush(etPassword, Brushes.Red);
                    MaterialDesignThemes.Wpf.HintAssist.SetHelperText(etPassword, "Contraseña incorrecta");
                    break;
                case ConstantsResponse.SRV_NACCESS_MASTER:
                    await DialogHost.Show(new MessageAppDialog("Error al iniciar sesion", "Se necesita acceder desde una cuenta master."), "RootDialog");
                    etPassword.Password = "";
                    etToken.Text = "";
                    break;
                case ConstantsResponse.SRV_LOGIN_COMPLETED:
                    SystemMainWindow sys = new SystemMainWindow();
                    sys.name = data.buffer.readString();
                    sys.lastname = data.buffer.readString();
                    sys.token = etToken.Text;
                    Client.instance.OnError -= OnError;
                    Client.instance.OnReceived -= OnReceived;
                    sys.Show();
                    Close();
                    break;
            }
        }
    }
}
