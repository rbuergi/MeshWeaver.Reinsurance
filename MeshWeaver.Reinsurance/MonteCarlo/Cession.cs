using System;
using MeshWeaver.Reinsurance.Cashflow;
using MeshWeaver.Reinsurance.ReinsuranceStructure;
using System.Linq;
using System.Collections.Generic;

namespace MeshWeaver.MonteCarlo;

public static class CessionExtension
{
    public static IEnumerable<CashflowModel> Cede(this IEnumerable<CashflowModel> inflowing, AcceptanceElement element)
        => Cede(inflowing, (dynamic)element);

    private static IEnumerable<CashflowModel> Cede(IEnumerable<CashflowModel> inflowing, ProportionalityCession cession)
        => inflowing.SelectMany(cf => cession.Scope(cf) ? Cede(cf, cession) : new[] { cf });

    private static IEnumerable<CashflowModel> Cede(CashflowModel cf, ProportionalityCession cession)
    {
        if (cf.Classifier.CashflowType is CashflowType.Loss)
        {
            var ceded = cf with
            {
                Cashflow = cf.Cashflow.Select(x => x * cession.CededPortion).ToArray(),
                Classifier = cf.Classifier with { CounterParty = CounterParty.Reinsurer }
            };
            return new[]
            {
                ceded,
                cf with { Cashflow = cf.Cashflow.Select(x => x * (1 - cession.CededPortion)).ToArray() }
            };

        }
        else
        {
            var ceded = cf with
            {
                Cashflow = cf.Cashflow.Select(x => x * (cession.CededPortion - cession.Commission)).ToArray(),
                Classifier = cf.Classifier with { CounterParty = CounterParty.Reinsurer }
            };
            return new[]
            {
                ceded,
                cf with { Cashflow = cf.Cashflow.Select(x => x * cession.Commission).ToArray(), 
                    Classifier = cf.Classifier with{CounterParty = CounterParty.Reinsurer, CashflowType = CashflowType.ExternalExpense}},
                cf with { Cashflow = cf.Cashflow.Select(x => x * (1 - cession.CededPortion)).ToArray() }
            };

        }


    }

    private static IEnumerable<CashflowModel> Cede(IEnumerable<CashflowModel> inflowing, ExcessOfLossLayer cession)
        => inflowing.SelectMany(cf => cession.Scope(cf) ? Cede(cf, cession) : new[] { cf });

    private static IEnumerable<CashflowModel> Cede(CashflowModel cf, ExcessOfLossLayer cession)
    {
        if (cf.Classifier.CashflowType is CashflowType.Loss)
        {
            var ceded = cf with
            {
                Cashflow = cf.Cashflow.Select(x => Math.Max(0, Math.Min(x - cession.AttachmentPoint, cession.Cover)))
                    .ToArray(),
                Classifier = cf.Classifier with { CounterParty = CounterParty.Reinsurer }
            };
            return new[]
            {
                ceded,
                cf with { Cashflow = cf.Cashflow.Minus(ceded.Cashflow) }
            };

        }

        else
        {
            var ceded = cf with
            {
                Cashflow = cf.Cashflow.Select(x => x * cession.Rate)
                    .ToArray(),
                Classifier = cf.Classifier with { CounterParty = CounterParty.Reinsurer }
            };
            return new[]
            {
                ceded,
                cf with { Cashflow = cf.Cashflow.Minus(ceded.Cashflow) }
            };

        }
    }

    public static double[] Minus(this double[] a, double[] b)
        => a.Zip(b, (x, y) => x - y).ToArray();
    public static double[] Plus(this double[] a, double[] b)
        => a.Zip(b, (x, y) => x + y).ToArray();
   public static double[] Times(this double[] a, double scalar)
        => a.Select(x => x*scalar).ToArray();
}