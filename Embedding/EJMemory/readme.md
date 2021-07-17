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

## Share C# array with Julia and .NET memory pinning
If there exists already an array in C#, how can we pass it to Julia? One obvious way is to first allocate an array (of a sufficient size) in Julia with `jl_alloc_array_1d` and then *copy* the data from C# array to the Julia array. Alternatively, we may want to **avoid the copy** and let Julia access the C# array data directly. 

Recall that C# (and the .NET runtime) has its own managed memory space, which is distinct from the managed memory of Julia. Another key issue with the .NET managed memory is that the runtime may move the object to a different memory location freely (we will not feel it if we don't work with pointers though). Consequently, if we simply pass a pointer to a location in C# managed memory into a Julia function, the pointer may become invalid sometime. That's why memory *pinning* is critical when passing data from C# to Julia without copy. See [Copying and Pinning](https://docs.microsoft.com/en-us/dotnet/framework/interop/copying-and-pinning) for more details.
>Pinning temporarily locks the data in its current memory location, thus keeping it from being relocated by the common language runtime's garbage collector. The marshaler pins data to reduce the overhead of copying and enhance performance. The type of the data determines whether it is copied or pinned during the marshaling process. Pinning is automatically performed during marshaling for objects such as String, however you can also manually pin memory using the `GCHandle` class.

In general applications, we pass mostly arrays of primitive types between C# and Julia. It is fortunate that the .NET runtime supports pinning of such types automatically (see [doc source](https://docs.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types))
>As an optimization, arrays of blittable primitive types and classes that contain only blittable members are pinned instead of copied during marshaling. 


On the other hand, the Julia array operation functions requires a `jl_array_t*` **instead of a raw pointer**, which means we have to undergo at least two steps
- first wrap a raw pointer (an array in C#) into a `jl_array_t *` with functions like [`jl_ptr_to_array_1d`](https://docs.julialang.org/en/v1/manual/embedding/#Memory-Management)
- secondly, call Julia functions to operate on the above `jl_array_t *`

Note that the automatic pinning during marshaling is valid for a single function call ONLY, which, unfortunately, renders this mechanism useless in the Julia case. As a result, we have to pin the object (like an array) manually in C# to prepare it for consumption in Julia using [`GCHandle`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle?view=net-5.0):
> When the handle has been allocated, you can use it to prevent the managed object from being collected by the garbage collector when an unmanaged client holds the only reference. Without such a handle, the object can be collected by the garbage collector before completing its work on behalf of the unmanaged client.


The final solution is
```csharp
// pin a C# array with `GCHandle`
double[] csArray = new double[] { 3.14, 1.2, 4.5, 6.7, -10 };
GCHandle h = GCHandle.Alloc(csArray, GCHandleType.Pinned);
// wrap the array into a `jl_array_t*` (interpreted as a Vector{Float64} in Julia)
IntPtr jlArray = Julia.jl_ptr_to_array_1d(arrayType, h.AddrOfPinnedObject(), csArray.Length, 0);
// apply in-place reverse!
IntPtr reverse_ = Julia.jl_eval_string("reverse!");
Julia.jl_call1(reverse_, jlArray);
// check the result
foreach (double ele in csArray)
    Console.WriteLine(ele);
// free the handle
h.Free();
```

:bangbang:**Caution**::bangbang: 
- The last argument of `jl_ptr_to_array_1d` must be 0 to tell that Julia does NOT own this memory; otherwise, Julia runtime will try to `free` this memory.

- The above `jlArray` wrapper returned by `jl_ptr_to_array_1d` is itself allocated and managed by Julia GC (see [source](https://github1s.com/JuliaLang/julia/blob/HEAD/src/array.c#L332)). Hence, a *safer* way is to also tell Julia that we are holding a reference to `jlArray` to ensure that `jlArray` survives subsequent `jl_...` calls (you should do it in production code). Check the above section on how to do it.

:blush: :grin: The above `GCHandle` boilerplate code can be simplifed with the `fixed` keyword. See the next tutorial [EJToyApp](../EJToyApp) for an example. 

## Multidimensional Arrays
> Julia's multidimensional arrays are stored in memory in column-major order. 

Thus, we may view the multidimensional array as a long 1D array and interpret the indexing carefully. Special attention should be paid to the fact the multi-dim arrays in C/C++/C# are stored in row-major order. Check the next tutorial [EJToyApp](../EJToyApp) for an example. 





