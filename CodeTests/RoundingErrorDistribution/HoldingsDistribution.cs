using System.Collections.Generic;

namespace RoundingErrorDistribution
{
    public class HoldingsDistribution
    {
        public HoldingDistributionType Type { get; set; }

        public List<Holding> Holdings { get; set; }
    }
}