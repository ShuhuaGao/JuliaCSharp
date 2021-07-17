# Toy application: solve linear equations in collaboration with Math.NET

Consider a linear system `Ax = b`. Given `A` and `b`, we want to get the value of `x`. This can be easily done in Julia by `x = A \ b`. 

We use the [MathNet](https://www.nuget.org/packages/MathNet.Numerics) package to build a random matrix `A` and a random vector `b` in C#. (Note that MathNet adopts a column-major storage just like Julia.) Then, try to communicate these data to Julia. There are in total four primary steps.

1. Share the vector `b` with Julia without copy using `jl_ptr_to_array_1d`
2. We may also use the `jl_ptr_to_array` to let Julia access the matrix `A` with no copy. However, the syntax to achieve 2D array reuse is very [verbose](https://discourse.julialang.org/t/api-reference-for-julia-embedding-in-c/3963/4?u=shuhua). Thus, an alternative way is taken: a 2D array is first allocated inside Julia with `jl_alloc_array_2d` and then the data of `A` is copied from C# to the array in Julia. (The extra copy cost may be ignored if this operation is infrequent.)
3. Perform `x = A \ b` in Julia.
4. Retrieve the resultant vector `x` from Julia to C#.

The main body of the code is
```csharp
fixed (double* pb = b.AsArray())  // pin `b` 
{
    // wrap b's data into a Julia vector
    IntPtr jlb = Julia.jl_ptr_to_array_1d(VectorFloat64, (IntPtr)pb, b.Count, 0);
    Julia.jl_call3(setindex_, REFS, jlb, jlb); // keep `jlb` live
    // allocate a 2D Julia matrix
    IntPtr jlA = Julia.jl_alloc_array_2d(MatrixFloat64, A.RowCount, A.ColumnCount);
    Julia.jl_call3(setindex_, REFS, jlA, jlA);
    IntPtr jlAData = Julia.jl_array_data(jlA);  // pointer to its data
    // copy data from A to jlA
    Marshal.Copy(A.AsColumnMajorArray(), 0, jlAData, A.RowCount * A.ColumnCount);
    // (optionally) print to verify jlA
    Julia.jl_call1(Julia.jl_eval_string("println"), jlA);
    // solve Ax = b
    IntPtr jlfunc = Julia.jl_eval_string("\\");
    IntPtr jlx = Julia.jl_call2(jlfunc, jlA, jlb);
    Julia.jl_call3(setindex_, REFS, jlx, jlx);
    // retrieve the data of jlx to C#
    Vector<double> x = Vector<double>.Build.Dense(A.ColumnCount);
    Marshal.Copy(Julia.jl_array_data(jlx), x.AsArray(), 0, x.Count);
    Console.WriteLine($"x = {x}");
    // after finish, allow Julia to garbage collect variables
    Julia.jl_call2(delete_, REFS, jlb);
    Julia.jl_call2(delete_, REFS, jlA);
    Julia.jl_call2(delete_, REFS, jlx);
}
```

Key points are

- The [`fixed`](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/fixed-statement) statement is used to simplify the pinning procedures. 
  > The fixed statement prevents the garbage collector from relocating a movable variable. The fixed statement is only permitted in an unsafe context. After the code in the `fixed` statement is executed, any pinned variables are unpinned and subject to garbage collection.

    In short, the memory taken by `b.AsArray()` is pinned such that Julia can use a pointer to access this block of memory safely. The pinning is released automatically once the scope `{}` finishes.

- In `MathNet`, the `AsArray` or `AsColumnMajorArray` method returns the underlying `double[]` or `double[,]` data storage (without any copy).

- `Marshal.copy` is used to copy memory between the two worlds. It is surely possible to avoid the copy though the code will become more verbose.

- The above `jl_array_data` is NOT `DllImport` since it is just a macro in the julia.h. Instead, we define this method as a utility function:
    ```csharp
    public static IntPtr jl_array_data(IntPtr jlArray) => Marshal.ReadIntPtr(jlArray);
    ```
    Check the previous [EJArray](https://github.com/ShuhuaGao/JuliaCSharp/tree/main/Embedding/EJArrays) tutorial for details.


## Summary
It is possible to embed Julia inside a .NET program via its C API provided in [julia.h](). In the tutorials so far, we have stuck to the raw C API with no custom wrappers in C#. As a consequence, the code seems verbose and does not respect the C# coding style.

Future work will focus on the design of convenient wrappers for interoperation with Julia, e.g., design a custom array class in C# that represents an `Array` in Julia.

