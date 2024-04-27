using Enums;
using Save;
using Save.Data;
using Unity.Services.Analytics;

namespace Analytics.Events
{
    public class CurrencyEvent : Event
    {
        protected static EAnalytics EventType => EAnalytics.CurrencyChanged;

        // Constructor for GameEnded event
        public CurrencyEvent(ECurrency currency, int qty, string context) : base(EventType.ToString())
        {
            // Set event parameters using SetParameter method
            SetParameter(EAnalyticsParam.Currency.ToString(), currency.ToString());
            SetParameter(EAnalyticsParam.Qty.ToString(), qty);
            SetParameter(EAnalyticsParam.Context.ToString(), context);

            if (qty > 0)
                StatCloudData.AddAnalytics(EventType, new SCurrencyEventCloudData(currency, qty, context));
        }
    }
}

