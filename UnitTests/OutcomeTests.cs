using BarelyFunctional;

namespace UnitTests;

[TestFixture]
public class OutcomeTests
{
    [Test]
    public void OutcomeSuccess_ShouldReturn_SuccessOutcome()
    {
        var outcome = Outcome<int>.Success(42);

        Assert.Multiple(() =>
        {
            Assert.That(outcome.IsSuccess);
            Assert.That(outcome.Match(value => value, _ => 0), Is.EqualTo(42));
        });
    }


    [Test]
    public void OutcomeFailure_ShouldReturn_FailureOutcome()
    {
        var error = Error.FromMessage("Test error");
        var outcome = Outcome<int>.Failure(error);

        Assert.Multiple(() =>
        {
            Assert.That(outcome.IsFailure);
            Assert.That(outcome.Error, Is.EqualTo(error));
        });
    }


    [Test]
    public void OutcomeOf_ShouldReturnSuccessOutcome_WhenTransformSucceeds()
    {
        var outcome = Outcome<int>.Of(() => 42);

        Assert.Multiple(() =>
        {
            Assert.That(outcome.IsSuccess);
            Assert.That(outcome.Match(value => value, _ => 0), Is.EqualTo(42));
        });
    }


    [Test]
    public void OutcomeWhere_SpecifiedValueDoesNotExist_ShouldReturnFailure()
    {
        var outcome =
            from value in Outcome<int>.Of(3)
            where value > 4
            select value;

        Assert.That(outcome.IsFailure);
    }


    [Test]
    public void OutcomeWhere_SpecifiedValueExists_ShouldReturnSuccess()
    {
        var outcome =
            from value in Outcome<int>.Of(3)
            where value > 2
            select value;

        Assert.That(outcome.IsSuccess);
    }
}