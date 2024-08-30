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

public static class OutcomeExtensions
{
    public static IEnumerable<Error> Errors<T>(this IEnumerable<Outcome<T>> collection) => collection
        .Select(outcome => outcome.Error)
        .OfType<Error>();


    public static Outcome<Unit> ToUnit<T>(this Outcome<T> outcome) =>
        outcome.IsSuccess
            ? Outcome<Unit>.Success()
            : Outcome<Unit>.Failure(outcome.Error!);

    /// <summary>
    /// Takes a collection of outcomes and returns an outcome that represents the result of the sequence operation.
    /// </summary>
    /// <typeparam name="T">The type of each outcome in the input collection.</typeparam>
    /// <param name="collection">The input collection of outcomes.</param>
    /// <returns>
    /// An outcome that represents the result of the sequence operation.
    /// If any outcome in the collection is a failure, the resulting outcome will also be a failure with the corresponding errors.
    /// If all outcomes in the collection are successes, the resulting outcome will be a success with a collection of the values.
    /// </returns>
    public static Outcome<IEnumerable<T>> Sequence<T>(
        this IEnumerable<Outcome<T>> collection)
    {
        var list = collection.ToList();

        if (list.Any(outcome => outcome.IsFailure))
            return Outcome<IEnumerable<T>>.Failure<IEnumerable<T>>(list.Errors());

        var values = list
            .Select(outcome => outcome.Match<T?>(value => value, _ => default))
            .OfType<T>();

        return Outcome<IEnumerable<T>>.Success<IEnumerable<T>>(values);
    }


    /// <summary>
    /// Traverses a collection and applies a function to each element, returning an outcome that represents the result of the traversal.
    /// </summary>
    /// <typeparam name="TOutput">The type of the elements in the resulting collection.</typeparam>
    /// <typeparam name="TInput">The type of the elements in the input collection.</typeparam>
    /// <param name="collection">The input collection to traverse.</param>
    /// <param name="func">The function to apply to each element in the collection.</param>
    /// <returns>
    /// An outcome that represents the result of the traversal.
    /// If all elements in the collection were successfully transformed, the outcome will contain a collection of the transformed values.
    /// If any element fails the transformation, the outcome will contain an error indicating the failure.
    /// </returns>
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