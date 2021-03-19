using System;

namespace Trail
{
    /// <summary>
    /// price class used by Trail PaymentKit to get and view prices of items.
    /// </summary>
    public class Price
    {
        public int AmountDividend;
        public int AmountDivisor;
        public string CurrencyISO4217;

        public float Amount
        {
            get { return (float)this.AmountDividend / (float)this.AmountDivisor; }
        }

        public Price(int amountDividend, int amountDivisor, string currencyISO4217)
        {
            this.AmountDividend = amountDividend;
            this.AmountDivisor = amountDivisor;
            this.CurrencyISO4217 = currencyISO4217;
        }

        public override string ToString()
        {
            return String.Format("{0} {1}", this.Amount, this.CurrencyISO4217);
        }
    }
}
