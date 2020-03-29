using System;
using System.Globalization;
using System.Linq;
using PayPalCheckoutSdk.Orders;

namespace PaypalDemo.Models.ViewModels
{
    public class OrderConfirmationViewModel
    {
        public string TransactionId { get; set; }
        public int OrderId { get; set; }
        public DateTime Created { get; set; }

        public static OrderConfirmationViewModel FromOrder(Order order)
        {
            return new OrderConfirmationViewModel
            {
                TransactionId = order.Id,
                Created = DateTime.Parse(order.CreateTime, null, DateTimeStyles.RoundtripKind),
                OrderId = int.Parse(order.PurchaseUnits.First().CustomId)
            };
        }
    }
}