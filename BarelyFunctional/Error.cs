using System.Net;

namespace BarelyFunctional;

public record Error
{
    private Error(Exception? exception = null, string? errorMessage = null, List<Error>? errors = null)
    {
        Exception = exception;
        Message = errorMessage;
        _errors = errors ?? [];
    }

    private readonly IEnumerable<Error> _errors;

    public bool IsExceptional =>
        Exception is not null || Errors.Any(error => error.IsExceptional) == true;

    public Exception? Exception { get; }
    public string? Message { get; }
    public IEnumerable<Error> Errors => _errors ?? [this];

    public static Error FromException(Exception exception) =>
        new(exception);

    public static Error FromMessage(string errorMessage) =>
        new(default, errorMessage);

    public static Error FromMany(IEnumerable<Error> errors) =>
        new(default, null, errors.ToList());

    public static implicit operator Error(string message) => 
        FromMessage(message);

    public static implicit operator Error(Exception exception) => 
        FromException(exception);
}