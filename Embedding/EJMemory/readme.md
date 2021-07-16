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

## Share C# array with Julia 
If there exists already an array in C#, how can we pass it to Julia? One obvious way is to first allocate an array (of a sufficient size) in Julia with `jl_alloc_array_1d` and then *copy* the data from C# array to the Julia array. Alternatively, we may want to avoid the copy and let Julia access the C# array data directly. 

Recall that C# (and the .NET runtime) has its own managed memory space, which is distinct from the managed memory of Julia. Another key issue with the .NET managed memory is that the runtime may move the object to a different memory location freely (we will not feel it if we don't work with pointers though). Consequently, if we simply pass a pointer to a location in C# managed memory into a Julia function, the pointer may become invalid sometime. That's why memory *pinning* is critical when passing data from C# to Julia without copy. See [Copying and Pinning](https://docs.microsoft.com/en-us/dotnet/framework/interop/copying-and-pinning) for more details.
>Pinning temporarily locks the data in its current memory location, thus keeping it from being relocated by the common language runtime's garbage collector. The marshaler pins data to reduce the overhead of copying and enhance performance. The type of the data determines whether it is copied or pinned during the marshaling process. Pinning is automatically performed during marshaling for objects such as String, however you can also manually pin memory using the GCHandle class.

In general applications, we pass mostly arrays of primitive types between C# and Julia. It is fortunate that the .NET runtime supports (see [doc source](https://docs.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types))
>As an optimization, arrays of blittable primitive types and classes that contain only blittable members are pinned instead of copied during marshaling. 

which means we get the pinning behavior automatically in most cases without any extra preparation. In addition, to allow Julia to change the data passed, the parameter should be annotated with the `[In, Out]` attribute.

Since the Julia is more than a raw pointer, we need to pass a pointer of the data array into the `jl_ptr_to_array_1d` to wrap it into a `jl_array_t *`.
The key C API is pasted below:
```c
jl_array_t *jl_ptr_to_array_1d(jl_value_t *atype, void *data, size_t nel, int own_buffer)
```
We see that any type of data can be accepted, whose actual type is specified by `atype` above.


Unlike the `DllImport` we have seen so far, we need to marshal an array type as `void *` instead of the raw `IntPtr` to take advantage of the array's pinning behavior. However, the method annotated by `DllImport` cannot be a generic one. Consequently, we need to define each type of array separately. For instance, two versions are given below (by function overloading).
```csharp
[DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr jl_ptr_to_array_1d(IntPtr type, double[] data, size_t length, int own_buffer);

[DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr jl_ptr_to_array_1d(IntPtr type, float[] data, size_t length, int own_buffer);
```




