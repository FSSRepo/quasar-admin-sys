using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using QASServer.database;

public class Database
{
    SqlConnection connection;
    private SqlCommand sqlcmd(string query)
    {
        Console.WriteLine(query);
        SqlCommand cmd = new SqlCommand(query, connection);
        cmd.ExecuteNonQuery();
        return cmd;
    }
    public Database()
    {
        try
        {
            connection = new SqlConnection("Data Source=LAPTOP-F95L7HC4;Initial Catalog=KantMarket;Integrated Security=true");
            connection.Open();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public AccountModel getAccount(string by,string value)
    {
        SqlCommand cmd = new SqlCommand("select * from Account where "+by+"='"+value+"'", connection);
        cmd.ExecuteNonQuery();
        using (SqlDataReader r = cmd.ExecuteReader()) {
            if (r.Read())
            {
                AccountModel model = new AccountModel();
                model.name = r["name_"].ToString();
                model.lastname = r["lastname"].ToString();
                model.direction = r["direccion"].ToString();
                model.age = int.Parse(r["age"].ToString());
                model.credit_card = r["credit_card"].ToString();
                model.email = r["email"].ToString();
                model.password = r["password_"].ToString();
                model.sex = int.Parse(r["sex"].ToString());
                model.id = r["id"].ToString();
                model.token = r["token"].ToString();
                model.master = r["type_"].ToString().Equals("0");
                return model;
            }
        }
        return null;
    }

    public void editMasterAccount(string id,string name,string lastname,string token,string password)
    {
        SqlCommand cmd = new SqlCommand(
                "update Account set "+
                "name_='"+ name + "', lastname='" + 
                lastname + "', token='"+token+"'"+
                (password.Length == 0 ? "" : ", password_='"+ EncryptPass.Encrypt(password) +"'")+
                " where id='"+id+"'", connection);
        cmd.ExecuteNonQuery();
    }
    public void changeOrderStatus(string id, int status)
    {
        SqlCommand cmd = new SqlCommand(
                "update OrderDelivery set " +
                "state_=" + status +
                " where id='" + id + "'", connection);
        cmd.ExecuteNonQuery();
    }

    public void updatePassword(string id, string pass)
    {
        SqlCommand cmd = new SqlCommand(
                "update Account set " +
                "password_='" + EncryptPass.Encrypt(pass) + "' where id='" + id + "'", connection);
        cmd.ExecuteNonQuery();
    }

    public void deleteOrder(string id)
    {
        sqlcmd("delete from OrderDelivery where id='" + id + "'");
    }

    public void deleteUser(string id)
    {
        // eliminar facturas>compras y pedidos
        List<string> inv_ids = new List<string>(); 
        using (SqlDataReader r = sqlcmd("select id from Invoice where customer_id='" + id+"'").ExecuteReader())
        {
            while (r.Read())
            {
                inv_ids.Add(r["id"].ToString());
            }
        }
        for (int i = 0;i < inv_ids.Count;i++)
        {
            sqlcmd("delete from Buy where invoice_id='" + inv_ids[i] + "'");
            sqlcmd("delete from OrderDelivery where invoice_id='" + inv_ids[i] + "'");
            sqlcmd("delete from Invoice where id='" + inv_ids[i] + "'");
        }
        sqlcmd("delete from Account where id='" + id + "'");
    }

    public List<AccountModel> getAccounts()
    {
        SqlCommand cmd = new SqlCommand("select * from Account", connection);
        cmd.ExecuteNonQuery();
        List<AccountModel> models = new List<AccountModel>();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                AccountModel model = new AccountModel();
                model.name = r["name_"].ToString();
                model.lastname = r["lastname"].ToString();
                model.direction = r["direccion"].ToString();
                model.age = int.Parse(r["age"].ToString());
                model.credit_card = r["credit_card"].ToString();
                model.password = r["password_"].ToString();
                model.email = r["email"].ToString();
                model.id = r["id"].ToString();
                model.token = r["token"].ToString();
                model.master = r["type_"].ToString().Equals("0");
                models.Add(model);
            }
        }
        return models;
    }

