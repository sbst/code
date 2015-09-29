using System;
using System.Collections.Generic;
using System.Linq;

//using RoundingErrorDistribution.YourCodeHere;

namespace RoundingErrorDistribution
{
    class Program
    {
        static void Main(string[] args)
        {
            var calculator = new HoldingsPercentageCalculator();

            TestAllocations(HoldingDistributionType.Good, calculator);
            TestAllocations(HoldingDistributionType.Bad, calculator);
            TestAllocations(HoldingDistributionType.Extreme, calculator);
            TestAllocations(HoldingDistributionType.Empty, calculator);

            Console.WriteLine("Press Enter to continue ...");
            Console.ReadLine();
        }

        private static void TestAllocations(HoldingDistributionType distributionType, HoldingsPercentageCalculator calculator)
        {
            var holdings = HoldingsDistributionFactory.GetDistribution(distributionType);
            var percentageHoldings = calculator.CalculatePercentages(holdings.Holdings);
            PrintResults(holdings, percentageHoldings);
        }

        private static void PrintResults(HoldingsDistribution holdings, List<PercentageHolding> percentageHoldings)
        {
            Console.WriteLine("====================================================");
            Console.WriteLine("Results for {0}", holdings.Type);
            Console.WriteLine("Input holdings: {0}", holdings.Holdings != null ? holdings.Holdings.Count : 0);
            Console.WriteLine("Result holdings: {0}", percentageHoldings != null ? percentageHoldings.Count : 0);

            if (percentageHoldings == null || !percentageHoldings.Any())
            {
                return;
            }

            Console.WriteLine("Percentage sum: {0:G}", percentageHoldings.Sum(a => a.PercentageAllocation));
        }
    }
}
