# Julia & C# interoperation via embedding

## Principle
As documented in [Embedding Julia](https://docs.julialang.org/en/v1/manual/embedding/#Embedding-Julia), Julia has exposed a set of [C API](https://github1s.com/JuliaLang/julia/blob/HEAD/src/julia.h) to allow other languages to integrate Julia code.  

In C# (or VB.net), we can resort to the [P/Invoke](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke) technique to call a C/C++ API. The overall work flow is thus:

![Workflow](workflow.png)

As emphasized above, C# and Julia have two separate worlds of managed memory. Proper memory management is critical.

## Tutorials
1. [Introduction](./EJStarter/): basic concepts and operations