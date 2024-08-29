# Barely Functional

## What is this?
This is a minimal functional library for C# that provides a single monadic container for your day-to-day functional basics. It is designed to be as simple as possible, with a single monadic container that can be used to chain operations together in a functional way.

## What is Outcome<T>?
Outcome<T> is a monadic container that can hold either a value of type T or an error message. It is designed to be used in a functional way, allowing you to chain operations together in a way that is easy to understand and use.

The Outcome will be in either a Success or Failure state. If it is in a Success state, it will contain a value of type T. If it is in a Failure state, it will contain an error.
Methods can be chained with methods like `Select()`, `SelectMany()`, `Traverse()`, and any errors occurring will shortcircuit the chain and return an Outcome in a failure state with the error information.

## Why create yet another functional library?
I wanted to create a minimal functional library that was easy to understand and use.
I found that many functional libraries for C# were too complex or had too many features that I didn't need. 
I wanted something simple that I could use in my day-to-day programming without having to learn a lot of new concepts or syntax.