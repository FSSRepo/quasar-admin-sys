using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using MaterialDesignThemes.Wpf;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Configurations;

namespace QuasarAdminSys
{
    /// <summary>
    /// Lógica de interacción para SystemMainWindow.xaml
    /// </summary>
    /// 
    public partial class SystemMainWindow : Window
    {
        List<OrderItem> orders;
        List<UserItem> users;
        List<ProductItem> produts;
        List<AnalyticsItem> analytics_list;
        List<ProdOrderItem> prod_order;

        public string name, lastname, token;
        OrderItem current_order;
        UserItem current_user;
        ProductItem current_product;
        public bool modify_user = false,modify_product = false;
        public bool update_order_by_user = false;
        public bool addingNewUser = false;

        public SeriesCollection orderCollection { set; get; }
        public SeriesCollection sexCollection { set; get; }
        public SeriesCollection agecollection { set; get; }
        public Func<double,string> ChartDateFormat { set; get; }
        public List<ProductItem> top_products;

        public DateTime StartDateChart { set; get; }
        public DateTime EndDateChart { set; get; }

        public SystemMainWindow() {
            InitializeComponent();
        }

        private void onWindowLoaded(object sender, RoutedEventArgs args) {
            DataContext = this;
            Client.instance.OnReceived += OnReceived;
            Client.instance.OnError += OnError;
            orders = new List<OrderItem>();
            users = new List<UserItem>();
            produts = new List<ProductItem>();
            prod_order = new List<ProdOrderItem>();
            analytics_list = new List<AnalyticsItem>();
            top_products = new List<ProductItem>();
            agecollection = new SeriesCollection();
            sexCollection = new SeriesCollection();
            var dayConfig = Mappers.Xy<ChartModel>()
                           .X(dayModel => dayModel.DateTime.Ticks)
                           .Y(dayModel => dayModel.Value);
            orderCollection = new SeriesCollection(dayConfig);
            dpFrom.SelectedDate = DateTime.Now.AddDays(-7);
            dpTo.SelectedDate = DateTime.Now;
            updateOrder();
            Thread.Sleep(60);
            updateUser();
            Thread.Sleep(60);
            updateProduct();
            Thread.Sleep(60);
            dpFrom.SelectedDateChanged += DatePickerChanged;
            dpTo.SelectedDateChanged += DatePickerChanged;
            Closing += closing;
            updateName();
            analytics_list.Add(new AnalyticsItem() { AnalyticName = "Pedidos" });
            analytics_list.Add(new AnalyticsItem() { AnalyticName = "Top Productos" });
            analytics_list.Add(new AnalyticsItem() { AnalyticName = "Personas" });
            lvAnalytics.ItemsSource = analytics_list;
            cbRange.SelectionChanged += OnChangeAnalyticsDate;
            dpAnalyticsFrom.SelectedDate = DateTime.Now.AddDays(-7);
            dpAnalyticsTo.SelectedDate = DateTime.Now;
            dpAnalyticsFrom.SelectedDateChanged += DatePickerAnalyticsChanged;
            dpAnalyticsTo.SelectedDateChanged += DatePickerAnalyticsChanged;
            lvAnalytics.SelectedItem = analytics_list[0];
            updateAnalytics();
        }

        public void updateName()
        {
            txtTitle.Content = "QAS - Control Panel [" + name+ " " +lastname+"]";
            txtNameApp.Content = name + " " + lastname;
        }
        
        private void closing(object sender, CancelEventArgs args)
        {
            Process.GetCurrentProcess().Kill();
        }

        private async void CloseBtn(object sender, EventArgs args)
        {
            await DialogHost.Show(new CloseAppDialog(), "RootDialog");
        }
        private void minimizeBtn(object sender, EventArgs args)
        {
            WindowState = WindowState.Minimized;
        }

        private void deleteOrderButton(object sender, EventArgs args)
        {
            Client.instance.begin(ConstantsResponse.CLT_DELETE_ORDER);
            Client.instance.output.writeString(current_order.OrderID);
            Client.instance.end();
            current_order = null;
            gdDetailsOrder.Visibility = Visibility.Hidden;
        }

