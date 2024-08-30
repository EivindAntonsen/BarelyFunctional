# Barely Functional

## What is this?
This is a minimal functional library for C# that provides a single monadic container for your day-to-day functional basics. It is designed to be as simple as possible, with a single monadic container that can be used to chain operations together in a functional way.

## What is `Outcome<T>`?
Outcome is a monadic container that can hold either a value of type T or an error message. 
It is designed to be used in a functional way, allowing you to chain operations together in a way that is easy to understand and use.

## Usage
````csharp
List<string> numbers = new List<string> { "7", "2", "4", "9", "8", "3", "1" };

HttpResponse response = numbers
    // Wrap the parsing of each number in their own Outcome<int>
    .Select(number => Outcome<int>.Of(() => int.Parse(number)))
    // Invert the IEnumerable<Outcome<int>> structure into the much easier
    // Outcome<IEnumerable<int>> structure with Traverse.
    .Traverse(i => i)
    // Filter the successful outcome with a predicate.
    .Where(ints => ints.Any(int.IsEvenInteger))
    // Handle both success and error cases with Match.
    .Match(success => HttpResponse.Ok(success),
           error   => HttpResponse.NotFound(error.Message));
````


## Installation
The project exists on nuget: https://www.nuget.org/packages/BarelyFunctional
It can be installed through your package manager of choice.

## Why create yet another functional library?
I wanted to create a minimal functional library that was easy to understand and use.
I found that many functional libraries for C# were too complex or had too many features that I didn't need. 
I wanted something simple that I could use in my day-to-day programming without having to learn a lot of new concepts or syntax.
