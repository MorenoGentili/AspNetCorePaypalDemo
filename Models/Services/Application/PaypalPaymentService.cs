using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PaypalDemo.Controllers;
using PaypalDemo.Models.Enums;
using PaypalDemo.Models.InputModels;
using PaypalDemo.Models.Options;
using PaypalDemo.Models.ViewModels;
using PayPalHttp;

namespace PaypalDemo.Models.Services.Application
{
    public class PaypalPaymentService : IPaymentService
    {
        private readonly LinkGenerator linkGenerator;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IOptionsMonitor<PaypalOptions> paypalOptions;
        private readonly ILogger<PaypalPaymentService> logger;

        public PaypalPaymentService(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator, IOptionsMonitor<PaypalOptions> optionMonitor, ILogger<PaypalPaymentService> logger)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.linkGenerator = linkGenerator;
            this.paypalOptions = optionMonitor;
            this.logger = logger;
        }
        public async Task<Uri> CreateUriForPayment(OrderCreateInputModel inputModel)
        {
            var options = paypalOptions.CurrentValue;
            PayPalEnvironment environment = CreateEnvironment();
            var client = new PayPalHttpClient(environment);

            var payment = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>()
                {
                    new PurchaseUnitRequest()
                    {
                        CustomId = inputModel.OrderId.ToString(),
                        Description = inputModel.Description,
                        AmountWithBreakdown = new AmountWithBreakdown()
                        {
                            CurrencyCode = inputModel.Price.Currency.ToString(),
                            Value = inputModel.Price.Amount.ToString(CultureInfo.InvariantCulture)
                        }
                    }
                },
                ApplicationContext = CreateApplicationContext()
            };

            //https://developer.paypal.com/docs/api/orders/v2/#orders_create
            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(payment);

            try
            {
                var response = await client.Execute(request);
                var statusCode = response.StatusCode;
                var result = response.Result<Order>();
                return new Uri(result.Links.Single(l => l.Rel == "approve").Href);

            }
            catch (HttpException httpException)
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();
                logger.LogError($"Il tentativo di reindirizzare l'utente alla pagina di pagamento è fallito con status code {statusCode} e debugId {debugId}");
                throw new PaymentException();
            }
        }


        public async Task<OrderConfirmationViewModel> ConfirmOrder(string token)
        {
            var options = paypalOptions.CurrentValue;
            PayPalEnvironment environment = CreateEnvironment();
            var client = new PayPalHttpClient(environment);

            var request = new OrdersCaptureRequest(token);
            request.Prefer("return=representation");
            request.RequestBody(new OrderActionRequest());
            try
            {
                var response = await client.Execute(request);
                Order order = response.Result<Order>();
                return OrderConfirmationViewModel.FromOrder(order);
            }
            catch (HttpException httpException)
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();
                logger.LogError($"Il tentativo di catturare il pagamento è fallito con status code {statusCode} e debugId {debugId}");
                throw new PaymentException();
            }
        }

        private PayPalEnvironment CreateEnvironment()
        {
            var options = paypalOptions.CurrentValue;
            if (options.Environment == PaypalEnvironment.Live)
            {
                return new LiveEnvironment(options.ClientId, options.ClientSecret);
            }
            else
            {
                return new SandboxEnvironment(options.ClientId, options.ClientSecret);
            }
        }

        private ApplicationContext CreateApplicationContext()
        {
            //Dopo che l'utente avrà pagato su paypal, lo facciamo tornare a questo url
            //da cui verifichieremo che la transazione sia avvenuta e marcheremo l'ordine come pagato          
            var returnUrl =  linkGenerator.GetUriByAction(httpContextAccessor.HttpContext,
                action: nameof(HomeController.ConfirmOrder),
                controller: "Home");

            //Questo invece è l'URL a cui tornare se l'utente decide di non pagare e sceglie di tornare indietro al sito
            var cancelUrl = linkGenerator.GetUriByAction(httpContextAccessor.HttpContext,
                action: nameof(HomeController.Index),
                controller: "Home");

            return new ApplicationContext
            {
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            };
        }
    }
}