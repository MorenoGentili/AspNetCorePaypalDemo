using System;
using PaypalDemo.Models.Enums;

namespace PaypalDemo.Models.ValueObjects
{
    public class Money
    {

        private decimal amount = 0;
        public decimal Amount
        { 
            get
            {
                return amount;
            }
            set
            {
                if (value < 0) {
                    throw new InvalidOperationException("The amount cannot be negative");
                }
                amount = value;
            }
        }
        public Currency Currency
        {
            get; set;
        }

        public override bool Equals(object obj)
        {
            var money = obj as Money;
            return money != null &&
                   Amount == money.Amount &&
                   Currency == money.Currency;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }
    }
}