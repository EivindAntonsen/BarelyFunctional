using BarelyFunctional;

namespace UnitTests;

[TestFixture]
public class OutcomeTests
{
    [Test]
    public void Outcome_CanBeFilteredForSpecificSuccess_WithLINQ()
    {
        var outcome =
            from value in Outcome<int>.Of(3)
            where value > 4
            select value;

        Assert.That(outcome.IsFailure);
    }
}