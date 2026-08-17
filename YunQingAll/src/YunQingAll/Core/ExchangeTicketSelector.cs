using System;
using System.Collections.Generic;
using System.Linq;

namespace RatopiaMod.YunQing.All.Core
{
    internal static class ExchangeTicketSelector
    {
        internal static T SelectOrOriginal<T>(
            T originalResult,
            List<T> tickets,
            ExchangeRateMode mode,
            Func<T, float> getExchangeRate,
            Action<List<T>> shuffle,
            Action<Exception> reportError)
        {
            try
            {
                var positiveMaximum = tickets.OrderByDescending(getExchangeRate).First();
                var negativeMaximum = tickets.OrderBy(getExchangeRate).First();
                shuffle(tickets);
                var positive = tickets.First(ticket => getExchangeRate(ticket) >= 0f);
                var negative = tickets.First(ticket => getExchangeRate(ticket) <= 0f);

                switch (mode)
                {
                    case ExchangeRateMode.POSITIVE:
                        return positive;
                    case ExchangeRateMode.POSITIVE_MAX:
                        return positiveMaximum;
                    case ExchangeRateMode.COMMON:
                        return originalResult;
                    case ExchangeRateMode.NEGATIVE:
                        return negative;
                    case ExchangeRateMode.NEGATIVE_MAX:
                        return negativeMaximum;
                    default:
                        return originalResult;
                }
            }
            catch (Exception error)
            {
                reportError?.Invoke(error);
                return originalResult;
            }
        }
    }
}
