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
    public void OutcomeOfDisposable_ShouldReturnSuccessOutcome_WhenTransformSucceeds()
    {
        var disposable = new TestDisposable();
        var outcome = Outcome<int>.OfDisposable(disposable, _ => 42);

        Assert.Multiple(() =>
        {
            Assert.That(outcome.IsSuccess);
            Assert.That(outcome.Match(value => value, _ => 0), Is.EqualTo(42));
            Assert.That(disposable.IsDisposed);
        });
    }


    [Test]
    public void OutcomeOfDisposable_WhenTransformFails_ShouldReturnFailure()
    {
        var disposable = new TestDisposable();
        var outcome = Outcome<int>.OfDisposable(disposable, _ => throw new InvalidOperationException("Test exception"));

        Assert.Multiple(() =>
        {
            Assert.That(outcome.IsFailure);
            Assert.That(outcome.Error?.Exception?.Message, Is.EqualTo("Test exception"));
            Assert.That(disposable.IsDisposed);
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


    [Test]
    public void OutcomeOfDisposable_AfterFunction_ShouldDispose()
    {
        var disposable = new TestDisposable();

        var outcome = Outcome<string>.OfDisposable(disposable, _ => throw new InvalidOperationException("Test exception"));

        Assert.Multiple(() =>
        {
            Assert.True(outcome.IsFailure);
            Assert.True(outcome.Error?.IsExceptional);
            Assert.AreEqual(outcome.Error?.Exception?.Message, "Test exception");
            Assert.True(disposable.IsDisposed);
        });
    }


    private sealed class TestDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; } = false;

        public bool IsDisposing { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            IsDisposing = true;

            if (disposing)
            {
                // Dispose managed resources
            }

            // Dispose unmanaged resources
            IsDisposed = true;
            IsDisposing = false;
        }
    }
}