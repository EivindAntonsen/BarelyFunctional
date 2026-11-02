using BarelyFunctional;

namespace UnitTests;

[TestFixture]
public class ExtensionsTests
{
    [Test]
    public void Sequence_ShouldReturnSuccess_WhenAllSucceed()
    {
        var outcomes = new[]
        {
            Outcome<int>.Success(1),
            Outcome<int>.Success(2),
            Outcome<int>.Success(3)
        };

        var result = outcomes.Sequence();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Match(values => values, _ => []), Is.EqualTo(new[] { 1, 2, 3 }));
        });
    }


    [Test]
    public void Sequence_ShouldAggregateAllErrors_WhenAnyFail()
    {
        var outcomes = new[]
        {
            Outcome<int>.Success(1),
            Outcome<int>.Failure("first"),
            Outcome<int>.Failure("second")
        };

        var result = outcomes.Sequence();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure);
            Assert.That(result.Errors.Count(), Is.EqualTo(2));
        });
    }


    [Test]
    public void Traverse_ShouldReturnSuccess_WhenAllSucceed()
    {
        var numbers = new[] { "1", "2", "3" };

        var result = numbers.Traverse(number => Outcome<int>.Of(() => int.Parse(number)));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Match(values => values, _ => []), Is.EqualTo(new[] { 1, 2, 3 }));
        });
    }


    [Test]
    public void Traverse_ShouldAggregateAllErrors_ConsistentWithSequence()
    {
        var numbers = new[] { "1", "nope", "also-nope" };

        var result = numbers.Traverse(number => Outcome<int>.Of(() => int.Parse(number)));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure);
            Assert.That(result.Errors.Count(), Is.EqualTo(2));
        });
    }


    [Test]
    public void ToUnit_ShouldPreserveAllErrors_WhenFailure()
    {
        var outcome = Outcome<int>.Failure([Error.FromMessage("a"), Error.FromMessage("b")]);

        var unit = outcome.ToUnit();

        Assert.Multiple(() =>
        {
            Assert.That(unit.IsFailure);
            Assert.That(unit.Errors.Count(), Is.EqualTo(2));
        });
    }
}
