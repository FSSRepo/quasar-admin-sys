using System.Windows.Media;

public class OrderItem
{
    public string OrderID;
    public string InvoiceID;

    public string OrderName { set; get; }
    public string DateTime { set; get; }
    public string State { set; get; }
    public SolidColorBrush SColor { set; get; }
}
