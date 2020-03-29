using PaypalDemo.Models.Enums;

namespace PaypalDemo.Models.Options
{
    public class PaypalOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public PaypalEnvironment Environment { get; set; }
    }
}