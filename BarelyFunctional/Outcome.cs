using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BarelyFunctional;

public readonly struct Outcome<T> : IEquatable<Outcome<T>>
{
    private T? Value { get; }

    public bool Equals(Outcome<T> other)
    {
        throw new NotImplementedException();
    }


    private Outcome(T? value, IEnumerable<Error> errors, bool isSuccess)
    {
        Value = value;
        Errors = errors;
        IsSuccess = isSuccess;
    }


    public static Outcome<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Outcome<T>(value, [], true);
    }


    public static Outcome<TResult> Success<TResult>(TResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Outcome<TResult>(value, [], true);
    }


    public static Outcome<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Outcome<T>(default, [error], false);
    }


    public static Outcome<TResult> Failure<TResult>(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Outcome<TResult>(default, [error], false);
    }


    public static Outcome<Unit> Success() =>
        new(new Unit(), [], true);


    public static Outcome<TResult> Failure<TResult>(IEnumerable<Error>? errors) =>
        new(default, errors ?? [], false);


    public static implicit operator Outcome<T>(Error error) =>
        Failure(error);


    public static implicit operator Outcome<T>(T t) =>
        Success(t);


    public static implicit operator T(Outcome<T> outcome) =>
        outcome.IsSuccess
            ? outcome.Value!
            : throw new InvalidCastException("Outcome is not in a success state!");


    public IEnumerable<Error> Errors { get; } = [];

    public bool IsSuccess { get; }

    public bool IsFailure =>
        !IsSuccess;

    public Error? Error =>
        Errors?.FirstOrDefault();


    public Outcome<TResult> Select<TResult>(Func<T, TResult> transform)
    {
        if (IsFailure)
            return Failure<TResult>(Error!);

        try
        {
            var mappedValue = transform(Value!);

            return Success(mappedValue);
        }
        catch (Exception exception)
        {
            return Failure<TResult>(Error.FromException(exception));
        }
    }
}