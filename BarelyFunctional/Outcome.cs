using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BarelyFunctional;

public readonly struct Outcome<T> : IEquatable<Outcome<T>>
{
    private T? Value { get; }


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


    public Outcome<TResult> SelectMany<TResult>(Func<T, Outcome<TResult>> transform)
    {
        if (IsFailure)
            return Failure<TResult>(Error!);

        try
        {
            var outcome = transform(Value!);

            return outcome;
        }
        catch (Exception exception)
        {
            return Failure<TResult>(Error.FromException(exception));
        }
    }


    public void ForEach(Action<T> action)
    {
        if (IsFailure)
            return;

        action(Value!);
    }


    public static Outcome<T> Of(Func<T> transform)
    {
        try
        {
            var result = transform();

            return Success(result);
        }
        catch (Exception exception)
        {
            return Failure(Error.FromException(exception));
        }
    }


    public static Outcome<Unit> Of(Action action)
    {
        try
        {
            action();
            return new Unit();
        }
        catch (Exception exception)
        {
            return Error.FromException(exception);
        }
    }


    public TResult Match<TResult>(Func<T, TResult> success, Func<Error, TResult> failure) =>
        IsFailure ? failure(Error!) : success(Value!);


    public Outcome<T> Also(Action action)
    {
        action();
        return this;
    }


    [Obsolete("Nulls? Where we're going we don't need nulls. ~Doc Brown (paraphrased - please don't use this method)", false)]
    public T? GetValueOrDefault() =>
        IsSuccess ? Value : default;


    public bool Equals(Outcome<T> other) =>
        EqualityComparer<T?>.Default.Equals(Value, other.Value)
        && Errors.Equals(other.Errors)
        && IsSuccess == other.IsSuccess;


    public override bool Equals(object? obj) =>
        obj is Outcome<T> other && Equals(other);


    public override int GetHashCode() =>
        HashCode.Combine(Value, Errors, IsSuccess);


    public static bool operator ==(Outcome<T> left, Outcome<T> right) =>
        left.Equals(right);


    public static bool operator !=(Outcome<T> left, Outcome<T> right) =>
        !(left == right);
}