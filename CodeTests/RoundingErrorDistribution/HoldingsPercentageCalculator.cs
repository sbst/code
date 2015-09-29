using System;
using System.Collections.Generic;

namespace RoundingErrorDistribution
{
    public class HoldingsPercentageCalculator
    {
        public List<PercentageHolding> CalculatePercentages(List<Holding> holdings)
        {
            // we need to find percentage for all holdings.Value
            // at first step we need to find the sum of all values
            decimal sumValue = decimal.Zero;
            foreach (Holding hold in holdings)
                sumValue = Decimal.Add(sumValue, hold.Value);
            // sumValue contain value - 100%
            // we need find percentage for each value:
            // holdings[0].Value * 100 / sumValue
            List<PercentageHolding> result = new List<PercentageHolding>();            
            foreach (Holding hold in holdings)
            {
                PercentageHolding item = new PercentageHolding();   // new item in list of PercentageHolding for return
                item.Holding = hold;    // PercentageHolding must contain link from holdings
                decimal tmp = Decimal.Multiply(hold.Value, 100M);   // holdings[0].Value * 100
                decimal tmp2 = Decimal.Divide(tmp, sumValue);   // divide by sumValue. on this step we can get an error
                decimal res = Decimal.Round(tmp2, 2, MidpointRounding.AwayFromZero);
                item.PercentageAllocation = res;
                result.Add(item);
            }
            // we get a result list for return, but the next step is checking the error
            decimal sumResult = decimal.Zero;
            foreach (PercentageHolding hold in result)
                sumResult = Decimal.Add(sumResult,hold.PercentageAllocation);   // calculating new sum in the result list
            decimal error = 100m - sumResult;   // find an error
            if ((error != 0m) && (error != 100m))   // if we get error then rounding
                result = roundingError(result, error);
            return result;
        }
        // we can get error like 0.0000000001 and -0.0000000001
        // in case then error > 0 we substract error from maximum value in the list
        // in another case error < 0 we add error to the minimum value in the list
        private List<PercentageHolding> roundingError(List<PercentageHolding> list, decimal error)
        {
            int Max = 0;
            int Min = 0;
            // find index of maximum and minimum element
            for (int i = 0; i < list.Count; i++)
            {
                if (list[Max].PercentageAllocation < list[i].PercentageAllocation)
                    Max = i;
                if (list[Min].PercentageAllocation > list[i].PercentageAllocation)
                    Min = i;
            }
            // substract or add
            if (error > 0m)
                list[Max].PercentageAllocation -= error;
            else
                list[Min].PercentageAllocation += error;
            return list;
        }
    }
}