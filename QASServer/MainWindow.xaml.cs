using MaterialDesignThemes.Wpf;
using QASServer.database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.IO.Compression;

namespace QASServer
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        Database db;

        public MainWindow()
        {
            InitializeComponent();
            //Server.init("192.168.43.104", 4000);
            Server.init("192.168.1.127", 4000);
            // Server.init("192.168.1.6", 4000);
            Server.instance.OnReceived = OnReceived;
            ServerInfoResolver.getInstance().OnUpdateView = OnInfoResolverView;
            dgClients.ItemsSource = ServerInfoResolver.getInstance().cn_clients;
            Server.instance.OnConnect = OnConnect;
            Server.instance.OnReport = OnReport;
            db = new Database();
            Closing += closing;
            Server.instance.OnServerCriticalFail = serverError;
            Server.instance.start();
        }


        private async void serverFail(string text)
        {
            await DialogHost.Show(new MessageDialog("Error:", text), "RootDialog");
        }

        private void closing(object sender, CancelEventArgs args)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void performDrag(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private async void CloseBtn(object sender, EventArgs args)
        {
            await DialogHost.Show(new CloseAppDialog(), "RootDialog");
        }
        private void minimizeBtn(object sender, EventArgs args)
        {
            WindowState = WindowState.Minimized;
        }

        private void serverError(object sender, string args)
        {
            this.Dispatcher.Invoke(() =>
            {
                serverFail(args);
            });
        }

        private void OnConnect(object sender, ServerData data)
        {

        }

        private void OnInfoResolverView(object sender, EventArgs args)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (Server.instance.listening)
                {
                    txtInfo.Content = "Socket: TCP/IP      Default database: SQL Server Database\nSocket Adress Listening: '" + Server.instance.ip + "/" + Server.instance.port + "' by " + toTimeFormat(Server.instance.time_count);
                }
                else
                {
                    txtInfo.Content = "Servidor no responde";
                }
                dgClients.ItemsSource = null;
                dgClients.ItemsSource = ServerInfoResolver.getInstance().cn_clients;
            });
        }

        private void OnReport(object sender, ReportData data)
        {
            if (data.type == ReportType.SERVER_CRITICAL_ERROR)
            {

            }
        }

        private string toTimeFormat(int count)
        {
            int minutes = (count / 60);
            return (minutes / 60) + ":" + ((minutes % 60 < 10) ? "0" : "") + (minutes % 60) + ":" + ((count % 60 < 10) ? "0" : "") + (count % 60);
        }

        private void OnReceived(object sender, ServerData data)
        {
            // ejecutar paralelamente las acciones
            switch (data.requestID)
            {
                case ConstantsResponse.CLT_LOGIN:
                    login(data);
                    break;
                case ConstantsResponse.CLT_ORDER_LIST:
                    orderList(data);
                    break;
                case ConstantsResponse.CLT_USER_LIST:
                    accountList(data);
                    break;
                case ConstantsResponse.CLT_LOGOUT:
                    logout(data);
                    break;
                case ConstantsResponse.CLT_APPLY_MASTER:
                    editMaster(data);
                    break;
                case ConstantsResponse.CLT_SELECT_ORDER:
                    selectOrder(data);
                    break;
                case ConstantsResponse.CLT_ORD_STATUS:
                    changeOrderStatus(data);
                    break;
                case ConstantsResponse.CLT_DELETE_ORDER:
                    deleteOrder(data);
                    break;
                case ConstantsResponse.CLT_SELECT_USER:
                    selectUser(data);
                    break;
                case ConstantsResponse.CLT_CREATE_USER:
                    addNewUser(data);
                    break;
                case ConstantsResponse.CLT_UPDATE_USER:
                    editUser(data);
                    break;
                case ConstantsResponse.CLT_DELETE_USER:
                    deleteUser(data);
                    break;
                case ConstantsResponse.CLT_PRODUCT_LIST:
                    productList(data);
                    break;
                case ConstantsResponse.CLT_SELECT_PRODUCT:
                    selectProduct(data);
                    break;
                case ConstantsResponse.CLT_UPDATE_PRODUCT:
                    editProduct(data);
                    break;
                case ConstantsResponse.CLT_GET_ORDER_POINTS:
                    sendOrderPoints(data);
                    break;
                case ConstantsResponse.CLT_TOP_PRODUCT:
                    sendTopProduct(data);
                    break;
                case ConstantsResponse.CLT_PERSON_ANALYTICS:
                    sendPersonAnalytics(data);
                    break;
                case ConstantsResponse.CLT_N_PRODUCT_LIST:
                    appProductList(data);
                    break;
                case ConstantsResponse.CLT_N_ORDER_LIST:
                    appOrderList(data);
                    break;
                case ConstantsResponse.CLT_N_INVOICE_LIST:
                    appInvoiceList(data);
                    break;
                case ConstantsResponse.CLT_FORGOT_TOKEN:
                    appForgetToken(data);
                    break;
                case ConstantsResponse.CLT_FORGOT_PASSWORD:
                    appForgetPassword(data);
                    break;
                case ConstantsResponse.CLT_N_CHANGE_PASS:
                    appChangePassword(data);
                    break;
                case ConstantsResponse.CLT_N_USER_INFO:
                    appUserInfo(data);
                    break;
                case ConstantsResponse.CLT_N_FULL_INVOICE:
                    appFullInvoiceInfo(data);
                    break;
                case ConstantsResponse.CLT_N_NEW_INVOICE:
                    appNewInvoice(data);
                    break;
                case ConstantsResponse.CLT_N_NEW_ORDER:
                    appNewOrder(data);
                    break;
                case ConstantsResponse.CLT_N_CANCEL_ORDER:
                    appCancelOrder(data);
                    break;
                case ConstantsResponse.CLT_N_UPDATE_INVOICE:
                    appUpdateInvoice(data);
                    break;
            }
        }

        private void login(ServerData data)
        {
            String token = data.buffer.readString();
            String password = data.buffer.readString();
            AccountModel acc = db.getAccount("token", token);
            if (acc == null)
            {
                data.client.begin(ConstantsResponse.SRV_INVALID_TOKEN);
                data.client.end();
                return;
            }
            if (!EncryptPass.Validate(password, acc.password))
            {
                data.client.begin(ConstantsResponse.SRV_INVALID_PASSWORD);
                data.client.end();
                return;
            }
            if (!data.client.session.master_access && acc.master ||
                data.client.session.master_access && !acc.master)
            {
                data.client.begin(ConstantsResponse.SRV_NACCESS_MASTER);
                data.client.end();
                return;
            }
            data.client.session.db_id_account = acc.id;
            data.client.session.login = true;
            data.client.begin(ConstantsResponse.SRV_LOGIN_COMPLETED);
            data.client.output.writeString(acc.name);
            data.client.output.writeString(acc.lastname);
            data.client.end();
        }

        private void logout(ServerData data)
        {
            data.client.session.login = false;
            data.client.begin(ConstantsResponse.SRV_LOGOUT_COMPLETED);
            data.client.end();
        }

        private void editMaster(ServerData data)
        {
            String name = data.buffer.readString();
            String lastname = data.buffer.readString();
            String token = data.buffer.readString();
            String password = data.buffer.readString();
            db.editMasterAccount(data.client.session.db_id_account, name, lastname, token, password);
        }

        private void changeOrderStatus(ServerData data)
        {
            string id = data.buffer.readString();
            int status = data.buffer.readInt();
            db.changeOrderStatus(id, status);
            data.client.begin(ConstantsResponse.SRV_UPDATE_ORDER);
            data.client.end();
            string acc_id = db.getInvoice("id", db.getOrder(id).invoice_id).customer_id;
            for (int i = 0; i < data.server.clients.Count; i++)
            {
                Session s = data.server.clients[i].session;
                if (!s.master_access
                    && s.login && s.db_id_account == acc_id)
                {
                    data.server.clients[i].begin(ConstantsResponse.SRV_N_ORDER_NOTIFY);
                    data.server.clients[i].output.writeString(id);
                    data.server.clients[i].output.writeInt(status);
                    data.server.clients[i].end();

                }
            }

        }

        private void deleteOrder(ServerData data)
        {
            string id = data.buffer.readString();
            string acc_id = db.getInvoice("id", db.getOrder(id).invoice_id).customer_id;
            db.deleteOrder(id);
            data.client.begin(ConstantsResponse.SRV_UPDATE_ORDER);
            data.client.end();
            for (int i = 0; i < data.server.clients.Count; i++)
            {
                Session s = data.server.clients[i].session;
                if (!s.master_access
                    && s.login && s.db_id_account == acc_id)
                {
                    data.server.clients[i].begin(ConstantsResponse.SRV_N_ORDER_NOTIFY);
                    data.server.clients[i].output.writeString(id);
                    data.server.clients[i].output.writeInt(3);
                    data.server.clients[i].end();

                }
            }
        }

        private void orderList(ServerData data)
        {
            // format date yy-MM-dd HH:mm:ss
            string start = data.buffer.readString();
            string end = data.buffer.readString();
            List<OrderModel> orders = db.getOrders(start, end);
            data.client.begin(ConstantsResponse.SRV_ORDER_LIST);
            data.client.output.writeInt(orders.Count);
            for (int i = 0; i < orders.Count; i++)
            {
                data.client.output.writeString(orders[i].id);
                data.client.output.writeString(orders[i].invoice_id);
                AccountModel acc = db.getAccount("id", db.getInvoice("id", orders[i].invoice_id).customer_id);
                data.client.output.writeString(acc.name + " " + acc.lastname);
                data.client.output.writeString(orders[i].date);
                data.client.output.writeInt(orders[i].status);
            }
            data.client.end();
        }

        private void accountList(ServerData data)
        {
            List<AccountModel> accounts = db.getAccounts();
            data.client.begin(ConstantsResponse.SRV_ACCOUNT_LIST);
            data.client.output.writeInt(accounts.Count);
            for (int i = 0; i < accounts.Count; i++)
            {
                data.client.output.writeString(accounts[i].id);
                data.client.output.writeString(accounts[i].name);
                data.client.output.writeString(accounts[i].lastname);
                data.client.output.writeByte(accounts[i].master ? 1 : 0);
            }
            data.client.end();
        }

        private void productList(ServerData data)
        {
            List<ProductModel> products = db.getProducts();
            data.client.begin(ConstantsResponse.SRV_PRODUCT_LIST);
            data.client.output.writeInt(products.Count);
            for (int i = 0; i < products.Count; i++)
            {
                data.client.output.writeString(products[i].id);
                data.client.output.writeString(products[i].name);
                data.client.output.writeString(products[i].res_image);
            }
            data.client.end();
        }

        private void selectOrder(ServerData data)
        {
            string id = data.buffer.readString();
            OrderModel order = db.getOrder(id);
            data.client.begin(ConstantsResponse.SRV_SELECT_ORDER);
            InvoiceModel inv = db.getInvoice("id", order.invoice_id);
            AccountModel acc = db.getAccount("id", inv.customer_id);
            List<ProductOrderModel> products = db.getProductsByInvoice(order.invoice_id);
            data.client.output.writeString(acc.name + " " + acc.lastname);
            data.client.output.writeString(inv.total_price + "");
            data.client.output.writeInt(order.status);
            data.client.output.writeInt(products.Count);
            for (int i = 0; i < products.Count; i++)
            {
                data.client.output.writeInt(products[i].count);
                data.client.output.writeString(products[i].name);
            }
            data.client.end();
        }
        private void selectUser(ServerData data)
        {
            string id = data.buffer.readString();
            AccountModel acc = db.getAccount("id", id);
            data.client.begin(ConstantsResponse.SRV_SELECT_USER);
            data.client.output.writeString(acc.name);
            data.client.output.writeString(acc.lastname);
            data.client.output.writeString(acc.direction);
            data.client.output.writeInt(acc.age);
            data.client.output.writeByte(acc.sex);
            data.client.output.writeString(acc.credit_card);
            data.client.output.writeString(acc.email);
            data.client.output.writeString(acc.token);
            data.client.end();
        }

        private void selectProduct(ServerData data)
        {
            string id = data.buffer.readString();
            ProductModel acc = db.getProduct(id);
            data.client.begin(ConstantsResponse.SRV_SELECT_PRODUCT);
            data.client.output.writeString(acc.name);
            data.client.output.writeString(acc.res_image);
            data.client.output.writeInt(acc.category);
            data.client.output.writeFloat(acc.price);
            data.client.output.writeInt(acc.status);
            data.client.end();
        }


        private void deleteUser(ServerData data)
        {
            string id = data.buffer.readString();
            db.deleteUser(id);
            data.client.begin(ConstantsResponse.SRV_UPDATE_USER);
            data.client.output.writeByte(0xD);
            data.client.end();
        }

        private void addNewUser(ServerData data)
        {
            string name = data.buffer.readString();
            string lastname = data.buffer.readString();
            string direction = data.buffer.readString();
            int age = data.buffer.readInt();
            int sex = data.buffer.readByte();
            string credit_card = data.buffer.readString();
            string email = data.buffer.readString();
            string token = data.buffer.readString();
            string password = EncryptPass.Encrypt(data.buffer.readString());
            db.createAccount(name, lastname, direction, credit_card, email, age, sex, token, password);
            data.client.begin(ConstantsResponse.SRV_UPDATE_USER);
            data.client.end();
        }

        private void editUser(ServerData data)
        {
            string id = data.buffer.readString();
            string name = data.buffer.readString();
            string lastname = data.buffer.readString();
            string direction = data.buffer.readString();
            int age = data.buffer.readInt();
            int sex = data.buffer.readByte();
            string credit_card = data.buffer.readString();
            string email = data.buffer.readString();
            string token = data.buffer.readString();
            string password = data.buffer.readString();
            db.updateAccount(id, name, lastname, direction, credit_card, email, age, sex, token, password);
            data.client.begin(ConstantsResponse.SRV_UPDATE_USER);
            data.client.output.writeByte(0xD);
            data.client.end();
        }

        private void editProduct(ServerData data)
        {
            string id = data.buffer.readString();
            string name = data.buffer.readString();
            string res_image = data.buffer.readString();
            float price = data.buffer.readFloat();
            int category = data.buffer.readInt();
            int status = data.buffer.readInt();
            db.updateProduct(id, name, res_image, price, category, status);
            data.client.begin(ConstantsResponse.SRV_UPDATE_PRODUCT);
            data.client.end();
        }
        private void sendOrderPoints(ServerData data)
        {
            // format date yy-MM-dd HH:mm:ss
            DateTime start = DateTime.Parse(data.buffer.readString());
            DateTime end = DateTime.Parse(data.buffer.readString());
            List<OrderModel> orders = db.getOrders(start.ToString("yyyy-MM-dd 00:00:00"), end.ToString("yyyy-MM-dd 23:59:59"));
            int day_count = (end - start).Days;
            data.client.begin(ConstantsResponse.SRV_ORDER_POINTS);
            data.client.output.writeInt(day_count + 1);
            for (int d = 0; d <= day_count; d++)
            {
                DateTime day = start.AddDays(d);
                int count = 0;
                for (int i = 0; i < orders.Count; i++)
                {
                    DateTime test = DateTime.Parse(orders[i].date);
                    if (test.Day == day.Day && test.Month == day.Month && orders[i].status == 2)
                    {
                        count++;
                    }
                }
                data.client.output.writeString(day.ToString("yyyy-MM-dd"));
                data.client.output.writeInt(count);
            }
            data.client.end();
        }

        private void sendTopProduct(ServerData data)
        {
            // format date yy-MM-dd HH:mm:ss
            DateTime start = DateTime.Parse(data.buffer.readString());
            DateTime end = DateTime.Parse(data.buffer.readString());
            List<OrderModel> orders = db.getOrders(start.ToString("yyyy-MM-dd 00:00:00"), end.ToString("yyyy-MM-dd 23:59:59"));
            List<ProductSerialize> products = new List<ProductSerialize>();
            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i].status == 2)
                {
                    db.addProductToList(orders[i].invoice_id, products);
                }
            }
            for (int i = 0; i < products.Count; i++)
            {
                for (int j = 0; j < products.Count - 1; j++)
                {
                    if (products[j].count > products[j + 1].count)
                    {
                        ProductSerialize temp = products[j];
                        products[j] = products[j + 1];
                        products[j + 1] = temp;
                    }
                }
            }
            data.client.begin(ConstantsResponse.SRV_TOP_PRODUCT);
            data.client.output.writeInt(products.Count > 20 ? 20 : products.Count);
            for (int i = products.Count - 1, j = 0; i >= 0; i--)
            {
                if (j > 20)
                {
                    break;
                }
                ProductModel prod = db.getProduct(products[i].ProductID);
                data.client.output.writeString(prod.name);
                data.client.output.writeString(prod.res_image);
                data.client.output.writeInt(products[i].count);
                j++;
            }
            data.client.end();
        }

        private void sendPersonAnalytics(ServerData data)
        {
            // format date yy-MM-dd HH:mm:ss
            DateTime start = DateTime.Parse(data.buffer.readString());
            DateTime end = DateTime.Parse(data.buffer.readString());
            List<OrderModel> orders = db.getOrders(start.ToString("yyyy-MM-dd 00:00:00"), end.ToString("yyyy-MM-dd 00:00:00"));
            int womens = 0, mens = 0;
            int e18_20 = 0, e21_30 = 0, e31_50 = 0, e51_70 = 0;
            for (int i = 0; i < orders.Count; i++)
            {
                AccountModel acc = db.getAccount("id", db.getInvoice("id", orders[i].invoice_id).customer_id);
                if (acc.sex == 0)
                {
                    womens++;
                }
                else if (acc.sex == 1)
                {
                    mens++;
                }
                if (acc.age >= 18 && acc.age <= 20) { e18_20++; }
                else if (acc.age >= 21 && acc.age <= 30) { e21_30++; }
                else if (acc.age >= 31 && acc.age <= 50) { e31_50++; }
                else if (acc.age >= 51 && acc.age <= 70) { e51_70++; }
            }
            data.client.begin(ConstantsResponse.SRV_PERSON_ANALYTICS);
            data.client.output.writeInt(womens);
            data.client.output.writeInt(mens);
            data.client.output.writeInt(e18_20);
            data.client.output.writeInt(e21_30);
            data.client.output.writeInt(e31_50);
            data.client.output.writeInt(e51_70);
            data.client.end();
        }

        /* APLICACION ANDROID REQUESTS */

        private void appProductList(ServerData data)
        {
            List<ProductModel> products = db.getProducts();
            data.client.begin(ConstantsResponse.SRV_N_PRODUCT_LIST);
            data.client.output.writeInt(products.Count);
            for (int i = 0; i < products.Count; i++)
            {
                data.client.output.writeString(products[i].id);
                data.client.output.writeString(products[i].name);
                data.client.output.writeString(products[i].res_image);
                data.client.output.writeFloat(products[i].price);
                data.client.output.writeByte(products[i].category);
                data.client.output.writeInt(products[i].status);
            }
            data.client.end();
        }

        private void appOrderList(ServerData data)
        {
            List<OrderModel> orders = db.getOrders(data.client.session.db_id_account);
            data.client.begin(ConstantsResponse.SRV_N_ORDER_LIST);
            data.client.output.writeInt(orders.Count);
            for (int i = 0; i < orders.Count; i++)
            {
                data.client.output.writeString(orders[i].id);
                data.client.output.writeString(orders[i].date);
                data.client.output.writeInt(orders[i].status);
            }
            data.client.end();
        }

        private void appInvoiceList(ServerData data)
        {
            List<InvoiceModel> invoices = db.getInvoices(data.client.session.db_id_account);
            data.client.begin(ConstantsResponse.SRV_N_INVOICE_LIST);
            data.client.output.writeInt(invoices.Count);
            for (int i = 0; i < invoices.Count; i++)
            {
                data.client.output.writeString(invoices[i].id);
                data.client.output.writeString(invoices[i].date);
                data.client.output.writeFloat(invoices[i].total_price);
                data.client.output.writeByte(db.hasOrderInvoice(invoices[i].id) ? 1 : 0);
            }
            data.client.end();
        }

        private void appForgetToken(ServerData data)
        {
            string email = data.buffer.readString();
            AccountModel acc = db.getAccount("email", email);
            if (acc == null)
            {
                data.client.begin(ConstantsResponse.SRV_FRPW_RESULT);
                data.client.output.writeInt(0);
                data.client.end();
            }
            else
            {
                data.client.begin(ConstantsResponse.SRV_FRPW_RESULT);
                data.client.output.writeInt(1);
                data.client.output.writeString(acc.token);
                data.client.end();
            }
        }

        private void appForgetPassword(ServerData data)
        {
            if (data.client.session.change_pass)
            {
                string code_pass = data.buffer.readString();
                if (data.client.session.code_pass == code_pass)
                {
                    data.client.begin(ConstantsResponse.SRV_FRPW_RESULT);
                    data.client.output.writeInt(5);
                    data.client.end();
                    data.client.session.change_pass = false;
                }
                else
                {
                    data.client.begin(ConstantsResponse.SRV_FRPW_RESULT);
                    data.client.output.writeInt(4);
                    data.client.end();
                }
            }
            else
            {
                string token = data.buffer.readString();
                AccountModel acc = db.getAccount("token", token);
                if (acc == null)
                {
                    data.client.begin(ConstantsResponse.SRV_FRPW_RESULT);
                    data.client.output.writeInt(2);
                    data.client.end();
                }
                else
                {
                    data.client.begin(ConstantsResponse.SRV_FRPW_RESULT);
                    data.client.output.writeInt(3);
                    data.client.output.writeString(acc.email);
                    data.client.end();
                    data.client.session.change_pass = true;
                    data.client.session.db_id_account = acc.id;
                    data.client.session.code_pass = generateRandomCode();
                    SendEmail(
                            "Recuperar contraseña",
                            acc.email,
                            "<h1>Kant Market Support Team</h1>" +
                            "<h3>Nuestro equipo recibio una solicitud de cambio de contraseña</h3>" +
                            "<p>Le pedimos que ingrese el siguiente codigo en la aplicacion y proceder al cambio de contraseña</p>" +
                            "<h2>" + data.client.session.code_pass + "</h2>");
                }
            }
        }

        private void appUserInfo(ServerData data)
        {
            AccountModel acc = db.getAccount("id", data.client.session.db_id_account);
            data.client.begin(ConstantsResponse.SRV_N_USER_INFO);
            data.client.output.writeString(acc.direction);
            data.client.output.writeString(acc.credit_card);
            data.client.end();
        }
        public void appFullInvoiceInfo(ServerData data)
        {
            string inv_id = data.buffer.readString();
            List<ProductOrderModel> products = db.getProductsIDByInvoice(inv_id);
            data.client.begin(ConstantsResponse.SRV_N_FULL_INVOICE);
            data.client.output.writeString(inv_id);
            data.client.output.writeInt(products.Count);
            for (int i = 0; i < products.Count; i++)
            {
                data.client.output.writeString(products[i].name);
                data.client.output.writeInt(products[i].count);
            }
            data.client.end();
        }

        public void appNewInvoice(ServerData data)
        {
            db.newInvoice(data.client.session.db_id_account);
            data.client.begin(ConstantsResponse.SRV_N_UPDATE_PROPS);
            data.client.end();
        }

        public void appNewOrder(ServerData data)
        {
            db.newOrder(data.buffer.readString());
            data.client.begin(ConstantsResponse.SRV_N_UPDATE_PROPS);
            data.client.end();
            AccountModel acc = db.getAccount("id", data.client.session.db_id_account);
            for (int i = 0;i< data.server.clients.Count;i++)
            {
                if (data.server.clients[i].session.master_access
                    && data.server.clients[i].session.login)
                {
                    data.server.clients[i].begin(ConstantsResponse.SRV_ORDER_NOTIFY);
                    data.server.clients[i].output.writeString("Nuevo pedido: "+acc.name+" "+acc.lastname);
                    data.server.clients[i].output.writeByte(1);
                    data.server.clients[i].end();

                }
            }
        }

        public void appCancelOrder(ServerData data)
        {
            db.deleteOrder(data.buffer.readString());
            data.client.begin(ConstantsResponse.SRV_N_UPDATE_PROPS);
            data.client.end();
            AccountModel acc = db.getAccount("id", data.client.session.db_id_account);
            for (int i = 0; i < data.server.clients.Count; i++)
            {
                if (data.server.clients[i].session.master_access
                    && data.server.clients[i].session.login)
                {
                    data.server.clients[i].begin(ConstantsResponse.SRV_ORDER_NOTIFY);
                    data.server.clients[i].output.writeString("Se cancelo un pedido: " + acc.name + " " + acc.lastname);
                    data.server.clients[i].output.writeByte(0);
                    data.server.clients[i].end();
                }
            }
        }

        public void appUpdateInvoice(ServerData data)
        {
            string invoice_id = data.buffer.readString();
            int count = data.buffer.readInt();
            float total_price = data.buffer.readFloat();
            db.updateTotalPrice(invoice_id, total_price);
            db.deleteBuys(invoice_id);
            for (int i = 0; i < count; i++)
            {
                db.addBuy(invoice_id, data.buffer.readString(), data.buffer.readInt());
            }
        }

        public void appChangePassword(ServerData data)
        {
            string new_pass = data.buffer.readString();
            db.updatePassword(data.client.session.db_id_account, new_pass);
            data.client.session.db_id_account = "";
            data.client.session.change_pass = false;
        }

        public static void SendEmail(string subject, string email, string body)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("steward021002@gmail.com");
                message.To.Add(new MailAddress(email));
                message.Subject = subject;
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = body;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com"; //for gmail host  
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("steward021002@gmail.com", "stewardgarcia8639");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception) { }
        }
        private string generateRandomCode()
        {
            string id = "";
            Random r = new Random();
            for (int i = 0; i < 5; i++)
            {
                id += (char)(r.Next(48, 57));
            }
            return id;
        }
    }
}