    public List<OrderModel> getOrders(string from, string to)
    {
        SqlCommand cmd = new SqlCommand("select * from OrderDelivery where receive_date between '" + from + "' and '" + to + "' order by receive_date desc", connection);
        cmd.ExecuteNonQuery();
        List<OrderModel> models = new List<OrderModel>();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                OrderModel model = new OrderModel();
                model.id = r["id"].ToString();
                model.invoice_id = r["invoice_id"].ToString();
                model.status = int.Parse(r["state_"].ToString());
                model.date = r["receive_date"].ToString();
                models.Add(model);
            }
        }
        return models;
    }
    public List<OrderModel> getOrders(string customer_id)
    {
        SqlCommand cmd = new SqlCommand("select o.* from OrderDelivery o inner join Invoice i on i.customer_id='"+customer_id+"' and o.invoice_id=i.id order by o.receive_date desc", connection);
        cmd.ExecuteNonQuery();
        List<OrderModel> models = new List<OrderModel>();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                OrderModel model = new OrderModel();
                model.id = r["id"].ToString();
                model.invoice_id = r["invoice_id"].ToString();
                model.status = int.Parse(r["state_"].ToString());
                model.date = r["receive_date"].ToString();
                models.Add(model);
            }
        }
        return models;
    }

    public OrderModel getOrder(string id)
    {
        SqlCommand cmd = new SqlCommand("select * from OrderDelivery where id='"+id+"'", connection);
        cmd.ExecuteNonQuery();
        OrderModel model = new OrderModel();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            if (r.Read())
            {
               model.id = r["id"].ToString();
                model.invoice_id = r["invoice_id"].ToString();
                model.status = int.Parse(r["state_"].ToString());
                model.date = r["receive_date"].ToString();
                return model;
            }
        }
        return null;
    }

    public InvoiceModel getInvoice(string by, string value)
    {
        SqlCommand cmd = new SqlCommand("select * from Invoice where "+by+"='" + value + "'", connection);
        cmd.ExecuteNonQuery();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            if (r.Read())
            {
                InvoiceModel model = new InvoiceModel();
                model.id = r["id"].ToString();
                model.customer_id = r["customer_id"].ToString();
                model.date = r["date_"].ToString();
                model.total_price = int.Parse(r["total_price"].ToString());
                return model;
            }
        }
        return null;
    }

    public List<InvoiceModel> getInvoices(string id)
    {
        SqlCommand cmd = new SqlCommand("select * from Invoice where customer_id='" + id + "'", connection);
        cmd.ExecuteNonQuery();
        List<InvoiceModel> models = new List<InvoiceModel>();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                InvoiceModel model = new InvoiceModel();
                model.id = r["id"].ToString();
                model.customer_id = r["customer_id"].ToString();
                model.date = r["date_"].ToString();
                model.total_price = int.Parse(r["total_price"].ToString());
                models.Add(model);
            }
        }
        return models;
    }

    public List<ProductModel> getProducts()
    {
        SqlCommand cmd = new SqlCommand("select * from Product", connection);
        cmd.ExecuteNonQuery();
        List<ProductModel> models = new List<ProductModel>();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                ProductModel model = new ProductModel();
                model.id = r["id"].ToString();
                model.name = r["name_"].ToString();
                model.price = int.Parse (r["price"].ToString());
                model.category = int.Parse(r["category"].ToString());
                model.res_image = r["res_image"].ToString();
                model.status = int.Parse(r["status_"].ToString());
                models.Add(model);
            }
        }
        return models;
    }

    public void addProductToList(string inv_id, List<ProductSerialize> products)
    {
        SqlCommand cmd = new SqlCommand("select product_id,count_ from Buy where invoice_id='" + inv_id+"'", connection);
        cmd.ExecuteNonQuery();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                string pro_id = r["product_id"].ToString();
                int count_ = int.Parse(r["count_"].ToString());
                if (!exist(products,pro_id,count_))
                {
                    products.Add(new ProductSerialize() { ProductID = pro_id, count = count_ });
                }
            }
        }
    }

    public bool hasOrderInvoice(string inv_id)
    {
        SqlCommand cmd = new SqlCommand("select * from OrderDelivery where invoice_id='" + inv_id + "'", connection);
        cmd.ExecuteNonQuery();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            if (r.Read()) return true;
        }
        return false;
    }

    private bool exist(List<ProductSerialize> products,string id,int count)
    {
        for (int i = 0;i < products.Count;i++)
        {
            if(products[i].ProductID == id)
            {
                products[i].count += count;
                return true;
            }
        }
        return false;
    }

    public void newInvoice(string customer_id)
    {
        sqlcmd("insert into Invoice values ('"+generateRandomID("Invoice") +"','"+customer_id+"','"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"',0)");
    }

    public void deleteBuys(string invoice_id)
    {
        sqlcmd("delete from Buy where invoice_id='" + invoice_id + "'");
    }
    public void updateTotalPrice(string invoice_id,float total_price)
    {
        sqlcmd("update Invoice set total_price="+total_price+" where id='" + invoice_id + "'");
    }
    public void addBuy(string invoice_id, string product_id, int count)
    {
        sqlcmd("insert into Buy values ('"+generateRandomID("Buy") +"','"+product_id+"','"+invoice_id+"',"+count+")");
    }

    public void newOrder(string invoice_id)
    {
        sqlcmd("insert into OrderDelivery values ('" + generateRandomID("OrderDelivery") + "','" + invoice_id + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0)");
    }

    public ProductModel getProduct(string id)
    {
        SqlCommand cmd = new SqlCommand("select * from Product where id='"+id+"'", connection);
        cmd.ExecuteNonQuery();
       using (SqlDataReader r = cmd.ExecuteReader())
        {
            if (r.Read())
            {
                ProductModel model = new ProductModel();
                model.id = r["id"].ToString();
                model.name = r["name_"].ToString();
                model.price = int.Parse(r["price"].ToString());
                model.category = int.Parse(r["category"].ToString());
                model.res_image = r["res_image"].ToString();
                model.status = int.Parse(r["status_"].ToString());
                return model;
            }
        }
        return null;
    }

    

    public List<ProductOrderModel> getProductsByInvoice(string invoice_id)
    {
        SqlCommand cmd = new SqlCommand("select p.name_,b.count_ from Product p inner join Buy b on b.invoice_id = '"+ invoice_id+"' and p.id = b.product_id", connection);
        cmd.ExecuteNonQuery();
        List<ProductOrderModel> models = new List<ProductOrderModel>();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {

                ProductOrderModel model = new ProductOrderModel();
                model.name = r["name_"].ToString();
                model.count = int.Parse( r["count_"].ToString());
                models.Add(model);
            }
        }
        return models;
    }

    public List<ProductOrderModel> getProductsIDByInvoice(string invoice_id)
    {
        SqlCommand cmd = new SqlCommand("select * from Buy where invoice_id = '" + invoice_id + "'", connection);
        cmd.ExecuteNonQuery();
        List<ProductOrderModel> models = new List<ProductOrderModel>();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                ProductOrderModel model = new ProductOrderModel();
                model.name = r["product_id"].ToString();
                model.count = int.Parse(r["count_"].ToString());
                models.Add(model);
            }
        }
        return models;
    }

    public void createAccount(
            string name,
            string lastname,
            string direction,
            string credit_card,
            string email,
            int age,
            int sex,
            string token,
            string password)
    {
        sqlcmd("insert into Account values (" +
                     "'" + generateRandomID("Account") + "'," +
                     "'" + name + "'," +
                     "'" + lastname + "'," +
                     "'" + direction + "'," +
                     "'" + credit_card + "'," +
                     age + "," +
                     sex + "," +
                     "'" + email + "'," +
                     "'" + token + "'," +
                     "'" + password + "'," +
                     "1)");
    }

    public void updateAccount(
        string id,
            string name,
            string lastname,
            string direction,
            string credit_card,
            string email,
            int age,
            int sex,
            string token,
            string password)
    {
        string query = "update Account set " +
            "name_='" + name + "'," +
                     "lastname='" + lastname + "'," +
                     "direccion='" + direction + "'," +
                     "credit_card='" + credit_card + "'," +
                     "age=" + age + "," +
                     "sex=" + sex + "," +
                     "email='"+email+"',"+
                     "token='" + token + "'" +
                     (password.Length == 0 ? "" : ",password_='" + EncryptPass.Encrypt(password) + "'") + " where id='" + id + "'";
        sqlcmd(query);
    }
    public void updateProduct(
        string id,
            string name,
            string res_iamge,
            float price,
            int category,
            int status)
    {
        string query = "update Product set " +
            "name_='" + name + "'," +
                     "category=" + category + "," +
                     "price=" + price + ",status_="+status+" where id='" + id + "'";
        sqlcmd(query);
    }

    private string generateRandomID(string table)
    {
        string id = "";
        Random r = new Random();
        for (int i = 0; i < 10; i++)
        {
            bool num = r.Next(-1, 2) < 0;
            if (num)
            {
                id += (char)(r.Next(48, 57));
            }
            else
            {
                id += (char)(r.Next(65, 90));
            }
        }
        if (existID(id,table))
        {
            return generateRandomID(table);
        }
        return id;
    }

    //  comprobar si hay un id similar
    private bool existID(string test_id,string table)
    {
        SqlCommand cmd = new SqlCommand("select * from "+table+" where id='" + test_id + "'", connection);
        cmd.ExecuteNonQuery();
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            if (r.Read()) return true;
        }
        return false;
    }
}