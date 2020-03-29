using System.ComponentModel.DataAnnotations;
using PaypalDemo.Models.ValueObjects;

namespace PaypalDemo.Models.InputModels
{
    public class OrderCreateInputModel
    {
        [Display(Name = "Id dell'ordine")]
        public int OrderId { get; set; }
        [Display(Name = "Descrizione sintetica")]
        public string Description { get; set; }
        [Display(Name = "Totale da pagare")]
        public Money Price { get; set; }
    }
}