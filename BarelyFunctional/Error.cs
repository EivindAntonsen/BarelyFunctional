
// ReSharper disable MemberCanBePrivate.Global

namespace BarelyFunctional;

public record Error
{
    private Error(Exception? exception = null, string? errorMessage = null, IEnumerable<Error>? errors = null)
    {
        Exception = exception;
        Message = errorMessage;
        Errors = errors ?? [];
    }


    public bool IsExceptional =>
        Exception is not null || Errors.Any(error => error.IsExceptional);


    public Exception? Exception { get; }

    public string? Message { get; }

    public IEnumerable<Error> Errors { get; }


    public static Error FromException(Exception exception) =>
        new(exception);


    public static Error FromMessage(string errorMessage) =>
        new(default, errorMessage);


    public static Error FromMany(IEnumerable<Error> errors) =>
        new(default, null, errors);


    public static implicit operator Error(string message) =>
        FromMessage(message);


    public static implicit operator Error(Exception exception) =>
        FromException(exception);
}