using BarelyFunctional;

namespace UnitTests;

[TestFixture]
public class OutcomeOperationsTests
{
    [Test]
    public void Select_ShouldTransform_WhenSuccess()
    {
        var outcome = Outcome<int>.Success(21).Select(value => value * 2);

        Assert.Multiple(() =>
        {
            Assert.That(outcome.IsSuccess);
            Assert.That(outcome.Match(value => value, _ => 0), Is.EqualTo(42));
        });
    }


    [Test]
    public void Select_ShouldPreserveAllErrors_WhenFailure()
    {
        var outcome = Outcome<int>
            .Failure([Error.FromMessage("a"), Error.FromMessage("b")])
            .Select(value => value * 2);

        Assert.Multiple(() =>
        {
            Assert.That(outcome.IsFailure);
            Assert.That(outcome.Errors.Count(), Is.EqualTo(2));
        });
    }


    [Test]
    public void Select_ShouldReturnFailure_WhenTransformThrows()
    {
        var outcome = Outcome<int>.Success(1).Select<int>(_ => throw new InvalidOperationException("boom"));

        Assert.Multiple(() =>
        {
            Assert.That(outcome.IsFailure);
            Assert.That(outcome.Error!.IsExceptional);
        });
    }


    [Test]
    public void SelectMany_ShouldChain_WhenSuccess()
    {
        var outcome = Outcome<int>.Success(21)
            .SelectMany(value => Outcome<int>.Success(value * 2));

        Assert.That(outcome.Match(value => value, _ => 0), Is.EqualTo(42));
    }


    [Test]
    public void SelectMany_ShouldReturnFailure_WhenInnerFails()
    {
        var outcome = Outcome<int>.Success(21)
            .SelectMany(_ => Outcome<int>.Failure("nope"));

        Assert.That(outcome.IsFailure);
    }


    [Test]
    public void Match_ShouldInvokeFailureBranch_WhenFailure()
    {
        var outcome = Outcome<int>.Failure("oops");

        var message = outcome.Match(_ => "success", error => error.Message!);

        Assert.That(message, Is.EqualTo("oops"));
    }
}
