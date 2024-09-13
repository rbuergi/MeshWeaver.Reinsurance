using System.Linq;
using FluentAssertions;
using MeshWeaver.MonteCarlo;
using MeshWeaver.Reinsurance.Cashflow;
using MeshWeaver.Reinsurance.ReinsuranceStructure;
using Xunit;

namespace MeshWeaver.Reinsurance.Test
{
    public class ReinsuranceEvaluationTest
    {
        private static readonly CashflowModel[] TestCashflow =
        [
            new CashflowModel(new() { CashflowType = CashflowType.Loss }, [0, 0.5, 1, 1.5, 2, 2.5]),
            new CashflowModel(new() { CashflowType = CashflowType.Premium }, [3, 3, 3, 3, 3, 3]),
        ];

        [Fact]
        public void EvaluateExcessOfLossCession()
        {
            var xl = new ExcessOfLossLayer(1, 1, 0.05);
            var ceded = TestCashflow.Cede(xl).ToArray();
            ceded.Should().HaveCount(4);
            ceded.Single(cf =>
                    cf.Classifier.CashflowType is CashflowType.Loss &&
                    cf.Classifier.CounterParty is CounterParty.Reinsurer)
                .Cashflow.Should().BeEquivalentTo([0, 0, 0, 0.5, 1, 1]);
        }
        [Fact]
        public void EvaluatePropLossCession()
        {
            var prop = new ProportionalityCession(0.5, 0.1);
            var ceded = TestCashflow.Cede(prop).ToArray();
            ceded.Should().HaveCount(5);
            ceded.Single(cf =>
                    cf.Classifier.CashflowType is CashflowType.Loss &&
                    cf.Classifier.CounterParty is CounterParty.Reinsurer)
                .Cashflow.Should().BeEquivalentTo([0, 0.25, 0.5, 0.75, 1, 1.25]);

            var cededPremiums = TestCashflow[1].Cashflow.Times(prop.CededPortion - prop.Commission);

            ceded.Single(cf =>
                    cf.Classifier.CashflowType is CashflowType.Premium &&
                    cf.Classifier.CounterParty is CounterParty.Reinsurer)
                .Cashflow.Minus(cededPremiums)
                .Select(x => x*x)
                .Sum()
                .Should().BeLessThan(1e-6);

        }
    }
}
