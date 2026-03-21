# AGENTS.md

This file provides guidance to AI coding agents working in this repository.

## Build

This is a .NET Framework 4.5 C# project using the old-style `.csproj` format (MSBuild ToolsVersion 12.0, Visual Studio 2013). It uses NuGet packages restored via `packages/` directory (not PackageReference).

```bash
msbuild Markov.sln            # build the solution
msbuild Markov.sln /p:Configuration=Release  # release build
```

There are no tests in this project.

## Usage

The CLI is a name generator powered by Markov chains. It reads a training file (one name per line) and generates new names:

```bash
Markov.exe -t <training-file> -n <count> [-o <order>] [-s <seed>] [-l] [-b <base>] [-p <step>]
```

Key options: `-t` training file (required), `-n` count (required), `-o` chain order (default 3), `-l` enables Laplace smoothing.

## Architecture

- **`MarkovModel<T>`** — Generic, n-order Markov chain. Stores learned transitions as a recursive tree of `Dictionary<T, MarkovNode<T>>` (depth = order + 1). Supports configurable chain order, Laplace smoothing, and adjustable probability weights (`BaseProbability`, `StepProbability`). This is the active implementation used by `Program.cs`.

- **`MarkovGenerator`** — Older, non-generic implementation hardcoded to characters with a fixed 2nd-order chain using a 3D probability array. Not currently used by `Program.cs`.

- **`MarkovNode<T>`** — Tree node with two forms: leaf nodes (hold a probability counter) and branch nodes (hold a `Children` dictionary). The `IsLeaf` property distinguishes them based on whether `Children` is null.

- **`Options`** — CLI argument parsing via CommandLineParser 2.9.1.

- **`Program`** — Entry point in the `NameGenerator` namespace. Trains a `MarkovModel<char>` from a file, then generates names that weren't in the training set.

## Dependencies

- **CommandLineParser 2.9.1** — via NuGet `packages/` directory.