        private void deleteUserBtn(object sender, EventArgs args)
        {
            Client.instance.begin(ConstantsResponse.CLT_DELETE_USER);
            Client.instance.output.writeString(current_user.UserID);
            Client.instance.end();
            resetUserView();
            current_user = null;
            gdEditUserView.Visibility = Visibility.Hidden;
            modify_user = false;
        }

        private void createNewUserBtn(object sender, EventArgs args)
        {
            if (addingNewUser)
            {
                return;
            }
            gdEditUserView.Visibility = Visibility.Visible;
            resetUserView();
            modify_user = true;
            clearEditTextUser();
            current_user = null;
            addingNewUser = true;
            btnUserDelete.Visibility = Visibility.Hidden;
            btnUserCancel.Visibility = Visibility.Visible;
        }

        private void cancelNewUser(object sender, EventArgs args)
        {
            addingNewUser = false;
            modify_user = false;
            gdEditUserView.Visibility = Visibility.Hidden;
            btnUserCancel.Visibility = Visibility.Hidden;
            btnUserDelete.Visibility = Visibility.Visible;
        }

        private void performDrag(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private void OnOrderListClick(object sender, MouseButtonEventArgs e)
        {
            if (lvOrders.SelectedItem != null)
            {
                current_order = (OrderItem)lvOrders.SelectedItem;
                Client.instance.begin(ConstantsResponse.CLT_SELECT_ORDER);
                Client.instance.output.writeString(current_order.OrderID);
                Client.instance.end();
            }
        }

        private async void OnUserListClick(object sender, MouseButtonEventArgs e)
        {
            if (lvUsers.SelectedItem != null)
            {
                if(current_user != null && ((UserItem)lvUsers.SelectedItem) == current_user)
                {
                    return;
                }
                if (((UserItem)lvUsers.SelectedItem).master)
                {
                    await DialogHost.Show(new MessageAppDialog("Acceso denegado","Solo el usuario propio puede modificar su informacion."), "RootDialog");
                    if (current_user != null)
                    {
                        lvUsers.SelectedItem = current_user;
                    }
                    return;
                }
                if (modify_user)
                {
                    await DialogHost.Show(new SaveChanges(this), "RootDialog");
                    return;
                }
                changeUserSelect();
            }
        }

        private async void OnProductListClick(object sender, MouseButtonEventArgs e)
        {
            if (lvProducts.SelectedItem != null)
            {
                gdProductDetails.Visibility = Visibility.Visible;
                if (current_product != null && ((ProductItem)lvProducts.SelectedItem).productID == current_product.productID)
                {
                    return;
                }
                if (modify_product)
                {
                    await DialogHost.Show(new SaveChanges(this), "RootDialog");
                    return;
                }
                changeProductSelect();
            }
        }

        private void OnAnalyticsListClick(object sender, MouseButtonEventArgs e)
        {
            if (lvAnalytics.SelectedItem != null)
            {
                switch (lvAnalytics.SelectedIndex)
                {
                    case 0:
                        {
                            graphPedidos.Visibility = Visibility.Visible;
                            cardAge.Visibility = Visibility.Collapsed;
                            cardSex.Visibility = Visibility.Collapsed;
                            cardTop1.Visibility = Visibility.Collapsed;
                            cardTop210.Visibility = Visibility.Collapsed;
                            AnalyticsTitle.Content = "Historial de pedidos";
                        }
                        break;
                    case 1:
                        {
                            cardAge.Visibility = Visibility.Collapsed;
                            cardSex.Visibility = Visibility.Collapsed;
                            graphPedidos.Visibility = Visibility.Collapsed;
                            cardTop1.Visibility = Visibility.Visible;
                            cardTop210.Visibility = Visibility.Visible;
                            AnalyticsTitle.Content = "Top de productos mas vendidos";
                        }
                        break;
                    case 2:
                        {
                            cardAge.Visibility = Visibility.Visible;
                            cardSex.Visibility = Visibility.Visible;
                            graphPedidos.Visibility = Visibility.Collapsed;
                            cardTop1.Visibility = Visibility.Collapsed;
                            cardTop210.Visibility = Visibility.Collapsed;
                            AnalyticsTitle.Content = "Informacion del cliente";
                        }
                        break;
                }
            }
        }

        public void changeProductSelect()
        {
            current_product = (ProductItem)lvProducts.SelectedItem;
            Client.instance.begin(ConstantsResponse.CLT_SELECT_PRODUCT);
            Client.instance.output.writeString(current_product.productID);
            Client.instance.end();
        }

        public void changeUserSelect()
        {
            current_user = (UserItem)lvUsers.SelectedItem;
            Client.instance.begin(ConstantsResponse.CLT_SELECT_USER);
            Client.instance.output.writeString(current_user.UserID);
            Client.instance.end();
        }

        private void OnChangeOrderStatus(object sender, SelectionChangedEventArgs args)
        {
            Client.instance.begin(ConstantsResponse.CLT_ORD_STATUS);
            Client.instance.output.writeString(current_order.OrderID);
            switch (cbStatus.SelectedIndex)
            {
                case 0: Client.instance.output.writeInt(-1); break;
                case 1: Client.instance.output.writeInt(0); break;
                case 2: Client.instance.output.writeInt(1); break;
                case 3: Client.instance.output.writeInt(2); break;
            }
            Client.instance.end();
        }

        private void OnChangeUserValue(object sender, RoutedEventArgs args)
        {
            modify_user = true;
        }

        private void OnChangeSex(object sender, SelectionChangedEventArgs args) {
            modify_user = true;
        }

        private void OnChangeProductValue(object sender, RoutedEventArgs args) {
            modify_product = true;
        }

        private void OnChangeCategory(object sender, SelectionChangedEventArgs args)
        {
            modify_product = true;
        }

        private void DatePickerChanged(object sender, SelectionChangedEventArgs args)
        {
            updateOrder();
        }

        private void DatePickerAnalyticsChanged(object sender, SelectionChangedEventArgs args)
        {
            updateAnalytics();
        }

        private void OnChangeAnalyticsDate(object sender, SelectionChangedEventArgs args)
        {
            dpAnalyticsFrom.SelectedDateChanged -= DatePickerAnalyticsChanged;
            dpAnalyticsTo.SelectedDateChanged -= DatePickerAnalyticsChanged;
            switch (cbRange.SelectedIndex)
            {
                case 0: // hace 7 dias
                    dpAnalyticsFrom.SelectedDate = DateTime.Now.AddDays(-7);
                    dpAnalyticsTo.SelectedDate = DateTime.Now;
                    break;
                case 1: // hace 15 dias
                    dpAnalyticsFrom.SelectedDate = DateTime.Now.AddDays(-15);
                    dpAnalyticsTo.SelectedDate = DateTime.Now;
                    break;
                case 2: // hace 1 mes
                    dpAnalyticsFrom.SelectedDate = DateTime.Now.AddDays(-30);
                    dpAnalyticsTo.SelectedDate = DateTime.Now;
                    break;
            }
            dpAnalyticsFrom.SelectedDateChanged += DatePickerAnalyticsChanged;
            dpAnalyticsTo.SelectedDateChanged += DatePickerAnalyticsChanged;
            updateAnalytics();
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
        private void OnReceived(object sender, ClientData data)
        {
            this.Dispatcher.Invoke(() => {
                switch (data.responseID)
                {
                    case ConstantsResponse.SRV_ORDER_LIST:
                        {
                            orders.Clear();
                            lvOrders.ItemsSource = null;
                            int count = data.buffer.readInt();
                            for (int i = 0; i < count;i++)
                            {
                                OrderItem itm = new OrderItem();
                                itm.OrderID = data.buffer.readString();
                                itm.InvoiceID = data.buffer.readString();
                                itm.OrderName = "Pedido [" + itm.OrderID + "] - " + data.buffer.readString();
                                itm.DateTime = data.buffer.readString();
                                switch (data.buffer.readInt())
                                {
                                    case -1:
                                        itm.State = "Fallido";
                                        itm.SColor = new SolidColorBrush(Colors.DarkRed);
                                        break;
                                    case 0:
                                        itm.State = "Sin revisar";
                                        itm.SColor = new SolidColorBrush(Colors.DarkGray);
                                        break;
                                    case 1:
                                        itm.State = "Procesando";
                                        itm.SColor = new SolidColorBrush(Colors.DarkBlue);
                                        break;
                                    case 2:
                                        itm.State = "Entregado";
                                        itm.SColor = new SolidColorBrush(Colors.DarkGreen);
                                        break;
                                }
                                orders.Add(itm);
                            }
                            lvOrders.ItemsSource = orders;
                        }
                        break;
                    case ConstantsResponse.SRV_LOGOUT_COMPLETED:
                        new LoginScreen().Show();
                        Closing -= closing;
                        Close();
                        break;
                    case ConstantsResponse.SRV_SELECT_ORDER:
                        {
                            gdDetailsOrder.Visibility = Visibility.Visible;
                            string client = data.buffer.readString();
                            string total_price = data.buffer.readString();
                            int status = data.buffer.readInt();
                            int total_products = data.buffer.readInt();
                            txtDetailsOrder.Content = 
                            "ID del pedido: "+current_order.OrderID+"\n"+
                            "Fecha del pedido: "+current_order.DateTime+"\n"+
                            "Id de factura: "+current_order.InvoiceID+"\n"+
                            "Cliente: "+ client+"\n"+
                            "Total de productos: "+ total_products+"\n"+
                            "Precio Total: " +total_price;
                            cbStatus.SelectionChanged -= OnChangeOrderStatus;
                            switch (status)
                            {
                                case -1: cbStatus.SelectedIndex = 0; break;
                                case 0: cbStatus.SelectedIndex = 1; break;
                                case 1: cbStatus.SelectedIndex = 2; break;
                                case 2: cbStatus.SelectedIndex = 3; break;
                            }
                            cbStatus.SelectionChanged += OnChangeOrderStatus;
                            prod_order.Clear();
                            for (int i = 0;i < total_products;i++)
                            {
                                ProdOrderItem itm = new ProdOrderItem();
                                itm.Count = data.buffer.readInt()+"";
                                itm.Name = data.buffer.readString();
                                prod_order.Add(itm);
                            }
                            dgProductOrder.ItemsSource = null;
                            dgProductOrder.ItemsSource = prod_order;
                            updateOrder();
                        }
                        break;
                    case ConstantsResponse.SRV_UPDATE_ORDER:
                        updateOrder();
                        break;
                    case ConstantsResponse.SRV_UPDATE_USER:
                        update_order_by_user = data.buffer.readByte() == 0xD;
                        updateUser();
                        break;
                    case ConstantsResponse.SRV_UPDATE_PRODUCT:
                        updateProduct();
                        break;
                    case ConstantsResponse.SRV_ACCOUNT_LIST:
                        {
                            users.Clear();
                            lvUsers.ItemsSource = null;
                            int count = data.buffer.readInt();
                            for (int i = 0; i < count; i++)
                            {
                                UserItem itm = new UserItem();
                                itm.UserID = data.buffer.readString();
                                string name = data.buffer.readString();
                                string lastname = data.buffer.readString();
                                itm.master = data.buffer.readBoolean();
                                itm.UserName = "[" + itm.UserID + "] " + name + " " + lastname + (itm.master ? " <master>" : "");
                                users.Add(itm);
                            }
                            lvUsers.ItemsSource = users;
                            if (update_order_by_user)
                            {
                                updateOrder();
                                gdDetailsOrder.Visibility = Visibility.Hidden;
                                update_order_by_user = false;
                            }
                        }
                            break;
                    case ConstantsResponse.SRV_SELECT_USER:
                        {
                            removeModifiers();
                            gdEditUserView.Visibility = Visibility.Visible;
                            etName.Text = data.buffer.readString();
                            etLastName.Text = data.buffer.readString();
                            etDirection.Text = data.buffer.readString();
                            etAge.Text =""+ data.buffer.readInt();
                            cbSex.SelectedIndex = data.buffer.readByte();
                            etCreditCard.Text = data.buffer.readString();
                            etEmail.Text = data.buffer.readString();
                            etToken.Text = data.buffer.readString();
                            btnUserCancel.Visibility = Visibility.Hidden;
                            btnUserDelete.Visibility = Visibility.Visible;
                            asignModifiers();
                        }
                        break;
                    case ConstantsResponse.SRV_PRODUCT_LIST:
                        {
                            produts.Clear();
                            lvProducts.ItemsSource = null;
                            int count = data.buffer.readInt();
                            for (int i = 0; i < count; i++)
                            {
                                ProductItem itm = new ProductItem();
                                itm.productID = data.buffer.readString();
                                string name = data.buffer.readString();
                                string res_img = data.buffer.readString();
                                itm.ProductName = "[" + itm.productID + "] " + name;
                                itm.ResImage = "resources/" + res_img + ".png";
                                produts.Add(itm);
                            }
                            lvProducts.ItemsSource = produts;
                        }

                        break;
                    case ConstantsResponse.SRV_SELECT_PRODUCT:
                        {
                            removeProductModifiers();
                            gdProductDetails.Visibility = Visibility.Visible;
                            etProductName.Text = data.buffer.readString();
                            ivProduct.Source = GetImageResource( data.buffer.readString()+".png");
                            int category = data.buffer.readInt();
                            cbCategory.SelectedIndex = category - 1;
                            etProductPrice.Text = "" + data.buffer.readFloat();
                            int status = data.buffer.readInt();
                            cbProductStatus.SelectedIndex = status + 1;
                            btnProductCancel.Visibility = Visibility.Hidden;
                            asignProductModifiers();
                        }
                        break;
                    case ConstantsResponse.SRV_ORDER_POINTS:
                        {
                            ChartValues<ChartModel> vals = new ChartValues<ChartModel>();
                            int count = data.buffer.readInt();
                            for (int i = 0;i < count; i++)
                            {
                                DateTime date_temp = DateTime.Parse(data.buffer.readString());
                                int order_count = data.buffer.readInt(); 
                                vals.Add(new ChartModel(date_temp,order_count));
                            }
                            orderCollection.Clear();
                            orderCollection.Add(new LineSeries()
                            {
                                Title = "Pedidos",
                                Values = vals
                            }); 
                        }
                        break;
                    case ConstantsResponse.SRV_TOP_PRODUCT:
                        {
                            int count = data.buffer.readInt();
                            if(count > 0)
                            {
                                top1ProductName.Content = data.buffer.readString();
                                ivTop1Product.Source = GetImageResource(data.buffer.readString() + ".png");
                                top1ProductBuys.Content = "Ventas del periodo: "+data.buffer.readInt();
                                top_products.Clear();
                                for(int i = 1; i < count; i++)
                                {
                                    string name = data.buffer.readString();
                                    string res = data.buffer.readString();
                                    int count_ = data.buffer.readInt();
                                    top_products.Add(new ProductItem()
                                    {
                                        ProductName = "Top "+(i+1)+": "+name+" ("+count_+")",
                                        ResImage = "resources/"+  res + ".png"
                                    });
                                }

                                lvTopProducts.ItemsSource = null;
                                lvTopProducts.ItemsSource = top_products;
                            }
                        }
                        break;
                    case ConstantsResponse.SRV_ORDER_NOTIFY:
                        Snack.MessageQueue?.Enqueue(data.buffer.readString(), null, null, null, false, true, TimeSpan.FromSeconds(5));
                        updateOrder();
                        break;
                    case ConstantsResponse.SRV_PERSON_ANALYTICS:
                        {
                            int 
                            womens = data.buffer.readInt(),
                            mens = data.buffer.readInt(),
                            e18_20 = data.buffer.readInt(), 
                            e21_30 = data.buffer.readInt(), 
                            e31_50 = data.buffer.readInt(), 
                            e51_70 = data.buffer.readInt();
                            sexCollection.Clear();
                            agecollection.Clear();
                            sexCollection.Add(new PieSeries()
                            {
                                Title = "Hombres",
                                DataLabels = true,
                                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation),
                                Values = new ChartValues<int> { mens }
                            });
                            sexCollection.Add(new PieSeries()
                            {
                                Title = "Mujeres",
                                DataLabels = true,
                                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation),
                                Values = new ChartValues<int> { womens }
                            });
                            agecollection.Add(new PieSeries()
                            {
                                Title = "18 - 20 años",
                                DataLabels = true,
                                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation),
                                Values = new ChartValues<int> { e18_20 }
                            });
                            agecollection.Add(new PieSeries()
                            {
                                Title = "21 - 30 años",
                                DataLabels = true,
                                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation),
                                Values = new ChartValues<int> { e21_30 }
                            });
                            agecollection.Add(new PieSeries()
                            {
                                Title = "31 - 50 años",
                                DataLabels = true,
                                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation),
                                Values = new ChartValues<int> { e31_50 }
                            });
                            agecollection.Add(new PieSeries()
                            {
                                Title = "51 - 70 años",
                                DataLabels = true,
                                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation),
                                Values = new ChartValues<int> { e51_70 }
                            });
                        }
                        break;
                    
                }
            });
        }

        private BitmapImage GetImageResource(string path)
        {
            return new BitmapImage(new Uri(@"pack://application:,,,/resources/" + path, UriKind.Absolute));
        }

        private void updateOrder()
        {
            Client.instance.begin(ConstantsResponse.CLT_ORDER_LIST);
            Client.instance.output.writeString(DateTime.Parse(dpFrom.SelectedDate.ToString()).ToString("yyyy-MM-dd 00:00:00"));
            Client.instance.output.writeString(DateTime.Parse(dpTo.SelectedDate.ToString()).ToString("yyyy-MM-dd 23:59:59"));
            Client.instance.end();
        }

        public void updateUser()
        {
            Client.instance.begin(ConstantsResponse.CLT_USER_LIST);
            Client.instance.end();
        }

        private void updateProduct()
        {
            Client.instance.begin(ConstantsResponse.CLT_PRODUCT_LIST);
            Client.instance.end();
        }

        private async void logout(object sender, EventArgs args)
        {
            await DialogHost.Show(new LogoutDialog(), "RootDialog");
        }

        private async void masterSettings(object sender, EventArgs args)
        {
            await DialogHost.Show(new MasterSettings(this), "RootDialog");
        }

        public void resetUserView()
        {
            TextFieldAssist.SetUnderlineBrush(etName, Brushes.Gray);
            TextFieldAssist.SetUnderlineBrush(etLastName, Brushes.Gray);
            TextFieldAssist.SetUnderlineBrush(etToken, Brushes.Gray);
            TextFieldAssist.SetUnderlineBrush(etCreditCard, Brushes.Gray);
            TextFieldAssist.SetUnderlineBrush(etAge, Brushes.Gray);
            TextFieldAssist.SetUnderlineBrush(etPassword, Brushes.Gray);
            TextFieldAssist.SetUnderlineBrush(etDirection, Brushes.Gray);
            HintAssist.SetHelperText(etName, "");
            HintAssist.SetHelperText(etAge, "");
            HintAssist.SetHelperText(etCreditCard, "");
            HintAssist.SetHelperText(etDirection, "");
            HintAssist.SetHelperText(etPassword, "");
            HintAssist.SetHelperText(etToken, "");
            HintAssist.SetHelperText(etLastName, "");
        }

        public void resetProductView()
        {
            TextFieldAssist.SetUnderlineBrush(etProductName, Brushes.Gray);
            TextFieldAssist.SetUnderlineBrush(etProductPrice, Brushes.Gray);
            HintAssist.SetHelperText(etProductName, "");
            HintAssist.SetHelperText(etProductPrice, "");
        }

        private void clearEditTextUser()
        {
            etName.Text = "";
            etLastName.Text = "";
            etCreditCard.Text = "0000-0000-0000-0000";
            etDirection.Text = "";
            etToken.Text = "";
            etAge.Text = "0";
            etPassword.Password = "";
            cbSex.SelectedIndex = -1;
        }


        public void setCurrentUser()
        {
            if (current_user != null)
            {
                lvUsers.SelectedItem = current_user;
            }
        }

        private void saveUserBtn(object sender, EventArgs args)
        {
            resetUserView();
            if (etName.Text.Length == 0)
            {
                TextFieldAssist.SetUnderlineBrush(etName, Brushes.Red);
                HintAssist.SetHelperText(etName, "Ingrese un nombre");
                return;
            }
            if (etLastName.Text.Length == 0)
            {
                TextFieldAssist.SetUnderlineBrush(etLastName, Brushes.Red);
               HintAssist.SetHelperText(etLastName, "Ingrese un apellido");
                return;
            }
            if (etDirection.Text.Length == 0)
            {
                TextFieldAssist.SetUnderlineBrush(etDirection, Brushes.Red);
                HintAssist.SetHelperText(etDirection, "Ingrese la direccion");
                return;
            }
            if (etAge.Text.Length == 0)
            {
                TextFieldAssist.SetUnderlineBrush(etAge, Brushes.Red);
                HintAssist.SetHelperText(etAge, "Ingrese la edad");
                return;
            }
            if (etCreditCard.Text.Length == 0)
            {
                TextFieldAssist.SetUnderlineBrush(etCreditCard, Brushes.Red);
                HintAssist.SetHelperText(etCreditCard, "Ingrese una tarjeta de credito");
                return;
            }
            if (etToken.Text.Length == 0)
            {
                TextFieldAssist.SetUnderlineBrush(etToken, Brushes.Red);
                HintAssist.SetHelperText(etToken, "Ingrese un token valido");
                return;
            }

            if (etPassword.Password.Length == 0 && addingNewUser)
            {
                TextFieldAssist.SetUnderlineBrush(etToken, Brushes.Red);
                HintAssist.SetHelperText(etToken, "Ingrese una contraseña (Al menos 8 characteres)");
                return;
            }

            if (etPassword.Password.Length > 0 && etPassword.Password.Length < 8)
            {
                TextFieldAssist.SetUnderlineBrush(etPassword, Brushes.Red);
                HintAssist.SetHelperText(etPassword, "Al menos 8 caracteres");
                return;
            }
            if(cbSex.SelectedIndex == -1)
            {
                cbSex.SelectedIndex = 0;
            }
            Client.instance.begin(
                addingNewUser ? ConstantsResponse.CLT_CREATE_USER : ConstantsResponse.CLT_UPDATE_USER);
            if (!addingNewUser)
            {
                Client.instance.output.writeString(current_user.UserID);
            }
            Client.instance.output.writeString(etName.Text);
            Client.instance.output.writeString(etLastName.Text);
            Client.instance.output.writeString(etDirection.Text);
            Client.instance.output.writeInt(int.Parse(etAge.Text));
            Client.instance.output.writeByte(cbSex.SelectedIndex);
            Client.instance.output.writeString(etCreditCard.Text);
            Client.instance.output.writeString(etEmail.Text);
            Client.instance.output.writeString(etToken.Text);
            Client.instance.output.writeString(etPassword.Password);
            Client.instance.end();
            addingNewUser = false;
            modify_user = false;
            btnUserCancel.Visibility = Visibility.Hidden;
            btnUserDelete.Visibility = Visibility.Visible;
        }

        private void saveProductBtn(object sender, EventArgs args)
        {
            resetProductView();
            if (etProductName.Text.Length == 0)
            {
                TextFieldAssist.SetUnderlineBrush(etProductName, Brushes.Red);
                HintAssist.SetHelperText(etProductName, "Ingrese un nombre");
                return;
            }
            if (etProductPrice.Text.Length == 0)
            {
                TextFieldAssist.SetUnderlineBrush(etProductPrice, Brushes.Red);
                HintAssist.SetHelperText(etProductPrice, "Ingrese un precio");
                return;
            }
            Client.instance.begin(ConstantsResponse.CLT_UPDATE_PRODUCT);
            Client.instance.output.writeString(current_product.productID);
            Client.instance.output.writeString(etProductName.Text);
            Client.instance.output.writeString("null");
            Client.instance.output.writeFloat(float.Parse(etProductPrice.Text));
            Client.instance.output.writeInt(cbCategory.SelectedIndex + 1);
            Client.instance.output.writeInt(cbProductStatus.SelectedIndex - 1);
            Client.instance.end();
            btnProductCancel.Visibility = Visibility.Hidden;
            modify_product = false;
        }

        private void asignModifiers()
        {
            etName.TextChanged += OnChangeUserValue;
            etLastName.TextChanged += OnChangeUserValue;
            etDirection.TextChanged += OnChangeUserValue;
            etPassword.PasswordChanged += OnChangeUserValue;
            etCreditCard.TextChanged += OnChangeUserValue;
            etAge.TextChanged += OnChangeUserValue;
            etToken.TextChanged += OnChangeUserValue;
            cbSex.SelectionChanged += OnChangeSex;
        }

        private void removeModifiers()
        {
            etName.TextChanged -= OnChangeUserValue;
            etLastName.TextChanged -= OnChangeUserValue;
            etDirection.TextChanged -= OnChangeUserValue;
            etPassword.PasswordChanged -= OnChangeUserValue;
            etCreditCard.TextChanged -= OnChangeUserValue;
            etAge.TextChanged -= OnChangeUserValue;
            etToken.TextChanged -= OnChangeUserValue;
            cbSex.SelectionChanged -= OnChangeSex;
        }

        private void asignProductModifiers()
        {
            etProductName.TextChanged += OnChangeProductValue;
            etProductPrice.TextChanged += OnChangeProductValue;
            cbCategory.SelectionChanged += OnChangeCategory;
        }
        private void removeProductModifiers()
        {
            etProductName.TextChanged -= OnChangeProductValue;
            etProductPrice.TextChanged -= OnChangeProductValue;
            cbCategory.SelectionChanged -= OnChangeCategory;
        }

        private void updateAnalytics()
        {
            //  pedidos
            graphPedidos.AxisX[0].MinValue = dpAnalyticsFrom.SelectedDate.Value.Ticks;
            graphPedidos.AxisX[0].MaxValue = dpAnalyticsTo.SelectedDate.Value.Ticks;
            ChartDateFormat = value => new DateTime((long)value).ToString("yyyy/MM/dd");
            Client.instance.begin(ConstantsResponse.CLT_GET_ORDER_POINTS);
            Client.instance.output.writeString(dpAnalyticsFrom.SelectedDate.ToString());
            Client.instance.output.writeString(dpAnalyticsTo.SelectedDate.ToString());
            Client.instance.end();
            Thread.Sleep(50);
            // top producto
            Client.instance.begin(ConstantsResponse.CLT_TOP_PRODUCT);
            Client.instance.output.writeString(dpAnalyticsFrom.SelectedDate.ToString());
            Client.instance.output.writeString(dpAnalyticsTo.SelectedDate.ToString());
            Client.instance.end();
            Thread.Sleep(50);
            // personas
            Client.instance.begin(ConstantsResponse.CLT_PERSON_ANALYTICS);
            Client.instance.output.writeString(dpAnalyticsFrom.SelectedDate.ToString());
            Client.instance.output.writeString(dpAnalyticsTo.SelectedDate.ToString());
            Client.instance.end();
        }
    }

    public class ChartModel
    {
        public DateTime DateTime { get; set; }
        public int Value { get; set; }

        public ChartModel(DateTime dateTime, int value)
        {
            this.DateTime = dateTime;
            this.Value = value;
        }
    }
}
