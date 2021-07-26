# Julia C API and multithreading

## Pitfall
**The Julia C API cannot be called directly from a *non-Julia* thread.** Here, a non-Julia thread refers to any thread other than the one that spawns Julia via `jl_init`. This is a inherent limitation of Julia: [Calling a Julia function from a non-Julia thread](https://github.com/JuliaLang/julia/issues/17573).

If a call to Julia C API is made in a non-Julia thread, then an access violation error occurs. Play with the following C# code to inspect this exception.
```csharp
class Program
{
    // The `Julia` class imply p/invokes libjulia.
    static void Main(string[] args)
    {
        Julia.jl_init__threading();
        IntPtr ans = Julia.jl_eval_string("nothing");
        Debug.Assert(ans != IntPtr.Zero); // all good here

        var t1 = new Thread(DoWork);
        t1.Start(); // start a new thread
        t1.Join(); // wait for t1 to finish
    }

    static void DoWork()
    {
        Julia.jl_eval_string("nothing"); // > System.AccessViolationException
    }
}
```

See also the related threads:
- https://discourse.julialang.org/t/c-embedding-interface-called-from-multiple-threads/18609/4
- https://discourse.julialang.org/t/embedding-julia-into-multithreading-apps/20122
- - https://discourse.julialang.org/t/communicating-with-external-c-threads/37696


Note also that it does not matter whether Julia is initialized in the main thread as long as all subsequent calls come from the same thread. The code below works.
```csharp
static void Main(string[] args)
{
    var t1 = new Thread(RunJuliaThread);
    t1.Start(); // start a new thread
    t1.Join(); // wait for t1 to finish
}


static void RunJuliaThread()
{
    Julia.jl_init__threading();
    IntPtr ans = Julia.jl_eval_string("1.3 + 2");
    Debug.Assert(ans != IntPtr.Zero); // all good here
    Console.WriteLine(Julia.jl_unbox_float64(ans));
}

```

The above fact is crucial when we enforce memory management for a Julia object with the common [disposal pattern](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose) in C#: a finalizer of a C# class is executed in a different thread.