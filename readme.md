# Barely Functional

## What is this?
This is a minimal functional library for C# that provides a single monadic container for your day-to-day functional basics. It is designed to be as simple as possible, with a single monadic container that can be used to chain operations together in a functional way.

## What is a monad?
A monadic container is a type that wraps a value and provides a way to chain operations together. In this library, the monadic container is called `Outcome<T>`. It is meant to represent the outcome of an operation or computation, and can be used to chain operations together in a functional way. The main benefit is it forces the developer to acknowledge both cases of an attempt at something: the successful and the failed cases.

The `Outcome<T>` monad can capture a computation or result of an action by wrapping the lambda in `Outcome<T>.Of(() => {})`. 
Then you can chain operations like `Select()`, `SelectMany()`, `ForEach()` to perform actions or functions on the contained value, or you can call `Match()` to handle the success or failure cases.

If you're dealing with collections, you may find the `Traverse()` or `Sequence()` methods to be helpful. 

If any operation in the chain of methods fails, the chain will be short-circuited and the final `Outcome<T>` will be in a failed state, with the error information stored.

## Why create yet another functional library?
I wanted to create a minimal functional library that was easy to understand and use.
I found that many functional libraries for C# were too complex or had too many features that I didn't need. 
I wanted something simple that I could use in my day-to-day programming without having to learn a lot of new concepts or syntax.