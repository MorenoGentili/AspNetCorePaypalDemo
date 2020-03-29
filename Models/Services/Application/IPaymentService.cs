using System;
using System.Threading.Tasks;
using PaypalDemo.Models.InputModels;
using PaypalDemo.Models.ViewModels;

namespace PaypalDemo.Models.Services.Application
{
    public interface IPaymentService
    {
        Task<Uri> CreateUriForPayment(OrderCreateInputModel inputModel);
        Task<OrderConfirmationViewModel> ConfirmOrder(string token);
    }
}