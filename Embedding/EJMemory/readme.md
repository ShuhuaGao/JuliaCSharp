# Memory management

As pointed out in the [documentation](https://docs.julialang.org/en/v1/manual/embedding/#Memory-Management), 
>Typically, Julia objects are freed by a garbage collector (GC), but the GC does not automatically know that we are holding a reference to a Julia value from C. This means the GC can free objects out from under you, rendering pointers invalid. Remember that most jl_... functions can sometimes invoke garbage collection.

That is, Julia GC does not know that we hold a reference in the .NET side. 
On the opposite side, if we pass some memory from C# to Julia without copy, then the .NET runtime for C# does not know Julia holds a reference, either. 

## Prohibit Julia from garbage collecting a variable

Since the first approach in the official guide with `JL_GC_PUSH` depends on complex macros, we use the second method here: keep a global reference to the variable in Julia until we finish our job. The official guide defines a global `IdDict` and stores a reference therein. 

```csharp
// Create a global `IdDict` during the initialization.
IntPtr REFS = Julia.jl_eval_string("const REFS = IdDict()");
IntPtr setindex_ = Julia.jl_eval_string("setindex!");
// `var` is a `Vector{Float64}`, which is mutable.
IntPtr var = Julia.jl_eval_string("[sqrt(2.0); sqrt(4.0); sqrt(6.0)]");
// To protect `var`, add its reference to `REFS`.
Julia.jl_call3(setindex_, REFS, var, var);
```
Next, we can play with this array and do some interesting work. In this example, we use `unsafe` code to access the array data. Note that no copy is done, and we are reading the data in Julia's managed memory directly.

```csharp
// read the array data with unsafe code and a raw pointer
unsafe
{
    double* data = (double*)Marshal.ReadIntPtr(var).ToPointer();
    for (int i = 0; i < 3; i++)
        Console.WriteLine(*(data + i));
}
```

