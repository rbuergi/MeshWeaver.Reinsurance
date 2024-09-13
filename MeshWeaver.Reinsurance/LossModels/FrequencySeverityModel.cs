namespace MeshWeaver.Reinsurance.LossModels;


public abstract record LossModel(double[] CashPattern);

public record FrequencySeverityModel(Distribution Frequency, Distribution Severity, double[] CashPattern);

public abstract record Distribution;

/// <summary>
/// This is the mean of the Poisson Distribution
/// </summary>
/// <param name="Lambda"></param>
public record PoissonDistribution(double Lambda) : Distribution;

/// <summary>
/// 
/// </summary>
/// <param name="X0"></param>
/// <param name="Alpha"></param>
public record ParetoDistribution(double X0, double Alpha) : Distribution;


public record AggregateLossModel(Distribution Aggregate, double[] CashPattern) : Distribution;