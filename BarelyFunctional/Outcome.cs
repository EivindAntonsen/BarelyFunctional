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
            var mappedValue = transform(Value!);

            return mappedValue is null
                ? Failure<TResult>("The function returned a null value.")
                : Success(mappedValue);
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
            var outcome = transform(Value!);

            return outcome;
        }
        catch (Exception exception)
        {
            return Failure<TResult>(exception);
        }
    }

    public Outcome<T> Where(Predicate<T> predicate) => this switch
    {
        _ when IsFailure =>
            Failure<T>(Error!),
        _ when IsSuccess && predicate(Value!) =>
            Success<T>(Value!),
        _ when IsSuccess && !predicate(Value!) =>
            Failure<T>("Although the Outcome was a success, its value did not match the provided predicate."),
        _ =>
            throw new ArgumentOutOfRangeException()
    };


    public Outcome<T> ForEach(Action<T> action)
    {
        if (IsSuccess)
            action(Value!);

        return this;
    }


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

public static class OutcomeExtensions
{
    public static IEnumerable<Error> Errors<T>(this IEnumerable<Outcome<T>> collection) => collection
        .Select(outcome => outcome.Error)
        .OfType<Error>();


    public static Outcome<Unit> ToUnit<T>(this Outcome<T> outcome) =>
        outcome.IsSuccess
            ? Outcome<Unit>.Success(new Unit())
            : Outcome<Unit>.Failure(outcome.Error!);


    public static Outcome<IEnumerable<T>> Sequence<T>(this IEnumerable<Outcome<T>> collection)
    {
        var list = collection.ToList();

        if (list.Any(outcome => outcome.IsFailure))
            return Outcome<IEnumerable<T>>.Failure<IEnumerable<T>>(list.Errors());

        var values = list
            .Select(outcome => outcome.Match<T?>(value => value, _ => default))
            .OfType<T>();

        return Outcome<IEnumerable<T>>.Success<IEnumerable<T>>(values);
    }


    public static Outcome<IEnumerable<TOutput>> Traverse<TOutput, TInput>(
        this IEnumerable<TInput> collection,
        Func<TInput, Outcome<TOutput>> func)
    {
        var values = new List<TOutput>();

        foreach (var item in collection)
        {
            var result = func(item);

            if (result.IsSuccess)
            {
                var value = result.Match(value => value, _ => default!);

                values.Add(value);
            }
            else
            {
                var error = result switch
                {
                    { IsFailure: true } when result.Errors.Count() > 1
                        => Error.FromMany(result.Errors),
                    { IsFailure: true } when result.Error!.IsExceptional
                        => Error.FromException(result.Error!.Exception!),
                    { IsFailure: true } when result.Error!.IsExceptional == false
                        => Error.FromMessage(result.Error!.Message!),
                    _
                        => throw new ArgumentOutOfRangeException(nameof(result), "Outcome was in an invalid state!")
                };

                return Outcome<IEnumerable<TOutput>>.Failure<IEnumerable<TOutput>>(error);
            }
        }

        return Outcome<IEnumerable<TOutput>>.Success(values.AsEnumerable());
    }
}