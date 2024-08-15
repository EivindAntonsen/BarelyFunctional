using BarelyFunctional;
using System;

namespace UnitTests;

[TestFixture]
public class ErrorTests
{
    [Test]
    public void FromException_ShouldCreateError_WithOnlyException()
    {
        var exception = new ArgumentNullException();
        var error = Error.FromException(exception);

        Assert.Multiple(() =>
        {
            Assert.That(error.Exception, Is.EqualTo(exception));
            Assert.That(error.Message, Is.Null);
        });
    }


    [Test]
    public void FromMessage_ShouldCreateError_WithOnlyMessage()
    {
        const string message = "This didn't work";
        var error = Error.FromMessage(message);

        Assert.Multiple(() =>
        {
            Assert.That(error.Message, Is.EqualTo(message));
            Assert.That(error.Exception, Is.Null);
        });
    }


    [Test]
    public void FromMany_ShouldCreateError_WithAllDetails()
    {
        int? someValue = null;
        var nullException = new ArgumentNullException(nameof(someValue));
        var nulLError = Error.FromException(nullException);

        const string message = "This didn't work";
        var messageError = Error.FromMessage(message);

        var errors = Error.FromMany([nulLError, messageError]);

        Assert.Multiple(() =>
        {
            Assert.That(errors.Errors.Any(error => error.IsExceptional && error.Exception == nullException));
            Assert.That(errors.Errors.Any(error => error is { IsExceptional: false, Message: message }));
        });
    }
}