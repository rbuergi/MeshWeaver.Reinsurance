using System;
using System.Collections.Immutable;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using MeshWeaver.Reinsurance.Cashflow;

namespace MeshWeaver.Reinsurance.ReinsuranceStructure;


public class NonPropOnlyAttribute : Attribute;
public class PropOnlyAttribute : Attribute;


public abstract record AcceptanceElement(Func<CashflowModel, bool> Scope)
{
}

[PropOnly]
public record ProportionalityCession(double CededPortion, double Commission, Func<CashflowModel, bool> Scope) :
    AcceptanceElement(cf => Scope(cf) && MyScopeFilter(cf))
{
    private static bool MyScopeFilter(CashflowModel cf)
        => cf.Classifier.CashflowType is CashflowType.Premium or CashflowType.Loss && cf.Classifier.CounterParty is CounterParty.Cedante;

    public ProportionalityCession(double CededPortion, double Commission)
        : this(CededPortion, Commission, _ => true) { }
}

public record ReinsuranceAcceptance
{
    public ImmutableList<AcceptanceElement> Elements { get; init; }
    public ReinsuranceAcceptance WithElement(AcceptanceElement element) =>
        this with { Elements = Elements.Add(element) };

}


[NonPropOnly]
public record ExcessOfLossLayer(double AttachmentPoint, double Cover, double Rate, double RateOnReinstatement, Func<CashflowModel, bool> Scope) : AcceptanceElement(cf => Scope(cf)&& MyScopeFilter(cf) ){

    private static bool MyScopeFilter(CashflowModel cf)
        => cf.Classifier.CashflowType is CashflowType.Loss or CashflowType.Premium && cf.Classifier.CounterParty is CounterParty.Cedante;

    public ExcessOfLossLayer(double AttachmentPoint, double Cover, double Rate, double RateOnReinstatement = 0)
    : this(AttachmentPoint, Cover,Rate,RateOnReinstatement, _ => true){}
}

