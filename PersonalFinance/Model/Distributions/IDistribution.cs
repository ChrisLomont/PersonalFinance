namespace Lomont.PersonalFinance.Model.Distributions
{
    public interface IDistribution
    {
        double Mean { get; set; }
        double Parameter { get; set; }
        double Sample(LocalRandom random);

    }
}
