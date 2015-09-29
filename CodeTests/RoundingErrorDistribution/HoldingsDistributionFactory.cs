using System;
using System.Collections.Generic;

namespace RoundingErrorDistribution
{
    public class HoldingsDistributionFactory
    {
        public static HoldingsDistribution GetDistribution(HoldingDistributionType distributionType)
        {
            switch (distributionType)
            {
                case HoldingDistributionType.Good:
                    return GetGoodDistributionHoldings();
                case HoldingDistributionType.Bad:
                    return GetBadDistributionHoldings();
                case HoldingDistributionType.Extreme:
                    return GetExtremeDistributionHoldings();  
                case HoldingDistributionType.Empty:
                    return GetEmptyDistributionHoldings();
                default:
                    throw new ArgumentOutOfRangeException("distributionType", distributionType, null);
            }
        }

        private static HoldingsDistribution GetEmptyDistributionHoldings()
        {
            var holdings = new List<Holding>();
            return new HoldingsDistribution { Type = HoldingDistributionType.Empty, Holdings = holdings };
        }

        private static HoldingsDistribution GetExtremeDistributionHoldings()
        {
            var holdings = new List<Holding>();
            holdings.Add(new Holding { Identifier = 123, Name = "Fund A", Value = 9910m });
            holdings.Add(new Holding { Identifier = 654, Name = "Fund B", Value = 45m });
            holdings.Add(new Holding { Identifier = 789, Name = "Fund C", Value = 45m });
            return new HoldingsDistribution { Type = HoldingDistributionType.Extreme, Holdings = holdings };
        }

        private static HoldingsDistribution GetBadDistributionHoldings()
        {
            var holdings = new List<Holding>();
            holdings.Add(new Holding { Identifier = 123, Name = "Fund A", Value = 3044m });
            holdings.Add(new Holding { Identifier = 654, Name = "Fund B", Value = 4045m });
            holdings.Add(new Holding { Identifier = 789, Name = "Fund C", Value = 244m });
            holdings.Add(new Holding { Identifier = 741, Name = "Fund D", Value = 1324m });
            holdings.Add(new Holding { Identifier = 963, Name = "Fund E", Value = 1321m });
            return new HoldingsDistribution { Type = HoldingDistributionType.Bad, Holdings = holdings };
        }

        private static HoldingsDistribution GetGoodDistributionHoldings()
        {
            var holdings = new List<Holding>();
            holdings.Add(new Holding{ Identifier = 123, Name = "Fund A", Value = 100m });
            holdings.Add(new Holding{ Identifier = 654, Name = "Fund B", Value = 200m });
            holdings.Add(new Holding{ Identifier = 789, Name = "Fund C", Value = 200m });
            holdings.Add(new Holding{ Identifier = 741, Name = "Fund D", Value = 100m });
            holdings.Add(new Holding{ Identifier = 963, Name = "Fund E", Value = 400m });

            return new HoldingsDistribution { Type = HoldingDistributionType.Good, Holdings = holdings };
        }
    }
}