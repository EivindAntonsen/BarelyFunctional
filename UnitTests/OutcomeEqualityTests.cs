using BarelyFunctional;

namespace UnitTests;

[TestFixture]
public class OutcomeEqualityTests
{
    [Test]
    public void Equals_ShouldBeTrue_ForEqualSuccesses()
    {
        var a = Outcome<int>.Success(42);
        var b = Outcome<int>.Success(42);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a == b);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        });
    }


    [Test]
    public void Equals_ShouldBeTrue_ForFailuresBuiltFromDifferentLists()
    {
        var a = Outcome<int>.Failure([Error.FromMessage("boom")]);
        var b = Outcome<int>.Failure([Error.FromMessage("boom")]);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a == b);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        });
    }


    [Test]
    public void Equals_ShouldBeFalse_ForDifferentErrors()
    {
        var a = Outcome<int>.Failure("a");
        var b = Outcome<int>.Failure("b");

        Assert.That(a, Is.Not.EqualTo(b));
    }


    [Test]
    public void DefaultOutcome_ShouldNotThrow_OnMembers()
    {
        Outcome<int> outcome = default;

        Assert.Multiple(() =>
        {
            Assert.That(outcome.IsFailure);
            Assert.That(outcome.Errors, Is.Empty);
            Assert.That(outcome.Error, Is.Null);
            Assert.DoesNotThrow(() => outcome.GetHashCode());
            Assert.DoesNotThrow(() => _ = outcome.Equals(outcome));
        });
    }


    [Test]
    public void ErrorEquality_ShouldBeValueBased_ForNestedErrors()
    {
        var a = Error.FromMany([Error.FromMessage("x"), Error.FromMessage("y")]);
        var b = Error.FromMany([Error.FromMessage("x"), Error.FromMessage("y")]);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        });
    }
}
