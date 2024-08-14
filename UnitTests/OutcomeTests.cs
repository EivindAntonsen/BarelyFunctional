using BarelyFunctional;
using System;

namespace UnitTests;

[TestFixture]
public class OutcomeTests
{
    

    [Test]
    public void FromException_ShouldCreateError_WithOnlyException()
    {
        var exception = new ArgumentNullException();
        var error = Error.FromException(exception);

        Assert.That(error.Exception, Is.EqualTo(exception));
        Assert.That(error.Message, Is.Null);
    }


    [Test]
    public void FromMessage_ShouldCreateError_WithOnlyMessage()
    {
        const string message = "This didn't work";
        var error = Error.FromMessage(message);

        Assert.That(error.Message, Is.EqualTo(message));
        Assert.That(error.Exception, Is.Null);
    }
}