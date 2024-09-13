namespace MeshWeaver.Reinsurance.Cashflow;

public record CashflowModel(BusinessClassifier Classifier,  double[] Cashflow)
{
}

public enum CashflowType
{
    Premium,
    Loss,
    InternalExpense,
    ExternalExpense,
    CapitalCost,
    Profit,
    Result,
}

public record BusinessClassifier()
{
    public int Year { get; init; }
    public string LoB { get; init; }
    public string Country { get; init; }
    public CounterParty CounterParty{get;init;}
    public CashflowType CashflowType{get;init;}
}

public enum CounterParty
{
    Cedante,
    Reinsurer,
}