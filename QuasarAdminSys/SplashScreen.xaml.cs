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
using System.Threading;
namespace QuasarAdminSys
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Thread thread = new Thread(new ThreadStart(simulateProgress));
            thread.Start();
        }

        private void simulateProgress()
        {
            float p = 0;
            for (; p < 100; p++)
            {
                this.Dispatcher.BeginInvoke(new Action(() => {
                    rectProg.Width = (int)((p / 100f) * 700);
                }));
                Thread.Sleep(40);
            }
            this.Dispatcher.BeginInvoke(new Action(() => {
                new LoginScreen().Show();
                Close();
            }));
        }
    }
}
