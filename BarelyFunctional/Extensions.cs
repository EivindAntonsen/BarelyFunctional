namespace BarelyFunctional;

public static class Extensions 
{
    public static IEnumerable<Error> Errors<T>(this IEnumerable<Outcome<T>> collection) => collection
        .Select(outcome => outcome.Error)
        .OfType<Error>();
    
    
    public static Outcome<Unit> ToUnit<T>(this Outcome<T> outcome) =>
        outcome.IsSuccess
            ? Outcome<Unit>.Success()
            : Outcome<Unit>.Failure(outcome.Errors);
    
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
            return Outcome<IEnumerable<T>>.Failure(list.Errors());
    
        var values = list
            .Select(outcome => outcome.Match<T?>(value => value, _ => default))
            .OfType<T>();
    
        return Outcome<IEnumerable<T>>.Success(values);
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
    /// If any element fails the transformation, the outcome will aggregate all the errors, consistent with <see cref="Sequence{T}"/>.
    /// </returns>
    public static Outcome<IEnumerable<TOutput>> Traverse<TOutput, TInput>(
        this IEnumerable<TInput> collection,
        Func<TInput, Outcome<TOutput>> func) =>
        collection
            .Select(func)
            .Sequence();
}