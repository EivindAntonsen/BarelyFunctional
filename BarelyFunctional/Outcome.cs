// ReSharper disable MemberCanBePrivate.Global

namespace BarelyFunctional;

public readonly struct Outcome<T> : IEquatable<Outcome<T>>
{
    private T? Value { get; }

    public IEnumerable<Error> Errors { get; } = [];

    public bool IsSuccess { get; }

    public bool IsFailure =>
        !IsSuccess;

    public Error? Error =>
        Errors?.FirstOrDefault();


    private Outcome(T? value, IEnumerable<Error> errors, bool isSuccess)
    {
        Value = value;
        Errors = errors;
        IsSuccess = isSuccess;
    }

    #region Initialization

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


    public static Outcome<T> Of(T value) =>
        Success(value);

    public static Outcome<T> Of(Func<T> transform)
    {
        try
        {
            return transform();
        }
        catch (Exception exception)
        {
            return Error.FromException(exception);
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

    public static Outcome<T> OfDisposable(IDisposable disposable, Func<IDisposable, T> transform)
    {
        try
        {
            return transform(disposable);
        }
        catch (Exception exception)
        {
            return Error.FromException(exception);
        }
        finally
        {
            disposable.Dispose();
        }
    }

    public static Outcome<Unit> OfDisposable(IDisposable disposable, Action<IDisposable> action)
    {
        try
        {
            action(disposable);
            return new Unit();
        }
        catch (Exception exception)
        {
            return Error.FromException(exception);
        }
        finally
        {
            disposable.Dispose();
        }
    }


    public static implicit operator Outcome<T>(Error error) =>
        Failure(error);


    public static implicit operator Outcome<T>(T value) =>
        Success(value);


    public static implicit operator T(Outcome<T> outcome) =>
        outcome.IsSuccess
            ? outcome.Value!
            : throw new InvalidCastException("Outcome is not in a success state!");

    #endregion

    #region Methods

    public Outcome<TResult> Select<TResult>(Func<T, TResult> transform)
    {
        if (IsFailure)
            return Failure<TResult>(Error!);

        try
        {
            var transformedValue = transform(Value!);

            return transformedValue is null
                ? Failure<TResult>("The function returned a null value.")
                : Success(transformedValue);
        }
        catch (Exception exception)
        {
            return Failure<TResult>(exception);
        }
    }


    public Outcome<TResult> SelectMany<TResult>(Func<T, Outcome<TResult>> transform)
    {
        if (IsFailure)
            return Failure<TResult>(Error!);

        try
        {
            return transform(Value!);
        }
        catch (Exception exception)
        {
            return Failure<TResult>(exception);
        }
    }

    public Outcome<T> Where(Predicate<T> predicate) => this switch
    {
        { IsFailure: true }
            => Failure<T>(Error!),
        { IsSuccess: true } when predicate(Value!)
            => Success<T>(Value!),
        { IsSuccess: true }
            => Failure<T>("Although the Outcome was a success, its value did not match the provided predicate."),
        _
            => throw new ArgumentOutOfRangeException()
    };


    public Outcome<T> ForEach(Action<T> action)
    {
        if (IsSuccess)
            action(Value!);

        return this;
    }


    public TResult Apply<TResult>(Func<Outcome<T>, TResult> func) =>
        func(this);


    public TResult Match<TResult>(Func<T, TResult> success, Func<Error, TResult> failure) =>
        IsFailure
            ? failure(Error!)
            : success(Value!);

    public Outcome<T> SelectError(Func<Error, Error> transform) =>
        IsFailure
            ? Failure(transform(Error!))
            : this;


    public Outcome<T> Also(Action action)
    {
        action();
        return this;
    }

    #endregion

    #region Equality

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

    #endregion
}