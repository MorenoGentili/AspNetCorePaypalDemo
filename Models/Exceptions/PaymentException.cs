using System;

namespace PaypalDemo.Models.InputModels
{
    public class PaymentException : Exception
    {
        public PaymentException(): base("Il pagamento non è andato a buon fine")
        {
    
        }
    }
}