using Data;
using Enums;
using System;
using System.Collections.Generic;
using Tools;

namespace Save.Data
{
    [Serializable]
    public class SCurrencyEventCloudData : SAnalyticsData
    {
        public ECurrency Currency;
        public string Context;

        public SCurrencyEventCloudData(ECurrency currency, int qty, string context) : base()
        {
            Currency    = currency;
            Context     = context;
            Count       = qty;
        }

        public override object GetValue(EAnalyticsParam analyticsParam)
        {
            switch (analyticsParam)
            {
                case EAnalyticsParam.Currency:
                    return Currency;

                case EAnalyticsParam.Context:
                    return Context;

                default:
                    return base.GetValue(analyticsParam);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is SCurrencyEventCloudData other)
            {
                return this.Currency == other.Currency
                       && this.Context == other.Context;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Currency, Context);
        }
    }
}