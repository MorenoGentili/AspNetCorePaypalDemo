using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaypalDemo.Models.Enums;
using PaypalDemo.Models.InputModels;
using PaypalDemo.Models.Services.Application;
using PaypalDemo.Models.ValueObjects;

namespace PaypalDemo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Paga l'ordine";
            //Mostriamo alcuni dati fittizi nell'interfaccia per consentire la modifica dell'id dell'ordine e dell'importo
            //In un'applicazione reale, questi valori arrivano dal database e per nessun motivo dovrebbero essere modificati dall'utente
            var inputModel = new OrderCreateInputModel
            {
                OrderId = 123456,
                Description = "Ordine merce",
                Price = new Money
                {
                    Amount = 11.00m,
                    Currency = Currency.EUR
                }
            };
            return View(inputModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(OrderCreateInputModel inputModel, [FromServices] IPaymentService paymentService)
        {
            Uri url = await paymentService.CreateUriForPayment(inputModel);
            return Redirect(url.ToString());
        }

        public async Task<IActionResult> ConfirmOrder(string token, [FromServices] IPaymentService paymentService)
        {
            var transactionResult = await paymentService.ConfirmOrder(token);
            ViewData["Title"] = "Conferma d'ordine";
            return View(transactionResult);
        }
    }
}
